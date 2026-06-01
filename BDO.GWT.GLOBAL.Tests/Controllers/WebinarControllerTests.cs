using BDO;
using BDO.GWT.Core;
using BDO.GWT.Web.Core.LiveSite.Controllers;
using BDO.GWT.Web.Core.Services;
using BDO.GWT.Web.Core.LiveSite.DTOs.Pages;
using BDO.GWT.Web.Core.DTOs.SchemaMarkups;
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

        private void SetupContent(IEnumerable<Webinar>? pages)
        {
            _contentServiceMock
                .Setup(x => x.GetPagesAsync<Webinar>(
                    It.IsAny<RetrievePagesParameters>(),
                    It.IsAny<Action<RetrievePagesQueryParameters>>(),
                    It.IsAny<RetrievalCacheSettings>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(pages!);
        }

        [Test]
        public async Task Index_ReturnsViewResult_WithCorrectViewName()
        {
            SetupContent(new List<Webinar> { new Webinar() });
            _eventServiceMock.Setup(s => s.GetWebinarPageDtoAsync(It.IsAny<Webinar>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new EventWebinarDto());
            _eventServiceMock.Setup(s => s.GetWebinarSchemaMarkupDto(It.IsAny<Webinar>(), It.IsAny<CancellationToken>()))
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
            var page = new Webinar();
            var pageDto = new EventWebinarDto();
            var schema = new EventSchemaMarkupDto();

            SetupContent(new List<Webinar> { page });
            _eventServiceMock.Setup(s => s.GetWebinarPageDtoAsync(page, It.IsAny<CancellationToken>()))
                .ReturnsAsync(pageDto);
            _eventServiceMock.Setup(s => s.GetWebinarSchemaMarkupDto(page, It.IsAny<CancellationToken>()))
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
        public async Task Index_PassesFirstPage_ToService()
        {
            var first = new Webinar();
            var second = new Webinar();
            SetupContent(new List<Webinar> { first, second });

            _eventServiceMock.Setup(s => s.GetWebinarPageDtoAsync(It.IsAny<Webinar>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new EventWebinarDto());
            _eventServiceMock.Setup(s => s.GetWebinarSchemaMarkupDto(It.IsAny<Webinar>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new EventSchemaMarkupDto());

            await _controller.Index(CancellationToken.None);

            _eventServiceMock.Verify(s => s.GetWebinarPageDtoAsync(first, It.IsAny<CancellationToken>()), Times.Once);
            _eventServiceMock.Verify(s => s.GetWebinarSchemaMarkupDto(first, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task Index_PassesNullPage_WhenNoResults()
        {
            SetupContent(new List<Webinar>());
            _eventServiceMock.Setup(s => s.GetWebinarPageDtoAsync(null!, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new EventWebinarDto());
            _eventServiceMock.Setup(s => s.GetWebinarSchemaMarkupDto(null!, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new EventSchemaMarkupDto());

            var result = await _controller.Index(CancellationToken.None);

            Assert.That(result, Is.InstanceOf<ViewResult>());
            _eventServiceMock.Verify(s => s.GetWebinarPageDtoAsync(null!, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task Index_PassesCancellationToken_ToServices()
        {
            using var cts = new CancellationTokenSource();
            var token = cts.Token;

            SetupContent(new List<Webinar> { new Webinar() });
            _eventServiceMock.Setup(s => s.GetWebinarPageDtoAsync(It.IsAny<Webinar>(), token))
                .ReturnsAsync(new EventWebinarDto());
            _eventServiceMock.Setup(s => s.GetWebinarSchemaMarkupDto(It.IsAny<Webinar>(), token))
                .ReturnsAsync(new EventSchemaMarkupDto());

            await _controller.Index(token);

            _eventServiceMock.Verify(s => s.GetWebinarPageDtoAsync(It.IsAny<Webinar>(), token), Times.Once);
            _eventServiceMock.Verify(s => s.GetWebinarSchemaMarkupDto(It.IsAny<Webinar>(), token), Times.Once);
        }

        [Test]
        public void Index_PropagatesException_FromContentService()
        {
            _contentServiceMock
                .Setup(x => x.GetPagesAsync<Webinar>(
                    It.IsAny<RetrievePagesParameters>(),
                    It.IsAny<Action<RetrievePagesQueryParameters>>(),
                    It.IsAny<RetrievalCacheSettings>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("boom"));

            Assert.ThrowsAsync<InvalidOperationException>(() => _controller.Index(CancellationToken.None));
        }
    }
}
