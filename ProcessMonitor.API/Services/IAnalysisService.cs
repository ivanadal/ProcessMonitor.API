using ProcessMonitor.API.Models;

namespace ProcessMonitor.API.Services
{
    public interface IAnalysisService
    {
        Task<AnalyzeResponse> AnalyzeAsync(string action, string guideline);
        
    }
}
