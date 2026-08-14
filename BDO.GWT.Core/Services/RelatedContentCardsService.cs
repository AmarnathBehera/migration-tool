using BDO.GWT.Core.Constants;
using BDO.GWT.Core.Data.Queries;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Cards;
using BDO.GWT.Core.Utilities;
using CMS.Websites;
using Kentico.Content.Web.Mvc;
using Microsoft.AspNetCore.Http;
using ReverseMarkdown.Converters;

namespace BDO.GWT.Core.Services;

public interface IRelatedContentCardService
{
    Task<FilterItemDto<OptionDto>[]> CreateFiltersAsync(
        HashSet<string> filterParameterNames,
        string? nodeAliasPath,
        CancellationToken cancellationToken);

    Task<PaginationDto<ContentPageCardDto>> GetFilteredItemsAsync(
        RelatedContentCardsQuery cardsQuery,
        string parentPath,
        string language,
        Action<RetrievePagesQueryParameters>? decorateQuery,
        Func<IWebPageFieldsSource, bool> isFeatured,  // fixed
        CancellationToken cancellationToken);
}

public class RelatedContentCardService : IRelatedContentCardService
{
    private const char UrlPathSeparator = '/';

    private readonly IContentRetriever _contentRetriever;
    private readonly ICardsService _cardsService;
    private readonly IWebPageUrlResolver _webPageUrlResolver;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public RelatedContentCardService(
        IContentRetriever contentRetriever,
        ICardsService cardsService,
        IWebPageUrlResolver webPageUrlResolver,
        IHttpContextAccessor httpContextAccessor)
    {
        _contentRetriever = contentRetriever;
        _cardsService = cardsService;
        _webPageUrlResolver = webPageUrlResolver;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<FilterItemDto<OptionDto>[]> CreateFiltersAsync(
        HashSet<string> filterParameterNames,
        string? nodeAliasPath,
        CancellationToken cancellationToken)
    {
        return await _cardsService.CreateFiltersAsync(filterParameterNames, nodeAliasPath, cancellationToken);
    }

    public async Task<PaginationDto<ContentPageCardDto>> GetFilteredItemsAsync(
        RelatedContentCardsQuery cardsQuery,
        string parentPath,
        string language,
        Action<RetrievePagesQueryParameters>? decorateQuery,
        Func<IWebPageFieldsSource, bool> isFeatured,  // fixed
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(parentPath))
            throw new ArgumentException("Parent path cannot be null or empty.", nameof(parentPath));

        var selectedTypes = cardsQuery.SelectedPageTypes ?? new List<string>();
        var allPages = new List<IWebPageFieldsSource>();

        foreach (var pageType in selectedTypes)
        {
            IEnumerable<IWebPageFieldsSource> pages = pageType switch
            {
                nameof(Insight) => await _contentRetriever.RetrievePages<Insight, Insight>(
                    new RetrievePagesParameters { LinkedItemsMaxLevel = 2, PathMatch = PathMatch.Children(parentPath) },
                    query => { ApplyCommonFilter(query); decorateQuery?.Invoke(query); },
                    RetrievalCacheSettings.CacheDisabled,
                    (_, page) => Task.FromResult(page),
                    cancellationToken),

                nameof(BlogPost) => await _contentRetriever.RetrievePages<BlogPost, BlogPost>(
                    new RetrievePagesParameters { LinkedItemsMaxLevel = 2, PathMatch = PathMatch.Children(parentPath) },
                    query => { ApplyCommonFilter(query); decorateQuery?.Invoke(query); },
                    RetrievalCacheSettings.CacheDisabled,
                    (_, page) => Task.FromResult(page),
                    cancellationToken),

                nameof(NewsArticle) => await _contentRetriever.RetrievePages<NewsArticle, NewsArticle>(
                    new RetrievePagesParameters { LinkedItemsMaxLevel = 2, PathMatch = PathMatch.Children(parentPath) },
                    query => { ApplyCommonFilter(query); decorateQuery?.Invoke(query); },
                    RetrievalCacheSettings.CacheDisabled,
                    (_, page) => Task.FromResult(page),
                    cancellationToken),

                nameof(Event) => await _contentRetriever.RetrievePages<Event, Event>(
                    new RetrievePagesParameters { LinkedItemsMaxLevel = 2, PathMatch = PathMatch.Children(parentPath) },
                    query => { ApplyCommonFilter(query); decorateQuery?.Invoke(query); },
                    RetrievalCacheSettings.CacheDisabled,
                    (_, page) => Task.FromResult(page),
                    cancellationToken),

                nameof(Webinar) => await _contentRetriever.RetrievePages<Webinar, Webinar>(
                    new RetrievePagesParameters { LinkedItemsMaxLevel = 2, PathMatch = PathMatch.Children(parentPath) },
                    query => { ApplyCommonFilter(query); decorateQuery?.Invoke(query); },
                    RetrievalCacheSettings.CacheDisabled,
                    (_, page) => Task.FromResult(page),
                    cancellationToken),

                nameof(MicrositePage) => await _contentRetriever.RetrievePages<MicrositePage, MicrositePage>(
                    new RetrievePagesParameters { LinkedItemsMaxLevel = 2, PathMatch = PathMatch.Children(parentPath) },
                    query => { ApplyCommonFilter(query); decorateQuery?.Invoke(query); },
                    RetrievalCacheSettings.CacheDisabled,
                    (_, page) => Task.FromResult(page),
                    cancellationToken),

                _ => Array.Empty<IWebPageFieldsSource>()
            };

            allPages.AddRange(pages);

        }

        var filters = await GetFilterPredicatesAsync(cardsQuery.Filters);

        var filtered = allPages
            .Where(page => filters.All(f => f(page)))
            .ToArray();

        // explicit cast fixes the async LINQ overload conflict
        var ordered = ((IEnumerable<IWebPageFieldsSource>)filtered)
            .OrderByDescending(x => isFeatured(x))
            .ThenByDescending(x =>
                x.SystemFields.ContentItemCommonDataLastPublishedWhen
                ?? x.SystemFields.ContentItemCommonDataFirstPublishedWhen)
            .ToArray();

        var currentPage = cardsQuery.CurrentPage ?? 1;
        var pageSize = cardsQuery.PageSize ?? 10;

        var paginated = ordered
            .Skip((currentPage - 1) * pageSize)
            .Take(pageSize)
            .ToArray();

        var itemDtos = await Task.WhenAll(
            paginated.Select(page => GetContentPageCardDtoAsync(page, isFeatured)));

        return new PaginationDto<ContentPageCardDto>
        {
            Data = itemDtos,
            TotalCount = ordered.Length
        };
    }

