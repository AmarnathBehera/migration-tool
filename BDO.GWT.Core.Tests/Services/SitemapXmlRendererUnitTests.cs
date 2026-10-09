
using BDO.GWT.Core.DTOs;
using BDO.GWT.Web.Core.Services;

namespace BDO.GWT.Core.Tests.Services;


public class SitemapXmlRendererUnitTests
{
    private readonly SitemapXmlRenderer _sitemapXmlRenderer = new();

    [Test]
    public void RenderXmlSitemapAsync_ReturnsCorrectXml()
    {
        DateTime[] dateTimes = 
        [
            DateTime.Parse("2024-07-02T11:44:49.7974465+03:00"),
            DateTime.Parse("2024-04-03T18:41:49.8913313+03:00"),
        ];

        var expectedXmlString = $"""<?xml version="1.0" encoding="utf-8"?><?xml-stylesheet type="text/xsl" href="/xsl/sitemap-stylesheet.xsl"?><urlset xmlns:xhtml="http://www.w3.org/1999/xhtml" xmlns="http://www.sitemaps.org/schemas/sitemap/0.9"><url><loc>https://localhost:5001/en-gb/about-us/about-page</loc><lastmod>{dateTimes[0]:yyyy-MM-ddTHH:mm:ss.fffffffzzz}</lastmod></url><url><loc>https://localhost:5001/en-gb/blogs/bdo-healthcare</loc><lastmod>{dateTimes[1]:yyyy-MM-ddTHH:mm:ss.fffffffzzz}</lastmod><xhtml:link href="https://localhost:5001/en-gb/blogs/bdo-healthcare" rel="alternate" hreflang="en-GB" /><xhtml:link href="https://localhost:5001/lt-lt/blogs/bdo-healthcare" rel="alternate" hreflang="lt-LT" /></url></urlset>""";

        var sitemapDto = new SitemapDto
        {
            Urls =
            [
                new SitemapUrlDto
                {
                    LocationUri = "https://localhost:5001/en-gb/about-us/about-page",
                    LastModificationDate = dateTimes[0],
                },
                new SitemapUrlDto
                {
                    LocationUri = "https://localhost:5001/en-gb/blogs/bdo-healthcare",
                    LastModificationDate = dateTimes[1],
                    AlternateUrls =
                    [
                        new SitemapAlternateUrlDto
                        {
                            Url = "https://localhost:5001/en-gb/blogs/bdo-healthcare",
                            LanguageTag = "en-GB",
                        },
                        new SitemapAlternateUrlDto
                        {
                            Url = "https://localhost:5001/lt-lt/blogs/bdo-healthcare",
                            LanguageTag = "lt-LT",
                        },
                    ],
                },
            ],
        };

        var actualXmlString = _sitemapXmlRenderer.RenderXmlSitemap(sitemapDto);

        Assert.That(actualXmlString, Is.EqualTo(expectedXmlString));
    }
}
