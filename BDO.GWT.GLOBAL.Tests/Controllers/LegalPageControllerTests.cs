using BDO.GWT.Core.DTOs;
using BDO.GWT.Web.Controllers;
using BDO.GWT.Web.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.GLOBAL.Tests.Controllers
{
    public class LegalPageControllerTests
    {
        private Mock<ILegalPageService> _legalPageServiceMock = null!;
        private LegalPageController _controller = null!;

        [SetUp]
        public void Setup()
        {
            _legalPageServiceMock = new Mock<ILegalPageService>();
            _controller = new LegalPageController(_legalPageServiceMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _controller.Dispose();
        }

        [Test]
        public async Task Index_ReturnsViewResult()
        {
            _legalPageServiceMock
                .Setup(x => x.GetLegalPageDtoAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new LegalPageDto());

            var result = await _controller.Index(CancellationToken.None);

            Assert.That(result, Is.InstanceOf<ViewResult>());
        }

        [Test]
        public async Task Index_PassesDtoFromService_AsModel()
        {
            var expectedDto = new LegalPageDto
            {
                Title = "Privacy Policy",
                BodyContent = "<p>Legal content here.</p>",
                Links = new List<LinkItemDto>
                {
                    new() { Title = "Terms of Use", URL = "/legal/terms" }
                }
            };

            _legalPageServiceMock
                .Setup(x => x.GetLegalPageDtoAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedDto);

            var result = await _controller.Index(CancellationToken.None) as ViewResult;

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result!.Model, Is.SameAs(expectedDto));
            });
        }

        [Test]
        public async Task Index_UsesCorrectViewName()
        {
            _legalPageServiceMock
                .Setup(x => x.GetLegalPageDtoAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new LegalPageDto());

            var result = await _controller.Index(CancellationToken.None) as ViewResult;

            Assert.That(result!.ViewName, Is.EqualTo("~/Views/LegalPage/Index.cshtml"));
        }

        [Test]
        public async Task Index_CallsServiceExactlyOnce()
        {
            _legalPageServiceMock
                .Setup(x => x.GetLegalPageDtoAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new LegalPageDto());

            await _controller.Index(CancellationToken.None);

            _legalPageServiceMock.Verify(
                x => x.GetLegalPageDtoAsync(It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task Index_PassesCancellationTokenToService()
        {
            using var cts = new CancellationTokenSource();
            var token = cts.Token;

            _legalPageServiceMock
                .Setup(x => x.GetLegalPageDtoAsync(token))
                .ReturnsAsync(new LegalPageDto());

            await _controller.Index(token);

            _legalPageServiceMock.Verify(x => x.GetLegalPageDtoAsync(token), Times.Once);
        }

        [Test]
        public async Task Index_ModelTitle_MatchesServiceResult()
        {
            var dto = new LegalPageDto { Title = "Cookie Policy" };

            _legalPageServiceMock
                .Setup(x => x.GetLegalPageDtoAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(dto);

            var result = await _controller.Index(CancellationToken.None) as ViewResult;
            var model = result!.Model as LegalPageDto;

            Assert.That(model!.Title, Is.EqualTo("Cookie Policy"));
        }

        [Test]
        public async Task Index_ModelBodyContent_MatchesServiceResult()
        {
            var dto = new LegalPageDto { BodyContent = "<p>Body text.</p>" };

            _legalPageServiceMock
                .Setup(x => x.GetLegalPageDtoAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(dto);

            var result = await _controller.Index(CancellationToken.None) as ViewResult;
            var model = result!.Model as LegalPageDto;

            Assert.That(model!.BodyContent, Is.EqualTo("<p>Body text.</p>"));
        }

        [Test]
        public async Task Index_ModelLinks_MatchesServiceResult()
        {
            var links = new List<LinkItemDto>
            {
                new() { Title = "Privacy", URL = "/legal/privacy" },
                new() { Title = "Terms",   URL = "/legal/terms"   }
            };

            var dto = new LegalPageDto { Links = links };

            _legalPageServiceMock
                .Setup(x => x.GetLegalPageDtoAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(dto);

            var result = await _controller.Index(CancellationToken.None) as ViewResult;
            var model = result!.Model as LegalPageDto;

            Assert.That(model!.Links, Is.SameAs(links));
        }
    }
}
