using ProcessMonitor.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProcessMonitor.Domain.Interfaces
{
    public interface IAnalysisRepository
    {
        Task<Analysis> AddAsync(Analysis analysis);
        Task<List<Analysis>> GetAllAsync();
        Task<int> GetTotalCountAsync();
        Task<Dictionary<string, int>> GetCountByResultAsync();
    }
}
