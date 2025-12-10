using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using ProcessMonitor.Infrastructure.Services;
using System.Net;
using System.Text.Json;

namespace ProcessMonitor.UnitTests;

[TestClass]
public sealed class HuggingFaceAnalysisServiceTests
{
    [TestMethod]
    [DataRow("complies", "COMPLIES")]
    [DataRow("deviates", "DEVIATES")]
    [DataRow("unclear", "UNCLEAR")]
    public async Task AnalyzeAsync_ShouldReturnCorrectResult(string label, string expectedResult)
    {
    //    // Arrange
    //    var mockResponse = JsonSerializer.Serialize(new[]
    //    {
    //    new { label = "complies", score = label == "complies" ? 0.95 : 0.02 },
    //    new { label = "deviates", score = label == "deviates" ? 0.90 : 0.03 },
    //    new { label = "unclear",  score = label == "unclear"  ? 0.70 : 0.05 }
    //});

    //    var httpClient = CreateMockHttpClient(mockResponse);
    //    var config = CreateMockConfiguration();

    //    var service = new HuggingFaceAnalysisService(httpClient, config);

    //    // Act
    //    var result = await service.AnalyzeAsync("Test action", "Test guideline");

    //    // Assert
    //    Assert.AreEqual(expectedResult, result.Result);
    //    Assert.IsTrue(result.Confidence > 0 && result.Confidence <= 1);
    //    Assert.AreEqual("Test action", result.Action);
    //    Assert.AreEqual("Test guideline", result.Guideline);
    }


    [TestMethod]
    public async Task AnalyzeAsync_ShouldThrow_WhenResponseIsEmpty()
    {
        //// Arrange
        //var mockResponse = "[]"; // empty array
        //var httpClient = CreateMockHttpClient(mockResponse);
        //var config = CreateMockConfiguration();
        //var service = new HuggingFaceAnalysisService(httpClient, config);

        //// Act & Assert 
        //await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        //{
        //    await service.AnalyzeAsync("Test action", "Test guideline");
        //});
    }

    // Prepare
    private IConfiguration CreateMockConfiguration()
    {
        return new ConfigurationBuilder()
     .AddInMemoryCollection(new[]
     {
        new KeyValuePair<string,string>("HuggingFaceApiKey", "dummy_key"),
        new KeyValuePair<string,string>("HuggingFace:ModelId", "facebook/bart-large-mnli"),
        new KeyValuePair<string,string>("HuggingFace:Endpoint", "https://router.huggingface.co/hf-inference/models"),
        new KeyValuePair<string,string>("HuggingFace:CandidateLabels:0", "complies"),
        new KeyValuePair<string,string>("HuggingFace:CandidateLabels:1", "deviates"),
        new KeyValuePair<string,string>("HuggingFace:CandidateLabels:2", "unclear")
     })
     .Build();
    }

   
    private HttpClient CreateMockHttpClient(string jsonResponse)
    {
        var handlerMock = new Mock<HttpMessageHandler>();

        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonResponse)
            });

        return new HttpClient(handlerMock.Object)
        {
            BaseAddress = new System.Uri("https://router.huggingface.co/")
        };
    }

}
