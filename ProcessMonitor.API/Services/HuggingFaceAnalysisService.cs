using ProcessMonitor.API.Models;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace ProcessMonitor.API.Services
{
    public class HuggingFaceAnalysisService : IAnalysisService
    {
        private readonly HttpClient _httpClient;
        private readonly string _modelId;
        private readonly string _endpoint;
        private readonly string[] _candidateLabels;

        public HuggingFaceAnalysisService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;

            var apiKey = config["HuggingFaceApiKey"]
                         ?? throw new InvalidOperationException("HuggingFaceApiKey is missing.");

            _modelId = config["HuggingFace:ModelId"]
           ?? throw new InvalidOperationException("HuggingFace:ModelId is missing");

            _endpoint = config["HuggingFace:Endpoint"]
                ?? throw new InvalidOperationException("HuggingFace:Endpoint is missing");

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", apiKey);

            _candidateLabels = config.GetSection("HuggingFace:CandidateLabels").Get<string[]>()
                       ?? throw new InvalidOperationException("HuggingFace:CandidateLabels missing.");
        }

        public async Task<AnalyzeResponse> AnalyzeAsync(string action, string guideline)
        {
            var payload = new
            {
                inputs = action,
                parameters = new
                {
                    candidate_labels = _candidateLabels
                }
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_endpoint}/{_modelId}", content);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();
            var hfItems = JsonSerializer.Deserialize<HuggingFaceResponseItem[]>(
                            responseJson,
                            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (hfItems == null || hfItems.Length == 0)
                throw new InvalidOperationException("Invalid response from Hugging Face API");

            var bestItem = hfItems!.OrderByDescending(x => x.Score).First();

            string result = bestItem.Label.Equals("complies", StringComparison.OrdinalIgnoreCase)
                ? "COMPLIES"
                : bestItem.Label.Equals("deviates", StringComparison.OrdinalIgnoreCase)
                    ? "DEVIATES"
                    : "UNCLEAR";

            double confidence = Math.Round(bestItem.Score, 4);

            return new AnalyzeResponse(
                action,
                guideline,
                result,
                confidence,
                DateTime.UtcNow
            );
        }
    }
}
