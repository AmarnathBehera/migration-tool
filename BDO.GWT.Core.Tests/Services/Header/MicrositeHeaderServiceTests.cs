using System.Reflection;
using BDO;
using BDO.GWT.Core;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Header;
using BDO.GWT.Core.Services;
using BDO.GWT.Core.Services.Header;
using CMS.Websites;
using CMS.Websites.Routing;
using Kentico.Content.Web.Mvc;
using Microsoft.AspNetCore.Http;
using Moq;

namespace BDO.GWT.Core.Tests.Services.Header
{
    public class MicrositeHeaderServiceTests
    {
        private Mock<IContentService> _contentServiceMock = null!;
        private Mock<IHomePageService> _homePageServiceMock = null!;
        private Mock<IWebsiteCulturesService> _websiteCulturesServiceMock = null!;
        private Mock<IWebsiteChannelContext> _websiteChannelContextMock = null!;
        private Mock<ILocalizedUrlsBuilder> _localizedUrlsBuilderMock = null!;
        private Mock<IHttpContextAccessor> _httpContextAccessorMock = null!;
        private Mock<IWebPageDataContextRetriever> _dataContextRetrieverMock = null!;
        private Mock<IWebPageUrlResolver> _urlResolverMock = null!;
        private Mock<IPageContext> _pageContextMock = null!;
        private Mock<IHeaderService> _headerServiceMock = null!;
        private MicrositeHeaderService _service = null!;

        [SetUp]
        public void Setup()
        {
            _contentServiceMock = new Mock<IContentService>();
            _homePageServiceMock = new Mock<IHomePageService>();
            _websiteCulturesServiceMock = new Mock<IWebsiteCulturesService>();
            _websiteChannelContextMock = new Mock<IWebsiteChannelContext>();
            _localizedUrlsBuilderMock = new Mock<ILocalizedUrlsBuilder>();
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            _dataContextRetrieverMock = new Mock<IWebPageDataContextRetriever>();
            _urlResolverMock = new Mock<IWebPageUrlResolver>();
            _pageContextMock = new Mock<IPageContext>();
            _headerServiceMock = new Mock<IHeaderService>();

            _service = new MicrositeHeaderService(
                _contentServiceMock.Object,
                _homePageServiceMock.Object,
                _websiteCulturesServiceMock.Object,
                _websiteChannelContextMock.Object,
                _localizedUrlsBuilderMock.Object,
                _httpContextAccessorMock.Object,
                _dataContextRetrieverMock.Object,
                _urlResolverMock.Object,
                _pageContextMock.Object,
                _headerServiceMock.Object
            );
        }

        // ── GetAsync ─────────────────────────────────────────────────────────

