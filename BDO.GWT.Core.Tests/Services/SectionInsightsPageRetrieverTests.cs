//using BDO;
//using BDO.GWT.Common.Exceptions;
//using BDO.GWT.Web.Core.Services;
//using CMS.ContentEngine;
//using CMS.Websites.Routing;
//using Kentico.Content.Web.Mvc;
//using Moq;

//namespace BDO.GWT.Core.Tests.Services;

//public class SectionInsightsPageRetrieverTests
//{
//    [Test]
//    public async Task GetSectionInsightsPageAsync_ReturnsFirstPage_WhenPageExists()
//    {
//        var contentRetrieverMock = new Mock<IContentRetriever>();
//        var websiteChannelContextMock = new Mock<IWebsiteChannelContext>();

//        websiteChannelContextMock
//            .Setup(x => x.WebsiteChannelID)
//            .Returns(17);

//        var expectedPage = new SectionInsightsPage
//        {
//            InsightListFilters = "businessLine"
//        };

//        contentRetrieverMock
//            .Setup(x => x.RetrievePages<SectionInsightsPage, SectionInsightsPage>(
//                It.IsAny<RetrievePagesParameters>(),
//                It.IsAny<Action<RetrievePagesQueryParameters>>(),
//                It.IsAny<RetrievalCacheSettings>(),
//                It.IsAny<Func<IContentQueryDataContainer, SectionInsightsPage, Task<SectionInsightsPage>>>(),
//                It.IsAny<CancellationToken>()))
//            .ReturnsAsync(new[] { expectedPage });

//        var result = await SectionInsightsPageRetriever.GetSectionInsightsPageAsync(
//            contentRetrieverMock.Object,
//            websiteChannelContextMock.Object,
//            CancellationToken.None);

//        Assert.That(result, Is.SameAs(expectedPage));
//    }

//    [Test]
//    public async Task GetRequiredSectionInsightsPageAsync_ThrowsMissingSectionPageException_WhenNoPageExists()
//    {
//        var contentRetrieverMock = new Mock<IContentRetriever>();
//        var websiteChannelContextMock = new Mock<IWebsiteChannelContext>();

//        websiteChannelContextMock
//            .Setup(x => x.WebsiteChannelID)
//            .Returns(17);

//        contentRetrieverMock
//            .Setup(x => x.RetrievePages<SectionInsightsPage, SectionInsightsPage>(
//                It.IsAny<RetrievePagesParameters>(),
//                It.IsAny<Action<RetrievePagesQueryParameters>>(),
//                It.IsAny<RetrievalCacheSettings>(),
//                It.IsAny<Func<IContentQueryDataContainer, SectionInsightsPage, Task<SectionInsightsPage>>>(),
//                It.IsAny<CancellationToken>()))
//            .ReturnsAsync(Array.Empty<SectionInsightsPage>());

//        var exception = Assert.ThrowsAsync<MissingSectionPageException>(async () =>
//            await SectionInsightsPageRetriever.GetRequiredSectionInsightsPageAsync(
//                contentRetrieverMock.Object,
//                websiteChannelContextMock.Object,
//                CancellationToken.None));

//        Assert.That(exception, Is.Not.Null);
//        Assert.That(exception!.Message, Does.Contain(nameof(SectionInsightsPage)));
//    }
//}
