using BDO.GWT.Core.Constants;
using BDO.GWT.Core.Data.Queries;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Cards;
using BDO.GWT.Core.Services;
using CMS.ContentEngine;
using CMS.Websites;
using Kentico.Content.Web.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Moq;

namespace BDO.GWT.Core.Tests.Services;

public class NewsArticleCardsServiceTests
{
    private Mock<ICardsService> _cardsServiceMock = null!;
    private Mock<ICultureContext> _cultureContextMock = null!;
    private Mock<IContentService> _contentServiceMock = null!;
    private Mock<IUrlHelperFactory> _urlHelperFactoryMock = null!;
    private Mock<IHttpContextAccessor> _httpContextAccessorMock = null!;
    private Mock<IWebPageUrlResolver> _webPageUrlResolverMock = null!;
    private Mock<IUrlHelper> _urlHelperMock = null!;
    private NewsArticleCardsService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _cardsServiceMock = new Mock<ICardsService>();
        _cultureContextMock = new Mock<ICultureContext>();
        _contentServiceMock = new Mock<IContentService>();
        _urlHelperFactoryMock = new Mock<IUrlHelperFactory>();
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _webPageUrlResolverMock = new Mock<IWebPageUrlResolver>();

        var httpContext = new DefaultHttpContext();
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        _urlHelperMock = new Mock<IUrlHelper>();
        _urlHelperMock.Setup(u => u.Content(It.IsAny<string?>())).Returns((string? path) => path);
        _urlHelperFactoryMock
            .Setup(f => f.GetUrlHelper(It.IsAny<ActionContext>()))
            .Returns(_urlHelperMock.Object);

        _cultureContextMock.Setup(c => c.CurrentLanguageTag).Returns("en-US");
        _contentServiceMock.Setup(c => c.GetCurrentChannelID()).Returns(1);

        _webPageUrlResolverMock
            .Setup(r => r.GetAbsoluteUrl(It.IsAny<IWebPageFieldsSource>()))
            .Returns("https://example.com/article");

