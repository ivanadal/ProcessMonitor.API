using Microsoft.Extensions.Logging;
using Moq;
using ProcessMonitor.Domain.Entities;
using ProcessMonitor.Domain.Interfaces;
using ProcessMonitor.Domain.Services;
using ProcessMonitor.Infrastructure.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProcessMonitor.UnitTests
{
    [TestClass]
    public class AnalysisDomainServiceTests
    {
        private Mock<IAnalysisRepository> _repoMock;
        private Mock<IAIAnalysisService> _hfMock;
        private AnalysisDomainService _service;
        protected Mock<ILogger<AnalysisDomainService>> _loggerMock;

        [TestInitialize]
        public void Setup()
        {
            _repoMock = new Mock<IAnalysisRepository>();
            _repoMock.Setup(r => r.AddAsync(It.IsAny<Analysis>()))
                     .ReturnsAsync((Analysis a) => a);

            _hfMock = new Mock<IAIAnalysisService>();
            _loggerMock = new Mock<ILogger<AnalysisDomainService>>();

            _service = new AnalysisDomainService(_repoMock.Object, _hfMock.Object, _loggerMock.Object);
        }

        [TestMethod]
        public async Task AnalyzeAsync_ShouldReturn_COMPLIES()
        {
            // Arrange
            var action = "Closed ticket #48219 and sent confirmation email";
            var guideline = "All closed tickets must include a confirmation email";

            _hfMock.Setup(s => s.AnalyzeAsync(action))
                   .ReturnsAsync(new HuggingFaceResult
                   {
                       Label = "complies",
                       Score = 0.94
                   });

            // Act
            var result = await _service.AnalyzeAsync(action, guideline);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("COMPLIES", result.Result);
            Assert.AreEqual(0.94, result.Confidence, 0.0001);

            // Verify repository was called
            _repoMock.Verify(r => r.AddAsync(It.IsAny<Analysis>()), Times.Once);

            // Verify AI service was called
            _hfMock.Verify(s => s.AnalyzeAsync(action), Times.Once);
        }

        [TestMethod]
        public async Task AnalyzeAsync_ShouldReturn_DEVIATES()
        {
            // Arrange
            var action = "Did not send confirmation email";
            var guideline = "All closed tickets must include a confirmation email";

            _hfMock.Setup(s => s.AnalyzeAsync(action))
                   .ReturnsAsync(new HuggingFaceResult
                   {
                       Label = "deviates",
                       Score = 0.87
                   });

            // Act
            var result = await _service.AnalyzeAsync(action, guideline);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("DEVIATES", result.Result);
            Assert.AreEqual(0.87, result.Confidence, 0.0001);

            // Verify repository was called
            _repoMock.Verify(r => r.AddAsync(It.IsAny<Analysis>()), Times.Once);

            // Verify AI service was called
            _hfMock.Verify(s => s.AnalyzeAsync(action), Times.Once);
        }

        [TestMethod]
        public async Task AnalyzeAsync_ShouldReturn_UNCLEAR_ForUnknownLabel()
        {
            // Arrange
            var action = "Some ambiguous action";
            var guideline = "Some guideline";

            _hfMock.Setup(s => s.AnalyzeAsync(action))
                   .ReturnsAsync(new HuggingFaceResult
                   {
                       Label = "maybe",
                       Score = 0.5
                   });

            // Act
            var result = await _service.AnalyzeAsync(action, guideline);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("UNCLEAR", result.Result);
            Assert.AreEqual(0.5, result.Confidence, 0.0001);

            // Verify repository was called
            _repoMock.Verify(r => r.AddAsync(It.IsAny<Analysis>()), Times.Once);

            // Verify AI service was called
            _hfMock.Verify(s => s.AnalyzeAsync(action), Times.Once);
        }
    }

}
