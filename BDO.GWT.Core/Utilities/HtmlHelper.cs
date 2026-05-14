


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




}

