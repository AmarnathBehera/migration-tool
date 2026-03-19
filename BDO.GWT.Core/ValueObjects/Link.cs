using System;
using System.Diagnostics.CodeAnalysis;

namespace BDO.GWT.Core.ValueObjects;

public abstract record Link
{
    public string? Path { get; init; }

    public string? Query { get; init; }

    public string? Hash { get; init; }

    public static Link Parse(string uriString)
    {
        var normalizedUri = uriString.TrimStart('~').TrimStart('/');
        if (!Uri.TryCreate(normalizedUri, UriKind.RelativeOrAbsolute, out var uri))
        {
            throw new ArgumentException("URI is not in correct format.", nameof(uriString));
        }

        Link link;

        if (uri.IsAbsoluteUri)
        {
            int? port = uri.IsDefaultPort ? null : uri.Port;

            var (path, query, hash) = ExtractRelativeUriParameters(uri);

            link = new AbsoluteLink
            {
                Scheme = uri.Scheme,
                SlashesAfterColon = "uri",//UriUtility.ExtractSlashesAfterColon(uri),
                UserInfo = string.IsNullOrEmpty(uri.UserInfo) ? null : uri.UserInfo,
                HostName = uri.Host,
                Port = port,
                Path = path,
                Query = query,
                Hash = hash,
            };
        }
        else
        {
            var baseUri = new Uri("https://localhost");
            var absoluteUri = new Uri(baseUri, uri);

            var (path, query, hash) = ExtractRelativeUriParameters(absoluteUri);

            link = new RelativeLink
            {
                Path = path,
                Query = query,
                Hash = hash,
            };
        }

        return link;
    }

    public static bool TryParse(string? uriString, [NotNullWhen(true)] out Link? link)
    {
        if (uriString is null)
        {
            link = null;
            return false;
        }

        try
        {
            link = Parse(uriString);
            return true;
        }
        catch
        {
            link = null;
            return false;
        }
    }

    public abstract string ToHtmlCompatibleString();

    public AbsoluteLink ToAbsolute(AbsoluteLink rootPath)
    {
        var absoluteLink = rootPath with
        {
            Path = Path,
            Query = Query,
            Hash = Hash,
        };

        return absoluteLink;
    }

    public AbsoluteLink ToAbsolute(string rootPath)
    {
        var absoluteLink = new AbsoluteLink(rootPath)
        {
            Path = Path,
            Query = Query,
            Hash = Hash,
        };

        return absoluteLink;
    }

    private static (string? Path, string? Query, string? Hash) ExtractRelativeUriParameters(Uri uri)
    {
        var path = uri.AbsolutePath.TrimStart('/');
        var query = uri.Query.TrimStart('?');
        var hash = uri.Fragment.TrimStart('#');

        return (path == "" ? null : path, query == "" ? null : query, hash == "" ? null : hash);
    }
}
