
using ProcessMonitor.Domain.Entities;

namespace ProcessMonitor.Domain.Interfaces
{
    public interface IAnalysisService
    {
        Task<Analysis> AnalyzeAsync(string action, string guideline);
        
    }
}