        [Test]
        public async Task GetAsync_WhenNoDataContext_ReturnsHeaderWithDefaults()
        {
            // Arrange — master page can't be resolved (TryRetrieve returns false)
            _dataContextRetrieverMock
                .Setup(r => r.TryRetrieve(out It.Ref<WebPageDataContext>.IsAny))
                .Returns(false);

            _homePageServiceMock
                .Setup(x => x.GetHomePageUrl(It.IsAny<CancellationToken>()))
                .ReturnsAsync("/home");

            _websiteCulturesServiceMock
                .Setup(x => x.GetDisplayName(_websiteChannelContextMock.Object))
                .Returns("BDO Site");

            _contentServiceMock
                .Setup(x => x.GetCurrentSiteLogoImageUrlAsync())
                .Returns("https://cdn/logo.png");

            _httpContextAccessorMock
                .SetupGet(x => x.HttpContext)
                .Returns(new DefaultHttpContext());

            _localizedUrlsBuilderMock
                .Setup(x => x.BuildAsync(It.IsAny<HttpContext>()))
                .ReturnsAsync(new List<LocalizedUrl>());

            // Act
            var result = await _service.GetAsync(_contentServiceMock.Object, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result!.SiteName, Is.EqualTo("BDO Site"));
                Assert.That(result.LogoURL, Is.EqualTo("/home"));
                Assert.That(result.LogoImageURL, Is.EqualTo("https://cdn/logo.png"));
                Assert.That(result.SearchPageURL, Is.EqualTo(string.Empty));
                Assert.That(result.MegaMenuItems, Is.Not.Null.And.Empty);
                Assert.That(result.MenuItems, Is.Not.Null.And.Empty);
                Assert.That(result.Contact, Is.Null);
                Assert.That(result.Cultures, Is.Not.Null.And.Empty);
            });
        }

        [Test]
        public async Task GetAsync_WhenHttpContextIsNull_ReturnsEmptyCultures()
        {
            _dataContextRetrieverMock
                .Setup(r => r.TryRetrieve(out It.Ref<WebPageDataContext>.IsAny))
                .Returns(false);

            _homePageServiceMock
                .Setup(x => x.GetHomePageUrl(It.IsAny<CancellationToken>()))
                .ReturnsAsync("/home");

            _websiteCulturesServiceMock
                .Setup(x => x.GetDisplayName(It.IsAny<IWebsiteChannelContext>()))
                .Returns("BDO Site");

            _contentServiceMock
                .Setup(x => x.GetCurrentSiteLogoImageUrlAsync())
                .Returns("https://cdn/logo.png");

            _httpContextAccessorMock
                .SetupGet(x => x.HttpContext)
                .Returns((HttpContext?)null);

            var result = await _service.GetAsync(_contentServiceMock.Object, CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Cultures, Is.Not.Null.And.Empty);
            _localizedUrlsBuilderMock.Verify(b => b.BuildAsync(It.IsAny<HttpContext>()), Times.Never);
        }

        [Test]
        public async Task GetAsync_WhenHomePageUrlIsNull_FallsBackToEmptyLogoUrl()
        {
            _dataContextRetrieverMock
                .Setup(r => r.TryRetrieve(out It.Ref<WebPageDataContext>.IsAny))
                .Returns(false);

            _homePageServiceMock
                .Setup(x => x.GetHomePageUrl(It.IsAny<CancellationToken>()))
                .ReturnsAsync((string?)null);

            _websiteCulturesServiceMock
                .Setup(x => x.GetDisplayName(It.IsAny<IWebsiteChannelContext>()))
                .Returns("BDO Site");

            _contentServiceMock
                .Setup(x => x.GetCurrentSiteLogoImageUrlAsync())
                .Returns(string.Empty);

            _httpContextAccessorMock
                .SetupGet(x => x.HttpContext)
                .Returns(new DefaultHttpContext());

            _localizedUrlsBuilderMock
                .Setup(x => x.BuildAsync(It.IsAny<HttpContext>()))
                .ReturnsAsync(new List<LocalizedUrl>());

            var result = await _service.GetAsync(_contentServiceMock.Object, CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.LogoURL, Is.EqualTo(string.Empty));
        }

        [Test]
        public async Task GetAsync_MapsCulturesFromLocalizedUrls()
        {
            _dataContextRetrieverMock
                .Setup(r => r.TryRetrieve(out It.Ref<WebPageDataContext>.IsAny))
                .Returns(false);

            _homePageServiceMock
                .Setup(x => x.GetHomePageUrl(It.IsAny<CancellationToken>()))
                .ReturnsAsync("/home");

            _websiteCulturesServiceMock
                .Setup(x => x.GetDisplayName(It.IsAny<IWebsiteChannelContext>()))
                .Returns("BDO Site");

            _contentServiceMock
                .Setup(x => x.GetCurrentSiteLogoImageUrlAsync())
                .Returns("https://cdn/logo.png");

            _httpContextAccessorMock
                .SetupGet(x => x.HttpContext)
                .Returns(new DefaultHttpContext());

            _localizedUrlsBuilderMock
                .Setup(x => x.BuildAsync(It.IsAny<HttpContext>()))
                .ReturnsAsync(new List<LocalizedUrl>
                {
                    new()
                    {
                        LanguageName = "en",
                        CultureFormat = "en-US",
                        DisplayName = "English",
                        ShortCode = "EN",
                        RelativeUrl = "/en",
                        AbsoluteUrl = "https://localhost/en",
                        IsDefault = true,
                        IsActive = true,
                    },
                    new()
                    {
                        LanguageName = "lt",
                        CultureFormat = "lt-LT",
                        DisplayName = "Lithuanian",
                        ShortCode = "LT",
                        RelativeUrl = "/lt",
                        AbsoluteUrl = "https://localhost/lt",
                    },
                });

            var result = await _service.GetAsync(_contentServiceMock.Object, CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Cultures.Count(), Is.EqualTo(2));
            Assert.Multiple(() =>
            {
                Assert.That(result.Cultures.ElementAt(0).Name, Is.EqualTo("English"));
                Assert.That(result.Cultures.ElementAt(0).ShortName, Is.EqualTo("EN"));
                Assert.That(result.Cultures.ElementAt(0).Url, Is.EqualTo("/en"));
                Assert.That(result.Cultures.ElementAt(1).Name, Is.EqualTo("Lithuanian"));
                Assert.That(result.Cultures.ElementAt(1).ShortName, Is.EqualTo("LT"));
                Assert.That(result.Cultures.ElementAt(1).Url, Is.EqualTo("/lt"));
            });
        }

        [Test]
        public void GetAsync_PropagatesException_FromLocalizedUrlsBuilder()
        {
            _dataContextRetrieverMock
                .Setup(r => r.TryRetrieve(out It.Ref<WebPageDataContext>.IsAny))
                .Returns(false);

            _homePageServiceMock
                .Setup(x => x.GetHomePageUrl(It.IsAny<CancellationToken>()))
                .ReturnsAsync("/home");

            _websiteCulturesServiceMock
                .Setup(x => x.GetDisplayName(It.IsAny<IWebsiteChannelContext>()))
                .Returns("BDO Site");

            _contentServiceMock
                .Setup(x => x.GetCurrentSiteLogoImageUrlAsync())
                .Returns(string.Empty);

            _httpContextAccessorMock
                .SetupGet(x => x.HttpContext)
                .Returns(new DefaultHttpContext());

            _localizedUrlsBuilderMock
                .Setup(x => x.BuildAsync(It.IsAny<HttpContext>()))
                .ThrowsAsync(new InvalidOperationException("boom"));

            Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _service.GetAsync(_contentServiceMock.Object, CancellationToken.None));
        }

        // ── GetMicrositeMasterPageAsync (private) ────────────────────────────

        [Test]
        public async Task GetMicrositeMasterPageAsync_WhenNoDataContext_ReturnsNull()
        {
            _dataContextRetrieverMock
                .Setup(r => r.TryRetrieve(out It.Ref<WebPageDataContext>.IsAny))
                .Returns(false);

            var result = await InvokePrivateAsync<MicrositeMasterPage?>(
                "GetMicrositeMasterPageAsync",
                new object[] { CancellationToken.None });

            Assert.That(result, Is.Null);
        }

        // ── BuildMegaMenuAsync (private) ─────────────────────────────────────

        [Test]
        public async Task BuildMegaMenuAsync_WhenMasterPageIsNull_ReturnsEmpty()
        {
            var result = await InvokePrivateAsync<IEnumerable<FirstLevelMenuItemDto>>(
                "BuildMegaMenuAsync",
                new object?[] { null, CancellationToken.None });

            Assert.That(result, Is.Not.Null.And.Empty);
        }

        [Test]
        public async Task BuildMegaMenuAsync_WhenMasterPageProvidedButNoDataContext_ReturnsEmpty()
        {
            _dataContextRetrieverMock
                .Setup(r => r.TryRetrieve(out It.Ref<WebPageDataContext>.IsAny))
                .Returns(false);

            var masterPage = new MicrositeMasterPage
            {
                SystemFields = new WebPageFields { WebPageItemID = 10 },
            };

            var result = await InvokePrivateAsync<IEnumerable<FirstLevelMenuItemDto>>(
                "BuildMegaMenuAsync",
                new object[] { masterPage, CancellationToken.None });

            Assert.That(result, Is.Not.Null.And.Empty);
            _contentServiceMock.Verify(
                c => c.GetChildPagesByParentGuidAsync<MicrositePage>(
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        // ── GetLogoPageLinkAsync (private) ───────────────────────────────────

        [Test]
        public async Task GetLogoPageLinkAsync_WhenMasterPageIsNull_ReturnsHomePageUrl()
        {
            _homePageServiceMock
                .Setup(x => x.GetHomePageUrl(It.IsAny<CancellationToken>()))
                .ReturnsAsync("/home");

            var result = await InvokePrivateAsync<string>(
                "GetLogoPageLinkAsync",
                new object?[] { null, CancellationToken.None });

            Assert.That(result, Is.EqualTo("/home"));
        }

        [Test]
        public async Task GetLogoPageLinkAsync_WhenMasterPageHasNonMicrositeLogoLinkTarget_ReturnsHomePageUrl()
        {
            _homePageServiceMock
                .Setup(x => x.GetHomePageUrl(It.IsAny<CancellationToken>()))
                .ReturnsAsync("/home");

            var masterPage = new MicrositeMasterPage
            {
                SystemFields = new WebPageFields { WebPageItemID = 5 },
                LogoLinkTarget = "external",
            };

            var result = await InvokePrivateAsync<string>(
                "GetLogoPageLinkAsync",
                new object[] { masterPage, CancellationToken.None });

            Assert.That(result, Is.EqualTo("/home"));
            _urlResolverMock.Verify(
                u => u.ResolvePageUrlAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public async Task GetLogoPageLinkAsync_WhenMicrositeLogoTarget_ButNoDataContext_ReturnsHomePageUrl()
        {
            _homePageServiceMock
                .Setup(x => x.GetHomePageUrl(It.IsAny<CancellationToken>()))
                .ReturnsAsync("/home");

            _dataContextRetrieverMock
                .Setup(r => r.TryRetrieve(out It.Ref<WebPageDataContext>.IsAny))
                .Returns(false);

            var masterPage = new MicrositeMasterPage
            {
                SystemFields = new WebPageFields { WebPageItemID = 5 },
                LogoLinkTarget = "microsite",
            };

            var result = await InvokePrivateAsync<string>(
                "GetLogoPageLinkAsync",
                new object[] { masterPage, CancellationToken.None });

            Assert.That(result, Is.EqualTo("/home"));
            _urlResolverMock.Verify(
                u => u.ResolvePageUrlAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        // ── GetHomePageUrlAsync (private) ────────────────────────────────────

        [Test]
        public async Task GetHomePageUrlAsync_WhenHomePageUrlIsNull_ReturnsEmpty()
        {
            _homePageServiceMock
                .Setup(x => x.GetHomePageUrl(It.IsAny<CancellationToken>()))
                .ReturnsAsync((string?)null);

            var result = await InvokePrivateAsync<string>(
                "GetHomePageUrlAsync",
                new object[] { CancellationToken.None });

            Assert.That(result, Is.EqualTo(string.Empty));
        }

        [Test]
        public async Task GetHomePageUrlAsync_WhenHomePageUrlIsSet_ReturnsValue()
        {
            _homePageServiceMock
                .Setup(x => x.GetHomePageUrl(It.IsAny<CancellationToken>()))
                .ReturnsAsync("/home/page");

            var result = await InvokePrivateAsync<string>(
                "GetHomePageUrlAsync",
                new object[] { CancellationToken.None });

            Assert.That(result, Is.EqualTo("/home/page"));
        }

        // ── GetCultureDtosAsync (private) ────────────────────────────────────

        [Test]
        public async Task GetCultureDtosAsync_WhenHttpContextIsNull_ReturnsEmpty()
        {
            _httpContextAccessorMock
                .SetupGet(x => x.HttpContext)
                .Returns((HttpContext?)null);

            var result = await InvokePrivateAsync<IEnumerable<CultureDto>>(
                "GetCultureDtosAsync",
                Array.Empty<object>());

            Assert.That(result, Is.Not.Null.And.Empty);
            _localizedUrlsBuilderMock.Verify(b => b.BuildAsync(It.IsAny<HttpContext>()), Times.Never);
        }

        [Test]
        public async Task GetCultureDtosAsync_MapsLocalizedUrlsToCultureDtos()
        {
            _httpContextAccessorMock
                .SetupGet(x => x.HttpContext)
                .Returns(new DefaultHttpContext());

            _localizedUrlsBuilderMock
                .Setup(x => x.BuildAsync(It.IsAny<HttpContext>()))
                .ReturnsAsync(new List<LocalizedUrl>
                {
                    new()
                    {
                        LanguageName = "en",
                        CultureFormat = "en-US",
                        DisplayName = "English",
                        ShortCode = "EN",
                        RelativeUrl = "/en",
                        AbsoluteUrl = "https://localhost/en",
                    },
                });

            var result = await InvokePrivateAsync<IEnumerable<CultureDto>>(
                "GetCultureDtosAsync",
                Array.Empty<object>());

            var list = result.ToList();
            Assert.That(list, Has.Count.EqualTo(1));
            Assert.Multiple(() =>
            {
                Assert.That(list[0].Name, Is.EqualTo("English"));
                Assert.That(list[0].ShortName, Is.EqualTo("EN"));
                Assert.That(list[0].Url, Is.EqualTo("/en"));
            });
        }

        // ── ResolveMicrositePageTitle (private static) ───────────────────────

        [Test]
        public void ResolveMicrositePageTitle_WhenPageTitleSet_ReturnsPageTitle()
        {
            var page = new MicrositePage
            {
                PageTitle = "Title",
                MetadataTitle = "Meta",
                SystemFields = new WebPageFields { WebPageItemName = "Name" },
            };

            var result = InvokePrivateStatic<string>("ResolveMicrositePageTitle", new object[] { page });

            Assert.That(result, Is.EqualTo("Title"));
        }

        [Test]
        public void ResolveMicrositePageTitle_WhenPageTitleEmpty_FallsBackToMetadataTitle()
        {
            var page = new MicrositePage
            {
                PageTitle = string.Empty,
                MetadataTitle = "Meta",
                SystemFields = new WebPageFields { WebPageItemName = "Name" },
            };

            var result = InvokePrivateStatic<string>("ResolveMicrositePageTitle", new object[] { page });

            Assert.That(result, Is.EqualTo("Meta"));
        }

        [Test]
        public void ResolveMicrositePageTitle_WhenTitleAndMetadataEmpty_FallsBackToWebPageItemName()
        {
            var page = new MicrositePage
            {
                PageTitle = string.Empty,
                MetadataTitle = string.Empty,
                SystemFields = new WebPageFields { WebPageItemName = "FallbackName" },
            };

            var result = InvokePrivateStatic<string>("ResolveMicrositePageTitle", new object[] { page });

            Assert.That(result, Is.EqualTo("FallbackName"));
        }

        // ── helpers ──────────────────────────────────────────────────────────

        private async Task<T> InvokePrivateAsync<T>(string methodName, object?[] args)
        {
            var method = typeof(MicrositeHeaderService)
                .GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance)!;
            var task = (Task<T>)method.Invoke(_service, args)!;
            return await task;
        }

        private static T InvokePrivateStatic<T>(string methodName, object[] args)
        {
            var method = typeof(MicrositeHeaderService)
                .GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Static)!;
            return (T)method.Invoke(null, args)!;
        }
    }
}
