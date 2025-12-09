using ProcessMonitor.API.Models;

namespace ProcessMonitor.API.Services
{
    public interface IAnalysisService
    {
        Task<AnalyzeResult> AnalyzeAsync(string action, string guideline);
        
    }
}
