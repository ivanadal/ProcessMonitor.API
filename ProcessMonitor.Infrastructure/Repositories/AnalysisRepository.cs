using Microsoft.EntityFrameworkCore;
using ProcessMonitor.Data;
using ProcessMonitor.Domain.Entities;
using ProcessMonitor.Domain.Interfaces;

namespace ProcessMonitor.Infrastructure.Repositories
{
    public class AnalysisRepository : IAnalysisRepository
    {
        private readonly AppDbContext _db;

        public AnalysisRepository(AppDbContext db)
        {
            _db = db;
        }
        public async Task<Analysis> AddAsync(Analysis analysis)
        {
            _db.Analyses.Add(analysis);
            await _db.SaveChangesAsync();
            return analysis;
        }

        public async Task<IEnumerable<Analysis>> GetHistoryAsync()
        {
            return await _db.Analyses.OrderByDescending(a => a.Timestamp).ToListAsync();
        }

        public Task<Dictionary<string, int>> GetCountByResultAsync()
        {
            throw new NotImplementedException();
        }

        public Task<int> GetTotalCountAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<PagedResult<Analysis>> GetPagedHistoryAsync(int page, int pageSize)
        {
            var query = _db.Analyses.OrderByDescending(a => a.Timestamp);

            var totalItems = await query.CountAsync();

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

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
            var total = await _db.Analyses.CountAsync();

            var byResult = await _db.Analyses
                .GroupBy(a => a.Result)
                .Select(g => new { Result = g.Key, Count = g.Count() })
                .ToListAsync();

            return new AnalysisSummary
            {
                TotalAll = total,
                TotalComplies = byResult.FirstOrDefault(x => x.Result == "COMPLIES")?.Count ?? 0,
                TotalDeviates = byResult.FirstOrDefault(x => x.Result == "DEVIATES")?.Count ?? 0,
                TotalUnclear = byResult.FirstOrDefault(x => x.Result == "UNCLEAR")?.Count ?? 0
            };
        }
    }
}
