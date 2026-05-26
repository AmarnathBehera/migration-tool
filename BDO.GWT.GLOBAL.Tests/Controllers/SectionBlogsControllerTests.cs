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
    public class SectionBlogsControllerTests
    {
        private Mock<ISectionBlogService> _sectionBlogServiceMock = null!;
        private SectionBlogsController _controller = null!;

        [SetUp]
        public void Setup()
        {
            _sectionBlogServiceMock = new Mock<ISectionBlogService>();
            _controller = new SectionBlogsController(_sectionBlogServiceMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _controller.Dispose();
        }

        [Test]
        public async Task Index_ReturnsViewResult_WithCorrectViewPath()
        {
            // Arrange
            var expectedDto = new SectionBlogsPageDto();
            _sectionBlogServiceMock
                .Setup(x => x.GetSectionBlogsDtoAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedDto);

            // Act
            var result = await _controller.Index(CancellationToken.None);

            // Assert
            var viewResult = result as ViewResult;
            Assert.That(viewResult, Is.Not.Null);
            Assert.That(viewResult!.ViewName, Is.EqualTo("~/Views/SectionBlogs/Index.cshtml"));
        }

        [Test]
        public async Task Index_ReturnsViewResult_WithExpectedModel()
        {
            // Arrange
            var expectedDto = new SectionBlogsPageDto
            {
                Hero = new HeroDto { Title = "Blog Hero" },
                MultiLinksCardsGrid = new MultiLinkCardsGridDto { Title = "All Blogs" }
            };
            _sectionBlogServiceMock
                .Setup(x => x.GetSectionBlogsDtoAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedDto);

            // Act
            var result = await _controller.Index(CancellationToken.None);

            // Assert
            var viewResult = result as ViewResult;
            Assert.That(viewResult, Is.Not.Null);
            Assert.That(viewResult!.Model, Is.EqualTo(expectedDto));
        }

        [Test]
        public async Task Index_CallsServiceOnce()
        {
            // Arrange
            _sectionBlogServiceMock
                .Setup(x => x.GetSectionBlogsDtoAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new SectionBlogsPageDto());

            // Act
            await _controller.Index(CancellationToken.None);

            // Assert
            _sectionBlogServiceMock.Verify(
                x => x.GetSectionBlogsDtoAsync(It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task Index_PassesCancellationToken_ToService()
        {
            // Arrange
            using var cts = new CancellationTokenSource();
            var token = cts.Token;

            _sectionBlogServiceMock
                .Setup(x => x.GetSectionBlogsDtoAsync(token))
                .ReturnsAsync(new SectionBlogsPageDto());

            // Act
            await _controller.Index(token);

            // Assert
            _sectionBlogServiceMock.Verify(
                x => x.GetSectionBlogsDtoAsync(token),
                Times.Once);
        }
    }
}