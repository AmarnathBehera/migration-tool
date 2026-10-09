using BDO.GWT.Core.DTOs;
using BDO.GWT.Web.Controllers;
using BDO.GWT.Web.Core.DTOs;
using BDO.GWT.Web.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.GLOBAL.Tests.Controllers
{
    public class SectionAboutControllerTests
    {
        private Mock<ISectionAboutPageService> _sectionAboutPageServiceMock = null!;
        private SectionAboutController _controller = null!;

        [SetUp]
        public void Setup()
        {
            _sectionAboutPageServiceMock = new Mock<ISectionAboutPageService>();
            _controller = new SectionAboutController(_sectionAboutPageServiceMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _controller.Dispose();
        }

        [Test]
        public async Task Index_ReturnsViewResult_WithAboutDto()
        {
            var expectedDto = new AboutDto
            {
                Hero = new HeroDto
                {
                    Layout = "Hero",
                    Title = "Section About Title",
                    Description = "Section About Description",
                    BackgroundImage = "/media/bg.jpg",
                    BackgroundImageAltText = "Background Alt",
                    MobileBackgroundImage = "/media/mobile.jpg",
                    MobileBackgroundImageAltText = "Mobile Alt",
                    CtaButtonType = "Primary",
                    CtaButtonText = "Learn More",
                    CtaButtonUrl = "/about"
                }
            };

            _sectionAboutPageServiceMock
                .Setup(s => s.GetAboutDtoAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedDto);

            var result = await _controller.Index(CancellationToken.None);

            Assert.That(result, Is.InstanceOf<ViewResult>());
            var viewResult = (ViewResult)result;
            Assert.Multiple(() =>
            {
                Assert.That(viewResult.ViewName, Is.EqualTo("~/Views/SectionAbout/Index.cshtml"));
                Assert.That(viewResult.Model, Is.EqualTo(expectedDto));
            });
        }

        [Test]
        public async Task Index_ReturnsViewResult_WithEmptyAboutDto_WhenServiceReturnsEmpty()
        {
            var emptyDto = new AboutDto();

            _sectionAboutPageServiceMock
                .Setup(s => s.GetAboutDtoAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(emptyDto);

            var result = await _controller.Index(CancellationToken.None);

            Assert.That(result, Is.InstanceOf<ViewResult>());
            var viewResult = (ViewResult)result;
            Assert.Multiple(() =>
            {
                Assert.That(viewResult.ViewName, Is.EqualTo("~/Views/SectionAbout/Index.cshtml"));
                Assert.That(viewResult.Model, Is.EqualTo(emptyDto));
            });
        }

        [Test]
        public async Task Index_PassesCancellationToken_ToService()
        {
            using var cts = new CancellationTokenSource();
            var token = cts.Token;

            _sectionAboutPageServiceMock
                .Setup(s => s.GetAboutDtoAsync(token))
                .ReturnsAsync(new AboutDto())
                .Verifiable();

            await _controller.Index(token);

            _sectionAboutPageServiceMock.Verify(s => s.GetAboutDtoAsync(token), Times.Once);
        }
    }
}
