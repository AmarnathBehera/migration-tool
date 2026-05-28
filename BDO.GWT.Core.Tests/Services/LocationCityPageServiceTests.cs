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
    public class LocationCityPageServiceTests
    {
        private Mock<IHeroService> _heroServiceMock = null!;
        private Mock<IContentService> _contentServiceMock = null!;
        private LocationCityPageService _service = null!;

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

            _service = new LocationCityPageService(
                _heroServiceMock.Object,
                _contentServiceMock.Object
            );
        }

        [Test]
        public async Task GetLocationCityDtoAsync_ReturnsDto_WithHeroMapped()
        {
            var page = CreateLocationCityPage();

            _contentServiceMock
                .Setup(x => x.GetPagesAsync<LocationOffice>(
                    It.IsAny<RetrievePagesParameters>(),
                    It.IsAny<Action<RetrievePagesQueryParameters>>(),
                    It.IsAny<RetrievalCacheSettings>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<LocationOffice>());

            var result = await _service.GetLocationCityDtoAsync(page, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result.Hero, Is.Not.Null);
                Assert.That(result.Hero.Title, Is.EqualTo("Hero Title"));
                Assert.That(result.Hero.Layout, Is.EqualTo("layout-1"));
            });
        }

        [Test]
        public async Task GetLocationCityDtoAsync_ReturnsDto_WithMainContentMapped()
        {
            var page = CreateLocationCityPage();

            _contentServiceMock
                .Setup(x => x.GetPagesAsync<LocationOffice>(
                    It.IsAny<RetrievePagesParameters>(),
                    It.IsAny<Action<RetrievePagesQueryParameters>>(),
                    It.IsAny<RetrievalCacheSettings>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<LocationOffice>());

            var result = await _service.GetLocationCityDtoAsync(page, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.MainContent, Is.Not.Null);
                Assert.That(result.MainContent.Title, Is.EqualTo("City Content Title"));
                Assert.That(result.MainContent.SectionTitle, Is.EqualTo("Section Title"));
            });
        }

        [Test]
        public async Task GetLocationCityDtoAsync_WhenNoOffices_ReturnsEmptyOfficeCards()
        {
            var page = CreateLocationCityPage();

            _contentServiceMock
                .Setup(x => x.GetPagesAsync<LocationOffice>(
                    It.IsAny<RetrievePagesParameters>(),
                    It.IsAny<Action<RetrievePagesQueryParameters>>(),
                    It.IsAny<RetrievalCacheSettings>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<LocationOffice>());

            var result = await _service.GetLocationCityDtoAsync(page, CancellationToken.None);

            Assert.That(result.MainContent.OfficeCards, Is.Empty);
        }

        [Test]
        public async Task GetLocationCityDtoAsync_WhenOfficesExist_ReturnsOfficeCards()
        {
            var page = CreateLocationCityPage();
            var offices = new List<LocationOffice>
            {
                CreateLocationOffice("Office One", "123 Main St", "Suite 1"),
                CreateLocationOffice("Office Two", "456 High St", null),
            };

            _contentServiceMock
                .Setup(x => x.GetPagesAsync<LocationOffice>(
                    It.IsAny<RetrievePagesParameters>(),
                    It.IsAny<Action<RetrievePagesQueryParameters>>(),
                    It.IsAny<RetrievalCacheSettings>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(offices);

            var result = await _service.GetLocationCityDtoAsync(page, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.MainContent.OfficeCards.Count(), Is.EqualTo(2));
                Assert.That(result.MainContent.OfficeCards.First().Name, Is.EqualTo("Office One"));
            });
        }

        [Test]
        public async Task GetLocationCityDtoAsync_MapsOfficeCardFields_Correctly()
        {
            var page = CreateLocationCityPage();
            var offices = new List<LocationOffice>
            {
                CreateLocationOffice("Main Office", "100 King St", "Floor 5", zip: "12345", phone: "+1 555 0000", email: "test@bdo.com")
            };

            _contentServiceMock
                .Setup(x => x.GetPagesAsync<LocationOffice>(
                    It.IsAny<RetrievePagesParameters>(),
                    It.IsAny<Action<RetrievePagesQueryParameters>>(),
                    It.IsAny<RetrievalCacheSettings>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(offices);

            var result = await _service.GetLocationCityDtoAsync(page, CancellationToken.None);
            var card = result.MainContent.OfficeCards.First();

            Assert.Multiple(() =>
            {
                Assert.That(card.Name, Is.EqualTo("Main Office"));
                Assert.That(card.AddressLine1, Is.EqualTo("100 King St"));
                Assert.That(card.AddressLine2, Is.EqualTo("Floor 5"));
                Assert.That(card.PostalCode, Is.EqualTo("12345"));
                Assert.That(card.PhoneNumber, Is.EqualTo("+1 555 0000"));
                Assert.That(card.Email, Is.EqualTo("test@bdo.com"));
                Assert.That(card.City, Is.EqualTo("City Meta Title"));
            });
        }

        [Test]
        [Ignore("page.GetUrl() requires Kentico IoC container. Refactor service to accept URL as parameter to enable testing.")]
        public async Task GetLocationCitySchemaMarkupDto_ReturnsCorrectValues()
        {
            var page = CreateLocationCityPage();
            page.MetadataTitle = "City Meta Title";
            page.MetadataDescription = "City Meta Description";
            page.MetadataTeaserImage = new List<ContentAttachment>();

            var office = CreateLocationOffice("Main Office", "100 King St", null, lat: "51.5", lon: "-0.1", zip: "EC1A", country: "GB");

            _contentServiceMock
                .Setup(x => x.GetPagesAsync<LocationOffice>(
                    It.IsAny<RetrievePagesParameters>(),
                    It.IsAny<Action<RetrievePagesQueryParameters>>(),
                    It.IsAny<RetrievalCacheSettings>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<LocationOffice> { office });

            _contentServiceMock
                .Setup(x => x.GetCurrentSiteLogoImageUrlAsync())
                .Returns("https://example.com/logo.png");

            _contentServiceMock
                .Setup(x => x.GetCurrentSiteLegalNameAsync())
                .Returns("BDO Legal");

            _contentServiceMock
                .Setup(x => x.GetFallbackMetadataDescription(It.IsAny<string>(), It.IsAny<string>()))
                .Returns("City Meta Description");

            var result = await _service.GetLocationCitySchemaMarkupDto(page, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result.Description, Is.EqualTo("City Meta Description"));
                Assert.That(result.Logo, Is.EqualTo("https://example.com/logo.png"));
                Assert.That(result.LegalName, Is.EqualTo("BDO Legal"));
                Assert.That(result.Latitude, Is.EqualTo("51.5"));
                Assert.That(result.Longitude, Is.EqualTo("-0.1"));
            });
        }

        [Test]
        [Ignore("page.GetUrl() requires Kentico IoC container. Refactor service to accept URL as parameter to enable testing.")]
        public async Task GetLocationCitySchemaMarkupDto_MapsAddressFromFirstOffice()
        {
            var page = CreateLocationCityPage();
            page.MetadataTeaserImage = new List<ContentAttachment>();

            var office = CreateLocationOffice("Main Office", "100 King St", "Floor 2", zip: "EC1A", country: "GB");

            _contentServiceMock
                .Setup(x => x.GetPagesAsync<LocationOffice>(
                    It.IsAny<RetrievePagesParameters>(),
                    It.IsAny<Action<RetrievePagesQueryParameters>>(),
                    It.IsAny<RetrievalCacheSettings>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<LocationOffice> { office });

            _contentServiceMock
                .Setup(x => x.GetCurrentSiteLogoImageUrlAsync())
                .Returns((string)null!);

            _contentServiceMock
                .Setup(x => x.GetCurrentSiteLegalNameAsync())
                .Returns((string)null!);

            _contentServiceMock
                .Setup(x => x.GetFallbackMetadataDescription(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(string.Empty);

            var result = await _service.GetLocationCitySchemaMarkupDto(page, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.Address, Is.Not.Null);
                Assert.That(result.Address.StreetAddress, Is.EqualTo("100 King St Floor 2"));
                Assert.That(result.Address.PostalCode, Is.EqualTo("EC1A"));
                Assert.That(result.Address.AddressCountry, Is.EqualTo("GB"));
                Assert.That(result.Address.AddressLocality, Is.EqualTo("City Meta Title"));
            });
        }

        [Test]
        [Ignore("page.GetUrl() requires Kentico IoC container. Refactor service to accept URL as parameter to enable testing.")]
        public async Task GetLocationCitySchemaMarkupDto_WhenNoOffices_AddressFieldsAreNull()
        {
            var page = CreateLocationCityPage();
            page.MetadataTeaserImage = new List<ContentAttachment>();

            _contentServiceMock
                .Setup(x => x.GetPagesAsync<LocationOffice>(
                    It.IsAny<RetrievePagesParameters>(),
                    It.IsAny<Action<RetrievePagesQueryParameters>>(),
                    It.IsAny<RetrievalCacheSettings>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<LocationOffice>());

            _contentServiceMock
                .Setup(x => x.GetCurrentSiteLogoImageUrlAsync())
                .Returns((string)null!);

            _contentServiceMock
                .Setup(x => x.GetCurrentSiteLegalNameAsync())
                .Returns((string)null!);

            _contentServiceMock
                .Setup(x => x.GetFallbackMetadataDescription(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(string.Empty);

            var result = await _service.GetLocationCitySchemaMarkupDto(page, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.Latitude, Is.Null);
                Assert.That(result.Longitude, Is.Null);
                Assert.That(result.Address.StreetAddress, Is.Null);
                Assert.That(result.Address.PostalCode, Is.Null);
                Assert.That(result.Address.AddressCountry, Is.Null);
            });
        }

        private static LocationCity CreateLocationCityPage()
        {
            return new LocationCity
            {
                SystemFields = new WebPageFields
                {
                    WebPageItemID = 1,
                    WebPageUrlPath = "/locations/city"
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
                ContentTitle = "City Content Title",
                SectionTitle = "Section Title",
                MetadataTitle = "City Meta Title",
                MetadataDescription = "City Meta Description",
                MetadataOGDescription = "City OG Description",
                MetadataTeaserImage = new List<ContentAttachment>(),
            };
        }

        private static LocationOffice CreateLocationOffice(
            string name,
            string addressLine1,
            string? addressLine2,
            string? lat = null,
            string? lon = null,
            string? zip = null,
            string? country = null,
            string? phone = null,
            string? email = null)
        {
            return new LocationOffice
            {
                MetadataTitle = name,
                LocationAddressLine1 = addressLine1,
                LocationAddressLine2 = addressLine2,
                LocationLatitude = lat,
                LocationLongitude = lon,
                LocationZip = zip,
                LocationCountry = country,
                LocationPhone = phone,
                LocationEmail = email,
                OfficeImage = new List<ContentAttachment>(),
            };
        }
    }
}