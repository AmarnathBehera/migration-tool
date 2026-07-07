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

public class InsightSectionCardListingPageServiceTests
{
    private Mock<IContentRetriever> _contentRetrieverMock = null!;
    private InsightSectionCardListingPageService _service = null!;

    [SetUp]
    public void Setup()
    {
        _contentRetrieverMock = new Mock<IContentRetriever>();

        _service = new InsightSectionCardListingPageService(
            Mock.Of<IFilteringHelper>(),
            Mock.Of<IHeroService>(),
            _contentRetrieverMock.Object,
            Mock.Of<IInsightCardService>(),
            Mock.Of<IWebsiteChannelContext>());
    }

    [Test]
    public void GetContentPaginationResultAsync_ThrowsInvalidOperation_WhenContentRetrieverDoesNotSupportServiceProvider()
    {
        Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _service.GetContentPaginationResultAsync(Guid.NewGuid(), new CardsQuery(), CancellationToken.None));
    }
}
