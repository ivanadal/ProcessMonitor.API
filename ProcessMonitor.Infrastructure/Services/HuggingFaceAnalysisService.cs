
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ProcessMonitor.Domain.Entities;
using ProcessMonitor.Domain.Interfaces;
using System.Text;
using System.Text.Json;
using Polly;
using Polly.Retry;
using System.Net;
using ProcessMonitor.Domain.Enums;

namespace ProcessMonitor.Infrastructure.Services
{
    public class HuggingFaceAnalysisService : IAIAnalysisService
    {
        private readonly HttpClient _httpClient;
        private readonly string _endpoint;
        private readonly string _modelId;
        private readonly float _threshold;
        private ILogger<HuggingFaceAnalysisService> _logger;

        public HuggingFaceAnalysisService(HttpClient httpClient, IConfiguration config, ILogger<HuggingFaceAnalysisService> logger)
        {
            _httpClient = httpClient;
            _endpoint = config["HuggingFace:Endpoint"] ?? throw new InvalidOperationException("HuggingFace:Endpoint missing");
            _modelId = config["HuggingFace:ModelId"] ?? throw new InvalidOperationException("HuggingFace:ModelId missing");
            _threshold = config.GetValue<float>("HuggingFace:ConfidenceThreshold", 0.6f);
            _logger = logger;
        }

        public async Task<HuggingFaceResult> AnalyzeAsync(string action, string guideline)
        {
            _logger.LogDebug($"{DateTime.UtcNow}: HuggingFaceAnalysisService: AnalyzeAsync method started.");

            var candidateLabels = Enum.GetNames<CandidateLabelsEnum>()
                                 .Select(l => $"{l.ToLower()} with guideline: {guideline}")
                                 .ToArray();

            var payload = new
            {
                inputs = action,
                parameters = new { candidate_labels = candidateLabels },
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var url = _endpoint + $"/{_modelId}";

            // Retry policy: only for network issues, 5xx server errors, and 429 Too Many Requests 
            AsyncRetryPolicy<HttpResponseMessage> retryPolicy = Policy
                .Handle<HttpRequestException>() 
                .OrResult<HttpResponseMessage>(r =>
                    r.StatusCode == HttpStatusCode.RequestTimeout || 
                    r.StatusCode == (HttpStatusCode)429 ||           
                    ((int)r.StatusCode >= 500 && (int)r.StatusCode < 600)) 
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                    onRetry: (outcome, timespan, attempt, context) =>
                    {
                        if (outcome.Exception != null)
                            _logger.LogWarning(outcome.Exception, $"Retry {attempt} due to network exception. Waiting {timespan.TotalSeconds}s.");
                        else
                            _logger.LogWarning($"Retry {attempt} due to status code {(int)outcome.Result.StatusCode}. Waiting {timespan.TotalSeconds}s.");
                    }
                );

            HttpResponseMessage response = await retryPolicy.ExecuteAsync(async () =>
            {
                return await _httpClient.PostAsync(url, content);
            });

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                _logger.LogError($"HuggingFace request failed. Status: {(int)response.StatusCode}, Url: {url}, Body: {errorBody}");
                throw new HttpRequestException($"HuggingFace API returned {(int)response.StatusCode}: {response.ReasonPhrase}");
            }

            var json = await response.Content.ReadAsStringAsync();
            var items = JsonSerializer.Deserialize<HuggingFaceResponseItem[]>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            // This could be also treated as UNCLEAR, it should be discussed depending on buisness needs
            if (items == null || items.Length == 0)
                throw new InvalidOperationException("HuggingFace returned an empty or invalid response.");

            var best = items.OrderByDescending(x => x.Score).First();

            _logger.LogDebug($"{DateTime.UtcNow}: HuggingFaceAnalysisService: AnalyzeAsync - best: Label->{best.Label}; Score ->{best.Score}");

            // Map full label string back to enum
            var simpleLabel = best.Label.Split(' ')[0].ToLower() switch
            {
                "complies" => best.Score >= _threshold ? CandidateLabelsEnum.COMPLIES : CandidateLabelsEnum.UNCLEAR,
                "deviates" => best.Score >= _threshold ? CandidateLabelsEnum.DEVIATES : CandidateLabelsEnum.UNCLEAR,
                "unclear" => CandidateLabelsEnum.UNCLEAR,
                _ => CandidateLabelsEnum.UNCLEAR
            };

            return new HuggingFaceResult
            {
                Label = simpleLabel.ToString(),
                Score = best.Score
            };
        }

    }
}
