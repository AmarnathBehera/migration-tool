using BDO.GWT.Common.Exceptions;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.Data.Queries;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Cards;
using BDO.GWT.Core.DTOs.Pages;
using BDO.GWT.Core.Helpers;
using BDO.GWT.Core.Services;
using BDO.GWT.Web.Core.DTOs;
using BDO.GWT.Web.Core.Services;
using CMS.ContentEngine;
using CMS.Websites;
using Kentico.Content.Web.Mvc;
using Moq;

namespace BDO.GWT.Core.Tests.Services;

public class NewsSectionPageServiceTests
{
    private Mock<INewsSectionPageService> _newsSectionPageServiceMock = null!;
    private Mock<IFilteringHelper> _filteringHelperMock = null!;
    private Mock<IHeroService> _heroServiceMock = null!;
    private Mock<INewsArticleCardsService> _newsArticleCardsServiceMock = null!;
    private Mock<IContentService> _contentServiceMock = null!;
    private Mock<ICultureContext> _cultureContextMock = null!;
    private Mock<IContentRetriever> _contentRetrieverMock = null!;
    private NewsSectionPageService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _newsSectionPageServiceMock = new Mock<INewsSectionPageService>();
        _filteringHelperMock = new Mock<IFilteringHelper>();
        _heroServiceMock = new Mock<IHeroService>();
        _newsArticleCardsServiceMock = new Mock<INewsArticleCardsService>();
        _contentServiceMock = new Mock<IContentService>();
        _cultureContextMock = new Mock<ICultureContext>();
        _contentRetrieverMock = new Mock<IContentRetriever>();

        _cultureContextMock
            .Setup(c => c.CurrentLanguageTag)
            .Returns("en-US");

        _heroServiceMock
            .Setup(h => h.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HeroDto());

        _filteringHelperMock
            .Setup(f => f.IsFilteringTooltipEnabledAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _service = new NewsSectionPageService(
            _filteringHelperMock.Object,
            _heroServiceMock.Object,
            _newsArticleCardsServiceMock.Object,
            _contentServiceMock.Object,
            _cultureContextMock.Object,
            _contentRetrieverMock.Object);
    }

