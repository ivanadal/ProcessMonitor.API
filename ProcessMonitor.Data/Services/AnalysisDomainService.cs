
using ProcessMonitor.Domain.Entities;
using ProcessMonitor.Domain.Interfaces;



namespace ProcessMonitor.Domain.Services
{
    public class AnalysisDomainService : IAnalysisService
    {
        private readonly IAnalysisRepository _repository;
        private readonly IAIAnalysisService _hfService;

        public AnalysisDomainService(IAnalysisRepository repository, IAIAnalysisService hfService)
        {
            _repository = repository;
            _hfService = hfService;
        }

        public async Task<AnalysisResult> AnalyzeAsync(string action, string guideline)
        {
            // Call external AI
            var hfItem = await _hfService.AnalyzeAsync(action);

            // Map HuggingFace result → Domain entity
            var result = new AnalysisResult
            {
                Action = action,
                Guideline = guideline,
                Result = hfItem.Label.Equals("complies", StringComparison.OrdinalIgnoreCase)
                            ? "COMPLIES"
                            : hfItem.Label.Equals("deviates", StringComparison.OrdinalIgnoreCase)
                                ? "DEVIATES"
                                : "UNCLEAR",
                Confidence = Math.Round(hfItem.Score, 4),
                Timestamp = DateTime.UtcNow
            };

            //await _repository.AddAsync(result);

            return result;
        }

        // Other methods: GetHistoryAsync, GetSummaryAsync
    }

}
