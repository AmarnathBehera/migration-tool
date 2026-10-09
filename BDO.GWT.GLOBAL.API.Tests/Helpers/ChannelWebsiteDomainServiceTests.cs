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

    [Test]
    public async Task GetCurrentWebsiteDomainAsync_WhenNoChannelContextAndHostMatches_ReturnsDomainFromHost()
    {
        // Arrange: No channel context, but host matches a domain in the system
        var hostDomain = "production.example.com";
        var websiteChannelInfoProviderMock = new Mock<IInfoProvider<WebsiteChannelInfo>>();
        var queryMock = new Mock<ObjectQuery<WebsiteChannelInfo>>();

        var websiteChannelInfo = CreateWebsiteChannelInfo(hostDomain);

        // First call to check by domain, second call for localhost fallback
        queryMock.SetupSequence(x => x.GetEnumerableTypedResultAsync(
                It.IsAny<CommandBehavior>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken?>()))
            .ReturnsAsync([websiteChannelInfo])
            .ReturnsAsync([]);

        queryMock.Setup(x => x.WhereEquals(nameof(WebsiteChannelInfo.WebsiteChannelDomain), hostDomain))
            .Returns(queryMock.Object);
        queryMock.Setup(x => x.TopN(1)).Returns(queryMock.Object);

        websiteChannelInfoProviderMock.Setup(x => x.Get()).Returns(queryMock.Object);

        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        var httpContext = new DefaultHttpContext
        {
            RequestServices = new ServiceCollection().BuildServiceProvider()
        };
        httpContext.Request.Scheme = "https";
        httpContext.Request.Host = new HostString(hostDomain);
        httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        var sut = new ChannelWebsiteDomainService(
            Mock.Of<IInfoProvider<ChannelInfo>>(),
            websiteChannelInfoProviderMock.Object,
            httpContextAccessorMock.Object);

        // Act
        var result = await sut.GetCurrentWebsiteDomainAsync();

        // Assert
        Assert.That(result, Is.EqualTo(hostDomain));
    }

    [Test]
    public async Task GetCurrentWebsiteDomainAsync_WhenNoChannelContextAndLocalhostHost_ReturnsFallbackDomain()
    {
        // Arrange: No channel context, localhost host
        var websiteChannelInfoProviderMock = new Mock<IInfoProvider<WebsiteChannelInfo>>();
        var queryMock = new Mock<ObjectQuery<WebsiteChannelInfo>>();

        // First call returns no result (no matching domain), second call returns no channels
        queryMock.SetupSequence(x => x.GetEnumerableTypedResultAsync(
                It.IsAny<CommandBehavior>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken?>()))
            .ReturnsAsync([])
            .ReturnsAsync([]);

        queryMock.Setup(x => x.WhereEquals(It.IsAny<string>(), It.IsAny<object>()))
            .Returns(queryMock.Object);
        queryMock.Setup(x => x.TopN(1)).Returns(queryMock.Object);

        websiteChannelInfoProviderMock.Setup(x => x.Get()).Returns(queryMock.Object);

        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        var httpContext = new DefaultHttpContext
        {
            RequestServices = new ServiceCollection().BuildServiceProvider()
        };
        httpContext.Request.Scheme = "http";
        httpContext.Request.Host = new HostString("localhost:5000");
        httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        var sut = new ChannelWebsiteDomainService(
            Mock.Of<IInfoProvider<ChannelInfo>>(),
            websiteChannelInfoProviderMock.Object,
            httpContextAccessorMock.Object);

        // Act
        var result = await sut.GetCurrentWebsiteDomainAsync();

        // Assert: Should return the fallback domain (scheme://host)
        Assert.That(result, Is.EqualTo("http://localhost:5000"));
    }

    [Test]
    public async Task GetCurrentWebsiteDomainAsync_WhenNoChannelContextAndEmptyHost_ReturnsNull()
    {
        // Arrange: No channel context, empty host
        var websiteChannelInfoProviderMock = new Mock<IInfoProvider<WebsiteChannelInfo>>();
        var queryMock = new Mock<ObjectQuery<WebsiteChannelInfo>>();

        queryMock.Setup(x => x.GetEnumerableTypedResultAsync(
                It.IsAny<CommandBehavior>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken?>()))
            .ReturnsAsync([]);

        queryMock.Setup(x => x.WhereEquals(It.IsAny<string>(), It.IsAny<object>()))
            .Returns(queryMock.Object);
        queryMock.Setup(x => x.TopN(1)).Returns(queryMock.Object);

        websiteChannelInfoProviderMock.Setup(x => x.Get()).Returns(queryMock.Object);

        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        var httpContext = new DefaultHttpContext
        {
            RequestServices = new ServiceCollection().BuildServiceProvider()
        };
        httpContext.Request.Scheme = "http";
        httpContext.Request.Host = new HostString(); // Empty host
        httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        var sut = new ChannelWebsiteDomainService(
            Mock.Of<IInfoProvider<ChannelInfo>>(),
            websiteChannelInfoProviderMock.Object,
            httpContextAccessorMock.Object);

        // Act
        var result = await sut.GetCurrentWebsiteDomainAsync();

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetCurrentWebsiteDomainAsync_WhenNoChannelContextAndLocalhostWithNoChannels_ReturnsFallbackDomain()
    {
        // Arrange: localhost but no channels available to fall back to
        var websiteChannelInfoProviderMock = new Mock<IInfoProvider<WebsiteChannelInfo>>();
        var queryMock = new Mock<ObjectQuery<WebsiteChannelInfo>>();

        queryMock.SetupSequence(x => x.GetEnumerableTypedResultAsync(
                It.IsAny<CommandBehavior>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken?>()))
            .ReturnsAsync([])
            .ReturnsAsync([]); // No channels even for fallback

        queryMock.Setup(x => x.WhereEquals(It.IsAny<string>(), It.IsAny<object>()))
            .Returns(queryMock.Object);
        queryMock.Setup(x => x.TopN(1)).Returns(queryMock.Object);

        websiteChannelInfoProviderMock.Setup(x => x.Get()).Returns(queryMock.Object);

        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        var httpContext = new DefaultHttpContext
        {
            RequestServices = new ServiceCollection().BuildServiceProvider()
        };
        httpContext.Request.Scheme = "http";
        httpContext.Request.Host = new HostString("localhost:3000");
        httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        var sut = new ChannelWebsiteDomainService(
            Mock.Of<IInfoProvider<ChannelInfo>>(),
            websiteChannelInfoProviderMock.Object,
            httpContextAccessorMock.Object);

        // Act
        var result = await sut.GetCurrentWebsiteDomainAsync();

        // Assert: Still returns the fallback domain
        Assert.That(result, Is.EqualTo("http://localhost:3000"));
    }

    [Test]
    public async Task GetCurrentWebsiteDomainAsync_WhenNoChannelContextAndLocalhostFallback_ReturnsFirstChannelDomain()
    {
        // Arrange: localhost fallback to get first available channel
        var firstChannelDomain = "first-channel.example.com";
        var websiteChannelInfoProviderMock = new Mock<IInfoProvider<WebsiteChannelInfo>>();
        var queryMock = new Mock<ObjectQuery<WebsiteChannelInfo>>();

        var firstChannelInfo = CreateWebsiteChannelInfo(firstChannelDomain);

        // First call: no matching domain, second call: get first channel for localhost
        queryMock.SetupSequence(x => x.GetEnumerableTypedResultAsync(
                It.IsAny<CommandBehavior>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken?>()))
            .ReturnsAsync([])
            .ReturnsAsync([firstChannelInfo]);

        queryMock.Setup(x => x.WhereEquals(It.IsAny<string>(), It.IsAny<object>()))
            .Returns(queryMock.Object);
        queryMock.Setup(x => x.TopN(1)).Returns(queryMock.Object);

        websiteChannelInfoProviderMock.Setup(x => x.Get()).Returns(queryMock.Object);

        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        var httpContext = new DefaultHttpContext
        {
            RequestServices = new ServiceCollection().BuildServiceProvider()
        };
        httpContext.Request.Scheme = "http";
        httpContext.Request.Host = new HostString("localhost:5000");
        httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        var sut = new ChannelWebsiteDomainService(
            Mock.Of<IInfoProvider<ChannelInfo>>(),
            websiteChannelInfoProviderMock.Object,
            httpContextAccessorMock.Object);

        // Act
        var result = await sut.GetCurrentWebsiteDomainAsync();

        // Assert
        Assert.That(result, Is.EqualTo(firstChannelDomain));
    }


    [Test]
    public async Task GetCurrentWebsiteDomainAsync_WithValidChannelContextAndValidDomain_ReturnsContextDomain()
    {
        // Arrange: Valid channel context with domain
        var contextDomain = "context-domain.example.com";
        var websiteChannelInfoProviderMock = new Mock<IInfoProvider<WebsiteChannelInfo>>();
        var queryMock = new Mock<ObjectQuery<WebsiteChannelInfo>>();

        var contextChannelInfo = CreateWebsiteChannelInfo(contextDomain);

        queryMock.Setup(x => x.WhereEquals(nameof(WebsiteChannelInfo.WebsiteChannelID), 99))
            .Returns(queryMock.Object);
        queryMock.Setup(x => x.TopN(1)).Returns(queryMock.Object);
        queryMock.Setup(x => x.GetEnumerableTypedResultAsync(
                It.IsAny<CommandBehavior>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken?>()))
            .ReturnsAsync([contextChannelInfo]);

        websiteChannelInfoProviderMock.Setup(x => x.Get()).Returns(queryMock.Object);

        var websiteChannelContextMock = new Mock<IWebsiteChannelContext>();
        websiteChannelContextMock.SetupGet(x => x.WebsiteChannelID).Returns(99);

        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        var httpContext = new DefaultHttpContext
        {
            RequestServices = new ServiceCollection()
                .AddSingleton(websiteChannelContextMock.Object)
                .BuildServiceProvider()
        };
        httpContext.Request.Scheme = "https";
        httpContext.Request.Host = new HostString("different-host.com");
        httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        var sut = new ChannelWebsiteDomainService(
            Mock.Of<IInfoProvider<ChannelInfo>>(),
            websiteChannelInfoProviderMock.Object,
            httpContextAccessorMock.Object);

        // Act
        var result = await sut.GetCurrentWebsiteDomainAsync();

        // Assert: Should return context domain, not host domain
        Assert.That(result, Is.EqualTo(contextDomain));
    }

    [Test]
    public async Task GetCurrentWebsiteDomainAsync_WithValidChannelContextButNoDomain_ReturnsFallback()
    {
        // Arrange: Valid channel context but no domain set, falls back to request domain
        var websiteChannelInfoProviderMock = new Mock<IInfoProvider<WebsiteChannelInfo>>();
        var queryMock = new Mock<ObjectQuery<WebsiteChannelInfo>>();

        queryMock.Setup(x => x.WhereEquals(nameof(WebsiteChannelInfo.WebsiteChannelID), 1))
            .Returns(queryMock.Object);
        queryMock.Setup(x => x.TopN(1)).Returns(queryMock.Object);
        queryMock.Setup(x => x.GetEnumerableTypedResultAsync(
                It.IsAny<CommandBehavior>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken?>()))
            .ReturnsAsync([]); // No channel found

        websiteChannelInfoProviderMock.Setup(x => x.Get()).Returns(queryMock.Object);

        var websiteChannelContextMock = new Mock<IWebsiteChannelContext>();
        websiteChannelContextMock.SetupGet(x => x.WebsiteChannelID).Returns(1);

        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        var httpContext = new DefaultHttpContext
        {
            RequestServices = new ServiceCollection()
                .AddSingleton(websiteChannelContextMock.Object)
                .BuildServiceProvider()
        };
        httpContext.Request.Scheme = "https";
        httpContext.Request.Host = new HostString("fallback-host.com");
        httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        var sut = new ChannelWebsiteDomainService(
            Mock.Of<IInfoProvider<ChannelInfo>>(),
            websiteChannelInfoProviderMock.Object,
            httpContextAccessorMock.Object);

        // Act
        var result = await sut.GetCurrentWebsiteDomainAsync();

        // Assert
        Assert.That(result, Is.EqualTo("https://fallback-host.com"));
    }

    [Test]
    public void GetWebsiteDomainAsync_WithNullChannelName_ReturnsNull()
    {
        // Arrange
        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        var sut = new ChannelWebsiteDomainService(
            Mock.Of<IInfoProvider<ChannelInfo>>(),
            Mock.Of<IInfoProvider<WebsiteChannelInfo>>(),
            httpContextAccessorMock.Object);

        // Act
        var result = sut.GetWebsiteDomainAsync(null!, CancellationToken.None);

        // Assert
        Assert.That(result.Result, Is.Null);
    }

    [Test]
    public void GetWebsiteDomainAsync_WithEmptyChannelName_ReturnsNull()
    {
        // Arrange
        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        var sut = new ChannelWebsiteDomainService(
            Mock.Of<IInfoProvider<ChannelInfo>>(),
            Mock.Of<IInfoProvider<WebsiteChannelInfo>>(),
            httpContextAccessorMock.Object);

        // Act
        var result = sut.GetWebsiteDomainAsync(string.Empty, CancellationToken.None);

        // Assert
        Assert.That(result.Result, Is.Null);
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
