
using Microsoft.Extensions.Logging;
using ProcessMonitor.Domain.Entities;
using ProcessMonitor.Domain.Exceptions;
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
            var startTime = DateTime.UtcNow;
            _logger.LogDebug($"{DateTime.UtcNow}: AnalysisDomainService: AnalyzeAsync method started.");

            HuggingFaceResult hfResult;

            try
            {
                // Call HuggingFace service
                hfResult = await _hfService.AnalyzeAsync(action, guideline);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"HuggingFaceAnalysisService failed for Action='{action}', Guideline='{guideline}'");
                throw new AnalysisServiceException("Failed to analyze action.", ex);
            }

            var analysis = new Analysis
            {
                Action = action,
                Guideline = guideline,
                Result = hfResult.Label,
                Confidence = Math.Round(hfResult.Score, 4),
                Timestamp = DateTime.UtcNow
            };


            _logger.LogDebug($"{DateTime.UtcNow}: AnalysisDomainService: result retrieved from external service. " +
                 $"Action='{analysis.Action}', Guideline='{analysis.Guideline}', Result='{analysis.Result}', " +
                 $"Confidence={analysis.Confidence}, Timestamp={analysis.Timestamp:O}");

            // NOTE: If this fails, in some production scenarios we would want to retry or queue for later processing
            await _repository.AddAsync(analysis);

            _logger.LogDebug($"{DateTime.UtcNow}: AnalysisDomainService: AnalyzeAsync method ended.");

            return analysis;
        }
    }
}
