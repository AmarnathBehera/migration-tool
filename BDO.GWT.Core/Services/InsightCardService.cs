using BDO.GWT.Core.Constants;
using BDO.GWT.Core.Data.Queries;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Cards;
using BDO.GWT.Core.Utilities;
using CMS.ContentEngine;
using CMS.Websites;
using Kentico.Content.Web.Mvc;
using Microsoft.AspNetCore.Http;

namespace BDO.GWT.Core.Services;
public interface IInsightCardService
{
    Task<FilterItemDto<OptionDto>[]> CreateFiltersAsync(HashSet<string> filterParameterNames, string? nodeAliasPath, CancellationToken cancellationToken);
    Task<PaginationDto<ContentPageCardDto>> GetFilteredItemsAsync (CardsQuery cardsQuery, string parentPath, string language,
        Action<RetrievePagesQueryParameters>? decorateQuery,
        Func<Insight, bool> isFeatured,
        CancellationToken cancellationToken);

}
public class InsightCardService : IInsightCardService
{
    private const char UrlPathSeparator = '/';

    private readonly IContentRetriever _contentRetriever;
    private readonly ICardsService _cardsService;
    private readonly IWebPageUrlResolver _webPageUrlResolver;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public InsightCardService(
        IContentRetriever contentRetriever,
        ICardsService cardsService,
        IWebPageUrlResolver webPageUrlResolver, IHttpContextAccessor httpContextAccessor)
    {
        _contentRetriever = contentRetriever;
        _cardsService = cardsService;
        _webPageUrlResolver = webPageUrlResolver;
        _httpContextAccessor= httpContextAccessor;
    }

   
 
    public async Task<FilterItemDto<OptionDto>[]> CreateFiltersAsync(HashSet<string> filterParameterNames, string? nodeAliasPath, CancellationToken cancellationToken)
    {
        return await _cardsService.CreateFiltersAsync(filterParameterNames, nodeAliasPath, cancellationToken);
    }

    public async Task<PaginationDto<ContentPageCardDto>> GetFilteredItemsAsync(
    CardsQuery cardsQuery,
    string parentPath,
    string language, Action<RetrievePagesQueryParameters>? decorateQuery,
    Func<Insight, bool> isFeatured,
    CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(parentPath))
        {
            throw new ArgumentException("Parent path cannot be null or empty.", nameof(parentPath));
        }

        var filters = await GetFilterPredicatesAsync(cardsQuery.Filters);

        var contentPages = await _contentRetriever.RetrievePages<Insight, Insight>(
            new RetrievePagesParameters
            {
                LinkedItemsMaxLevel = 4,

                PathMatch = PathMatch.Children(parentPath)

            },
            query =>
            {
                ApplyCommonFilter(query);
                decorateQuery?.Invoke(query);
            },
            RetrievalCacheSettings.CacheDisabled,
            (_, page) => Task.FromResult(page),
            cancellationToken);

        var filteredContentPages = contentPages
            .Where(page => filters.All(filter => filter(page)))
            .ToArray();

        var orderedContentPages = filteredContentPages
            .OrderByDescending(isFeatured)
            .ThenByDescending(x =>
                x.SystemFields.ContentItemCommonDataLastPublishedWhen
                ?? x.SystemFields.ContentItemCommonDataFirstPublishedWhen)
            .ToArray();

        var currentPage = cardsQuery.CurrentPage ?? 1;
        var pageSize = cardsQuery.PageSize ?? 10;

        var paginatedContentPages = orderedContentPages
            .Skip((currentPage - 1) * pageSize)
            .Take(pageSize)
            .ToArray();

        var itemDtos = await Task.WhenAll(
            paginatedContentPages.Select(page =>
                GetContentPageCardDtoAsync(page, isFeatured)));

