using BDO.GWT.Core.Utilities;
using System;

namespace BDO.GWT.Core.ValueObjects;

public record AbsoluteLink : Link
{
    public string Scheme { get; init; } = null!;

    public string? SlashesAfterColon { get; set; }

    public string? UserInfo { get; set; }

    public string HostName { get; init; } = null!;

    public int? Port { get; init; }

    public AbsoluteLink()
    {
    }

    public AbsoluteLink(string uriString)
    {
        var link = Parse(uriString);

        if (link is not AbsoluteLink absoluteLink)
        {
            throw new ArgumentException("URI is not in correct format.", nameof(uriString));
        }

        Scheme = absoluteLink.Scheme;
        SlashesAfterColon = absoluteLink.SlashesAfterColon;
        UserInfo = absoluteLink.UserInfo;
        HostName = absoluteLink.HostName;
        Port = absoluteLink.Port;
        Path = absoluteLink.Path;
        Query = absoluteLink.Query;
        Hash = absoluteLink.Hash;
    }

    public static AbsoluteLink operator +(AbsoluteLink absoluteLink, Link link)
    {
        var combinedLink = new AbsoluteLink
        {
            Scheme = absoluteLink.Scheme,
            SlashesAfterColon = absoluteLink.SlashesAfterColon,
            UserInfo = absoluteLink.UserInfo,
            HostName = absoluteLink.HostName,
            Port = absoluteLink.Port,
            Path = UriUtility.CombinePaths(absoluteLink.Path, link.Path),
            Query = link.Query,
            Hash = link.Hash,
        };

        return combinedLink;
    }

    public override string ToString()
    {
        var uriString = $"{Scheme}:";

        if (SlashesAfterColon != null)
        {
            uriString += SlashesAfterColon;
        }

        if (UserInfo != null)
        {
            uriString += $"{UserInfo}@";
        }

        uriString += HostName;

        if (Port != null)
        {
            uriString += $":{Port}";
        }

        if (Path != null)
        {
            uriString += $"{(string.IsNullOrEmpty(HostName) ? "" : "/")}{Path}";
        }

        if (Query != null)
        {
            uriString += $"?{Query}";
        }

        if (Hash != null)
        {
            uriString += $"#{Hash}";
        }

        return uriString;
    }

    public override string ToHtmlCompatibleString()
    {
        return ToString();
    }

    public Uri ToUri()
    {
        var htmlCompatibleString = ToHtmlCompatibleString();

        var uri = new Uri(htmlCompatibleString);

        return uri;
    }
}
