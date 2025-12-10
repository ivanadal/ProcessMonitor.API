
using Microsoft.Extensions.Configuration;
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

        public HuggingFaceAnalysisService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _endpoint = config["HuggingFace:Endpoint"] ?? throw new InvalidOperationException("HuggingFace:Endpoint missing");
            _modelId = config["HuggingFace:ModelId"] ?? throw new InvalidOperationException("HuggingFace:ModelId missing");
        }

        public async Task<HuggingFaceResult> AnalyzeAsync(string action)
        {
            var payload = new
            {
                inputs = action,
                parameters = new { candidate_labels = _candidateLabels }
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var url = _endpoint + $"/{_modelId}";

            var response = await _httpClient.PostAsync(url, content);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var items = JsonSerializer.Deserialize<HuggingFaceResponseItem[]>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            var best = items!.OrderByDescending(x => x.Score).First();

            return new HuggingFaceResult
            {
                Label = best.Label,
                Score = best.Score
            };
        }
    }

}