        _service = new NewsArticleCardsService(
            _cardsServiceMock.Object,
            _cultureContextMock.Object,
            _contentServiceMock.Object,
            _urlHelperFactoryMock.Object,
            _httpContextAccessorMock.Object,
            _webPageUrlResolverMock.Object);
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

        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Length.EqualTo(1));
            Assert.That(result[0].Title, Is.EqualTo("Service Area"));
        });

        _cardsServiceMock.Verify(
            x => x.CreateFiltersAsync(filters, "/news", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task CreateFiltersAsync_WhenNoFilters_ReturnsEmptyArray()
    {
        _cardsServiceMock
            .Setup(x => x.CreateFiltersAsync(
                It.IsAny<HashSet<string>>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<FilterItemDto<OptionDto>>());

        var result = await _service.CreateFiltersAsync(new HashSet<string>(), null, CancellationToken.None);

        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task CreateFiltersAsync_PassesNullNodeAliasPath_ToCardsService()
    {
        _cardsServiceMock
            .Setup(x => x.CreateFiltersAsync(
                It.IsAny<HashSet<string>>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<FilterItemDto<OptionDto>>());

        await _service.CreateFiltersAsync(new HashSet<string>(), null, CancellationToken.None);

        _cardsServiceMock.Verify(
            x => x.CreateFiltersAsync(It.IsAny<HashSet<string>>(), null, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task CreateFiltersAsync_PassesCancellationToken_ToCardsService()
    {
        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        _cardsServiceMock
            .Setup(x => x.CreateFiltersAsync(
                It.IsAny<HashSet<string>>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<FilterItemDto<OptionDto>>());

        await _service.CreateFiltersAsync(new HashSet<string>(), null, token);

        _cardsServiceMock.Verify(
            x => x.CreateFiltersAsync(It.IsAny<HashSet<string>>(), It.IsAny<string?>(), token),
            Times.Once);
    }
    [Test]
    public async Task GetFilteredItemsAsync_ReturnsEmptyPagination_WhenNoNewsGroupsFound()
    {
        var newsSection = CreateSectionNews(100);

        SetupNewsGroupReturns(Enumerable.Empty<NewsGroup>());

        var result = await _service.GetFilteredItemsAsync(
            [newsSection], "en-US", new CardsQuery(), null, _ => false, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Data, Is.Empty);
            Assert.That(result.TotalCount, Is.EqualTo(0));
        });
    }

    [Test]
    public async Task GetFilteredItemsAsync_ReturnsEmptyPagination_WhenNewsGroupsReturnsNull()
    {
        var newsSection = CreateSectionNews(100);

        _contentServiceMock
            .Setup(s => s.GetPagesAsync<NewsGroup>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((IEnumerable<NewsGroup>)null!);

        var result = await _service.GetFilteredItemsAsync(
            [newsSection], "en-US", new CardsQuery(), null, _ => false, CancellationToken.None);

        Assert.That(result.Data, Is.Empty);
        _contentServiceMock.Verify(
            s => s.GetPagesAsync<NewsArticle>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task GetFilteredItemsAsync_AggregatesNewsGroups_FromMultipleSections()
    {
        var section1 = CreateSectionNews(100);
        var section2 = CreateSectionNews(101);

        _contentServiceMock
            .SetupSequence(s => s.GetPagesAsync<NewsGroup>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { CreateNewsGroup(200) })
            .ReturnsAsync(new[] { CreateNewsGroup(201) });

        SetupNewsArticleReturns(Enumerable.Empty<NewsArticle>());

        await _service.GetFilteredItemsAsync(
            [section1, section2], "en-US", new CardsQuery(), null, _ => false, CancellationToken.None);

        _contentServiceMock.Verify(
            s => s.GetPagesAsync<NewsGroup>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()),
            Times.Exactly(2));

        _contentServiceMock.Verify(
            s => s.GetPagesAsync<NewsArticle>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
    [Test]
    public async Task GetFilteredItemsAsync_QueriesNewsArticles_WithExpectedParameters()
    {
        var newsSection = CreateSectionNews(100);

        SetupNewsGroupReturns(new[] { CreateNewsGroup(200) });
        SetupNewsArticleReturns(Enumerable.Empty<NewsArticle>());

        await _service.GetFilteredItemsAsync(
            [newsSection], "en-US", new CardsQuery(), null, _ => false, CancellationToken.None);

        _contentServiceMock.Verify(
            s => s.GetPagesAsync<NewsArticle>(
                It.Is<RetrievePagesParameters>(p =>
                    p.LinkedItemsMaxLevel == 4 &&
                    p.LanguageName == "en-US" &&
                    p.UseLanguageFallbacks == false),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task GetFilteredItemsAsync_UsesLanguageFallbacksFalse_OnNewsGroupQuery()
    {
        var newsSection = CreateSectionNews(100);

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
            [newsSection], "fr-FR", new CardsQuery(), null, _ => false, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(capturedParams, Is.Not.Null);
            Assert.That(capturedParams!.LanguageName, Is.EqualTo("fr-FR"));
            Assert.That(capturedParams.UseLanguageFallbacks, Is.False);
        });
    }

    [Test]
    public async Task GetFilteredItemsAsync_PassesCancellationToken_ToContentService()
    {
        using var cts = new CancellationTokenSource();
        var token = cts.Token;
        var newsSection = CreateSectionNews(100);

        SetupNewsGroupReturns(new[] { CreateNewsGroup(200) });
        SetupNewsArticleReturns(Enumerable.Empty<NewsArticle>());

        await _service.GetFilteredItemsAsync(
            [newsSection], "en-US", new CardsQuery(), null, _ => false, token);

        _contentServiceMock.Verify(
            s => s.GetPagesAsync<NewsGroup>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                token),
            Times.Once);

        _contentServiceMock.Verify(
            s => s.GetPagesAsync<NewsArticle>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                token),
            Times.Once);
    }
    [Test]
    public async Task GetFilteredItemsAsync_DefaultsToPage1AndPageSize10_WhenQueryHasNoPaging()
    {
        var newsSection = CreateSectionNews(100);
        var articles = Enumerable.Range(1, 25).Select(i => CreateNewsArticle(i)).ToArray();

        SetupNewsGroupReturns(new[] { CreateNewsGroup(200) });
        SetupNewsArticleReturns(articles);

        var result = await _service.GetFilteredItemsAsync(
            [newsSection], "en-US", new CardsQuery(), null, _ => false, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Data, Has.Length.EqualTo(10));
            Assert.That(result.TotalCount, Is.EqualTo(10));
        });
    }

    [Test]
    public async Task GetFilteredItemsAsync_RespectsCustomPageSize()
    {
        var newsSection = CreateSectionNews(100);
        var articles = Enumerable.Range(1, 10).Select(i => CreateNewsArticle(i)).ToArray();

        SetupNewsGroupReturns(new[] { CreateNewsGroup(200) });
        SetupNewsArticleReturns(articles);

        var query = new CardsQuery { CurrentPage = 1, PageSize = 3 };
        var result = await _service.GetFilteredItemsAsync(
            [newsSection], "en-US", query, null, _ => false, CancellationToken.None);

        Assert.That(result.Data, Has.Length.EqualTo(3));
    }

    [Test]
    public async Task GetFilteredItemsAsync_RespectsCurrentPage()
    {
        var newsSection = CreateSectionNews(100);
        var articles = Enumerable.Range(1, 10)
            .Select(i => CreateNewsArticle(i, title: $"Article {i}"))
            .ToArray();

        SetupNewsGroupReturns(new[] { CreateNewsGroup(200) });
        SetupNewsArticleReturns(articles);

        var query = new CardsQuery { CurrentPage = 2, PageSize = 3 };
        var result = await _service.GetFilteredItemsAsync(
            [newsSection], "en-US", query, null, _ => false, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Data, Has.Length.EqualTo(3));
            Assert.That(result.Data[0].Title, Is.EqualTo("Article 4"));
        });
    }
    [Test]
    public async Task GetFilteredItemsAsync_OrdersFeaturedItemsFirst()
    {
        var newsSection = CreateSectionNews(100);
        var featured = CreateNewsArticle(1, title: "Featured");
        var regular = CreateNewsArticle(2, title: "Regular");

        SetupNewsGroupReturns(new[] { CreateNewsGroup(200) });
        SetupNewsArticleReturns(new[] { regular, featured });

        var result = await _service.GetFilteredItemsAsync(
            [newsSection], "en-US", new CardsQuery(),
            null,
            article => article.MetadataTitle == "Featured",
            CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Data[0].Title, Is.EqualTo("Featured"));
            Assert.That(result.Data[0].IsFeatured, Is.True);
            Assert.That(result.Data[1].Title, Is.EqualTo("Regular"));
            Assert.That(result.Data[1].IsFeatured, Is.False);
        });
    }

    [Test]
    public async Task GetFilteredItemsAsync_FiltersOutNullArticles()
    {
        var newsSection = CreateSectionNews(100);
        var validArticle = CreateNewsArticle(1, title: "Valid");

        SetupNewsGroupReturns(new[] { CreateNewsGroup(200) });
        SetupNewsArticleReturns(new[] { validArticle, null! });

        var result = await _service.GetFilteredItemsAsync(
            [newsSection], "en-US", new CardsQuery(), null, _ => false, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Data, Has.Length.EqualTo(1));
            Assert.That(result.Data[0].Title, Is.EqualTo("Valid"));
        });
    }

    [Test]
    public async Task GetFilteredItemsAsync_MapsNewsArticleFields_ToCardDto()
    {
        var newsSection = CreateSectionNews(100);
        var publishDate = new DateTime(2024, 6, 1);
        var article = CreateNewsArticle(1,
            title: "Mapped",
            description: "<p>Hello</p>",
            imageUrl: "/media/img.jpg",
            imageAlt: "alt-text",
            publishDate: publishDate);

        SetupNewsGroupReturns(new[] { CreateNewsGroup(200) });
        SetupNewsArticleReturns(new[] { article });

        _webPageUrlResolverMock
            .Setup(r => r.GetAbsoluteUrl(article))
            .Returns("https://example.com/news/mapped");

        var result = await _service.GetFilteredItemsAsync(
            [newsSection], "en-US", new CardsQuery(), null, _ => false, CancellationToken.None);

        var card = result.Data[0];
        Assert.Multiple(() =>
        {
            Assert.That(card.Title, Is.EqualTo("Mapped"));
            Assert.That(card.Description, Is.EqualTo("Hello"));
            Assert.That(card.ImageURL, Is.EqualTo("/media/img.jpg"));
            Assert.That(card.ImageAltText, Is.EqualTo("alt-text"));
            Assert.That(card.Date, Is.EqualTo(publishDate));
            Assert.That(card.UpdatedOnDate, Is.Null);
            Assert.That(card.isComingSoon, Is.False);
            Assert.That(card.ReadMoreLink, Is.Not.Null);
            Assert.That(card.ReadMoreLink.URL, Is.EqualTo("https://example.com/news/mapped"));
            Assert.That(card.ReadMoreLink.Title, Is.EqualTo(ResourceStringKey.LiveSite.ReadMore));
        });
    }

    [Test]
    public async Task GetFilteredItemsAsync_UsesFirstPublishedDate_WhenLastPublishedIsNull()
    {
        var newsSection = CreateSectionNews(100);
        var firstPublished = new DateTime(2024, 1, 1);
        var article = CreateNewsArticle(1, publishDate: null, firstPublishDate: firstPublished);

        SetupNewsGroupReturns(new[] { CreateNewsGroup(200) });
        SetupNewsArticleReturns(new[] { article });

        var result = await _service.GetFilteredItemsAsync(
            [newsSection], "en-US", new CardsQuery(), null, _ => false, CancellationToken.None);

        Assert.That(result.Data[0].Date, Is.EqualTo(firstPublished));
    }

    [Test]
    public async Task GetFilteredItemsAsync_SetsUpdatedOnDate_WhenNotMinValue()
    {
        var newsSection = CreateSectionNews(100);
        var updated = new DateTime(2024, 7, 1);
        var article = CreateNewsArticle(1, updatedOn: updated);

        SetupNewsGroupReturns(new[] { CreateNewsGroup(200) });
        SetupNewsArticleReturns(new[] { article });

        var result = await _service.GetFilteredItemsAsync(
            [newsSection], "en-US", new CardsQuery(), null, _ => false, CancellationToken.None);

        Assert.That(result.Data[0].UpdatedOnDate, Is.EqualTo(updated));
    }

    [Test]
    public async Task GetFilteredItemsAsync_MapsContentTypeName_WhenPresent()
    {
        var newsSection = CreateSectionNews(100);
        var article = CreateNewsArticle(1);
        article.ContentTypeFilter = new[]
        {
            new Contenttypes { Name = "Press Release" }
        };

        SetupNewsGroupReturns(new[] { CreateNewsGroup(200) });
        SetupNewsArticleReturns(new[] { article });

        var result = await _service.GetFilteredItemsAsync(
            [newsSection], "en-US", new CardsQuery(), null, _ => false, CancellationToken.None);

        Assert.That(result.Data[0].Type, Is.EqualTo("Press Release"));
    }

    [Test]
    public async Task GetFilteredItemsAsync_ContentTypeIsNull_WhenNoContentTypes()
    {
        var newsSection = CreateSectionNews(100);
        var article = CreateNewsArticle(1);
        article.ContentTypeFilter = Array.Empty<Contenttypes>();

        SetupNewsGroupReturns(new[] { CreateNewsGroup(200) });
        SetupNewsArticleReturns(new[] { article });

        var result = await _service.GetFilteredItemsAsync(
            [newsSection], "en-US", new CardsQuery(), null, _ => false, CancellationToken.None);

        Assert.That(result.Data[0].Type, Is.Null);
    }
    [Test]
    public async Task GetFilteredItemsAsync_AppliesQueryFilters_WhenProvided()
    {
        var newsSection = CreateSectionNews(100);

        SetupNewsGroupReturns(new[] { CreateNewsGroup(200) });
        SetupNewsArticleReturns(Enumerable.Empty<NewsArticle>());

        var queryFilters = new List<Action<WhereParameters>>
        {
            _ => { /* no-op */ }
        };

        var result = await _service.GetFilteredItemsAsync(
            [newsSection], "en-US", new CardsQuery(), queryFilters, _ => false, CancellationToken.None);

        Assert.That(result.Data, Is.Empty);

        _contentServiceMock.Verify(
            s => s.GetPagesAsync<NewsArticle>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task GetFilteredItemsAsync_HandlesNullQueryFilters()
    {
        var newsSection = CreateSectionNews(100);

        SetupNewsGroupReturns(new[] { CreateNewsGroup(200) });
        SetupNewsArticleReturns(Enumerable.Empty<NewsArticle>());

        var result = await _service.GetFilteredItemsAsync(
            [newsSection], "en-US", new CardsQuery(), null, _ => false, CancellationToken.None);

        Assert.That(result.Data, Is.Empty);
    }

    [Test]
    public async Task GetFilteredItemsAsync_AppliesUnknownFilterParameter_IgnoresIt()
    {
        var newsSection = CreateSectionNews(100);

        SetupNewsGroupReturns(new[] { CreateNewsGroup(200) });
        SetupNewsArticleReturns(Enumerable.Empty<NewsArticle>());

        var cardsQuery = new CardsQuery
        {
            Filters = new Dictionary<string, string>
            {
                ["unknownFilter"] = "value"
            }
        };

        var result = await _service.GetFilteredItemsAsync(
            [newsSection], "en-US", cardsQuery, null, _ => false, CancellationToken.None);

        Assert.That(result.Data, Is.Empty);

        _contentServiceMock.Verify(
            s => s.GetPageByTypeAndNameAsync(
                It.IsAny<Type>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task GetFilteredItemsAsync_AppliesPageFilter_WhenFilterParameterMatchesPageType()
    {
        var newsSection = CreateSectionNews(100);
        var matchedPage = new NewsArticle
        {
            SystemFields = new WebPageFields { ContentItemGUID = Guid.NewGuid() }
        };

        SetupNewsGroupReturns(new[] { CreateNewsGroup(200) });
        SetupNewsArticleReturns(Enumerable.Empty<NewsArticle>());

        _contentServiceMock
            .Setup(s => s.GetPageByTypeAndNameAsync(
                It.IsAny<Type>(),
                "service-value",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(matchedPage);

        var cardsQuery = new CardsQuery
        {
            Filters = new Dictionary<string, string>
            {
                [FilterParameterName.Service] = "service-value"
            }
        };

        await _service.GetFilteredItemsAsync(
            [newsSection], "en-US", cardsQuery, null, _ => false, CancellationToken.None);

        _contentServiceMock.Verify(
            s => s.GetPageByTypeAndNameAsync(
                It.IsAny<Type>(),
                "service-value",
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task GetFilteredItemsAsync_SkipsPageFilter_WhenPageNotFound()
    {
        var newsSection = CreateSectionNews(100);

        SetupNewsGroupReturns(new[] { CreateNewsGroup(200) });
        SetupNewsArticleReturns(Enumerable.Empty<NewsArticle>());

        _contentServiceMock
            .Setup(s => s.GetPageByTypeAndNameAsync(
                It.IsAny<Type>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((IWebPageFieldsSource?)null);

        var cardsQuery = new CardsQuery
        {
            Filters = new Dictionary<string, string>
            {
                [FilterParameterName.BusinessLine] = "missing-value"
            }
        };

        var result = await _service.GetFilteredItemsAsync(
            [newsSection], "en-US", cardsQuery, null, _ => false, CancellationToken.None);

        Assert.That(result.Data, Is.Empty);
    }

    [Test]
    public async Task GetFilteredItemsAsync_AppliesTagPageFilter_WhenFilterParameterIsContentType()
    {
        var newsSection = CreateSectionNews(100);
        var matchedPage = new Contenttypes
        {
            SystemFields = new WebPageFields { ContentItemGUID = Guid.NewGuid() }
        };

        SetupNewsGroupReturns(new[] { CreateNewsGroup(200) });
        SetupNewsArticleReturns(Enumerable.Empty<NewsArticle>());

        _contentServiceMock
            .Setup(s => s.GetPageByTypeAndNameAsync(
                It.IsAny<Type>(),
                "press-release",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(matchedPage);

        var cardsQuery = new CardsQuery
        {
            Filters = new Dictionary<string, string>
            {
                [FilterParameterName.ContentType] = "press-release"
            }
        };

        await _service.GetFilteredItemsAsync(
            [newsSection], "en-US", cardsQuery, null, _ => false, CancellationToken.None);

        _contentServiceMock.Verify(
            s => s.GetPageByTypeAndNameAsync(
                It.IsAny<Type>(),
                "press-release",
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
    private void SetupNewsGroupReturns(IEnumerable<NewsGroup> data)
    {
        _contentServiceMock
            .Setup(s => s.GetPagesAsync<NewsGroup>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(data);
    }

    private void SetupNewsArticleReturns(IEnumerable<NewsArticle> data)
    {
        _contentServiceMock
            .Setup(s => s.GetPagesAsync<NewsArticle>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(data);
    }

    private static SectionNews CreateSectionNews(int webPageItemId) =>
        new()
        {
            SystemFields = new WebPageFields { WebPageItemID = webPageItemId }
        };

    private static NewsGroup CreateNewsGroup(int webPageItemId) =>
        new()
        {
            SystemFields = new WebPageFields { WebPageItemID = webPageItemId }
        };

    private static NewsArticle CreateNewsArticle(
        int id,
        string title = "Article",
        string description = "",
        string? imageUrl = null,
        string imageAlt = "",
        DateTime? publishDate = null,
        DateTime? firstPublishDate = null,
        DateTime? updatedOn = null)
    {
        return new NewsArticle
        {
            SystemFields = new WebPageFields
            {
                WebPageItemID = id,
                ContentItemID = id,
                ContentItemGUID = Guid.NewGuid(),
                ContentItemCommonDataLastPublishedWhen = publishDate,
                ContentItemCommonDataFirstPublishedWhen = firstPublishDate ?? new DateTime(2024, 1, 1)
            },
            MetadataTitle = title,
            MetadataDescription = description,
            ImageFileM = imageUrl != null
                ? new[] { new ContentAttachment { ContentAttachmentAsset = new ContentItemAsset { Url = imageUrl } } }
                : Array.Empty<ContentAttachment>(),
            ImageFileAltTextM = imageAlt,
            ContentTypeFilter = Array.Empty<Contenttypes>(),
            UpdatedOn = updatedOn ?? DateTime.MinValue
        };
    }
}
