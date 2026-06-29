using BDO.GWT.Core.Services;
using BDO.GWT.Core.ValueObjects;
using CMS.Websites;
using Kentico.Content.Web.Mvc;
using Moq;

namespace BDO.GWT.GLOBAL.Tests.Services
{
    public class PageUrlServiceTests
    {
        private Mock<IWebPageUrlRetriever> _pageUrlRetrieverMock = null!;
        private PageUrlService _service = null!;

        // A single reusable presentation URL for most tests.
        private static readonly AbsoluteLink PresentationUrl = new("https://example.com");

        [SetUp]
        public void SetUp()
        {
            _pageUrlRetrieverMock = new Mock<IWebPageUrlRetriever>();
            _service = new PageUrlService(_pageUrlRetrieverMock.Object);
        }

        // Creates a WebPageUrl whose RelativePath is the supplied tilde-prefixed path.
        private static WebPageUrl MakeWebPageUrl(string relativePath) =>
            new(relativePath, $"https://example.com/{relativePath.TrimStart('~').TrimStart('/')}");

        #region Interface shape

        [Test]
        public void Service_ImplementsIPageUrlService() =>
            Assert.That(typeof(IPageUrlService).IsAssignableFrom(typeof(PageUrlService)), Is.True);

        [Test]
        public void Service_CanBeInstantiated_WithMockedDependency()
        {
            Assert.That(_service, Is.Not.Null);
            Assert.That(_service, Is.InstanceOf<IPageUrlService>());
        }

        #endregion

        #region GetPageRelativeUrlAsync

