using BDO.GWT.Core.Constants;
using BDO.GWT.Core.Data.Queries;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.Services;
using BDO.GWT.Web.Core.LiveSite.Controllers.API;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BDO.GWT.GLOBAL.Tests.Controllers
{
    public class EventsSectionPageControllerTests
    {
        private Mock<ISectionEventsService> _serviceMock = null!;
        private EventsSectionPageController _controller = null!;

        [SetUp]
        public void Setup()
        {
            _serviceMock = new Mock<ISectionEventsService>();
            _controller = new EventsSectionPageController(_serviceMock.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };
        }

        [TearDown]
        public void TearDown() { }

        [Test]
        public async Task Items_ReturnsOkResult_WithPaginationDto()
        {
            var expected = new PaginationDto<EventsWebinarsCardDto>
            {
                Data = new[] { new EventsWebinarsCardDto { Title = "T" } },
                TotalCount = 1,
            };

            _serviceMock
                .Setup(s => s.GetFilteredItemsAsync(It.IsAny<CardsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            var result = await _controller.Items(new CardsQuery { Filters = new Dictionary<string, string>() }, CancellationToken.None);

            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            var ok = (OkObjectResult)result.Result!;
            Assert.That(ok.Value, Is.SameAs(expected));
        }


        [Test]
        public async Task Items_AddsShowOngoingEventsFilter()
        {
            CardsQuery? captured = null;
            _serviceMock
                .Setup(s => s.GetFilteredItemsAsync(It.IsAny<CardsQuery>(), It.IsAny<CancellationToken>()))
                .Callback<CardsQuery, CancellationToken>((q, _) => captured = q)
                .ReturnsAsync(new PaginationDto<EventsWebinarsCardDto>());

            await _controller.Items(new CardsQuery { Filters = new Dictionary<string, string>() }, CancellationToken.None);

            Assert.That(captured, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(captured!.Filters.ContainsKey(FilterParameterName.ShowOngoingEvents), Is.True);
                Assert.That(captured.Filters[FilterParameterName.ShowOngoingEvents], Is.EqualTo(bool.TrueString));
            });
        }

        [Test]
        public async Task Items_PassesCancellationToken()
        {
            using var cts = new CancellationTokenSource();
            var token = cts.Token;

            _serviceMock
                .Setup(s => s.GetFilteredItemsAsync(It.IsAny<CardsQuery>(), token))
                .ReturnsAsync(new PaginationDto<EventsWebinarsCardDto>());

            await _controller.Items(new CardsQuery { Filters = new Dictionary<string, string>() }, token);

            _serviceMock.Verify(s => s.GetFilteredItemsAsync(It.IsAny<CardsQuery>(), token), Times.Once);
        }

        [Test]
        public void Items_PropagatesException_FromService()
        {
            _serviceMock
                .Setup(s => s.GetFilteredItemsAsync(It.IsAny<CardsQuery>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("boom"));

            Assert.ThrowsAsync<InvalidOperationException>(() =>
                _controller.Items(new CardsQuery { Filters = new Dictionary<string, string>() }, CancellationToken.None));
        }
    }
}
