using BDO.GWT.GLOBAL.API.Helpers;
using CMS.ContentEngine;
using CMS.DataEngine;
using CMS.Websites;
using CMS.Websites.Routing;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Data;
using System.Runtime.CompilerServices;

namespace BDO.GWT.GLOBAL.API.Tests.Helpers;

[TestFixture]
public class ChannelWebsiteDomainServiceTests
{
    [Test]
    public async Task GetCurrentWebsiteDomainAsync_WhenWebsiteChannelContextAvailable_ReturnsDomain()
    {
        var websiteChannelInfoProviderMock = new Mock<IInfoProvider<WebsiteChannelInfo>>();
        var queryMock = new Mock<ObjectQuery<WebsiteChannelInfo>>();
        queryMock.Setup(x => x.WhereEquals(nameof(WebsiteChannelInfo.WebsiteChannelID), 77)).Returns(queryMock.Object);
        queryMock.Setup(x => x.TopN(1)).Returns(queryMock.Object);
        var websiteChannelInfo = CreateWebsiteChannelInfo("gwt-xbyk.dev.bdo.global");

        queryMock.Setup(x => x.GetEnumerableTypedResultAsync(
                It.IsAny<CommandBehavior>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken?>()))
            .ReturnsAsync([websiteChannelInfo]);
        websiteChannelInfoProviderMock.Setup(x => x.Get()).Returns(queryMock.Object);

        var websiteChannelContextMock = new Mock<IWebsiteChannelContext>();
        websiteChannelContextMock.SetupGet(x => x.WebsiteChannelID).Returns(77);

        var serviceProvider = new ServiceCollection()
            .AddSingleton(websiteChannelContextMock.Object)
            .BuildServiceProvider();

        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        httpContextAccessorMock.Setup(x => x.HttpContext).Returns(new DefaultHttpContext
        {
            RequestServices = serviceProvider
        });

        var sut = new ChannelWebsiteDomainService(
            Mock.Of<IInfoProvider<ChannelInfo>>(),
            websiteChannelInfoProviderMock.Object,
            httpContextAccessorMock.Object);

        var result = await sut.GetCurrentWebsiteDomainAsync();

        Assert.That(result, Is.EqualTo("gwt-xbyk.dev.bdo.global"));
    }

    [Test]
    public async Task GetCurrentWebsiteDomainAsync_WhenContextMissing_ReturnsNull()
    {
        var websiteChannelInfoProviderMock = new Mock<IInfoProvider<WebsiteChannelInfo>>();
        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext?)null);

        var sut = new ChannelWebsiteDomainService(
            Mock.Of<IInfoProvider<ChannelInfo>>(),
            websiteChannelInfoProviderMock.Object,
            httpContextAccessorMock.Object);

        var result = await sut.GetCurrentWebsiteDomainAsync();

        Assert.That(result, Is.Null);
    }

    private static FakeWebsiteChannelInfo CreateWebsiteChannelInfo(string domain)
    {
        var websiteChannelInfo = (FakeWebsiteChannelInfo)RuntimeHelpers.GetUninitializedObject(typeof(FakeWebsiteChannelInfo));
        websiteChannelInfo.WebsiteChannelDomain = domain;
        return websiteChannelInfo;
    }

    private sealed class FakeWebsiteChannelInfo : WebsiteChannelInfo
    {
        private string? websiteChannelDomain;

        public override string WebsiteChannelDomain
        {
            get => websiteChannelDomain ?? string.Empty;
            set => websiteChannelDomain = value;
        }
    }
}
