using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.Services;
using BDO.GWT.Web.Core.DTOs;
using GWTXbyKWeb.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.GLOBAL.Tests.Controllers
{
    public class BusinessLineControllerTests
    {
        private Mock<IBusinessLinePageService> _businessLinePageServiceMock = null!;
        private BusinessLineController _controller = null!;

        [SetUp]
        public void Setup()
        {
            _businessLinePageServiceMock = new Mock<IBusinessLinePageService>();
            _controller = new BusinessLineController(_businessLinePageServiceMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _controller.Dispose();
        }

        [Test]
        public async Task Index_ReturnsViewResult_WithCorrectViewPath()
        {
            _businessLinePageServiceMock
                .Setup(x => x.GetBusinessLinePageDtoAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new BusinessLinePageDto());

            var result = await _controller.Index(CancellationToken.None);

            var viewResult = result as ViewResult;
            Assert.That(viewResult, Is.Not.Null);
            Assert.That(viewResult!.ViewName, Is.EqualTo("~/Views/BusinessLine/Index.cshtml"));
        }

        [Test]
        public async Task Index_ReturnsViewResult_WithMappedModel()
        {
            var expectedDto = new BusinessLinePageDto
            {
                Title = "Test Business Line",
                BodyContent = "<p>Content</p>",
                Hero = new HeroDto { Title = "Hero Title" },
                ShowSideNavigation = true,
                Sidebox = new SideBoxDto
                {
                    Title = "Side Nav",
                    Links = []
                }
            };

            _businessLinePageServiceMock
                .Setup(x => x.GetBusinessLinePageDtoAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedDto);

            var result = await _controller.Index(CancellationToken.None);

            var viewResult = result as ViewResult;
            Assert.That(viewResult, Is.Not.Null);

            var model = viewResult!.Model as BusinessLinePageDto;
            Assert.Multiple(() =>
            {
                Assert.That(model, Is.Not.Null);
                Assert.That(model!.Title, Is.EqualTo("Test Business Line"));
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
            _businessLinePageServiceMock
                .Setup(x => x.GetBusinessLinePageDtoAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new BusinessLinePageDto());

            var result = await _controller.Index(CancellationToken.None);

            var viewResult = result as ViewResult;
            Assert.That(viewResult, Is.Not.Null);

            var model = viewResult!.Model as BusinessLinePageDto;
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
            _businessLinePageServiceMock
                .Setup(x => x.GetBusinessLinePageDtoAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new BusinessLinePageDto());

            await _controller.Index(CancellationToken.None);

            _businessLinePageServiceMock.Verify(
                x => x.GetBusinessLinePageDtoAsync(It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}