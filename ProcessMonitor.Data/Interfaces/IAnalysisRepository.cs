using ProcessMonitor.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProcessMonitor.Domain.Interfaces
{
    public interface IAnalysisRepository
    {
        Task<Analysis> AddAsync(Analysis analysis);
        Task<IEnumerable<Analysis>> GetHistoryAsync();
        Task<PagedResult<Analysis>> GetPagedHistoryAsync(int page, int pageSize);
        Task<AnalysisSummary> GetSummaryAsync();
    }
}
