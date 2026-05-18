using BDO.GWT.Core.Data;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.services;
using BDO.GWT.Web.Core.DTOs;
using BDO.GWT.Web.Core.Services;
using CMS.ContentEngine;
using CMS.Websites;
using Kentico.Content.Web.Mvc;
using Moq;

namespace BDO.GWT.Core.Tests.Services
{
    public class SectionServicesServiceTests
    {
        private Mock<IHeroService> _heroServiceMock = null!;
        private Mock<IContentService> _contentServiceMock = null!;
        private Mock<IPageUtility> _pageUtilityMock = null!;
        private SectionServicesService _service = null!;

        [SetUp]
        public void Setup()
        {
            _heroServiceMock = new Mock<IHeroService>();
            _contentServiceMock = new Mock<IContentService>();
            _pageUtilityMock = new Mock<IPageUtility>();

            _heroServiceMock
                .Setup(x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HeroDto { Title = "Hero" });

            _pageUtilityMock
                .Setup(x => x.GetMultiLinkCardsAsync<BusinessLine, ServiceArea>(
                    It.IsAny<int>(),
                    It.IsAny<Func<BusinessLine, string>>(),
                    It.IsAny<Func<BusinessLine, string>>(),
                    It.IsAny<Func<BusinessLine, string?>>(),
                    It.IsAny<Func<BusinessLine, string?>>(),
                    It.IsAny<Func<ServiceArea, string>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(Array.Empty<MultiLinkCardDto>());

            _service = new SectionServicesService(
                _heroServiceMock.Object,
                _contentServiceMock.Object,
                _pageUtilityMock.Object);
        }

        [Test]
        public async Task GetSectionServicesDtoAsync_ReturnsEmptyDto_WhenPageIsNull()
        {
            _contentServiceMock
                .Setup(x => x.RetrieveCurrentPageAsync<SectionServices>(
                    It.IsAny<RetrieveCurrentPageParameters>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((SectionServices?)null);

            var result = await _service.GetSectionServicesDtoAsync(CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result.Hero, Is.Null);
                Assert.That(result.MultiLinksCardsGrid, Is.Null);
            });
        }

        [Test]
        public async Task GetSectionServicesDtoAsync_ReturnsDto_WithNoCategories()
        {
            var page = CreateSectionServicesPage();

            _contentServiceMock
                .Setup(x => x.RetrieveCurrentPageAsync<SectionServices>(
                    It.IsAny<RetrieveCurrentPageParameters>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(page);

            var result = await _service.GetSectionServicesDtoAsync(CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result.Hero, Is.Not.Null);
                Assert.That(result.MultiLinksCardsGrid, Is.Not.Null);
                Assert.That(result.MultiLinksCardsGrid.Cards, Is.Empty);
                Assert.That(result.MultiLinksCardsGrid.Title, Is.EqualTo("Grid Title"));
                Assert.That(result.MultiLinksCardsGrid.Description, Is.EqualTo("Grid Summary"));
            });
        }

        [Test]
        public async Task GetSectionServicesDtoAsync_ReturnsMappedCards_WithCategoriesAndServices()
        {
            var page = CreateSectionServicesPage();
            var cards = new[]
            {
                new MultiLinkCardDto
                {
                    Title = "Category 1",
                    Description = "Desc 1",
                    ImageURL = "/cat-img.png",
                    ImageAltText = "Alt 1",
                    ReadMoreLinks = new[] { new LinkItemDto { Title = "Service 1", URL = "/service-1" } }
                }
            };

            _contentServiceMock
                .Setup(x => x.RetrieveCurrentPageAsync<SectionServices>(
                    It.IsAny<RetrieveCurrentPageParameters>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(page);

            _pageUtilityMock
                .Setup(x => x.GetMultiLinkCardsAsync<BusinessLine, ServiceArea>(
                    It.IsAny<int>(),
                    It.IsAny<Func<BusinessLine, string>>(),
                    It.IsAny<Func<BusinessLine, string>>(),
                    It.IsAny<Func<BusinessLine, string?>>(),
                    It.IsAny<Func<BusinessLine, string?>>(),
                    It.IsAny<Func<ServiceArea, string>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(cards);

            var result = await _service.GetSectionServicesDtoAsync(CancellationToken.None);

            Assert.That(result.MultiLinksCardsGrid.Cards, Has.Length.EqualTo(1));

            var card = result.MultiLinksCardsGrid.Cards![0];
            Assert.Multiple(() =>
            {
                Assert.That(card.Title, Is.EqualTo("Category 1"));
                Assert.That(card.Description, Is.EqualTo("Desc 1"));
                Assert.That(card.ImageURL, Is.EqualTo("/cat-img.png"));
                Assert.That(card.ImageAltText, Is.EqualTo("Alt 1"));
                Assert.That(card.ReadMoreLinks, Has.Length.EqualTo(1));
                Assert.That(card.ReadMoreLinks[0].Title, Is.EqualTo("Service 1"));
            });
        }

        [Test]
        public async Task GetSectionServicesDtoAsync_CallsPageUtility_Once()
        {
            var page = CreateSectionServicesPage();

            _contentServiceMock
                .Setup(x => x.RetrieveCurrentPageAsync<SectionServices>(
                    It.IsAny<RetrieveCurrentPageParameters>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(page);

            await _service.GetSectionServicesDtoAsync(CancellationToken.None);

            _pageUtilityMock.Verify(
                x => x.GetMultiLinkCardsAsync<BusinessLine, ServiceArea>(
                    It.IsAny<int>(),
                    It.IsAny<Func<BusinessLine, string>>(),
                    It.IsAny<Func<BusinessLine, string>>(),
                    It.IsAny<Func<BusinessLine, string?>>(),
                    It.IsAny<Func<BusinessLine, string?>>(),
                    It.IsAny<Func<ServiceArea, string>>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task GetSectionServicesDtoAsync_MapsHeroData_Correctly()
        {
            var page = CreateSectionServicesPage();
            page.HeroTitle = "Test Hero Title";
            page.HeroDescription = "Test Hero Desc";
            page.HeroLayout = "FullWidth";

            _contentServiceMock
                .Setup(x => x.RetrieveCurrentPageAsync<SectionServices>(
                    It.IsAny<RetrieveCurrentPageParameters>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(page);

            await _service.GetSectionServicesDtoAsync(CancellationToken.None);

            _heroServiceMock.Verify(x => x.GetHeroDtoAsync(
                It.Is<HeroData>(h =>
                    h.Title == "Test Hero Title" &&
                    h.Description == "Test Hero Desc" &&
                    h.Layout == "FullWidth"),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task GetSectionServicesDtoAsync_MapsShowMultiLinks_ToGridDto()
        {
            var page = CreateSectionServicesPage();
            page.ShowMultiLinks = true;

            _contentServiceMock
                .Setup(x => x.RetrieveCurrentPageAsync<SectionServices>(
                    It.IsAny<RetrieveCurrentPageParameters>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(page);

            var result = await _service.GetSectionServicesDtoAsync(CancellationToken.None);

            Assert.That(result.MultiLinksCardsGrid.IsMultiLinks, Is.True);
        }

        [Test]
        public async Task GetSectionServicesDtoAsync_HandlesNullHeroImages()
        {
            var page = CreateSectionServicesPage();
            page.HeroBackgroundImage = null;
            page.HeroBackgroundImage_Mobile = null;
            page.HeroCallToActionButtonUrl = null;

            _contentServiceMock
                .Setup(x => x.RetrieveCurrentPageAsync<SectionServices>(
                    It.IsAny<RetrieveCurrentPageParameters>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(page);

            await _service.GetSectionServicesDtoAsync(CancellationToken.None);

            _heroServiceMock.Verify(x => x.GetHeroDtoAsync(
                It.Is<HeroData>(h =>
                    h.BackgroundImage == null &&
                    h.MobileBackgroundImage == null &&
                    h.CallToActionButtonUrl == null),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        private static SectionServices CreateSectionServicesPage()
        {
            return new SectionServices
            {
                GridTitle = "Grid Title",
                GridSummary = "Grid Summary",
                HeroTitle = "Hero Title",
                HeroDescription = "Hero Description",
                HeroLayout = "Default",
                ShowMultiLinks = false,
                HeroBackgroundImage = new List<ContentAttachment>(),
                HeroBackgroundImage_Mobile = new List<ContentAttachment>(),
                HeroCallToActionButtonUrl = new List<IWebPageFieldsSource>(),
                SystemFields = new WebPageFields
                {
                    WebPageItemID = 1
                }
            };
        }
    }
}