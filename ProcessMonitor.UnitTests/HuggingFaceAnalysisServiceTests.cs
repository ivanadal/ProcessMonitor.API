using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using ProcessMonitor.Infrastructure.Services;
using System.Net;
using System.Text;
using System.Text.Json;

namespace ProcessMonitor.UnitTests;

[TestClass]
public sealed class HuggingFaceAnalysisServiceTests
{
    protected Mock<ILogger<HuggingFaceAnalysisService>> loggerMock;
    [TestInitialize]
    public void BaseSetup()
    {
        loggerMock = new Mock<ILogger<HuggingFaceAnalysisService>>();
    }
    private HttpClient CreateMockHttpClient(HttpStatusCode status, string responseJson, Action<HttpRequestMessage>? inspectRequest = null)
    {
        var handler = new Mock<HttpMessageHandler>();

        handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync((HttpRequestMessage request, CancellationToken token) =>
            {
                inspectRequest?.Invoke(request);

                return new HttpResponseMessage
                {
                    StatusCode = status,
                    Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
                };
            });

        return new HttpClient(handler.Object);
    }

    private IConfiguration BuildConfig(string endpoint = "https://api", string modelId = "model")
    {
        var dict = new Dictionary<string, string?>
        {
            ["HuggingFace:Endpoint"] = endpoint,
            ["HuggingFace:ModelId"] = modelId
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(dict!)
            .Build();
    }


    [TestMethod]
    public async Task AnalyzeAsync_ReturnsHighestScoreItem()
    {
        // Arrange
        string json = """
    [
        { "label": "A", "score": 0.1 },
        { "label": "B", "score": 0.9 },
        { "label": "C", "score": 0.5 }
    ]
    """;

        HttpClient client = CreateMockHttpClient(HttpStatusCode.OK, json);

        var config = BuildConfig("https://api", "model1");

        var service = new HuggingFaceAnalysisService(client, config, loggerMock.Object);

        // Act
        var result = await service.AnalyzeAsync("do something");

        // Assert
        Assert.AreEqual("B", result.Label);
        Assert.AreEqual(0.9, result.Score);
    }

    [TestMethod]
    public async Task AnalyzeAsync_SendsCorrectPayloadAndUrl()
    {
        string? capturedUrl = null;
        string? capturedBody = null;

        var client = CreateMockHttpClient(
            HttpStatusCode.OK,
            "[{\"label\":\"X\",\"score\":0.3}]",
            req =>
            {
                capturedUrl = req.RequestUri!.ToString();
                capturedBody = req.Content!.ReadAsStringAsync().Result;
            });

        var config = BuildConfig("https://api", "abc123");
        var service = new HuggingFaceAnalysisService(client, config, loggerMock.Object);

        await service.AnalyzeAsync("run");

        Assert.AreEqual("https://api/abc123", capturedUrl);
        Assert.Contains("\"inputs\":\"run\"", capturedBody);
        Assert.Contains("\"candidate_labels\"", capturedBody);
    }

    [TestMethod]
    public async Task AnalyzeAsync_ThrowsOnFailureStatusCode()
    {
        var client = CreateMockHttpClient(HttpStatusCode.BadRequest, "err");
        var config = BuildConfig();
        var service = new HuggingFaceAnalysisService(client, config, loggerMock.Object);

        await Assert.ThrowsAsync<HttpRequestException>(() =>
            service.AnalyzeAsync("x"));
    }

    //[TestMethod]
    //public async Task AnalyzeAsync_ThrowsIfDeserializedNull()
    //{
    //    var client = CreateMockHttpClient(HttpStatusCode.OK, "null");
    //    var config = BuildConfig();

    //    var service = new HuggingFaceAnalysisService(client, config, loggerMock.Object);

    //    await Assert.ThrowsAsync<NullReferenceException>(() =>
    //        service.AnalyzeAsync("test"));
    //}

    [TestMethod]
    public async Task AnalyzeAsync_ThrowsIfArrayEmpty()
    {
        var client = CreateMockHttpClient(HttpStatusCode.OK, "[]");
        var config = BuildConfig();

        var service = new HuggingFaceAnalysisService(client, config, loggerMock.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.AnalyzeAsync("test"));
    }

    [TestMethod]
    public async Task AnalyzeAsync_ChoosesLeastNegativeScore()
    {
        var client = CreateMockHttpClient(HttpStatusCode.OK, """
    [
        { "label": "A", "score": -10 },
        { "label": "B", "score": -1 },
        { "label": "C", "score": -5 }
    ]
    """);

        var config = BuildConfig();
        var service = new HuggingFaceAnalysisService(client, config, loggerMock.Object);

        var result = await service.AnalyzeAsync("x");

        Assert.AreEqual("B", result.Label);
        Assert.AreEqual(-1, result.Score);
    }

}
