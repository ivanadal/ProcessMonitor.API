using ProcessMonitor.API.Models;
using System.Net.Http;
using System.Net.Http.Headers;

namespace ProcessMonitor.API.Services
{
    public class HuggingFaceAnalysisService : IAnalysisService
    {
        private readonly HttpClient _httpClient;
        public HuggingFaceAnalysisService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;

            var apiKey = config["HuggingFaceApiKey"]
                         ?? throw new InvalidOperationException("HuggingFaceApiKey is missing.");

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", apiKey);
        }
        public Task<AnalyzeResult> AnalyzeAsync(string action, string guideline)
        {
            throw new NotImplementedException();
        }
    }
}
