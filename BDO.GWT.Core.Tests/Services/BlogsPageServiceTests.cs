using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BDO.GWT.Common.Exceptions;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.Data.Queries;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Cards;
using BDO.GWT.Core.DTOs.Pages;
using BDO.GWT.Core.Helpers;
using BDO.GWT.Core.Services;
using BDO.GWT.Core.ValueObjects;
using BDO.GWT.Web.Core.DTOs;
using BDO.GWT.Web.Core.Services;
using CMS.ContentEngine;
using CMS.Websites;
using Kentico.Content.Web.Mvc;
using Moq;
using NUnit.Framework;

namespace BDO.GWT.Core.Tests.Services;

[TestFixture]
public class BlogsPageServiceTests
{
    private Mock<IFilteringHelper> _filteringHelperMock;
    private Mock<IHeroService> _heroServiceMock;
    private Mock<IBlogPageCardsService> _blogPageCardsServiceMock;
    private Mock<IContentService> _contentServiceMock;
    private Mock<ICultureContext> _cultureContextMock;
    private Mock<IContentRetriever> _contentRetrieverMock;
    private BlogsPageService _sut;

    [SetUp]
    public void SetUp()
    {
        _filteringHelperMock = new Mock<IFilteringHelper>();
        _heroServiceMock = new Mock<IHeroService>();
        _blogPageCardsServiceMock = new Mock<IBlogPageCardsService>();
        _contentServiceMock = new Mock<IContentService>();
        _cultureContextMock = new Mock<ICultureContext>();
        _contentRetrieverMock = new Mock<IContentRetriever>();

        _sut = new BlogsPageService(
            _filteringHelperMock.Object,
            _heroServiceMock.Object,
            _blogPageCardsServiceMock.Object,
            _contentServiceMock.Object,
            _cultureContextMock.Object,
            _contentRetrieverMock.Object);
    }

    private static Blog CreateBlog(
        int webPageItemId = 1,
        Guid? contentItemGuid = null,
        bool enableFilters = false,
        string? blogPostListFilters = null)
    {
        return new Blog
        {
            SystemFields = new WebPageFields
            {
                WebPageItemID = webPageItemId,
                ContentItemGUID = contentItemGuid ?? Guid.NewGuid()
            },
            HeroLayout = "Default",
            HeroTitle = "Test Hero Title",
            HeroDescription = "Test Description",
            HeroBackgroundImageAltText = "alt-bg",
            HeroBackgroundImage_MobileAltText = "alt-mobile",
            HeroCallToActionButtonType = "primary",
            HeroCallToActionButtonText = "CTA Text",
            HeroBackgroundImage = Array.Empty<ContentAttachment>(),
            HeroBackgroundImage_Mobile = Array.Empty<ContentAttachment>(),
            HeroCallToActionButtonUrl = Array.Empty<IWebPageFieldsSource>(),
            ItemListAdvancedSearchLinkUrl = Array.Empty<WebPageRelatedItem>(),
            BlogPostListFeaturedItems = Array.Empty<WebPageRelatedItem>(),
            ItemListFilteringEnabled = enableFilters,
            BlogPostListFilters = blogPostListFilters,
            ItemListTitle = "List Title",
            ItemListDescription = "List Desc",
            ItemListAdvancedSearchDescription = "Search Desc",
            ItemListAdvancedSearchLinkText = "Search Link"
        };
    }

    // ==========================================================================
    // GetBlogsPageDtoAsync
    // ==========================================================================

    #region GetBlogsPageDtoAsync

