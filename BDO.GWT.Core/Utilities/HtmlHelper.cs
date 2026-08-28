


using BDO.GWT.Core.ValueObjects;
using HtmlAgilityPack;
using System.Web;

namespace BDO.GWT.Core.Utilities;

public static class HtmlHelper
{
    public static string? ExtractTextFromHtml(string? html)
    {
        if (html == null) return null;

        var htmlDocument = new HtmlDocument();

        htmlDocument.LoadHtml(html);

        var text = HttpUtility.HtmlDecode(htmlDocument.DocumentNode.InnerText);

        return text;
    }

    public static List<string> ExtractUrlsFromHtml(string? html)
    {
        if (html == null)
        {
            return new List<string>();
        }

        var htmlDocument = new HtmlDocument();

        htmlDocument.LoadHtml(html);

        var links = htmlDocument.DocumentNode
            .SelectNodes("//a[@href]")?
            .Select(node => node.GetAttributeValue("href", string.Empty))
            .Where(href => !string.IsNullOrWhiteSpace(href))
            .ToList() ?? new List<string>();

        return links;
    }

    /// <summary>
    /// Extracts links from HTML and converts relative links to absolute links if necessary.
    /// </summary>
    /// <param name="html"></param>
    /// <param name="ensureAbsolute">Makes sure the returned links are always Absolute. Either by filtering out the relative ones, or by converting the relative ones to absolute using the presentationUrl.</param>
    /// <param name="presentationUrl">URL to be used as base for the relative links.</param>
    /// <returns></returns>
    public static List<Link> ExtractLinksFromHtml(string? html, bool ensureAbsolute = false, AbsoluteLink? presentationUrl = null)
    {
        var links = ExtractUrlsFromHtml(html)
            .Select(Link.Parse)
            .ToList();

        if (presentationUrl is null)
        {
            return links.Where(link => link is AbsoluteLink).ToList();
        }

        return links.Select(link => link is RelativeLink ? link.ToAbsolute(presentationUrl) : link).ToList();
    }


}

