
//namespace BDO.GWT.Core.Services;
//using System;
//using System.Linq;
//using System.Threading;
//using System.Threading.Tasks;



//public interface IRssPageService
//{
//    Task<string> GetRssFeedXmlAsync(CancellationToken cancellationToken);
//}

//public class RssPageService : IRssPageService
//{
//    private readonly IFileService _fileService;
//    private readonly IMultiTypePageRetriever _multiTypePageRetriever;
//    private readonly IPageUrlService _pageUrlService;
//    private readonly IRetriever<RssCategory> _rssCategoryRetriever;
//    private readonly IRetriever<Site> _siteRetriever;
//    private readonly ISiteContext _siteContext;

//    public RssPageService(IFileService fileService, IMultiTypePageRetriever multiTypePageRetriever, IPageUrlService pageUrlService, IRetriever<RssCategory> rssCategoryRetriever, IRetriever<Site> siteRetriever, ISiteContext siteContext)
//    {
//        _fileService = fileService;
//        _multiTypePageRetriever = multiTypePageRetriever;
//        _pageUrlService = pageUrlService;
//        _rssCategoryRetriever = rssCategoryRetriever;
//        _siteRetriever = siteRetriever;
//        _siteContext = siteContext;
//    }

//    public async Task<string> GetRssFeedXmlAsync(Rss currentPage, CancellationToken cancellationToken)
//    {
//        var currentSite = await _siteRetriever.RetrieveEntityAsync(query => query.Where(x => x.ID == _siteContext.CurrentSiteID), cancellationToken) ?? throw new InvalidOperationException("Could not retrieve current site entity.");

//        var pageAbsoluteUrl = await _pageUrlService.GetPageAbsoluteUrlAsync(currentSite.PresentationURL, currentPage.NodeID, currentPage.LanguageTag, cancellationToken);

//        var feed = await BuildRssFeedAsync(currentSite, currentPage, cancellationToken);

//        var rssUrl = pageAbsoluteUrl.ToHtmlCompatibleString();

//        var rssFeedXml = await RssUtility.GetRssFeedXml(feed, rssUrl);

//        return rssFeedXml;
//    }

//    private async Task<RssFeed> BuildRssFeedAsync(Site site, Rss page, CancellationToken cancellationToken)
//    {
//        var feed = new RssFeed
//        {
//            Channel = new RssChannel
//            {
//                Title = page.RssFeedTitle,
//                Link = site.PresentationURL.ToHtmlCompatibleString(),
//                Description = page.RssFeedDescription,
//                Language = page.LanguageTag.ToString(),
//                Category = page.RssCategory,
//            },
//        };

//        var ancestorPage = await _multiTypePageRetriever.RetrievePageAsync(query => query.Where(x => x.NodeGUID == page.RssPath), cancellationToken);

//        var rssItemPages = ancestorPage is not null
//            ? (await _multiTypePageRetriever.RetrievePagesAsync<IRssItem>(query => query
//                .Where(x => x.NodeAliasPath.StartsWith(ancestorPage.NodeAliasPath))
//                .OrderBy(x => x.Descending(nameof(IRssItem.PublishFrom)))
//                .Take(page.RssTopN)
//            , cancellationToken))
//                .ToArray()
//            : Array.Empty<IRssItem>();

//        var rssItemAbsoluteUrlByNodeId = await _pageUrlService.GetPageAbsoluteUrlByNodeIdAsync(site.PresentationURL, rssItemPages.Select(x => x.NodeID), page.LanguageTag, cancellationToken);

//        var rssCategoryIds = rssItemPages.SelectMany(rssItemPage => rssItemPage.MetadataRssCategory).Distinct();

//        var rssCategoryById = (await _rssCategoryRetriever.RetrieveEntitiesAsync(query => query
//            .Where(x => rssCategoryIds.Contains(x.ID)
//        ), cancellationToken))
//            .ToDictionary(x => x.ID, x => x);

//        foreach (var rssItemPage in rssItemPages)
//        {
//            var url = rssItemAbsoluteUrlByNodeId[rssItemPage.NodeID];

//            var categories = rssItemPage.MetadataRssCategory
//                .Select(rssCategoryId => rssCategoryById.TryGetValue(rssCategoryId, out var rssCategory) ? rssCategory.Name : null)
//                .OfType<string>()
//                .ToList();

//            var item = new RssChannelItem
//            {
//                Title = rssItemPage.MetadataTitle,
//                Description = HtmlHelper.ExtractTextFromHtml(rssItemPage.MetadataDescription)?.Trim(),
//                PubDate = rssItemPage.PublishFrom,
//                Link = url.ToUri(),
//                Categories = categories,
//                GUID = url.ToHtmlCompatibleString(),
//            };

//            if (rssItemPage.MetadataRssImage is not null)
//            {
//                var file = await _fileService.GetFileAsync(rssItemPage.MetadataRssImage, cancellationToken);

//                if (file is not null)
//                {
//                    var imageUrl = rssItemPage.MetadataRssImage is RelativeLink relativeLink ? site.PresentationURL + relativeLink : (AbsoluteLink)rssItemPage.MetadataRssImage;

//                    item.Enclosure = new RssChannelItemEnclosure
//                    {
//                        Length = file.Size,
//                        Type = file.MimeType,
//                        Url = imageUrl.ToUri(),
//                    };
//                }
//            }

//            feed.Channel.Items.Add(item);
//        }

//        return feed;
//    }
//}
