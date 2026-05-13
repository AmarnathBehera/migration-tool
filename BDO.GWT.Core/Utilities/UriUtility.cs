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