    [Test]
    public async Task GetNewsSectionDtoAsync_ReturnsExpectedDto()
    {
        var expectedDto = new NewsSectionDto
        {
            Title = "Section Title",
            Hero = new HeroDto { Title = "Hero Title" },
            CardsGrid = new CardsGridDto
            {
                Title = "Cards Title",
                FilteringTooltipEnabled = true,
                ApiEndpointName = "NewsSectionPage",
                PageSize = 20
            }
        };

        _newsSectionPageServiceMock
            .Setup(x => x.GetNewsSectionDtoAsync(It.IsAny<SectionNews>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedDto);

        var result = await _newsSectionPageServiceMock.Object.GetNewsSectionDtoAsync(
            new SectionNews(), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Title, Is.EqualTo("Section Title"));
            Assert.That(result.Hero, Is.Not.Null);
            Assert.That(result.Hero.Title, Is.EqualTo("Hero Title"));
            Assert.That(result.CardsGrid, Is.Not.Null);
            Assert.That(result.CardsGrid.ApiEndpointName, Is.EqualTo("NewsSectionPage"));
            Assert.That(result.CardsGrid.PageSize, Is.EqualTo(20));
        });
    }

    [Test]
    public async Task GetNewsArticlePaginationResultAsync_ReturnsPaginationDto()
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

        _newsSectionPageServiceMock
            .Setup(x => x.GetNewsArticlePaginationResultAsync(
                It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CardsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedDto);

        var result = await _newsSectionPageServiceMock.Object.GetNewsArticlePaginationResultAsync(
            Guid.NewGuid(), "en", new CardsQuery(), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Data, Has.Length.EqualTo(2));
            Assert.That(result.TotalCount, Is.EqualTo(2));
        });
    }

    [Test]
    public async Task GetNewsArticlePaginationResultAsync_WhenNoArticles_ReturnsEmptyPagination()
    {
        var emptyDto = new PaginationDto<NewsPageCardDto>
        {
            Data = Array.Empty<NewsPageCardDto>(),
            TotalCount = 0
        };

        _newsSectionPageServiceMock
            .Setup(x => x.GetNewsArticlePaginationResultAsync(
                It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CardsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyDto);

        var result = await _newsSectionPageServiceMock.Object.GetNewsArticlePaginationResultAsync(
            Guid.NewGuid(), "en", new CardsQuery(), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Data, Is.Empty);
            Assert.That(result.TotalCount, Is.EqualTo(0));
        });
    }

    [Test]
    public async Task GetNewsSectionDtoAsync_InvokedOnce()
    {
        _newsSectionPageServiceMock
            .Setup(x => x.GetNewsSectionDtoAsync(It.IsAny<SectionNews>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new NewsSectionDto
            {
                Title = "Test",
                Hero = new HeroDto(),
                CardsGrid = new CardsGridDto { FilteringTooltipEnabled = false }
            });

        var page = new SectionNews();
        await _newsSectionPageServiceMock.Object.GetNewsSectionDtoAsync(page, CancellationToken.None);

        _newsSectionPageServiceMock.Verify(
            x => x.GetNewsSectionDtoAsync(page, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GetNewsArticlePaginationResultAsync_InvokedOnce()
    {
        _newsSectionPageServiceMock
            .Setup(x => x.GetNewsArticlePaginationResultAsync(
                It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CardsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaginationDto<NewsPageCardDto> { Data = [], TotalCount = 0 });

        var cardsQuery = new CardsQuery();
        await _newsSectionPageServiceMock.Object.GetNewsArticlePaginationResultAsync(
            Guid.NewGuid(), "en", cardsQuery, CancellationToken.None);

        _newsSectionPageServiceMock.Verify(
            x => x.GetNewsArticlePaginationResultAsync(
                It.IsAny<Guid>(), It.IsAny<string>(), cardsQuery, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public void GetNewsArticlePaginationResultAsync_ThrowsMissingSectionPageException_WhenSectionNotFound()
    {
        _newsSectionPageServiceMock
            .Setup(x => x.GetNewsArticlePaginationResultAsync(
                It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CardsQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new MissingSectionPageException("No pages found."));

        Assert.ThrowsAsync<MissingSectionPageException>(() =>
            _newsSectionPageServiceMock.Object.GetNewsArticlePaginationResultAsync(
                Guid.NewGuid(), "en", new CardsQuery(), CancellationToken.None));
    }

    [Test]
    public async Task GetNewsSectionDtoAsync_ReturnsDto_WithNullHeroBackgroundImage_WhenNotConfigured()
    {
        var expectedDto = new NewsSectionDto
        {
            Title = "No Image Section",
            Hero = new HeroDto { BackgroundImage = null },
            CardsGrid = new CardsGridDto { FilteringTooltipEnabled = false },
        };

        _newsSectionPageServiceMock
            .Setup(x => x.GetNewsSectionDtoAsync(It.IsAny<SectionNews>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedDto);

        var result = await _newsSectionPageServiceMock.Object.GetNewsSectionDtoAsync(
            new SectionNews(), CancellationToken.None);

        Assert.That(result.Hero.BackgroundImage, Is.Null);
    }

    [Test]
    public async Task GetNewsSectionDtoAsync_ReturnsDto_WithFilteringDisabled_WhenNotConfigured()
    {
        var expectedDto = new NewsSectionDto
        {
            Title = "No Filter Section",
            Hero = new HeroDto(),
            CardsGrid = new CardsGridDto
            {
                Filters = null,
                FilteringTooltipEnabled = false
            }
        };

        _newsSectionPageServiceMock
            .Setup(x => x.GetNewsSectionDtoAsync(It.IsAny<SectionNews>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedDto);

        var result = await _newsSectionPageServiceMock.Object.GetNewsSectionDtoAsync(
            new SectionNews(), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.CardsGrid.Filters, Is.Null);
            Assert.That(result.CardsGrid.FilteringTooltipEnabled, Is.False);
        });
    }

    [Test]
    public async Task GetNewsSectionDtoAsync_ReturnsDto_WithAdvancedSearchFields_WhenConfigured()
    {
        var expectedDto = new NewsSectionDto
        {
            Title = "Advanced Search Section",
            Hero = new HeroDto(),
            CardsGrid = new CardsGridDto
            {
                FilteringTooltipEnabled = false,
                AdvancedSearchPrelinkText = "Looking for something specific?",
                AdvancedSearchLinkText = "Advanced Search",
                AdvancedSearchLink = "/search"
            }
        };

        _newsSectionPageServiceMock
            .Setup(x => x.GetNewsSectionDtoAsync(It.IsAny<SectionNews>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedDto);

        var result = await _newsSectionPageServiceMock.Object.GetNewsSectionDtoAsync(
            new SectionNews(), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.CardsGrid.AdvancedSearchPrelinkText, Is.EqualTo("Looking for something specific?"));
            Assert.That(result.CardsGrid.AdvancedSearchLinkText, Is.EqualTo("Advanced Search"));
            Assert.That(result.CardsGrid.AdvancedSearchLink, Is.EqualTo("/search"));
        });
    }

    [Test]
    public async Task GetNewsSectionDtoAsync_ReturnsEmptyAdvancedSearchLink_WhenNoPageConfigured()
    {
        var expectedDto = new NewsSectionDto
        {
            Title = "Section",
            Hero = new HeroDto(),
            CardsGrid = new CardsGridDto
            {
                FilteringTooltipEnabled = false,
                AdvancedSearchLink = string.Empty
            }
        };

        _newsSectionPageServiceMock
            .Setup(x => x.GetNewsSectionDtoAsync(It.IsAny<SectionNews>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedDto);

        var result = await _newsSectionPageServiceMock.Object.GetNewsSectionDtoAsync(
            new SectionNews(), CancellationToken.None);

        Assert.That(result.CardsGrid.AdvancedSearchLink, Is.EqualTo(string.Empty));
    }

    [Test]
    public async Task GetNewsArticlePaginationResultAsync_PassesCardsQueryThrough()
    {
        var expectedQuery = new CardsQuery
        {
            PageSize = 10,
            Filters = new Dictionary<string, string> { { "category", "finance" } }
        };

        CardsQuery? capturedQuery = null;

        _newsSectionPageServiceMock
            .Setup(x => x.GetNewsArticlePaginationResultAsync(
                It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CardsQuery>(), It.IsAny<CancellationToken>()))
            .Callback<Guid, string, CardsQuery, CancellationToken>((_, _, q, _) => capturedQuery = q)
            .ReturnsAsync(new PaginationDto<NewsPageCardDto> { Data = [], TotalCount = 0 });

        await _newsSectionPageServiceMock.Object.GetNewsArticlePaginationResultAsync(
            Guid.NewGuid(), "en-US", expectedQuery, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(capturedQuery, Is.Not.Null);
            Assert.That(capturedQuery!.PageSize, Is.EqualTo(10));
            Assert.That(capturedQuery.Filters["category"], Is.EqualTo("finance"));
        });
    }

    [Test]
    public async Task GetNewsArticlePaginationResultAsync_PassesLanguageThrough()
    {
        const string expectedLanguage = "fr-FR";
        string? capturedLanguage = null;

        _newsSectionPageServiceMock
            .Setup(x => x.GetNewsArticlePaginationResultAsync(
                It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CardsQuery>(), It.IsAny<CancellationToken>()))
            .Callback<Guid, string, CardsQuery, CancellationToken>((_, lang, _, _) => capturedLanguage = lang)
            .ReturnsAsync(new PaginationDto<NewsPageCardDto> { Data = [], TotalCount = 0 });

        await _newsSectionPageServiceMock.Object.GetNewsArticlePaginationResultAsync(
            Guid.NewGuid(), expectedLanguage, new CardsQuery(), CancellationToken.None);

        Assert.That(capturedLanguage, Is.EqualTo(expectedLanguage));
    }

    [Test]
    public async Task GetNewsArticlePaginationResultAsync_PassesPageGuidThrough()
    {
        var expectedGuid = Guid.NewGuid();
        Guid capturedGuid = Guid.Empty;

        _newsSectionPageServiceMock
            .Setup(x => x.GetNewsArticlePaginationResultAsync(
                It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CardsQuery>(), It.IsAny<CancellationToken>()))
            .Callback<Guid, string, CardsQuery, CancellationToken>((guid, _, _, _) => capturedGuid = guid)
            .ReturnsAsync(new PaginationDto<NewsPageCardDto> { Data = [], TotalCount = 0 });

        await _newsSectionPageServiceMock.Object.GetNewsArticlePaginationResultAsync(
            expectedGuid, "en", new CardsQuery(), CancellationToken.None);

        Assert.That(capturedGuid, Is.EqualTo(expectedGuid));
    }

    [Test]
    public async Task GetNewsSectionDtoAsync_Implementation_SetsPageTitleOnDto()
    {
        var page = new SectionNews
        {
            PageTitle = "Real Page Title",
            SystemFields = new WebPageFields { ContentItemGUID = Guid.NewGuid() }
        };

        var result = await _service.GetNewsSectionDtoAsync(page, CancellationToken.None);

        Assert.That(result.Title, Is.EqualTo("Real Page Title"));
    }

    [Test]
    public async Task GetNewsSectionDtoAsync_Implementation_CallsHeroService()
    {
        var page = new SectionNews
        {
            PageTitle = "Title",
            SystemFields = new WebPageFields { ContentItemGUID = Guid.NewGuid() }
        };

        await _service.GetNewsSectionDtoAsync(page, CancellationToken.None);

        _heroServiceMock.Verify(
            h => h.GetHeroDtoAsync(It.IsAny<HeroData>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task GetNewsSectionDtoAsync_Implementation_DoesNotCallFiltersService_WhenFilteringDisabled()
    {
        var page = new SectionNews
        {
            PageTitle = "Title",
            ItemListFilteringEnabled = false,
            SystemFields = new WebPageFields { ContentItemGUID = Guid.NewGuid() }
        };

        var result = await _service.GetNewsSectionDtoAsync(page, CancellationToken.None);

        _newsArticleCardsServiceMock.Verify(
            s => s.CreateFiltersAsync(
                It.IsAny<HashSet<string>>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()),
            Times.Never);

        Assert.That(result.CardsGrid.Filters, Is.Null);
    }

    [Test]
    public async Task GetNewsSectionDtoAsync_Implementation_CallsFiltersService_WhenFilteringEnabled()
    {
        var page = new SectionNews
        {
            PageTitle = "Title",
            ItemListFilteringEnabled = true,
            ItemListFilters = "Category;Region",
            SystemFields = new WebPageFields { ContentItemGUID = Guid.NewGuid() }
        };

        _newsArticleCardsServiceMock
            .Setup(s => s.CreateFiltersAsync(
                It.IsAny<HashSet<string>>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<FilterItemDto<OptionDto>>());

        await _service.GetNewsSectionDtoAsync(page, CancellationToken.None);

        _newsArticleCardsServiceMock.Verify(
            s => s.CreateFiltersAsync(
                It.Is<HashSet<string>>(f => f.Contains("Category") && f.Contains("Region")),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task GetNewsSectionDtoAsync_Implementation_SetsApiEndpointNameWithContentItemGuid()
    {
        var contentItemGuid = Guid.NewGuid();
        var page = new SectionNews
        {
            PageTitle = "Title",
            SystemFields = new WebPageFields { ContentItemGUID = contentItemGuid }
        };

        var result = await _service.GetNewsSectionDtoAsync(page, CancellationToken.None);

        Assert.That(result.CardsGrid.ApiEndpointName,
            Is.EqualTo($"NewsSectionPage/{contentItemGuid}"));
    }

    [Test]
    public async Task GetNewsSectionDtoAsync_Implementation_SetsPageSizeToTwenty()
    {
        var page = new SectionNews
        {
            PageTitle = "Title",
            SystemFields = new WebPageFields { ContentItemGUID = Guid.NewGuid() }
        };

        var result = await _service.GetNewsSectionDtoAsync(page, CancellationToken.None);

        Assert.That(result.CardsGrid.PageSize, Is.EqualTo(20));
    }

    [Test]
    public async Task GetNewsSectionDtoAsync_Implementation_SetsEmptyAdvancedSearchLink_WhenNoLinkPageConfigured()
    {
        var page = new SectionNews
        {
            PageTitle = "Title",
            ItemListAdvancedSearchLinkUrl = null,
            SystemFields = new WebPageFields { ContentItemGUID = Guid.NewGuid() }
        };

        var result = await _service.GetNewsSectionDtoAsync(page, CancellationToken.None);

        Assert.That(result.CardsGrid.AdvancedSearchLink, Is.EqualTo(string.Empty));
    }

    [Test]
    public void GetNewsArticlePaginationResultAsync_Implementation_ThrowsMissingSectionPageException_WhenSectionNotFound()
    {
        _contentServiceMock
            .Setup(s => s.GetPagesAsync<SectionNews>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<SectionNews>());

        Assert.ThrowsAsync<MissingSectionPageException>(() =>
            _service.GetNewsArticlePaginationResultAsync(
                Guid.NewGuid(), "en-US", new CardsQuery(), CancellationToken.None));
    }

    [Test]
    public async Task GetNewsArticlePaginationResultAsync_Implementation_UsesCurrentLanguageTag()
    {
        RetrievePagesParameters? capturedParameters = null;

        _contentServiceMock
            .Setup(s => s.GetPagesAsync<SectionNews>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .Callback<RetrievePagesParameters, Action<RetrievePagesQueryParameters>, RetrievalCacheSettings, CancellationToken>(
                (p, _, _, _) => capturedParameters = p)
            .ReturnsAsync(Array.Empty<SectionNews>());

        try
        {
            await _service.GetNewsArticlePaginationResultAsync(
                Guid.NewGuid(), "en-US", new CardsQuery(), CancellationToken.None);
        }
        catch (MissingSectionPageException) { /* expected */ }

        Assert.Multiple(() =>
        {
            Assert.That(capturedParameters, Is.Not.Null);
            Assert.That(capturedParameters!.LanguageName, Is.EqualTo("en-US"));
            Assert.That(capturedParameters.UseLanguageFallbacks, Is.False);
            Assert.That(capturedParameters.LinkedItemsMaxLevel, Is.EqualTo(2));
        });
    }

    [Test]
    public async Task GetNewsArticlePaginationResultAsync_Implementation_DelegatesToNewsArticleCardsService_WhenSectionFound()
    {
        var pageGuid = Guid.NewGuid();
        var newsSection = new SectionNews
        {
            ItemListFeaturedItems = Array.Empty<NewsArticle>()
        };
        var cardsQuery = new CardsQuery { PageSize = 20 };
        var expectedResult = new PaginationDto<NewsPageCardDto>
        {
            Data = new[] { new NewsPageCardDto { Title = "Article" } },
            TotalCount = 1
        };

        _contentServiceMock
            .Setup(s => s.GetPagesAsync<SectionNews>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { newsSection });

        _newsArticleCardsServiceMock
            .Setup(s => s.GetFilteredItemsAsync(
                It.IsAny<List<SectionNews>>(),
                It.IsAny<string>(),
                It.IsAny<CardsQuery>(),
                It.IsAny<List<Action<WhereParameters>>>(),
                It.IsAny<Func<NewsArticle, bool>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        var result = await _service.GetNewsArticlePaginationResultAsync(
            pageGuid, "en-US", cardsQuery, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Data, Has.Length.EqualTo(1));
            Assert.That(result.TotalCount, Is.EqualTo(1));
        });

        _newsArticleCardsServiceMock.Verify(
            s => s.GetFilteredItemsAsync(
                It.Is<List<SectionNews>>(list => list.Count == 1 && list[0] == newsSection),
                "en-US",
                cardsQuery,
                null,
                It.IsAny<Func<NewsArticle, bool>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}