using BDO.GWT.Core;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Cards;
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
using System.Reflection;

namespace BDO.GWT.GLOBAL.Tests.Components.Widget
{
    public class EventsWebinarsSliderWidgetTests
    {
        private static readonly string[] ExpectedFilters =
        [
            "Service",
            "Industry"
        ];

        private Mock<IEventsWebinarsSliderService> _serviceMock = null!;
        private Mock<IContentService> _contentServiceMock = null!;
        private Mock<IContentRetriever> _contentRetrieverMock = null!;
        private EventsWebinarsSliderWidget _widget = null!;
        private DefaultHttpContext _httpContext = null!;

        [SetUp]
        public void SetUp()
        {
            _serviceMock = new Mock<IEventsWebinarsSliderService>();
            _contentServiceMock = new Mock<IContentService>();
            _contentRetrieverMock = new Mock<IContentRetriever>();

            _widget = CreateWidget();

            _httpContext = new DefaultHttpContext();
            _httpContext.RequestAborted = CancellationToken.None;

            _widget.ViewComponentContext = new ViewComponentContext
            {
                ViewContext = new ViewContext
                {
                    HttpContext = _httpContext,
                    ViewData = new ViewDataDictionary(
                        new EmptyModelMetadataProvider(),
                        new ModelStateDictionary())
                }
            };
        }

        private EventsWebinarsSliderWidget CreateWidget()
        {
            var ctor = typeof(EventsWebinarsSliderWidget)
                .GetConstructors(BindingFlags.Public | BindingFlags.Instance)
                .OrderByDescending(c => c.GetParameters().Length)
                .First();

            var args = ctor.GetParameters()
                .Select(p => ResolveConstructorArgument(p.ParameterType))
                .ToArray();

            return (EventsWebinarsSliderWidget)Activator.CreateInstance(typeof(EventsWebinarsSliderWidget), args)!;
        }

        private object ResolveConstructorArgument(Type parameterType)
        {
            if (parameterType == typeof(IEventsWebinarsSliderService))
            {
                return _serviceMock.Object;
            }

            if (parameterType == typeof(IContentRetriever))
            {
                return _contentRetrieverMock.Object;
            }

            if (parameterType == typeof(IContentService))
            {
                return _contentServiceMock.Object;
            }

            var mockType = typeof(Mock<>).MakeGenericType(parameterType);
            var mock = Activator.CreateInstance(mockType)!;
            return mockType.GetProperty("Object")!.GetValue(mock)!;
        }

