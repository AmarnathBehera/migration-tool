using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.Services;
using BDO.GWT.Web.Core.DTOs;
using BDO.GWT.Web.Core.Services;
using GWTXbyKWeb.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BDO.GWT.GLOBAL.Tests.Controllers
{
    public class SitemapPageControllerTests
    {
        private Mock<ISitemapPageService> _sitemapPageServiceMock = null!;
        private SitemapPageController _controller = null!;

        [SetUp]
        public void Setup()
        {
            _sitemapPageServiceMock = new Mock<ISitemapPageService>();
            _controller = new SitemapPageController(_sitemapPageServiceMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _controller.Dispose();
        }

        [Test]
        public async Task Index_ReturnsViewResult()
        {
            _sitemapPageServiceMock
                .Setup(x => x.GetSitemapPageDtoAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new SitemapPageDto());

            var result = await _controller.Index(CancellationToken.None);

            Assert.That(result, Is.InstanceOf<ViewResult>());
        }

        [Test]
        public async Task Index_ReturnsViewResult_WithMappedModel()
        {
            var expectedDto = new SitemapPageDto
            {
                Hero = new HeroDto
                {
                    Title = "Sitemap Hero"
                },
                Items =
                [
                    new SitemapItemDto
                    {
                        Title = "Services",
                        Link = "/services"
                    }
                ]
            };

            _sitemapPageServiceMock
                .Setup(x => x.GetSitemapPageDtoAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedDto);

            var result = await _controller.Index(CancellationToken.None);

            var viewResult = result as ViewResult;

            Assert.That(viewResult, Is.Not.Null);

            var model = viewResult!.Model as SitemapPageDto;

            Assert.Multiple(() =>
            {
                Assert.That(model, Is.Not.Null);
                Assert.That(model!.Hero.Title, Is.EqualTo("Sitemap Hero"));
                Assert.That(model.Items, Has.Count.EqualTo(1));
                Assert.That(model.Items[0].Title, Is.EqualTo("Services"));
                Assert.That(model.Items[0].Link, Is.EqualTo("/services"));
            });
        }

        [Test]
        public async Task Index_ReturnsViewResult_WithEmptyDto_WhenServiceReturnsDefault()
        {
            var dto = new SitemapPageDto
            {
                Items = new List<SitemapItemDto>()
            };

            _sitemapPageServiceMock
                .Setup(x => x.GetSitemapPageDtoAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(dto);

            var result = await _controller.Index(CancellationToken.None);

            var viewResult = result as ViewResult;

            Assert.That(viewResult, Is.Not.Null);

            var model = viewResult!.Model as SitemapPageDto;

            Assert.Multiple(() =>
            {
                Assert.That(model, Is.Not.Null);
                Assert.That(model!.Items, Is.Empty);
            });
        }

        [Test]
        public async Task Index_CallsServiceExactlyOnce()
        {
            _sitemapPageServiceMock
                .Setup(x => x.GetSitemapPageDtoAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new SitemapPageDto());

            await _controller.Index(CancellationToken.None);

            _sitemapPageServiceMock.Verify(
                x => x.GetSitemapPageDtoAsync(It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task Index_PassesCancellationTokenToService()
        {
            var cancellationToken = new CancellationToken();

            _sitemapPageServiceMock
                .Setup(x => x.GetSitemapPageDtoAsync(cancellationToken))
                .ReturnsAsync(new SitemapPageDto());

            await _controller.Index(cancellationToken);

            _sitemapPageServiceMock.Verify(
                x => x.GetSitemapPageDtoAsync(cancellationToken),
                Times.Once);
        }
    }
}