    private static void ApplyCommonFilter(RetrievePagesQueryParameters pageQuery)
    {
        pageQuery.Where(where =>
            where.WhereFalse(nameof(Insight.IsSearchExcluded))
                 .Or()
                 .WhereNull(nameof(Insight.IsSearchExcluded)));
    }

    private static Task<List<Func<IWebPageFieldsSource, bool>>> GetFilterPredicatesAsync(
        IDictionary<string, string> filterParameterValueByName)
    {
        var predicates = new List<Func<IWebPageFieldsSource, bool>>();

        foreach (var (filterParameterName, filterParameterValue) in filterParameterValueByName)
        {
            if (string.IsNullOrWhiteSpace(filterParameterValue))
                continue;

            Func<IWebPageFieldsSource, bool>? predicate = filterParameterName switch
            {
                FilterParameterName.BusinessLine => page => page switch
                {
                    Insight i => MatchesLinkedItemName(i.MetadataBusinessLine, filterParameterValue),
                    BlogPost b => MatchesLinkedItemName(b.MetadataBusinessLine, filterParameterValue),
                    NewsArticle n => MatchesLinkedItemName(n.MetadataBusinessLine, filterParameterValue),
                    Event e => MatchesLinkedItemName(e.MetadataBusinessLine, filterParameterValue),
                    Webinar w => MatchesLinkedItemName(w.MetadataBusinessLine, filterParameterValue),
                    MicrositePage m => MatchesLinkedItemName(m.BusinessLines, filterParameterValue),
                    _ => false
                },

                FilterParameterName.ServiceArea => page => page switch
                {
                    Insight i => MatchesLinkedItemName(i.MetadataServiceAreas, filterParameterValue),
                    BlogPost b => MatchesLinkedItemName(b.MetadataServiceAreas, filterParameterValue),
                    NewsArticle n => MatchesLinkedItemName(n.MetadataServiceAreas, filterParameterValue),
                    Event e => MatchesLinkedItemName(e.MetadataServiceAreas, filterParameterValue),
                    Webinar w => MatchesLinkedItemName(w.MetadataServiceAreas, filterParameterValue),
                    MicrositePage m => MatchesLinkedItemName(m.ServiceAreas, filterParameterValue),
                    _ => false
                },

                FilterParameterName.IndustryCategory => page => page switch
                {
                    Insight i => MatchesLinkedItemName(i.MetadataIndustryCategories, filterParameterValue),
                    BlogPost b => MatchesLinkedItemName(b.MetadataIndustryCategories, filterParameterValue),
                    NewsArticle n => MatchesLinkedItemName(n.MetadataIndustryCategories, filterParameterValue),
                    Event e => MatchesLinkedItemName(e.MetadataIndustryCategories, filterParameterValue),
                    Webinar w => MatchesLinkedItemName(w.MetadataIndustryCategories, filterParameterValue),
                    MicrositePage m => MatchesLinkedItemName(m.IndustryCategories, filterParameterValue),
                    _ => false
                },

                FilterParameterName.ContentType =>  page => page switch
                {
                    Insight i => MatchesLinkedItemName(i.ContentType, filterParameterValue),
                    BlogPost b => MatchesLinkedItemName(b.ContentType, filterParameterValue),
                    NewsArticle n => MatchesLinkedItemName(n.ContentType, filterParameterValue),
                    Event e => MatchesLinkedItemName(e.ContentType, filterParameterValue),
                    Webinar w => MatchesLinkedItemName(w.ContentType, filterParameterValue),
                    MicrositePage m => MatchesLinkedItemName(m.MicrositeContentType, filterParameterValue),
                    _ => false
                },



                _ => null
            };

            if (predicate is not null)
                predicates.Add(predicate);
        }

        return Task.FromResult(predicates);
    }

