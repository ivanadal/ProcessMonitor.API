using ProcessMonitor.Domain.Entities;


namespace ProcessMonitor.Domain.Interfaces
{
    public interface IAIAnalysisService
    {
        Task<HuggingFaceResult> AnalyzeAsync(string action);
    }
}
