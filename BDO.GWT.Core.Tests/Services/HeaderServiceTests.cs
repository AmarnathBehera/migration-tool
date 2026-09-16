using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Header;
using BDO.GWT.Core.Services;
using BDO.GWT.Core.Services.Header;
using CMS.Websites;
using CMS.Websites.Routing;
using Kentico.Content.Web.Mvc;
using Microsoft.AspNetCore.Http;
using CMS.Helpers;
using Moq;
using Kentico.Content.Web.Mvc.Routing;
using CMS.ContentEngine;
using Microsoft.Testing.Platform.Configurations;
using Microsoft.Extensions.Configuration;

namespace BDO.GWT.Core.Tests.Services
{
    public class HeaderServiceTests
    {
        private Mock<IContentService> _contentServiceMock = null!;
        private Mock<IContentRetriever> _contentRetrieverMock = null!;
        private Mock<IHomePageService> _homePageServiceMock = null!;
        private Mock<IWebsiteCulturesService> _websiteCulturesServiceMock = null!;
        private Mock<IWebsiteChannelContext> _websiteChannelContextMock = null!;
        private Mock<ILocalizedUrlsBuilder> _localizedUrlsBuilderMock = null!;
        private Mock<IHttpContextAccessor> _httpContextAccessorMock = null!;
        private Mock<IProgressiveCache> _progressiveCacheMock = null!;
        private Mock<IPreferredLanguageRetriever> _preferredLanguageRetrieverMock = null!;
        private Mock<IWebPageUrlResolver> _urlResolverMock = null!;
        private Mock<Microsoft.Extensions.Configuration.IConfiguration> _configurationMock = null!;
        private Microsoft.Extensions.Configuration.IConfiguration _configuration = null!;
        private HeaderService _service = null!;

        [SetUp]
        public void Setup()
        {
            _contentServiceMock = new Mock<IContentService>();
            _contentRetrieverMock = new Mock<IContentRetriever>();
            _homePageServiceMock = new Mock<IHomePageService>();
            _websiteCulturesServiceMock = new Mock<IWebsiteCulturesService>();
            _websiteChannelContextMock = new Mock<IWebsiteChannelContext>();
            _localizedUrlsBuilderMock = new Mock<ILocalizedUrlsBuilder>();
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            _preferredLanguageRetrieverMock = new Mock<IPreferredLanguageRetriever>();
            _progressiveCacheMock = new Mock<IProgressiveCache>();
            _urlResolverMock = new Mock<IWebPageUrlResolver>();

            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["HeaderMenuCacheInMin"] = "10"
                })
                .Build();

