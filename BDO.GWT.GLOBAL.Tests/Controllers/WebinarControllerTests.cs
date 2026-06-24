using BDO.GWT.Core;
using BDO.GWT.Web.Core.DTOs.SchemaMarkups;
using BDO.GWT.Web.Core.LiveSite.Controllers;
using BDO.GWT.Web.Core.LiveSite.DTOs.Pages;
using BDO.GWT.Web.Core.Services;
using CMS.Websites.Routing;
using Kentico.Content.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BDO.GWT.GLOBAL.Tests.Controllers
{
    public class WebinarControllerTests
    {
        private Mock<IEventWebinarPageService> _eventServiceMock = null!;
        private Mock<IContentService> _contentServiceMock = null!;
        private Mock<IWebsiteChannelContext> _channelMock = null!;
        private WebinarController _controller = null!;

        [SetUp]
        public void Setup()
        {
            _eventServiceMock = new Mock<IEventWebinarPageService>();
            _contentServiceMock = new Mock<IContentService>();
            _channelMock = new Mock<IWebsiteChannelContext>();
            _channelMock.Setup(x => x.WebsiteChannelID).Returns(1);

            _controller = new WebinarController(
                _eventServiceMock.Object,
                _channelMock.Object,
                _contentServiceMock.Object);
        }

        [TearDown]
        public void TearDown() => _controller.Dispose();

        // Matches what the controller actually calls
        private void SetupContent(Webinar? page)
        {
            _contentServiceMock
                .Setup(x => x.RetrieveCurrentPageAsync<Webinar>(
                    It.IsAny<RetrieveCurrentPageParameters>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(page);
        }

        [Test]
        public async Task Index_ReturnsViewResult_WithCorrectViewName()
        {
            var page = new Mock<Webinar>().Object;
            SetupContent(page);
            _eventServiceMock
                .Setup(s => s.GetWebinarPageDtoAsync(page, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new EventWebinarDto());
            _eventServiceMock
                .Setup(s => s.GetWebinarSchemaMarkupDto(page, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new EventSchemaMarkupDto());

            var result = await _controller.Index(CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.InstanceOf<ViewResult>());
                Assert.That(((ViewResult)result).ViewName, Is.EqualTo("~/Views/EventWebinar/Index.cshtml"));
            });
        }

        [Test]
        public async Task Index_ModelIsTupleOfDtoAndSchema()
        {
            var page = new Mock<Webinar>().Object;
            var pageDto = new EventWebinarDto();
            var schema = new EventSchemaMarkupDto();

            SetupContent(page);
            _eventServiceMock
                .Setup(s => s.GetWebinarPageDtoAsync(page, It.IsAny<CancellationToken>()))
                .ReturnsAsync(pageDto);
            _eventServiceMock
                .Setup(s => s.GetWebinarSchemaMarkupDto(page, It.IsAny<CancellationToken>()))
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
            var page = new Mock<Webinar>().Object;
            SetupContent(page);
            _eventServiceMock
                .Setup(s => s.GetWebinarPageDtoAsync(page, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new EventWebinarDto());
            _eventServiceMock
                .Setup(s => s.GetWebinarSchemaMarkupDto(page, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new EventSchemaMarkupDto());

            await _controller.Index(CancellationToken.None);

            _eventServiceMock.Verify(
                s => s.GetWebinarPageDtoAsync(page, It.IsAny<CancellationToken>()), Times.Once);
            _eventServiceMock.Verify(
                s => s.GetWebinarSchemaMarkupDto(page, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task Index_ReturnsNotFound_WhenPageIsNull()
        {
            SetupContent(null);

            var result = await _controller.Index(CancellationToken.None);

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
            _eventServiceMock.Verify(
                s => s.GetWebinarPageDtoAsync(It.IsAny<Webinar>(), It.IsAny<CancellationToken>()),
                Times.Never);
            _eventServiceMock.Verify(
                s => s.GetWebinarSchemaMarkupDto(It.IsAny<Webinar>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public async Task Index_PassesCancellationToken_ToServices()
        {
            using var cts = new CancellationTokenSource();
            var token = cts.Token;
            var page = new Mock<Webinar>().Object;

            SetupContent(page);
            _eventServiceMock
                .Setup(s => s.GetWebinarPageDtoAsync(page, token))
                .ReturnsAsync(new EventWebinarDto());
            _eventServiceMock
                .Setup(s => s.GetWebinarSchemaMarkupDto(page, token))
                .ReturnsAsync(new EventSchemaMarkupDto());

            await _controller.Index(token);

            _eventServiceMock.Verify(
                s => s.GetWebinarPageDtoAsync(page, token), Times.Once);
            _eventServiceMock.Verify(
                s => s.GetWebinarSchemaMarkupDto(page, token), Times.Once);
        }

        [Test]
        public void Index_PropagatesException_FromContentService()
        {
            _contentServiceMock
                .Setup(x => x.RetrieveCurrentPageAsync<Webinar>(
                    It.IsAny<RetrieveCurrentPageParameters>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("boom"));

            Assert.ThrowsAsync<InvalidOperationException>(
                () => _controller.Index(CancellationToken.None));
        }

        [Test]
        public void Index_PropagatesException_FromEventService()
        {
            var page = new Mock<Webinar>().Object;
            SetupContent(page);
            _eventServiceMock
                .Setup(s => s.GetWebinarPageDtoAsync(It.IsAny<Webinar>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("service down"));

            Assert.ThrowsAsync<InvalidOperationException>(
                () => _controller.Index(CancellationToken.None));
        }
    }
}