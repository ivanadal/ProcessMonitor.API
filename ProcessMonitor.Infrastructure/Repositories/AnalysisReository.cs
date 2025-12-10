using ProcessMonitor.Domain.Entities;
using ProcessMonitor.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProcessMonitor.Infrastructure.Repositories
{
    public class AnalysisRepository : IAnalysisRepository
    {
        public Task AddAsync(Analysis analysis)
        {
            throw new NotImplementedException();
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
