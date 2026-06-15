using BDO.GWT.Common.IO;
using BDO.GWT.Core.Data.Rss;
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

        var xmlWriterSettings = new XmlWriterSettings
        {
            Async = true,
            Indent = true,
        };

        await using var xmlWriter = XmlWriter.Create(stringWriter, xmlWriterSettings);

        formatter.WriteTo(xmlWriter);

        await xmlWriter.FlushAsync();

        var result = stringWriter.ToString();

        return result;
    }

    private static SyndicationFeed ConvertToSyndicationFeed(RssFeed rssFeed, string rssUrl)
    {
        var channel = rssFeed.Channel;

        var feedAlternateLink = new Uri(channel.Link);

        var feed = new SyndicationFeed(channel.Title, channel.Description, feedAlternateLink)
        {
            Language = channel.Language,
            Items = channel.Items.Select(x =>
            {
                var item = new SyndicationItem
                {
                    Summary = string.IsNullOrEmpty(x.Description) ? null : new TextSyndicationContent(x.Description),
                    Id = x.GUID,
                    PublishDate = x.PubDate != null && x.PubDate != DateTimeOffset.MinValue ? x.PubDate.Value : default,
                    Title = x.Title != null ? new TextSyndicationContent(x.Title) : null,
                };

                if (x.Author != null)
                {
                    var syndicationPerson = new SyndicationPerson(x.Author);
                    item.Authors.Add(syndicationPerson);
                }

                foreach (var syndicationCategory in x.Categories.Select(category => new SyndicationCategory(category)))
                {
                    item.Categories.Add(syndicationCategory);
                }

                if (x.Enclosure != null)
                {
                    item.ElementExtensions.Add(
                        new XElement("enclosure",
                            new XAttribute("length", x.Enclosure.Length),
                            new XAttribute("type", x.Enclosure.Type),
                            new XAttribute("url", x.Enclosure.Url)
                        )
                    );
                }

                if (x.Link != null)
                {
                    var syndicationLink = new SyndicationLink(x.Link, "alternate", null, null, 0);
                    item.Links.Add(syndicationLink);
                }

                return item;
            }),
        };

        if (!string.IsNullOrEmpty(channel.Category))
        {
            var syndicationCategory = new SyndicationCategory(channel.Category);

            feed.Categories.Add(syndicationCategory);
        }

        feed.AttributeExtensions.Add(
            new XmlQualifiedName("atom", XNamespace.Xmlns.ToString()),
            "http://www.w3.org/2005/Atom"
        );

        var atomNamespace = XNamespace.Get("http://www.w3.org/2005/Atom");

        feed.ElementExtensions.Add(new XElement(atomNamespace + "link", new XAttribute("href", rssUrl), new XAttribute("rel", "self")));

        return feed;
    }
}
