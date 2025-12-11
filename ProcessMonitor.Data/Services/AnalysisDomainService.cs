
using Microsoft.Extensions.Logging;
using ProcessMonitor.Domain.Entities;
using ProcessMonitor.Domain.Interfaces;



namespace ProcessMonitor.Domain.Services
{
    public class AnalysisDomainService : IAnalysisService
    {
        private readonly IAnalysisRepository _repository;
        private readonly IAIAnalysisService _hfService;
        private readonly ILogger _logger;

        public AnalysisDomainService(IAnalysisRepository repository, IAIAnalysisService hfService, ILogger<AnalysisDomainService> logger)
        {
            _repository = repository;
            _hfService = hfService;
            _logger = logger;
        }

        public async Task<Analysis> AnalyzeAsync(string action, string guideline)
        {
            _logger.LogDebug($"{DateTime.UtcNow}: AnalysisDomainService: AnalyzeAsync method started.");
            // Call external AI
            var hfItem = await _hfService.AnalyzeAsync(action);

            // Map HuggingFace result → Domain entity
            var result = new Analysis
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

            _logger.LogDebug($"{DateTime.UtcNow}: AnalysisDomainService: result retrieved from external service. " +
                 $"Action='{result.Action}', Guideline='{result.Guideline}', Result='{result.Result}', " +
                 $"Confidence={result.Confidence}, Timestamp={result.Timestamp:O}");

            await _repository.AddAsync(result);

            _logger.LogDebug($"{DateTime.UtcNow}: AnalysisDomainService: AnalyzeAsync method ended.");

            return result;
        }

    }

}
