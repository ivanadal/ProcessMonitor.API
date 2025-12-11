using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProcessMonitor.Data;
using ProcessMonitor.Domain.Entities;
using ProcessMonitor.Domain.Interfaces;
using ProcessMonitor.Infrastructure.Repositories;

namespace ProcessMonitor.Tests.IntegrationTests
{
    [TestClass]
    public class AnalyzeEndpointTests
    {
        private const string TestApiKey = "test-key";

        [TestMethod]
        public async Task PostAnalyze_PersistsAndReturnsAnalysis_UsingRealRepositoryAndFakeAI()
        {
            var expectedAction = "Closed ticket #48219 and sent confirmation email";
            var expectedGuideline = "All closed tickets must include a confirmation email";

            await using var factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureAppConfiguration((context, cfg) =>
                    {
                        cfg.AddInMemoryCollection(new[]
                        {
                            new KeyValuePair<string, string>("ApiKey", TestApiKey)
                        });
                    });

                    builder.ConfigureServices(services =>
                    {
                        // Ensure only one EF provider is registered: remove app's registrations
                        services.RemoveAll(typeof(AppDbContext));
                        services.RemoveAll(typeof(DbContextOptions<AppDbContext>));

                        // Use isolated InMemory DB for the test
                        var inMemoryDbName = "AnalyzeTestDb_" + Guid.NewGuid().ToString("N");
                        services.AddDbContext<AppDbContext>(opts => opts.UseInMemoryDatabase(inMemoryDbName));

                        // Ensure real repository is used
                        services.RemoveAll(typeof(IAnalysisRepository));
                        services.AddScoped<IAnalysisRepository, AnalysisRepository>();

                        // Replace external AI service with deterministic test double
                        services.RemoveAll(typeof(IAIAnalysisService));
                        services.AddSingleton<IAIAnalysisService>(new TestAIAnalysisService("complies", 0.87654));
                    });
                });

            var client = factory.CreateClient();
            client.DefaultRequestHeaders.Add("X-Api-Key", TestApiKey);

            var payload = new
            {
                Action = expectedAction,
                Guideline = expectedGuideline
            };

            var json = JsonSerializer.Serialize(payload);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("/v1/processmonitor/analyze", content);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, "Expected 200 OK from analyze endpoint.");

            var responseJson = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseJson);
            var root = doc.RootElement;

            Assert.AreEqual(expectedAction, root.GetProperty("action").GetString(), "Action mismatch in response.");
            Assert.AreEqual(expectedGuideline, root.GetProperty("guideline").GetString(), "Guideline mismatch in response.");
            Assert.AreEqual("COMPLIES", root.GetProperty("result").GetString(), "Result mapping mismatch.");
            // Confidence is rounded to 4 decimals in domain service
            Assert.AreEqual(0.8765, root.GetProperty("confidence").GetDouble(), 0.00001, "Confidence rounding mismatch.");

            // Verify repository persisted the analysis
            using (var scope = factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var items = db.Analyses.ToList();
                Assert.AreEqual(1, items.Count, "Expected one persisted Analysis record.");

                var stored = items.Single();
                Assert.AreEqual(expectedAction, stored.Action);
                Assert.AreEqual(expectedGuideline, stored.Guideline);
                Assert.AreEqual("COMPLIES", stored.Result);
                Assert.AreEqual(Math.Round(0.87654, 4), stored.Confidence);
            }
        }

        // Simple deterministic AI test double
        private class TestAIAnalysisService : IAIAnalysisService
        {
            private readonly string _label;
            private readonly double _score;

            public TestAIAnalysisService(string label, double score)
            {
                _label = label;
                _score = score;
            }

            public Task<HuggingFaceResult> AnalyzeAsync(string action)
            {
                return Task.FromResult(new HuggingFaceResult
                {
                    Label = _label,
                    Score = _score
                });
            }
        }
    }
}