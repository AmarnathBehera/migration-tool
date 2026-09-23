using BDO.GWT.Core.Constants;
using BDO.GWT.Core.Data.Queries;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Cards;
using BDO.GWT.Core.Utilities;
using CMS.Websites;
using Kentico.Content.Web.Mvc;
using Microsoft.AspNetCore.Http;

namespace BDO.GWT.Core.Services;

public interface IRelatedContentCardService
{
    Task<FilterItemDto<OptionDto>[]> CreateFiltersAsync(
        HashSet<string> filterParameterNames,
        string? nodeAliasPath,
        CancellationToken cancellationToken);

    Task<PaginationDto<RelatedContentCardDto>> GetFilteredItemsAsync(
        RelatedContentCardsQuery cardsQuery,
        string parentPath,
        string language,
        Action<RetrievePagesQueryParameters>? decorateQuery,
        Func<IWebPageFieldsSource, bool> isFeatured,
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

    public async Task<PaginationDto<RelatedContentCardDto>> GetFilteredItemsAsync(
        RelatedContentCardsQuery cardsQuery,
        string parentPath,
        string language,
        Action<RetrievePagesQueryParameters>? decorateQuery,
        Func<IWebPageFieldsSource, bool> isFeatured,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(parentPath))
            throw new ArgumentException("Parent path cannot be null or empty.", nameof(parentPath));

        var selectedTypes = cardsQuery.SelectedPageTypes ?? new List<string>();
        var allPages = new List<IWebPageFieldsSource>();

        async Task<IEnumerable<IWebPageFieldsSource>> RetrieveAsync<T>()
            where T : class, IWebPageFieldsSource
        {
            try
            {
                return await _contentRetriever.RetrievePages<T, T>(
                    new RetrievePagesParameters
                    {
                    LinkedItemsMaxLevel = 2,
                        PathMatch = PathMatch.Children(parentPath),
                        LanguageName = language
                    },
                    query => { ApplyCommonFilter(query); decorateQuery?.Invoke(query); },
                    RetrievalCacheSettings.CacheDisabled,
                    (_, page) => Task.FromResult(page),
                    cancellationToken);
            }
            catch (InvalidOperationException)
            {
                // A malformed content item (for example an unbindable asset field on a linked
                // item) must not prevent the remaining page types from being rendered.
                return Array.Empty<IWebPageFieldsSource>();
            }
        }

        foreach (var pageType in selectedTypes)
        {
            IEnumerable<IWebPageFieldsSource> pages = pageType switch
            {
                nameof(Insight)       => await RetrieveAsync<Insight>(),
                nameof(BlogPost)      => await RetrieveAsync<BlogPost>(),
                nameof(NewsArticle)   => await RetrieveAsync<NewsArticle>(),
                nameof(Event)         => await RetrieveAsync<Event>(),
                nameof(Webinar)       => await RetrieveAsync<Webinar>(),
                nameof(MicrositePage) => await RetrieveAsync<MicrositePage>(),
                _ => Array.Empty<IWebPageFieldsSource>()
            };

            allPages.AddRange(pages);

        }

        var filters = await GetFilterPredicatesAsync(cardsQuery.Filters);

        var filtered = allPages
            .Where(page => filters.All(f => f(page)))
            .ToArray();

        var ordered = ((IEnumerable<IWebPageFieldsSource>)filtered)
            .OrderByDescending(x => isFeatured(x))
            .ThenBy(x => GetOrderingDate(x))
            .ToArray();

        var currentPage = cardsQuery.CurrentPage ?? 1;
        var pageSize = cardsQuery.PageSize ?? 10;

        var paginated = ordered
            .Skip((currentPage - 1) * pageSize)
            .Take(pageSize)
            .ToArray();

        var itemDtos = (await Task.WhenAll(
            paginated.Select(page => GetContentPageCardDtoAsync(page, isFeatured))))
            .Where(card => card is not null)
            .ToArray();

        return new PaginationDto<RelatedContentCardDto>
        {
            Data = itemDtos!,
            TotalCount = ordered.Length
        };
    }

