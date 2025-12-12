using System;
using System.Linq;
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
using ProcessMonitor.API.DTOs;
using ProcessMonitor.Data;
using ProcessMonitor.Domain.Entities;
using ProcessMonitor.Domain.Interfaces;
using ProcessMonitor.Infrastructure.Repositories;

namespace ProcessMonitor.Tests.IntegrationTests
{
    [TestClass]
    public class HistoryEndpointTests
    {
        private const string TestApiKey = "test-key";

        [TestMethod]
        public async Task GetHistory_ReturnsPagedResults_UsingRealRepository_WithSqliteInMemory()
        {
            // keep single in-memory SQLite connection open for test lifetime
            using var connection = new SqliteConnection("Data Source=:memory:;Cache=Shared");
            connection.Open();

            await using var factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    // run host in Development so middleware returns detailed errors
                    builder.UseEnvironment("Development");

                    builder.ConfigureAppConfiguration((context, cfg) =>
                    {
                        cfg.AddInMemoryCollection(new[]
                        {
                            new KeyValuePair<string, string>("ApiKey", TestApiKey)
                        });
                    });

                    builder.ConfigureServices(services =>
                    {
                        // ensure we only register one EF provider
                        services.RemoveAll(typeof(AppDbContext));
                        services.RemoveAll(typeof(DbContextOptions<AppDbContext>));

                        // register relational in-memory sqlite connection
                        services.AddDbContext<AppDbContext>(opts => opts.UseSqlite(connection));

                        // use real repository
                        services.RemoveAll(typeof(IAnalysisRepository));
                        services.AddScoped<IAnalysisRepository, AnalysisRepository>();
                    });
                });

            // seed DB using host service provider after host is built
            using (var scope = factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.Database.EnsureCreated();

                db.Analyses.AddRange(
                    new Analysis
                    {
                        Action = "First action",
                        Guideline = "g1",
                        Result = "COMPLIES",
                        Confidence = 0.9,
                        Timestamp = DateTime.UtcNow.AddMinutes(-30)
                    },
                    new Analysis
                    {
                        Action = "Second action",
                        Guideline = "g2",
                        Result = "DEVIATES",
                        Confidence = 0.5,
                        Timestamp = DateTime.UtcNow.AddMinutes(-20)
                    },
                    new Analysis
                    {
                        Action = "Third action",
                        Guideline = "g3",
                        Result = "UNCLEAR",
                        Confidence = 0.1,
                        Timestamp = DateTime.UtcNow.AddMinutes(-10)
                    }
                );

                await db.SaveChangesAsync();
            }

            var client = factory.CreateClient();
            client.DefaultRequestHeaders.Add("X-Api-Key", TestApiKey);

            // request page 1, pageSize 2
            var response = await client.GetAsync("/v1/processmonitor/history?page=1&pageSize=2");

            // if non-OK, show response body (dumps detailed exception in Development)
            if (response.StatusCode != HttpStatusCode.OK)
            {
                var body = await response.Content.ReadAsStringAsync();
                try
                {
                    using var doc = JsonDocument.Parse(body);
                    var pretty = JsonSerializer.Serialize(doc, new JsonSerializerOptions { WriteIndented = true });
                    Assert.Fail($"Expected 200 OK but got {(int)response.StatusCode} {response.StatusCode}. Body:\n{pretty}");
                }
                catch
                {
                    Assert.Fail($"Expected 200 OK but got {(int)response.StatusCode} {response.StatusCode}. Body:\n{body}");
                }
            }

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var json = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var result = JsonSerializer.Deserialize<HistoryQueryResponse<Analysis>>(json, options);

            Assert.IsNotNull(result, "Response deserialized to null.");
            Assert.AreEqual(1, result.Page, "Page mismatch.");
            Assert.AreEqual(2, result.PageSize, "PageSize mismatch.");
            Assert.AreEqual(3, result.TotalItems, "TotalItems mismatch.");
            Assert.AreEqual(2, result.TotalPages, "TotalPages mismatch.");
            Assert.IsNotNull(result.Items, "Items expected but null.");
            Assert.AreEqual(2, result.Items.Count, "Items count for page 1");

            // Verify ordering is descending by Timestamp: newest first
            Assert.IsTrue(result.Items[0].Timestamp >= result.Items[1].Timestamp, "Items not ordered descending by Timestamp.");

            // Verify the returned actions are the two most recent
            var actions = result.Items.Select(i => i.Action).ToArray();
            CollectionAssert.Contains(actions, "Third action");
            CollectionAssert.Contains(actions, "Second action");
            CollectionAssert.DoesNotContain(actions, "First action");
        }
    }
}