using BDO.GWT.Core.Controllers;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.Services;
using BDO.GWT.Web.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.GLOBAL.Tests.Controllers;

[TestFixture]
public class SitemapXmlControllerTests
{
    private Mock<ISitemapService> _sitemapServiceMock;
    private Mock<ISitemapXmlRenderer> _sitemapXmlRendererMock;
    private SitemapXmlController _sut;

    [SetUp]
    public void SetUp()
    {
        _sitemapServiceMock = new Mock<ISitemapService>();
        _sitemapXmlRendererMock = new Mock<ISitemapXmlRenderer>();

        _sut = new SitemapXmlController(
            _sitemapServiceMock.Object,
            _sitemapXmlRendererMock.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _sut.Dispose();
    }

    [Test]
    public async Task Index_ReturnsContentResult()
    {
        _sitemapServiceMock
            .Setup(s => s.GetSitemapDtoAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SitemapDto { Urls = [] });
        _sitemapXmlRendererMock
            .Setup(r => r.RenderXmlSitemap(It.IsAny<SitemapDto>()))
            .Returns("<urlset/>");

        var result = await _sut.Index(CancellationToken.None);

        Assert.That(result, Is.InstanceOf<ContentResult>());
    }

    [Test]
    public async Task Index_ReturnsApplicationXmlContentType()
    {
        _sitemapServiceMock
            .Setup(s => s.GetSitemapDtoAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SitemapDto { Urls = [] });
        _sitemapXmlRendererMock
            .Setup(r => r.RenderXmlSitemap(It.IsAny<SitemapDto>()))
            .Returns("<urlset/>");

        var result = await _sut.Index(CancellationToken.None) as ContentResult;

        Assert.That(result!.ContentType, Is.EqualTo("application/xml"));
    }

    [Test]
    public async Task Index_ReturnsXmlFromRenderer()
    {
        var expectedXml = "<?xml version=\"1.0\"?><urlset><url><loc>https://example.com</loc></url></urlset>";

        _sitemapServiceMock
            .Setup(s => s.GetSitemapDtoAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SitemapDto { Urls = [] });
        _sitemapXmlRendererMock
            .Setup(r => r.RenderXmlSitemap(It.IsAny<SitemapDto>()))
            .Returns(expectedXml);

        var result = await _sut.Index(CancellationToken.None) as ContentResult;

        Assert.That(result!.Content, Is.EqualTo(expectedXml));
    }
}