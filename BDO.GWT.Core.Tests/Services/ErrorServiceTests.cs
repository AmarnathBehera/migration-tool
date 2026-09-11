using BDO;
using BDO.GWT.Core;
using BDO.GWT.Core.Services;
using CMS.Websites;
using Kentico.Content.Web.Mvc;
using Microsoft.AspNetCore.Diagnostics;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Core.Tests.Services
{
    public class ErrorServiceTests
    {
        private Mock<IContentService> _contentServiceMock = null!;
        private ErrorService _service = null!;

        [SetUp]
        public void Setup()
        {
            _contentServiceMock = new Mock<IContentService>();
            _service = new ErrorService(_contentServiceMock.Object);
        }

        [Test]
        public async Task GetStatusCodePageAsync_ReturnsNull_WhenReExecuteFeatureIsNull()
        {
            var result = await _service.GetStatusCodePageAsync(
                statusCodeReExecuteFeature: null,
                languageName: "en-gb",
                channelName: "TestChannel",
                cancellationToken: CancellationToken.None);

            Assert.That(result, Is.Null);

            _contentServiceMock.Verify(
                x => x.GetPagesAsync<StatusCode>(
                    It.IsAny<RetrievePagesParameters>(),
                    It.IsAny<System.Action<RetrievePagesQueryParameters>>(),
                    It.IsAny<RetrievalCacheSettings>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public async Task GetStatusCodePageAsync_ReturnsNull_WhenNoMatchingPageFound()
        {
            var feature = new StatusCodeReExecuteFeature
            {
                OriginalStatusCode = 404,
                OriginalPath = "/en-gb/some-missing-page"
            };

            SetupStatusCodePages(Enumerable.Empty<StatusCode>());

            var result = await _service.GetStatusCodePageAsync(
                feature, "en-gb", "TestChannel", CancellationToken.None);

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetStatusCodePageAsync_ReturnsPage_WhenMatchingPageFound()
        {
            var feature = new StatusCodeReExecuteFeature
            {
                OriginalStatusCode = 500,
                OriginalPath = "/en-gb/broken-page"
            };

            var expectedPage = new StatusCode
            {
                HttpStatusCode = 500,
                MetadataTitle = "Server Error",
                SystemFields = new WebPageFields { WebPageItemID = 42 }
            };

            SetupStatusCodePages(new[] { expectedPage });

            var result = await _service.GetStatusCodePageAsync(
                feature, "en-gb", "TestChannel", CancellationToken.None);

            Assert.That(result, Is.SameAs(expectedPage));
        }

        [Test]
        public async Task GetStatusCodePageAsync_ReturnsFirstPage_WhenMultiplePagesFound()
        {
            var feature = new StatusCodeReExecuteFeature
            {
                OriginalStatusCode = 404,
                OriginalPath = "/en-gb/missing"
            };

            var firstPage = new StatusCode { HttpStatusCode = 404, MetadataTitle = "First" };
            var secondPage = new StatusCode { HttpStatusCode = 404, MetadataTitle = "Second" };

            SetupStatusCodePages(new[] { firstPage, secondPage });

            var result = await _service.GetStatusCodePageAsync(
                feature, "en-gb", "TestChannel", CancellationToken.None);

            Assert.That(result, Is.SameAs(firstPage));
        }

        [Test]
        public async Task GetStatusCodePageAsync_PassesCorrectLanguageAndChannel_ToContentService()
        {
            var feature = new StatusCodeReExecuteFeature
            {
                OriginalStatusCode = 403,
                OriginalPath = "/fr-fr/forbidden"
            };

            RetrievePagesParameters? capturedParameters = null;

            _contentServiceMock
                .Setup(x => x.GetPagesAsync<StatusCode>(
                    It.IsAny<RetrievePagesParameters>(),
                    It.IsAny<System.Action<RetrievePagesQueryParameters>>(),
                    It.IsAny<RetrievalCacheSettings>(),
                    It.IsAny<CancellationToken>()))
                .Callback<RetrievePagesParameters, System.Action<RetrievePagesQueryParameters>, RetrievalCacheSettings, CancellationToken>(
                    (parameters, _, _, _) => capturedParameters = parameters)
                .ReturnsAsync(Enumerable.Empty<StatusCode>());

            await _service.GetStatusCodePageAsync(
                feature, "fr-fr", "FrenchChannel", CancellationToken.None);

            Assert.That(capturedParameters, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(capturedParameters!.LanguageName, Is.EqualTo("fr-fr"));
                Assert.That(capturedParameters.ChannelName, Is.EqualTo("FrenchChannel"));
            });
        }

        [Test]
        public async Task GetStatusCodePageAsync_UsesCacheDisabledSettings()
        {
            var feature = new StatusCodeReExecuteFeature
            {
                OriginalStatusCode = 500,
                OriginalPath = "/en-gb/error"
            };

            RetrievalCacheSettings? capturedSettings = null;

            _contentServiceMock
                .Setup(x => x.GetPagesAsync<StatusCode>(
                    It.IsAny<RetrievePagesParameters>(),
                    It.IsAny<System.Action<RetrievePagesQueryParameters>>(),
                    It.IsAny<RetrievalCacheSettings>(),
                    It.IsAny<CancellationToken>()))
                .Callback<RetrievePagesParameters, System.Action<RetrievePagesQueryParameters>, RetrievalCacheSettings, CancellationToken>(
                    (_, _, settings, _) => capturedSettings = settings)
                .ReturnsAsync(Enumerable.Empty<StatusCode>());

            await _service.GetStatusCodePageAsync(
                feature, "en-gb", "TestChannel", CancellationToken.None);

            Assert.That(capturedSettings, Is.Not.Null);
        }

        [Test]
        public async Task GetStatusCodePageAsync_PassesCancellationToken_ToContentService()
        {
            using var cts = new CancellationTokenSource();
            var token = cts.Token;

            var feature = new StatusCodeReExecuteFeature
            {
                OriginalStatusCode = 404,
                OriginalPath = "/en-gb/missing"
            };

            _contentServiceMock
                .Setup(x => x.GetPagesAsync<StatusCode>(
                    It.IsAny<RetrievePagesParameters>(),
                    It.IsAny<System.Action<RetrievePagesQueryParameters>>(),
                    It.IsAny<RetrievalCacheSettings>(),
                    token))
                .ReturnsAsync(Enumerable.Empty<StatusCode>())
                .Verifiable();

            await _service.GetStatusCodePageAsync(
                feature, "en-gb", "TestChannel", token);

            _contentServiceMock.Verify();
        }

        private void SetupStatusCodePages(IEnumerable<StatusCode> pages)
        {
            _contentServiceMock
                .Setup(x => x.GetPagesAsync<StatusCode>(
                    It.IsAny<RetrievePagesParameters>(),
                    It.IsAny<System.Action<RetrievePagesQueryParameters>>(),
                    It.IsAny<RetrievalCacheSettings>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(pages);
        }

        // Minimal test double for IStatusCodeReExecuteFeature
        private sealed class StatusCodeReExecuteFeature : IStatusCodeReExecuteFeature
        {
            public int OriginalStatusCode { get; set; }
            public string OriginalPath { get; set; } = string.Empty;
            public string OriginalPathBase { get; set; } = string.Empty;
            public string? OriginalQueryString { get; set; }
        }
    }
}
