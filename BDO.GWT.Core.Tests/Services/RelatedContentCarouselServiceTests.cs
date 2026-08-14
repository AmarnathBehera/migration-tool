using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using BDO.GWT.Core.Constants;
using BDO.GWT.Core.Data.Queries;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Cards;
using BDO.GWT.Core.Helpers;
using BDO.GWT.Core.LiveSite.Data;
using BDO.GWT.Core.Services;
using BDO.GWT.Web.Core.LiveSite.Data;
using CMS.ContentEngine;
using CMS.Websites;
using Kentico.Content.Web.Mvc;
using Moq;
using NUnit.Framework;

namespace BDO.GWT.Core.Tests.Services;

[TestFixture]
public class RelatedContentCarouselServiceTests
{
    private Mock<IFilteringHelper> _filteringHelperMock;
    private Mock<IRelatedContentCardService> _relatedContentCardServiceMock;
    private Mock<IContentService> _contentServiceMock;
    private RelatedContentCarouselService _sut;

    [SetUp]
    public void SetUp()
    {
        _filteringHelperMock = new Mock<IFilteringHelper>();
        _relatedContentCardServiceMock = new Mock<IRelatedContentCardService>();
        _contentServiceMock = new Mock<IContentService>();

        _contentServiceMock.Setup(x => x.GetCurrentLanguage()).Returns("en-gb");
        _filteringHelperMock
            .Setup(x => x.IsFilteringTooltipEnabledAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _relatedContentCardServiceMock
            .Setup(x => x.GetFilteredItemsAsync(
                It.IsAny<RelatedContentCardsQuery>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<Func<IWebPageFieldsSource, bool>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaginationDto<ContentPageCardDto>
            {
                Data = Array.Empty<ContentPageCardDto>(),
                TotalCount = 0
            });

        _sut = new RelatedContentCarouselService(
            _filteringHelperMock.Object,
            _relatedContentCardServiceMock.Object,
            _contentServiceMock.Object);
    }

    // -------------------------------------------------------------------------
    // GetRelatedContentCarouselDtoAsync
    // -------------------------------------------------------------------------

    [Test]
    public async Task GetRelatedContentCarouselDtoAsync_FilteringEnabled_CallsCreateFiltersAsync()
    {
        var currentPage = CreateFakePage();
        var data = CreateData(filteringEnabled: true, filters: new HashSet<string> { FilterParameterName.ContentType });

        _relatedContentCardServiceMock
            .Setup(x => x.CreateFiltersAsync(
                It.IsAny<HashSet<string>>(),
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<FilterItemDto<OptionDto>>());

        await _sut.GetRelatedContentCarouselDtoAsync(currentPage, data, CancellationToken.None);

        _relatedContentCardServiceMock.Verify(x =>
            x.CreateFiltersAsync(
                It.Is<HashSet<string>>(s => s.Contains(FilterParameterName.ContentType)),
                null,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task GetRelatedContentCarouselDtoAsync_FilteringDisabled_DoesNotCallCreateFiltersAsync()
    {
        var currentPage = CreateFakePage();
        var data = CreateData(filteringEnabled: false);

        await _sut.GetRelatedContentCarouselDtoAsync(currentPage, data, CancellationToken.None);

        _relatedContentCardServiceMock.Verify(x =>
            x.CreateFiltersAsync(
                It.IsAny<HashSet<string>>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task GetRelatedContentCarouselDtoAsync_FilteringDisabled_FiltersOnDtoIsNull()
    {
        var currentPage = CreateFakePage();
        var data = CreateData(filteringEnabled: false);

        var dto = await _sut.GetRelatedContentCarouselDtoAsync(currentPage, data, CancellationToken.None);

        Assert.That(dto.Filters, Is.Null);
    }

    [Test]
    public async Task GetRelatedContentCarouselDtoAsync_SetsCurrentPageGuidOnDto()
    {
        var currentPage = CreateFakePage();
        var data = CreateData();

        var dto = await _sut.GetRelatedContentCarouselDtoAsync(currentPage, data, CancellationToken.None);

        Assert.That(dto.CurrentPageGuid, Is.EqualTo(currentPage.SystemFields.ContentItemGUID));
    }

    [Test]
    public async Task GetRelatedContentCarouselDtoAsync_SetsPageTypeOnDto()
    {
        var currentPage = CreateFakePage();
        var data = CreateData();

        var dto = await _sut.GetRelatedContentCarouselDtoAsync(currentPage, data, CancellationToken.None);

        Assert.That(dto.PageType, Is.EqualTo(currentPage.GetType().Name));
    }

    [Test]
    public async Task GetRelatedContentCarouselDtoAsync_SeeAllLinkBothPresent_SetsSeeAllLink()
    {
        var currentPage = CreateFakePage();
        var data = CreateData();
        data.SeeAllLinkText = "See All";
        data.SeeAllLinkUrl = "/insights";
        data.SeeAllLinkURLTarget = "_blank";

        var dto = await _sut.GetRelatedContentCarouselDtoAsync(currentPage, data, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(dto.SeeAllLink, Is.Not.Null);
            Assert.That(dto.SeeAllLink?.Title, Is.EqualTo("See All"));
            Assert.That(dto.SeeAllLink?.URL, Is.EqualTo("/insights"));
            Assert.That(dto.SeeAllLink?.Target, Is.EqualTo("_blank"));
        });
    }

    [Test]
    public async Task GetRelatedContentCarouselDtoAsync_SeeAllLinkTextMissing_SeeAllLinkIsNull()
    {
        var currentPage = CreateFakePage();
        var data = CreateData();
        data.SeeAllLinkText = null;
        data.SeeAllLinkUrl = "/insights";

        var dto = await _sut.GetRelatedContentCarouselDtoAsync(currentPage, data, CancellationToken.None);

        Assert.That(dto.SeeAllLink, Is.Null);
    }

    [Test]
    public async Task GetRelatedContentCarouselDtoAsync_SeeAllLinkUrlMissing_SeeAllLinkIsNull()
    {
        var currentPage = CreateFakePage();
        var data = CreateData();
        data.SeeAllLinkText = "See All";
        data.SeeAllLinkUrl = null;

        var dto = await _sut.GetRelatedContentCarouselDtoAsync(currentPage, data, CancellationToken.None);

        Assert.That(dto.SeeAllLink, Is.Null);
    }

    [Test]
    public async Task GetRelatedContentCarouselDtoAsync_SeeAllLinkTargetNull_DefaultsToSelf()
    {
        var currentPage = CreateFakePage();
        var data = CreateData();
        data.SeeAllLinkText = "See All";
        data.SeeAllLinkUrl = "/insights";
        data.SeeAllLinkURLTarget = null;

        var dto = await _sut.GetRelatedContentCarouselDtoAsync(currentPage, data, CancellationToken.None);

        Assert.That(dto.SeeAllLink?.Target, Is.EqualTo("_self"));
    }

    [Test]
    public async Task GetRelatedContentCarouselDtoAsync_SelectedPageTypesPropagatedToDto()
    {
        var currentPage = CreateFakePage();
        var selectedTypes = new List<string> { nameof(Insight), nameof(BlogPost) };
        var data = CreateData(selectedPageTypes: selectedTypes);

        var dto = await _sut.GetRelatedContentCarouselDtoAsync(currentPage, data, CancellationToken.None);

        Assert.That(dto.SelectedPageTypes, Is.EquivalentTo(selectedTypes));
    }

    [Test]
    public async Task GetRelatedContentCarouselDtoAsync_SelectedPageTypesPropagatedToInitialCardsQuery()
    {
        var currentPage = CreateFakePage();
        var selectedTypes = new List<string> { nameof(Insight), nameof(BlogPost) };
        var data = CreateData(selectedPageTypes: selectedTypes);

        RelatedContentCardsQuery? capturedQuery = null;
        _relatedContentCardServiceMock
            .Setup(x => x.GetFilteredItemsAsync(
                It.IsAny<RelatedContentCardsQuery>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<Func<IWebPageFieldsSource, bool>>(),
                It.IsAny<CancellationToken>()))
            .Callback<RelatedContentCardsQuery, string, string, Action<RetrievePagesQueryParameters>, Func<IWebPageFieldsSource, bool>, CancellationToken>(
                (q, _, _, _, _, _) => capturedQuery = q)
            .ReturnsAsync(new PaginationDto<ContentPageCardDto> { Data = Array.Empty<ContentPageCardDto>(), TotalCount = 0 });

        await _sut.GetRelatedContentCarouselDtoAsync(currentPage, data, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(capturedQuery, Is.Not.Null);
            Assert.That(capturedQuery!.SelectedPageTypes, Is.EquivalentTo(selectedTypes));
        });
    }

    [Test]
    public async Task GetRelatedContentCarouselDtoAsync_MapsAllDataFields()
    {
        var currentPage = CreateFakePage();
        var data = CreateData();
        data.Title = "Test Title";
        data.Description = "Test Description";
        data.TopMargin = "mt-4";
        data.BottomMargin = "mb-4";
        data.WidgetID = "widget-123";

        var dto = await _sut.GetRelatedContentCarouselDtoAsync(currentPage, data, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(dto.Title, Is.EqualTo("Test Title"));
            Assert.That(dto.Description, Is.EqualTo("Test Description"));
            Assert.That(dto.TopMargin, Is.EqualTo("mt-4"));
            Assert.That(dto.BottomMargin, Is.EqualTo("mb-4"));
            Assert.That(dto.WidgetID, Is.EqualTo("widget-123"));
        });
    }

    // -------------------------------------------------------------------------
    // GetAPIFilteredItemsAsync
    // -------------------------------------------------------------------------

    [Test]
    public async Task GetAPIFilteredItemsAsync_PassesQueryToCardService()
    {
        var query = new RelatedContentCardsQuery
        {
            CurrentPageGuid = Guid.NewGuid(),
            PageType = nameof(Insight),
            SelectedPageTypes = new List<string> { nameof(Insight) },
            Filters = new Dictionary<string, string> { { FilterParameterName.ContentType, "Article" } }
        };

        RelatedContentCardsQuery? capturedQuery = null;
        _relatedContentCardServiceMock
            .Setup(x => x.GetFilteredItemsAsync(
                It.IsAny<RelatedContentCardsQuery>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<Func<IWebPageFieldsSource, bool>>(),
                It.IsAny<CancellationToken>()))
            .Callback<RelatedContentCardsQuery, string, string, Action<RetrievePagesQueryParameters>, Func<IWebPageFieldsSource, bool>, CancellationToken>(
                (q, _, _, _, _, _) => capturedQuery = q)
            .ReturnsAsync(new PaginationDto<ContentPageCardDto> { Data = Array.Empty<ContentPageCardDto>(), TotalCount = 0 });

        await _sut.GetAPIFilteredItemsAsync(query, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(capturedQuery, Is.Not.Null);
            Assert.That(capturedQuery!.SelectedPageTypes, Is.EquivalentTo(query.SelectedPageTypes));
        });
    }

    [Test]
    public async Task GetAPIFilteredItemsAsync_NullCurrentPageGuid_DoesNotThrow()
    {
        var query = new RelatedContentCardsQuery
        {
            CurrentPageGuid = null,
            PageType = null,
            SelectedPageTypes = new List<string> { nameof(Insight) }
        };

        Assert.DoesNotThrowAsync(() => _sut.GetAPIFilteredItemsAsync(query, CancellationToken.None));
    }

    [Test]
    public async Task GetAPIFilteredItemsAsync_CardServiceReturnsNull_ReturnsEmptyArray()
    {
        var query = new RelatedContentCardsQuery
        {
            SelectedPageTypes = new List<string> { nameof(Insight) }
        };

        _relatedContentCardServiceMock
            .Setup(x => x.GetFilteredItemsAsync(
                It.IsAny<RelatedContentCardsQuery>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<Func<IWebPageFieldsSource, bool>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((PaginationDto<ContentPageCardDto>)null!);

        var result = await _sut.GetAPIFilteredItemsAsync(query, CancellationToken.None);

        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task GetAPIFilteredItemsAsync_DefaultsCurrentPageToOne()
    {
        var query = new RelatedContentCardsQuery
        {
            CurrentPage = null,
            SelectedPageTypes = new List<string> { nameof(Insight) }
        };

        RelatedContentCardsQuery? capturedQuery = null;
        _relatedContentCardServiceMock
            .Setup(x => x.GetFilteredItemsAsync(
                It.IsAny<RelatedContentCardsQuery>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<Func<IWebPageFieldsSource, bool>>(),
                It.IsAny<CancellationToken>()))
            .Callback<RelatedContentCardsQuery, string, string, Action<RetrievePagesQueryParameters>, Func<IWebPageFieldsSource, bool>, CancellationToken>(
                (q, _, _, _, _, _) => capturedQuery = q)
            .ReturnsAsync(new PaginationDto<ContentPageCardDto> { Data = Array.Empty<ContentPageCardDto>(), TotalCount = 0 });

        await _sut.GetAPIFilteredItemsAsync(query, CancellationToken.None);

        Assert.That(capturedQuery!.CurrentPage, Is.EqualTo(1));
    }

    [Test]
    public async Task GetAPIFilteredItemsAsync_UnknownPageType_NoDecorateQuery()
    {
        // When pageType is not a known filter type (e.g. a regular page), 
        // decorateQuery should be null — no DB-level filter applied
        var query = new RelatedContentCardsQuery
        {
            PageType = "SomePage",
            CurrentPageGuid = Guid.NewGuid(),
            SelectedPageTypes = new List<string> { nameof(Insight) }
        };

        Action<RetrievePagesQueryParameters>? capturedDecorate = null;
        _relatedContentCardServiceMock
            .Setup(x => x.GetFilteredItemsAsync(
                It.IsAny<RelatedContentCardsQuery>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<Func<IWebPageFieldsSource, bool>>(),
                It.IsAny<CancellationToken>()))
            .Callback<RelatedContentCardsQuery, string, string, Action<RetrievePagesQueryParameters>, Func<IWebPageFieldsSource, bool>, CancellationToken>(
                (_, _, _, d, _, _) => capturedDecorate = d)
            .ReturnsAsync(new PaginationDto<ContentPageCardDto> { Data = Array.Empty<ContentPageCardDto>(), TotalCount = 0 });

        await _sut.GetAPIFilteredItemsAsync(query, CancellationToken.None);

        Assert.That(capturedDecorate, Is.Null);
    }

    [Test]
    public async Task GetAPIFilteredItemsAsync_KnownFilterPageType_DecorateQueryIsSet()
    {
        // When pageType IS a known filter type, decorateQuery should be populated
        var query = new RelatedContentCardsQuery
        {
            PageType = FilterParameterName.BusinessLine,
            CurrentPageGuid = Guid.NewGuid(),
            SelectedPageTypes = new List<string> { nameof(Insight) }
        };

        Action<RetrievePagesQueryParameters>? capturedDecorate = null;
        _relatedContentCardServiceMock
            .Setup(x => x.GetFilteredItemsAsync(
                It.IsAny<RelatedContentCardsQuery>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<Func<IWebPageFieldsSource, bool>>(),
                It.IsAny<CancellationToken>()))
            .Callback<RelatedContentCardsQuery, string, string, Action<RetrievePagesQueryParameters>, Func<IWebPageFieldsSource, bool>, CancellationToken>(
                (_, _, _, d, _, _) => capturedDecorate = d)
            .ReturnsAsync(new PaginationDto<ContentPageCardDto> { Data = Array.Empty<ContentPageCardDto>(), TotalCount = 0 });

        await _sut.GetAPIFilteredItemsAsync(query, CancellationToken.None);

        Assert.That(capturedDecorate, Is.Not.Null);
    }

    [Test]
    public async Task GetAPIFilteredItemsAsync_PageSizeDefaultsTen_WhenNoFilterPageType()
    {
        var query = new RelatedContentCardsQuery
        {
            PageType = "SomePage",
            PageSize = null,
            SelectedPageTypes = new List<string> { nameof(Insight) }
        };

        RelatedContentCardsQuery? capturedQuery = null;
        _relatedContentCardServiceMock
            .Setup(x => x.GetFilteredItemsAsync(
                It.IsAny<RelatedContentCardsQuery>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<Func<IWebPageFieldsSource, bool>>(),
                It.IsAny<CancellationToken>()))
            .Callback<RelatedContentCardsQuery, string, string, Action<RetrievePagesQueryParameters>, Func<IWebPageFieldsSource, bool>, CancellationToken>(
                (q, _, _, _, _, _) => capturedQuery = q)
            .ReturnsAsync(new PaginationDto<ContentPageCardDto> { Data = Array.Empty<ContentPageCardDto>(), TotalCount = 0 });

        await _sut.GetAPIFilteredItemsAsync(query, CancellationToken.None);

        Assert.That(capturedQuery!.PageSize, Is.EqualTo(10));
    }

    [Test]
    public async Task GetAPIFilteredItemsAsync_PageSizeDefaultsTwenty_WhenFilterPageType()
    {
        var query = new RelatedContentCardsQuery
        {
            PageType = FilterParameterName.BusinessLine,
            CurrentPageGuid = Guid.NewGuid(),
            PageSize = null,
            SelectedPageTypes = new List<string> { nameof(Insight) }
        };

        RelatedContentCardsQuery? capturedQuery = null;
        _relatedContentCardServiceMock
            .Setup(x => x.GetFilteredItemsAsync(
                It.IsAny<RelatedContentCardsQuery>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<Func<IWebPageFieldsSource, bool>>(),
                It.IsAny<CancellationToken>()))
            .Callback<RelatedContentCardsQuery, string, string, Action<RetrievePagesQueryParameters>, Func<IWebPageFieldsSource, bool>, CancellationToken>(
                (q, _, _, _, _, _) => capturedQuery = q)
            .ReturnsAsync(new PaginationDto<ContentPageCardDto> { Data = Array.Empty<ContentPageCardDto>(), TotalCount = 0 });

        await _sut.GetAPIFilteredItemsAsync(query, CancellationToken.None);

        Assert.That(capturedQuery!.PageSize, Is.EqualTo(20));
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static FakePage CreateFakePage(Guid? guid = null)
    {
        return new FakePage(guid ?? Guid.NewGuid());
    }

    private static RelatedContentCarouselData CreateData(
        bool filteringEnabled = false,
        HashSet<string>? filters = null,
        List<string>? selectedPageTypes = null)
    {
        return new RelatedContentCarouselData
        {
            FilteringEnabled = filteringEnabled,
            Filters = filters ?? new HashSet<string>(),
            SelectedPageTypes = selectedPageTypes ?? new List<string> { nameof(Insight) },
            FeaturedPageNodeGuids = new List<ContentItemReference>()
        };
    }

    /// <summary>
    /// Minimal IWebPageFieldsSource fake — avoids sealed XbyK types.
    /// </summary>
    private class FakePage : IWebPageFieldsSource
    {
        public FakePage(Guid guid)
        {
            // Use RuntimeHelpers to create WebPageFields without invoking constructor
            // (same pattern used elsewhere in BDO.GWT tests for sealed XbyK types)
            var fields = (WebPageFields)RuntimeHelpers.GetUninitializedObject(typeof(WebPageFields));

            // Set ContentItemGUID via reflection
            typeof(WebPageFields)
                .GetProperty(nameof(WebPageFields.ContentItemGUID))
                ?.SetValue(fields, guid);

            SystemFields = fields;
        }

        public WebPageFields SystemFields { get; set; }
    }
}