        return new PaginationDto<ContentPageCardDto>
        {
            Data = itemDtos,
            TotalCount = orderedContentPages.Length
        };
    }

    private static void ApplyCommonFilter(RetrievePagesQueryParameters pageQuery)
    {
        pageQuery.Where(where =>
            where.WhereFalse(nameof(Insight.IsSearchExcluded))
                 .Or()
                 .WhereNull(nameof(Insight.IsSearchExcluded)));
    }

    private static Task<List<Func<Insight, bool>>> GetFilterPredicatesAsync(IDictionary<string, string> filterParameterValueByName)
    {
        var predicates = new List<Func<Insight, bool>>();

        foreach (var (filterParameterName, filterParameterValue) in filterParameterValueByName)
        {
            if (string.IsNullOrWhiteSpace(filterParameterValue))
            {
                continue;
            }

            Func<Insight, bool>? predicate = null;

            switch (filterParameterName)
            {
                case FilterParameterName.BusinessLine:
                    predicate = x => MatchesLinkedItemName(x.MetadataBusinessLine, filterParameterValue);
                    break;
                case FilterParameterName.ServiceArea:
                    predicate = x => MatchesLinkedItemName(x.MetadataServiceAreas, filterParameterValue);
                    break;
                case FilterParameterName.IndustryCategory:
                    predicate = x => MatchesLinkedItemName(x.MetadataIndustryCategories, filterParameterValue);
                    break;
                case FilterParameterName.SpecialtyCategory:
                    predicate = x => MatchesLinkedItemName(x.MetadataSpecialtiesCategories, filterParameterValue);
                    break;
                case FilterParameterName.MicrositeMasterPage:
                    predicate = x => MatchesLinkedItemName(x.MicrositeMasterPages, filterParameterValue);
                    break;
                case FilterParameterName.OfficeLocation:
                    predicate = x => MatchesLinkedItemName(x.MetadataOfficeLocation, filterParameterValue);
                    break;
                case FilterParameterName.BusinessIssue:
                    predicate = x => MatchesLinkedItemName(x.MetadataBusinessIssues, filterParameterValue);
                    break;
                case FilterParameterName.ContentType:
                    predicate = x => MatchesLinkedItemName(x.ContentType, filterParameterValue);
                    break;
            }

            if (predicate is not null)
            {
                predicates.Add(predicate);
            }
        }

        return Task.FromResult(predicates);
    }

    private static bool MatchesLinkedItemName<TItem>(IEnumerable<TItem>? items, string expectedValue)
        where TItem : class
    {
        if (items is null)
        {
            return false;
        }

        foreach (var item in items)
        {
            var candidate =
                item.GetType().GetProperty("Name")?.GetValue(item) as string
                ?? item.GetType().GetProperty("MetadataTitle")?.GetValue(item) as string
                ?? item.GetType().GetProperty("SystemFields")?.GetValue(item)?.GetType().GetProperty("ContentItemName")?.GetValue(item.GetType().GetProperty("SystemFields")?.GetValue(item)!) as string;

            if (!string.IsNullOrWhiteSpace(candidate) && string.Equals(candidate, expectedValue, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
    private string? GetAbsoluteAssetUrl(string? url)
{
    if (string.IsNullOrWhiteSpace(url))
    {
        return null;
    }

    // Already absolute URL
    if (Uri.TryCreate(url, UriKind.Absolute, out _))
    {
        return url;
    }

    var request = _httpContextAccessor.HttpContext?.Request;

    if (request is null)
    {
        return url.Replace("~/", "/");
    }

    var relativeUrl = GetRelativeUrl(url);

    return $"{request.Scheme}://{request.Host}{relativeUrl}";
}

    private static string GetRelativeUrl(string url)
{
    if (url.StartsWith("~/"))
    {
        // converts ~/getContentAsset/... to /getContentAsset/...
        return url[1..];
    }

    if (url.StartsWith(UrlPathSeparator))
    {
        return url;
    }

    return UrlPathSeparator + url;
}
    private Task<ContentPageCardDto> GetContentPageCardDtoAsync(Insight page, Func<Insight, bool> isFeatured)
    {
        var imageUrl = page.CopyInsight.FirstOrDefault()?.ImageFile
        .FirstOrDefault()?
        .ContentAttachmentAsset?
        .Url;
        var card = new ContentPageCardDto
        {
            IsFeatured = isFeatured(page),
            ImageURL = GetAbsoluteAssetUrl(imageUrl),
            isComingSoon = page.ComingSoon,
            ImageAltText = page.CopyInsight.FirstOrDefault()?.ImageFileAltText,
       
            Type = page.ContentType?.FirstOrDefault()?.Name,
            Title = page.MetadataTitle,
            Date = page.SystemFields.ContentItemCommonDataLastPublishedWhen ?? page.SystemFields.ContentItemCommonDataFirstPublishedWhen,
            UpdatedOnDate = null,
            Description = HtmlHelper.ExtractTextFromHtml(page.MetadataDescription),
            ReadMoreLink = new LinkItemDto
            {
                Title = ResourceStringKey.LiveSite.ReadMore,
                URL = _webPageUrlResolver.GetAbsoluteUrl(page)
            }
        };

        return Task.FromResult(card);
    }
}