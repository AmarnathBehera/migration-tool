using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.LiveSite.Services;
using BDO.GWT.GLOBAL.Components.Widget.InsightCarouselWidget;
using BDO.GWT.GLOBAL.Widgets;
using BDO.GWT.Web.Core.Data;
using CMS.ContentEngine;
using Kentico.Content.Web.Mvc;
using Kentico.PageBuilder.Web.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Moq;
using System.Reflection;
using System.Runtime.Serialization;

namespace BDO.GWT.GLOBAL.Tests.Widgets
{
    [TestFixture]
    public class InsightCarouselWidgetTests
    {
        private const string ExpectedViewPath =
            "~/Components/Widget/InsightCarouselWidget/_InsightCarouselWidget.cshtml";

        private Mock<IInsightCarouselService> _insightCarouselServiceMock = null!;
        private Mock<IContentRetriever> _contentRetrieverMock = null!;
        private InsightCarouselWidget _widget = null!;
        private InsightCarouselDto _defaultDto = null!;

        [SetUp]
        public void Setup()
        {
            _insightCarouselServiceMock = new Mock<IInsightCarouselService>();
            _contentRetrieverMock = new Mock<IContentRetriever>();

            _defaultDto = new InsightCarouselDto { FilteringTooltipEnabled = false };

            _insightCarouselServiceMock
                .Setup(x => x.GetInsightCarouselAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<InsightCarouselData>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(_defaultDto);

            _widget = new InsightCarouselWidget(
                _insightCarouselServiceMock.Object,
                _contentRetrieverMock.Object);

            _widget.ViewComponentContext = new ViewComponentContext
            {
                ViewContext = new ViewContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };
        }

        // ── Null page guard ──────────────────────────────────────────────────

        [Test]
        public async Task InvokeAsync_WhenPageIsNull_ReturnsEmptyDtoView()
        {
            var model = CreateComponentViewModel(null, new InsightCarouselWidgetProperties());

            var result = await _widget.InvokeAsync(model) as ViewViewComponentResult;

            Assert.That(result, Is.Not.Null);
            var dto = result!.ViewData?.Model as InsightCarouselDto;
            Assert.Multiple(() =>
            {
                Assert.That(result.ViewName, Is.EqualTo(ExpectedViewPath));
                Assert.That(dto, Is.Not.Null);
                Assert.That(dto!.FilteringTooltipEnabled, Is.False);
            });
        }

        [Test]
        public async Task InvokeAsync_WhenPageIsNull_DoesNotCallService()
        {
            var model = CreateComponentViewModel(null, new InsightCarouselWidgetProperties());

            await _widget.InvokeAsync(model);

            _insightCarouselServiceMock.Verify(x => x.GetInsightCarouselAsync(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<InsightCarouselData>(),
                It.IsAny<CancellationToken>()), Times.Never);
        }

        // ── Return type / view name for non-null page ────────────────────────

        [Test]
        public async Task InvokeAsync_WithValidPage_ReturnsViewViewComponentResult()
        {
            var page = CreateRoutedWebPage(Guid.NewGuid(), "BDO.SomePage");
            var model = CreateComponentViewModel(page, new InsightCarouselWidgetProperties());

            var result = await _widget.InvokeAsync(model);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.TypeOf<ViewViewComponentResult>());
                Assert.That(((ViewViewComponentResult)result).ViewName, Is.EqualTo(ExpectedViewPath));
                Assert.That(((ViewViewComponentResult)result).ViewData?.Model, Is.SameAs(_defaultDto));
            });
        }

        [Test]
        public async Task InvokeAsync_WithValidPage_CallsServiceWithPageGuidAndContentTypeName()
        {
            var guid = Guid.NewGuid();
            const string typeName = "BDO.InsightPage";
            var page = CreateRoutedWebPage(guid, typeName);
            var model = CreateComponentViewModel(page, new InsightCarouselWidgetProperties());

            await _widget.InvokeAsync(model);

            _insightCarouselServiceMock.Verify(x => x.GetInsightCarouselAsync(
                guid,
                typeName,
                It.IsAny<InsightCarouselData>(),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        // ── Property mapping ────────────────────────────────────────────────

        [Test]
        public async Task InvokeAsync_WithAllProperties_MapsPropertiesToDataCorrectly()
        {
            var page = CreateRoutedWebPage(Guid.NewGuid(), "BDO.InsightPage");
            var featured = new[]
            {
                new ContentItemReference { Identifier = Guid.NewGuid() },
                new ContentItemReference { Identifier = Guid.NewGuid() }
            };
            var filters = new List<string> { "contentType", "businessLine" };

            var properties = new InsightCarouselWidgetProperties
            {
                Title = "Insights",
                Description = "Latest insights",
                FilteringEnabled = true,
                Filters = filters,
                FeaturedInsights = featured,
                BackgroundImageAltText = "Alt",
                SeeAllLinkText = "See all",
                SeeAllLinkURL = "/insights",
                SeeAllLinkURLTarget = "_blank",
                TopMargin = "20px",
                BottomMargin = "40px",
                WidgetID = "insight-carousel",
                BackgroundImages = Enumerable.Empty<ContentItemReference>()
            };

            InsightCarouselData? captured = null;
            _insightCarouselServiceMock
                .Setup(x => x.GetInsightCarouselAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<InsightCarouselData>(),
                    It.IsAny<CancellationToken>()))
                .Callback<Guid, string, InsightCarouselData, CancellationToken>(
                    (_, _, data, _) => captured = data)
                .ReturnsAsync(_defaultDto);

            await _widget.InvokeAsync(CreateComponentViewModel(page, properties));

            Assert.That(captured, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(captured!.Title, Is.EqualTo("Insights"));
                Assert.That(captured.Description, Is.EqualTo("Latest insights"));
                Assert.That(captured.FilteringEnabled, Is.True);
                Assert.That(captured.Filters, Is.EquivalentTo(filters));
                Assert.That(captured.FeaturedInsightNodeGuids, Is.SameAs(featured));
                Assert.That(captured.BackgroundImageURL, Is.Null);
                Assert.That(captured.BackgroundImageAltText, Is.EqualTo("Alt"));
                Assert.That(captured.SeeAllLinkText, Is.EqualTo("See all"));
                Assert.That(captured.SeeAllLinkUrl, Is.EqualTo("/insights"));
                Assert.That(captured.SeeAllLinkURLTarget, Is.EqualTo("_blank"));
                Assert.That(captured.TopMargin, Is.EqualTo("20px"));
                Assert.That(captured.BottomMargin, Is.EqualTo("40px"));
                Assert.That(captured.WidgetID, Is.EqualTo("insight-carousel"));
            });
        }

        [Test]
        public async Task InvokeAsync_WithoutBackgroundImages_SetsBackgroundImageUrlNull()
        {
            var page = CreateRoutedWebPage(Guid.NewGuid(), "BDO.InsightPage");
            var properties = new InsightCarouselWidgetProperties
            {
                BackgroundImages = Enumerable.Empty<ContentItemReference>()
            };

            InsightCarouselData? captured = null;
            _insightCarouselServiceMock
                .Setup(x => x.GetInsightCarouselAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<InsightCarouselData>(),
                    It.IsAny<CancellationToken>()))
                .Callback<Guid, string, InsightCarouselData, CancellationToken>(
                    (_, _, data, _) => captured = data)
                .ReturnsAsync(_defaultDto);

            await _widget.InvokeAsync(CreateComponentViewModel(page, properties));

            Assert.That(captured, Is.Not.Null);
            Assert.That(captured!.BackgroundImageURL, Is.Null);

            // Extension method RetrieveContentByGuids should not be invoked when there are no images.
            // We assert the underlying mock was never touched (no setups needed for that branch).
            _contentRetrieverMock.VerifyNoOtherCalls();
        }

        [Test]
        public async Task InvokeAsync_WithNullBackgroundImages_TreatedAsEmpty()
        {
            var page = CreateRoutedWebPage(Guid.NewGuid(), "BDO.InsightPage");
            var properties = new InsightCarouselWidgetProperties
            {
                BackgroundImages = null!
            };

            InsightCarouselData? captured = null;
            _insightCarouselServiceMock
                .Setup(x => x.GetInsightCarouselAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<InsightCarouselData>(),
                    It.IsAny<CancellationToken>()))
                .Callback<Guid, string, InsightCarouselData, CancellationToken>(
                    (_, _, data, _) => captured = data)
                .ReturnsAsync(_defaultDto);

            await _widget.InvokeAsync(CreateComponentViewModel(page, properties));

            Assert.That(captured, Is.Not.Null);
            Assert.That(captured!.BackgroundImageURL, Is.Null);
        }

        [Test]
        public async Task InvokeAsync_WithNullFilters_SetsEmptyFilterHashSet()
        {
            var page = CreateRoutedWebPage(Guid.NewGuid(), "BDO.InsightPage");
            var properties = new InsightCarouselWidgetProperties
            {
                Filters = null!
            };

            InsightCarouselData? captured = null;
            _insightCarouselServiceMock
                .Setup(x => x.GetInsightCarouselAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<InsightCarouselData>(),
                    It.IsAny<CancellationToken>()))
                .Callback<Guid, string, InsightCarouselData, CancellationToken>(
                    (_, _, data, _) => captured = data)
                .ReturnsAsync(_defaultDto);

            await _widget.InvokeAsync(CreateComponentViewModel(page, properties));

            Assert.That(captured, Is.Not.Null);
            Assert.That(captured!.Filters, Is.Empty);
        }

        [Test]
        public async Task InvokeAsync_WithNullFeaturedInsights_SetsEmptyCollection()
        {
            var page = CreateRoutedWebPage(Guid.NewGuid(), "BDO.InsightPage");
            var properties = new InsightCarouselWidgetProperties
            {
                FeaturedInsights = null!
            };

            InsightCarouselData? captured = null;
            _insightCarouselServiceMock
                .Setup(x => x.GetInsightCarouselAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<InsightCarouselData>(),
                    It.IsAny<CancellationToken>()))
                .Callback<Guid, string, InsightCarouselData, CancellationToken>(
                    (_, _, data, _) => captured = data)
                .ReturnsAsync(_defaultDto);

            await _widget.InvokeAsync(CreateComponentViewModel(page, properties));

            Assert.That(captured, Is.Not.Null);
            Assert.That(captured!.FeaturedInsightNodeGuids, Is.Empty);
        }

        [Test]
        public async Task InvokeAsync_PassesRequestAbortedTokenToService()
        {
            var page = CreateRoutedWebPage(Guid.NewGuid(), "BDO.InsightPage");
            using var cts = new CancellationTokenSource();

            _widget.ViewComponentContext = new ViewComponentContext
            {
                ViewContext = new ViewContext
                {
                    HttpContext = new DefaultHttpContext { RequestAborted = cts.Token }
                }
            };

            CancellationToken capturedToken = default;
            _insightCarouselServiceMock
                .Setup(x => x.GetInsightCarouselAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<InsightCarouselData>(),
                    It.IsAny<CancellationToken>()))
                .Callback<Guid, string, InsightCarouselData, CancellationToken>(
                    (_, _, _, token) => capturedToken = token)
                .ReturnsAsync(_defaultDto);

            await _widget.InvokeAsync(CreateComponentViewModel(page, new InsightCarouselWidgetProperties()));

            Assert.That(capturedToken, Is.EqualTo(cts.Token));
        }

        [Test]
        public async Task InvokeAsync_ReturnsDtoFromService_AsViewModel()
        {
            var page = CreateRoutedWebPage(Guid.NewGuid(), "BDO.InsightPage");
            var expectedDto = new InsightCarouselDto { FilteringTooltipEnabled = true };

            _insightCarouselServiceMock
                .Setup(x => x.GetInsightCarouselAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<InsightCarouselData>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedDto);

            var result = await _widget.InvokeAsync(CreateComponentViewModel(page, new InsightCarouselWidgetProperties()))
                as ViewViewComponentResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.ViewData?.Model, Is.SameAs(expectedDto));
        }

        // Note: The true branch of `model.Properties.BackgroundImages?.Any() == true`
        // invokes the extension method IContentRetriever.RetrieveContentByGuids<T>(...),
        // which cannot be mocked directly with Moq (static extensions). This mirrors
        // the approach used by RelatedContentCarouselWidgetTests in this repository.

        // ── Helpers ──────────────────────────────────────────────────────────

        private sealed class PageInfo
        {
            public Guid Guid { get; init; }
            public string ContentTypeName { get; init; } = string.Empty;
        }

        private static PageInfo CreateRoutedWebPage(Guid webPageItemGuid, string contentTypeName) =>
            new() { Guid = webPageItemGuid, ContentTypeName = contentTypeName };

        private static ComponentViewModel<T> CreateComponentViewModel<T>(PageInfo? page, T properties)
            where T : class, Kentico.Builder.Web.Mvc.IComponentProperties, new()
        {
#pragma warning disable SYSLIB0050
            var model = (ComponentViewModel<T>)FormatterServices.GetUninitializedObject(typeof(ComponentViewModel<T>));
#pragma warning restore SYSLIB0050

            var propertiesProp = typeof(ComponentViewModel<T>).GetProperty(
                "Properties", BindingFlags.Public | BindingFlags.Instance);
            propertiesProp?.SetValue(model, properties);

            if (page is not null)
            {
                var pageProperty = typeof(ComponentViewModel<T>).GetProperty(
                    "Page", BindingFlags.Public | BindingFlags.Instance);
                if (pageProperty is not null)
                {
                    var pageType = pageProperty.PropertyType;
#pragma warning disable SYSLIB0050
                    var pageInstance = FormatterServices.GetUninitializedObject(pageType);
#pragma warning restore SYSLIB0050

                    pageType.GetProperty("WebPageItemGUID", BindingFlags.Public | BindingFlags.Instance)
                        ?.SetValue(pageInstance, page.Guid);
                    pageType.GetProperty("ContentTypeName", BindingFlags.Public | BindingFlags.Instance)
                        ?.SetValue(pageInstance, page.ContentTypeName);

                    pageProperty.SetValue(model, pageInstance);
                }
            }

            return model;
        }
    }
}