    [Test]
    public async Task GetBlogsPageDtoAsync_WhenCurrentPageIsNull_ReturnsNull()
    {
        var result = await _sut.GetBlogsPageDtoAsync(null!, CancellationToken.None);

        Assert.That(result, Is.Null);
        _heroServiceMock.Verify(
            h => h.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task GetBlogsPageDtoAsync_ReturnsDto_WithCorrectApiEndpoint()
    {
        var blog = CreateBlog(webPageItemId: 42);
        _heroServiceMock
            .Setup(h => h.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HeroDto());
        _filteringHelperMock
            .Setup(f => f.IsFilteringTooltipEnabledAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _sut.GetBlogsPageDtoAsync(blog, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.CardsGrid!.ApiEndpointName, Is.EqualTo("BlogPosts/42"));
        });
    }

    [Test]
    public async Task GetBlogsPageDtoAsync_FiltersDisabled_FiltersPropertyIsNull()
    {
        var blog = CreateBlog(enableFilters: false);
        _heroServiceMock
            .Setup(h => h.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HeroDto());
        _filteringHelperMock
            .Setup(f => f.IsFilteringTooltipEnabledAsync(false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _sut.GetBlogsPageDtoAsync(blog, CancellationToken.None);

        Assert.That(result!.CardsGrid!.Filters, Is.Null);
        _blogPageCardsServiceMock.Verify(
            s => s.CreateBlogPageFiltersAsync(It.IsAny<HashSet<string>>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task GetBlogsPageDtoAsync_FiltersEnabled_CallsCreateFiltersWithParsedSet()
    {
        var blog = CreateBlog(enableFilters: true, blogPostListFilters: "keywords;businessLine; service ");
        HashSet<string>? captured = null;

        _heroServiceMock
            .Setup(h => h.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HeroDto());
        _filteringHelperMock
            .Setup(f => f.IsFilteringTooltipEnabledAsync(true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _blogPageCardsServiceMock
            .Setup(s => s.CreateBlogPageFiltersAsync(It.IsAny<HashSet<string>>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Callback<HashSet<string>, int, CancellationToken>((f, _, _) => captured = f)
            .ReturnsAsync(Array.Empty<FilterItemDto<OptionDto>>());

        await _sut.GetBlogsPageDtoAsync(blog, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(captured, Is.Not.Null);
            Assert.That(captured!, Does.Contain("keywords"));
            Assert.That(captured!, Does.Contain("businessLine"));
            Assert.That(captured!, Does.Contain("service"));
            Assert.That(captured!, Has.Count.EqualTo(3));
        });
    }

    [Test]
    public async Task GetBlogsPageDtoAsync_FiltersEnabled_NullFilterString_UsesEmptySet()
    {
        var blog = CreateBlog(enableFilters: true, blogPostListFilters: null);
        HashSet<string>? captured = null;

        _heroServiceMock
            .Setup(h => h.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HeroDto());
        _filteringHelperMock
            .Setup(f => f.IsFilteringTooltipEnabledAsync(true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _blogPageCardsServiceMock
            .Setup(s => s.CreateBlogPageFiltersAsync(It.IsAny<HashSet<string>>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Callback<HashSet<string>, int, CancellationToken>((f, _, _) => captured = f)
            .ReturnsAsync(Array.Empty<FilterItemDto<OptionDto>>());

        await _sut.GetBlogsPageDtoAsync(blog, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(captured, Is.Not.Null);
            Assert.That(captured!, Is.Empty);
        });
    }

    [Test]
    public async Task GetBlogsPageDtoAsync_MapsHeroDataFieldsCorrectly()
    {
        var blog = CreateBlog();
        HeroData? capturedHero = null;

        _heroServiceMock
            .Setup(h => h.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()))
            .Callback<HeroData, CancellationToken>((d, _) => capturedHero = d)
            .ReturnsAsync(new HeroDto());
        _filteringHelperMock
            .Setup(f => f.IsFilteringTooltipEnabledAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        await _sut.GetBlogsPageDtoAsync(blog, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(capturedHero, Is.Not.Null);
            Assert.That(capturedHero!.Layout, Is.EqualTo("Default"));
            Assert.That(capturedHero.Title, Is.EqualTo("Test Hero Title"));
            Assert.That(capturedHero.Description, Is.EqualTo("Test Description"));
            Assert.That(capturedHero.BackgroundImageAltText, Is.EqualTo("alt-bg"));
            Assert.That(capturedHero.MobileBackgroundImageAltText, Is.EqualTo("alt-mobile"));
            Assert.That(capturedHero.CallToActionButtonText, Is.EqualTo("CTA Text"));
            Assert.That(capturedHero.BackgroundImage, Is.Null);
            Assert.That(capturedHero.MobileBackgroundImage, Is.Null);
            Assert.That(capturedHero.CallToActionButtonUrl, Is.Null);
        });
    }

    [Test]
    public async Task GetBlogsPageDtoAsync_EnableFilters_ReflectedInReturnedDto()
    {
        var blog = CreateBlog(enableFilters: true, blogPostListFilters: "keywords");
        _heroServiceMock
            .Setup(h => h.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HeroDto());
        _filteringHelperMock
            .Setup(f => f.IsFilteringTooltipEnabledAsync(true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _blogPageCardsServiceMock
            .Setup(s => s.CreateBlogPageFiltersAsync(It.IsAny<HashSet<string>>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<FilterItemDto<OptionDto>>());

        var result = await _sut.GetBlogsPageDtoAsync(blog, CancellationToken.None);

        Assert.That(result!.EnableFilters, Is.True);
    }

    [Test]
    public async Task GetBlogsPageDtoAsync_CardsGrid_HasFixedPageSize20()
    {
        var blog = CreateBlog();
        _heroServiceMock
            .Setup(h => h.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HeroDto());
        _filteringHelperMock
            .Setup(f => f.IsFilteringTooltipEnabledAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _sut.GetBlogsPageDtoAsync(blog, CancellationToken.None);

        Assert.That(result!.CardsGrid!.PageSize, Is.EqualTo(20));
    }

    [Test]
    public async Task GetBlogsPageDtoAsync_MapsListTitleAndDescription()
    {
        var blog = CreateBlog();
        _heroServiceMock
            .Setup(h => h.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HeroDto());
        _filteringHelperMock
            .Setup(f => f.IsFilteringTooltipEnabledAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _sut.GetBlogsPageDtoAsync(blog, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result!.CardsGrid!.Title, Is.EqualTo("List Title"));
            Assert.That(result!.CardsGrid!.Description, Is.EqualTo("List Desc"));
            Assert.That(result!.CardsGrid!.AdvancedSearchPrelinkText, Is.EqualTo("Search Desc"));
            Assert.That(result!.CardsGrid!.AdvancedSearchLinkText, Is.EqualTo("Search Link"));
        });
    }

    #endregion

    // ==========================================================================
    // GetBlogsPaginationResultAsync
    // ==========================================================================

    #region GetBlogsPaginationResultAsync

    [Test]
    public void GetBlogsPaginationResultAsync_BlogPageNotFound_ThrowsMissingSectionPageException()
    {
        _contentServiceMock
            .Setup(s => s.GetPagesAsync<Blog>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<Blog>());

        Assert.ThrowsAsync<MissingSectionPageException>(async () =>
            await _sut.GetBlogsPaginationResultAsync(99, "en-US", new CardsQuery(), CancellationToken.None));
    }

    [Test]
    public async Task GetBlogsPaginationResultAsync_BlogPageNotFound_DoesNotCallCardsService()
    {
        _contentServiceMock
            .Setup(s => s.GetPagesAsync<Blog>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<Blog>());

        try
        {
            await _sut.GetBlogsPaginationResultAsync(1, "en", new CardsQuery(), CancellationToken.None);
        }
        catch (MissingSectionPageException) { /* expected */ }

        _blogPageCardsServiceMock.Verify(
            s => s.GetFilteredItemsAsync(
                It.IsAny<string>(), It.IsAny<CardsQuery>(),
                It.IsAny<Action<ContentItemQueryBuilder>?>(),
                It.IsAny<Func<BlogPost, bool>>(),
                It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task GetBlogsPaginationResultAsync_PageFound_PassesCorrectLanguageAndPageId()
    {
        var guid = Guid.NewGuid();
        var blog = CreateBlog(webPageItemId: 77, contentItemGuid: guid);

        _contentServiceMock
            .Setup(s => s.GetPagesAsync<Blog>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { blog });

        _contentServiceMock
            .Setup(s => s.GetItemAsync<Blog>(guid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(blog);

        _blogPageCardsServiceMock
            .Setup(s => s.GetFilteredItemsAsync(
                It.IsAny<string>(), It.IsAny<CardsQuery>(),
                It.IsAny<Action<ContentItemQueryBuilder>?>(),
                It.IsAny<Func<BlogPost, bool>>(),
                It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaginationDto<BlogPageCardDto> { Data = Array.Empty<BlogPageCardDto>(), TotalCount = 0 });

        await _sut.GetBlogsPaginationResultAsync(77, "fr-FR", new CardsQuery(), CancellationToken.None);

        _blogPageCardsServiceMock.Verify(
            s => s.GetFilteredItemsAsync(
                "fr-FR",
                It.IsAny<CardsQuery>(),
                null,
                It.IsAny<Func<BlogPost, bool>>(),
                77,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task GetBlogsPaginationResultAsync_ReturnsMappedPaginationDto()
    {
        var guid = Guid.NewGuid();
        var blog = CreateBlog(webPageItemId: 10, contentItemGuid: guid);

        _contentServiceMock
            .Setup(s => s.GetPagesAsync<Blog>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { blog });

        _contentServiceMock
            .Setup(s => s.GetItemAsync<Blog>(guid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(blog);

        var expectedCards = new[] { new BlogPageCardDto { Title = "Post A" }, new BlogPageCardDto { Title = "Post B" } };

        _blogPageCardsServiceMock
            .Setup(s => s.GetFilteredItemsAsync(
                It.IsAny<string>(), It.IsAny<CardsQuery>(),
                It.IsAny<Action<ContentItemQueryBuilder>?>(),
                It.IsAny<Func<BlogPost, bool>>(),
                It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaginationDto<BlogPageCardDto> { Data = expectedCards, TotalCount = 25 });

        var result = await _sut.GetBlogsPaginationResultAsync(10, "en-US", new CardsQuery(), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result.TotalCount, Is.EqualTo(25));
            Assert.That(result.Data, Has.Length.EqualTo(2));
            Assert.That(result.Data![0]!.Title, Is.EqualTo("Post A"));
            Assert.That(result.Data![1]!.Title, Is.EqualTo("Post B"));
        });
    }

    [Test]
    public async Task GetBlogsPaginationResultAsync_NullPaginationFromService_ReturnsEmptyDto()
    {
        var guid = Guid.NewGuid();
        var blog = CreateBlog(contentItemGuid: guid);

        _contentServiceMock
            .Setup(s => s.GetPagesAsync<Blog>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { blog });

        _contentServiceMock
            .Setup(s => s.GetItemAsync<Blog>(guid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(blog);

        _blogPageCardsServiceMock
            .Setup(s => s.GetFilteredItemsAsync(
                It.IsAny<string>(), It.IsAny<CardsQuery>(),
                It.IsAny<Action<ContentItemQueryBuilder>?>(),
                It.IsAny<Func<BlogPost, bool>>(),
                It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PaginationDto<BlogPageCardDto>)null!);

        var result = await _sut.GetBlogsPaginationResultAsync(1, "en", new CardsQuery(), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result.TotalCount, Is.EqualTo(0));
        });
    }

    [Test]
    public async Task GetBlogsPaginationResultAsync_FeaturedPredicate_MatchesFeaturedGuids()
    {
        var featuredGuid1 = Guid.NewGuid();
        var featuredGuid2 = Guid.NewGuid();
        var guid = Guid.NewGuid();

        var blog = CreateBlog(contentItemGuid: guid);
        blog.BlogPostListFeaturedItems = new[]
        {
            new WebPageRelatedItem { WebPageGuid = featuredGuid1 },
            new WebPageRelatedItem { WebPageGuid = featuredGuid2 }
        };

        _contentServiceMock
            .Setup(s => s.GetPagesAsync<Blog>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { blog });

        _contentServiceMock
            .Setup(s => s.GetItemAsync<Blog>(guid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(blog);

        Func<BlogPost, bool>? capturedPredicate = null;

        _blogPageCardsServiceMock
            .Setup(s => s.GetFilteredItemsAsync(
                It.IsAny<string>(), It.IsAny<CardsQuery>(),
                It.IsAny<Action<ContentItemQueryBuilder>?>(),
                It.IsAny<Func<BlogPost, bool>>(),
                It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Callback<string, CardsQuery, Action<ContentItemQueryBuilder>?, Func<BlogPost, bool>, int, CancellationToken>(
                (_, _, _, pred, _, _) => capturedPredicate = pred)
            .ReturnsAsync(new PaginationDto<BlogPageCardDto> { Data = Array.Empty<BlogPageCardDto>(), TotalCount = 0 });

        await _sut.GetBlogsPaginationResultAsync(1, "en", new CardsQuery(), CancellationToken.None);

        Assert.That(capturedPredicate, Is.Not.Null);

        var featured1 = new BlogPost { SystemFields = new WebPageFields { ContentItemGUID = featuredGuid1 } };
        var featured2 = new BlogPost { SystemFields = new WebPageFields { ContentItemGUID = featuredGuid2 } };
        var nonFeatured = new BlogPost { SystemFields = new WebPageFields { ContentItemGUID = Guid.NewGuid() } };

        Assert.Multiple(() =>
        {
            Assert.That(capturedPredicate!(featured1), Is.True);
            Assert.That(capturedPredicate!(featured2), Is.True);
            Assert.That(capturedPredicate!(nonFeatured), Is.False);
        });
    }

    [Test]
    public async Task GetBlogsPaginationResultAsync_BlogHasNoFeaturedItems_PredicateAlwaysReturnsFalse()
    {
        var guid = Guid.NewGuid();
        var blog = CreateBlog(contentItemGuid: guid);
        blog.BlogPostListFeaturedItems = Array.Empty<WebPageRelatedItem>();

        _contentServiceMock
            .Setup(s => s.GetPagesAsync<Blog>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { blog });

        _contentServiceMock
            .Setup(s => s.GetItemAsync<Blog>(guid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(blog);

        Func<BlogPost, bool>? capturedPredicate = null;

        _blogPageCardsServiceMock
            .Setup(s => s.GetFilteredItemsAsync(
                It.IsAny<string>(), It.IsAny<CardsQuery>(),
                It.IsAny<Action<ContentItemQueryBuilder>?>(),
                It.IsAny<Func<BlogPost, bool>>(),
                It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Callback<string, CardsQuery, Action<ContentItemQueryBuilder>?, Func<BlogPost, bool>, int, CancellationToken>(
                (_, _, _, pred, _, _) => capturedPredicate = pred)
            .ReturnsAsync(new PaginationDto<BlogPageCardDto> { Data = Array.Empty<BlogPageCardDto>(), TotalCount = 0 });

        await _sut.GetBlogsPaginationResultAsync(1, "en", new CardsQuery(), CancellationToken.None);

        var anyPost = new BlogPost { SystemFields = new WebPageFields { ContentItemGUID = Guid.NewGuid() } };
        Assert.That(capturedPredicate!(anyPost), Is.False);
    }

    [Test]
    public async Task GetBlogsPaginationResultAsync_GetItemAsyncReturnsNull_FeaturedSetIsEmpty()
    {
        var guid = Guid.NewGuid();
        var blog = CreateBlog(contentItemGuid: guid);

        _contentServiceMock
            .Setup(s => s.GetPagesAsync<Blog>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { blog });

        // GetItemAsync returns null → featured set should be empty
        _contentServiceMock
            .Setup(s => s.GetItemAsync<Blog>(guid, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Blog?)null);

        Func<BlogPost, bool>? capturedPredicate = null;

        _blogPageCardsServiceMock
            .Setup(s => s.GetFilteredItemsAsync(
                It.IsAny<string>(), It.IsAny<CardsQuery>(),
                It.IsAny<Action<ContentItemQueryBuilder>?>(),
                It.IsAny<Func<BlogPost, bool>>(),
                It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Callback<string, CardsQuery, Action<ContentItemQueryBuilder>?, Func<BlogPost, bool>, int, CancellationToken>(
                (_, _, _, pred, _, _) => capturedPredicate = pred)
            .ReturnsAsync(new PaginationDto<BlogPageCardDto> { Data = Array.Empty<BlogPageCardDto>(), TotalCount = 0 });

        await _sut.GetBlogsPaginationResultAsync(1, "en", new CardsQuery(), CancellationToken.None);

        var anyPost = new BlogPost { SystemFields = new WebPageFields { ContentItemGUID = Guid.NewGuid() } };
        Assert.That(capturedPredicate!(anyPost), Is.False);
    }

    [Test]
    public async Task GetBlogsPaginationResultAsync_PassesCardsQueryToService()
    {
        var guid = Guid.NewGuid();
        var blog = CreateBlog(contentItemGuid: guid);
        var cardsQuery = new CardsQuery { CurrentPage = 3, PageSize = 5 };

        _contentServiceMock
            .Setup(s => s.GetPagesAsync<Blog>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { blog });

        _contentServiceMock
            .Setup(s => s.GetItemAsync<Blog>(guid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(blog);

        CardsQuery? capturedQuery = null;

        _blogPageCardsServiceMock
            .Setup(s => s.GetFilteredItemsAsync(
                It.IsAny<string>(), It.IsAny<CardsQuery>(),
                It.IsAny<Action<ContentItemQueryBuilder>?>(),
                It.IsAny<Func<BlogPost, bool>>(),
                It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Callback<string, CardsQuery, Action<ContentItemQueryBuilder>?, Func<BlogPost, bool>, int, CancellationToken>(
                (_, q, _, _, _, _) => capturedQuery = q)
            .ReturnsAsync(new PaginationDto<BlogPageCardDto> { Data = Array.Empty<BlogPageCardDto>(), TotalCount = 0 });

        await _sut.GetBlogsPaginationResultAsync(1, "en", cardsQuery, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(capturedQuery, Is.Not.Null);
            Assert.That(capturedQuery!.CurrentPage, Is.EqualTo(3));
            Assert.That(capturedQuery.PageSize, Is.EqualTo(5));
        });
    }

    #endregion
}