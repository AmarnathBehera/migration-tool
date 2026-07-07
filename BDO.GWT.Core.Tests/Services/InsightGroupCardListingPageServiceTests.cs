using BDO.GWT.Core;
using BDO.GWT.Core.Data.Queries;
using BDO.GWT.Core.Helpers;
using BDO.GWT.Core.Services;
using BDO.GWT.Web.Core.Services;
using CMS.Websites;
using CMS.Websites.Routing;
using Kentico.Content.Web.Mvc;
using Moq;

namespace BDO.GWT.Core.Tests.Services;

public class InsightGroupCardListingPageServiceTests
{
    private Mock<IContentRetriever> _contentRetrieverMock = null!;
    private InsightGroupCardListingPageService _service = null!;

    [SetUp]
    public void Setup()
    {
        _contentRetrieverMock = new Mock<IContentRetriever>();

        _service = new InsightGroupCardListingPageService(
            Mock.Of<IInsightCardService>(),
            Mock.Of<IWebsiteChannelContext>(),
            _contentRetrieverMock.Object,
            Mock.Of<IFilteringHelper>(),
            Mock.Of<IHeroService>());
    }

    [Test]
    public void GetContentPaginationResultAsync_ThrowsInvalidOperation_WhenContentRetrieverDoesNotSupportServiceProvider()
    {
        var exception = Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _service.GetContentPaginationResultAsync(Guid.NewGuid(), new CardsQuery(), CancellationToken.None));

        Assert.That(exception, Is.Not.Null);
    }

    [Test]
    public void GetInsightPageDtoAsync_ThrowsInvalidOperation_WhenContentRetrieverDoesNotSupportServiceProvider()
    {
        var exception = Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _service.GetInsightPageDtoAsync(new InsightGroup(), CancellationToken.None));

        Assert.That(exception, Is.Not.Null);
    }
}
