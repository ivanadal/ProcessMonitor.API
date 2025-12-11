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
using System;
using System.Net;
using System.Text.Json;

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
        public async Task GetSummary_ReturnsExpectedSummary_UsingRealRepository()
        {
            await using var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            await using var factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureAppConfiguration((context, cfg) =>
                    {
                        cfg.AddInMemoryCollection(new Dictionary<string, string>
                        {
                    { "ApiKey", TestApiKey }
                        });
                    });

                    builder.ConfigureServices(services =>
                    {
                        // Remove the existing AppDbContext registration
                        var descriptor = services.SingleOrDefault(
                            d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                        if (descriptor != null) services.Remove(descriptor);

                        services.RemoveAll(typeof(AppDbContext));

                        // Register SQLite In-Memory database for testing
                        services.AddDbContext<AppDbContext>(options =>
                            options.UseSqlite(connection));

                        // Replace repository with the real one
                        services.RemoveAll(typeof(IAnalysisRepository));
                        services.AddScoped<IAnalysisRepository, AnalysisRepository>();
                    });
                });

            // Seed database
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

            // Act
            var response = await client.GetAsync("/v1/processmonitor/summary");

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var json = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var actual = JsonSerializer.Deserialize<AnalysisSummary>(json, options);

            Assert.IsNotNull(actual);
            Assert.AreEqual(_expected.TotalAll, actual.TotalAll);
            Assert.AreEqual(_expected.TotalComplies, actual.TotalComplies);
            Assert.AreEqual(_expected.TotalDeviates, actual.TotalDeviates);
            Assert.AreEqual(_expected.TotalUnclear, actual.TotalUnclear);
        }

    }
}