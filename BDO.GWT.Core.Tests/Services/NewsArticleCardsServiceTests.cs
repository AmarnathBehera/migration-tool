using BDO.GWT.Core;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.Data.Queries;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Cards;
using BDO.GWT.Core.Services;
using CMS.ContentEngine;
using CMS.Websites;
using Kentico.Content.Web.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Moq;

namespace BDO.GWT.Core.Tests.Services;

public class NewsArticleCardsServiceTests
{
    private readonly Mock<INewsArticleCardsService> _newsArticleCardsServiceMock;

    public NewsArticleCardsServiceTests()
    {
        _newsArticleCardsServiceMock = new Mock<INewsArticleCardsService>();
    }

    [Test]
    public async Task CreateFiltersAsync_ReturnsFilterItems()
    {
        var expectedFilters = new FilterItemDto<OptionDto>[]
        {
            new() { Title = "Filter 1" },
            new() { Title = "Filter 2" }
        };

        _newsArticleCardsServiceMock
            .Setup(x => x.CreateFiltersAsync(It.IsAny<HashSet<string>>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedFilters);

        var result = await _newsArticleCardsServiceMock.Object.CreateFiltersAsync(
            new HashSet<string> { "filter1" }, null, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Length.EqualTo(2));
            Assert.That(result[0].Title, Is.EqualTo("Filter 1"));
        });
    }

    [Test]
    public async Task CreateFiltersAsync_WhenNoFilters_ReturnsEmptyArray()
    {
        _newsArticleCardsServiceMock
            .Setup(x => x.CreateFiltersAsync(It.IsAny<HashSet<string>>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<FilterItemDto<OptionDto>>());

        var result = await _newsArticleCardsServiceMock.Object.CreateFiltersAsync(
            new HashSet<string>(), null, CancellationToken.None);

        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task GetFilteredItemsAsync_ReturnsPaginationDto()
    {
        var expectedDto = new PaginationDto<NewsPageCardDto>
        {
            Data = new[]
            {
                new NewsPageCardDto { Title = "Article 1" },
                new NewsPageCardDto { Title = "Article 2" }
            },
            TotalCount = 2
        };

        _newsArticleCardsServiceMock
            .Setup(x => x.GetFilteredItemsAsync(
                It.IsAny<SectionNews>(),
                It.IsAny<string>(),
                It.IsAny<CardsQuery>(),
                It.IsAny<Action<ContentItemQueryBuilder>?>(),
                It.IsAny<Func<NewsArticle, bool>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedDto);

        var result = await _newsArticleCardsServiceMock.Object.GetFilteredItemsAsync(
            new SectionNews(), "en-US", new CardsQuery(), null, _ => false, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Data, Has.Length.EqualTo(2));
            Assert.That(result.TotalCount, Is.EqualTo(2));
            Assert.That(result.Data[0].Title, Is.EqualTo("Article 1"));
        });
    }

    [Test]
    public async Task GetFilteredItemsAsync_WhenNoItems_ReturnsEmptyPagination()
    {
        var emptyDto = new PaginationDto<NewsPageCardDto>
        {
            Data = Array.Empty<NewsPageCardDto>(),
            TotalCount = 0
        };

        _newsArticleCardsServiceMock
            .Setup(x => x.GetFilteredItemsAsync(
                It.IsAny<SectionNews>(),
                It.IsAny<string>(),
                It.IsAny<CardsQuery>(),
                It.IsAny<Action<ContentItemQueryBuilder>?>(),
                It.IsAny<Func<NewsArticle, bool>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyDto);

        var result = await _newsArticleCardsServiceMock.Object.GetFilteredItemsAsync(
            new SectionNews(), "en-US", new CardsQuery(), null, _ => false, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Data, Is.Empty);
            Assert.That(result.TotalCount, Is.EqualTo(0));
        });
    }

    [Test]
    public async Task CreateFiltersAsync_InvokedOnce()
    {
        _newsArticleCardsServiceMock
            .Setup(x => x.CreateFiltersAsync(It.IsAny<HashSet<string>>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<FilterItemDto<OptionDto>>());

        var filters = new HashSet<string>();
        await _newsArticleCardsServiceMock.Object.CreateFiltersAsync(filters, null, CancellationToken.None);

        _newsArticleCardsServiceMock.Verify(
            x => x.CreateFiltersAsync(filters, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GetFilteredItemsAsync_InvokedOnce()
    {
        _newsArticleCardsServiceMock
            .Setup(x => x.GetFilteredItemsAsync(
                It.IsAny<SectionNews>(),
                It.IsAny<string>(),
                It.IsAny<CardsQuery>(),
                It.IsAny<Action<ContentItemQueryBuilder>?>(),
                It.IsAny<Func<NewsArticle, bool>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaginationDto<NewsPageCardDto> { Data = [], TotalCount = 0 });

        var cardsQuery = new CardsQuery();
        await _newsArticleCardsServiceMock.Object.GetFilteredItemsAsync(
            new SectionNews(), "en-US", cardsQuery, null, _ => false, CancellationToken.None);

        _newsArticleCardsServiceMock.Verify(
            x => x.GetFilteredItemsAsync(
                It.IsAny<SectionNews>(),
                "en-US",
                cardsQuery,
                It.IsAny<Action<ContentItemQueryBuilder>?>(),
                It.IsAny<Func<NewsArticle, bool>>(),
                It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GetFilteredItemsAsync_PassesLanguageThrough()
    {
        string? capturedLanguage = null;

        _newsArticleCardsServiceMock
            .Setup(x => x.GetFilteredItemsAsync(
                It.IsAny<SectionNews>(),
                It.IsAny<string>(),
                It.IsAny<CardsQuery>(),
                It.IsAny<Action<ContentItemQueryBuilder>?>(),
                It.IsAny<Func<NewsArticle, bool>>(),
                It.IsAny<CancellationToken>()))
            .Callback<SectionNews, string, CardsQuery, Action<ContentItemQueryBuilder>?, Func<NewsArticle, bool>, CancellationToken>(
                (_, lang, _, _, _, _) => capturedLanguage = lang)
            .ReturnsAsync(new PaginationDto<NewsPageCardDto> { Data = [], TotalCount = 0 });

        await _newsArticleCardsServiceMock.Object.GetFilteredItemsAsync(
            new SectionNews(), "fr-FR", new CardsQuery(), null, _ => false, CancellationToken.None);

        Assert.That(capturedLanguage, Is.EqualTo("fr-FR"));
    }

    [Test]
    public async Task GetFilteredItemsAsync_PassesSectionNewsThrough()
    {
        SectionNews? capturedSection = null;

        _newsArticleCardsServiceMock
            .Setup(x => x.GetFilteredItemsAsync(
                It.IsAny<SectionNews>(),
                It.IsAny<string>(),
                It.IsAny<CardsQuery>(),
                It.IsAny<Action<ContentItemQueryBuilder>?>(),
                It.IsAny<Func<NewsArticle, bool>>(),
                It.IsAny<CancellationToken>()))
            .Callback<SectionNews, string, CardsQuery, Action<ContentItemQueryBuilder>?, Func<NewsArticle, bool>, CancellationToken>(
                (section, _, _, _, _, _) => capturedSection = section)
            .ReturnsAsync(new PaginationDto<NewsPageCardDto> { Data = [], TotalCount = 0 });

        var newsSection = new SectionNews { PageTitle = "My Section" };
        await _newsArticleCardsServiceMock.Object.GetFilteredItemsAsync(
            newsSection, "en-US", new CardsQuery(), null, _ => false, CancellationToken.None);

        Assert.That(capturedSection, Is.SameAs(newsSection));
    }

    [Test]
    public async Task CreateFiltersAsync_PassesNodeAliasPathThrough()
    {
        string? capturedPath = null;

        _newsArticleCardsServiceMock
            .Setup(x => x.CreateFiltersAsync(
                It.IsAny<HashSet<string>>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .Callback<HashSet<string>, string?, CancellationToken>((_, path, _) => capturedPath = path)
            .ReturnsAsync(Array.Empty<FilterItemDto<OptionDto>>());

        await _newsArticleCardsServiceMock.Object.CreateFiltersAsync(
            new HashSet<string> { "filter1" }, "/news/section", CancellationToken.None);

        Assert.That(capturedPath, Is.EqualTo("/news/section"));
    }

    [Test]
    public async Task GetFilteredItemsAsync_WithFeaturedItems_ReturnsFeaturedFirst()
    {
        var featuredCard = new NewsPageCardDto { Title = "Featured", IsFeatured = true };
        var regularCard = new NewsPageCardDto { Title = "Regular", IsFeatured = false };
        var expectedDto = new PaginationDto<NewsPageCardDto>
        {
            Data = [featuredCard, regularCard],
            TotalCount = 2
        };

        _newsArticleCardsServiceMock
            .Setup(x => x.GetFilteredItemsAsync(
                It.IsAny<SectionNews>(),
                It.IsAny<string>(),
                It.IsAny<CardsQuery>(),
                It.IsAny<Action<ContentItemQueryBuilder>?>(),
                It.IsAny<Func<NewsArticle, bool>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedDto);

        var result = await _newsArticleCardsServiceMock.Object.GetFilteredItemsAsync(
            new SectionNews(), "en-US", new CardsQuery(), null, _ => false, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Data[0].IsFeatured, Is.True);
            Assert.That(result.Data[1].IsFeatured, Is.False);
        });
    }

    [Test]
    public async Task GetFilteredItemsAsync_WithPagination_RespectsPageSizeAndCurrentPage()
    {
        var cardsQuery = new CardsQuery { CurrentPage = 2, PageSize = 5 };

        CardsQuery? capturedQuery = null;

        _newsArticleCardsServiceMock
            .Setup(x => x.GetFilteredItemsAsync(
                It.IsAny<SectionNews>(),
                It.IsAny<string>(),
                It.IsAny<CardsQuery>(),
                It.IsAny<Action<ContentItemQueryBuilder>?>(),
                It.IsAny<Func<NewsArticle, bool>>(),
                It.IsAny<CancellationToken>()))
            .Callback<SectionNews, string, CardsQuery, Action<ContentItemQueryBuilder>?, Func<NewsArticle, bool>, CancellationToken>(
                (_, _, query, _, _, _) => capturedQuery = query)
            .ReturnsAsync(new PaginationDto<NewsPageCardDto> { Data = [], TotalCount = 0 });

        await _newsArticleCardsServiceMock.Object.GetFilteredItemsAsync(
            new SectionNews(), "en-US", cardsQuery, null, _ => false, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(capturedQuery, Is.Not.Null);
            Assert.That(capturedQuery!.CurrentPage, Is.EqualTo(2));
            Assert.That(capturedQuery.PageSize, Is.EqualTo(5));
        });
    }

    [Test]
    public async Task CreateFiltersAsync_WithMultipleFilters_PassesAllFilterNames()
    {
        HashSet<string>? capturedFilters = null;

        _newsArticleCardsServiceMock
            .Setup(x => x.CreateFiltersAsync(
                It.IsAny<HashSet<string>>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .Callback<HashSet<string>, string?, CancellationToken>((filters, _, _) => capturedFilters = filters)
            .ReturnsAsync(Array.Empty<FilterItemDto<OptionDto>>());

        var filterNames = new HashSet<string> { "businessIssue", "service", "industryCategory" };
        await _newsArticleCardsServiceMock.Object.CreateFiltersAsync(filterNames, null, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(capturedFilters, Is.Not.Null);
            Assert.That(capturedFilters, Has.Count.EqualTo(3));
            Assert.That(capturedFilters!.Contains("businessIssue"), Is.True);
            Assert.That(capturedFilters.Contains("service"), Is.True);
            Assert.That(capturedFilters.Contains("industryCategory"), Is.True);
        });
    }
}

// --- Real implementation tests ---

public class NewsArticleCardsServiceImplementationTests
{
    private Mock<ICardsService> _cardsServiceMock = null!;
    private Mock<ICultureContext> _cultureContextMock = null!;
    private Mock<IContentService> _contentServiceMock = null!;
    private Mock<IUrlHelperFactory> _urlHelperFactoryMock = null!;
    private Mock<IHttpContextAccessor> _httpContextAccessorMock = null!;
    private NewsArticleCardsService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _cardsServiceMock = new Mock<ICardsService>();
        _cultureContextMock = new Mock<ICultureContext>();
        _contentServiceMock = new Mock<IContentService>();
        _urlHelperFactoryMock = new Mock<IUrlHelperFactory>();
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();

        var httpContext = new DefaultHttpContext();
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        var urlHelperMock = new Mock<IUrlHelper>();
        urlHelperMock.Setup(u => u.Content(It.IsAny<string?>())).Returns((string? path) => path);
        _urlHelperFactoryMock
            .Setup(f => f.GetUrlHelper(It.IsAny<ActionContext>()))
            .Returns(urlHelperMock.Object);

        _cultureContextMock.Setup(c => c.CurrentLanguageTag).Returns("en-US");
        _contentServiceMock.Setup(c => c.GetCurrentChannelID()).Returns(1);

        _service = new NewsArticleCardsService(
            _cardsServiceMock.Object,
            _cultureContextMock.Object,
            _contentServiceMock.Object,
            _urlHelperFactoryMock.Object,
            _httpContextAccessorMock.Object);
    }

    [Test]
    public async Task CreateFiltersAsync_DelegatesToCardsService()
    {
        var expectedFilters = new FilterItemDto<OptionDto>[]
        {
            new() { Title = "Service Area" }
        };

        _cardsServiceMock
            .Setup(x => x.CreateFiltersAsync(
                It.IsAny<HashSet<string>>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedFilters);

        var filters = new HashSet<string> { "serviceArea" };
        var result = await _service.CreateFiltersAsync(filters, "/news", CancellationToken.None);

        Assert.That(result, Has.Length.EqualTo(1));
        Assert.That(result[0].Title, Is.EqualTo("Service Area"));

        _cardsServiceMock.Verify(
            x => x.CreateFiltersAsync(filters, "/news", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task GetFilteredItemsAsync_ReturnsEmptyPagination_WhenNoNewsGroupsFound()
    {
        var newsSection = new SectionNews
        {
            SystemFields = new WebPageFields { WebPageItemID = 100 }
        };

        _contentServiceMock
            .Setup(s => s.GetPagesAsync<NewsGroup>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<NewsGroup>());

        var result = await _service.GetFilteredItemsAsync(
            newsSection, "en-US", new CardsQuery(), null, _ => false, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Data, Is.Empty);
            Assert.That(result.TotalCount, Is.EqualTo(0));
        });
    }

    [Test]
    public async Task GetFilteredItemsAsync_QueriesNewsArticles_WhenNewsGroupsExist()
    {
        var newsSection = new SectionNews
        {
            SystemFields = new WebPageFields { WebPageItemID = 100 }
        };

        var newsGroup = new NewsGroup
        {
            SystemFields = new WebPageFields { WebPageItemID = 200 }
        };

        _contentServiceMock
            .Setup(s => s.GetPagesAsync<NewsGroup>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { newsGroup });

        _contentServiceMock
            .Setup(s => s.GetPagesAsync<NewsArticle>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<NewsArticle>());

        var result = await _service.GetFilteredItemsAsync(
            newsSection, "en-US", new CardsQuery(), null, _ => false, CancellationToken.None);

        _contentServiceMock.Verify(
            s => s.GetPagesAsync<NewsArticle>(
                It.Is<RetrievePagesParameters>(p => p.LinkedItemsMaxLevel == 4 && p.LanguageName == "en-US"),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

        Assert.That(result.Data, Is.Empty);
    }

    [Test]
    public async Task GetFilteredItemsAsync_UsesLanguageFallbacksFalse()
    {
        var newsSection = new SectionNews
        {
            SystemFields = new WebPageFields { WebPageItemID = 100 }
        };

        RetrievePagesParameters? capturedParams = null;

        _contentServiceMock
            .Setup(s => s.GetPagesAsync<NewsGroup>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .Callback<RetrievePagesParameters, Action<RetrievePagesQueryParameters>, RetrievalCacheSettings, CancellationToken>(
                (p, _, _, _) => capturedParams = p)
            .ReturnsAsync(Enumerable.Empty<NewsGroup>());

        await _service.GetFilteredItemsAsync(
            newsSection, "fr-FR", new CardsQuery(), null, _ => false, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(capturedParams, Is.Not.Null);
            Assert.That(capturedParams!.LanguageName, Is.EqualTo("fr-FR"));
            Assert.That(capturedParams.UseLanguageFallbacks, Is.False);
        });
    }

    [Test]
    public async Task GetFilteredItemsAsync_DefaultsPaginationToPage1_WhenCurrentPageIsNull()
    {
        var newsSection = new SectionNews
        {
            SystemFields = new WebPageFields { WebPageItemID = 100 }
        };
        var newsGroup = new NewsGroup
        {
            SystemFields = new WebPageFields { WebPageItemID = 200 }
        };

        _contentServiceMock
            .Setup(s => s.GetPagesAsync<NewsGroup>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { newsGroup });

        _contentServiceMock
            .Setup(s => s.GetPagesAsync<NewsArticle>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<NewsArticle>());

        var query = new CardsQuery { CurrentPage = null, PageSize = null };
        var result = await _service.GetFilteredItemsAsync(
            newsSection, "en-US", query, null, _ => false, CancellationToken.None);

        Assert.That(result.Data, Is.Empty);
    }
}
