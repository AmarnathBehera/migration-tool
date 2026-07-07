using BDO.GWT.Common.Exceptions;
using BDO.GWT.Core;
using BDO.GWT.Core.Data.Queries;
using BDO.GWT.Core.Helpers;
using BDO.GWT.Core.Services;
using BDO.GWT.Web.Core.Services;
using CMS.ContentEngine;
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
    public void GetContentPaginationResultAsync_ThrowsContentItemNotFoundException_WhenPageIsNotFound()
    {
        var pageGuid = Guid.NewGuid();

        _contentRetrieverMock
            .Setup(x => x.RetrievePages<IWebPageFieldsSource, IWebPageFieldsSource>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<Func<IContentQueryDataContainer, IWebPageFieldsSource, Task<IWebPageFieldsSource>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<IWebPageFieldsSource>());

        var exception = Assert.ThrowsAsync<ContentItemNotFoundException>(async () =>
            await _service.GetContentPaginationResultAsync(pageGuid, new CardsQuery(), CancellationToken.None));

        Assert.That(exception!.Message, Does.Contain(pageGuid.ToString()));
    }

    [Test]
    public void GetContentPaginationResultAsync_ThrowsMissingSectionPageException_WhenNoSectionInsightsPageExists()
    {
        var pageGuid = Guid.NewGuid();
        var pageMock = new Mock<IWebPageFieldsSource>();
        pageMock.Setup(x => x.SystemFields).Returns(new WebPageFields
        {
            WebPageItemTreePath = "/Insights/Category"
        });

        _contentRetrieverMock
            .Setup(x => x.RetrievePages<IWebPageFieldsSource, IWebPageFieldsSource>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<Func<IContentQueryDataContainer, IWebPageFieldsSource, Task<IWebPageFieldsSource>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { pageMock.Object });
        _contentRetrieverMock
            .Setup(x => x.RetrievePages<SectionInsightsPage, SectionInsightsPage>(
                It.IsAny<RetrievePagesParameters>(),
                It.IsAny<Action<RetrievePagesQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<Func<IContentQueryDataContainer, SectionInsightsPage, Task<SectionInsightsPage>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<SectionInsightsPage>());

        var exception = Assert.ThrowsAsync<MissingSectionPageException>(async () =>
            await _service.GetContentPaginationResultAsync(pageGuid, new CardsQuery(), CancellationToken.None));

        Assert.That(exception!.Message, Does.Contain(nameof(SectionInsightsPage)));
    }

    [Test]
    public void GetContentSectionDtoAsync_ThrowsContentItemNotFoundException_WhenCurrentPageIsNull()
    {
        _contentRetrieverMock
            .Setup(x => x.RetrieveCurrentPage<SectionInsightsPage>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<Action<RetrieveCurrentPageQueryParameters>>(),
                It.IsAny<RetrievalCacheSettings>(),
                It.IsAny<Func<IContentQueryDataContainer, SectionInsightsPage, Task<SectionInsightsPage>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((SectionInsightsPage)null!);

        var exception = Assert.ThrowsAsync<ContentItemNotFoundException>(async () =>
            await _service.GetContentSectionDtoAsync(new SectionInsightsPage(), CancellationToken.None));

        Assert.That(exception!.Message, Does.Contain(nameof(SectionInsightsPage)));
    }
}