        private static ComponentViewModel<EventsWebinarsSliderWidgetProperties> CreateModel(
            EventsWebinarsSliderWidgetProperties properties,
            Guid? pageGuid = null)
        {
            var model = new ComponentViewModel<EventsWebinarsSliderWidgetProperties>();

            typeof(ComponentViewModel<EventsWebinarsSliderWidgetProperties>)
                .GetProperty("Properties")
                ?.SetValue(model, properties);

            if (pageGuid.HasValue)
            {
                var page = new RoutedWebPage
                {
                    WebPageItemGUID = pageGuid.Value
                };

                typeof(ComponentViewModel<EventsWebinarsSliderWidgetProperties>)
                    .GetProperty("Page")
                    ?.SetValue(model, page);
            }

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

        private void SetupContentServiceMock(IWebPageFieldsSource? page = null)
        {
            _contentServiceMock
                .Setup(x => x.GetItemsByGUIDAsync(It.IsAny<List<Guid>>()))
                .ReturnsAsync(new List<IWebPageFieldsSource> { page ?? new Mock<IWebPageFieldsSource>().Object });
        }

        [Test]
        public async Task InvokeAsync_WhenPageIsNull_ReturnsDefaultDto()
        {
            var model = CreateModel(new EventsWebinarsSliderWidgetProperties());

            var result = await _widget.InvokeAsync(model);

            var viewResult = result as ViewViewComponentResult;
            var dto = viewResult?.ViewData?.Model as EventsWebinarsSliderDto;

            Assert.Multiple(() =>
            {
                Assert.That(viewResult, Is.Not.Null);
                Assert.That(
                    viewResult!.ViewName,
                    Is.EqualTo("~/Components/Widget/EventsWebinarsSliderWidget/_EventsWebinarsSliderWidget.cshtml"));
                Assert.That(dto, Is.Not.Null);
                Assert.That(dto!.FilteringTooltipEnabled, Is.False);
            });

            _contentServiceMock.Verify(
                x => x.GetItemsByGUIDAsync(It.IsAny<List<Guid>>()),
                Times.Never);

            _serviceMock.Verify(
                x => x.GetEventsWebinarsSliderAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<EventsWebinarsSliderData>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public async Task InvokeAsync_WithAllProperties_MapsPropertiesToDataCorrectly()
        {
            var pageGuid = Guid.NewGuid();

            EventsWebinarsSliderData? capturedData = null;
            var expectedDto = CreateDto();

            _serviceMock
                .Setup(x => x.GetEventsWebinarsSliderAsync(
                    pageGuid,
                    It.IsAny<string>(),               // widget passes "RoutedWebPage"
                    It.IsAny<EventsWebinarsSliderData>(),
                    It.IsAny<CancellationToken>()))
                .Callback<Guid, string, EventsWebinarsSliderData, CancellationToken>(
                    (_, __, data, ___) => capturedData = data)
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

            var model = CreateModel(properties, pageGuid);

            var result = await _widget.InvokeAsync(model);

            var viewResult = result as ViewViewComponentResult;

            Assert.Multiple(() =>
            {
                Assert.That(viewResult, Is.Not.Null);
                Assert.That(capturedData, Is.Not.Null);

                Assert.That(
                    viewResult!.ViewName,
                    Is.EqualTo("~/Components/Widget/EventsWebinarsSliderWidget/_EventsWebinarsSliderWidget.cshtml"));

                Assert.That(viewResult.ViewData?.Model, Is.SameAs(expectedDto));

                Assert.That(capturedData!.Title, Is.EqualTo("Test Title"));
                Assert.That(capturedData.Description, Is.EqualTo("Test Description"));
                Assert.That(capturedData.FilteringEnabled, Is.True);
                Assert.That(capturedData.Filters, Is.EquivalentTo(ExpectedFilters));
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
            SetupContentServiceMock();

            EventsWebinarsSliderData? capturedData = null;

            _serviceMock
                .Setup(x => x.GetEventsWebinarsSliderAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<EventsWebinarsSliderData>(),
                    It.IsAny<CancellationToken>()))
                .Callback<Guid, string, EventsWebinarsSliderData, CancellationToken>(
                    (_, __, data, _) => capturedData = data)
                .ReturnsAsync(CreateDto());

            var model = CreateModel(new EventsWebinarsSliderWidgetProperties
            {
                Title = "Title",
                BackgroundImageAltText = string.Empty
            }, Guid.NewGuid());

            await _widget.InvokeAsync(model);

            Assert.Multiple(() =>
            {
                Assert.That(capturedData, Is.Not.Null);
                Assert.That(capturedData!.BackgroundImageAltText, Is.Null);
            });
        }

        [Test]
        public async Task InvokeAsync_WithNullFilters_SetsEmptyFilterCollection()
        {
            SetupContentServiceMock();

            EventsWebinarsSliderData? capturedData = null;

            _serviceMock
                .Setup(x => x.GetEventsWebinarsSliderAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<EventsWebinarsSliderData>(),
                    It.IsAny<CancellationToken>()))
                .Callback<Guid, string, EventsWebinarsSliderData, CancellationToken>(
                    (_, __, data, _) => capturedData = data)
                .ReturnsAsync(CreateDto());

            var model = CreateModel(new EventsWebinarsSliderWidgetProperties
            {
                Title = "Title",
                Filters = null!
            }, Guid.NewGuid());

            await _widget.InvokeAsync(model);

            Assert.Multiple(() =>
            {
                Assert.That(capturedData, Is.Not.Null);
                Assert.That(capturedData!.Filters, Is.Empty);
            });
        }

        [Test]
        public async Task InvokeAsync_ReturnsViewViewComponentResult()
        {
            SetupContentServiceMock();

            _serviceMock
                .Setup(x => x.GetEventsWebinarsSliderAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<EventsWebinarsSliderData>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(CreateDto());

            var model = CreateModel(new EventsWebinarsSliderWidgetProperties
            {
                Title = "Heading"
            }, Guid.NewGuid());

            var result = await _widget.InvokeAsync(model);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result, Is.TypeOf<ViewViewComponentResult>());
            });
        }
    }
}