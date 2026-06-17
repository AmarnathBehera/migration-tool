using BDO.GWT.Common.IO;
using BDO.GWT.Core.Data.Rss;
using System.Net.ServerSentEvents;
using System.ServiceModel.Syndication;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace BDO.GWT.Core.Utilities;

public static class RssUtility
{
    public static async Task<string> GetRssFeedXml(RssFeed data, string rssUrl)
    {
        var feed = ConvertToSyndicationFeed(data, rssUrl);
        var formatter = new Rss20FeedFormatter(feed, false);

        await using var stringWriter = new StringWriterWithEncoding(Encoding.UTF8);
        var xmlWriterSettings = new XmlWriterSettings { Async = true, Indent = true };
        await using var xmlWriter = XmlWriter.Create(stringWriter, xmlWriterSettings);

        formatter.WriteTo(xmlWriter);
        await xmlWriter.FlushAsync();

        return stringWriter.ToString();
    }

    private static SyndicationFeed ConvertToSyndicationFeed(RssFeed rssFeed, string rssUrl)
    {
        var channel = rssFeed.Channel;
        var feed = new SyndicationFeed(channel.Title, channel.Description, new Uri(channel.Link))
        {
            Language = channel.Language,
            Items = channel.Items.Select(ConvertToSyndicationItem),
        };

        if (!string.IsNullOrEmpty(channel.Category))
            feed.Categories.Add(new SyndicationCategory(channel.Category));

        feed.AttributeExtensions.Add(
            new XmlQualifiedName("atom", XNamespace.Xmlns.ToString()),
            "http://www.w3.org/2005/Atom");

        var atomNamespace = XNamespace.Get("http://www.w3.org/2005/Atom");
        feed.ElementExtensions.Add(new XElement(atomNamespace + "link",
            new XAttribute("href", rssUrl),
            new XAttribute("rel", "self")));

        return feed;
    }

    private static SyndicationItem ConvertToSyndicationItem(RssChannelItem x)
    {
        var item = new SyndicationItem
        {
            Summary = string.IsNullOrEmpty(x.Description) ? null : new TextSyndicationContent(x.Description),
            Id = x.GUID,
            PublishDate = x.PubDate is not null && x.PubDate != DateTimeOffset.MinValue ? x.PubDate.Value : default,
            Title = x.Title != null ? new TextSyndicationContent(x.Title) : null,
        };

        if (x.Author != null)
            item.Authors.Add(new SyndicationPerson(x.Author));

        foreach (var category in x.Categories)
            item.Categories.Add(new SyndicationCategory(category));

        if (x.Enclosure != null)
            item.ElementExtensions.Add(new XElement("enclosure",
                new XAttribute("length", x.Enclosure.Length),
                new XAttribute("type", x.Enclosure.Type),
                new XAttribute("url", x.Enclosure.Url)));

        if (x.Link != null)
            item.Links.Add(new SyndicationLink(x.Link, "alternate", null, null, 0));

        return item;
    }
}