            _service = new HeaderService(
                _contentServiceMock.Object,
                _contentRetrieverMock.Object,
                _homePageServiceMock.Object,
                _websiteCulturesServiceMock.Object,
                _websiteChannelContextMock.Object,
                _localizedUrlsBuilderMock.Object,
                _httpContextAccessorMock.Object,
                _progressiveCacheMock.Object,
                _preferredLanguageRetrieverMock.Object,
                _urlResolverMock.Object,
                _configuration
            );
        }

        [Test]
        public void GetAsync_WhenExceptionThrown_PropagatesException()
        {
            _httpContextAccessorMock
                .SetupGet(x => x.HttpContext)
                .Returns(new DefaultHttpContext());

            _localizedUrlsBuilderMock
                .Setup(x => x.BuildAsync(It.IsAny<HttpContext>()))
                .ThrowsAsync(new Exception());

            // Make the cache actually invoke the factory delegate so the
            // exception thrown inside BuildHeaderAsync can propagate.
            _progressiveCacheMock
                .Setup(x => x.LoadAsync(
                    It.IsAny<Func<CacheSettings, Task<HeaderDto?>>>(),
                    It.IsAny<CacheSettings>()))
                .Returns((Func<CacheSettings, Task<HeaderDto?>> factory, CacheSettings settings) =>
                    factory(settings));

            Assert.ThrowsAsync<Exception>(async () =>
                await _service.GetAsync(_contentServiceMock.Object, CancellationToken.None));
        }

        [Test]
        public async Task GetAsync_ReturnsHeaderDto_WithExpectedValues()
        {
            // Arrange
            var fakeHomePageUrl = "/home";

            // Make the cache actually invoke the factory delegate so that
            // BuildHeaderAsync runs and a real HeaderDto is returned.
            _progressiveCacheMock
                .Setup(x => x.LoadAsync(
                    It.IsAny<Func<CacheSettings, Task<HeaderDto?>>>(),
                    It.IsAny<CacheSettings>()))
                .Returns((Func<CacheSettings, Task<HeaderDto?>> factory, CacheSettings settings) =>
                    factory(settings));

            _homePageServiceMock
                .Setup(x => x.GetHomePageUrl(It.IsAny<CancellationToken>()))
                .ReturnsAsync(fakeHomePageUrl);

            _httpContextAccessorMock
                .SetupGet(x => x.HttpContext)
                .Returns(new DefaultHttpContext());

            _preferredLanguageRetrieverMock
                .Setup(x => x.Get())
                .Returns("en");

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
                        RelativeUrl = "/en/home",
                        AbsoluteUrl = "https://localhost/en/home",
                        IsDefault = true,
                        IsActive = true,
                    },
                    new()
                    {
                        LanguageName = "lt",
                        CultureFormat = "lt-LT",
                        DisplayName = "Lithuanian",
                        ShortCode = "LT",
                        RelativeUrl = "/lt/home",
                        AbsoluteUrl = "https://localhost/lt/home",
                    },
                });

            _websiteCulturesServiceMock
                .Setup(x => x.GetDisplayName(_websiteChannelContextMock.Object))
                .Returns("BDO Global");

            // BuildMegaMenuAsync now resolves first-level nav items via
            // IContentRetriever.RetrievePages (bound with linked items),
            // not via IContentService.GetContentType.
            var firstLevelItem = new NavigationFirstLevelItem { Title = "FirstLevel" };

            _contentRetrieverMock
                .Setup(x => x.RetrievePages<NavigationFirstLevelItem, NavigationFirstLevelItem>(
                    It.IsAny<RetrievePagesParameters>(),
                    It.IsAny<Action<RetrievePagesQueryParameters>>(),
                    It.IsAny<RetrievalCacheSettings>(),
                    It.IsAny<Func<IContentQueryDataContainer, NavigationFirstLevelItem, Task<NavigationFirstLevelItem>>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<NavigationFirstLevelItem> { firstLevelItem });

            // Act
            var headerDto = await _service.GetAsync(_contentServiceMock.Object, CancellationToken.None);

            // Assert
            Assert.Multiple(() =>
            {
                // Basic info
                Assert.That(headerDto.SiteName, Is.EqualTo("BDO Global"));
                Assert.That(headerDto.LogoURL, Is.EqualTo(fakeHomePageUrl));
                Assert.That(headerDto.LogoImageURL, Is.Not.Null.And.Not.Empty);

                // Cultures
                Assert.That(headerDto.Cultures.ElementAt(0).Name, Is.EqualTo("English"));
                Assert.That(headerDto.Cultures.ElementAt(0).ShortName, Is.EqualTo("EN"));
                Assert.That(headerDto.Cultures.ElementAt(0).Url, Is.EqualTo("/en/home"));

                Assert.That(headerDto.Cultures.ElementAt(1).Name, Is.EqualTo("Lithuanian"));
                Assert.That(headerDto.Cultures.ElementAt(1).ShortName, Is.EqualTo("LT"));
                Assert.That(headerDto.Cultures.ElementAt(1).Url, Is.EqualTo("/lt/home"));

                // Mega menu
                Assert.That(headerDto.MegaMenuItems, Is.Not.Null.And.Not.Empty);
                Assert.That(headerDto.MegaMenuItems.First().Title, Is.EqualTo("FirstLevel"));

                // Other collections
                Assert.That(headerDto.MenuItems, Is.Not.Null);
                Assert.That(headerDto.Contact, Is.Null.Or.InstanceOf<LinkItemDto>());
            });
        }

        [Test]
        public async Task GetLogoPageLinkAsync_ReturnsEmpty_WhenHomePageIsNull()
        {
            // Arrange
            _homePageServiceMock
                .Setup(x => x.GetHomePageUrl(It.IsAny<CancellationToken>()))
                .ReturnsAsync((string?)null);

            // Act
            var method = _service.GetType()
    .GetMethod("GetLogoPageLinkAsync",
               System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

            var task = (Task<string>)method.Invoke(_service, new object[] { CancellationToken.None })!;
            var result = await task;

            // Assert
            Assert.That(result, Is.EqualTo(string.Empty));
        }
        [Test]
        public async Task BuildMegaMenuAsync_ReturnsFirstLevelMenu()
        {
            // Arrange
            var targetPage = new NavigationFirstLevelItem
            {
                Title = "TargetPage",
                SystemFields = new WebPageFields { WebPageUrlPath = "/first-level" }
            };

            var firstLevelItem = new NavigationFirstLevelItem
            {
                Title = "FirstLevel",
                SystemFields = new WebPageFields { WebPageItemID = 1 },
                TargetURL = new List<IWebPageFieldsSource> { targetPage }
            };

            // BuildMegaMenuAsync now resolves first-level items via
            // IContentRetriever.RetrievePages (with linked items bound),
            // not via IContentService.GetContentType/GetItemsByGUIDAsync.
            _contentRetrieverMock
                .Setup(x => x.RetrievePages<NavigationFirstLevelItem, NavigationFirstLevelItem>(
                    It.IsAny<RetrievePagesParameters>(),
                    It.IsAny<Action<RetrievePagesQueryParameters>>(),
                    It.IsAny<RetrievalCacheSettings>(),
                    It.IsAny<Func<IContentQueryDataContainer, NavigationFirstLevelItem, Task<NavigationFirstLevelItem>>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<NavigationFirstLevelItem> { firstLevelItem });

            // URL resolution for the target page now goes through the
            // injected, mockable IWebPageUrlResolver rather than the
            // static (IoC-container-dependent) GetUrl() extension method.
            _urlResolverMock
                .Setup(x => x.GetRelativePath(targetPage))
                .Returns("/first-level");

            // Second-level / featured-level lookups are left unmocked on purpose;
            // GetChildPagesByParentGuidAsync returns null by default, and the
            // service methods already fall back to empty lists.

            // Act
            var method = _service.GetType()
                .GetMethod("BuildMegaMenuAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

            var task = (Task<IEnumerable<FirstLevelMenuItemDto>>)method.Invoke(_service, new object[] { CancellationToken.None })!;
            var megaMenu = await task;

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(megaMenu, Is.Not.Null);
                Assert.That(megaMenu.Any(), Is.True);
                Assert.That(megaMenu.First().Title, Is.EqualTo("FirstLevel"));
                Assert.That(megaMenu.First().URL, Is.EqualTo("/first-level"));
                Assert.That(megaMenu.First().Children, Is.Empty);
                Assert.That(megaMenu.First().Featured, Is.Null.Or.Empty);
            });
        }

        [Test]
        public async Task GetCultureDtosAsync_ReturnsEmpty_WhenHttpContextIsNull()
        {
            // Arrange
            _httpContextAccessorMock
                .SetupGet(x => x.HttpContext)
                .Returns((HttpContext?)null);

            // Act
            var result = await _service.GetCultureDtosAsync(CancellationToken.None);

            // Assert
            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task BuildTopMenuAsync_ReturnsEmptyMenuAndNullContact_WhenNoPages()
        {
            // Arrange
            _contentRetrieverMock
                .Setup(x => x.RetrievePages<Navigation, Navigation>(
                    It.IsAny<RetrievePagesParameters>(),
                    It.IsAny<Action<RetrievePagesQueryParameters>>(),
                    It.IsAny<RetrievalCacheSettings>(),
                    It.IsAny<Func<IContentQueryDataContainer, Navigation, Task<Navigation>>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(Enumerable.Empty<Navigation>());

            // Act
            var method = _service.GetType()
                .GetMethod("BuildTopMenuAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

            var task = (Task<(IEnumerable<LinkItemDto> TopMenu, LinkItemDto? Contact)>)method.Invoke(_service, new object[] { CancellationToken.None })!;
            var result = await task;

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.TopMenu, Is.Empty);
                Assert.That(result.Contact, Is.Null);
            });
        }

        [Test]
        public async Task BuildTopMenuAsync_ReturnsTopMenuItems_WhenPagesAndTopMenuExist()
        {
            // Arrange
            var linkedPage = new Navigation { Title = "Link1", SystemFields = new WebPageFields() };

            var navigationPage = new Navigation
            {
                Title = "NavRoot",
                SystemFields = new WebPageFields(),
                TopMenu = new List<IWebPageFieldsSource> { linkedPage }
            };

            _contentRetrieverMock
                .Setup(x => x.RetrievePages<Navigation, Navigation>(
                    It.IsAny<RetrievePagesParameters>(),
                    It.IsAny<Action<RetrievePagesQueryParameters>>(),
                    It.IsAny<RetrievalCacheSettings>(),
                    It.IsAny<Func<IContentQueryDataContainer, Navigation, Task<Navigation>>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Navigation> { navigationPage });

            _urlResolverMock
                .Setup(x => x.GetRelativePath(linkedPage))
                .Returns("/menu-link");

            // No Contact page — avoids the static GetUrl() service-locator call.
            _contentRetrieverMock
                .Setup(x => x.RetrievePages<Contact, Contact>(
                    It.IsAny<RetrievePagesParameters>(),
                    It.IsAny<Action<RetrievePagesQueryParameters>>(),
                    It.IsAny<RetrievalCacheSettings>(),
                    It.IsAny<Func<IContentQueryDataContainer, Contact, Task<Contact>>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(Enumerable.Empty<Contact>());

            // Act
            var method = _service.GetType()
                .GetMethod("BuildTopMenuAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

            var task = (Task<(IEnumerable<LinkItemDto> TopMenu, LinkItemDto? Contact)>)method.Invoke(_service, new object[] { CancellationToken.None })!;
            var result = await task;

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.TopMenu.First().URL, Is.EqualTo("/menu-link"));
                Assert.That(result.Contact, Is.Not.Null);
                Assert.That(result.Contact!.Title, Is.Empty);
                Assert.That(result.Contact!.URL, Is.Empty);
            });
        }

        [Test]
        public async Task BuildSecondLevelAsync_ReturnsSecondLevelItems()
        {
            // Arrange
            var secondLevelItem = new NavigationSecondLevelItem
            {
                Title = "Second",
                SystemFields = new WebPageFields(),
                TargetURL = null // avoids the static GetUrl() service-locator call
            };

            _contentServiceMock
                .Setup(s => s.GetChildPagesByParentGuidAsync<NavigationSecondLevelItem>(
                    It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<NavigationSecondLevelItem> { secondLevelItem });

            _contentServiceMock
                .Setup(s => s.GetChildPagesByParentGuidAsync<NavigationThirdLevelItem>(
                    It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Enumerable.Empty<NavigationThirdLevelItem>());

            // Act
            var method = _service.GetType()
                .GetMethod("BuildSecondLevelAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

            var task = (Task<List<SecondLevelMenuItemDto>>)method.Invoke(_service, new object[] { 1, CancellationToken.None })!;
            var result = await task;

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result, Has.Count.EqualTo(1));
                Assert.That(result[0].Title, Is.EqualTo("Second"));
                Assert.That(result[0].URL, Is.Empty);
                Assert.That(result[0].Children, Is.Empty);
            });
        }

        [Test]
        public async Task BuildThirdLevelAsync_ReturnsThirdLevelItems()
        {
            // Arrange
            var thirdLevelItem = new NavigationThirdLevelItem
            {
                Title = "Third",
                SystemFields = new WebPageFields(),
                TargetURL = null
            };

            _contentServiceMock
                .Setup(s => s.GetChildPagesByParentGuidAsync<NavigationThirdLevelItem>(
                    It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<NavigationThirdLevelItem> { thirdLevelItem });

            // Act
            var method = _service.GetType()
                .GetMethod("BuildThirdLevelAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

            var task = (Task<List<ThirdLevelMenuItemDto>>)method.Invoke(_service, new object[] { 1 })!;
            var result = await task;

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result, Has.Count.EqualTo(1));
                Assert.That(result[0].Title, Is.EqualTo("Third"));
                Assert.That(result[0].URL, Is.Empty);
            });
        }

        [Test]
        public async Task BuildFeaturedLevelAsync_ReturnsFeaturedItems_WithImageUrl()
        {
            // Arrange
            var featuredItem = new NavigationFeaturedItem
            {
                Title = "Featured",
                Description = "Desc",
                SystemFields = new WebPageFields(),
                TargetURL = null,
                ImageAltText = "Alt",
                Image = new List<ContentAttachment>
                {
                    new() { ContentAttachmentAsset = new ContentItemAsset { Url = "/img.png" } }
                }
            };

            _contentServiceMock
                .Setup(s => s.GetChildPagesByParentGuidAsync<NavigationFeaturedItem>(
                    It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<NavigationFeaturedItem> { featuredItem });

            // Act
            var method = _service.GetType()
                .GetMethod("BuildFeaturedLevelAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

            var task = (Task<List<FeaturedMenuItemDto>>)method.Invoke(_service, new object[] { 1, CancellationToken.None })!;
            var result = await task;

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result, Has.Count.EqualTo(1));
                Assert.That(result[0].Title, Is.EqualTo("Featured"));
                Assert.That(result[0].Description, Is.EqualTo("Desc"));
                Assert.That(result[0].ImageURL, Is.EqualTo("/img.png"));
                Assert.That(result[0].ImageAltText, Is.EqualTo("Alt"));
                Assert.That(result[0].URL, Is.Empty);
            });
        }
    }
}