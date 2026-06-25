using BDO.GWT.Web.Core.LiveSite.Controllers.API;
using BDO.GWT.Web.Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Moq;
using System.Net.Mime;
using System.Text;

namespace BDO.GWT.GLOBAL.Tests.Controllers
{
    public class OutlookEventControllerTests
    {
        private Mock<IEventWebinarPageService> _serviceMock = null!;
        private Mock<IUrlHelper> _urlHelperMock = null!;
        private OutlookEventController controller = null!;

        private readonly Guid _pageGuid = Guid.Parse("11111111-1111-1111-1111-111111111111");
        private const string SourceUrl = "https://localhost/OutlookEvent/event/11111111-1111-1111-1111-111111111111";

        [SetUp]
        public void SetUp()
        {
            _serviceMock = new Mock<IEventWebinarPageService>();
            _urlHelperMock = new Mock<IUrlHelper>();

            _urlHelperMock
                .Setup(x => x.Action(It.IsAny<UrlActionContext>()))
                .Returns(SourceUrl);

            controller = new OutlookEventController(_serviceMock.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                },
                Url = _urlHelperMock.Object
            };

            controller.ControllerContext.HttpContext.Request.Scheme = "https";
        }

        [Test]
        public async Task DownloadOutlookEvent_ReturnsFileResult_WhenTypeIsEvent()
        {
            // Arrange
            const string card = "BEGIN:VCALENDAR\r\nEVENT CARD\r\nEND:VCALENDAR";

            _serviceMock
                .Setup(s => s.GenerateOutlookEventCardContent(
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(card);

            // Act
            var result = await controller.DownloadOutlookEvent(
                _pageGuid,
                "event",
                CancellationToken.None);

            // Assert
            Assert.That(result, Is.TypeOf<FileContentResult>());
        }

        [Test]
        public async Task DownloadOutlookEvent_ReturnsFileResult_WhenTypeIsWebinar()
        {
            // Arrange
            const string card = "BEGIN:VCALENDAR\r\nWEBINAR CARD\r\nEND:VCALENDAR";

            _serviceMock
                .Setup(s => s.GenerateOutlookWebinarCardContent(
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(card);

            // Act
            var result = await controller.DownloadOutlookEvent(
                _pageGuid,
                "webinar",
                CancellationToken.None);

            // Assert
            Assert.That(result, Is.TypeOf<FileContentResult>());
        }

        [Test]
        public async Task DownloadOutlookEvent_ReturnsFile_WithCorrectContent_WhenTypeIsEvent()
        {
            // Arrange
            const string card = "BEGIN:VCALENDAR\r\nEVENT CARD\r\nEND:VCALENDAR";

            _serviceMock
                .Setup(s => s.GenerateOutlookEventCardContent(
                    _pageGuid,
                    SourceUrl,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(card);

            // Act
            var result = await controller.DownloadOutlookEvent(
                _pageGuid,
                "event",
                CancellationToken.None);

            var fileResult = result as FileContentResult;

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(fileResult, Is.Not.Null);
                Assert.That(fileResult!.ContentType, Is.EqualTo(MediaTypeNames.Application.Octet));
                Assert.That(fileResult.FileDownloadName, Is.EqualTo("Event.ics"));
                Assert.That(fileResult.FileContents, Is.EqualTo(Encoding.Default.GetBytes(card)));
            });
        }

        [Test]
        public async Task DownloadOutlookEvent_ReturnsFile_WithCorrectContent_WhenTypeIsWebinar()
        {
            // Arrange
            const string card = "BEGIN:VCALENDAR\r\nWEBINAR CARD\r\nEND:VCALENDAR";

            _serviceMock
                .Setup(s => s.GenerateOutlookWebinarCardContent(
                    _pageGuid,
                    SourceUrl,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(card);

            // Act
            var result = await controller.DownloadOutlookEvent(
                _pageGuid,
                "webinar",
                CancellationToken.None);

            var fileResult = result as FileContentResult;

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(fileResult, Is.Not.Null);
                Assert.That(fileResult!.ContentType, Is.EqualTo(MediaTypeNames.Application.Octet));
                Assert.That(fileResult.FileDownloadName, Is.EqualTo("Event.ics"));
                Assert.That(fileResult.FileContents, Is.EqualTo(Encoding.Default.GetBytes(card)));
            });
        }

        [Test]
        public async Task DownloadOutlookEvent_CallsEventService_WhenTypeIsEvent()
        {
            // Arrange
            _serviceMock
                .Setup(s => s.GenerateOutlookEventCardContent(
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync("BEGIN:VCALENDAR\r\nEND:VCALENDAR");

            // Act
            await controller.DownloadOutlookEvent(
                _pageGuid,
                "event",
                CancellationToken.None);

            // Assert
            _serviceMock.Verify(
                s => s.GenerateOutlookEventCardContent(
                    _pageGuid,
                    SourceUrl,
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _serviceMock.Verify(
                s => s.GenerateOutlookWebinarCardContent(
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public async Task DownloadOutlookEvent_CallsWebinarService_WhenTypeIsWebinar()
        {
            // Arrange
            _serviceMock
                .Setup(s => s.GenerateOutlookWebinarCardContent(
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync("BEGIN:VCALENDAR\r\nEND:VCALENDAR");

            // Act
            await controller.DownloadOutlookEvent(
                _pageGuid,
                "webinar",
                CancellationToken.None);

            // Assert
            _serviceMock.Verify(
                s => s.GenerateOutlookWebinarCardContent(
                    _pageGuid,
                    SourceUrl,
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _serviceMock.Verify(
                s => s.GenerateOutlookEventCardContent(
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public async Task DownloadOutlookEvent_IsCaseInsensitive_ForEventType()
        {
            // Arrange
            _serviceMock
                .Setup(s => s.GenerateOutlookEventCardContent(
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync("BEGIN:VCALENDAR\r\nEND:VCALENDAR");

            // Act
            var result = await controller.DownloadOutlookEvent(
                _pageGuid,
                "EVENT",
                CancellationToken.None);

            // Assert
            Assert.That(result, Is.TypeOf<FileContentResult>());

            _serviceMock.Verify(
                s => s.GenerateOutlookEventCardContent(
                    _pageGuid,
                    SourceUrl,
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task DownloadOutlookEvent_IsCaseInsensitive_ForWebinarType()
        {
            // Arrange
            _serviceMock
                .Setup(s => s.GenerateOutlookWebinarCardContent(
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync("BEGIN:VCALENDAR\r\nEND:VCALENDAR");

            // Act
            var result = await controller.DownloadOutlookEvent(
                _pageGuid,
                "WEBINAR",
                CancellationToken.None);

            // Assert
            Assert.That(result, Is.TypeOf<FileContentResult>());

            _serviceMock.Verify(
                s => s.GenerateOutlookWebinarCardContent(
                    _pageGuid,
                    SourceUrl,
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task DownloadOutlookEvent_ReturnsNotFound_WhenEventCardIsNull()
        {
            // Arrange
            _serviceMock
                .Setup(s => s.GenerateOutlookEventCardContent(
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((string?)null);

            // Act
            var result = await controller.DownloadOutlookEvent(
                _pageGuid,
                "event",
                CancellationToken.None);

            // Assert
            Assert.That(result, Is.TypeOf<NotFoundResult>());
        }

        [Test]
        public async Task DownloadOutlookEvent_ReturnsNotFound_WhenWebinarCardIsNull()
        {
            // Arrange
            _serviceMock
                .Setup(s => s.GenerateOutlookWebinarCardContent(
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((string?)null);

            // Act
            var result = await controller.DownloadOutlookEvent(
                _pageGuid,
                "webinar",
                CancellationToken.None);

            // Assert
            Assert.That(result, Is.TypeOf<NotFoundResult>());
        }

        [Test]
        public void DownloadOutlookEvent_ThrowsArgumentException_WhenTypeIsInvalid()
        {
            // Act
            var exception = Assert.ThrowsAsync<ArgumentException>(() =>
                controller.DownloadOutlookEvent(
                    _pageGuid,
                    "invalid",
                    CancellationToken.None));

            // Assert
            Assert.That(exception, Is.Not.Null);
            Assert.That(exception!.Message, Is.EqualTo("invalid type not found"));

            _serviceMock.Verify(
                s => s.GenerateOutlookEventCardContent(
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);

            _serviceMock.Verify(
                s => s.GenerateOutlookWebinarCardContent(
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public async Task DownloadOutlookEvent_PassesCancellationToken_ToEventService()
        {
            // Arrange
            CancellationToken capturedToken = default;

            _serviceMock
                .Setup(s => s.GenerateOutlookEventCardContent(
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .Callback<Guid, string, CancellationToken>((_, _, ct) => capturedToken = ct)
                .ReturnsAsync("BEGIN:VCALENDAR\r\nEND:VCALENDAR");

            using var cts = new CancellationTokenSource();

            // Act
            await controller.DownloadOutlookEvent(
                _pageGuid,
                "event",
                cts.Token);

            // Assert
            Assert.That(capturedToken, Is.EqualTo(cts.Token));
        }

        [Test]
        public async Task DownloadOutlookEvent_PassesPageGuid_ToEventService()
        {
            // Arrange
            Guid capturedGuid = Guid.Empty;

            _serviceMock
                .Setup(s => s.GenerateOutlookEventCardContent(
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .Callback<Guid, string, CancellationToken>((guid, _, _) => capturedGuid = guid)
                .ReturnsAsync("BEGIN:VCALENDAR\r\nEND:VCALENDAR");

            // Act
            await controller.DownloadOutlookEvent(
                _pageGuid,
                "event",
                CancellationToken.None);

            // Assert
            Assert.That(capturedGuid, Is.EqualTo(_pageGuid));
        }

        [Test]
        public async Task DownloadOutlookEvent_PassesGeneratedSource_ToEventService()
        {
            // Arrange
            string? capturedSource = null;

            _serviceMock
                .Setup(s => s.GenerateOutlookEventCardContent(
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .Callback<Guid, string, CancellationToken>((_, source, _) => capturedSource = source)
                .ReturnsAsync("BEGIN:VCALENDAR\r\nEND:VCALENDAR");

            // Act
            await controller.DownloadOutlookEvent(
                _pageGuid,
                "event",
                CancellationToken.None);

            // Assert
            Assert.That(capturedSource, Is.EqualTo(SourceUrl));
        }

        [Test]
        public async Task DownloadOutlookEvent_UsesUrlAction_ToGenerateSource()
        {
            // Arrange
            _serviceMock
                .Setup(s => s.GenerateOutlookEventCardContent(
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync("BEGIN:VCALENDAR\r\nEND:VCALENDAR");

            // Act
            await controller.DownloadOutlookEvent(
                _pageGuid,
                "event",
                CancellationToken.None);

            // Assert
            _urlHelperMock.Verify(
                x => x.Action(It.IsAny<UrlActionContext>()),
                Times.Once);
        }
    }
}