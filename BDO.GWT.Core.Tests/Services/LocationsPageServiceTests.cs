using BDO.GWT.Core.Data;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Pages;
using BDO.GWT.Core.Services;
using BDO.GWT.Web.Core.DTOs;
using BDO.GWT.Web.Core.Services;
using CMS.ContentEngine;
using CMS.Websites;
using Kentico.Content.Web.Mvc;
using Moq;

namespace BDO.GWT.Core.Tests.Services
{
    public class LocationsPageServiceTests
    {
        private Mock<IHeroService> _heroServiceMock = null!;
        private Mock<IContentService> _contentServiceMock = null!;
        private LocationsPageService _service = null!;

        [SetUp]
        public void Setup()
        {
            _heroServiceMock = new Mock<IHeroService>();
            _contentServiceMock = new Mock<IContentService>();

            _heroServiceMock
                .Setup(x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((HeroData data, CancellationToken _) => new HeroDto
                {
                    Layout = data.Layout,
                    Title = data.Title,
                    Description = data.Description,
                    CtaButtonType = data.CallToActionButtonType,
                    CtaButtonText = data.CallToActionButtonText,
                });

            _service = new LocationsPageService(
                _heroServiceMock.Object,
                _contentServiceMock.Object
            );
        }

        [Test]
        public async Task GetLocationsDtoAsync_ReturnsDto_WithHeroMapped()
        {
            var page = CreateLocationsPage();

            _contentServiceMock
                .Setup(x => x.GetPagesAsync<LocationCity>(
                    It.IsAny<RetrievePagesParameters>(),
                    It.IsAny<Action<RetrievePagesQueryParameters>>(),
                    It.IsAny<RetrievalCacheSettings>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<LocationCity>());

            _contentServiceMock
                .Setup(x => x.GetItemsByGUIDAsync(It.IsAny<List<Guid>>()))
                .ReturnsAsync(Enumerable.Empty<IWebPageFieldsSource>());

            var result = await _service.GetLocationsDtoAsync(page, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result.Hero, Is.Not.Null);
                Assert.That(result.Hero.Title, Is.EqualTo("Hero Title"));
                Assert.That(result.Hero.Layout, Is.EqualTo("layout-1"));
            });
        }

        [Test]
        public async Task GetLocationsDtoAsync_ReturnsDto_WithMainContentMapped()
        {
            var page = CreateLocationsPage();

            _contentServiceMock
                .Setup(x => x.GetPagesAsync<LocationCity>(
                    It.IsAny<RetrievePagesParameters>(),
                    It.IsAny<Action<RetrievePagesQueryParameters>>(),
                    It.IsAny<RetrievalCacheSettings>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<LocationCity>());

            _contentServiceMock
                .Setup(x => x.GetItemsByGUIDAsync(It.IsAny<List<Guid>>()))
                .ReturnsAsync(Enumerable.Empty<IWebPageFieldsSource>());

            var result = await _service.GetLocationsDtoAsync(page, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.MainContent, Is.Not.Null);
                Assert.That(result.MainContent.Title, Is.EqualTo("Description Title"));
                Assert.That(result.MainContent.ListTitle, Is.EqualTo("Locations Title"));
            });
        }

