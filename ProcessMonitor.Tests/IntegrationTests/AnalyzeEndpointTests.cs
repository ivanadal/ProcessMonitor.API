using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
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
        public async Task PostAnalyze_PersistsAndReturnsAnalysis_UsingRealRepositoryAndFakeAI_WithSQLiteInMemory()
        {
            var expectedAction = "Closed ticket #48219 and sent confirmation email";
            var expectedGuideline = "All closed tickets must include a confirmation email";

            // Create a single in-memory SQLite connection and keep it open for the test lifetime.
            using var connection = new SqliteConnection("Data Source=:memory:;Cache=Shared");
            connection.Open();

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
                        // Remove existing EF registrations so we can register our SQLite in-memory connection.
                        services.RemoveAll(typeof(AppDbContext));
                        services.RemoveAll(typeof(DbContextOptions<AppDbContext>));

                        // Register AppDbContext using the open SQLite in-memory connection (relational provider).
                        services.AddDbContext<AppDbContext>(opts =>
                        {
                            opts.UseSqlite(connection);
                        });

                        // Use the real repository implementation backed by the in-memory SQLite DB
                        services.RemoveAll(typeof(IAnalysisRepository));
                        services.AddScoped<IAnalysisRepository, AnalysisRepository>();

                        // Replace external AI service with deterministic test double
                        services.RemoveAll(typeof(IAIAnalysisService));
                        services.AddSingleton<IAIAnalysisService>(new TestAIAnalysisService("COMPLIES", 0.87654));
                    });
                });

            // Ensure database schema exists and seed data if needed (do this after host is built)
            using (var scope = factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                // If your app uses migrations, use Migrate(); otherwise EnsureCreated is fine.
                // Using relational SQLite provider allows Migrate() if migrations exist.
                db.Database.EnsureCreated();

                // No initial seed required for this test (we assert the record persisted by the POST)
                // But you can seed here if needed.
            }

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

            public Task<HuggingFaceResult> AnalyzeAsync(string action, string guideline)
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