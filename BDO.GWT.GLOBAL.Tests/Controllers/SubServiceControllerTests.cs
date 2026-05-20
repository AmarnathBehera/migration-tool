using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.Services;
using BDO.GWT.Web.Core.DTOs;
using GWTXbyKWeb.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BDO.GWT.GLOBAL.Tests.Controllers
{
    public class SubServiceControllerTests
    {
        private Mock<ISubServicesPageService> _subServicesPageServiceMock = null!;
        private SubServiceController _controller = null!;

        [SetUp]
        public void Setup()
        {
            _subServicesPageServiceMock = new Mock<ISubServicesPageService>();
            _controller = new SubServiceController(_subServicesPageServiceMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _controller.Dispose();
        }

        [Test]
        public async Task Index_ReturnsViewResult_WithCorrectViewPath()
        {
            _subServicesPageServiceMock
                .Setup(x => x.GetSubServicesPageDtoAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new SubServicePageDto());

            var result = await _controller.Index(CancellationToken.None);

            var viewResult = result as ViewResult;
            Assert.That(viewResult, Is.Not.Null);
            Assert.That(viewResult!.ViewName, Is.EqualTo("~/Views/SubServices/Index.cshtml"));
        }

        [Test]
        public async Task Index_ReturnsViewResult_WithMappedModel()
        {
            var expectedDto = new SubServicePageDto
            {
                Hero = new HeroDto { Title = "Sub Service Hero" }
            };

            _subServicesPageServiceMock
                .Setup(x => x.GetSubServicesPageDtoAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedDto);

            var result = await _controller.Index(CancellationToken.None);

            var viewResult = result as ViewResult;
            Assert.That(viewResult, Is.Not.Null);

            var model = viewResult!.Model as SubServicePageDto;
            Assert.That(model, Is.Not.Null);
            Assert.That(model!.Hero.Title, Is.EqualTo("Sub Service Hero"));
        }

        [Test]
        public async Task Index_ReturnsViewResult_WithEmptyDto_WhenServiceReturnsDefault()
        {
            _subServicesPageServiceMock
                .Setup(x => x.GetSubServicesPageDtoAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new SubServicePageDto());

            var result = await _controller.Index(CancellationToken.None);

            var viewResult = result as ViewResult;
            Assert.That(viewResult, Is.Not.Null);

            var model = viewResult!.Model as SubServicePageDto;
            Assert.That(model, Is.Not.Null);
            Assert.That(model!.Hero, Is.Null);
        }

        [Test]
        public async Task Index_CallsServiceExactlyOnce()
        {
            _subServicesPageServiceMock
                .Setup(x => x.GetSubServicesPageDtoAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new SubServicePageDto());

            await _controller.Index(CancellationToken.None);

            _subServicesPageServiceMock.Verify(
                x => x.GetSubServicesPageDtoAsync(It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}