        [Test]
        public async Task GetPageRelativeUrlAsync_ReturnsRelativeLink_WhenRetrieverSucceeds()
        {
            _pageUrlRetrieverMock
                .Setup(r => r.Retrieve(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(MakeWebPageUrl("~/en/about"));

            var result = await _service.GetPageRelativeUrlAsync(1, "en", CancellationToken.None);

            Assert.That(result, Is.InstanceOf<RelativeLink>());
        }

        [Test]
        public async Task GetPageRelativeUrlAsync_ReturnsCorrectPath()
        {
            _pageUrlRetrieverMock
                .Setup(r => r.Retrieve(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(MakeWebPageUrl("~/en/about-us"));

            var result = await _service.GetPageRelativeUrlAsync(1, "en", CancellationToken.None);

            Assert.That(result.ToHtmlCompatibleString(), Is.EqualTo("/en/about-us"));
        }

        [Test]
        public async Task GetPageRelativeUrlAsync_LowercasesRelativePath()
        {
            _pageUrlRetrieverMock
                .Setup(r => r.Retrieve(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(MakeWebPageUrl("~/EN/ABOUT-US"));

            var result = await _service.GetPageRelativeUrlAsync(1, "en", CancellationToken.None);

            Assert.That(result.ToHtmlCompatibleString(), Is.EqualTo("/en/about-us"));
        }

        [Test]
        public void GetPageRelativeUrlAsync_WhenRetrieverReturnsNull_ThrowsInvalidOperationException()
        {
            _pageUrlRetrieverMock
                .Setup(r => r.Retrieve(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((WebPageUrl?)null);

            Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _service.GetPageRelativeUrlAsync(99, "en", CancellationToken.None));
        }

        [Test]
        public void GetPageRelativeUrlAsync_ExceptionMessage_ContainsWebPageItemId()
        {
            _pageUrlRetrieverMock
                .Setup(r => r.Retrieve(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((WebPageUrl?)null);

            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _service.GetPageRelativeUrlAsync(123, "en", CancellationToken.None));

            Assert.That(ex!.Message, Does.Contain("123"));
        }

        [Test]
        public void GetPageRelativeUrlAsync_ExceptionMessage_ContainsLanguageTag()
        {
            _pageUrlRetrieverMock
                .Setup(r => r.Retrieve(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((WebPageUrl?)null);

            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _service.GetPageRelativeUrlAsync(1, "fr-CA", CancellationToken.None));

            Assert.That(ex!.Message, Does.Contain("fr-CA"));
        }

        [Test]
        public void GetPageRelativeUrlAsync_WhenCancelled_ThrowsOperationCanceledException()
        {
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            Assert.ThrowsAsync<OperationCanceledException>(async () =>
                await _service.GetPageRelativeUrlAsync(1, "en", cts.Token));
        }

        [Test]
        public async Task GetPageRelativeUrlAsync_PassesCorrectWebPageItemId_ToRetriever()
        {
            _pageUrlRetrieverMock
                .Setup(r => r.Retrieve(77, It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(MakeWebPageUrl("~/en/page"));

            await _service.GetPageRelativeUrlAsync(77, "en", CancellationToken.None);

            _pageUrlRetrieverMock.Verify(
                r => r.Retrieve(77, It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task GetPageRelativeUrlAsync_PassesCorrectLanguageTag_ToRetriever()
        {
            _pageUrlRetrieverMock
                .Setup(r => r.Retrieve(It.IsAny<int>(), "de", It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(MakeWebPageUrl("~/de/page"));

            await _service.GetPageRelativeUrlAsync(1, "de", CancellationToken.None);

            _pageUrlRetrieverMock.Verify(
                r => r.Retrieve(It.IsAny<int>(), "de", It.IsAny<bool>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task GetPageRelativeUrlAsync_PassesCancellationToken_ToRetriever()
        {
            using var cts = new CancellationTokenSource();
            var token = cts.Token;

            _pageUrlRetrieverMock
                .Setup(r => r.Retrieve(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>(), token))
                .ReturnsAsync(MakeWebPageUrl("~/en/page"));

            await _service.GetPageRelativeUrlAsync(1, "en", token);

            _pageUrlRetrieverMock.Verify(
                r => r.Retrieve(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>(), token),
                Times.Once);
        }

        #endregion

        #region GetPageAbsoluteUrlAsync

        [Test]
        public async Task GetPageAbsoluteUrlAsync_ReturnsAbsoluteLink()
        {
            _pageUrlRetrieverMock
                .Setup(r => r.Retrieve(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(MakeWebPageUrl("~/en/page"));

            var result = await _service.GetPageAbsoluteUrlAsync(
                PresentationUrl, 1, "en", CancellationToken.None);

            Assert.That(result, Is.InstanceOf<AbsoluteLink>());
        }

        [Test]
        public async Task GetPageAbsoluteUrlAsync_CombinesPresentationUrlAndRelativePath()
        {
            _pageUrlRetrieverMock
                .Setup(r => r.Retrieve(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(MakeWebPageUrl("~/en/contact"));

            var result = await _service.GetPageAbsoluteUrlAsync(
                PresentationUrl, 1, "en", CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.ToString(), Does.StartWith("https://example.com"));
                Assert.That(result.ToString(), Does.Contain("en/contact"));
            });
        }

        [Test]
        public async Task GetPageAbsoluteUrlAsync_LowercasesRelativePath()
        {
            _pageUrlRetrieverMock
                .Setup(r => r.Retrieve(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(MakeWebPageUrl("~/EN/UPPERCASE-SEGMENT"));

            var result = await _service.GetPageAbsoluteUrlAsync(
                PresentationUrl, 1, "en", CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.ToString(), Does.Not.Contain("UPPERCASE-SEGMENT"));
                Assert.That(result.ToString(), Does.Contain("en/uppercase-segment"));
            });
        }

        [Test]
        public void GetPageAbsoluteUrlAsync_WhenRetrieverReturnsNull_ThrowsInvalidOperationException()
        {
            _pageUrlRetrieverMock
                .Setup(r => r.Retrieve(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((WebPageUrl?)null);

            Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _service.GetPageAbsoluteUrlAsync(PresentationUrl, 1, "en", CancellationToken.None));
        }

        [Test]
        public void GetPageAbsoluteUrlAsync_WhenCancelled_ThrowsOperationCanceledException()
        {
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            Assert.ThrowsAsync<OperationCanceledException>(async () =>
                await _service.GetPageAbsoluteUrlAsync(PresentationUrl, 1, "en", cts.Token));
        }

        #endregion

        #region GetPageAbsoluteUrlByLanguageTagAsync

        [Test]
        public async Task GetPageAbsoluteUrlByLanguageTagAsync_ReturnsOneEntryPerLanguageTag()
        {
            _pageUrlRetrieverMock
                .Setup(r => r.Retrieve(It.IsAny<int>(), "en", It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(MakeWebPageUrl("~/en/page"));

            _pageUrlRetrieverMock
                .Setup(r => r.Retrieve(It.IsAny<int>(), "fr", It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(MakeWebPageUrl("~/fr/page"));

            var result = await _service.GetPageAbsoluteUrlByLanguageTagAsync(
                PresentationUrl, 1, ["en", "fr"], CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result, Has.Count.EqualTo(2));
                Assert.That(result.ContainsKey("en"), Is.True);
                Assert.That(result.ContainsKey("fr"), Is.True);
            });
        }

        [Test]
        public async Task GetPageAbsoluteUrlByLanguageTagAsync_SkipsLanguage_WhenRetrieverReturnsNull()
        {
            _pageUrlRetrieverMock
                .Setup(r => r.Retrieve(It.IsAny<int>(), "en", It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(MakeWebPageUrl("~/en/page"));

            _pageUrlRetrieverMock
                .Setup(r => r.Retrieve(It.IsAny<int>(), "de", It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((WebPageUrl?)null);

            var result = await _service.GetPageAbsoluteUrlByLanguageTagAsync(
                PresentationUrl, 1, ["en", "de"], CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result, Has.Count.EqualTo(1));
                Assert.That(result.ContainsKey("en"), Is.True);
                Assert.That(result.ContainsKey("de"), Is.False);
            });
        }

        [Test]
        public async Task GetPageAbsoluteUrlByLanguageTagAsync_DeduplicatesLanguageTags_CaseInsensitive()
        {
            _pageUrlRetrieverMock
                .Setup(r => r.Retrieve(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(MakeWebPageUrl("~/en/page"));

            var result = await _service.GetPageAbsoluteUrlByLanguageTagAsync(
                PresentationUrl, 1, ["en", "EN", "En"], CancellationToken.None);

            Assert.That(result, Has.Count.EqualTo(1));
            _pageUrlRetrieverMock.Verify(
                r => r.Retrieve(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task GetPageAbsoluteUrlByLanguageTagAsync_EmptyLanguageTags_ReturnsEmptyDictionary()
        {
            var result = await _service.GetPageAbsoluteUrlByLanguageTagAsync(
                PresentationUrl, 1, [], CancellationToken.None);

            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task GetPageAbsoluteUrlByLanguageTagAsync_ValuesAreAbsoluteLinks()
        {
            _pageUrlRetrieverMock
                .Setup(r => r.Retrieve(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(MakeWebPageUrl("~/en/page"));

            var result = await _service.GetPageAbsoluteUrlByLanguageTagAsync(
                PresentationUrl, 1, ["en"], CancellationToken.None);

            Assert.That(result["en"], Is.InstanceOf<AbsoluteLink>());
        }

        [Test]
        public async Task GetPageAbsoluteUrlByLanguageTagAsync_UrlContainsPresentationHost()
        {
            _pageUrlRetrieverMock
                .Setup(r => r.Retrieve(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(MakeWebPageUrl("~/en/page"));

            var result = await _service.GetPageAbsoluteUrlByLanguageTagAsync(
                PresentationUrl, 1, ["en"], CancellationToken.None);

            Assert.That(result["en"].ToString(), Does.StartWith("https://example.com"));
        }

        [Test]
        public void GetPageAbsoluteUrlByLanguageTagAsync_WhenCancelled_ThrowsOperationCanceledException()
        {
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            Assert.ThrowsAsync<OperationCanceledException>(async () =>
                await _service.GetPageAbsoluteUrlByLanguageTagAsync(
                    PresentationUrl, 1, ["en"], cts.Token));
        }

        #endregion

        #region GetPageRelativeUrlByNodeIdAsync

        [Test]
        public async Task GetPageRelativeUrlByNodeIdAsync_EmptyIds_ReturnsEmptyDictionary()
        {
            var result = await _service.GetPageRelativeUrlByNodeIdAsync(
                [], "en", CancellationToken.None);

            Assert.That(result, Is.Empty);
            _pageUrlRetrieverMock.Verify(
                r => r.Retrieve(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public async Task GetPageRelativeUrlByNodeIdAsync_ReturnsRelativeLinkPerFoundId()
        {
            _pageUrlRetrieverMock
                .Setup(r => r.Retrieve(10, It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(MakeWebPageUrl("~/en/page-10"));

            _pageUrlRetrieverMock
                .Setup(r => r.Retrieve(20, It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(MakeWebPageUrl("~/en/page-20"));

            var result = await _service.GetPageRelativeUrlByNodeIdAsync(
                [10, 20], "en", CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result, Has.Count.EqualTo(2));
                Assert.That(result.ContainsKey(10), Is.True);
                Assert.That(result.ContainsKey(20), Is.True);
            });
        }

        [Test]
        public async Task GetPageRelativeUrlByNodeIdAsync_SkipsId_WhenRetrieverReturnsNull()
        {
            _pageUrlRetrieverMock
                .Setup(r => r.Retrieve(10, It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(MakeWebPageUrl("~/en/found"));

            _pageUrlRetrieverMock
                .Setup(r => r.Retrieve(99, It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((WebPageUrl?)null);

            var result = await _service.GetPageRelativeUrlByNodeIdAsync(
                [10, 99], "en", CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result, Has.Count.EqualTo(1));
                Assert.That(result.ContainsKey(10), Is.True);
                Assert.That(result.ContainsKey(99), Is.False);
            });
        }

        [Test]
        public async Task GetPageRelativeUrlByNodeIdAsync_DeduplicatesIds_BeforeCallingRetriever()
        {
            _pageUrlRetrieverMock
                .Setup(r => r.Retrieve(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(MakeWebPageUrl("~/en/page"));

            var result = await _service.GetPageRelativeUrlByNodeIdAsync(
                [5, 5, 5], "en", CancellationToken.None);

            Assert.That(result, Has.Count.EqualTo(1));
            _pageUrlRetrieverMock.Verify(
                r => r.Retrieve(5, It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task GetPageRelativeUrlByNodeIdAsync_LowercasesRelativePath()
        {
            _pageUrlRetrieverMock
                .Setup(r => r.Retrieve(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(MakeWebPageUrl("~/EN/UPPERCASE-PATH"));

            var result = await _service.GetPageRelativeUrlByNodeIdAsync(
                [1], "en", CancellationToken.None);

            Assert.That(result[1].ToHtmlCompatibleString(), Is.EqualTo("/en/uppercase-path"));
        }

        [Test]
        public async Task GetPageRelativeUrlByNodeIdAsync_ValuesAreRelativeLinks()
        {
            _pageUrlRetrieverMock
                .Setup(r => r.Retrieve(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(MakeWebPageUrl("~/en/page"));

            var result = await _service.GetPageRelativeUrlByNodeIdAsync(
                [1], "en", CancellationToken.None);

            Assert.That(result[1], Is.InstanceOf<RelativeLink>());
        }

        [Test]
        public void GetPageRelativeUrlByNodeIdAsync_WhenCancelled_ThrowsOperationCanceledException()
        {
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            // Cancellation check fires before any IDs are processed.
            Assert.ThrowsAsync<OperationCanceledException>(async () =>
                await _service.GetPageRelativeUrlByNodeIdAsync([1], "en", cts.Token));
        }

        [Test]
        public void GetPageRelativeUrlByNodeIdAsync_WhenCancelled_WithEmptyIds_ThrowsOperationCanceledException()
        {
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            // ThrowIfCancellationRequested fires before the empty-list early-return.
            Assert.ThrowsAsync<OperationCanceledException>(async () =>
                await _service.GetPageRelativeUrlByNodeIdAsync([], "en", cts.Token));
        }

        [Test]
        public async Task GetPageRelativeUrlByNodeIdAsync_PassesCorrectLanguageTag_ToRetriever()
        {
            _pageUrlRetrieverMock
                .Setup(r => r.Retrieve(It.IsAny<int>(), "ja", It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(MakeWebPageUrl("~/ja/page"));

            await _service.GetPageRelativeUrlByNodeIdAsync([1], "ja", CancellationToken.None);

            _pageUrlRetrieverMock.Verify(
                r => r.Retrieve(It.IsAny<int>(), "ja", It.IsAny<bool>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        #endregion

        #region GetPageAbsoluteUrlByNodeIdAsync

        [Test]
        public async Task GetPageAbsoluteUrlByNodeIdAsync_EmptyInput_ReturnsEmptyDictionary()
        {
            var result = await _service.GetPageAbsoluteUrlByNodeIdAsync(
                new Dictionary<AbsoluteLink, IEnumerable<int>>(), "en", CancellationToken.None);

            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task GetPageAbsoluteUrlByNodeIdAsync_ReturnsAbsoluteLinkPerPageId()
        {
            _pageUrlRetrieverMock
                .Setup(r => r.Retrieve(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(MakeWebPageUrl("~/en/page"));

            var result = await _service.GetPageAbsoluteUrlByNodeIdAsync(
                new Dictionary<AbsoluteLink, IEnumerable<int>> { [PresentationUrl] = [1, 2] },
                "en", CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result, Has.Count.EqualTo(2));
                Assert.That(result.ContainsKey(1), Is.True);
                Assert.That(result.ContainsKey(2), Is.True);
            });
        }

        [Test]
        public async Task GetPageAbsoluteUrlByNodeIdAsync_ValuesAreAbsoluteLinks()
        {
            _pageUrlRetrieverMock
                .Setup(r => r.Retrieve(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(MakeWebPageUrl("~/en/page"));

            var result = await _service.GetPageAbsoluteUrlByNodeIdAsync(
                new Dictionary<AbsoluteLink, IEnumerable<int>> { [PresentationUrl] = [1] },
                "en", CancellationToken.None);

            Assert.That(result[1], Is.InstanceOf<AbsoluteLink>());
        }

        [Test]
        public async Task GetPageAbsoluteUrlByNodeIdAsync_CombinesPresentationUrlAndRelativePath()
        {
            _pageUrlRetrieverMock
                .Setup(r => r.Retrieve(1, It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(MakeWebPageUrl("~/en/services"));

            var result = await _service.GetPageAbsoluteUrlByNodeIdAsync(
                new Dictionary<AbsoluteLink, IEnumerable<int>> { [PresentationUrl] = [1] },
                "en", CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result[1].ToString(), Does.StartWith("https://example.com"));
                Assert.That(result[1].ToString(), Does.Contain("en/services"));
            });
        }

        [Test]
        public async Task GetPageAbsoluteUrlByNodeIdAsync_AggregatesAcrossMultiplePresentationUrls()
        {
            var secondUrl = new AbsoluteLink("https://second.example.com");

            _pageUrlRetrieverMock
                .Setup(r => r.Retrieve(1, It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(MakeWebPageUrl("~/en/page-1"));

            _pageUrlRetrieverMock
                .Setup(r => r.Retrieve(2, It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(MakeWebPageUrl("~/en/page-2"));

            var input = new Dictionary<AbsoluteLink, IEnumerable<int>>
            {
                [PresentationUrl] = [1],
                [secondUrl] = [2],
            };

            var result = await _service.GetPageAbsoluteUrlByNodeIdAsync(input, "en", CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result, Has.Count.EqualTo(2));
                Assert.That(result[1].ToString(), Does.Contain("example.com"));
                Assert.That(result[2].ToString(), Does.Contain("second.example.com"));
            });
        }

        [Test]
        public async Task GetPageAbsoluteUrlByNodeIdAsync_SkipsPageId_WhenRetrieverReturnsNull()
        {
            _pageUrlRetrieverMock
                .Setup(r => r.Retrieve(1, It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(MakeWebPageUrl("~/en/found"));

            _pageUrlRetrieverMock
                .Setup(r => r.Retrieve(99, It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((WebPageUrl?)null);

            var result = await _service.GetPageAbsoluteUrlByNodeIdAsync(
                new Dictionary<AbsoluteLink, IEnumerable<int>> { [PresentationUrl] = [1, 99] },
                "en", CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result, Has.Count.EqualTo(1));
                Assert.That(result.ContainsKey(1), Is.True);
                Assert.That(result.ContainsKey(99), Is.False);
            });
        }

        #endregion
    }
}