        [Test]
        public async Task GetLocationsDtoAsync_ReturnsDto_WithBannerMapped()
        {
            var page = CreateLocationsPage();

            _contentServiceMock
                .Setup(x => x.GetPagesAsync<LocationCity>(
                    It.IsAny<RetrievePagesParameters>(),
                    It.IsAny<Action<RetrievePagesQueryParameters>>(),
                    It.IsAny<RetrievalCacheSettings>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<LocationCity>());

            _contentServiceMock
                .Setup(x => x.GetItemsByGUIDAsync(It.IsAny<List<Guid>>()))
                .ReturnsAsync(Enumerable.Empty<IWebPageFieldsSource>());

            var result = await _service.GetLocationsDtoAsync(page, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.Banner, Is.Not.Null);
                Assert.That(result.Banner.Theme, Is.EqualTo("dark"));
            });
        }

        [Test]
        public async Task GetLocationsDtoAsync_WhenNoChildPages_ReturnsEmptyLocationsList()
        {
            var page = CreateLocationsPage();

            _contentServiceMock
                .Setup(x => x.GetPagesAsync<LocationCity>(
                    It.IsAny<RetrievePagesParameters>(),
                    It.IsAny<Action<RetrievePagesQueryParameters>>(),
                    It.IsAny<RetrievalCacheSettings>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<LocationCity>());

            _contentServiceMock
                .Setup(x => x.GetItemsByGUIDAsync(It.IsAny<List<Guid>>()))
                .ReturnsAsync(Enumerable.Empty<IWebPageFieldsSource>());

            var result = await _service.GetLocationsDtoAsync(page, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.MainContent.Locations, Is.Not.Null);
                Assert.That(result.MainContent.Locations.Count(), Is.EqualTo(1));
                Assert.That(result.MainContent.Locations.First().Items.Count(), Is.EqualTo(0));
            });
        }

        [Test]
        public async Task GetLocationsDtoAsync_WhenBannerButtonUrlIsNull_PrimaryButtonIsNull()
        {
            var page = CreateLocationsPage();
            page.BannerButtonURL = null;
            page.BannerButtonText = null;

            _contentServiceMock
                .Setup(x => x.GetPagesAsync<LocationCity>(
                    It.IsAny<RetrievePagesParameters>(),
                    It.IsAny<Action<RetrievePagesQueryParameters>>(),
                    It.IsAny<RetrievalCacheSettings>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<LocationCity>());

            var result = await _service.GetLocationsDtoAsync(page, CancellationToken.None);

            Assert.That(result.Banner.PrimaryButton, Is.Null);
        }

        [Test]
        [Ignore("page.GetUrl() requires Kentico IoC container. Refactor service to accept URL as parameter to enable testing.")]
        public async Task GetLocationsSchemaMarkupDto_ReturnsCorrectValues()
        {
            var page = CreateLocationsPage();
            page.SystemFields = new WebPageFields
            {
                WebPageItemID = 1,
                WebPageUrlPath = "/locations"
            };
            page.MetadataTitle = "Locations Meta Title";
            page.MetadataDescription = "Locations Meta Description";
            page.MetadataTeaserImage = new List<ContentAttachment>();

            _contentServiceMock
                .Setup(x => x.GetCurrentSiteLogoImageUrlAsync())
                .Returns("https://example.com/logo.png");

            _contentServiceMock
                .Setup(x => x.GetCurrentSiteLegalNameAsync())
                .Returns("BDO Legal");

            var result = await _service.GetLocationsSchemaMarkupDto(page, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result.Name, Is.EqualTo("Locations Meta Title"));
                Assert.That(result.Description, Is.EqualTo("Locations Meta Description"));
                Assert.That(result.Logo, Is.EqualTo("https://example.com/logo.png"));
                Assert.That(result.LegalName, Is.EqualTo("BDO Legal"));
            });
        }

        [Test]
        [Ignore("page.GetUrl() requires Kentico IoC container. Refactor service to accept URL as parameter to enable testing.")]
        public async Task GetLocationsSchemaMarkupDto_WhenLogoIsNull_ReturnsEmptyString()
        {
            var page = CreateLocationsPage();
            page.SystemFields = new WebPageFields
            {
                WebPageItemID = 1,
                WebPageUrlPath = "/locations"
            };
            page.MetadataTeaserImage = new List<ContentAttachment>();

            _contentServiceMock
                .Setup(x => x.GetCurrentSiteLogoImageUrlAsync())
                .Returns((string)null!);

            _contentServiceMock
                .Setup(x => x.GetCurrentSiteLegalNameAsync())
                .Returns((string)null!);

            var result = await _service.GetLocationsSchemaMarkupDto(page, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.Logo, Is.EqualTo(string.Empty));
                Assert.That(result.LegalName, Is.EqualTo(string.Empty));
            });
        }

        private static Locations CreateLocationsPage()
        {
            return new Locations
            {
                SystemFields = new WebPageFields
                {
                    WebPageItemID = 1,
                    WebPageUrlPath = "/locations"
                },
                HeroLayout = "layout-1",
                HeroTitle = "Hero Title",
                HeroDescription = "Hero Description",
                HeroBackgroundImage = new List<ContentAttachment>(),
                HeroBackgroundImage_Mobile = new List<ContentAttachment>(),
                HeroCallToActionButtonUrl = new List<IWebPageFieldsSource>(),
                HeroCallToActionButtonType = "primary",
                HeroCallToActionButtonText = "Click Here",
                HeroBackgroundImageAltText = "Alt text",
                HeroBackgroundImage_MobileAltText = "Mobile alt text",
                DescriptionTitle = "Description Title",
                DescriptionBody = "<p>Body content</p>",
                LocationsTitle = "Locations Title",
                BannerContent = "<p>Banner content</p>",
                BannerBackgroundImage = new List<ContentAttachment>(),
                BannerTheme = "dark",
                BannerButtonType = "primary",
                BannerButtonText = "Banner Button",
                BannerButtonURL = new List<WebPageRelatedItem>(),
                BannerButtonLinkTarget = "_self",
                MetadataTitle = "Meta Title",
                MetadataDescription = "Meta Description",
                MetadataTeaserImage = new List<ContentAttachment>(),
            };
        }
    }
}