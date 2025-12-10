using ProcessMonitor.Data;
using ProcessMonitor.Domain.Entities;
using ProcessMonitor.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

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

        public Task<List<Analysis>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Dictionary<string, int>> GetCountByResultAsync()
        {
            throw new NotImplementedException();
        }

        public Task<int> GetTotalCountAsync()
        {
            throw new NotImplementedException();
        }
    }
}
