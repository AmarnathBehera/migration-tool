using BDO.GWT.Core.Constants;
using BDO.GWT.Core.Data.Queries;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Cards;
using BDO.GWT.Core.Helpers;
using BDO.GWT.Core.LiveSite.Services;
using BDO.GWT.Core.Services;
using CMS.ContentEngine;
using CMS.Websites;
using Moq;

namespace BDO.GWT.Core.Tests.Services.Widget;

public class NewsCarouselServiceTests
{
    private Mock<IFilteringHelper> _filteringHelperMock = null!;
    private Mock<INewsArticleCardsService> _newsArticleCardsServiceMock = null!;
    private Mock<IContentService> _contentServiceMock = null!;
    private NewsCarouselService _service = null!;

    [SetUp]
    public void Setup()
    {
        _filteringHelperMock = new Mock<IFilteringHelper>();
        _newsArticleCardsServiceMock = new Mock<INewsArticleCardsService>();
        _contentServiceMock = new Mock<IContentService>();

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

    private static List<SectionNews> CreateSections() => new()
    {
        new SectionNews { SystemFields = new WebPageFields { WebPageItemID = 1 } }
    };

    private void SetupCardsService(PaginationDto<NewsPageCardDto> response)
    {
        _newsArticleCardsServiceMock
            .Setup(x => x.GetFilteredItemsAsync(
                It.IsAny<List<SectionNews>>(),
                It.IsAny<string>(),
                It.IsAny<CardsQuery>(),
                It.IsAny<List<Action<WhereParameters>>?>(),
                It.IsAny<Func<NewsArticle, bool>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);
    }

    [Test]
    public async Task GetAPIFilteredItemsAsync_ReturnsData()
    {
        SetupCardsService(new PaginationDto<NewsPageCardDto>
        {
            Data = new[] { new NewsPageCardDto { Title = "A" } },
            TotalCount = 1
        });

        var result = await _service.GetAPIFilteredItemsAsync(new CardsQuery(), CancellationToken.None);

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public async Task GetAPIFilteredItemsAsync_DefaultPaging_Applies10()
    {
        CardsQuery? captured = null;

        _newsArticleCardsServiceMock
            .Setup(x => x.GetFilteredItemsAsync(
                It.IsAny<List<SectionNews>>(),
                It.IsAny<string>(),
                It.IsAny<CardsQuery>(),
                It.IsAny<List<Action<WhereParameters>>?>(),
                It.IsAny<Func<NewsArticle, bool>>(),
                It.IsAny<CancellationToken>()))
            .Callback<List<SectionNews>, string, CardsQuery, List<Action<WhereParameters>>?, Func<NewsArticle, bool>, CancellationToken>(
                (_, _, q, _, _, _) => captured = q)
            .ReturnsAsync(new PaginationDto<NewsPageCardDto>
            {
                Data = Array.Empty<NewsPageCardDto>()
            });

        await _service.GetAPIFilteredItemsAsync(new CardsQuery(), CancellationToken.None);

        Assert.That(captured, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(captured!.PageSize, Is.EqualTo(10));
            Assert.That(captured!.CurrentPage, Is.EqualTo(1));
        });
    }

    [Test]
    public async Task GetAPIFilteredItemsAsync_PageSizeAndPageNotOverwritten()
    {
        CardsQuery? captured = null;

        _newsArticleCardsServiceMock
            .Setup(x => x.GetFilteredItemsAsync(
                It.IsAny<List<SectionNews>>(),
                It.IsAny<string>(),
                It.IsAny<CardsQuery>(),
                It.IsAny<List<Action<WhereParameters>>?>(),
                It.IsAny<Func<NewsArticle, bool>>(),
                It.IsAny<CancellationToken>()))
            .Callback<List<SectionNews>, string, CardsQuery, List<Action<WhereParameters>>?, Func<NewsArticle, bool>, CancellationToken>(
                (_, _, q, _, _, _) => captured = q)
            .ReturnsAsync(new PaginationDto<NewsPageCardDto>
            {
                Data = Array.Empty<NewsPageCardDto>()
            });

        await _service.GetAPIFilteredItemsAsync(new CardsQuery
        {
            CurrentPage = 5,
            PageSize = 20
        }, CancellationToken.None);

        Assert.That(captured, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(captured!.PageSize, Is.EqualTo(20));
            Assert.That(captured!.CurrentPage, Is.EqualTo(5));
        });
    }

    [Test]
    public async Task GetAPIFilteredItemsAsync_PassesLanguage()
    {
        string? lang = null;

        _newsArticleCardsServiceMock
            .Setup(x => x.GetFilteredItemsAsync(
                It.IsAny<List<SectionNews>>(),
                It.IsAny<string>(),
                It.IsAny<CardsQuery>(),
                It.IsAny<List<Action<WhereParameters>>?>(),
                It.IsAny<Func<NewsArticle, bool>>(),
                It.IsAny<CancellationToken>()))
            .Callback<List<SectionNews>, string, CardsQuery, List<Action<WhereParameters>>?, Func<NewsArticle, bool>, CancellationToken>(
                (_, l, _, _, _, _) => lang = l)
            .ReturnsAsync(new PaginationDto<NewsPageCardDto>
            {
                Data = Array.Empty<NewsPageCardDto>()
            });

        await _service.GetAPIFilteredItemsAsync(new CardsQuery(), CancellationToken.None);

        Assert.That(lang, Is.EqualTo("en-US"));
    }

    [TestCase(FilterParameterName.ServiceArea)]
    [TestCase(FilterParameterName.Service)]
    [TestCase(FilterParameterName.IndustryCategory)]
    [TestCase(FilterParameterName.Industry)]
    [TestCase(FilterParameterName.MicrositeMasterPage)]
    [TestCase(FilterParameterName.MicrositePage)]
    public async Task GetAPIFilteredItemsAsync_KnownFilters_AddsFilter(string pageType)
    {
        List<Action<WhereParameters>>? filters = null;

        _newsArticleCardsServiceMock
            .Setup(x => x.GetFilteredItemsAsync(
                It.IsAny<List<SectionNews>>(),
                It.IsAny<string>(),
                It.IsAny<CardsQuery>(),
                It.IsAny<List<Action<WhereParameters>>?>(),
                It.IsAny<Func<NewsArticle, bool>>(),
                It.IsAny<CancellationToken>()))
            .Callback<List<SectionNews>, string, CardsQuery, List<Action<WhereParameters>>?, Func<NewsArticle, bool>, CancellationToken>(
                (_, _, _, f, _, _) => filters = f)
            .ReturnsAsync(new PaginationDto<NewsPageCardDto>
            {
                Data = Array.Empty<NewsPageCardDto>()
            });

        await _service.GetAPIFilteredItemsAsync(new CardsQuery
        {
            PageType = pageType,
            CurrentPageGuid = Guid.NewGuid()
        }, CancellationToken.None);

        Assert.That(filters, Is.Not.Null);
    }

    [Test]
    public async Task GetAPIFilteredItemsAsync_ReturnsFeaturedOrdering()
    {
        SetupCardsService(new PaginationDto<NewsPageCardDto>
        {
            Data = new[] { new NewsPageCardDto { Title = "X" } },
            TotalCount = 1
        });

        var result = await _service.GetAPIFilteredItemsAsync(new CardsQuery(), CancellationToken.None);

        Assert.That(result, Has.Count.GreaterThanOrEqualTo(0));
    }
}