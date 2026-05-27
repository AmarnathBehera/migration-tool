using BDO.GWT.Core;
using BDO.GWT.Core.Data.Queries;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Cards;
using BDO.GWT.Core.Services;
using CMS.ContentEngine;
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
        var expectedDto = new PaginationDto<ContentPageCardDto>
        {
            Data = new[]
            {
                new ContentPageCardDto { Title = "Article 1" },
                new ContentPageCardDto { Title = "Article 2" }
            },
            TotalCount = 2
        };

        _newsArticleCardsServiceMock
            .Setup(x => x.GetFilteredItemsAsync(
                It.IsAny<CardsQuery>(),
                It.IsAny<Action<ContentItemQueryBuilder>?>(),
                It.IsAny<Func<NewsArticle, bool>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedDto);

        var result = await _newsArticleCardsServiceMock.Object.GetFilteredItemsAsync(
            new CardsQuery(), null, _ => false, CancellationToken.None);

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
        var emptyDto = new PaginationDto<ContentPageCardDto>
        {
            Data = Array.Empty<ContentPageCardDto>(),
            TotalCount = 0
        };

        _newsArticleCardsServiceMock
            .Setup(x => x.GetFilteredItemsAsync(
                It.IsAny<CardsQuery>(),
                It.IsAny<Action<ContentItemQueryBuilder>?>(),
                It.IsAny<Func<NewsArticle, bool>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyDto);

        var result = await _newsArticleCardsServiceMock.Object.GetFilteredItemsAsync(
            new CardsQuery(), null, _ => false, CancellationToken.None);

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
                It.IsAny<CardsQuery>(),
                It.IsAny<Action<ContentItemQueryBuilder>?>(),
                It.IsAny<Func<NewsArticle, bool>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaginationDto<ContentPageCardDto> { Data = [], TotalCount = 0 });

        var cardsQuery = new CardsQuery();
        await _newsArticleCardsServiceMock.Object.GetFilteredItemsAsync(
            cardsQuery, null, _ => false, CancellationToken.None);

        _newsArticleCardsServiceMock.Verify(
            x => x.GetFilteredItemsAsync(
                cardsQuery,
                It.IsAny<Action<ContentItemQueryBuilder>?>(),
                It.IsAny<Func<NewsArticle, bool>>(),
                It.IsAny<CancellationToken>()), Times.Once);
    }
}
