using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.Services;

using BDO.GWT.Web.Core.LiveSite.DTOs;
using System.IO;
using System.Xml;

namespace BDO.GWT.Web.Core.Services;

public class SitemapXmlRenderer : ISitemapXmlRenderer
{

    private const string XhtmlNamespace = "http://www.w3.org/1999/xhtml";
    public string RenderXmlSitemap(SitemapDto sitemapDto)
    {
        var xmlDocument = new XmlDocument();

        var xmlDeclaration = xmlDocument.CreateXmlDeclaration("1.0", "utf-8", null);

        xmlDocument.AppendChild(xmlDeclaration);

        var xmlProcessingInstruction = xmlDocument.CreateProcessingInstruction("xml-stylesheet", "type=\"text/xsl\" href=\"/xsl/sitemap-stylesheet.xsl\"");

        xmlDocument.AppendChild(xmlProcessingInstruction);

        var urlSetElement = xmlDocument.CreateElement("urlset");

        urlSetElement.SetAttribute("xmlns:xhtml", "http://www.w3.org/1999/xhtml");

        urlSetElement.SetAttribute("xmlns", "http://www.sitemaps.org/schemas/sitemap/0.9");

        xmlDocument.AppendChild(urlSetElement);

        foreach (var url in sitemapDto.Urls)
        {
            var urlElement = CreateUrlElement(xmlDocument, url);

            urlSetElement.AppendChild(urlElement);
        }

        using var stringWriter = new StringWriter();

        using var xmlTextWriter = new XmlTextWriter(stringWriter);

        xmlDocument.WriteContentTo(xmlTextWriter);

        var xmlString = stringWriter.ToString();

        return xmlString;
    }

    private static XmlElement CreateUrlElement(XmlDocument xmlDocument, SitemapUrlDto sitemapUrlDto)
    {
        var urlElement = xmlDocument.CreateElement("url");

        var locElement = xmlDocument.CreateElement("loc");

        locElement.InnerText = sitemapUrlDto.LocationUri;

        urlElement.AppendChild(locElement);

        if (sitemapUrlDto.LastModificationDate.HasValue)
        {
            var lastModElement = xmlDocument.CreateElement("lastmod");

            lastModElement.InnerText = sitemapUrlDto.LastModificationDate.Value.ToString("yyyy-MM-ddTHH:mm:ss.fffffffzzz");

            urlElement.AppendChild(lastModElement);
        }

        foreach (var alternateUrl in sitemapUrlDto.AlternateUrls ?? [])
        {
            var linkElement = xmlDocument.CreateElement("xhtml", "link", XhtmlNamespace);

            linkElement.SetAttribute("href", alternateUrl.Url);

            linkElement.SetAttribute("rel", "alternate");

            linkElement.SetAttribute("hreflang", alternateUrl.LanguageTag);

            urlElement.AppendChild(linkElement);
        }

        return urlElement;
    }
}
