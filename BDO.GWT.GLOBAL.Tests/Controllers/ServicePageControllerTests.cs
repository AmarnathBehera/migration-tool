using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.Services;
using BDO.GWT.Web.Core.DTOs;
using GWTXbyKWeb.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BDO.GWT.GLOBAL.Tests.Controllers
{
    public class ServicePageControllerTests
    {
        private Mock<IServiceAreaAndServicePageService> _serviceAreaAndServicePageServiceMock = null!;
        private ServicePageController _controller = null!;

        [SetUp]
        public void Setup()
        {
            _serviceAreaAndServicePageServiceMock = new Mock<IServiceAreaAndServicePageService>();
            _controller = new ServicePageController(_serviceAreaAndServicePageServiceMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _controller.Dispose();
        }

        [Test]
        public async Task Index_ReturnsViewResult_WithCorrectViewPath()
        {
            _serviceAreaAndServicePageServiceMock
                .Setup(x => x.GetServicePageDtoAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ServicePageDto());

            var result = await _controller.Index(CancellationToken.None);

            var viewResult = result as ViewResult;
            Assert.That(viewResult, Is.Not.Null);
            Assert.That(viewResult!.ViewName, Is.EqualTo("~/Views/ServicePage/Index.cshtml"));
        }

        [Test]
        public async Task Index_ReturnsViewResult_WithMappedModel()
        {
            var expectedDto = new ServicePageDto
            {
                Hero = new HeroDto { Title = "Service Hero" },
                ShowSideNavigation = true,
                Sidebox = new SideBoxDto
                {
                    Title = "Side Nav",
                    Links = []
                }
            };

            _serviceAreaAndServicePageServiceMock
                .Setup(x => x.GetServicePageDtoAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedDto);

            var result = await _controller.Index(CancellationToken.None);

            var viewResult = result as ViewResult;
            Assert.That(viewResult, Is.Not.Null);

            var model = viewResult!.Model as ServicePageDto;
            Assert.Multiple(() =>
            {
                Assert.That(model, Is.Not.Null);
                Assert.That(model!.Hero.Title, Is.EqualTo("Service Hero"));
                Assert.That(model.ShowSideNavigation, Is.True);
                Assert.That(model.Sidebox, Is.Not.Null);
                Assert.That(model.Sidebox!.Title, Is.EqualTo("Side Nav"));
            });
        }

        [Test]
        public async Task Index_ReturnsViewResult_WithEmptyDto_WhenServiceReturnsDefault()
        {
            _serviceAreaAndServicePageServiceMock
                .Setup(x => x.GetServicePageDtoAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ServicePageDto());

            var result = await _controller.Index(CancellationToken.None);

            var viewResult = result as ViewResult;
            Assert.That(viewResult, Is.Not.Null);

            var model = viewResult!.Model as ServicePageDto;
            Assert.Multiple(() =>
            {
                Assert.That(model, Is.Not.Null);
                Assert.That(model!.Hero, Is.Null);
                Assert.That(model.Sidebox, Is.Null);
                Assert.That(model.ShowSideNavigation, Is.False);
            });
        }

        [Test]
        public async Task Index_CallsServiceExactlyOnce()
        {
            _serviceAreaAndServicePageServiceMock
                .Setup(x => x.GetServicePageDtoAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ServicePageDto());

            await _controller.Index(CancellationToken.None);

            _serviceAreaAndServicePageServiceMock.Verify(
                x => x.GetServicePageDtoAsync(It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}