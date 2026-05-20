using BDO.GWT.Core;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.services;
using BDO.GWT.Core.Services;
using BDO.GWT.Core.Utilities;
using BDO.GWT.Web.Core.DTOs;
using BDO.GWT.Web.Core.Services;
using CMS.Websites;
using Kentico.Content.Web.Mvc;
using Moq;

namespace BDO.GWT.Core.Tests.Services
{
    public class BusinessLinePageServiceTests
    {
        private Mock<IHeroService> _heroServiceMock = null!;
        private Mock<IContentService> _contentServiceMock = null!;
        private Mock<IPageUtility> _pageUtilityMock = null!;
        private BusinessLinePageService _service = null!;

        [SetUp]
        public void Setup()
        {
            _heroServiceMock = new Mock<IHeroService>();
            _contentServiceMock = new Mock<IContentService>();
            _pageUtilityMock = new Mock<IPageUtility>();

            _heroServiceMock
                .Setup(x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HeroDto { Title = "Hero Title" });

            _service = new BusinessLinePageService(
                _heroServiceMock.Object,
                _contentServiceMock.Object,
                _pageUtilityMock.Object);
        }

        [Test]
        public async Task GetBusinessLinePageDtoAsync_ReturnsEmptyDto_WhenPageIsNull()
        {
            _contentServiceMock
                .Setup(x => x.RetrieveCurrentPageAsync<BusinessLine>(
                    It.IsAny<RetrieveCurrentPageParameters>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((BusinessLine?)null);

            var result = await _service.GetBusinessLinePageDtoAsync(CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result.Hero, Is.Null);
                Assert.That(result.Title, Is.Null);
                Assert.That(result.BodyContent, Is.Null);
                Assert.That(result.Sidebox, Is.Null);
            });
        }

        [Test]
        public async Task GetBusinessLinePageDtoAsync_ReturnsDto_WhenPageExists()
        {
            var page = CreateBusinessLinePage(showSideNavigation: false);

            _contentServiceMock
                .Setup(x => x.RetrieveCurrentPageAsync<BusinessLine>(
                    It.IsAny<RetrieveCurrentPageParameters>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(page);

            var result = await _service.GetBusinessLinePageDtoAsync(CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result.Title, Is.EqualTo("Test Title"));
                Assert.That(result.Hero, Is.Not.Null);
                Assert.That(result.ShowSideNavigation, Is.False);
                Assert.That(result.Sidebox, Is.Null);
            });
        }

        [Test]
        public async Task GetBusinessLinePageDtoAsync_ReturnsSidebox_WhenSideNavigationEnabled()
        {
            var page = CreateBusinessLinePage(showSideNavigation: true);
            var expectedSidebox = new SideBoxDto
            {
                Title = "Side Nav",
                Links = []
            };

            _contentServiceMock
                .Setup(x => x.RetrieveCurrentPageAsync<BusinessLine>(
                    It.IsAny<RetrieveCurrentPageParameters>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(page);

            _pageUtilityMock
                .Setup(x => x.GetSideboxDtoAsync<ServiceArea, BusinessLine>(
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<Func<ServiceArea, string>>(),
                    It.IsAny<Func<BusinessLine, string>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedSidebox);

            var result = await _service.GetBusinessLinePageDtoAsync(CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.ShowSideNavigation, Is.True);
                Assert.That(result.Sidebox, Is.Not.Null);
                Assert.That(result.Sidebox!.Title, Is.EqualTo("Side Nav"));
            });
        }

        [Test]
        public async Task GetBusinessLinePageDtoAsync_DoesNotCallGetSidebox_WhenSideNavigationDisabled()
        {
            var page = CreateBusinessLinePage(showSideNavigation: false);

            _contentServiceMock
                .Setup(x => x.RetrieveCurrentPageAsync<BusinessLine>(
                    It.IsAny<RetrieveCurrentPageParameters>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(page);

            await _service.GetBusinessLinePageDtoAsync(CancellationToken.None);

            _pageUtilityMock.Verify(
                x => x.GetSideboxDtoAsync<ServiceArea, BusinessLine>(
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<Func<ServiceArea, string>>(),
                    It.IsAny<Func<BusinessLine, string>>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public async Task GetBusinessLinePageDtoAsync_CallsHeroService_WhenPageExists()
        {
            var page = CreateBusinessLinePage(showSideNavigation: false);

            _contentServiceMock
                .Setup(x => x.RetrieveCurrentPageAsync<BusinessLine>(
                    It.IsAny<RetrieveCurrentPageParameters>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(page);

            await _service.GetBusinessLinePageDtoAsync(CancellationToken.None);

            _heroServiceMock.Verify(
                x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task GetBusinessLinePageDtoAsync_DoesNotCallHeroService_WhenPageIsNull()
        {
            _contentServiceMock
                .Setup(x => x.RetrieveCurrentPageAsync<BusinessLine>(
                    It.IsAny<RetrieveCurrentPageParameters>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((BusinessLine?)null);

            await _service.GetBusinessLinePageDtoAsync(CancellationToken.None);

            _heroServiceMock.Verify(
                x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        private static BusinessLine CreateBusinessLinePage(bool showSideNavigation)
        {
            return new BusinessLine
            {
                SystemFields = new WebPageFields
                {
                    WebPageItemID = 1,
                    WebPageItemParentID = 2
                },
                ContentTitle = "Test Title",
                ContentBody = "<p>Body</p>",
                HeroLayout = "default",
                HeroTitle = "Hero",
                HeroDescription = "Hero Desc",
                HeroBackgroundImageAltText = "Alt",
                HeroBackgroundImage_MobileAltText = "Mobile Alt",
                HeroCallToActionButtonType = "link",
                HeroCallToActionButtonText = "Click",
                HeroBackgroundImage = [],
                HeroBackgroundImage_Mobile = [],
                HeroCallToActionButtonUrl = [],
                ShowSideNavigation = showSideNavigation,
                SideNavigationTitle = "Side Nav Title",
                TileIcon = []
            };
        }
    }
}