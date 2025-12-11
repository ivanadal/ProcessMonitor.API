using System;
using System.Linq;
using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
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
        public async Task GetHistory_ReturnsPagedResults_UsingRealRepository()
        {
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
                        // Remove existing EF registrations so only one provider is active
                        services.RemoveAll(typeof(AppDbContext));
                        services.RemoveAll(typeof(DbContextOptions<AppDbContext>));

                        // Add an isolated in-memory DB for this test
                        var inMemoryDbName = "HistoryTestDb_" + Guid.NewGuid().ToString("N");
                        services.AddDbContext<AppDbContext>(opts => opts.UseInMemoryDatabase(inMemoryDbName));

                        // Ensure real repository is used
                        services.RemoveAll(typeof(IAnalysisRepository));
                        services.AddScoped<IAnalysisRepository, AnalysisRepository>();
                    });
                });

            // Seed the in-memory DB after the host is built
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

            // Request first page with pageSize = 2
            var response = await client.GetAsync("/v1/processmonitor/history?page=1&pageSize=2");

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