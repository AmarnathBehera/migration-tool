using BDO.GWT.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BDO.GWT.GLOBAL.Tests.Controllers;

[TestFixture]
public class RssControllerTests
{
    private Mock<IRssPageService> _rssPageServiceMock;
    private BDO.GWT.Controllers.RssController _sut;

    [SetUp]
    public void SetUp()
    {
        _rssPageServiceMock = new Mock<IRssPageService>();
        _sut = new BDO.GWT.Controllers.RssController(_rssPageServiceMock.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _sut.Dispose();
    }

    [Test]
    public async Task Index_ReturnsContentResult()
    {
        _rssPageServiceMock
            .Setup(s => s.GetRssFeedXmlAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync("<rss/>");

        var result = await _sut.Index(CancellationToken.None);

        Assert.That(result, Is.InstanceOf<ContentResult>());
    }

    [Test]
    public async Task Index_ReturnsXmlContentType()
    {
        _rssPageServiceMock
            .Setup(s => s.GetRssFeedXmlAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync("<rss/>");

        var result = await _sut.Index(CancellationToken.None) as ContentResult;

        Assert.That(result!.ContentType, Is.EqualTo("application/xml"));
    }

    [Test]
    public async Task Index_ReturnsXmlFromService()
    {
        var xml = "<rss><channel><title>Test</title></channel></rss>";
        _rssPageServiceMock
            .Setup(s => s.GetRssFeedXmlAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(xml);

        var result = await _sut.Index(CancellationToken.None) as ContentResult;

        Assert.That(result!.Content, Is.EqualTo(xml));
    }

    [Test]
    public async Task Index_CallsServiceOnce()
    {
        _rssPageServiceMock
            .Setup(s => s.GetRssFeedXmlAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync("<rss/>");

        await _sut.Index(CancellationToken.None);

        _rssPageServiceMock.Verify(
            s => s.GetRssFeedXmlAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task Index_PropagatesCancellationToken()
    {
        var cts = new CancellationTokenSource();
        var token = cts.Token;

        _rssPageServiceMock
            .Setup(s => s.GetRssFeedXmlAsync(token))
            .ReturnsAsync("<rss/>");

        await _sut.Index(token);

        _rssPageServiceMock.Verify(
            s => s.GetRssFeedXmlAsync(token),
            Times.Once);
    }
}