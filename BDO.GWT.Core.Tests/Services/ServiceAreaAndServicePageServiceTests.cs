using BDO.GWT.Core;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.services;
using BDO.GWT.Core.Services;
using BDO.GWT.Web.Core.DTOs;
using BDO.GWT.Web.Core.Services;
using CMS.Websites;
using Kentico.Content.Web.Mvc;
using Moq;

namespace BDO.GWT.Core.Tests.Services
{
    public class ServiceAreaAndServicePageServiceTests
    {
        private Mock<IHeroService> _heroServiceMock = null!;
        private Mock<IContentService> _contentServiceMock = null!;
        private Mock<IPageUtility> _pageUtilityMock = null!;
        private ServiceAreaAndServicePageService _service = null!;

        [SetUp]
        public void Setup()
        {
            _heroServiceMock = new Mock<IHeroService>();
            _contentServiceMock = new Mock<IContentService>();
            _pageUtilityMock = new Mock<IPageUtility>();

            _heroServiceMock
                .Setup(x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HeroDto { Title = "Hero Title" });

            _service = new ServiceAreaAndServicePageService(
                _heroServiceMock.Object,
                _contentServiceMock.Object,
                _pageUtilityMock.Object);
        }

        #region GetServiceAreaPageDtoAsync

        [Test]
        public async Task GetServiceAreaPageDtoAsync_ReturnsEmptyDto_WhenPageIsNull()
        {
            _contentServiceMock
                .Setup(x => x.RetrieveCurrentPageAsync<ServiceArea>(
                    It.IsAny<RetrieveCurrentPageParameters>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((ServiceArea?)null);

            var result = await _service.GetServiceAreaPageDtoAsync(CancellationToken.None);

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
        public async Task GetServiceAreaPageDtoAsync_ReturnsDto_WhenPageExists()
        {
            var page = CreateServiceAreaPage(showSideNavigation: false);

            _contentServiceMock
                .Setup(x => x.RetrieveCurrentPageAsync<ServiceArea>(
                    It.IsAny<RetrieveCurrentPageParameters>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(page);

            var result = await _service.GetServiceAreaPageDtoAsync(CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result.Title, Is.EqualTo("Test Title"));
                Assert.That(result.BodyContent, Is.EqualTo("<p>Body</p>"));
                Assert.That(result.Hero, Is.Not.Null);
                Assert.That(result.ShowSideNavigation, Is.False);
                Assert.That(result.Sidebox, Is.Null);
            });
        }

        [Test]
        public async Task GetServiceAreaPageDtoAsync_ReturnsSidebox_WhenSideNavigationEnabled()
        {
            var page = CreateServiceAreaPage(showSideNavigation: true);
            var expectedSidebox = new SideBoxDto { Title = "Side Nav", Links = [] };

            _contentServiceMock
                .Setup(x => x.RetrieveCurrentPageAsync<ServiceArea>(
                    It.IsAny<RetrieveCurrentPageParameters>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(page);

            _pageUtilityMock
                .Setup(x => x.GetSideboxDtoAsync<Service, ServiceArea>(
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<Func<Service, string>>(),
                    It.IsAny<Func<ServiceArea, string>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedSidebox);

            var result = await _service.GetServiceAreaPageDtoAsync(CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.ShowSideNavigation, Is.True);
                Assert.That(result.Sidebox, Is.Not.Null);
                Assert.That(result.Sidebox!.Title, Is.EqualTo("Side Nav"));
            });
        }

        [Test]
        public async Task GetServiceAreaPageDtoAsync_DoesNotCallGetSidebox_WhenSideNavigationDisabled()
        {
            var page = CreateServiceAreaPage(showSideNavigation: false);

            _contentServiceMock
                .Setup(x => x.RetrieveCurrentPageAsync<ServiceArea>(
                    It.IsAny<RetrieveCurrentPageParameters>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(page);

            await _service.GetServiceAreaPageDtoAsync(CancellationToken.None);

            _pageUtilityMock.Verify(
                x => x.GetSideboxDtoAsync<Service, ServiceArea>(
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<Func<Service, string>>(),
                    It.IsAny<Func<ServiceArea, string>>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public async Task GetServiceAreaPageDtoAsync_CallsHeroService_WhenPageExists()
        {
            var page = CreateServiceAreaPage(showSideNavigation: false);

            _contentServiceMock
                .Setup(x => x.RetrieveCurrentPageAsync<ServiceArea>(
                    It.IsAny<RetrieveCurrentPageParameters>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(page);

            await _service.GetServiceAreaPageDtoAsync(CancellationToken.None);

            _heroServiceMock.Verify(
                x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task GetServiceAreaPageDtoAsync_DoesNotCallHeroService_WhenPageIsNull()
        {
            _contentServiceMock
                .Setup(x => x.RetrieveCurrentPageAsync<ServiceArea>(
                    It.IsAny<RetrieveCurrentPageParameters>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((ServiceArea?)null);

            await _service.GetServiceAreaPageDtoAsync(CancellationToken.None);

            _heroServiceMock.Verify(
                x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        #endregion

        #region GetServicePageDtoAsync

        [Test]
        public async Task GetServicePageDtoAsync_ReturnsEmptyDto_WhenPageIsNull()
        {
            _contentServiceMock
                .Setup(x => x.RetrieveCurrentPageAsync<Service>(
                    It.IsAny<RetrieveCurrentPageParameters>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((Service?)null);

            var result = await _service.GetServicePageDtoAsync(CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result.Hero, Is.Null);
                Assert.That(result.Sidebox, Is.Null);
            });
        }

        [Test]
        public async Task GetServicePageDtoAsync_ReturnsDto_WhenPageExists()
        {
            var page = CreateServicePage(showSideNavigation: false);

            _contentServiceMock
                .Setup(x => x.RetrieveCurrentPageAsync<Service>(
                    It.IsAny<RetrieveCurrentPageParameters>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(page);

            var result = await _service.GetServicePageDtoAsync(CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result.Hero, Is.Not.Null);
                Assert.That(result.ShowSideNavigation, Is.False);
                Assert.That(result.Sidebox, Is.Null);
            });
        }

        [Test]
        public async Task GetServicePageDtoAsync_ReturnsSidebox_WhenSideNavigationEnabled()
        {
            var page = CreateServicePage(showSideNavigation: true);
            var expectedSidebox = new SideBoxDto { Title = "Sub Service Nav", Links = [] };

            _contentServiceMock
                .Setup(x => x.RetrieveCurrentPageAsync<Service>(
                    It.IsAny<RetrieveCurrentPageParameters>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(page);

            _pageUtilityMock
                .Setup(x => x.GetSideboxDtoAsync<Sub_Service, Service>(
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<Func<Sub_Service, string>>(),
                    It.IsAny<Func<Service, string>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedSidebox);

            var result = await _service.GetServicePageDtoAsync(CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.ShowSideNavigation, Is.True);
                Assert.That(result.Sidebox, Is.Not.Null);
                Assert.That(result.Sidebox!.Title, Is.EqualTo("Sub Service Nav"));
            });
        }

        [Test]
        public async Task GetServicePageDtoAsync_DoesNotCallGetSidebox_WhenSideNavigationDisabled()
        {
            var page = CreateServicePage(showSideNavigation: false);

            _contentServiceMock
                .Setup(x => x.RetrieveCurrentPageAsync<Service>(
                    It.IsAny<RetrieveCurrentPageParameters>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(page);

            await _service.GetServicePageDtoAsync(CancellationToken.None);

            _pageUtilityMock.Verify(
                x => x.GetSideboxDtoAsync<Sub_Service, Service>(
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<Func<Sub_Service, string>>(),
                    It.IsAny<Func<Service, string>>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public async Task GetServicePageDtoAsync_CallsHeroService_WhenPageExists()
        {
            var page = CreateServicePage(showSideNavigation: false);

            _contentServiceMock
                .Setup(x => x.RetrieveCurrentPageAsync<Service>(
                    It.IsAny<RetrieveCurrentPageParameters>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(page);

            await _service.GetServicePageDtoAsync(CancellationToken.None);

            _heroServiceMock.Verify(
                x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task GetServicePageDtoAsync_DoesNotCallHeroService_WhenPageIsNull()
        {
            _contentServiceMock
                .Setup(x => x.RetrieveCurrentPageAsync<Service>(
                    It.IsAny<RetrieveCurrentPageParameters>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((Service?)null);

            await _service.GetServicePageDtoAsync(CancellationToken.None);

            _heroServiceMock.Verify(
                x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        #endregion

        #region Helpers

        private static ServiceArea CreateServiceAreaPage(bool showSideNavigation)
        {
            return new ServiceArea
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

        private static Service CreateServicePage(bool showSideNavigation)
        {
            return new Service
            {
                SystemFields = new WebPageFields
                {
                    WebPageItemID = 10,
                    WebPageItemParentID = 20
                },
                HeroLayout = "default",
                HeroTitle = "Service Hero",
                HeroDescription = "Service Hero Desc",
                HeroBackgroundImageAltText = "Alt",
                HeroBackgroundImage_MobileAltText = "Mobile Alt",
                HeroCallToActionButtonType = "link",
                HeroCallToActionButtonText = "Click",
                HeroBackgroundImage = [],
                HeroBackgroundImage_Mobile = [],
                HeroCallToActionButtonUrl = [],
                ShowSideNavigation = showSideNavigation,
                SideNavigationTitle = "Side Nav Title"
            };
        }

        #endregion
    }
}