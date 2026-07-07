using BDO.GWT.Core.Data.Queries;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.Services;
using CMS.Websites;
using Kentico.Content.Web.Mvc;
using Microsoft.AspNetCore.Http;
using Moq;

namespace BDO.GWT.Core.Tests.Services;

public class InsightCardServiceTests
{
    private Mock<IContentRetriever> _contentRetrieverMock = null!;
    private Mock<ICardsService> _cardsServiceMock = null!;
    private Mock<IWebPageUrlResolver> _webPageUrlResolverMock = null!;
    private Mock<IHttpContextAccessor> _httpContextAccessorMock = null!;
    private InsightCardService _service = null!;

    [SetUp]
    public void Setup()
    {
        _contentRetrieverMock = new Mock<IContentRetriever>();
        _cardsServiceMock = new Mock<ICardsService>();
        _webPageUrlResolverMock = new Mock<IWebPageUrlResolver>();
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();

        _service = new InsightCardService(
            _contentRetrieverMock.Object,
            _cardsServiceMock.Object,
            _webPageUrlResolverMock.Object,
            _httpContextAccessorMock.Object);
    }

    [Test]
    public async Task CreateFiltersAsync_DelegatesToCardsService()
    {
        var filters = new HashSet<string> { "businessLine" };
        var expected = new[] { new FilterItemDto<OptionDto> { Name = "businessLine", Options = Array.Empty<OptionDto>() } };

        _cardsServiceMock
            .Setup(x => x.CreateFiltersAsync(filters, "/insights", It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _service.CreateFiltersAsync(filters, "/insights", CancellationToken.None);

        Assert.That(result, Is.SameAs(expected));
        _cardsServiceMock.Verify(x => x.CreateFiltersAsync(filters, "/insights", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public void GetFilteredItemsAsync_Throws_WhenParentPathIsEmpty()
    {
        var query = new CardsQuery();

        Assert.ThrowsAsync<ArgumentException>(async () =>
            await _service.GetFilteredItemsAsync(
                query,
                string.Empty,
                null,
                _ => false,
                CancellationToken.None));
    }

    [Test]
    public void GetFilteredItemsAsync_Throws_WhenParentPathIsWhitespace()
    {
        var query = new CardsQuery();

        Assert.ThrowsAsync<ArgumentException>(async () =>
            await _service.GetFilteredItemsAsync(
                query,
                "   ",
                null,
                _ => false,
                CancellationToken.None));
    }

    [Test]
    public void GetFilteredItemsAsync_Throws_WhenParentPathIsNull()
    {
        var query = new CardsQuery();

        Assert.ThrowsAsync<ArgumentException>(async () =>
            await _service.GetFilteredItemsAsync(
                query,
                null!,
                null,
                _ => false,
                CancellationToken.None));
    }
}
