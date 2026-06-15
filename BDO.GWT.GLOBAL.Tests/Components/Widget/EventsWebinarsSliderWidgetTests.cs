using BDO.GWT.Core.Data;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.Services;
using BDO.GWT.Xperience.Core.LiveSite.Components.Widgets.EventsWebinarsSliderWidget;
using CMS.Websites;
using Kentico.Content.Web.Mvc;
using Kentico.PageBuilder.Web.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;

namespace BDO.GWT.GLOBAL.Tests.Components.Widget
{
    public class EventsWebinarsSliderWidgetTests
    {
        private Mock<IEventsWebinarsSliderService> _serviceMock = null!;
        private Mock<IPageContext> _pageContextMock = null!;
        private Mock<IContentRetriever> _contentRetrieverMock = null!;
        private EventsWebinarsSliderWidget _widget = null!;

        [SetUp]
        public void SetUp()
        {
            _serviceMock = new Mock<IEventsWebinarsSliderService>();
            _pageContextMock = new Mock<IPageContext>();
            _contentRetrieverMock = new Mock<IContentRetriever>();

            _widget = new EventsWebinarsSliderWidget(
                _serviceMock.Object,
                _pageContextMock.Object,
                _contentRetrieverMock.Object);

            _widget.ViewComponentContext = new ViewComponentContext
            {
                ViewContext = new ViewContext
                {
                    HttpContext = new DefaultHttpContext(),
                    ViewData = new ViewDataDictionary(
                        new EmptyModelMetadataProvider(),
                        new ModelStateDictionary())
                }
            };
        }

        private static ComponentViewModel<EventsWebinarsSliderWidgetProperties> CreateModel(
            EventsWebinarsSliderWidgetProperties properties)
        {
            var model = new ComponentViewModel<EventsWebinarsSliderWidgetProperties>();

            typeof(ComponentViewModel<EventsWebinarsSliderWidgetProperties>)
                .GetProperty("Properties")
                ?.SetValue(model, properties);

            return model;
        }

        private static EventsWebinarsSliderDto CreateDto()
        {
            return new EventsWebinarsSliderDto
            {
                FilteringTooltipEnabled = false,
                Items = Enumerable.Empty<EventsWebinarsCardDto>(),
                TopMargin = string.Empty,
                BottomMargin = string.Empty
            };
        }

