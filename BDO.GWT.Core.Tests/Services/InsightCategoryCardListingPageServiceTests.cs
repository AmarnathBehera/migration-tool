using BDO;
using BDO.GWT.Core.Data.Queries;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Cards;
using BDO.GWT.Core.Helpers;
using BDO.GWT.Core.Services;
using BDO.GWT.Web.Core.Services;
using CMS.ContentEngine;
using CMS.Websites.Routing;
using Kentico.Content.Web.Mvc;
using Moq;

namespace BDO.GWT.Core.Tests.Services;

public class InsightCategoryCardListingPageServiceTests
{
    private Mock<IContentRetriever> _contentRetrieverMock = null!;
    private Mock<IInsightCardService> _insightCardServiceMock = null!;
    private InsightCategoryCardListingPageService _service = null!;

    [SetUp]
    public void Setup()
    {
        _contentRetrieverMock = new Mock<IContentRetriever>();
        _insightCardServiceMock = new Mock<IInsightCardService>();
        _insightCardServiceMock
            .Setup(x => x.GetFilteredItemsAsync(
                It.IsAny<CardsQuery>(),
                It.IsAny<string>(),
                It.IsAny<Action<RetrievePagesQueryParameters>?>(),
                It.IsAny<Func<Insight, bool>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaginationDto<ContentPageCardDto>
            {
                Data = Array.Empty<ContentPageCardDto>(),
                TotalCount = 0
            });

        _service = new InsightCategoryCardListingPageService(
            _insightCardServiceMock.Object,
            Mock.Of<IWebsiteChannelContext>(),
            _contentRetrieverMock.Object,
            Mock.Of<IFilteringHelper>(),
            Mock.Of<IHeroService>());
    }

    [Test]
    public void GetContentPaginationResultAsync_ThrowsContentItemNotFoundException_WhenPageIsNotFound()
    {
        var pageGuid = Guid.NewGuid();

        _contentRetrieverMock
            .Setup(x => x.RetrievePages<InsightCategory, InsightCategory>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<Func<IContentQueryDataContainer, InsightCategory, Task<InsightCategory>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<InsightCategory>());

        var exception = Assert.ThrowsAsync<ContentItemNotFoundException>(async () =>
            await _service.GetContentPaginationResultAsync(pageGuid, new CardsQuery(), CancellationToken.None));

        Assert.That(exception!.Message, Does.Contain(pageGuid.ToString()));
    }

    [Test]
    public void GetContentPaginationResultAsync_DoesNotThrow_WhenPageIsFound()
    {
        var pageGuid = Guid.NewGuid();
        var page = new InsightCategory();

        _contentRetrieverMock
            .Setup(x => x.RetrievePages<InsightCategory, InsightCategory>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<Func<IContentQueryDataContainer, InsightCategory, Task<InsightCategory>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { page });

        Assert.DoesNotThrowAsync(async () =>
            await _service.GetContentPaginationResultAsync(pageGuid, new CardsQuery(), CancellationToken.None));
    }

    [Test]
    public void GetInsightPageDtoAsync_ThrowsInvalidOperationException_WhenCurrentPageIsNull()
    {
        _contentRetrieverMock
            .Setup(x => x.RetrieveCurrentPage<InsightCategory>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<Action<RetrieveCurrentPageQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<Func<IContentQueryDataContainer, InsightCategory, Task<InsightCategory>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((InsightCategory)null!);

        var exception = Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _service.GetInsightPageDtoAsync(new InsightCategory(), CancellationToken.None));

        Assert.That(exception!.Message, Does.Contain("Current page of type InsightCategory could not be retrieved"));
    }

    [Test]
    public void GetInsightPageDtoAsync_ThrowsInvalidOperationException_WhenSystemFieldsIsNull()
    {
        // A freshly constructed InsightCategory has a null SystemFields, which
        // exercises the second guard clause once RetrieveCurrentPage succeeds.
        var pageWithoutSystemFields = new InsightCategory();

        _contentRetrieverMock
            .Setup(x => x.RetrieveCurrentPage<InsightCategory>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<Action<RetrieveCurrentPageQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<Func<IContentQueryDataContainer, InsightCategory, Task<InsightCategory>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(pageWithoutSystemFields);

        var exception = Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _service.GetInsightPageDtoAsync(new InsightCategory(), CancellationToken.None));

        Assert.That(exception!.Message, Does.Contain("System fields for InsightCategory page are unavailable"));
    }
}