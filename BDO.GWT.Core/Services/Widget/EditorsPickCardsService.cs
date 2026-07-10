using BDO.GWT.Core;
using BDO.GWT.Core.Constants;
using BDO.GWT.Core.Data.Queries;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Cards;
using BDO.GWT.Core.Helpers;
using BDO.GWT.Core.Utilities;
using CMS.ContentEngine;
using CMS.Websites;
using CMS.Websites.Routing;
using Kentico.Content.Web.Mvc;
using System.Reflection;

namespace BDO.GWT.Core.Services;

public interface IEditorsPickCardService
{
    Task<FilterItemDto<OptionDto>[]> CreateFiltersAsync(
        HashSet<string> filters,
        string? treePath,
        CancellationToken cancellationToken);

    Task<PaginationDto<EditorsPickCardDto>> GetFilteredItemsAsync(
        CardsQuery cardsQuery,
        HashSet<Guid> selectedPageGuids,
        Func<IWebPageFieldsSource, bool> isFeatured,
        CancellationToken cancellationToken);
}

public class EditorsPickCardService : IEditorsPickCardService
{
    private readonly ICardsService _cardsService;
    private readonly IContentService _contentService;
    private readonly IWebPageUrlRetriever _webPageUrlRetriever;
    private readonly IContentRetriever _contentRetriever;
    private static readonly string[] EditorPickContentTypes =
[
    BlogPost.CONTENT_TYPE_NAME,
    Event.CONTENT_TYPE_NAME,
    MicrositePage.CONTENT_TYPE_NAME,
    NewsArticle.CONTENT_TYPE_NAME,
    Webinar.CONTENT_TYPE_NAME,
];

    private readonly Func<IEnumerable<string>, IEnumerable<Guid>, RetrieveContentOfContentTypesParameters, CancellationToken, Task<IEnumerable<IWebPageFieldsSource>>> _retrieveContentByGuidsAsync;

    public EditorsPickCardService(
        ICardsService cardsService,
        IContentService contentService,
        IWebPageUrlRetriever webPageUrlRetriever,
        IContentRetriever contentRetriever)
    {
        _cardsService = cardsService;
        _contentService = contentService;
        _webPageUrlRetriever = webPageUrlRetriever;
        _contentRetriever = contentRetriever;
        _retrieveContentByGuidsAsync = (contentTypes, guids, parameters, ct) =>
            _contentRetriever.RetrieveContentOfContentTypesByGuids<IWebPageFieldsSource>(
                contentTypes, guids.ToList(), parameters, ct);
    }

    public async Task<FilterItemDto<OptionDto>[]> CreateFiltersAsync(
        HashSet<string> filters,
        string? treePath,
        CancellationToken cancellationToken)
    {
        return await _cardsService.CreateFiltersAsync(filters, treePath, cancellationToken);
    }

    public async Task<PaginationDto<EditorsPickCardDto>> GetFilteredItemsAsync(
        CardsQuery cardsQuery,
        HashSet<Guid> selectedPageGuids,
        Func<IWebPageFieldsSource, bool> isFeatured,
        CancellationToken cancellationToken)
    {
        cardsQuery ??= new CardsQuery();
        cardsQuery.CurrentPage ??= 1;
        cardsQuery.PageSize ??= 10;

        if (selectedPageGuids.Count == 0)
        {
            return new PaginationDto<EditorsPickCardDto>
            {
                Data = Array.Empty<EditorsPickCardDto>(),
                TotalCount = 0
            };
        }



        var pages = await _retrieveContentByGuidsAsync(
                EditorPickContentTypes,
                selectedPageGuids,
                new RetrieveContentOfContentTypesParameters() { LinkedItemsMaxLevel = 2 },
                cancellationToken);

        var filtered = ApplyFilters(pages, cardsQuery.Filters)
            .OrderByDescending(isFeatured)
            .ThenByDescending(GetPublishDateUtc)
            .ToArray();

        var paged = filtered
            .Skip((cardsQuery.CurrentPage.Value - 1) * cardsQuery.PageSize.Value)
            .Take(cardsQuery.PageSize.Value)
            .ToArray();

        var itemDtos = await MapCardsAsync(paged, isFeatured, cancellationToken);

        return new PaginationDto<EditorsPickCardDto>
        {
            Data = itemDtos,
            TotalCount = filtered.Length
        };
    }

