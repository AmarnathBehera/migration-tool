using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.Services;
using BDO.GWT.Web.Core.DTOs;
using GWTXbyKWeb.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BDO.GWT.GLOBAL.Tests.Controllers
{
    public class ServiceAreaControllerTests
    {
        private Mock<IServiceAreaAndServicePageService> _serviceAreaAndServicePageServiceMock = null!;
        private ServiceAreaController _controller = null!;

        [SetUp]
        public void Setup()
        {
            _serviceAreaAndServicePageServiceMock = new Mock<IServiceAreaAndServicePageService>();
            _controller = new ServiceAreaController(_serviceAreaAndServicePageServiceMock.Object);
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
                .Setup(x => x.GetServiceAreaPageDtoAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ServicesAreaPageDto());

            var result = await _controller.Index(CancellationToken.None);

            var viewResult = result as ViewResult;
            Assert.That(viewResult, Is.Not.Null);
            Assert.That(viewResult!.ViewName, Is.EqualTo("~/Views/ServiceArea/Index.cshtml"));
        }

        [Test]
        public async Task Index_ReturnsViewResult_WithMappedModel()
        {
            var expectedDto = new ServicesAreaPageDto
            {
                Title = "Test Service Area",
                BodyContent = "<p>Content</p>",
                Hero = new HeroDto { Title = "Hero Title" },
                ShowSideNavigation = true,
                Sidebox = new SideBoxDto
                {
                    Title = "Side Nav",
                    Links = []
                }
            };

            _serviceAreaAndServicePageServiceMock
                .Setup(x => x.GetServiceAreaPageDtoAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedDto);

            var result = await _controller.Index(CancellationToken.None);

            var viewResult = result as ViewResult;
            Assert.That(viewResult, Is.Not.Null);

            var model = viewResult!.Model as ServicesAreaPageDto;
            Assert.Multiple(() =>
            {
                Assert.That(model, Is.Not.Null);
                Assert.That(model!.Title, Is.EqualTo("Test Service Area"));
                Assert.That(model.BodyContent, Is.EqualTo("<p>Content</p>"));
                Assert.That(model.Hero.Title, Is.EqualTo("Hero Title"));
                Assert.That(model.ShowSideNavigation, Is.True);
                Assert.That(model.Sidebox, Is.Not.Null);
                Assert.That(model.Sidebox!.Title, Is.EqualTo("Side Nav"));
            });
        }

        [Test]
        public async Task Index_ReturnsViewResult_WithEmptyDto_WhenServiceReturnsDefault()
        {
            _serviceAreaAndServicePageServiceMock
                .Setup(x => x.GetServiceAreaPageDtoAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ServicesAreaPageDto());

            var result = await _controller.Index(CancellationToken.None);

            var viewResult = result as ViewResult;
            Assert.That(viewResult, Is.Not.Null);

            var model = viewResult!.Model as ServicesAreaPageDto;
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
                .Setup(x => x.GetServiceAreaPageDtoAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ServicesAreaPageDto());

            await _controller.Index(CancellationToken.None);

            _serviceAreaAndServicePageServiceMock.Verify(
                x => x.GetServiceAreaPageDtoAsync(It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}