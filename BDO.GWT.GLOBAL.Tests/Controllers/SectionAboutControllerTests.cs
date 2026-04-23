using BDO;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.Services;
using BDO.GWT.Web.Controllers;
using BDO.GWT.Web.Core.DTOs;
using BDO.GWT.Web.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.GLOBAL.Tests.Controllers
{
    public class SectionAboutControllerTests
    {
        private Mock<IPageContext> _pageContextMock = null!;
        private Mock<IAboutPageService<SectionAbout>> _aboutPageServiceMock = null!;
        private SectionAboutController _controller = null!;

        [SetUp]
        public void Setup()
        {
            _pageContextMock = new Mock<IPageContext>();
            _aboutPageServiceMock = new Mock<IAboutPageService<SectionAbout>>();
            _controller = new SectionAboutController(_pageContextMock.Object, _aboutPageServiceMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _controller.Dispose();
        }

        [Test]
        public async Task Index_ReturnsViewResult_WithDtoFromService()
        {
            var sectionAboutPage = new SectionAbout();
            var expectedDto = new AboutDto { Hero = new HeroDto { Layout = "Default", Title = "Section About" } };

            _pageContextMock
                .Setup(s => s.GetCurrentPageAsync<SectionAbout>(It.IsAny<CancellationToken>()))
                .ReturnsAsync(sectionAboutPage);

            _aboutPageServiceMock
                .Setup(s => s.GetAboutDtoAsync(sectionAboutPage, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedDto);

            var result = await _controller.Index(CancellationToken.None);

            var viewResult = result as ViewResult;
            Assert.Multiple(() =>
            {
                Assert.That(viewResult, Is.Not.Null);
                Assert.That(viewResult?.Model, Is.SameAs(expectedDto));
            });
        }

        [Test]
        public async Task Index_WhenPageContextReturnsNull_ReturnsNotFound()
        {
            _pageContextMock
                .Setup(s => s.GetCurrentPageAsync<SectionAbout>(It.IsAny<CancellationToken>()))
                .ReturnsAsync((SectionAbout?)null);

            var result = await _controller.Index(CancellationToken.None);

            Assert.That(result, Is.TypeOf<NotFoundResult>());

            _aboutPageServiceMock.Verify(
                s => s.GetAboutDtoAsync(It.IsAny<SectionAbout>(), It.IsAny<CancellationToken>()),
                Times.Never
            );
        }

        [Test]
        public async Task Index_PassesCorrectCancellationToken_ToService()
        {
            var token = new CancellationTokenSource().Token;
            var sectionAboutPage = new SectionAbout();

            _pageContextMock
                .Setup(s => s.GetCurrentPageAsync<SectionAbout>(token))
                .ReturnsAsync(sectionAboutPage);

            _aboutPageServiceMock
                .Setup(s => s.GetAboutDtoAsync(sectionAboutPage, token))
                .ReturnsAsync(new AboutDto { Hero = new HeroDto { Layout = "Default" } });

            await _controller.Index(token);

            _aboutPageServiceMock.Verify(
                s => s.GetAboutDtoAsync(sectionAboutPage, token),
                Times.Once
            );
        }

        [Test]
        public async Task Index_ReturnsCorrectViewName()
        {
            var sectionAboutPage = new SectionAbout();

            _pageContextMock
                .Setup(s => s.GetCurrentPageAsync<SectionAbout>(It.IsAny<CancellationToken>()))
                .ReturnsAsync(sectionAboutPage);

            _aboutPageServiceMock
                .Setup(s => s.GetAboutDtoAsync(sectionAboutPage, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new AboutDto { Hero = new HeroDto { Layout = "Default" } });

            var result = await _controller.Index(CancellationToken.None) as ViewResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.ViewName, Is.EqualTo("~/Views/About/Index.cshtml"));
        }

        [Test]
        public async Task Index_PassesCorrectPageInstance_ToService()
        {
            var sectionAboutPage = new SectionAbout();

            _pageContextMock
                .Setup(s => s.GetCurrentPageAsync<SectionAbout>(It.IsAny<CancellationToken>()))
                .ReturnsAsync(sectionAboutPage);

            _aboutPageServiceMock
                .Setup(s => s.GetAboutDtoAsync(sectionAboutPage, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new AboutDto { Hero = new HeroDto { Layout = "Default" } });

            await _controller.Index(CancellationToken.None);

            _aboutPageServiceMock.Verify(
                s => s.GetAboutDtoAsync(sectionAboutPage, It.IsAny<CancellationToken>()),
                Times.Once
            );
        }

        [Test]
        public void Index_WhenServiceThrows_ExceptionIsPropagated()
        {
            var sectionAboutPage = new SectionAbout();

            _pageContextMock
                .Setup(s => s.GetCurrentPageAsync<SectionAbout>(It.IsAny<CancellationToken>()))
                .ReturnsAsync(sectionAboutPage);

            _aboutPageServiceMock
                .Setup(s => s.GetAboutDtoAsync(sectionAboutPage, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception());

            Assert.ThrowsAsync<Exception>(async () =>
                await _controller.Index(CancellationToken.None));
        }
    }
}