    private static bool MatchesLinkedItemName<TItem>(IEnumerable<TItem>? items, string expectedValue)
        where TItem : class
    {
        if (items is null) return false;

        foreach (var item in items)
        {
            var candidate =
                item.GetType().GetProperty("Name")?.GetValue(item) as string
                ?? item.GetType().GetProperty("MetadataTitle")?.GetValue(item) as string
                ?? item.GetType().GetProperty("SystemFields")?.GetValue(item)
                    ?.GetType().GetProperty("ContentItemName")
                    ?.GetValue(item.GetType().GetProperty("SystemFields")?.GetValue(item)!) as string;

            if (!string.IsNullOrWhiteSpace(candidate) &&
                string.Equals(candidate, expectedValue, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    private string? GetAbsoluteAssetUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return null;

        if (Uri.TryCreate(url, UriKind.Absolute, out _)) return url;

        var request = _httpContextAccessor.HttpContext?.Request;

        if (request is null) return url.Replace("~/", "/");

        return $"{request.Scheme}://{request.Host}{GetRelativeUrl(url)}";
    }

    private static string GetRelativeUrl(string url)
    {
        if (url.StartsWith("~/")) return url[1..];
        if (url.StartsWith(UrlPathSeparator)) return url;
        return UrlPathSeparator + url;
    }

    // fixed: IWebPageFieldsSource + type switch for card mapping
    private Task<ContentPageCardDto> GetContentPageCardDtoAsync(
        IWebPageFieldsSource page,
        Func<IWebPageFieldsSource, bool> isFeatured)
    {
        var card = page switch
        {
            Insight i => BuildInsightCard(i, isFeatured),
            BlogPost b => BuildBlogPostCard(b, isFeatured),
            NewsArticle n => BuildNewsArticleCard(n, isFeatured),
            Event e => BuildEventCard(e, isFeatured),
            Webinar w => BuildWebinarCard(w, isFeatured),
            MicrositePage m => BuildMicrositePageCard(m, isFeatured),
            _ => BuildFallbackCard(page, isFeatured)
        };

        return Task.FromResult(card);
    }

    private ContentPageCardDto BuildInsightCard(Insight page, Func<IWebPageFieldsSource, bool> isFeatured)
    {
        var imageUrl = page.InsightContent.FirstOrDefault()?.ImageFile
            .FirstOrDefault()?.ContentAttachmentAsset?.Url;

        return new ContentPageCardDto
        {
            IsFeatured = isFeatured(page),
            ImageURL = GetAbsoluteAssetUrl(imageUrl),
            isComingSoon = page.ComingSoon,
            ImageAltText = page.InsightContent.FirstOrDefault()?.ImageFileAltText,
            Type = page.ContentType?.FirstOrDefault()?.Name,
            Title = page.MetadataTitle,
            Date = page.SystemFields.ContentItemCommonDataLastPublishedWhen
                   ?? page.SystemFields.ContentItemCommonDataFirstPublishedWhen,
            UpdatedOnDate = null,
            Description = HtmlHelper.ExtractTextFromHtml(page.MetadataDescription),
            ReadMoreLink = new LinkItemDto
            {
                Title = ResourceStringKey.LiveSite.ReadMore,
                URL = _webPageUrlResolver.GetAbsoluteUrl(page)
            }
        };
    }

    private ContentPageCardDto BuildBlogPostCard(BlogPost page, Func<IWebPageFieldsSource, bool> isFeatured)
    {
        var imageUrl = page.MetadataTeaserImage.FirstOrDefault()?.ContentAttachmentAsset?.Url;

        return new ContentPageCardDto
        {
            IsFeatured = isFeatured(page),
            ImageURL = GetAbsoluteAssetUrl(imageUrl),
            isComingSoon = false,
            ImageAltText = page.MetadataTeaserImageAltText,
            Type = page.ContentType?.FirstOrDefault()?.Name,
            Title = page.MetadataTitle,
            Date = page.SystemFields.ContentItemCommonDataLastPublishedWhen
                   ?? page.SystemFields.ContentItemCommonDataFirstPublishedWhen,
            UpdatedOnDate = null,
            Description = HtmlHelper.ExtractTextFromHtml(page.MetadataDescription),
            ReadMoreLink = new LinkItemDto
            {
                Title = ResourceStringKey.LiveSite.ReadMore,
                URL = _webPageUrlResolver.GetAbsoluteUrl(page)
            }
        };
    }

    // follow the same pattern for NewsArticle, Event, Webinar, MicrositePage
    // adjust image/alt text fields to match each type's actual properties
    private ContentPageCardDto BuildNewsArticleCard(NewsArticle page, Func<IWebPageFieldsSource, bool> isFeatured) =>
        BuildGenericCard(page, isFeatured, page.MetadataTitle, page.MetadataDescription, page.ContentType?.FirstOrDefault()?.Name);

    private ContentPageCardDto BuildEventCard(Event page, Func<IWebPageFieldsSource, bool> isFeatured) =>
        BuildGenericCard(page, isFeatured, page.MetadataTitle, page.MetadataDescription, page.ContentType?.FirstOrDefault()?.Name);

    private ContentPageCardDto BuildWebinarCard(Webinar page, Func<IWebPageFieldsSource, bool> isFeatured) =>
        BuildGenericCard(page, isFeatured, page.MetadataTitle, page.MetadataDescription, page.ContentType?.FirstOrDefault()?.Name);

    private ContentPageCardDto BuildMicrositePageCard(MicrositePage page, Func<IWebPageFieldsSource, bool> isFeatured) =>
        BuildGenericCard(page, isFeatured, page.MetadataTitle, page.MetadataDescription, null);

    private ContentPageCardDto BuildFallbackCard(IWebPageFieldsSource page, Func<IWebPageFieldsSource, bool> isFeatured) =>
        BuildGenericCard(page, isFeatured, null, null, null);

    private ContentPageCardDto BuildGenericCard(
        IWebPageFieldsSource page,
        Func<IWebPageFieldsSource, bool> isFeatured,
        string? title,
        string? description,
        string? type)
    {
        return new ContentPageCardDto
        {
            IsFeatured = isFeatured(page),
            ImageURL = null,
            isComingSoon = false,
            ImageAltText = null,
            Type = type,
            Title = title ?? string.Empty,
            Date = page.SystemFields.ContentItemCommonDataLastPublishedWhen
                   ?? page.SystemFields.ContentItemCommonDataFirstPublishedWhen,
            UpdatedOnDate = null,
            Description = HtmlHelper.ExtractTextFromHtml(description),
            ReadMoreLink = new LinkItemDto
            {
                Title = ResourceStringKey.LiveSite.ReadMore,
                URL = _webPageUrlResolver.GetAbsoluteUrl(page)
            }
        };
    }
}