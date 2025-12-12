using System;
using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
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
    public class SummaryEndpointTests
    {
        private const string TestApiKey = "test-key";
        private readonly AnalysisSummary _expected = new AnalysisSummary
        {
            TotalAll = 3,
            TotalComplies = 1,
            TotalDeviates = 1,
            TotalUnclear = 1
        };

        [TestMethod]
        public async Task GetSummary_ReturnsExpectedSummary_UsingRealRepository_WithSqliteInMemory()
        {
            // Create a single in-memory SQLite connection and keep it open for the test lifetime.
            using var connection = new SqliteConnection("Data Source=:memory:;Cache=Shared");
            connection.Open();

            await using var factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    // Ensure the host environment is Development so ExceptionMiddleware emits details
                    builder.UseEnvironment("Development");

                    builder.ConfigureAppConfiguration((context, cfg) =>
                    {
                        // provide ApiKey (environment already set above)
                        cfg.AddInMemoryCollection(new[]
                        {
                            new KeyValuePair<string, string>("ApiKey", TestApiKey)
                        });
                    });

                    builder.ConfigureServices(services =>
                    {
                        // Remove existing EF registrations so only our relational provider remains
                        services.RemoveAll(typeof(AppDbContext));
                        services.RemoveAll(typeof(DbContextOptions<AppDbContext>));

                        // Register AppDbContext using the open SQLite in-memory connection
                        services.AddDbContext<AppDbContext>(opts => opts.UseSqlite(connection));

                        // Replace repository with the real repository implementation
                        services.RemoveAll(typeof(IAnalysisRepository));
                        services.AddScoped<IAnalysisRepository, AnalysisRepository>();
                    });
                });

            // Seed the relational in-memory DB after the host is built
            using (var scope = factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.Database.EnsureCreated();

                db.Analyses.AddRange(
                    new Analysis
                    {
                        Action = "Closed ticket #1",
                        Guideline = "guideline-1",
                        Result = "COMPLIES",
                        Confidence = 0.95,
                        Timestamp = DateTime.UtcNow.AddMinutes(-3)
                    },
                    new Analysis
                    {
                        Action = "Updated record #2",
                        Guideline = "guideline-2",
                        Result = "DEVIATES",
                        Confidence = 0.6,
                        Timestamp = DateTime.UtcNow.AddMinutes(-2)
                    },
                    new Analysis
                    {
                        Action = "Commented on task #3",
                        Guideline = "guideline-3",
                        Result = "UNCLEAR",
                        Confidence = 0.2,
                        Timestamp = DateTime.UtcNow.AddMinutes(-1)
                    }
                );

                await db.SaveChangesAsync();
            }

            var client = factory.CreateClient();
            client.DefaultRequestHeaders.Add("X-Api-Key", TestApiKey);

            var response = await client.GetAsync("/v1/processmonitor/summary");

            // If the endpoint returned a non-success status, dump response content to make the failure visible
            if (response.StatusCode != HttpStatusCode.OK)
            {
                string body = await response.Content.ReadAsStringAsync();
                try
                {
                    using var doc = JsonDocument.Parse(body);
                    var pretty = JsonSerializer.Serialize(doc, new JsonSerializerOptions { WriteIndented = true });
                    Assert.Fail($"Expected HTTP 200 OK but got {(int)response.StatusCode} {response.StatusCode}. Response body:\n{pretty}");
                }
                catch
                {
                    Assert.Fail($"Expected HTTP 200 OK but got {(int)response.StatusCode} {response.StatusCode}. Response body:\n{body}");
                }
            }

            var json = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var actual = JsonSerializer.Deserialize<AnalysisSummary>(json, options);

            Assert.IsNotNull(actual, "Response body deserialized to null.");
            Assert.AreEqual(_expected.TotalAll, actual.TotalAll, "TotalAll mismatch.");
            Assert.AreEqual(_expected.TotalComplies, actual.TotalComplies, "TotalComplies mismatch.");
            Assert.AreEqual(_expected.TotalDeviates, actual.TotalDeviates, "TotalDeviates mismatch.");
            Assert.AreEqual(_expected.TotalUnclear, actual.TotalUnclear, "TotalUnclear mismatch.");
        }
    }
}