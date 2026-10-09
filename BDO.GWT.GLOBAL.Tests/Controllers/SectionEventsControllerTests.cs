using BDO;
using BDO.GWT.Core;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Pages;
using BDO.GWT.Core.Services;
using BDO.GWT.Web.Core.DTOs;
using GWTXbyKWeb.Controllers;
using Kentico.Content.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BDO.GWT.GLOBAL.Tests.Controllers
{
    public class SectionEventsControllerTests
    {
        private Mock<IContentService> _contentServiceMock = null!;
        private Mock<ISectionEventsService> _sectionEventsServiceMock = null!;
        private SectionEventsController _controller = null!;

        [SetUp]
        public void Setup()
        {
            _contentServiceMock = new Mock<IContentService>();
            _sectionEventsServiceMock = new Mock<ISectionEventsService>();
            _controller = new SectionEventsController(
                _contentServiceMock.Object,
                _sectionEventsServiceMock.Object);
        }

        [TearDown]
        public void TearDown() => _controller.Dispose();

        [Test]
        public async Task Index_ReturnsNotFound_WhenCurrentPageIsNull()
        {
            _contentServiceMock
                .Setup(s => s.RetrieveCurrentPageAsync<SectionEvents>(
                    It.IsAny<RetrieveCurrentPageParameters>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((SectionEvents?)null);

            var result = await _controller.Index(CancellationToken.None);

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
            _sectionEventsServiceMock.Verify(
                s => s.GetEventsSectionDtoAsync(It.IsAny<SectionEvents>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public async Task Index_ReturnsViewResult_WithSectionEventsDto()
        {
            var page = new SectionEvents();
            var dto = new SectionEventsDto
            {
                Hero = new HeroDto { Title = "Events" },
                CardsGrid = new CardsGridDto { FilteringTooltipEnabled = true }
            };

            _contentServiceMock
                .Setup(s => s.RetrieveCurrentPageAsync<SectionEvents>(
                    It.IsAny<RetrieveCurrentPageParameters>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(page);

            _sectionEventsServiceMock
                .Setup(s => s.GetEventsSectionDtoAsync(page, It.IsAny<CancellationToken>()))
                .ReturnsAsync(dto);

            var result = await _controller.Index(CancellationToken.None);

            Assert.That(result, Is.InstanceOf<ViewResult>());
            var view = (ViewResult)result;
            Assert.Multiple(() =>
            {
                Assert.That(view.ViewName, Is.EqualTo("~/Views/SectionEvents/Index.cshtml"));
                Assert.That(view.Model, Is.SameAs(dto));
            });
        }

        [Test]
        public async Task Index_PassesCancellationToken_ToServices()
        {
            using var cts = new CancellationTokenSource();
            var token = cts.Token;

            var page = new SectionEvents();
            _contentServiceMock
                .Setup(s => s.RetrieveCurrentPageAsync<SectionEvents>(
                    It.IsAny<RetrieveCurrentPageParameters>(), token))
                .ReturnsAsync(page);

            _sectionEventsServiceMock
                .Setup(s => s.GetEventsSectionDtoAsync(page, token))
                .ReturnsAsync(new SectionEventsDto());

            await _controller.Index(token);

            _sectionEventsServiceMock.Verify(s => s.GetEventsSectionDtoAsync(page, token), Times.Once);
        }

        [Test]
        public void Index_PropagatesException_FromService()
        {
            var page = new SectionEvents();
            _contentServiceMock
                .Setup(s => s.RetrieveCurrentPageAsync<SectionEvents>(
                    It.IsAny<RetrieveCurrentPageParameters>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(page);

            _sectionEventsServiceMock
                .Setup(s => s.GetEventsSectionDtoAsync(page, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("boom"));

            Assert.ThrowsAsync<InvalidOperationException>(
                () => _controller.Index(CancellationToken.None));
        }
    }
}
