using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using BDO.GWT.Core.Constants;
using BDO.GWT.Core.Data.Queries;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Cards;
using BDO.GWT.Core.Services;
using CMS.ContentEngine;
using CMS.Websites;
using Kentico.Content.Web.Mvc;
using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;

namespace BDO.GWT.Core.Tests.Services;

[TestFixture]
public class RelatedContentCardServiceTests
{
    private Mock<IContentRetriever> _contentRetrieverMock;
    private Mock<ICardsService> _cardsServiceMock;
    private Mock<IWebPageUrlResolver> _webPageUrlResolverMock;
    private Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private RelatedContentCardService _sut;

    [SetUp]
    public void SetUp()
    {
        _contentRetrieverMock = new Mock<IContentRetriever>();
        _cardsServiceMock = new Mock<ICardsService>();
        _webPageUrlResolverMock = new Mock<IWebPageUrlResolver>();
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();

        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext)null!);
        _webPageUrlResolverMock
            .Setup(x => x.GetAbsoluteUrl(It.IsAny<IWebPageFieldsSource>()))
            .Returns("/some-url");

        _sut = new RelatedContentCardService(
            _contentRetrieverMock.Object,
            _cardsServiceMock.Object,
            _webPageUrlResolverMock.Object,
            _httpContextAccessorMock.Object);
    }

    // -------------------------------------------------------------------------
    // CreateFiltersAsync
    // -------------------------------------------------------------------------

    [Test]
    public async Task CreateFiltersAsync_DelegatesToCardsService()
    {
        var filterNames = new HashSet<string> { FilterParameterName.ContentType };
        var expected = new[] { new FilterItemDto<OptionDto>() };

        _cardsServiceMock
            .Setup(x => x.CreateFiltersAsync(filterNames, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _sut.CreateFiltersAsync(filterNames, null, CancellationToken.None);

        Assert.That(result, Is.EqualTo(expected));
    }

    // -------------------------------------------------------------------------
    // GetFilteredItemsAsync — argument validation
    // -------------------------------------------------------------------------

    [Test]
    public void GetFilteredItemsAsync_EmptyParentPath_ThrowsArgumentException()
    {
        var query = new RelatedContentCardsQuery
        {
            SelectedPageTypes = new List<string> { nameof(Insight) }
        };

        Assert.ThrowsAsync<ArgumentException>(() =>
            _sut.GetFilteredItemsAsync(query, string.Empty, "en-gb", null, _ => false, CancellationToken.None));
    }

    [Test]
    public void GetFilteredItemsAsync_NullParentPath_ThrowsArgumentException()
    {
        var query = new RelatedContentCardsQuery
        {
            SelectedPageTypes = new List<string> { nameof(Insight) }
        };

        Assert.ThrowsAsync<ArgumentException>(() =>
            _sut.GetFilteredItemsAsync(query, string.Empty, "en-gb", null, _ => false, CancellationToken.None));
    }

    // -------------------------------------------------------------------------
    // GetFilteredItemsAsync — SelectedPageTypes / retrieval fan-out
    // -------------------------------------------------------------------------

    [Test]
    public async Task GetFilteredItemsAsync_EmptySelectedPageTypes_ReturnsEmptyResult()
    {
        var query = new RelatedContentCardsQuery
        {
            SelectedPageTypes = new List<string>(),
            CurrentPage = 1,
            PageSize = 10
        };

        var result = await _sut.GetFilteredItemsAsync(query, "/", "en-gb", null, _ => false, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Data, Is.Empty);
            Assert.That(result.TotalCount, Is.EqualTo(0));
        });
    }

    [Test]
    public async Task GetFilteredItemsAsync_NullSelectedPageTypes_ReturnsEmptyResult()
    {
        var query = new RelatedContentCardsQuery
        {
            SelectedPageTypes = null,
            CurrentPage = 1,
            PageSize = 10
        };

        var result = await _sut.GetFilteredItemsAsync(query, "/", "en-gb", null, _ => false, CancellationToken.None);

        Assert.That(result.Data, Is.Empty);
    }

    [Test]
    public async Task GetFilteredItemsAsync_UnknownPageType_IgnoredGracefully()
    {
        var query = new RelatedContentCardsQuery
        {
            SelectedPageTypes = new List<string> { "UnknownType" },
            CurrentPage = 1,
            PageSize = 10
        };

        var result = await _sut.GetFilteredItemsAsync(query, "/", "en-gb", null, _ => false, CancellationToken.None);

        Assert.That(result.Data, Is.Empty);
    }

    [Test]
    public async Task GetFilteredItemsAsync_CallsRetrievePagesForEachSelectedType()
    {
        SetupInsightRetrieval(Array.Empty<Insight>());
        SetupBlogPostRetrieval(Array.Empty<BlogPost>());

        var query = new RelatedContentCardsQuery
        {
            SelectedPageTypes = new List<string> { nameof(Insight), nameof(BlogPost) },
            CurrentPage = 1,
            PageSize = 10
        };

        await _sut.GetFilteredItemsAsync(query, "/", "en-gb", null, _ => false, CancellationToken.None);

        _contentRetrieverMock.Verify(x =>
            x.RetrievePages<Insight, Insight>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<Func<IContentQueryDataContainer, Insight, Task<Insight>>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _contentRetrieverMock.Verify(x =>
            x.RetrievePages<BlogPost, BlogPost>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<Func<IContentQueryDataContainer, BlogPost, Task<BlogPost>>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // -------------------------------------------------------------------------
    // GetFilteredItemsAsync — language propagation
    // -------------------------------------------------------------------------

    [Test]
    public async Task GetFilteredItemsAsync_PassesLanguageToRetrievePagesParameters()
    {
        RetrievePagesParameters? capturedParams = null;

        _contentRetrieverMock
            .Setup(x => x.RetrievePages<Insight, Insight>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<Func<IContentQueryDataContainer, Insight, Task<Insight>>>(),
                It.IsAny<CancellationToken>()))
            .Callback<RetrievePagesParameters, Action<RetrievePagesQueryParameters>, RetrievalCacheSettings,
                Func<IContentQueryDataContainer, Insight, Task<Insight>>, CancellationToken>(
                (p, _, _, _, _) => capturedParams = p)
            .ReturnsAsync(Array.Empty<Insight>());

        var query = new RelatedContentCardsQuery
        {
            SelectedPageTypes = new List<string> { nameof(Insight) },
            CurrentPage = 1,
            PageSize = 10
        };

        await _sut.GetFilteredItemsAsync(query, "/", "fr-fr", null, _ => false, CancellationToken.None);

        Assert.That(capturedParams?.LanguageName, Is.EqualTo("fr-fr"));
    }

    [Test]
    public async Task GetFilteredItemsAsync_DifferentLanguages_PassedThroughForEachType()
    {
        RetrievePagesParameters? insightParams = null;
        RetrievePagesParameters? blogParams = null;

        _contentRetrieverMock
            .Setup(x => x.RetrievePages<Insight, Insight>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<Func<IContentQueryDataContainer, Insight, Task<Insight>>>(),
                It.IsAny<CancellationToken>()))
            .Callback<RetrievePagesParameters, Action<RetrievePagesQueryParameters>, RetrievalCacheSettings,
                Func<IContentQueryDataContainer, Insight, Task<Insight>>, CancellationToken>(
                (p, _, _, _, _) => insightParams = p)
            .ReturnsAsync(Array.Empty<Insight>());

        _contentRetrieverMock
            .Setup(x => x.RetrievePages<BlogPost, BlogPost>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<Func<IContentQueryDataContainer, BlogPost, Task<BlogPost>>>(),
                It.IsAny<CancellationToken>()))
            .Callback<RetrievePagesParameters, Action<RetrievePagesQueryParameters>, RetrievalCacheSettings,
                Func<IContentQueryDataContainer, BlogPost, Task<BlogPost>>, CancellationToken>(
                (p, _, _, _, _) => blogParams = p)
            .ReturnsAsync(Array.Empty<BlogPost>());

        var query = new RelatedContentCardsQuery
        {
            SelectedPageTypes = new List<string> { nameof(Insight), nameof(BlogPost) },
            CurrentPage = 1,
            PageSize = 10
        };

        await _sut.GetFilteredItemsAsync(query, "/", "es-es", null, _ => false, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(insightParams?.LanguageName, Is.EqualTo("es-es"));
            Assert.That(blogParams?.LanguageName, Is.EqualTo("es-es"));
        });
    }

    // -------------------------------------------------------------------------
    // GetFilteredItemsAsync — pagination
    // -------------------------------------------------------------------------

    [Test]
    public async Task GetFilteredItemsAsync_PaginatesCorrectly_FirstPage()
    {
        var insights = CreateInsights(15);
        SetupInsightRetrieval(insights);

        var query = new RelatedContentCardsQuery
        {
            SelectedPageTypes = new List<string> { nameof(Insight) },
            CurrentPage = 1,
            PageSize = 5
        };

        var result = await _sut.GetFilteredItemsAsync(query, "/", "en-gb", null, _ => false, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Data.Count, Is.EqualTo(5));
            Assert.That(result.TotalCount, Is.EqualTo(15));
        });
    }

    [Test]
    public async Task GetFilteredItemsAsync_PaginatesCorrectly_SecondPage()
    {
        var insights = CreateInsights(15);
        SetupInsightRetrieval(insights);

        var query = new RelatedContentCardsQuery
        {
            SelectedPageTypes = new List<string> { nameof(Insight) },
            CurrentPage = 2,
            PageSize = 5
        };

        var result = await _sut.GetFilteredItemsAsync(query, "/", "en-gb", null, _ => false, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Data.Count, Is.EqualTo(5));
            Assert.That(result.TotalCount, Is.EqualTo(15));
        });
    }

    [Test]
    public async Task GetFilteredItemsAsync_LastPagePartial_ReturnsRemainder()
    {
        var insights = CreateInsights(7);
        SetupInsightRetrieval(insights);

        var query = new RelatedContentCardsQuery
        {
            SelectedPageTypes = new List<string> { nameof(Insight) },
            CurrentPage = 2,
            PageSize = 5
        };

        var result = await _sut.GetFilteredItemsAsync(query, "/", "en-gb", null, _ => false, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Data.Count, Is.EqualTo(2));
            Assert.That(result.TotalCount, Is.EqualTo(7));
        });
    }

    [Test]
    public async Task GetFilteredItemsAsync_DefaultsCurrentPageToOne()
    {
        var insights = CreateInsights(3);
        SetupInsightRetrieval(insights);

        var query = new RelatedContentCardsQuery
        {
            SelectedPageTypes = new List<string> { nameof(Insight) },
            CurrentPage = null,
            PageSize = 10
        };

        var result = await _sut.GetFilteredItemsAsync(query, "/", "en-gb", null, _ => false, CancellationToken.None);

        Assert.That(result.Data.Count, Is.EqualTo(3));
    }

    [Test]
    public async Task GetFilteredItemsAsync_DefaultsPageSizeToTen()
    {
        var insights = CreateInsights(20);
        SetupInsightRetrieval(insights);

        var query = new RelatedContentCardsQuery
        {
            SelectedPageTypes = new List<string> { nameof(Insight) },
            CurrentPage = 1,
            PageSize = null
        };

        var result = await _sut.GetFilteredItemsAsync(query, "/", "en-gb", null, _ => false, CancellationToken.None);

        Assert.That(result.Data.Count, Is.EqualTo(10));
    }

    // -------------------------------------------------------------------------
    // GetFilteredItemsAsync — ordering
    // -------------------------------------------------------------------------

    [Test]
    public async Task GetFilteredItemsAsync_FeaturedItemsAppearFirst()
    {
        var featuredGuid = Guid.NewGuid();
        var regularGuid = Guid.NewGuid();

        var insights = new[]
        {
            CreateInsight(regularGuid, new InsightOptions { PublishedDate = DateTime.UtcNow.AddDays(-1) }),
            CreateInsight(featuredGuid, new InsightOptions { PublishedDate = DateTime.UtcNow.AddDays(-10) })
        };

        SetupInsightRetrieval(insights);

        var query = new RelatedContentCardsQuery
        {
            SelectedPageTypes = new List<string> { nameof(Insight) },
            CurrentPage = 1,
            PageSize = 10
        };

        var result = await _sut.GetFilteredItemsAsync(
            query, "/", "en-gb", null,
            page => page.SystemFields.ContentItemGUID == featuredGuid,
            CancellationToken.None);

        Assert.That(result.Data.First().IsFeatured, Is.True);
    }

    

    // -------------------------------------------------------------------------
    // GetFilteredItemsAsync — in-memory filtering
    // -------------------------------------------------------------------------

    [Test]
    public async Task GetFilteredItemsAsync_EmptyFilters_ReturnsAllPages()
    {
        var insights = CreateInsights(3);
        SetupInsightRetrieval(insights);

        var query = new RelatedContentCardsQuery
        {
            SelectedPageTypes = new List<string> { nameof(Insight) },
            Filters = new Dictionary<string, string>(),
            CurrentPage = 1,
            PageSize = 10
        };

        var result = await _sut.GetFilteredItemsAsync(query, "/", "en-gb", null, _ => false, CancellationToken.None);

        Assert.That(result.TotalCount, Is.EqualTo(3));
    }

    [Test]
    public async Task GetFilteredItemsAsync_FilterWithBlankValue_TreatedAsNoFilter()
    {
        var insights = CreateInsights(3);
        SetupInsightRetrieval(insights);

        var query = new RelatedContentCardsQuery
        {
            SelectedPageTypes = new List<string> { nameof(Insight) },
            Filters = new Dictionary<string, string> { { FilterParameterName.ContentType, "" } },
            CurrentPage = 1,
            PageSize = 10
        };

        var result = await _sut.GetFilteredItemsAsync(query, "/", "en-gb", null, _ => false, CancellationToken.None);

        Assert.That(result.TotalCount, Is.EqualTo(3));
    }

    [Test]
    public async Task GetFilteredItemsAsync_UnknownFilterParameterName_Ignored()
    {
        var insights = CreateInsights(3);
        SetupInsightRetrieval(insights);

        var query = new RelatedContentCardsQuery
        {
            SelectedPageTypes = new List<string> { nameof(Insight) },
            Filters = new Dictionary<string, string> { { "unknownFilter", "someValue" } },
            CurrentPage = 1,
            PageSize = 10
        };

        var result = await _sut.GetFilteredItemsAsync(query, "/", "en-gb", null, _ => false, CancellationToken.None);

        Assert.That(result.TotalCount, Is.EqualTo(3));
    }

    [Test]
    public async Task GetFilteredItemsAsync_MultiplePageTypes_ResultsMerged()
    {
        var insights = CreateInsights(3);
        var blogPosts = CreateBlogPosts(2);

        SetupInsightRetrieval(insights);
        SetupBlogPostRetrieval(blogPosts);

        var query = new RelatedContentCardsQuery
        {
            SelectedPageTypes = new List<string> { nameof(Insight), nameof(BlogPost) },
            CurrentPage = 1,
            PageSize = 10
        };

        var result = await _sut.GetFilteredItemsAsync(query, "/", "en-gb", null, _ => false, CancellationToken.None);

        Assert.That(result.TotalCount, Is.EqualTo(5));
    }

    // -------------------------------------------------------------------------
    // GetFilteredItemsAsync — decorateQuery
    // -------------------------------------------------------------------------

    [Test]
    public async Task GetFilteredItemsAsync_DecorateQueryIsInvoked()
    {
        bool decorateCalled = false;
        Action<RetrievePagesQueryParameters> decorateQuery = _ => decorateCalled = true;

        Action<RetrievePagesQueryParameters>? capturedQueryAction = null;

        _contentRetrieverMock
            .Setup(x => x.RetrievePages<Insight, Insight>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<Func<IContentQueryDataContainer, Insight, Task<Insight>>>(),
                It.IsAny<CancellationToken>()))
            .Callback<RetrievePagesParameters, Action<RetrievePagesQueryParameters>, RetrievalCacheSettings,
                Func<IContentQueryDataContainer, Insight, Task<Insight>>, CancellationToken>(
                (_, queryAction, _, _, _) => capturedQueryAction = queryAction)
            .ReturnsAsync(Array.Empty<Insight>());

        var query = new RelatedContentCardsQuery
        {
            SelectedPageTypes = new List<string> { nameof(Insight) },
            CurrentPage = 1,
            PageSize = 10
        };

        await _sut.GetFilteredItemsAsync(query, "/", "en-gb", decorateQuery, _ => false, CancellationToken.None);

        Assert.That(capturedQueryAction, Is.Not.Null);
        decorateQuery(null!);
        Assert.That(decorateCalled, Is.True);
    }

    // -------------------------------------------------------------------------
    // GetFilteredItemsAsync — additional page-type retrieval branches
    // -------------------------------------------------------------------------

    [Test]
    public async Task GetFilteredItemsAsync_NewsArticleType_CallsRetrievePages()
    {
        SetupRetrieval<NewsArticle>(Array.Empty<NewsArticle>());

        var query = new RelatedContentCardsQuery
        {
            SelectedPageTypes = new List<string> { nameof(NewsArticle) },
            CurrentPage = 1,
            PageSize = 10
        };

        var result = await _sut.GetFilteredItemsAsync(query, "/", "en-gb", null, _ => false, CancellationToken.None);

        Assert.That(result.TotalCount, Is.EqualTo(0));
        _contentRetrieverMock.Verify(x =>
            x.RetrievePages<NewsArticle, NewsArticle>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<Func<IContentQueryDataContainer, NewsArticle, Task<NewsArticle>>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task GetFilteredItemsAsync_EventType_CallsRetrievePages()
    {
        SetupRetrieval<Event>(Array.Empty<Event>());

        var query = new RelatedContentCardsQuery
        {
            SelectedPageTypes = new List<string> { nameof(Event) },
            CurrentPage = 1,
            PageSize = 10
        };

        await _sut.GetFilteredItemsAsync(query, "/", "en-gb", null, _ => false, CancellationToken.None);

        _contentRetrieverMock.Verify(x =>
            x.RetrievePages<Event, Event>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<Func<IContentQueryDataContainer, Event, Task<Event>>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task GetFilteredItemsAsync_WebinarType_CallsRetrievePages()
    {
        SetupRetrieval<Webinar>(Array.Empty<Webinar>());

        var query = new RelatedContentCardsQuery
        {
            SelectedPageTypes = new List<string> { nameof(Webinar) },
            CurrentPage = 1,
            PageSize = 10
        };

        await _sut.GetFilteredItemsAsync(query, "/", "en-gb", null, _ => false, CancellationToken.None);

        _contentRetrieverMock.Verify(x =>
            x.RetrievePages<Webinar, Webinar>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<Func<IContentQueryDataContainer, Webinar, Task<Webinar>>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task GetFilteredItemsAsync_MicrositePageType_CallsRetrievePages()
    {
        SetupRetrieval<MicrositePage>(Array.Empty<MicrositePage>());

        var query = new RelatedContentCardsQuery
        {
            SelectedPageTypes = new List<string> { nameof(MicrositePage) },
            CurrentPage = 1,
            PageSize = 10
        };

        await _sut.GetFilteredItemsAsync(query, "/", "en-gb", null, _ => false, CancellationToken.None);

        _contentRetrieverMock.Verify(x =>
            x.RetrievePages<MicrositePage, MicrositePage>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<Func<IContentQueryDataContainer, MicrositePage, Task<MicrositePage>>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // -------------------------------------------------------------------------
    // GetFilteredItemsAsync — filter predicates
    // -------------------------------------------------------------------------

    [Test]
    public async Task GetFilteredItemsAsync_ContentTypeFilter_MatchesInsight()
    {
        var contentType = CreateContentType("Article");
        var matching = CreateInsight(Guid.NewGuid(), new InsightOptions { Title = "Match", ContentTypes = new[] { contentType } });
        var other = CreateInsight(Guid.NewGuid(), new InsightOptions { Title = "NoMatch", ContentTypes = new[] { CreateContentType("Video") } });
        SetupInsightRetrieval(new[] { matching, other });

        var query = new RelatedContentCardsQuery
        {
            SelectedPageTypes = new List<string> { nameof(Insight) },
            Filters = new Dictionary<string, string> { { FilterParameterName.ContentType, "Article" } },
            CurrentPage = 1,
            PageSize = 10
        };

        var result = await _sut.GetFilteredItemsAsync(query, "/", "en-gb", null, _ => false, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.TotalCount, Is.EqualTo(1));
            Assert.That(result.Data.Single().Title, Is.EqualTo("Match"));
        });
    }

    [Test]
    public async Task GetFilteredItemsAsync_BusinessLineFilter_MatchesInsight()
    {
        var bl = CreateNamedLinkedItem<BusinessLine>("Audit");
        var matching = CreateInsight(Guid.NewGuid(), new InsightOptions { Title = "Match", BusinessLines = new[] { bl } });
        var other = CreateInsight(Guid.NewGuid(), new InsightOptions { Title = "NoMatch" });
        SetupInsightRetrieval(new[] { matching, other });

        var query = new RelatedContentCardsQuery
        {
            SelectedPageTypes = new List<string> { nameof(Insight) },
            Filters = new Dictionary<string, string> { { FilterParameterName.BusinessLine, "Audit" } },
            CurrentPage = 1,
            PageSize = 10
        };

        var result = await _sut.GetFilteredItemsAsync(query, "/", "en-gb", null, _ => false, CancellationToken.None);

        Assert.That(result.TotalCount, Is.EqualTo(1));
    }

    [Test]
    public async Task GetFilteredItemsAsync_ServiceAreaFilter_MatchesInsight()
    {
        var sa = CreateNamedLinkedItem<ServiceArea>("Tax");
        var matching = CreateInsight(Guid.NewGuid(), new InsightOptions { ServiceAreas = new[] { sa } });
        var other = CreateInsight(Guid.NewGuid());
        SetupInsightRetrieval(new[] { matching, other });

        var query = new RelatedContentCardsQuery
        {
            SelectedPageTypes = new List<string> { nameof(Insight) },
            Filters = new Dictionary<string, string> { { FilterParameterName.ServiceArea, "Tax" } },
            CurrentPage = 1,
            PageSize = 10
        };

        var result = await _sut.GetFilteredItemsAsync(query, "/", "en-gb", null, _ => false, CancellationToken.None);

        Assert.That(result.TotalCount, Is.EqualTo(1));
    }

    [Test]
    public async Task GetFilteredItemsAsync_IndustryCategoryFilter_MatchesInsight()
    {
        var ic = CreateNamedLinkedItem<IndustryCategory>("Retail");
        var matching = CreateInsight(Guid.NewGuid(), new InsightOptions { IndustryCategories = new[] { ic } });
        var other = CreateInsight(Guid.NewGuid());
        SetupInsightRetrieval(new[] { matching, other });

        var query = new RelatedContentCardsQuery
        {
            SelectedPageTypes = new List<string> { nameof(Insight) },
            Filters = new Dictionary<string, string> { { FilterParameterName.IndustryCategory, "Retail" } },
            CurrentPage = 1,
            PageSize = 10
        };

        var result = await _sut.GetFilteredItemsAsync(query, "/", "en-gb", null, _ => false, CancellationToken.None);

        Assert.That(result.TotalCount, Is.EqualTo(1));
    }

    [Test]
    public async Task GetFilteredItemsAsync_ContentTypeFilter_NoMatch_ReturnsEmpty()
    {
        var insight = CreateInsight(Guid.NewGuid(), new InsightOptions { ContentTypes = new[] { CreateContentType("Video") } });
        SetupInsightRetrieval(new[] { insight });

        var query = new RelatedContentCardsQuery
        {
            SelectedPageTypes = new List<string> { nameof(Insight) },
            Filters = new Dictionary<string, string> { { FilterParameterName.ContentType, "Article" } },
            CurrentPage = 1,
            PageSize = 10
        };

        var result = await _sut.GetFilteredItemsAsync(query, "/", "en-gb", null, _ => false, CancellationToken.None);

        Assert.That(result.TotalCount, Is.EqualTo(0));
    }

    // -------------------------------------------------------------------------
    // Card DTO mapping — Insight
    // -------------------------------------------------------------------------

    [Test]
    public async Task GetFilteredItemsAsync_InsightCard_MapsMetadataTitleToCardTitle()
    {
        var insight = CreateInsight(Guid.NewGuid(), new InsightOptions { Title = "My Insight Title" });
        SetupInsightRetrieval(new[] { insight });

        var query = BasicQuery(nameof(Insight));
        var result = await _sut.GetFilteredItemsAsync(query, "/", "en-gb", null, _ => false, CancellationToken.None);

        Assert.That(result.Data.Single().Title, Is.EqualTo("My Insight Title"));
    }

    [Test]
    public async Task GetFilteredItemsAsync_InsightCard_MapsComingSoon()
    {
        var insight = CreateInsight(Guid.NewGuid(), new InsightOptions { ComingSoon = true });
        SetupInsightRetrieval(new[] { insight });

        var result = await _sut.GetFilteredItemsAsync(BasicQuery(nameof(Insight)), "/", "en-gb", null, _ => false, CancellationToken.None);

        Assert.That(result.Data.Single().isComingSoon, Is.True);
    }

    [Test]
    public async Task GetFilteredItemsAsync_BlogPostCard_IsComingSoonAlwaysFalse()
    {
        var blogPost = CreateBlogPost(Guid.NewGuid());
        SetupRetrieval<BlogPost>(new[] { blogPost });

        var result = await _sut.GetFilteredItemsAsync(BasicQuery(nameof(BlogPost)), "/", "en-gb", null, _ => false, CancellationToken.None);

        Assert.That(result.Data.Single().isComingSoon, Is.False);
    }

    [Test]
    public async Task GetFilteredItemsAsync_InsightCard_MapsContentTypeAndDateAndDescription()
    {
        var publishDate = DateTime.UtcNow.AddDays(-2);
        var insight = CreateInsight(Guid.NewGuid(), new InsightOptions
        {
            PublishedDate = publishDate,
            Title = "Card",
            ContentTypes = new[] { CreateContentType("Article") },
            Description = "<p>Some description</p>"
        });
        SetupInsightRetrieval(new[] { insight });

        var result = await _sut.GetFilteredItemsAsync(BasicQuery(nameof(Insight)), "/", "en-gb", null, _ => false, CancellationToken.None);
        var card = result.Data.Single();

        Assert.Multiple(() =>
        {
            Assert.That(card.Type, Is.EqualTo("Article"));
            Assert.That(card.Date, Is.EqualTo(publishDate));
            Assert.That(card.Description, Does.Contain("Some description"));
            Assert.That(card.ReadMoreLink.Title, Is.EqualTo(ResourceStringKey.LiveSite.ReadMore));
        });
    }

    [Test]
    public async Task GetFilteredItemsAsync_FeaturedPage_IsFeaturedTrueOnCard()
    {
        var guid = Guid.NewGuid();
        var insight = CreateInsight(guid);
        SetupInsightRetrieval(new[] { insight });

        var result = await _sut.GetFilteredItemsAsync(
            BasicQuery(nameof(Insight)), "/", "en-gb", null,
            page => page.SystemFields.ContentItemGUID == guid,
            CancellationToken.None);

        Assert.That(result.Data.Single().IsFeatured, Is.True);
    }

    [Test]
    public async Task GetFilteredItemsAsync_NonFeaturedPage_IsFeaturedFalseOnCard()
    {
        var insight = CreateInsight(Guid.NewGuid());
        SetupInsightRetrieval(new[] { insight });

        var result = await _sut.GetFilteredItemsAsync(
            BasicQuery(nameof(Insight)), "/", "en-gb", null, _ => false, CancellationToken.None);

        Assert.That(result.Data.Single().IsFeatured, Is.False);
    }

    [Test]
    public async Task GetFilteredItemsAsync_ReadMoreLinkSetOnCard()
    {
        var insight = CreateInsight(Guid.NewGuid());
        SetupInsightRetrieval(new[] { insight });

        _webPageUrlResolverMock
            .Setup(x => x.GetAbsoluteUrl(It.IsAny<IWebPageFieldsSource>()))
            .Returns("/my-insight");

        var result = await _sut.GetFilteredItemsAsync(BasicQuery(nameof(Insight)), "/", "en-gb", null, _ => false, CancellationToken.None);

        Assert.That(result.Data.Single().ReadMoreLink.URL, Is.EqualTo("/my-insight"));
    }

    // -------------------------------------------------------------------------
    // Card DTO mapping — BlogPost
    // -------------------------------------------------------------------------

    [Test]
    public async Task GetFilteredItemsAsync_BlogPostCard_MapsMetadataTitleToCardTitle()
    {
        var blogPost = CreateBlogPost(Guid.NewGuid(), title: "My Blog Title");
        SetupBlogPostRetrieval(new[] { blogPost });

        var result = await _sut.GetFilteredItemsAsync(BasicQuery(nameof(BlogPost)), "/", "en-gb", null, _ => false, CancellationToken.None);

        Assert.That(result.Data.Single().Title, Is.EqualTo("My Blog Title"));
    }

    [Test]
    public async Task GetFilteredItemsAsync_BlogPostCard_MapsContentTypeAndReadMore()
    {
        var blogPost = CreateBlogPost(Guid.NewGuid(), title: "Blog", contentTypes: new[] { CreateContentType("Post") });
        SetupBlogPostRetrieval(new[] { blogPost });

        _webPageUrlResolverMock
            .Setup(x => x.GetAbsoluteUrl(It.IsAny<IWebPageFieldsSource>()))
            .Returns("/blog-url");

        var result = await _sut.GetFilteredItemsAsync(BasicQuery(nameof(BlogPost)), "/", "en-gb", null, _ => false, CancellationToken.None);
        var card = result.Data.Single();

        Assert.Multiple(() =>
        {
            Assert.That(card.Type, Is.EqualTo("Post"));
            Assert.That(card.ReadMoreLink.URL, Is.EqualTo("/blog-url"));
        });
    }

    [Test]
    public async Task GetFilteredItemsAsync_BlogPostCard_ImageUrlMappedFromTeaserImage()
    {
        var blogPost = CreateBlogPost(Guid.NewGuid(), teaserImageUrl: "https://cdn.example.com/blog.jpg");
        SetupBlogPostRetrieval(new[] { blogPost });

        var result = await _sut.GetFilteredItemsAsync(BasicQuery(nameof(BlogPost)), "/", "en-gb", null, _ => false, CancellationToken.None);

        Assert.That(result.Data.Single().ImageURL, Is.EqualTo("https://cdn.example.com/blog.jpg"));
    }

    [Test]
    public async Task GetFilteredItemsAsync_BlogPostCard_NullTeaserImage_ImageUrlIsNull()
    {
        var blogPost = CreateBlogPost(Guid.NewGuid());
        SetupBlogPostRetrieval(new[] { blogPost });

        var result = await _sut.GetFilteredItemsAsync(BasicQuery(nameof(BlogPost)), "/", "en-gb", null, _ => false, CancellationToken.None);

        Assert.That(result.Data.Single().ImageURL, Is.Null);
    }

    // -------------------------------------------------------------------------
    // Card DTO mapping — NewsArticle
    // -------------------------------------------------------------------------

    [Test]
    public async Task GetFilteredItemsAsync_NewsArticleCard_MapsTitleAndReadMore()
    {
        var article = CreateNewsArticle(Guid.NewGuid(), title: "Breaking News");
        SetupRetrieval<NewsArticle>(new[] { article });

        _webPageUrlResolverMock
            .Setup(x => x.GetAbsoluteUrl(It.IsAny<IWebPageFieldsSource>()))
            .Returns("/news-url");

        var result = await _sut.GetFilteredItemsAsync(BasicQuery(nameof(NewsArticle)), "/", "en-gb", null, _ => false, CancellationToken.None);
        var card = result.Data.Single();

        Assert.Multiple(() =>
        {
            Assert.That(card.Title, Is.EqualTo("Breaking News"));
            Assert.That(card.ReadMoreLink.URL, Is.EqualTo("/news-url"));
        });
    }

    [Test]
    public async Task GetFilteredItemsAsync_NewsArticleCard_ImageUrlMappedFromTeaserImage()
    {
        var article = CreateNewsArticle(Guid.NewGuid(), teaserImageUrl: "https://cdn.example.com/news.jpg");
        SetupRetrieval<NewsArticle>(new[] { article });

        var result = await _sut.GetFilteredItemsAsync(BasicQuery(nameof(NewsArticle)), "/", "en-gb", null, _ => false, CancellationToken.None);

        Assert.That(result.Data.Single().ImageURL, Is.EqualTo("https://cdn.example.com/news.jpg"));
    }

    [Test]
    public async Task GetFilteredItemsAsync_NewsArticleCard_NullTeaserImage_ImageUrlIsNull()
    {
        var article = CreateNewsArticle(Guid.NewGuid());
        SetupRetrieval<NewsArticle>(new[] { article });

        var result = await _sut.GetFilteredItemsAsync(BasicQuery(nameof(NewsArticle)), "/", "en-gb", null, _ => false, CancellationToken.None);

        Assert.That(result.Data.Single().ImageURL, Is.Null);
    }

    // -------------------------------------------------------------------------
    // Card DTO mapping — Event
    // -------------------------------------------------------------------------

    [Test]
    public async Task GetFilteredItemsAsync_EventCard_MapsTitleAndReadMore()
    {
        var ev = CreateEvent(Guid.NewGuid(), title: "Annual Conference");
        SetupRetrieval<Event>(new[] { ev });

        _webPageUrlResolverMock
            .Setup(x => x.GetAbsoluteUrl(It.IsAny<IWebPageFieldsSource>()))
            .Returns("/event-url");

        var result = await _sut.GetFilteredItemsAsync(BasicQuery(nameof(Event)), "/", "en-gb", null, _ => false, CancellationToken.None);
        var card = result.Data.Single();

        Assert.Multiple(() =>
        {
            Assert.That(card.Title, Is.EqualTo("Annual Conference"));
            Assert.That(card.ReadMoreLink.URL, Is.EqualTo("/event-url"));
        });
    }

    [Test]
    public async Task GetFilteredItemsAsync_EventCard_ImageUrlMappedFromTeaserImage()
    {
        var ev = CreateEvent(Guid.NewGuid(), teaserImageUrl: "https://cdn.example.com/event.jpg");
        SetupRetrieval<Event>(new[] { ev });

        var result = await _sut.GetFilteredItemsAsync(BasicQuery(nameof(Event)), "/", "en-gb", null, _ => false, CancellationToken.None);

        Assert.That(result.Data.Single().ImageURL, Is.EqualTo("https://cdn.example.com/event.jpg"));
    }

    [Test]
    public async Task GetFilteredItemsAsync_EventCard_NullTeaserImage_ImageUrlIsNull()
    {
        var ev = CreateEvent(Guid.NewGuid());
        SetupRetrieval<Event>(new[] { ev });

        var result = await _sut.GetFilteredItemsAsync(BasicQuery(nameof(Event)), "/", "en-gb", null, _ => false, CancellationToken.None);

        Assert.That(result.Data.Single().ImageURL, Is.Null);
    }

    // -------------------------------------------------------------------------
    // Card DTO mapping — Webinar
    // -------------------------------------------------------------------------

    [Test]
    public async Task GetFilteredItemsAsync_WebinarCard_MapsTitleAndReadMore()
    {
        var webinar = CreateWebinar(Guid.NewGuid(), title: "Tech Webinar");
        SetupRetrieval<Webinar>(new[] { webinar });

        _webPageUrlResolverMock
            .Setup(x => x.GetAbsoluteUrl(It.IsAny<IWebPageFieldsSource>()))
            .Returns("/webinar-url");

        var result = await _sut.GetFilteredItemsAsync(BasicQuery(nameof(Webinar)), "/", "en-gb", null, _ => false, CancellationToken.None);
        var card = result.Data.Single();

        Assert.Multiple(() =>
        {
            Assert.That(card.Title, Is.EqualTo("Tech Webinar"));
            Assert.That(card.ReadMoreLink.URL, Is.EqualTo("/webinar-url"));
        });
    }

    [Test]
    public async Task GetFilteredItemsAsync_WebinarCard_ImageUrlMappedFromTeaserImage()
    {
        var webinar = CreateWebinar(Guid.NewGuid(), teaserImageUrl: "https://cdn.example.com/webinar.jpg");
        SetupRetrieval<Webinar>(new[] { webinar });

        var result = await _sut.GetFilteredItemsAsync(BasicQuery(nameof(Webinar)), "/", "en-gb", null, _ => false, CancellationToken.None);

        Assert.That(result.Data.Single().ImageURL, Is.EqualTo("https://cdn.example.com/webinar.jpg"));
    }

    [Test]
    public async Task GetFilteredItemsAsync_WebinarCard_NullTeaserImage_ImageUrlIsNull()
    {
        var webinar = CreateWebinar(Guid.NewGuid());
        SetupRetrieval<Webinar>(new[] { webinar });

        var result = await _sut.GetFilteredItemsAsync(BasicQuery(nameof(Webinar)), "/", "en-gb", null, _ => false, CancellationToken.None);

        Assert.That(result.Data.Single().ImageURL, Is.Null);
    }

    // -------------------------------------------------------------------------
    // Card DTO mapping — MicrositePage
    // -------------------------------------------------------------------------

    [Test]
    public async Task GetFilteredItemsAsync_MicrositePageCard_MapsTitleAndReadMore()
    {
        var microsite = CreateMicrositePage(Guid.NewGuid(), title: "Microsite Hub");
        SetupRetrieval<MicrositePage>(new[] { microsite });

        _webPageUrlResolverMock
            .Setup(x => x.GetAbsoluteUrl(It.IsAny<IWebPageFieldsSource>()))
            .Returns("/microsite-url");

        var result = await _sut.GetFilteredItemsAsync(BasicQuery(nameof(MicrositePage)), "/", "en-gb", null, _ => false, CancellationToken.None);
        var card = result.Data.Single();

        Assert.Multiple(() =>
        {
            Assert.That(card.Title, Is.EqualTo("Microsite Hub"));
            Assert.That(card.ReadMoreLink.URL, Is.EqualTo("/microsite-url"));
        });
    }

    [Test]
    public async Task GetFilteredItemsAsync_MicrositePageCard_ImageUrlMappedFromTeaserImage()
    {
        var microsite = CreateMicrositePage(Guid.NewGuid(), teaserImageUrl: "https://cdn.example.com/microsite.jpg");
        SetupRetrieval<MicrositePage>(new[] { microsite });

        var result = await _sut.GetFilteredItemsAsync(BasicQuery(nameof(MicrositePage)), "/", "en-gb", null, _ => false, CancellationToken.None);

        Assert.That(result.Data.Single().ImageURL, Is.EqualTo("https://cdn.example.com/microsite.jpg"));
    }

    [Test]
    public async Task GetFilteredItemsAsync_MicrositePageCard_NullTeaserImage_ImageUrlIsNull()
    {
        var microsite = CreateMicrositePage(Guid.NewGuid());
        SetupRetrieval<MicrositePage>(new[] { microsite });

        var result = await _sut.GetFilteredItemsAsync(BasicQuery(nameof(MicrositePage)), "/", "en-gb", null, _ => false, CancellationToken.None);

        Assert.That(result.Data.Single().ImageURL, Is.Null);
    }

    [Test]
    public async Task GetFilteredItemsAsync_MicrositePageCard_MapsContentTypeFromMicrositeContentType()
    {
        var microsite = CreateMicrositePage(Guid.NewGuid(), micrositeContentTypeName: "Hub");
        SetupRetrieval<MicrositePage>(new[] { microsite });

        var result = await _sut.GetFilteredItemsAsync(BasicQuery(nameof(MicrositePage)), "/", "en-gb", null, _ => false, CancellationToken.None);

        Assert.That(result.Data.Single().Type, Is.EqualTo("Hub"));
    }

    // -------------------------------------------------------------------------
    // GetFilteredItemsAsync — exception resilience
    // -------------------------------------------------------------------------

    [Test]
    public async Task GetFilteredItemsAsync_RetrievePagesThrowsInvalidOperationException_TreatedAsEmptyForThatType()
    {
        _contentRetrieverMock
            .Setup(x => x.RetrievePages<Insight, Insight>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<Func<IContentQueryDataContainer, Insight, Task<Insight>>>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Unbindable asset field"));

        var query = new RelatedContentCardsQuery
        {
            SelectedPageTypes = new List<string> { nameof(Insight) },
            CurrentPage = 1,
            PageSize = 10
        };

        var result = await _sut.GetFilteredItemsAsync(query, "/", "en-gb", null, _ => false, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Data, Is.Empty);
            Assert.That(result.TotalCount, Is.EqualTo(0));
        });
    }

    [Test]
    public async Task GetFilteredItemsAsync_OnePageTypeThrows_OtherPageTypesStillReturned()
    {
        _contentRetrieverMock
            .Setup(x => x.RetrievePages<Insight, Insight>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<Func<IContentQueryDataContainer, Insight, Task<Insight>>>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Bad asset"));

        SetupBlogPostRetrieval(CreateBlogPosts(2));

        var query = new RelatedContentCardsQuery
        {
            SelectedPageTypes = new List<string> { nameof(Insight), nameof(BlogPost) },
            CurrentPage = 1,
            PageSize = 10
        };

        var result = await _sut.GetFilteredItemsAsync(query, "/", "en-gb", null, _ => false, CancellationToken.None);

        Assert.That(result.TotalCount, Is.EqualTo(2));
    }

    // -------------------------------------------------------------------------
    // GetFilteredItemsAsync — Event/Webinar ordering by date
    // -------------------------------------------------------------------------

    [Test]
    public async Task GetFilteredItemsAsync_Events_OrderedBySoonestUpcomingFirst()
    {
        var soon = CreateEvent(Guid.NewGuid(), title: "Soon");
        SetProperty(soon, nameof(Event.EventDateFrom), DateTime.UtcNow.AddDays(1));

        var later = CreateEvent(Guid.NewGuid(), title: "Later");
        SetProperty(later, nameof(Event.EventDateFrom), DateTime.UtcNow.AddDays(30));

        SetupRetrieval<Event>(new[] { later, soon });

        var result = await _sut.GetFilteredItemsAsync(BasicQuery(nameof(Event)), "/", "en-gb", null, _ => false, CancellationToken.None);

        Assert.That(result.Data[0].Title, Is.EqualTo("Later"));
    }

    [Test]
    public async Task GetFilteredItemsAsync_EventCard_MapsDateToFromEventDateTo()
    {
        var from = DateTime.UtcNow.AddDays(1);
        var to = DateTime.UtcNow.AddDays(2);
        var ev = CreateEvent(Guid.NewGuid());
        SetProperty(ev, nameof(Event.EventDateFrom), from);
        SetProperty(ev, nameof(Event.EventDateTo), to);
        SetupRetrieval<Event>(new[] { ev });

        var result = await _sut.GetFilteredItemsAsync(BasicQuery(nameof(Event)), "/", "en-gb", null, _ => false, CancellationToken.None);
        var card = result.Data.Single();

        Assert.Multiple(() =>
        {
            Assert.That(card.Date, Is.EqualTo(from));
            Assert.That(card.DateTo, Is.EqualTo(to));
        });
    }

    [Test]
    public async Task GetFilteredItemsAsync_WebinarCard_MapsDateToFromEventDateTo()
    {
        var from = DateTime.UtcNow.AddDays(3);
        var to = DateTime.UtcNow.AddDays(4);
        var webinar = CreateWebinar(Guid.NewGuid());
        SetProperty(webinar, nameof(Webinar.EventDateFrom), from);
        SetProperty(webinar, nameof(Webinar.EventDateTo), to);
        SetupRetrieval<Webinar>(new[] { webinar });

        var result = await _sut.GetFilteredItemsAsync(BasicQuery(nameof(Webinar)), "/", "en-gb", null, _ => false, CancellationToken.None);
        var card = result.Data.Single();

        Assert.Multiple(() =>
        {
            Assert.That(card.Date, Is.EqualTo(from));
            Assert.That(card.DateTo, Is.EqualTo(to));
        });
    }

    // -------------------------------------------------------------------------
    // GetFilteredItemsAsync — image alt text mapping
    // -------------------------------------------------------------------------

    [Test]
    public async Task GetFilteredItemsAsync_BlogPostCard_MapsImageAltText()
    {
        var blogPost = CreateBlogPost(Guid.NewGuid(), teaserImageUrl: "https://cdn.example.com/blog.jpg");
        SetProperty(blogPost, nameof(BlogPost.MetadataTeaserImageAltText), "Blog Alt Text");
        SetupBlogPostRetrieval(new[] { blogPost });

        var result = await _sut.GetFilteredItemsAsync(BasicQuery(nameof(BlogPost)), "/", "en-gb", null, _ => false, CancellationToken.None);

        Assert.That(result.Data.Single().ImageAltText, Is.EqualTo("Blog Alt Text"));
    }

    // -------------------------------------------------------------------------
    // GetFilteredItemsAsync — MicrositePage filters
    // -------------------------------------------------------------------------

    [Test]
    public async Task GetFilteredItemsAsync_MicrositePage_BusinessLineFilter_Matches()
    {
        var bl = CreateNamedLinkedItem<BusinessLine>("Consulting");
        var matching = CreateMicrositePage(Guid.NewGuid(), title: "Match");
        SetProperty(matching, nameof(MicrositePage.BusinessLines), new[] { bl });
        var other = CreateMicrositePage(Guid.NewGuid(), title: "NoMatch");
        SetupRetrieval<MicrositePage>(new[] { matching, other });

        var query = new RelatedContentCardsQuery
        {
            SelectedPageTypes = new List<string> { nameof(MicrositePage) },
            Filters = new Dictionary<string, string> { { FilterParameterName.BusinessLine, "Consulting" } },
            CurrentPage = 1,
            PageSize = 10
        };

        var result = await _sut.GetFilteredItemsAsync(query, "/", "en-gb", null, _ => false, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.TotalCount, Is.EqualTo(1));
            Assert.That(result.Data.Single().Title, Is.EqualTo("Match"));
        });
    }

    // -------------------------------------------------------------------------
    // GetFilteredItemsAsync — combined filters (AND logic)
    // -------------------------------------------------------------------------

    [Test]
    public async Task GetFilteredItemsAsync_MultipleFilters_AppliedWithAndLogic()
    {
        var bl = CreateNamedLinkedItem<BusinessLine>("Audit");
        var sa = CreateNamedLinkedItem<ServiceArea>("Tax");

        var matchesBoth = CreateInsight(Guid.NewGuid(), new InsightOptions
        {
            Title = "Both",
            BusinessLines = new[] { bl },
            ServiceAreas = new[] { sa }
        });
        var matchesOnlyBusinessLine = CreateInsight(Guid.NewGuid(), new InsightOptions
        {
            Title = "OnlyBL",
            BusinessLines = new[] { bl }
        });
        SetupInsightRetrieval(new[] { matchesBoth, matchesOnlyBusinessLine });

        var query = new RelatedContentCardsQuery
        {
            SelectedPageTypes = new List<string> { nameof(Insight) },
            Filters = new Dictionary<string, string>
            {
                { FilterParameterName.BusinessLine, "Audit" },
                { FilterParameterName.ServiceArea, "Tax" }
            },
            CurrentPage = 1,
            PageSize = 10
        };

        var result = await _sut.GetFilteredItemsAsync(query, "/", "en-gb", null, _ => false, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.TotalCount, Is.EqualTo(1));
            Assert.That(result.Data.Single().Title, Is.EqualTo("Both"));
        });
    }

    // -------------------------------------------------------------------------
    // CreateFiltersAsync — additional coverage
    // -------------------------------------------------------------------------

    [Test]
    public async Task CreateFiltersAsync_PassesNodeAliasPathThrough()
    {
        var filterNames = new HashSet<string> { FilterParameterName.BusinessLine };
        string? capturedPath = null;

        _cardsServiceMock
            .Setup(x => x.CreateFiltersAsync(filterNames, It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .Callback<HashSet<string>, string?, CancellationToken>((_, path, _) => capturedPath = path)
            .ReturnsAsync(Array.Empty<FilterItemDto<OptionDto>>());

        await _sut.CreateFiltersAsync(filterNames, "/some/path", CancellationToken.None);

        Assert.That(capturedPath, Is.EqualTo("/some/path"));
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static RelatedContentCardsQuery BasicQuery(string pageType) => new()
    {
        SelectedPageTypes = new List<string> { pageType },
        CurrentPage = 1,
        PageSize = 10
    };

    private void SetupRetrieval<T>(IEnumerable<T> pages)
        where T : class, IWebPageFieldsSource
    {
        _contentRetrieverMock
            .Setup(x => x.RetrievePages<T, T>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<Func<IContentQueryDataContainer, T, Task<T>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(pages.ToArray());
    }

    private void SetupInsightRetrieval(IEnumerable<Insight> pages) => SetupRetrieval(pages);
    private void SetupBlogPostRetrieval(IEnumerable<BlogPost> pages) => SetupRetrieval(pages);

    private static Contenttypes CreateContentType(string name)
    {
        var ct = (Contenttypes)RuntimeHelpers.GetUninitializedObject(typeof(Contenttypes));
        SetProperty(ct, nameof(Contenttypes.Name), name);
        return ct;
    }

    private static T CreateNamedLinkedItem<T>(string name) where T : class
    {
        var item = (T)RuntimeHelpers.GetUninitializedObject(typeof(T));
        var nameProp = typeof(T).GetProperty("Name", BindingFlags.Public | BindingFlags.Instance);
        if (nameProp?.CanWrite == true) nameProp.SetValue(item, name);
        var metadataTitleProp = typeof(T).GetProperty("MetadataTitle", BindingFlags.Public | BindingFlags.Instance);
        if (metadataTitleProp?.CanWrite == true) metadataTitleProp.SetValue(item, name);
        return item;
    }

    private static ContentAttachment CreateTeaserImageAttachment(string url)
    {
        var asset = (ContentItemAsset)RuntimeHelpers.GetUninitializedObject(typeof(ContentItemAsset));
        SetProperty(asset, nameof(ContentItemAsset.Url), url);

        var attachment = (ContentAttachment)RuntimeHelpers.GetUninitializedObject(typeof(ContentAttachment));
        SetProperty(attachment, nameof(ContentAttachment.ContentAttachmentAsset), asset);

        return attachment;
    }

    private static WebPageFields CreateWebPageFields(Guid guid, DateTime? publishedDate = null)
    {
        var fields = (WebPageFields)RuntimeHelpers.GetUninitializedObject(typeof(WebPageFields));
        SetProperty(fields, nameof(WebPageFields.ContentItemGUID), guid);
        SetProperty(fields, nameof(WebPageFields.ContentItemCommonDataLastPublishedWhen), publishedDate ?? DateTime.UtcNow);
        return fields;
    }

    private sealed class InsightOptions
    {
        public DateTime? PublishedDate { get; init; }
        public string Title { get; init; } = "Test Insight";
        public IEnumerable<Contenttypes>? ContentTypes { get; init; }
        public IEnumerable<BusinessLine>? BusinessLines { get; init; }
        public IEnumerable<ServiceArea>? ServiceAreas { get; init; }
        public IEnumerable<IndustryCategory>? IndustryCategories { get; init; }
        public string? Description { get; init; }
        public bool ComingSoon { get; init; }
    }

    private static Insight CreateInsight(Guid guid, InsightOptions? options = null)
    {
        options ??= new InsightOptions();
        var insight = (Insight)RuntimeHelpers.GetUninitializedObject(typeof(Insight));

        SetProperty(insight, nameof(Insight.SystemFields), CreateWebPageFields(guid, options.PublishedDate));
        SetProperty(insight, nameof(Insight.MetadataTitle), options.Title);
        SetProperty(insight, nameof(Insight.MetadataDescription), options.Description);
        SetProperty(insight, nameof(Insight.ContentType), options.ContentTypes ?? Enumerable.Empty<Contenttypes>());
        SetProperty(insight, nameof(Insight.MetadataBusinessLine), options.BusinessLines ?? Enumerable.Empty<BusinessLine>());
        SetProperty(insight, nameof(Insight.MetadataServiceAreas), options.ServiceAreas ?? Enumerable.Empty<ServiceArea>());
        SetProperty(insight, nameof(Insight.MetadataIndustryCategories), options.IndustryCategories ?? Enumerable.Empty<IndustryCategory>());
        SetProperty(insight, nameof(Insight.ComingSoon), options.ComingSoon);

        var insightContentProp = typeof(Insight).GetProperty(nameof(Insight.InsightContent));
        var elementType = insightContentProp?.PropertyType.GetGenericArguments().FirstOrDefault();
        if (elementType != null)
            SetProperty(insight, nameof(Insight.InsightContent), Array.CreateInstance(elementType, 0));

        return insight;
    }

    private static BlogPost CreateBlogPost(
        Guid guid,
        string title = "Test BlogPost",
        IEnumerable<Contenttypes>? contentTypes = null,
        string? teaserImageUrl = null)
    {
        var blogPost = (BlogPost)RuntimeHelpers.GetUninitializedObject(typeof(BlogPost));

        SetProperty(blogPost, nameof(BlogPost.SystemFields), CreateWebPageFields(guid));
        SetProperty(blogPost, nameof(BlogPost.MetadataTitle), title);
        SetProperty(blogPost, nameof(BlogPost.ContentType), contentTypes ?? Enumerable.Empty<Contenttypes>());
        SetProperty(blogPost, nameof(BlogPost.MetadataBusinessLine), Enumerable.Empty<BusinessLine>());
        SetProperty(blogPost, nameof(BlogPost.MetadataServiceAreas), Enumerable.Empty<ServiceArea>());
        SetProperty(blogPost, nameof(BlogPost.MetadataIndustryCategories), Enumerable.Empty<IndustryCategory>());

        var teaserImage = teaserImageUrl is not null
            ? new[] { CreateTeaserImageAttachment(teaserImageUrl) }
            : Enumerable.Empty<ContentAttachment>();
        SetProperty(blogPost, nameof(BlogPost.MetadataTeaserImage), teaserImage);

        return blogPost;
    }

    private static NewsArticle CreateNewsArticle(
        Guid guid,
        string title = "Test NewsArticle",
        string? teaserImageUrl = null)
    {
        var article = (NewsArticle)RuntimeHelpers.GetUninitializedObject(typeof(NewsArticle));

        SetProperty(article, nameof(NewsArticle.SystemFields), CreateWebPageFields(guid));
        SetProperty(article, nameof(NewsArticle.MetadataTitle), title);
        SetProperty(article, nameof(NewsArticle.ContentType), Enumerable.Empty<Contenttypes>());
        SetProperty(article, nameof(NewsArticle.MetadataBusinessLine), Enumerable.Empty<BusinessLine>());
        SetProperty(article, nameof(NewsArticle.MetadataServiceAreas), Enumerable.Empty<ServiceArea>());
        SetProperty(article, nameof(NewsArticle.MetadataIndustryCategories), Enumerable.Empty<IndustryCategory>());

        var teaserImage = teaserImageUrl is not null
            ? new[] { CreateTeaserImageAttachment(teaserImageUrl) }
            : Enumerable.Empty<ContentAttachment>();
        SetProperty(article, nameof(NewsArticle.MetadataTeaserImage), teaserImage);

        return article;
    }

    private static Event CreateEvent(
        Guid guid,
        string title = "Test Event",
        string? teaserImageUrl = null)
    {
        var ev = (Event)RuntimeHelpers.GetUninitializedObject(typeof(Event));

        SetProperty(ev, nameof(Event.SystemFields), CreateWebPageFields(guid));
        SetProperty(ev, nameof(Event.MetadataTitle), title);
        SetProperty(ev, nameof(Event.ContentTypeEvent), Enumerable.Empty<Contenttypes>());
        SetProperty(ev, nameof(Event.MetadataBusinessLineEvent), Enumerable.Empty<BusinessLine>());
        SetProperty(ev, nameof(Event.MetadataServiceAreasEvent), Enumerable.Empty<ServiceArea>());
        SetProperty(ev, nameof(Event.MetadataIndustryCategoriesEvent), Enumerable.Empty<IndustryCategory>());

        var teaserImage = teaserImageUrl is not null
            ? new[] { CreateTeaserImageAttachment(teaserImageUrl) }
            : Enumerable.Empty<ContentAttachment>();
        SetProperty(ev, nameof(Event.MetadataTeaserImage), teaserImage);

        return ev;
    }

    private static Webinar CreateWebinar(
        Guid guid,
        string title = "Test Webinar",
        string? teaserImageUrl = null)
    {
        var webinar = (Webinar)RuntimeHelpers.GetUninitializedObject(typeof(Webinar));

        SetProperty(webinar, nameof(Webinar.SystemFields), CreateWebPageFields(guid));
        SetProperty(webinar, nameof(Webinar.MetadataTitle), title);
        SetProperty(webinar, nameof(Webinar.ContentTypeEvent), Enumerable.Empty<Contenttypes>());
        SetProperty(webinar, nameof(Webinar.MetadataBusinessLineEvent), Enumerable.Empty<BusinessLine>());
        SetProperty(webinar, nameof(Webinar.MetadataServiceAreasEvent), Enumerable.Empty<ServiceArea>());
        SetProperty(webinar, nameof(Webinar.MetadataIndustryCategoriesEvent), Enumerable.Empty<IndustryCategory>());

        var teaserImage = teaserImageUrl is not null
            ? new[] { CreateTeaserImageAttachment(teaserImageUrl) }
            : Enumerable.Empty<ContentAttachment>();
        SetProperty(webinar, nameof(Webinar.MetadataTeaserImage), teaserImage);

        return webinar;
    }

    private static MicrositePage CreateMicrositePage(
        Guid guid,
        string title = "Test MicrositePage",
        string? teaserImageUrl = null,
        string? micrositeContentTypeName = null)
    {
        var microsite = (MicrositePage)RuntimeHelpers.GetUninitializedObject(typeof(MicrositePage));

        SetProperty(microsite, nameof(MicrositePage.SystemFields), CreateWebPageFields(guid));
        SetProperty(microsite, nameof(MicrositePage.MetadataTitle), title);
        SetProperty(microsite, nameof(MicrositePage.BusinessLines), Enumerable.Empty<BusinessLine>());
        SetProperty(microsite, nameof(MicrositePage.ServiceAreas), Enumerable.Empty<ServiceArea>());
        SetProperty(microsite, nameof(MicrositePage.IndustryCategories), Enumerable.Empty<IndustryCategory>());

        var teaserImage = teaserImageUrl is not null
            ? new[] { CreateTeaserImageAttachment(teaserImageUrl) }
            : Enumerable.Empty<ContentAttachment>();
        SetProperty(microsite, nameof(MicrositePage.MetadataTeaserImage), teaserImage);

        if (micrositeContentTypeName is not null)
        {
            var ct = CreateContentType(micrositeContentTypeName);
            SetProperty(microsite, nameof(MicrositePage.MicrositeContentType), new[] { ct });
        }
        else
        {
            SetProperty(microsite, nameof(MicrositePage.MicrositeContentType), Enumerable.Empty<Contenttypes>());
        }

        return microsite;
    }

    private static Insight[] CreateInsights(int count) =>
        Enumerable.Range(0, count)
            .Select(i => CreateInsight(Guid.NewGuid(), new InsightOptions
            {
                PublishedDate = DateTime.UtcNow.AddDays(-i),
                Title = $"Insight {i}"
            }))
            .ToArray();

    private static BlogPost[] CreateBlogPosts(int count) =>
        Enumerable.Range(0, count)
            .Select(i => CreateBlogPost(Guid.NewGuid(), title: $"BlogPost {i}"))
            .ToArray();

    private static void SetProperty(object obj, string propertyName, object? value)
    {
        obj.GetType()
            .GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance)
            ?.SetValue(obj, value);
    }
}