    private static IEnumerable<IWebPageFieldsSource> ApplyFilters(
        IEnumerable<IWebPageFieldsSource> pages,
        IDictionary<string, string> filters)
    {
        if (filters.Count == 0)
        {
            return pages;
        }

        IEnumerable<IWebPageFieldsSource> filtered = pages;

        foreach (var (filterName, filterValue) in filters)
        {
            if (string.IsNullOrWhiteSpace(filterValue))
            {
                continue;
            }

            filtered = filterName switch
            {
                FilterParameterName.ContentType => filtered.Where(page =>
                    string.Equals(GetPageTypeName(page), filterValue, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(GetSingleOrLinkedName(page, "ContentType"), filterValue, StringComparison.OrdinalIgnoreCase)),

                FilterParameterName.BusinessLine => filtered.Where(page =>
                    HasLinkedValue(page, filterValue, "MetadataBusinessLines", "MetadataBusinessLine")),

                FilterParameterName.ServiceArea => filtered.Where(page =>
                    HasLinkedValue(page, filterValue, "MetadataServiceAreas")),

                FilterParameterName.IndustryCategory => filtered.Where(page =>
                    HasLinkedValue(page, filterValue, "MetadataIndustryCategories")),

                _ => filtered
            };
        }

        return filtered;
    }

    private async Task<EditorsPickCardDto[]> MapCardsAsync(
        IEnumerable<IWebPageFieldsSource> pages,
        Func<IWebPageFieldsSource, bool> isFeatured,
        CancellationToken cancellationToken)
    {
        var language = _contentService.GetCurrentLanguage();
        var cards = new List<EditorsPickCardDto>();

        foreach (var page in pages)
        {
            var url = await TryGetUrlAsync(page, language, cancellationToken);

            cards.Add(new EditorsPickCardDto
            {
                IsFeatured = isFeatured(page),
                Type = GetSingleOrLinkedName(page, "ContentType"),
                ContentType = GetPageTypeName(page),
                Title = GetStringProperty(page, "MetadataTitle")
                    ?? GetStringProperty(page, "MetadataOGTitle")
                    ?? page.SystemFields.ContentItemName
                    ?? string.Empty,
                Description = NormalizeDescription(GetStringProperty(page, "MetadataDescription")),
                ImageURL = GetImageUrl(page),
                ImageAltText = GetStringProperty(page, "MetadataTeaserImageAltText")
                    ?? GetStringProperty(page, "ImageFileAltText"),
                Date = GetPublishDateUtc(page),
                UpdatedOnDate = GetDateTimeProperty(page, "UpdatedOn"),
                ReadMoreLink = url is null
                    ? null
                    : new LinkItemDto
                    {
                        Title = "Read more",
                        URL = url
                    }
            });
        }

        return cards.ToArray();
    }

    private async Task<string?> TryGetUrlAsync(
        IWebPageFieldsSource page,
        string language,
        CancellationToken cancellationToken)
    {
        try
        {
            var pageUrl = await _webPageUrlRetriever.Retrieve(page, language, cancellationToken);
            return pageUrl.AbsoluteUrl;
        }
        catch
        {
            return null;
        }
    }

    private static bool HasLinkedValue(IWebPageFieldsSource page, string value, params string[] propertyNames)
    {
        foreach (var propertyName in propertyNames)
        {
            var names = GetLinkedNames(page, propertyName);
            if (names.Any(x => string.Equals(x, value, StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }
        }

        return false;
    }

    private static IEnumerable<string> GetLinkedNames(object source, string propertyName)
    {
        var property = source.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
        if (property?.GetValue(source) is not System.Collections.IEnumerable items)
        {
            return Enumerable.Empty<string>();
        }

        var values = new List<string>();

        foreach (var item in items)
        {
            if (item is null)
            {
                continue;
            }

            var name = item.GetType().GetProperty("Name")?.GetValue(item)?.ToString()
                       ?? item.GetType().GetProperty("CodeName")?.GetValue(item)?.ToString()
                       ?? item.GetType().GetProperty("MetadataTitle")?.GetValue(item)?.ToString()
                       ?? item.GetType().GetProperty("MetadataOGTitle")?.GetValue(item)?.ToString();

            if (!string.IsNullOrWhiteSpace(name))
            {
                values.Add(name);
            }
        }

        return values;
    }

    private static string? GetSingleOrLinkedName(object source, string propertyName)
    {
        var linked = GetLinkedNames(source, propertyName).FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(linked))
        {
            return linked;
        }

        var property = source.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
        if (property?.PropertyType == typeof(string))
        {
            return property.GetValue(source) as string;
        }

        return null;
    }

    private static string? GetImageUrl(object source)
    {
        foreach (var propertyName in new[] { "MetadataTeaserImage", "ImageFile" })
        {
            var property = source.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            if (property?.GetValue(source) is not System.Collections.IEnumerable items)
            {
                continue;
            }

            foreach (var item in items)
            {
                var url = item?.GetType()
                    .GetProperty("ContentAttachmentAsset")
                    ?.GetValue(item)?
                    .GetType()
                    .GetProperty("Url")
                    ?.GetValue(item.GetType().GetProperty("ContentAttachmentAsset")?.GetValue(item))?
                    .ToString();

                if (!string.IsNullOrWhiteSpace(url))
                {
                    return url;
                }
            }
        }

        return null;
    }

    private static string NormalizeDescription(string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return string.Empty;
        }

        return HtmlHelper.ExtractTextFromHtml(description) ?? string.Empty;
    }

    private static string GetPageTypeName(IWebPageFieldsSource page) => page.GetType().Name;

    private static string? GetStringProperty(object source, string propertyName) =>
        source.GetType()
            .GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance)?
            .GetValue(source)?
            .ToString();

    private static DateTime? GetDateTimeProperty(object source, string propertyName)
    {
        var value = source.GetType()
            .GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance)?
            .GetValue(source);

        if (value is DateTime date)
        {
            return date == DateTime.MinValue ? null : date;
        }

        return null;
    }

    private static DateTime? GetPublishDateUtc(IWebPageFieldsSource page)
    {
        var directDate = GetDateTimeProperty(page, "DocumentPublishFrom");
        if (directDate is not null)
        {
            return directDate;
        }

        var systemFields = page.SystemFields;
        var type = systemFields.GetType();

        var lastPublished = type.GetProperty("ContentItemCommonDataLastPublishedWhen")?.GetValue(systemFields) as DateTime?;
        var firstPublished = type.GetProperty("ContentItemCommonDataFirstPublishedWhen")?.GetValue(systemFields) as DateTime?;

        return lastPublished ?? firstPublished;
    }
}