        [Test]
        public async Task InvokeAsync_WhenCurrentPageIsNull_ReturnsEmptyView()
        {
            _pageContextMock
                .Setup(x => x.GetCurrentPageAsync<IWebPageFieldsSource>(It.IsAny<CancellationToken>()))
                .ReturnsAsync((IWebPageFieldsSource?)null);

            var model = CreateModel(new EventsWebinarsSliderWidgetProperties());

            var result = await _widget.InvokeAsync(model);

            var viewResult = result as ViewViewComponentResult;

            Assert.That(viewResult, Is.Not.Null);

            Assert.Multiple(() =>
            {
                Assert.That(
                    viewResult!.ViewName,
                    Is.EqualTo("~/Components/Widget/EventsWebinarsSliderWidget/_EventsWebinarsSliderWidget.cshtml"));

                Assert.That(viewResult.ViewData?.Model, Is.Null);
            });

            _serviceMock.Verify(
                x => x.GetEventsWebinarsSliderAsync(
                    It.IsAny<IWebPageFieldsSource>(),
                    It.IsAny<EventsWebinarsSliderData>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public async Task InvokeAsync_WithAllProperties_MapsPropertiesToDataCorrectly()
        {
            var currentPageMock = new Mock<IWebPageFieldsSource>();

            _pageContextMock
                .Setup(x => x.GetCurrentPageAsync<IWebPageFieldsSource>(It.IsAny<CancellationToken>()))
                .ReturnsAsync(currentPageMock.Object);

            EventsWebinarsSliderData? capturedData = null;

            var expectedDto = CreateDto();

            _serviceMock
                .Setup(x => x.GetEventsWebinarsSliderAsync(
                    currentPageMock.Object,
                    It.IsAny<EventsWebinarsSliderData>(),
                    It.IsAny<CancellationToken>()))
                .Callback<IWebPageFieldsSource, EventsWebinarsSliderData, CancellationToken>(
                    (_, data, _) => capturedData = data)
                .ReturnsAsync(expectedDto);

            var properties = new EventsWebinarsSliderWidgetProperties
            {
                Title = "Test Title",
                Description = "Test Description",
                FilteringEnabled = true,
                Filters = new List<string> { "Service", "Industry" },
                BackgroundImageAltText = "Alt text",
                SeeAllLinkText = "See all",
                SeeAllLinkURL = "/events",
                SeeAllLinkURLTarget = "_blank",
                TopMargin = "20px",
                BottomMargin = "40px",
                WidgetID = "events-widget-1"
            };

            var model = CreateModel(properties);

            var result = await _widget.InvokeAsync(model);

            var viewResult = result as ViewViewComponentResult;

            Assert.That(viewResult, Is.Not.Null);
            Assert.That(capturedData, Is.Not.Null);

            Assert.Multiple(() =>
            {
                Assert.That(
                    viewResult!.ViewName,
                    Is.EqualTo("~/Components/Widget/EventsWebinarsSliderWidget/_EventsWebinarsSliderWidget.cshtml"));

                Assert.That(viewResult.ViewData?.Model, Is.SameAs(expectedDto));

                Assert.That(capturedData!.Title, Is.EqualTo("Test Title"));
                Assert.That(capturedData.Description, Is.EqualTo("Test Description"));
                Assert.That(capturedData.FilteringEnabled, Is.True);
                Assert.That(capturedData.Filters, Is.EquivalentTo(new[] { "Service", "Industry" }));
                Assert.That(capturedData.BackgroundImageAltText, Is.EqualTo("Alt text"));
                Assert.That(capturedData.SeeAllLinkText, Is.EqualTo("See all"));
                Assert.That(capturedData.SeeAllLinkUrl, Is.EqualTo("/events"));
                Assert.That(capturedData.SeeAllLinkURLTarget, Is.EqualTo("_blank"));
                Assert.That(capturedData.TopMargin, Is.EqualTo("20px"));
                Assert.That(capturedData.BottomMargin, Is.EqualTo("40px"));
                Assert.That(capturedData.WidgetID, Is.EqualTo("events-widget-1"));
            });
        }

        [Test]
        public async Task InvokeAsync_WithEmptyBackgroundAltText_SetsAltTextToNull()
        {
            var currentPageMock = new Mock<IWebPageFieldsSource>();

            _pageContextMock
                .Setup(x => x.GetCurrentPageAsync<IWebPageFieldsSource>(It.IsAny<CancellationToken>()))
                .ReturnsAsync(currentPageMock.Object);

            EventsWebinarsSliderData? capturedData = null;

            _serviceMock
                .Setup(x => x.GetEventsWebinarsSliderAsync(
                    It.IsAny<IWebPageFieldsSource>(),
                    It.IsAny<EventsWebinarsSliderData>(),
                    It.IsAny<CancellationToken>()))
                .Callback<IWebPageFieldsSource, EventsWebinarsSliderData, CancellationToken>(
                    (_, data, _) => capturedData = data)
                .ReturnsAsync(CreateDto());

            var model = CreateModel(new EventsWebinarsSliderWidgetProperties
            {
                Title = "Title",
                BackgroundImageAltText = string.Empty
            });

            await _widget.InvokeAsync(model);

            Assert.That(capturedData, Is.Not.Null);
            Assert.That(capturedData!.BackgroundImageAltText, Is.Null);
        }

        [Test]
        public async Task InvokeAsync_WithNullFilters_SetsEmptyFilterCollection()
        {
            var currentPageMock = new Mock<IWebPageFieldsSource>();

            _pageContextMock
                .Setup(x => x.GetCurrentPageAsync<IWebPageFieldsSource>(It.IsAny<CancellationToken>()))
                .ReturnsAsync(currentPageMock.Object);

            EventsWebinarsSliderData? capturedData = null;

            _serviceMock
                .Setup(x => x.GetEventsWebinarsSliderAsync(
                    It.IsAny<IWebPageFieldsSource>(),
                    It.IsAny<EventsWebinarsSliderData>(),
                    It.IsAny<CancellationToken>()))
                .Callback<IWebPageFieldsSource, EventsWebinarsSliderData, CancellationToken>(
                    (_, data, _) => capturedData = data)
                .ReturnsAsync(CreateDto());

            var model = CreateModel(new EventsWebinarsSliderWidgetProperties
            {
                Title = "Title",
                Filters = null
            });

            await _widget.InvokeAsync(model);

            Assert.That(capturedData, Is.Not.Null);
            Assert.That(capturedData!.Filters, Is.Empty);
        }

        [Test]
        public async Task InvokeAsync_ReturnsViewViewComponentResult()
        {
            var currentPageMock = new Mock<IWebPageFieldsSource>();

            _pageContextMock
                .Setup(x => x.GetCurrentPageAsync<IWebPageFieldsSource>(It.IsAny<CancellationToken>()))
                .ReturnsAsync(currentPageMock.Object);

            _serviceMock
                .Setup(x => x.GetEventsWebinarsSliderAsync(
                    It.IsAny<IWebPageFieldsSource>(),
                    It.IsAny<EventsWebinarsSliderData>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(CreateDto());

            var model = CreateModel(new EventsWebinarsSliderWidgetProperties
            {
                Title = "Heading"
            });

            var result = await _widget.InvokeAsync(model);

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.TypeOf<ViewViewComponentResult>());
        }
    }
}