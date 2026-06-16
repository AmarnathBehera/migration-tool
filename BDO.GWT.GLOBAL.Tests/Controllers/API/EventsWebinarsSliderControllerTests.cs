using BDO.GWT.Core.Constants;
using BDO.GWT.Core.Controllers.API;
using BDO.GWT.Core.Data.Queries;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Cards;
using BDO.GWT.Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BDO.GWT.GLOBAL.Tests.Controllers
{
    public class EventsWebinarsSliderControllerTests
    {
        private Mock<IEventsWebinarsSliderService> _serviceMock = null!;
        private EventsWebinarsSliderController _controller = null!;

        [SetUp]
        public void SetUp()
        {
            _serviceMock = new Mock<IEventsWebinarsSliderService>();

            _controller = new EventsWebinarsSliderController(_serviceMock.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };
        }

        [Test]
        public async Task Items_ReturnsOkResult()
        {
            // Arrange
            _serviceMock
                .Setup(s => s.GetAPIFilteredItemsAsync(
                    It.IsAny<CardsQuery>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<EventsWebinarsCardDto>());

            // Act
            var result = await _controller.Items(new CardsQuery(), CancellationToken.None);

            // Assert
            Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
        }

        [Test]
        public async Task Items_ReturnsItems_FromService()
        {
            // Arrange
            var expected = new List<EventsWebinarsCardDto>
            {
                new EventsWebinarsCardDto { Title = "Event 1" },
                new EventsWebinarsCardDto { Title = "Event 2" }
            };

            _serviceMock
                .Setup(s => s.GetAPIFilteredItemsAsync(
                    It.IsAny<CardsQuery>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            // Act
            var result = await _controller.Items(new CardsQuery(), CancellationToken.None);
            var ok = result.Result as OkObjectResult;

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(ok, Is.Not.Null);
                Assert.That(ok!.Value, Is.EqualTo(expected));
            });
        }

        [Test]
        public async Task Items_AddsShowOngoingEventsFilter()
        {
            // Arrange
            CardsQuery? capturedQuery = null;

            _serviceMock
                .Setup(s => s.GetAPIFilteredItemsAsync(
                    It.IsAny<CardsQuery>(),
                    It.IsAny<CancellationToken>()))
                .Callback<CardsQuery, CancellationToken>((q, _) => capturedQuery = q)
                .ReturnsAsync(new List<EventsWebinarsCardDto>());

            // Act
            await _controller.Items(new CardsQuery(), CancellationToken.None);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(capturedQuery, Is.Not.Null);
                Assert.That(
                    capturedQuery!.Filters,
                    Contains.Key(FilterParameterName.ShowOngoingEvents));
                Assert.That(
                    capturedQuery.Filters[FilterParameterName.ShowOngoingEvents],
                    Is.EqualTo(bool.TrueString));
            });
        }

        [Test]
        public async Task Items_CallsService_Once()
        {
            // Arrange
            _serviceMock
                .Setup(s => s.GetAPIFilteredItemsAsync(
                    It.IsAny<CardsQuery>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<EventsWebinarsCardDto>());

            // Act
            await _controller.Items(new CardsQuery(), CancellationToken.None);

            // Assert
            _serviceMock.Verify(
                s => s.GetAPIFilteredItemsAsync(
                    It.IsAny<CardsQuery>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task Items_PassesQueryFromRequest_ToService()
        {
            // Arrange
            CardsQuery? capturedQuery = null;

            _serviceMock
                .Setup(s => s.GetAPIFilteredItemsAsync(
                    It.IsAny<CardsQuery>(),
                    It.IsAny<CancellationToken>()))
                .Callback<CardsQuery, CancellationToken>((q, _) => capturedQuery = q)
                .ReturnsAsync(new List<EventsWebinarsCardDto>());

            var inputQuery = new CardsQuery
            {
                CurrentPage = 2,
                PageSize = 5
            };

            // Act
            await _controller.Items(inputQuery, CancellationToken.None);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(capturedQuery, Is.Not.Null);
                Assert.That(capturedQuery!.CurrentPage, Is.EqualTo(2));
                Assert.That(capturedQuery.PageSize, Is.EqualTo(5));
            });
        }

        [Test]
        public async Task Items_WithEmptyServiceResult_ReturnsOkWithEmptyCollection()
        {
            // Arrange
            _serviceMock
                .Setup(s => s.GetAPIFilteredItemsAsync(
                    It.IsAny<CardsQuery>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<EventsWebinarsCardDto>());

            // Act
            var result = await _controller.Items(new CardsQuery(), CancellationToken.None);
            var ok = result.Result as OkObjectResult;
            var items = ok!.Value as IEnumerable<EventsWebinarsCardDto>;

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(items, Is.Not.Null);
                Assert.That(items, Is.Empty);
            });
        }

        [Test]
        public async Task Items_PassesCancellationToken_ToService()
        {
            // Arrange
            CancellationToken capturedToken = default;

            _serviceMock
                .Setup(s => s.GetAPIFilteredItemsAsync(
                    It.IsAny<CardsQuery>(),
                    It.IsAny<CancellationToken>()))
                .Callback<CardsQuery, CancellationToken>((_, ct) => capturedToken = ct)
                .ReturnsAsync(new List<EventsWebinarsCardDto>());

            using var cts = new CancellationTokenSource();

            // Act
            await _controller.Items(new CardsQuery(), cts.Token);

            // Assert
            Assert.That(capturedToken, Is.EqualTo(cts.Token));
        }
    }
}