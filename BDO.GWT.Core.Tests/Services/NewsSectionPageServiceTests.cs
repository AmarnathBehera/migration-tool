using BDO.GWT.Core;
using BDO.GWT.Core.Data.Queries;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Cards;
using BDO.GWT.Core.DTOs.Pages;
using BDO.GWT.Core.Services;
using BDO.GWT.Web.Core.DTOs;
using Moq;

namespace BDO.GWT.Core.Tests.Services;

public class NewsSectionPageServiceTests
{
    private readonly Mock<INewsSectionPageService> _newsSectionPageServiceMock;

    public NewsSectionPageServiceTests()
    {
        _newsSectionPageServiceMock = new Mock<INewsSectionPageService>();
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
            .Setup(x => x.GetNewsArticlePaginationResultAsync(It.IsAny<string>(), It.IsAny<CardsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedDto);

        var result = await _newsSectionPageServiceMock.Object.GetNewsArticlePaginationResultAsync(
            "en", new CardsQuery(), CancellationToken.None);

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
            .Setup(x => x.GetNewsArticlePaginationResultAsync(It.IsAny<string>(), It.IsAny<CardsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyDto);

        var result = await _newsSectionPageServiceMock.Object.GetNewsArticlePaginationResultAsync(
            "en", new CardsQuery(), CancellationToken.None);

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
            .Setup(x => x.GetNewsArticlePaginationResultAsync(It.IsAny<string>(), It.IsAny<CardsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaginationDto<NewsPageCardDto> { Data = [], TotalCount = 0 });

        var cardsQuery = new CardsQuery();
        await _newsSectionPageServiceMock.Object.GetNewsArticlePaginationResultAsync(
            "en", cardsQuery, CancellationToken.None);

        _newsSectionPageServiceMock.Verify(
            x => x.GetNewsArticlePaginationResultAsync(It.IsAny<string>(), cardsQuery, It.IsAny<CancellationToken>()), Times.Once);
    }
}
