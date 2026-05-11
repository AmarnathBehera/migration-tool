using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using BDO.GWT.Core.Extensions;

namespace BDO.GWT.Core.Utilities;

public static class UriUtility
{
    public static string? CombinePaths(string? path1, string? path2)
    {
        var combinedPath = (path1, path2) switch
        {
            (null, null) => null,
            (null, "") => "",
            (null, _) => path2,
            ("", null) => "",
            ("", "") => "",
            ("", _) => path2,
            (_, null) => path1,
            (_, "") => path1,
            (_, _) => $"{path1.TrimEnd('/')}/{path2.TrimStart('/')}",
        };

        return combinedPath;
    }

    public static string? ExtractSlashesAfterColon(Uri uri)
    {
        if (uri.OriginalString.StartsWith($"{uri.Scheme}:///"))
        {
            return "///";
        }

        if (uri.OriginalString.StartsWith($"{uri.Scheme}://"))
        {
            return "//";
        }

        if (uri.OriginalString.StartsWith($"{uri.Scheme}:/"))
        {
            return "/";
        }

        return null;
    }
}

public static class HtmlUtility
{
    private static readonly string[] UrlTypeHtmlAttributes = { "href", "src", "srcset", "poster" };

    [return: NotNullIfNotNull("html")]
    public static string? RemoveTildeFromUrlTypeHtmlAttributes(string? html)
    {
        if (html == null)
        {
            return null;
        }

        var indexOfTildePath = html.IndexOf("~/", StringComparison.Ordinal);

        if (indexOfTildePath < 1)
        {
            return html;
        }

        html = ProcessHtml(html, indexOfTildePath);

        return html;
    }

    private static string ProcessHtml(string html, int indexOfTildePath)
    {
        var stringBuilder = new StringBuilder(html.Length);

        var startIndex = 0;

        for (; indexOfTildePath >= 1; indexOfTildePath = html.IndexOf("~/", indexOfTildePath + 2, StringComparison.Ordinal))
        {
            var substring = html.AsSpan().Slice(startIndex, indexOfTildePath - startIndex).TrimEnd(null);

            substring = RemoveTrailingQuotes(substring);

            if (!IsTildeWithinHtmlAttribute(substring))
            {
                continue;
            }

            if (startIndex < indexOfTildePath)
            {
                stringBuilder.Append(html, startIndex, indexOfTildePath - startIndex);
            }

            startIndex = indexOfTildePath + 1;
        }

        if (startIndex < html.Length)
        {
            stringBuilder.Append(html, startIndex, html.Length - startIndex);
        }

        html = stringBuilder.ToString();

        return html;
    }

    private static ReadOnlySpan<char> RemoveTrailingQuotes(ReadOnlySpan<char> substring)
    {
        var lastCharacter = substring[^1];

        return lastCharacter switch
        {
            '"' or '\'' => substring.TrimEnd(new[] { lastCharacter }).TrimEnd(null),
            _ when substring.EndsWith("&quot;", StringComparison.Ordinal) => substring[..^"&quot;".Length].TrimEnd(null),
            _ => substring,
        };
    }

    private static bool IsTildeWithinHtmlAttribute(ReadOnlySpan<char> substring)
    {
        if (substring.EndsWith("=", StringComparison.Ordinal))
        {
            if (substring.TrimEnd(new[] { '=' }).TrimEnd(null).EndsWithAny(StringComparison.OrdinalIgnoreCase, UrlTypeHtmlAttributes))
            {
                return true;
            }
        }
        else if (substring.EndsWith("(", StringComparison.Ordinal))
        {
            if (substring.TrimEnd(new[] { '(' }).TrimEnd(null).EndsWith("url", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
