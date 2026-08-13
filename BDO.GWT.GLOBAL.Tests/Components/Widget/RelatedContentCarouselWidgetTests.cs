using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using BDO.GWT.Core;
using BDO.GWT.Core.Components.Widgets.RelatedContentCarouselWidget;
using BDO.GWT.Core.Constants;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.Services;
using BDO.GWT.Web.Core.LiveSite.Data;
using BDO.GWT.Xperience.Core.LiveSite.Components.Widgets.RelatedContentCarouselWidget;
using CMS.ContentEngine;
using CMS.Websites;
using Kentico.Content.Web.Mvc;
using Kentico.PageBuilder.Web.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Moq;
using NUnit.Framework;

namespace BDO.GWT.GLOBAL.Tests.Components.Widget;

[TestFixture]
public class RelatedContentCarouselWidgetTests
{
    private Mock<IRelatedContentCarouselService> _relatedContentCarouselServiceMock;
    private Mock<IContentService> _contentServiceMock;
    private Mock<IContentRetriever> _contentRetrieverMock;
    private RelatedContentCarouselWidget _sut;

    private static readonly Guid PageGuid = Guid.NewGuid();

    [SetUp]
    public void SetUp()
    {
        _relatedContentCarouselServiceMock = new Mock<IRelatedContentCarouselService>();
        _contentServiceMock = new Mock<IContentService>();
        _contentRetrieverMock = new Mock<IContentRetriever>();

        // Default: GetItemsByGUIDAsync returns a fake page
        _contentServiceMock
            .Setup(x => x.GetItemsByGUIDAsync(It.IsAny<List<Guid>>()))
            .ReturnsAsync(new List<IWebPageFieldsSource> { CreateFakePage(PageGuid) });

        // Default: carousel service returns an empty DTO
        _relatedContentCarouselServiceMock
            .Setup(x => x.GetRelatedContentCarouselDtoAsync(
                It.IsAny<IWebPageFieldsSource>(),
                It.IsAny<RelatedContentCarouselData>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RelatedContentCarouselDto { FilteringTooltipEnabled = false });

        // Note: RetrieveContentByGuids is an extension method and cannot be mocked directly.
        // Tests do not set BackgroundImages, so this path is not exercised.

        _sut = new RelatedContentCarouselWidget(
            _relatedContentCarouselServiceMock.Object,
            _contentRetrieverMock.Object,
            _contentServiceMock.Object);

        var httpContext = new DefaultHttpContext();
        _sut.ViewComponentContext = new ViewComponentContext
        {
            ViewContext = new Microsoft.AspNetCore.Mvc.Rendering.ViewContext
            {
                HttpContext = httpContext
            }
        };
    }

    // -------------------------------------------------------------------------
    // Null page guard
    // -------------------------------------------------------------------------

    [Test]
    public async Task InvokeAsync_NullPageGuid_ReturnsEmptyDto()
    {
        var model = CreateModel(new ModelOptions { Guid = null });

        var result = await _sut.InvokeAsync(model);

        var viewResult = result as ViewViewComponentResult;
        Assert.That(viewResult, Is.Not.Null);
        var dto = viewResult!.ViewData?.Model as RelatedContentCarouselDto;
        Assert.That(dto, Is.Not.Null);
        Assert.That(dto!.FilteringTooltipEnabled, Is.False);
    }

    [Test]
    public async Task InvokeAsync_NullPageGuid_DoesNotCallCarouselService()
    {
        var model = CreateModel(new ModelOptions { Guid = null });

        await _sut.InvokeAsync(model);

        _relatedContentCarouselServiceMock.Verify(x =>
            x.GetRelatedContentCarouselDtoAsync(
                It.IsAny<IWebPageFieldsSource>(),
                It.IsAny<RelatedContentCarouselData>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task InvokeAsync_NullPageGuid_DoesNotCallContentService()
    {
        var model = CreateModel(new ModelOptions { Guid = null });

        await _sut.InvokeAsync(model);

        _contentServiceMock.Verify(x =>
            x.GetItemsByGUIDAsync(It.IsAny<List<Guid>>()),
            Times.Never);
    }

    // -------------------------------------------------------------------------
    // Happy path — page resolved correctly
    // -------------------------------------------------------------------------

    [Test]
    public async Task InvokeAsync_ValidPage_CallsGetItemsByGUIDWithCorrectGuid()
    {
        var model = CreateModel(new ModelOptions { Guid = PageGuid });

        await _sut.InvokeAsync(model);

        _contentServiceMock.Verify(x =>
            x.GetItemsByGUIDAsync(It.Is<List<Guid>>(list => list.Contains(PageGuid))),
            Times.Once);
    }

    [Test]
    public async Task InvokeAsync_ValidPage_CallsCarouselService()
    {
        var model = CreateModel(new ModelOptions { Guid = PageGuid });

        await _sut.InvokeAsync(model);

        _relatedContentCarouselServiceMock.Verify(x =>
            x.GetRelatedContentCarouselDtoAsync(
                It.IsAny<IWebPageFieldsSource>(),
                It.IsAny<RelatedContentCarouselData>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task InvokeAsync_ValidPage_ReturnsViewWithDto()
    {
        var expectedDto = new RelatedContentCarouselDto
        {
            FilteringTooltipEnabled = true,
            Title = "Test Title"
        };

        _relatedContentCarouselServiceMock
            .Setup(x => x.GetRelatedContentCarouselDtoAsync(
                It.IsAny<IWebPageFieldsSource>(),
                It.IsAny<RelatedContentCarouselData>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedDto);

        var model = CreateModel(new ModelOptions { Guid = PageGuid });

        var result = await _sut.InvokeAsync(model);

        var viewResult = result as ViewViewComponentResult;
        Assert.That(viewResult, Is.Not.Null);
        Assert.That(viewResult!.ViewData?.Model, Is.EqualTo(expectedDto));
    }

    [Test]
    public async Task InvokeAsync_ValidPage_RendersCorrectView()
    {
        var model = CreateModel(new ModelOptions { Guid = PageGuid });

        var result = await _sut.InvokeAsync(model);

        var viewResult = result as ViewViewComponentResult;
        Assert.That(viewResult?.ViewName, Does.Contain("_RelatedContentCarouselWidget.cshtml"));
    }

    // -------------------------------------------------------------------------
    // SelectedPageTypes — default fallback
    // -------------------------------------------------------------------------

    [Test]
    public async Task InvokeAsync_NullSelectedPageTypes_UsesDefaultPageTypes()
    {
        var model = CreateModel(new ModelOptions { Guid = PageGuid, SelectedPageTypes = null });

        RelatedContentCarouselData? capturedData = null;
        _relatedContentCarouselServiceMock
            .Setup(x => x.GetRelatedContentCarouselDtoAsync(
                It.IsAny<IWebPageFieldsSource>(),
                It.IsAny<RelatedContentCarouselData>(),
                It.IsAny<CancellationToken>()))
            .Callback<IWebPageFieldsSource, RelatedContentCarouselData, CancellationToken>(
                (_, data, _) => capturedData = data)
            .ReturnsAsync(new RelatedContentCarouselDto { FilteringTooltipEnabled = false });

        await _sut.InvokeAsync(model);

        Assert.Multiple(() =>
        {
            Assert.That(capturedData, Is.Not.Null);
            Assert.That(capturedData!.SelectedPageTypes, Is.Not.Empty);
            Assert.That(capturedData.SelectedPageTypes, Contains.Item(nameof(Insight)));
            Assert.That(capturedData.SelectedPageTypes, Contains.Item(nameof(BlogPost)));
            Assert.That(capturedData.SelectedPageTypes, Contains.Item(nameof(Event)));
            Assert.That(capturedData.SelectedPageTypes, Contains.Item(nameof(NewsArticle)));
            Assert.That(capturedData.SelectedPageTypes, Contains.Item(nameof(Webinar)));
            Assert.That(capturedData.SelectedPageTypes, Contains.Item(nameof(MicrositePage)));
        })
    }

    [Test]
    public async Task InvokeAsync_SelectedPageTypesProvided_UsesThem()
    {
        var selectedTypes = new List<string> { nameof(Insight), nameof(BlogPost) };
        var model = CreateModel(new ModelOptions { Guid = PageGuid, SelectedPageTypes = selectedTypes });

        RelatedContentCarouselData? capturedData = null;
        _relatedContentCarouselServiceMock
            .Setup(x => x.GetRelatedContentCarouselDtoAsync(
                It.IsAny<IWebPageFieldsSource>(),
                It.IsAny<RelatedContentCarouselData>(),
                It.IsAny<CancellationToken>()))
            .Callback<IWebPageFieldsSource, RelatedContentCarouselData, CancellationToken>(
                (_, data, _) => capturedData = data)
            .ReturnsAsync(new RelatedContentCarouselDto { FilteringTooltipEnabled = false });

        await _sut.InvokeAsync(model);

        Assert.That(capturedData!.SelectedPageTypes, Is.EquivalentTo(selectedTypes));
    }

    // -------------------------------------------------------------------------
    // Filters — default fallback
    // -------------------------------------------------------------------------

    [Test]
    public async Task InvokeAsync_NullFilters_UsesDefaultFilters()
    {
        var model = CreateModel(new ModelOptions { Guid = PageGuid, Filters = null });

        RelatedContentCarouselData? capturedData = null;
        _relatedContentCarouselServiceMock
            .Setup(x => x.GetRelatedContentCarouselDtoAsync(
                It.IsAny<IWebPageFieldsSource>(),
                It.IsAny<RelatedContentCarouselData>(),
                It.IsAny<CancellationToken>()))
            .Callback<IWebPageFieldsSource, RelatedContentCarouselData, CancellationToken>(
                (_, data, _) => capturedData = data)
            .ReturnsAsync(new RelatedContentCarouselDto { FilteringTooltipEnabled = false });

        await _sut.InvokeAsync(model);

        Assert.Multiple(() =>
        {
            Assert.That(capturedData!.Filters, Is.Not.Empty);
            Assert.That(capturedData.Filters, Contains.Item(FilterParameterName.ContentType));
            Assert.That(capturedData.Filters, Contains.Item(FilterParameterName.BusinessLine));
            Assert.That(capturedData.Filters, Contains.Item(FilterParameterName.IndustryCategory));
        });
    }

    [Test]
    public async Task InvokeAsync_FiltersProvided_UsesThem()
    {
        var filters = new List<string> { FilterParameterName.ContentType };
        var model = CreateModel(new ModelOptions { Guid = PageGuid, Filters = filters });

        RelatedContentCarouselData? capturedData = null;
        _relatedContentCarouselServiceMock
            .Setup(x => x.GetRelatedContentCarouselDtoAsync(
                It.IsAny<IWebPageFieldsSource>(),
                It.IsAny<RelatedContentCarouselData>(),
                It.IsAny<CancellationToken>()))
            .Callback<IWebPageFieldsSource, RelatedContentCarouselData, CancellationToken>(
                (_, data, _) => capturedData = data)
            .ReturnsAsync(new RelatedContentCarouselDto { FilteringTooltipEnabled = false });

        await _sut.InvokeAsync(model);

        Assert.That(capturedData!.Filters, Is.EquivalentTo(filters));
    }

    // -------------------------------------------------------------------------
    // Data mapping — properties propagated to RelatedContentCarouselData
    // -------------------------------------------------------------------------

    [Test]
    public async Task InvokeAsync_MapsAllPropertiesToData()
    {
        var model = CreateModel(new ModelOptions
        {
            Guid = PageGuid,
            Title = "My Title",
            Description = "My Description",
            FilteringEnabled = true,
            SeeAllLinkText = "See All",
            SeeAllLinkUrl = "/all",
            SeeAllLinkTarget = "_blank",
            TopMargin = "20px",
            BottomMargin = "40px"
        });

        RelatedContentCarouselData? capturedData = null;
        _relatedContentCarouselServiceMock
            .Setup(x => x.GetRelatedContentCarouselDtoAsync(
                It.IsAny<IWebPageFieldsSource>(),
                It.IsAny<RelatedContentCarouselData>(),
                It.IsAny<CancellationToken>()))
            .Callback<IWebPageFieldsSource, RelatedContentCarouselData, CancellationToken>(
                (_, data, _) => capturedData = data)
            .ReturnsAsync(new RelatedContentCarouselDto { FilteringTooltipEnabled = false });

        await _sut.InvokeAsync(model);

        Assert.Multiple(() =>
        {
            Assert.That(capturedData, Is.Not.Null);
            Assert.That(capturedData!.Title, Is.EqualTo("My Title"));
            Assert.That(capturedData.Description, Is.EqualTo("My Description"));
            Assert.That(capturedData.FilteringEnabled, Is.True);
            Assert.That(capturedData.SeeAllLinkText, Is.EqualTo("See All"));
            Assert.That(capturedData.SeeAllLinkUrl, Is.EqualTo("/all"));
            Assert.That(capturedData.SeeAllLinkURLTarget, Is.EqualTo("_blank"));
            Assert.That(capturedData.TopMargin, Is.EqualTo("20px"));
            Assert.That(capturedData.BottomMargin, Is.EqualTo("40px"));
        });
    }

    [Test]
    public async Task InvokeAsync_EmptyBackgroundImageAltText_NullOnData()
    {
        var model = CreateModel(new ModelOptions { Guid = PageGuid, BackgroundImageAltText = "" });

        RelatedContentCarouselData? capturedData = null;
        _relatedContentCarouselServiceMock
            .Setup(x => x.GetRelatedContentCarouselDtoAsync(
                It.IsAny<IWebPageFieldsSource>(),
                It.IsAny<RelatedContentCarouselData>(),
                It.IsAny<CancellationToken>()))
            .Callback<IWebPageFieldsSource, RelatedContentCarouselData, CancellationToken>(
                (_, data, _) => capturedData = data)
            .ReturnsAsync(new RelatedContentCarouselDto { FilteringTooltipEnabled = false });

        await _sut.InvokeAsync(model);

        Assert.That(capturedData!.BackgroundImageAltText, Is.Null);
    }

    [Test]
    public async Task InvokeAsync_NonEmptyBackgroundImageAltText_PropagatedToData()
    {
        var model = CreateModel(new ModelOptions { Guid = PageGuid, BackgroundImageAltText = "A nice background" });

        RelatedContentCarouselData? capturedData = null;
        _relatedContentCarouselServiceMock
            .Setup(x => x.GetRelatedContentCarouselDtoAsync(
                It.IsAny<IWebPageFieldsSource>(),
                It.IsAny<RelatedContentCarouselData>(),
                It.IsAny<CancellationToken>()))
            .Callback<IWebPageFieldsSource, RelatedContentCarouselData, CancellationToken>(
                (_, data, _) => capturedData = data)
            .ReturnsAsync(new RelatedContentCarouselDto { FilteringTooltipEnabled = false });

        await _sut.InvokeAsync(model);

        Assert.That(capturedData!.BackgroundImageAltText, Is.EqualTo("A nice background"));
    }

    // -------------------------------------------------------------------------
    // Background image retrieval
    // -------------------------------------------------------------------------
    // Note: RetrieveContentByGuids is an extension method on IContentRetriever and
    // cannot be set up or verified via Moq. Tests that would exercise the background
    // image retrieval path have been intentionally omitted.

    [Test]
    public async Task InvokeAsync_NoBackgroundImageRetrieved_UrlIsNullOnData()
    {
        var model = CreateModel(new ModelOptions { Guid = PageGuid, BackgroundImages = null });

        RelatedContentCarouselData? capturedData = null;
        _relatedContentCarouselServiceMock
            .Setup(x => x.GetRelatedContentCarouselDtoAsync(
                It.IsAny<IWebPageFieldsSource>(),
                It.IsAny<RelatedContentCarouselData>(),
                It.IsAny<CancellationToken>()))
            .Callback<IWebPageFieldsSource, RelatedContentCarouselData, CancellationToken>(
                (_, data, _) => capturedData = data)
            .ReturnsAsync(new RelatedContentCarouselDto { FilteringTooltipEnabled = false });

        await _sut.InvokeAsync(model);

        Assert.That(capturedData!.BackgroundImageUrl, Is.Null);
    }

    // -------------------------------------------------------------------------
    // Featured pages
    // -------------------------------------------------------------------------

    [Test]
    public async Task InvokeAsync_NullFeaturedPages_PassesEmptyCollectionToData()
    {
        var model = CreateModel(new ModelOptions { Guid = PageGuid, FeaturedPages = null });

        RelatedContentCarouselData? capturedData = null;
        _relatedContentCarouselServiceMock
            .Setup(x => x.GetRelatedContentCarouselDtoAsync(
                It.IsAny<IWebPageFieldsSource>(),
                It.IsAny<RelatedContentCarouselData>(),
                It.IsAny<CancellationToken>()))
            .Callback<IWebPageFieldsSource, RelatedContentCarouselData, CancellationToken>(
                (_, data, _) => capturedData = data)
            .ReturnsAsync(new RelatedContentCarouselDto { FilteringTooltipEnabled = false });

        await _sut.InvokeAsync(model);

        Assert.That(capturedData!.FeaturedPageNodeGuids, Is.Empty);
    }

    [Test]
    public async Task InvokeAsync_FeaturedPagesProvided_PassedToData()
    {
        var featured = new List<ContentItemReference>
        {
            new() { Identifier = Guid.NewGuid() },
            new() { Identifier = Guid.NewGuid() }
        };
        var model = CreateModel(new ModelOptions { Guid = PageGuid, FeaturedPages = featured });

        RelatedContentCarouselData? capturedData = null;
        _relatedContentCarouselServiceMock
            .Setup(x => x.GetRelatedContentCarouselDtoAsync(
                It.IsAny<IWebPageFieldsSource>(),
                It.IsAny<RelatedContentCarouselData>(),
                It.IsAny<CancellationToken>()))
            .Callback<IWebPageFieldsSource, RelatedContentCarouselData, CancellationToken>(
                (_, data, _) => capturedData = data)
            .ReturnsAsync(new RelatedContentCarouselDto { FilteringTooltipEnabled = false });

        await _sut.InvokeAsync(model);

        Assert.That(capturedData!.FeaturedPageNodeGuids, Is.EquivalentTo(featured));
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    /// <summary>
    /// Options bag for <see cref="CreateModel"/>. Avoids Sonar S107 (too many parameters)
    /// while keeping each test's intent explicit via object-initializer syntax.
    /// </summary>
    private sealed class ModelOptions
    {
        public Guid? Guid { get; init; }
        public string? Title { get; init; }
        public string? Description { get; init; }
        public bool FilteringEnabled { get; init; }
        public List<string>? SelectedPageTypes { get; init; }
        public List<string>? Filters { get; init; }
        public IEnumerable<ContentItemReference>? FeaturedPages { get; init; }
        public IEnumerable<ContentItemReference>? BackgroundImages { get; init; }
        public string? BackgroundImageAltText { get; init; }
        public string? SeeAllLinkText { get; init; }
        public string? SeeAllLinkUrl { get; init; }
        public string? SeeAllLinkTarget { get; init; } = "_self";
        public string? TopMargin { get; init; }
        public string? BottomMargin { get; init; }
    }

    private static ComponentViewModel<RelatedContentCarouselWidgetProperties> CreateModel(ModelOptions options)
    {
        var properties = new RelatedContentCarouselWidgetProperties
        {
            Title = options.Title,
            Description = options.Description,
            FilteringEnabled = options.FilteringEnabled,
            SelectedPageTypes = options.SelectedPageTypes,
            Filters = options.Filters,
            FeaturedPages = options.FeaturedPages,
            BackgroundImages = options.BackgroundImages ?? Enumerable.Empty<ContentItemReference>(),
            BackgroundImageAltText = options.BackgroundImageAltText ?? "",
            SeeAllLinkText = options.SeeAllLinkText,
            SeeAllLinkURL = options.SeeAllLinkUrl,
            SeeAllLinkURLTarget = options.SeeAllLinkTarget,
            TopMargin = options.TopMargin,
            BottomMargin = options.BottomMargin
        };

        // ComponentViewModel is sealed — use RuntimeHelpers + reflection
        var viewModel = (ComponentViewModel<RelatedContentCarouselWidgetProperties>)
            RuntimeHelpers.GetUninitializedObject(typeof(ComponentViewModel<RelatedContentCarouselWidgetProperties>));

        SetProperty(viewModel, nameof(ComponentViewModel<RelatedContentCarouselWidgetProperties>.Properties), properties);

        if (options.Guid.HasValue)
        {
            // model.Page is IWebPageDataContext or similar — find the correct type
            // by checking what ComponentViewModel.Page returns in your XbyK version.
            // Use reflection to set it via the property directly to avoid type name issues.
            var pageProperty = typeof(ComponentViewModel<RelatedContentCarouselWidgetProperties>)
                .GetProperty("Page", BindingFlags.Public | BindingFlags.Instance);

            if (pageProperty != null)
            {
                var pageType = pageProperty.PropertyType;
                var fakePage = RuntimeHelpers.GetUninitializedObject(pageType);

                // Try to set WebPageItemGUID on whatever the Page type is
                var guidProperty = pageType.GetProperty("WebPageItemGUID", BindingFlags.Public | BindingFlags.Instance);
                guidProperty?.SetValue(fakePage, options.Guid.Value);

                pageProperty.SetValue(viewModel, fakePage);
            }
        }

        return viewModel;
    }

    private static IWebPageFieldsSource CreateFakePage(Guid guid)
    {
        var fields = (WebPageFields)RuntimeHelpers.GetUninitializedObject(typeof(WebPageFields));
        SetProperty(fields, nameof(WebPageFields.ContentItemGUID), guid);
        return new FakePage { SystemFields = fields };
    }


    private static void SetProperty(object obj, string propertyName, object? value)
    {
        obj.GetType()
            .GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance)
            ?.SetValue(obj, value);
    }

    private class FakePage : IWebPageFieldsSource
    {
        public WebPageFields SystemFields { get; set; } = null!;
    }
}