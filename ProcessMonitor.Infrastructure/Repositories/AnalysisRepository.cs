using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Polly.Caching;
using ProcessMonitor.Data;
using ProcessMonitor.Domain.Entities;
using ProcessMonitor.Domain.Interfaces;

namespace ProcessMonitor.Infrastructure.Repositories
{
    public class AnalysisRepository : IAnalysisRepository
    {
        private readonly AppDbContext _db;
        private readonly ILogger<AnalysisRepository> _logger;

        public AnalysisRepository(AppDbContext db, ILogger<AnalysisRepository> logger)
        {
            _db = db;
            _logger = logger;
        }
        public async Task<Analysis> AddAsync(Analysis analysis)
        {
            _logger.LogDebug($"{DateTime.UtcNow}: AnalysisRepository: AddAsync started.");

            _db.Analyses.Add(analysis);
            await _db.SaveChangesAsync();
            _logger.LogDebug($"{DateTime.UtcNow}: AnalysisRepository: AddAsync ended.");
            return analysis;

        }

        public async Task<IEnumerable<Analysis>> GetHistoryAsync()
        {
            return await _db.Analyses.OrderByDescending(a => a.Timestamp).ToListAsync();
        }

        public async Task<PagedResult<Analysis>> GetPagedHistoryAsync(int page, int pageSize)
        {
            _logger.LogDebug($"{DateTime.UtcNow}: AnalysisRepository: GetPagedHistoryAsync started.");

            var totalItems = await _db.Analyses.CountAsync();

            var items = await _db.Analyses.OrderByDescending(a => a.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            _logger.LogDebug($"{DateTime.UtcNow}: AnalysisRepository: GetPagedHistoryAsync ended.");

            return new PagedResult<Analysis>
            {
                Items = items,
                TotalItems = totalItems,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<AnalysisSummary> GetSummaryAsync()
        {
            _logger.LogDebug($"{DateTime.UtcNow}: AnalysisRepository: GetSummaryAsync started.");

            throw new Exception();

            var byResult = await _db.Analyses
                .GroupBy(a => a.Result)
                .Select(g => new { Result = g.Key, Count = g.Count() })
                .ToListAsync();

            var resultDict = byResult.ToDictionary(x => x.Result, x => x.Count);

            var totalComplies = resultDict.GetValueOrDefault("COMPLIES", 0);
            var totalDeviates = resultDict.GetValueOrDefault("DEVIATES", 0);
            var totalUnclear = resultDict.GetValueOrDefault("UNCLEAR", 0);

            _logger.LogDebug($"{DateTime.UtcNow}: AnalysisRepository: GetSummaryAsync ended.");

            return new AnalysisSummary
            {
                TotalComplies = totalComplies,
                TotalDeviates = totalDeviates,
                TotalUnclear = totalUnclear,
                TotalAll = totalComplies + totalDeviates + totalUnclear,
            };
        }

    }
    
}
