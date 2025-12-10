
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ProcessMonitor.Domain.Entities;
using ProcessMonitor.Domain.Interfaces;
using System.Text;
using System.Text.Json;

namespace ProcessMonitor.Infrastructure.Services
{
    public class HuggingFaceAnalysisService : IAIAnalysisService
    {
        private readonly HttpClient _httpClient;
        private readonly string[] _candidateLabels = new[] { "complies", "deviates", "unclear" };
        private readonly string _endpoint;
        private readonly string _modelId;
        private ILogger<HuggingFaceAnalysisService> _logger;

        public HuggingFaceAnalysisService(HttpClient httpClient, IConfiguration config, ILogger<HuggingFaceAnalysisService> logger)
        {
            _httpClient = httpClient;
            _endpoint = config["HuggingFace:Endpoint"] ?? throw new InvalidOperationException("HuggingFace:Endpoint missing");
            _modelId = config["HuggingFace:ModelId"] ?? throw new InvalidOperationException("HuggingFace:ModelId missing");
            _logger = logger;
        }

        public async Task<HuggingFaceResult> AnalyzeAsync(string action)
        {
            _logger.LogDebug($"{DateTime.UtcNow}: HuggingFaceAnalysisService: AnalyzeAsync method started.");
            var payload = new
            {
                inputs = action,
                parameters = new { candidate_labels = _candidateLabels }
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var url = _endpoint + $"/{_modelId}";

            HttpResponseMessage response;
            try
            {
                response = await _httpClient.PostAsync(url, content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to reach HuggingFace endpoint: {url}");
                throw; 
            }

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();

                _logger.LogError(
                    $"HuggingFace request failed. Status: {(int)response.StatusCode}, Url: {url}, Body: {errorBody}");

                throw new HttpRequestException(
                    $"HuggingFace API returned {(int)response.StatusCode}: {response.ReasonPhrase}"
                );
            }
            else
            {
                _logger.LogDebug($"{DateTime.UtcNow}: HuggingFaceAnalysisService: AnalyzeAsync - response obtained.");
            }             

            var json = await response.Content.ReadAsStringAsync();
            var items = JsonSerializer.Deserialize<HuggingFaceResponseItem[]>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (items == null || items.Length == 0)
                throw new InvalidOperationException("HuggingFace returned an empty or invalid response.");

            var best = items!.OrderByDescending(x => x.Score).First();

            _logger.LogDebug($"{DateTime.UtcNow}: HuggingFaceAnalysisService: AnalyzeAsync - best: Label->{best.Label}; Score ->{best.Score}");

            return new HuggingFaceResult
            {
                Label = best.Label,
                Score = best.Score
            };
        }
    }

}
