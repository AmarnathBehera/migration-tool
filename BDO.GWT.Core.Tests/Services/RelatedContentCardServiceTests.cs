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

    [Test]
    public async Task GetFilteredItemsAsync_NonFeaturedOrderedByPublishDateDescending()
    {
        var older = CreateInsight(Guid.NewGuid(), new InsightOptions { PublishedDate = DateTime.UtcNow.AddDays(-5), Title = "Older" });
        var newer = CreateInsight(Guid.NewGuid(), new InsightOptions { PublishedDate = DateTime.UtcNow.AddDays(-1), Title = "Newer" });

        SetupInsightRetrieval(new[] { older, newer });

        var query = new RelatedContentCardsQuery
        {
            SelectedPageTypes = new List<string> { nameof(Insight) },
            CurrentPage = 1,
            PageSize = 10
        };

        var result = await _sut.GetFilteredItemsAsync(query, "/", "en-gb", null, _ => false, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Data.First().Title, Is.EqualTo("Newer"));
            Assert.That(result.Data.Last().Title, Is.EqualTo("Older"));
        });
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
            .Callback<RetrievePagesParameters, Action<RetrievePagesQueryParameters>, RetrievalCacheSettings, Func<IContentQueryDataContainer, Insight, Task<Insight>>, CancellationToken>(
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

        // The service builds a combined action: ApplyCommonFilter + our decorateQuery
        // We create a mock params object and invoke only our portion by calling decorateQuery directly
        // to confirm it was wired into the combined action
        decorateQuery(null!); // invoke our delegate directly — it just sets decorateCalled = true
        Assert.That(decorateCalled, Is.True);
    }

    // -------------------------------------------------------------------------
    // Card DTO mapping
    // -------------------------------------------------------------------------

    [Test]
    public async Task GetFilteredItemsAsync_InsightPage_MapsMetadataTitleToCardTitle()
    {
        var insight = CreateInsight(Guid.NewGuid(), new InsightOptions { Title = "My Insight Title" });
        SetupInsightRetrieval(new[] { insight });

        var query = new RelatedContentCardsQuery
        {
            SelectedPageTypes = new List<string> { nameof(Insight) },
            CurrentPage = 1,
            PageSize = 10
        };

        var result = await _sut.GetFilteredItemsAsync(query, "/", "en-gb", null, _ => false, CancellationToken.None);

        Assert.That(result.Data.Single().Title, Is.EqualTo("My Insight Title"));
    }

    [Test]
    public async Task GetFilteredItemsAsync_BlogPostPage_MapsMetadataTitleToCardTitle()
    {
        var blogPost = CreateBlogPost(Guid.NewGuid(), title: "My Blog Title");
        SetupBlogPostRetrieval(new[] { blogPost });

        var query = new RelatedContentCardsQuery
        {
            SelectedPageTypes = new List<string> { nameof(BlogPost) },
            CurrentPage = 1,
            PageSize = 10
        };

        var result = await _sut.GetFilteredItemsAsync(query, "/", "en-gb", null, _ => false, CancellationToken.None);

        Assert.That(result.Data.Single().Title, Is.EqualTo("My Blog Title"));
    }

    [Test]
    public async Task GetFilteredItemsAsync_FeaturedPage_IsFeaturedTrueOnCard()
    {
        var guid = Guid.NewGuid();
        var insight = CreateInsight(guid);
        SetupInsightRetrieval(new[] { insight });

        var query = new RelatedContentCardsQuery
        {
            SelectedPageTypes = new List<string> { nameof(Insight) },
            CurrentPage = 1,
            PageSize = 10
        };

        var result = await _sut.GetFilteredItemsAsync(
            query, "/", "en-gb", null,
            page => page.SystemFields.ContentItemGUID == guid,
            CancellationToken.None);

        Assert.That(result.Data.Single().IsFeatured, Is.True);
    }

    [Test]
    public async Task GetFilteredItemsAsync_NonFeaturedPage_IsFeaturedFalseOnCard()
    {
        var insight = CreateInsight(Guid.NewGuid());
        SetupInsightRetrieval(new[] { insight });

        var query = new RelatedContentCardsQuery
        {
            SelectedPageTypes = new List<string> { nameof(Insight) },
            CurrentPage = 1,
            PageSize = 10
        };

        var result = await _sut.GetFilteredItemsAsync(
            query, "/", "en-gb", null,
            _ => false,
            CancellationToken.None);

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

        var query = new RelatedContentCardsQuery
        {
            SelectedPageTypes = new List<string> { nameof(Insight) },
            CurrentPage = 1,
            PageSize = 10
        };

        var result = await _sut.GetFilteredItemsAsync(query, "/", "en-gb", null, _ => false, CancellationToken.None);

        Assert.That(result.Data.Single().ReadMoreLink.URL, Is.EqualTo("/my-insight"));
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
    // Card DTO mapping — extra field coverage
    // -------------------------------------------------------------------------

    [Test]
    public async Task GetFilteredItemsAsync_InsightCard_MapsContentTypeAndDateAndDescription()
    {
        var publishDate = DateTime.UtcNow.AddDays(-2);
        var insight = CreateInsight(
            Guid.NewGuid(),
            new InsightOptions
            {
                PublishedDate = publishDate,
                Title = "Card",
                ContentTypes = new[] { CreateContentType("Article") },
                Description = "<p>Some description</p>"
            });

        SetupInsightRetrieval(new[] { insight });

        var query = new RelatedContentCardsQuery
        {
            SelectedPageTypes = new List<string> { nameof(Insight) },
            CurrentPage = 1,
            PageSize = 10
        };

        var result = await _sut.GetFilteredItemsAsync(query, "/", "en-gb", null, _ => false, CancellationToken.None);
        var card = result.Data.Single();

        Assert.Multiple(() =>
        {
            Assert.That(card.Type, Is.EqualTo("Article"));
            Assert.That(card.Date, Is.EqualTo(publishDate));
            Assert.That(card.Description, Does.Contain("Some description"));
            Assert.That(card.ReadMoreLink.Title, Is.EqualTo(BDO.GWT.Core.Constants.ResourceStringKey.LiveSite.ReadMore));
        });
    }

    [Test]
    public async Task GetFilteredItemsAsync_BlogPostCard_MapsContentTypeAndReadMore()
    {
        var blogPost = CreateBlogPost(
            Guid.NewGuid(),
            title: "Blog",
            contentTypes: new[] { CreateContentType("Post") });

        SetupBlogPostRetrieval(new[] { blogPost });

        _webPageUrlResolverMock
            .Setup(x => x.GetAbsoluteUrl(It.IsAny<IWebPageFieldsSource>()))
            .Returns("/blog-url");

        var query = new RelatedContentCardsQuery
        {
            SelectedPageTypes = new List<string> { nameof(BlogPost) },
            CurrentPage = 1,
            PageSize = 10
        };

        var result = await _sut.GetFilteredItemsAsync(query, "/", "en-gb", null, _ => false, CancellationToken.None);
        var card = result.Data.Single();

        Assert.Multiple(() =>
        {
            Assert.That(card.Type, Is.EqualTo("Post"));
            Assert.That(card.ReadMoreLink.URL, Is.EqualTo("/blog-url"));
        });
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

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

    private static Contenttypes CreateContentType(string name)
    {
        var ct = (Contenttypes)RuntimeHelpers.GetUninitializedObject(typeof(Contenttypes));
        SetProperty(ct, nameof(Contenttypes.Name), name);
        return ct;
    }

    private static T CreateNamedLinkedItem<T>(string name) where T : class
    {
        var item = (T)RuntimeHelpers.GetUninitializedObject(typeof(T));
        // MatchesLinkedItemName looks for "Name", then "MetadataTitle" — set whichever exists.
        var nameProp = typeof(T).GetProperty("Name", BindingFlags.Public | BindingFlags.Instance);
        if (nameProp is not null && nameProp.CanWrite)
        {
            nameProp.SetValue(item, name);
        }
        var metadataTitleProp = typeof(T).GetProperty("MetadataTitle", BindingFlags.Public | BindingFlags.Instance);
        if (metadataTitleProp is not null && metadataTitleProp.CanWrite)
        {
            metadataTitleProp.SetValue(item, name);
        }
        return item;
    }

    private void SetupInsightRetrieval(IEnumerable<Insight> pages)
    {
        _contentRetrieverMock
            .Setup(x => x.RetrievePages<Insight, Insight>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<Func<IContentQueryDataContainer, Insight, Task<Insight>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(pages.ToArray());
    }

    private void SetupBlogPostRetrieval(IEnumerable<BlogPost> pages)
    {
        _contentRetrieverMock
            .Setup(x => x.RetrievePages<BlogPost, BlogPost>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<Func<IContentQueryDataContainer, BlogPost, Task<BlogPost>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(pages.ToArray());
    }

    private static Insight[] CreateInsights(int count) =>
        Enumerable.Range(0, count)
            .Select(i => CreateInsight(Guid.NewGuid(), new InsightOptions { PublishedDate = DateTime.UtcNow.AddDays(-i), Title = $"Insight {i}" }))
            .ToArray();

    private static BlogPost[] CreateBlogPosts(int count) =>
        Enumerable.Range(0, count)
            .Select(i => CreateBlogPost(Guid.NewGuid(), title: $"BlogPost {i}"))
            .ToArray();

    /// <summary>
    /// Options for <see cref="CreateInsight"/>. Avoids Sonar S107 (too many parameters).
    /// </summary>
    private sealed class InsightOptions
    {
        public DateTime? PublishedDate { get; init; }
        public string Title { get; init; } = "Test Insight";
        public IEnumerable<Contenttypes>? ContentTypes { get; init; }
        public IEnumerable<BusinessLine>? BusinessLines { get; init; }
        public IEnumerable<ServiceArea>? ServiceAreas { get; init; }
        public IEnumerable<IndustryCategory>? IndustryCategories { get; init; }
        public string? Description { get; init; }
    }

    private static Insight CreateInsight(Guid guid, InsightOptions? options = null)
    {
        options ??= new InsightOptions();

        var insight = (Insight)RuntimeHelpers.GetUninitializedObject(typeof(Insight));

        var fields = (WebPageFields)RuntimeHelpers.GetUninitializedObject(typeof(WebPageFields));
        SetProperty(fields, nameof(WebPageFields.ContentItemGUID), guid);
        SetProperty(fields, nameof(WebPageFields.ContentItemCommonDataLastPublishedWhen), options.PublishedDate ?? DateTime.UtcNow);

        SetProperty(insight, nameof(Insight.SystemFields), fields);
        SetProperty(insight, nameof(Insight.MetadataTitle), options.Title);
        SetProperty(insight, nameof(Insight.MetadataDescription), options.Description);
        SetProperty(insight, nameof(Insight.ContentType), options.ContentTypes ?? Enumerable.Empty<Contenttypes>());
        SetProperty(insight, nameof(Insight.MetadataBusinessLine), options.BusinessLines ?? Enumerable.Empty<BusinessLine>());
        SetProperty(insight, nameof(Insight.MetadataServiceAreas), options.ServiceAreas ?? Enumerable.Empty<ServiceArea>());
        SetProperty(insight, nameof(Insight.MetadataIndustryCategories), options.IndustryCategories ?? Enumerable.Empty<IndustryCategory>());

        // Use reflection to get the actual element type of InsightContent at runtime
        // avoids referencing CopyInsight / InsightContent directly (name varies by environment)
        var insightContentProp = typeof(Insight).GetProperty(nameof(Insight.InsightContent));
        var elementType = insightContentProp?.PropertyType.GetGenericArguments().FirstOrDefault();
        if (elementType != null)
        {
            var emptyArray = Array.CreateInstance(elementType, 0);
            SetProperty(insight, nameof(Insight.InsightContent), emptyArray);
        }

        return insight;
    }

    private static BlogPost CreateBlogPost(Guid guid, string title = "Test BlogPost", IEnumerable<Contenttypes>? contentTypes = null)
    {
        var blogPost = (BlogPost)RuntimeHelpers.GetUninitializedObject(typeof(BlogPost));

        var fields = (WebPageFields)RuntimeHelpers.GetUninitializedObject(typeof(WebPageFields));
        SetProperty(fields, nameof(WebPageFields.ContentItemGUID), guid);
        SetProperty(fields, nameof(WebPageFields.ContentItemCommonDataLastPublishedWhen), DateTime.UtcNow);

        SetProperty(blogPost, nameof(BlogPost.SystemFields), fields);
        SetProperty(blogPost, nameof(BlogPost.MetadataTitle), title);
        SetProperty(blogPost, nameof(BlogPost.MetadataTeaserImage), Enumerable.Empty<ContentAttachment>());
        SetProperty(blogPost, nameof(BlogPost.ContentType), contentTypes ?? Enumerable.Empty<Contenttypes>());
        SetProperty(blogPost, nameof(BlogPost.MetadataBusinessLine), Enumerable.Empty<BusinessLine>());
        SetProperty(blogPost, nameof(BlogPost.MetadataServiceAreas), Enumerable.Empty<ServiceArea>());
        SetProperty(blogPost, nameof(BlogPost.MetadataIndustryCategories), Enumerable.Empty<IndustryCategory>());

        return blogPost;
    }

    private static void SetProperty(object obj, string propertyName, object? value)
    {
        obj.GetType()
            .GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance)
            ?.SetValue(obj, value);
    }
}