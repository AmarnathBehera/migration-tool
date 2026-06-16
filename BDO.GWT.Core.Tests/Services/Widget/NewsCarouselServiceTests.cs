using BDO.GWT.Core;
using BDO.GWT.Core.Constants;
using BDO.GWT.Core.Data.Queries;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Cards;
using BDO.GWT.Core.Helpers;
using BDO.GWT.Core.LiveSite.Services;
using BDO.GWT.Core.Services;
using CMS.ContentEngine;
using CMS.Websites;
using CMS.Websites.Routing;
using Moq;

namespace BDO.GWT.Core.Tests.Services.Widget;

public class NewsCarouselServiceTests
{
    private Mock<IFilteringHelper> _filteringHelperMock = null!;
    private Mock<INewsArticleCardsService> _newsArticleCardsServiceMock = null!;
    private Mock<IContentService> _contentServiceMock = null!;
    private Mock<IWebsiteChannelContext> _websiteChannelContextMock = null!;
    private Mock<IContentQueryExecutor> _contentQueryExecutorMock = null!;
    private NewsCarouselService _service = null!;

    [SetUp]
    public void Setup()
    {
        _filteringHelperMock = new Mock<IFilteringHelper>();
        _newsArticleCardsServiceMock = new Mock<INewsArticleCardsService>();
        _contentServiceMock = new Mock<IContentService>();
        _websiteChannelContextMock = new Mock<IWebsiteChannelContext>();
        _contentQueryExecutorMock = new Mock<IContentQueryExecutor>();

        _contentServiceMock
            .Setup(x => x.GetCurrentLanguage())
            .Returns("en-US");

        _contentServiceMock
            .Setup(x => x.RetrievePagesAsync<SectionNews>(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SectionNews?>());

        _service = new NewsCarouselService(
            _filteringHelperMock.Object,
            _newsArticleCardsServiceMock.Object,
            _contentServiceMock.Object);
    }

    private (List<Action<WhereParameters>>? Filters, CardsQuery? Query) SetupFilteredItemsCapture()
    {
        List<Action<WhereParameters>>? capturedFilters = null;
        CardsQuery? capturedQuery = null;

        _newsArticleCardsServiceMock
            .Setup(x => x.GetFilteredItemsAsync(
                It.IsAny<List<SectionNews>>(),
                It.IsAny<string>(),
                It.IsAny<CardsQuery>(),
                It.IsAny<List<Action<WhereParameters>>?>(),
                It.IsAny<Func<NewsArticle, bool>>(),
                It.IsAny<CancellationToken>()))
            .Callback<List<SectionNews>, string, CardsQuery, List<Action<WhereParameters>>?, Func<NewsArticle, bool>, CancellationToken>(
                (_, _, q, filters, _, _) =>
                {
                    capturedQuery = q;
                    capturedFilters = filters;
                })
            .ReturnsAsync(new PaginationDto<NewsPageCardDto> { Data = [], TotalCount = 0 });

        // Capture happens during the Act step below; return references that get
        // populated by the time assertions run.
        return (capturedFilters, capturedQuery);
    }

    [Test]
    public async Task GetAPIFilteredItemsAsync_PageTypeMatchesKnownFilter_AddsFilter_DefaultsPageSizeTo20()
    {
        // Arrange
        var query = new CardsQuery
        {
            PageType = FilterParameterName.BusinessLine,
            CurrentPageGuid = Guid.NewGuid(),
        };

        List<Action<WhereParameters>>? capturedFilters = null;
        CardsQuery? capturedQuery = null;

        _newsArticleCardsServiceMock
            .Setup(x => x.GetFilteredItemsAsync(
                It.IsAny<List<SectionNews>>(),
                It.IsAny<string>(),
                It.IsAny<CardsQuery>(),
                It.IsAny<List<Action<WhereParameters>>?>(),
                It.IsAny<Func<NewsArticle, bool>>(),
                It.IsAny<CancellationToken>()))
            .Callback<List<SectionNews>, string, CardsQuery, List<Action<WhereParameters>>?, Func<NewsArticle, bool>, CancellationToken>(
                (_, _, q, filters, _, _) =>
                {
                    capturedQuery = q;
                    capturedFilters = filters;
                })
            .ReturnsAsync(new PaginationDto<NewsPageCardDto> { Data = [], TotalCount = 0 });

        // Act
        var result = await _service.GetAPIFilteredItemsAsync(query, CancellationToken.None);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(capturedFilters, Has.Count.EqualTo(1));
            Assert.That(capturedQuery!.PageSize, Is.EqualTo(20));
            Assert.That(capturedQuery.CurrentPage, Is.EqualTo(1));
        });
    }

    [Test]
    public async Task GetAPIFilteredItemsAsync_PageTypeIsNullOrEmpty_NoFilterAdded_DefaultsPageSizeTo10()
    {
        // Arrange
        var query = new CardsQuery { PageType = null };

        List<Action<WhereParameters>>? capturedFilters = null;
        CardsQuery? capturedQuery = null;

        _newsArticleCardsServiceMock
            .Setup(x => x.GetFilteredItemsAsync(
                It.IsAny<List<SectionNews>>(),
                It.IsAny<string>(),
                It.IsAny<CardsQuery>(),
                It.IsAny<List<Action<WhereParameters>>?>(),
                It.IsAny<Func<NewsArticle, bool>>(),
                It.IsAny<CancellationToken>()))
            .Callback<List<SectionNews>, string, CardsQuery, List<Action<WhereParameters>>?, Func<NewsArticle, bool>, CancellationToken>(
                (_, _, q, filters, _, _) =>
                {
                    capturedQuery = q;
                    capturedFilters = filters;
                })
            .ReturnsAsync(new PaginationDto<NewsPageCardDto> { Data = [], TotalCount = 0 });

        // Act
        var result = await _service.GetAPIFilteredItemsAsync(query, CancellationToken.None);

        // Assert: still falls through to the service call, does NOT return null
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(capturedFilters, Is.Empty);
            Assert.That(capturedQuery!.PageSize, Is.EqualTo(10));
        });
    }

    [Test]
    public async Task GetAPIFilteredItemsAsync_PageTypeNotRecognizedBySwitch_FallsThroughWithoutFilter()
    {
        // Arrange: a value that isn't one of the 7 handled FilterParameterName cases
        // (and isn't even one of the other FilterParameterName constants - pure garbage input)
        var query = new CardsQuery { PageType = "notARealFilterType" };

        List<Action<WhereParameters>>? capturedFilters = null;
        CardsQuery? capturedQuery = null;

        _newsArticleCardsServiceMock
            .Setup(x => x.GetFilteredItemsAsync(
                It.IsAny<List<SectionNews>>(),
                It.IsAny<string>(),
                It.IsAny<CardsQuery>(),
                It.IsAny<List<Action<WhereParameters>>?>(),
                It.IsAny<Func<NewsArticle, bool>>(),
                It.IsAny<CancellationToken>()))
            .Callback<List<SectionNews>, string, CardsQuery, List<Action<WhereParameters>>?, Func<NewsArticle, bool>, CancellationToken>(
                (_, _, q, filters, _, _) =>
                {
                    capturedQuery = q;
                    capturedFilters = filters;
                })
            .ReturnsAsync(new PaginationDto<NewsPageCardDto> { Data = [], TotalCount = 0 });

        // Act
        var result = await _service.GetAPIFilteredItemsAsync(query, CancellationToken.None);

        // Assert: unrecognized type behaves the same as "no page type" - no early return
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(capturedFilters, Is.Empty);
            Assert.That(capturedQuery!.PageSize, Is.EqualTo(10));
        });
    }

    [TestCase(FilterParameterName.ServiceArea, nameof(NewsArticle.MetadataServiceAreas))]
    [TestCase(FilterParameterName.Service, nameof(NewsArticle.MetadataServices))]
    [TestCase(FilterParameterName.IndustryCategory, nameof(NewsArticle.MetadataIndustryCategories))]
    [TestCase(FilterParameterName.Industry, nameof(NewsArticle.MetadataIndustries))]
    [TestCase(FilterParameterName.MicrositeMasterPage, nameof(NewsArticle.MicrositeMasterPages))]
    [TestCase(FilterParameterName.MicrositePage, nameof(NewsArticle.MicrositePages))]
    public async Task GetAPIFilteredItemsAsync_EachKnownPageType_AddsExactlyOneFilter(string pageType, string _)
    {
        // Arrange
        var query = new CardsQuery
        {
            PageType = pageType,
            CurrentPageGuid = Guid.NewGuid(),
        };

        List<Action<WhereParameters>>? capturedFilters = null;

        _newsArticleCardsServiceMock
            .Setup(x => x.GetFilteredItemsAsync(
                It.IsAny<List<SectionNews>>(),
                It.IsAny<string>(),
                It.IsAny<CardsQuery>(),
                It.IsAny<List<Action<WhereParameters>>?>(),
                It.IsAny<Func<NewsArticle, bool>>(),
                It.IsAny<CancellationToken>()))
            .Callback<List<SectionNews>, string, CardsQuery, List<Action<WhereParameters>>?, Func<NewsArticle, bool>, CancellationToken>(
                (_, _, _, filters, _, _) => capturedFilters = filters)
            .ReturnsAsync(new PaginationDto<NewsPageCardDto> { Data = [], TotalCount = 0 });

        // Act
        await _service.GetAPIFilteredItemsAsync(query, CancellationToken.None);

        // Assert
        Assert.That(capturedFilters, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task GetAPIFilteredItemsAsync_ExplicitPageSizeAndCurrentPage_AreNotOverridden()
    {
        // Arrange
        var query = new CardsQuery
        {
            PageType = FilterParameterName.Service,
            CurrentPageGuid = Guid.NewGuid(),
            CurrentPage = 3,
            PageSize = 5,
        };

        CardsQuery? capturedQuery = null;

        _newsArticleCardsServiceMock
            .Setup(x => x.GetFilteredItemsAsync(
                It.IsAny<List<SectionNews>>(),
                It.IsAny<string>(),
                It.IsAny<CardsQuery>(),
                It.IsAny<List<Action<WhereParameters>>?>(),
                It.IsAny<Func<NewsArticle, bool>>(),
                It.IsAny<CancellationToken>()))
            .Callback<List<SectionNews>, string, CardsQuery, List<Action<WhereParameters>>?, Func<NewsArticle, bool>, CancellationToken>(
                (_, _, q, _, _, _) => capturedQuery = q)
            .ReturnsAsync(new PaginationDto<NewsPageCardDto> { Data = [], TotalCount = 0 });

        // Act
        await _service.GetAPIFilteredItemsAsync(query, CancellationToken.None);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(capturedQuery!.CurrentPage, Is.EqualTo(3));
            Assert.That(capturedQuery.PageSize, Is.EqualTo(5));
        });
    }

    [Test]
    public async Task GetAPIFilteredItemsAsync_PassesLanguageFromContentService()
    {
        // Arrange
        var query = new CardsQuery();
        string? capturedLanguage = null;

        _contentServiceMock
            .Setup(x => x.GetCurrentLanguage())
            .Returns("fr-FR");

        _newsArticleCardsServiceMock
            .Setup(x => x.GetFilteredItemsAsync(
                It.IsAny<List<SectionNews>>(),
                It.IsAny<string>(),
                It.IsAny<CardsQuery>(),
                It.IsAny<List<Action<WhereParameters>>?>(),
                It.IsAny<Func<NewsArticle, bool>>(),
                It.IsAny<CancellationToken>()))
            .Callback<List<SectionNews>, string, CardsQuery, List<Action<WhereParameters>>?, Func<NewsArticle, bool>, CancellationToken>(
                (_, language, _, _, _, _) => capturedLanguage = language)
            .ReturnsAsync(new PaginationDto<NewsPageCardDto> { Data = [], TotalCount = 0 });

        // Act
        await _service.GetAPIFilteredItemsAsync(query, CancellationToken.None);

        // Assert
        Assert.That(capturedLanguage, Is.EqualTo("fr-FR"));
    }

    [Test]
    public async Task GetAPIFilteredItemsAsync_ReturnsDataFromUnderlyingService()
    {
        // Arrange
        var query = new CardsQuery();
        var expectedItems = new List<NewsPageCardDto>
        {
            new() { Title = "Article A" },
        };

        _newsArticleCardsServiceMock
            .Setup(x => x.GetFilteredItemsAsync(
                It.IsAny<List<SectionNews>>(),
                It.IsAny<string>(),
                It.IsAny<CardsQuery>(),
                It.IsAny<List<Action<WhereParameters>>?>(),
                It.IsAny<Func<NewsArticle, bool>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaginationDto<NewsPageCardDto> { Data = expectedItems.ToArray(), TotalCount = 1 });

        // Act
        var result = await _service.GetAPIFilteredItemsAsync(query, CancellationToken.None);

        // Assert
        Assert.That(result, Is.EquivalentTo(expectedItems));
    }
}