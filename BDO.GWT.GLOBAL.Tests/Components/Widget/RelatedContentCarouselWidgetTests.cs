using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
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

namespace BDO.GWT.Core.Tests.Widgets;

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
        var model = CreateModel(pageGuid: null);

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
        var model = CreateModel(pageGuid: null);

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
        var model = CreateModel(pageGuid: null);

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
        var model = CreateModel(pageGuid: PageGuid);

        await _sut.InvokeAsync(model);

        _contentServiceMock.Verify(x =>
            x.GetItemsByGUIDAsync(It.Is<List<Guid>>(list => list.Contains(PageGuid))),
            Times.Once);
    }

    [Test]
    public async Task InvokeAsync_ValidPage_CallsCarouselService()
    {
        var model = CreateModel(pageGuid: PageGuid);

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

        var model = CreateModel(pageGuid: PageGuid);

        var result = await _sut.InvokeAsync(model);

        var viewResult = result as ViewViewComponentResult;
        Assert.That(viewResult, Is.Not.Null);
        Assert.That(viewResult!.ViewData?.Model, Is.EqualTo(expectedDto));
    }

    [Test]
    public async Task InvokeAsync_ValidPage_RendersCorrectView()
    {
        var model = CreateModel(pageGuid: PageGuid);

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
        var model = CreateModel(pageGuid: PageGuid, selectedPageTypes: null);

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

        Assert.That(capturedData, Is.Not.Null);
        Assert.That(capturedData!.SelectedPageTypes, Is.Not.Empty);
        Assert.That(capturedData.SelectedPageTypes, Contains.Item(nameof(Insight)));
        Assert.That(capturedData.SelectedPageTypes, Contains.Item(nameof(BlogPost)));
        Assert.That(capturedData.SelectedPageTypes, Contains.Item(nameof(Event)));
        Assert.That(capturedData.SelectedPageTypes, Contains.Item(nameof(NewsArticle)));
        Assert.That(capturedData.SelectedPageTypes, Contains.Item(nameof(Webinar)));
        Assert.That(capturedData.SelectedPageTypes, Contains.Item(nameof(MicrositePage)));
    }

    [Test]
    public async Task InvokeAsync_SelectedPageTypesProvided_UsesThem()
    {
        var selectedTypes = new List<string> { nameof(Insight), nameof(BlogPost) };
        var model = CreateModel(pageGuid: PageGuid, selectedPageTypes: selectedTypes);

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
        var model = CreateModel(pageGuid: PageGuid, filters: null);

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

        Assert.That(capturedData!.Filters, Is.Not.Empty);
        Assert.That(capturedData.Filters, Contains.Item(FilterParameterName.ContentType));
        Assert.That(capturedData.Filters, Contains.Item(FilterParameterName.BusinessLine));
        Assert.That(capturedData.Filters, Contains.Item(FilterParameterName.IndustryCategory));
    }

    [Test]
    public async Task InvokeAsync_FiltersProvided_UsesThem()
    {
        var filters = new List<string> { FilterParameterName.ContentType };
        var model = CreateModel(pageGuid: PageGuid, filters: filters);

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
        var model = CreateModel(
            pageGuid: PageGuid,
            title: "My Title",
            description: "My Description",
            filteringEnabled: true,
            seeAllLinkText: "See All",
            seeAllLinkUrl: "/all",
            seeAllLinkTarget: "_blank",
            topMargin: "20px",
            bottomMargin: "40px");

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

        Assert.That(capturedData, Is.Not.Null);
        Assert.That(capturedData!.Title, Is.EqualTo("My Title"));
        Assert.That(capturedData.Description, Is.EqualTo("My Description"));
        Assert.That(capturedData.FilteringEnabled, Is.True);
        Assert.That(capturedData.SeeAllLinkText, Is.EqualTo("See All"));
        Assert.That(capturedData.SeeAllLinkUrl, Is.EqualTo("/all"));
        Assert.That(capturedData.SeeAllLinkURLTarget, Is.EqualTo("_blank"));
        Assert.That(capturedData.TopMargin, Is.EqualTo("20px"));
        Assert.That(capturedData.BottomMargin, Is.EqualTo("40px"));
    }

    [Test]
    public async Task InvokeAsync_EmptyBackgroundImageAltText_NullOnData()
    {
        var model = CreateModel(pageGuid: PageGuid, backgroundImageAltText: "");

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
        var model = CreateModel(pageGuid: PageGuid, backgroundImageAltText: "A nice background");

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
        var model = CreateModel(pageGuid: PageGuid, backgroundImages: null);

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
        var model = CreateModel(pageGuid: PageGuid, featuredPages: null);

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
        var model = CreateModel(pageGuid: PageGuid, featuredPages: featured);

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

    private static ComponentViewModel<RelatedContentCarouselWidgetProperties> CreateModel(
        Guid? pageGuid = null,
        string? title = null,
        string? description = null,
        bool filteringEnabled = false,
        List<string>? selectedPageTypes = null,
        List<string>? filters = null,
        IEnumerable<ContentItemReference>? featuredPages = null,
        IEnumerable<ContentItemReference>? backgroundImages = null,
        string? backgroundImageAltText = null,
        string? seeAllLinkText = null,
        string? seeAllLinkUrl = null,
        string? seeAllLinkTarget = "_self",
        string? topMargin = null,
        string? bottomMargin = null)
    {
        var properties = new RelatedContentCarouselWidgetProperties
        {
            Title = title,
            Description = description,
            FilteringEnabled = filteringEnabled,
            SelectedPageTypes = selectedPageTypes,
            Filters = filters,
            FeaturedPages = featuredPages,
            BackgroundImages = backgroundImages ?? Enumerable.Empty<ContentItemReference>(),
            BackgroundImageAltText = backgroundImageAltText ?? "",
            SeeAllLinkText = seeAllLinkText,
            SeeAllLinkURL = seeAllLinkUrl,
            SeeAllLinkURLTarget = seeAllLinkTarget,
            TopMargin = topMargin,
            BottomMargin = bottomMargin
        };

        // ComponentViewModel is sealed — use RuntimeHelpers + reflection
        var viewModel = (ComponentViewModel<RelatedContentCarouselWidgetProperties>)
            RuntimeHelpers.GetUninitializedObject(typeof(ComponentViewModel<RelatedContentCarouselWidgetProperties>));

        SetProperty(viewModel, nameof(ComponentViewModel<RelatedContentCarouselWidgetProperties>.Properties), properties);

        if (pageGuid.HasValue)
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
                guidProperty?.SetValue(fakePage, pageGuid.Value);

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

    private static ContentAttachment CreateFakeAttachment(string url)
    {
        var attachment = (ContentAttachment)RuntimeHelpers.GetUninitializedObject(typeof(ContentAttachment));
        var asset = (ContentItemAsset)RuntimeHelpers.GetUninitializedObject(typeof(ContentItemAsset));
        SetProperty(asset, nameof(ContentItemAsset.Url), url);
        SetProperty(attachment, nameof(ContentAttachment.ContentAttachmentAsset), asset);
        return attachment;
    }

    private static void SetProperty(object obj, string propertyName, object? value)
    {
        obj.GetType()
            .GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance)
            ?.SetValue(obj, value);
    }

    private class FakePage : IWebPageFieldsSource
    {
        public WebPageFields SystemFields { get; set; }
    }
}