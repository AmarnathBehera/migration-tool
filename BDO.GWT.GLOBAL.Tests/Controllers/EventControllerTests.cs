using BDO.GWT.Core;
using BDO.GWT.Web.Core.LiveSite.Controllers;
using BDO.GWT.Web.Core.Services;
using BDO.GWT.Web.Core.DTOs;
using BDO.GWT.Web.Core.LiveSite.DTOs.Pages;
using BDO.GWT.Web.Core.DTOs.SchemaMarkups;
using Kentico.Content.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BDO.GWT.GLOBAL.Tests.Controllers
{
    public class EventControllerTests
    {
        private Mock<IEventWebinarPageService> _eventServiceMock = null!;
        private Mock<IContentService> _contentServiceMock = null!;
        private EventController _controller = null!;

        [SetUp]
        public void Setup()
        {
            _eventServiceMock = new Mock<IEventWebinarPageService>();
            _contentServiceMock = new Mock<IContentService>();

            _controller = new EventController(
                _eventServiceMock.Object,
                _contentServiceMock.Object);
        }

        [TearDown]
        public void TearDown() => _controller.Dispose();

        private void SetupContent(Event? page)
        {
            _contentServiceMock
                .Setup(x => x.RetrieveCurrentPageAsync<Event>(
                    It.IsAny<RetrieveCurrentPageParameters>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(page);
        }

        [Test]
        public async Task Index_ReturnsViewResult_WithCorrectViewName()
        {
            var page = new Mock<Event>().Object;
            SetupContent(page);
            _eventServiceMock.Setup(s => s.GetEventPageDtoAsync(page, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new EventWebinarDto());
            _eventServiceMock.Setup(s => s.GetEventSchemaMarkupDto(page, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new EventSchemaMarkupDto());

            var result = await _controller.Index(CancellationToken.None);

            Assert.That(result, Is.InstanceOf<ViewResult>());
            var view = (ViewResult)result;
            Assert.That(view.ViewName, Is.EqualTo("~/Views/EventWebinar/Index.cshtml"));
        }

        [Test]
        public async Task Index_ModelIsTupleOfDtoAndSchema()
        {
            var page = new Mock<Event>().Object;
            var pageDto = new EventWebinarDto();
            var schema = new EventSchemaMarkupDto();

            SetupContent(page);
            _eventServiceMock.Setup(s => s.GetEventPageDtoAsync(page, It.IsAny<CancellationToken>()))
                .ReturnsAsync(pageDto);
            _eventServiceMock.Setup(s => s.GetEventSchemaMarkupDto(page, It.IsAny<CancellationToken>()))
                .ReturnsAsync(schema);

            var view = (ViewResult)await _controller.Index(CancellationToken.None);
            var model = ((EventWebinarDto, EventSchemaMarkupDto))view.Model!;

            Assert.Multiple(() =>
            {
                Assert.That(model.Item1, Is.SameAs(pageDto));
                Assert.That(model.Item2, Is.SameAs(schema));
            });
        }

        [Test]
        public async Task Index_PassesRetrievedPage_ToService()
        {
            var page = new Mock<Event>().Object;
            SetupContent(page);
            _eventServiceMock.Setup(s => s.GetEventPageDtoAsync(page, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new EventWebinarDto());
            _eventServiceMock.Setup(s => s.GetEventSchemaMarkupDto(page, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new EventSchemaMarkupDto());

            await _controller.Index(CancellationToken.None);

            _eventServiceMock.Verify(s => s.GetEventPageDtoAsync(page, It.IsAny<CancellationToken>()), Times.Once);
            _eventServiceMock.Verify(s => s.GetEventSchemaMarkupDto(page, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task Index_ReturnsNotFound_WhenPageIsNull()
        {
            SetupContent(null);

            var result = await _controller.Index(CancellationToken.None);

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
            _eventServiceMock.Verify(
                s => s.GetEventPageDtoAsync(It.IsAny<Event>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public async Task Index_PassesCancellationToken_ToServices()
        {
            using var cts = new CancellationTokenSource();
            var token = cts.Token;
            var page = new Mock<Event>().Object;

            SetupContent(page);
            _eventServiceMock.Setup(s => s.GetEventPageDtoAsync(page, token))
                .ReturnsAsync(new EventWebinarDto());
            _eventServiceMock.Setup(s => s.GetEventSchemaMarkupDto(page, token))
                .ReturnsAsync(new EventSchemaMarkupDto());

            await _controller.Index(token);

            _eventServiceMock.Verify(s => s.GetEventPageDtoAsync(page, token), Times.Once);
            _eventServiceMock.Verify(s => s.GetEventSchemaMarkupDto(page, token), Times.Once);
        }

        [Test]
        public void Index_PropagatesException_FromContentService()
        {
            _contentServiceMock
                .Setup(x => x.RetrieveCurrentPageAsync<Event>(
                    It.IsAny<RetrieveCurrentPageParameters>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("boom"));

            Assert.ThrowsAsync<InvalidOperationException>(
                () => _controller.Index(CancellationToken.None));
        }

        [Test]
        public void Index_PropagatesException_FromEventService()
        {
            var page = new Mock<Event>().Object;
            SetupContent(page);
            _eventServiceMock
                .Setup(s => s.GetEventPageDtoAsync(It.IsAny<Event>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("service down"));

            Assert.ThrowsAsync<InvalidOperationException>(
                () => _controller.Index(CancellationToken.None));
        }
    }
}