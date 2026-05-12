using BDO.GWT.Core.DTOs;
using BDO.GWT.Web.Controllers;
using BDO.GWT.Web.Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BDO.GWT.GLOBAL.Tests.Controllers
{
    public class SiteConfigurationControllerTests
    {
        private Mock<ISiteConfigurationService> _siteConfigurationServiceMock = null!;
        private SiteConfigurationController _controller = null!;

        [SetUp]
        public void Setup()
        {
            _siteConfigurationServiceMock = new Mock<ISiteConfigurationService>();
            _controller = new SiteConfigurationController(_siteConfigurationServiceMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _controller?.Dispose();
        }

        [Test]
        public async Task GetSiteConfiguration_ReturnsNotModified_WhenHashMatchesCachedHash()
        {
            // Arrange
            const string hash = "abc123";

            _siteConfigurationServiceMock
                .Setup(x => x.GetCachedSiteConfigurationHash())
                .Returns(hash);

            // Act
            var actionResult = await _controller.GetSiteConfiguration(hash, CancellationToken.None);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(actionResult.Result, Is.InstanceOf<StatusCodeResult>());
                Assert.That(((StatusCodeResult)actionResult.Result!).StatusCode, Is.EqualTo(StatusCodes.Status304NotModified));
            });

            _siteConfigurationServiceMock.Verify(
                x => x.GetSiteConfigurationDtoAsync(It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public async Task GetSiteConfiguration_ReturnsConfiguration_WhenHashDiffersFromCachedHash()
        {
            // Arrange
            const string requestHash = "old-hash";
            const string cachedHash = "new-hash";

            var dto = new SiteConfigurationDto
            {
                Translations = new Dictionary<string, IDictionary<string, string>>
                {
                    ["en"] = new Dictionary<string, string> { ["greeting"] = "Hello" },
                },
                Hash = cachedHash,
            };

            _siteConfigurationServiceMock
                .Setup(x => x.GetCachedSiteConfigurationHash())
                .Returns(cachedHash);

            _siteConfigurationServiceMock
                .Setup(x => x.GetSiteConfigurationDtoAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(dto);

            // Act
            var actionResult = await _controller.GetSiteConfiguration(requestHash, CancellationToken.None);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(actionResult.Value, Is.SameAs(dto));
                Assert.That(actionResult.Result, Is.Null);
            });

            _siteConfigurationServiceMock.Verify(
                x => x.GetSiteConfigurationDtoAsync(It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task GetSiteConfiguration_ReturnsConfiguration_WhenCachedHashIsNull()
        {
            // Arrange
            var dto = new SiteConfigurationDto
            {
                Translations = new Dictionary<string, IDictionary<string, string>>(),
                Hash = "fresh-hash",
            };

            _siteConfigurationServiceMock
                .Setup(x => x.GetCachedSiteConfigurationHash())
                .Returns((string)null!);

            _siteConfigurationServiceMock
                .Setup(x => x.GetSiteConfigurationDtoAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(dto);

            // Act
            var actionResult = await _controller.GetSiteConfiguration("any-hash", CancellationToken.None);

            // Assert
            Assert.That(actionResult.Value, Is.SameAs(dto));

            _siteConfigurationServiceMock.Verify(
                x => x.GetSiteConfigurationDtoAsync(It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task GetSiteConfiguration_PassesCancellationTokenToService()
        {
            // Arrange
            using var cts = new CancellationTokenSource();
            var token = cts.Token;

            _siteConfigurationServiceMock
                .Setup(x => x.GetCachedSiteConfigurationHash())
                .Returns((string)null!);

            _siteConfigurationServiceMock
                .Setup(x => x.GetSiteConfigurationDtoAsync(token))
                .ReturnsAsync(new SiteConfigurationDto
                {
                    Translations = new Dictionary<string, IDictionary<string, string>>(),
                    Hash = "h",
                });

            // Act
            await _controller.GetSiteConfiguration("hash", token);

            // Assert
            _siteConfigurationServiceMock.Verify(
                x => x.GetSiteConfigurationDtoAsync(token),
                Times.Once);
        }

        [Test]
        public void GetSiteConfiguration_PropagatesException_FromService()
        {
            // Arrange
            _siteConfigurationServiceMock
                .Setup(x => x.GetCachedSiteConfigurationHash())
                .Returns((string)null!);

            _siteConfigurationServiceMock
                .Setup(x => x.GetSiteConfigurationDtoAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("boom"));

            // Act + Assert
            Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _controller.GetSiteConfiguration("hash", CancellationToken.None));
        }
    }
}
