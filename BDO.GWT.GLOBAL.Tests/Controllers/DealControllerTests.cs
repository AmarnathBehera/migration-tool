using BDO.GWT.Web.Core.DTOs;
using BDO.GWT.Web.Core.DTOs.Pages;
using BDO.GWT.Web.Core.LiveSite.Controllers;
using BDO.GWT.Web.Core.LiveSite.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.GLOBAL.Tests.Controllers
{
    public class DealControllerTests
    {
        private Mock<IDealPageService> _dealPageServiceMock = null!;
        private DealController _controller = null!;

        [SetUp]
        public void Setup()
        {
            _dealPageServiceMock = new Mock<IDealPageService>();
            _controller = new DealController(_dealPageServiceMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _controller.Dispose();
        }

        [Test]
        public async Task Index_ReturnsViewResult()
        {
            _dealPageServiceMock
                .Setup(x => x.GetDealPageDtoAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DealDto());

            var result = await _controller.Index(CancellationToken.None);

            Assert.That(result, Is.InstanceOf<ViewResult>());
        }

        [Test]
        public async Task Index_PassesDtoFromService_AsModel()
        {
            var expectedDto = new DealDto
            {
                Title = "Sample Deal",
                ClientName = "Client A",
                Content = "<p>Body</p>",
                Hero = new HeroDto { Title = "Hero" }
            };

            _dealPageServiceMock
                .Setup(x => x.GetDealPageDtoAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedDto);

            var result = await _controller.Index(CancellationToken.None) as ViewResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Model, Is.SameAs(expectedDto));
        }

        [Test]
        public async Task Index_CallsServiceExactlyOnce()
        {
            _dealPageServiceMock
                .Setup(x => x.GetDealPageDtoAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DealDto());

            await _controller.Index(CancellationToken.None);

            _dealPageServiceMock.Verify(
                x => x.GetDealPageDtoAsync(It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task Index_PassesCancellationTokenToService()
        {
            using var cts = new CancellationTokenSource();
            var token = cts.Token;

            _dealPageServiceMock
                .Setup(x => x.GetDealPageDtoAsync(token))
                .ReturnsAsync(new DealDto());

            await _controller.Index(token);

            _dealPageServiceMock.Verify(x => x.GetDealPageDtoAsync(token), Times.Once);
        }
    }
}
