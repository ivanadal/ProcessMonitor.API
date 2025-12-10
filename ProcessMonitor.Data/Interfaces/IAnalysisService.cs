
using ProcessMonitor.Domain.Entities;

namespace ProcessMonitor.Domain.Interfaces
{
    public interface IAnalysisService
    {
        Task<AnalysisResult> AnalyzeAsync(string action, string guideline);
        
    }
}