    // Returns a key that is always sorted with ThenByDescending: regular pages keep their
    // "most recently published first" order, while Events and Webinars use an inverted start
    // date so that the soonest upcoming one appears first.
    private static DateTime GetOrderingDate(IWebPageFieldsSource page)
    {
        if (page is Event e)
            return DateTime.MaxValue - (e.EventDateFrom - DateTime.MinValue);

        if (page is Webinar w)
            return DateTime.MaxValue - (w.EventDateFrom - DateTime.MinValue);

        return page.SystemFields.ContentItemCommonDataLastPublishedWhen
               ?? page.SystemFields.ContentItemCommonDataFirstPublishedWhen
               ?? DateTime.MinValue;
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

    private Task<RelatedContentCardDto> GetContentPageCardDtoAsync(
        IWebPageFieldsSource page,
        Func<IWebPageFieldsSource, bool> isFeatured)
    {
        static string? Teaser(IEnumerable<ContentAttachment>? img) =>
            img?.FirstOrDefault()?.ContentAttachmentAsset?.Url;

        var card = page switch
        {
            Insight i => BuildCard(i, isFeatured, new CardContent(
                i.InsightContent.FirstOrDefault()?.ImageFile.FirstOrDefault()?.ContentAttachmentAsset?.Url,
                i.InsightContent.FirstOrDefault()?.ImageFileAltText,
                i.ContentType?.FirstOrDefault()?.Name, i.MetadataTitle, i.MetadataDescription, i.ComingSoon)),

            BlogPost b     => BuildCard(b, isFeatured, new CardContent(Teaser(b.MetadataTeaserImage), b.MetadataTeaserImageAltText, b.ContentType?.FirstOrDefault()?.Name, b.MetadataTitle, b.MetadataDescription)),
            NewsArticle n  => BuildCard(n, isFeatured, new CardContent(Teaser(n.MetadataTeaserImage), n.MetadataTeaserImageAltText, n.ContentType?.FirstOrDefault()?.Name, n.MetadataTitle, n.MetadataDescription)),
            Event e        => BuildCard(e, isFeatured, new CardContent(Teaser(e.MetadataTeaserImage), e.MetadataTeaserImageAltText, e.ContentType?.FirstOrDefault()?.Name, e.MetadataTitle, e.MetadataDescription, DateOverride: e.EventDateFrom, DateToOverride: e.EventDateTo)),
            Webinar w      => BuildCard(w, isFeatured, new CardContent(Teaser(w.MetadataTeaserImage), w.MetadataTeaserImageAltText, w.ContentType?.FirstOrDefault()?.Name, w.MetadataTitle, w.MetadataDescription, DateOverride: w.EventDateFrom, DateToOverride: w.EventDateTo)),
            MicrositePage m => BuildCard(m, isFeatured, new CardContent(Teaser(m.MetadataTeaserImage), m.MetadataTeaserImageAltText, m.MicrositeContentType?.FirstOrDefault()?.Name, m.MetadataTitle, m.MetadataDescription)),

            _ => null
        };

        return Task.FromResult(card!);
    }

    private RelatedContentCardDto BuildCard(
        IWebPageFieldsSource page,
        Func<IWebPageFieldsSource, bool> isFeatured,
        CardContent content) =>
        new RelatedContentCardDto
        {
            IsFeatured = isFeatured(page),
            ImageURL = GetAbsoluteAssetUrl(content.ImageUrl),
            isComingSoon = content.IsComingSoon,
            ImageAltText = content.ImageAltText,
            Type = content.Type,
            Title = content.Title ?? string.Empty,
            Date = content.DateOverride
                   ?? page.SystemFields.ContentItemCommonDataLastPublishedWhen
                   ?? page.SystemFields.ContentItemCommonDataFirstPublishedWhen,
            DateTo = content.DateToOverride,
            UpdatedOnDate = null,

            Description = HtmlHelper.ExtractTextFromHtml(content.Description),
            ReadMoreLink = new LinkItemDto
            {
                Title = ResourceStringKey.LiveSite.ReadMore,
                URL = _webPageUrlResolver.GetAbsoluteUrl(page)
            }
        };

    private readonly record struct CardContent(
        string? ImageUrl,
        string? ImageAltText,
        string? Type,
        string? Title,
        string? Description,
        bool IsComingSoon = false,
        DateTime? DateOverride = null,
        DateTime? DateToOverride = null);
}