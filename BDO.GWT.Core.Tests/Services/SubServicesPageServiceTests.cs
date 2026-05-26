using BDO.GWT.Core;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.Services;
using BDO.GWT.Web.Core.DTOs;
using BDO.GWT.Web.Core.Services;
using CMS.Websites;
using Kentico.Content.Web.Mvc;
using Moq;

namespace BDO.GWT.Core.Tests.Services
{
    public class SubServicesPageServiceTests
    {
        private Mock<IHeroService> _heroServiceMock = null!;
        private Mock<IContentService> _contentServiceMock = null!;
        private SubServicesPageService _service = null!;

        [SetUp]
        public void Setup()
        {
            _heroServiceMock = new Mock<IHeroService>();
            _contentServiceMock = new Mock<IContentService>();

            _heroServiceMock
                .Setup(x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HeroDto { Title = "Hero Title" });

            _service = new SubServicesPageService(
                _heroServiceMock.Object,
                _contentServiceMock.Object);
        }

        [Test]
        public async Task GetSubServicesPageDtoAsync_ReturnsEmptyDto_WhenPageIsNull()
        {
            _contentServiceMock
                .Setup(x => x.RetrieveCurrentPageAsync<Sub_Service>(
                    It.IsAny<RetrieveCurrentPageParameters>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((Sub_Service?)null);

            var result = await _service.GetSubServicesPageDtoAsync(CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Hero, Is.Null);
        }

        [Test]
        public async Task GetSubServicesPageDtoAsync_ReturnsDto_WhenPageExists()
        {
            var page = CreateSubServicePage();

            _contentServiceMock
                .Setup(x => x.RetrieveCurrentPageAsync<Sub_Service>(
                    It.IsAny<RetrieveCurrentPageParameters>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(page);

            var result = await _service.GetSubServicesPageDtoAsync(CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Hero, Is.Not.Null);
            Assert.That(result.Hero.Title, Is.EqualTo("Hero Title"));
        }

        [Test]
        public async Task GetSubServicesPageDtoAsync_CallsHeroService_WhenPageExists()
        {
            var page = CreateSubServicePage();

            _contentServiceMock
                .Setup(x => x.RetrieveCurrentPageAsync<Sub_Service>(
                    It.IsAny<RetrieveCurrentPageParameters>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(page);

            await _service.GetSubServicesPageDtoAsync(CancellationToken.None);

            _heroServiceMock.Verify(
                x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task GetSubServicesPageDtoAsync_DoesNotCallHeroService_WhenPageIsNull()
        {
            _contentServiceMock
                .Setup(x => x.RetrieveCurrentPageAsync<Sub_Service>(
                    It.IsAny<RetrieveCurrentPageParameters>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((Sub_Service?)null);

            await _service.GetSubServicesPageDtoAsync(CancellationToken.None);

            _heroServiceMock.Verify(
                x => x.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        private static Sub_Service CreateSubServicePage()
        {
            return new Sub_Service
            {
                SystemFields = new WebPageFields
                {
                    WebPageItemID = 1,
                    WebPageItemParentID = 2
                },
                HeroLayout = "default",
                HeroTitle = "Sub Service Hero",
                HeroDescription = "Sub Service Hero Desc",
                HeroBackgroundImageAltText = "Alt",
                HeroBackgroundImage_MobileAltText = "Mobile Alt",
                HeroCallToActionButtonType = "link",
                HeroCallToActionButtonText = "Click",
                HeroBackgroundImage = [],
                HeroBackgroundImage_Mobile = [],
                HeroCallToActionButtonUrl = []
            };
        }
    }
}