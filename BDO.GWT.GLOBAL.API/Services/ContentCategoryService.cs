using BDO.GWT.Common.Core.Exceptions;
using BDO.GWT.Core;
using BDO.GWT.Core.Utilities;
using BDO.GWT.Core.ValueObjects;
using BDO.GWT.GLOBAL.API.DTOs;
using BDO.GWT.GLOBAL.API.Helpers;
using CMS.ContentEngine;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Websites;
using System.Globalization;

namespace BDO.GWT.GLOBAL.API.Services;

public interface IContentCategoryService
{
    Task<IEnumerable<ContentCategoryDto>> GetContentCategoriesAsync(int channelId, string culture, CancellationToken cancellationToken);

    Task<ContentDto> GetContentAsync(int channelId, string culture, int? insightCount, int? whatsNewsCount, ContentCategoryFilter[] categoryFilters, CancellationToken cancellationToken);
}

public class ContentCategoryService : IContentCategoryService
{
    private const int DefaultInsightCount = 15;
    private const int MaxInsightCount = 50;
    private const int MaxSubCategoryIdsPerFilter = 50;
    private const int CacheMinutes = 10;
    private const string FallbackDisplayDateFormat = "dd MMMM yyyy";
    private static readonly HashSet<string> ValidCategoryCodes = new() { "BusinessIssue", "BusinessLine", "ServiceArea", "Industry" };
    private static readonly HashSet<string> OrderableCategoryColumns = new(StringComparer.Ordinal)
    {
        nameof(Businessissue.Name),
        "MetadataTitle",
    };

    private readonly IContentQueryExecutor _contentQueryExecutor;
    private readonly IMappingHelper _mappingHelper;
    private readonly IInfoProvider<ChannelInfo> _channelInfoProvider;
    private readonly IInfoProvider<WebsiteChannelInfo> _websiteChannelInfoProvider;
    private readonly IInfoProvider<ContentLanguageInfo> _contentLanguageInfoProvider;
    private readonly IValidationState _validationState;
    private readonly IWebPageUrlRetriever _webPageUrlRetriever;
    private readonly IProgressiveCache _cache;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ContentCategoryService> _logger;

    public ContentCategoryService(
        IContentQueryExecutor contentQueryExecutor,
        IMappingHelper mappingHelper,
        IInfoProvider<ChannelInfo> channelInfoProvider,
        IInfoProvider<WebsiteChannelInfo> websiteChannelInfoProvider,
        IInfoProvider<ContentLanguageInfo> contentLanguageInfoProvider,
        IValidationState validationState,
        IWebPageUrlRetriever webPageUrlRetriever,
        IProgressiveCache cache,
        IConfiguration configuration,
        ILogger<ContentCategoryService> logger)
    {
        _contentQueryExecutor = contentQueryExecutor;
        _mappingHelper = mappingHelper;
        _channelInfoProvider = channelInfoProvider;
        _websiteChannelInfoProvider = websiteChannelInfoProvider;
        _contentLanguageInfoProvider = contentLanguageInfoProvider;
        _validationState = validationState;
        _webPageUrlRetriever = webPageUrlRetriever;
        _cache = cache;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<IEnumerable<ContentCategoryDto>> GetContentCategoriesAsync(int channelId, string culture, CancellationToken cancellationToken)
    {
        var channel = await GetChannelAsync(channelId, cancellationToken);
        var languageTag = await ResolveCultureAsync(channelId, culture, cancellationToken);

        return new List<ContentCategoryDto>
        {
            await GetContentCategoryDtoAsync("Business Issue", "BusinessIssue", Businessissue.CONTENT_TYPE_NAME, nameof(Businessissue.Name), channel.ChannelName, languageTag, cancellationToken),
            await GetContentCategoryDtoAsync("Business Line", "BusinessLine", BusinessLine.CONTENT_TYPE_NAME, "MetadataTitle", channel.ChannelName, languageTag, cancellationToken),
            await GetContentCategoryDtoAsync("Service Area", "ServiceArea", ServiceArea.CONTENT_TYPE_NAME, "MetadataTitle", channel.ChannelName, languageTag, cancellationToken),
            await GetContentCategoryDtoAsync("Industry", "Industry", Industry.CONTENT_TYPE_NAME, "MetadataTitle", channel.ChannelName, languageTag, cancellationToken),
        };
    }

    public async Task<ContentDto> GetContentAsync(
        int channelId, string culture, int? insightCount, int? whatsNewsCount,
        ContentCategoryFilter[] categoryFilters, CancellationToken cancellationToken)
    {
        var channel = await GetChannelAsync(channelId, cancellationToken);
        var languageTag = await ResolveCultureAsync(channelId, culture, cancellationToken);

        categoryFilters ??= Array.Empty<ContentCategoryFilter>();
        ValidateContentCategoryFilters(categoryFilters);

        var topN = NormalizeInsightCount(insightCount);
        var subCategoryIdsByCategoryCode = BuildSubCategoryIdsByCategoryCode(categoryFilters);

        var projections = await GetInsightProjectionsAsync(
            channel.ChannelName, languageTag, topN, categoryFilters.Length != 0, subCategoryIdsByCategoryCode, cancellationToken);

        var insights = await BuildInsightDtosAsync(projections, languageTag, cancellationToken);

        return new ContentDto
        {
            InsightsSearchURL = await GetInsightSearchUrlAsync(channel.ChannelName, languageTag.Locale, cancellationToken),
            Insights = insights,
        };
    }

    private static int NormalizeInsightCount(int? insightCount)
    {
        if (insightCount is null or <= 0)
        {
            return DefaultInsightCount;
        }

        return Math.Min(insightCount.Value, MaxInsightCount);
    }


    private async Task<ContentCategoryDto> GetContentCategoryDtoAsync(
        string name, string codeName, string contentTypeName, string titleColumn,
        string channelName, LanguageTag languageTag, CancellationToken cancellationToken)
    {
        var cacheSettings = new CacheSettings(CacheMinutes, nameof(ContentCategoryService), nameof(GetContentCategoryDtoAsync), channelName, languageTag.Locale, contentTypeName)
        {
            GetCacheDependency = () => CacheHelper.GetCacheDependency(new[]
            {
                $"webpageitem|bychannel|{channelName}|bycontenttype|{contentTypeName}",
            }),
        };

        var subCategories = await _cache.LoadAsync(
            async _ => await GetSubCategoriesAsync(contentTypeName, titleColumn, channelName, languageTag, cancellationToken),
            cacheSettings);

        return new ContentCategoryDto
        {
            Name = name,
            Code = codeName,
            SubCategories = subCategories,
        };
    }

    private async Task<List<ContentSubCategoryDto>> GetSubCategoriesAsync(
        string contentTypeName, string titleColumn, string channelName, LanguageTag languageTag, CancellationToken cancellationToken)
    {
        var idColumn = nameof(ContentItemFields.ContentItemID);

        var builder = new ContentItemQueryBuilder()
            .ForContentType(contentTypeName, config => config
                .ForWebsite(channelName)
                .Columns(idColumn, titleColumn)
                .OrderBy(GetCategoryOrderColumn(titleColumn, languageTag)))
            .InLanguage(languageTag.Locale);

        var rows = await _contentQueryExecutor.GetResult(
            builder,
            row => new ContentSubCategoryDto
            {
                Id = row.GetValue<int>(idColumn),
                Name = row.GetValue<string>(titleColumn),
            },
            cancellationToken: cancellationToken);

        return rows.ToList();
    }

    private static OrderByColumn GetCategoryOrderColumn(string columnName, LanguageTag languageTag)
    {
        if (!OrderableCategoryColumns.Contains(columnName))
        {
            throw new ArgumentException($"Column '{columnName}' is not allowed in ORDER BY.", nameof(columnName));
        }

        // CultureToCollationMapper is keyed by the full language tag (e.g. "en-GB", "sr-Cyrl-RS").
        var collation = CultureToCollationMapper.GetCollationForCulture(languageTag.ToString());

        return new OrderByColumn(
            collation is null ? columnName : $"{columnName} COLLATE {collation}",
            OrderDirection.Ascending);
    }


    private async Task<IReadOnlyList<InsightProjection>> GetInsightProjectionsAsync(
        string channelName, LanguageTag languageTag, int topN, bool hasFilters,
        IReadOnlyDictionary<string, int[]> subCategoryIdsByCategoryCode, CancellationToken cancellationToken)
    {
        var linkingFields = hasFilters
            ? GetLinkingFields(subCategoryIdsByCategoryCode)
            : new List<(string FieldName, int[] Ids)> { (string.Empty, Array.Empty<int>()) };

        if (linkingFields.Count == 0)
        {
            return Array.Empty<InsightProjection>();
        }

        var projections = new List<InsightProjection>();

        foreach (var (fieldName, ids) in linkingFields)
        {
            projections.AddRange(await GetPagesAsync<BlogPost>(
                BlogPost.CONTENT_TYPE_NAME, nameof(BlogPost), channelName, languageTag, topN, fieldName, ids, MapBlogPost, 1, cancellationToken));

            projections.AddRange(await GetPagesAsync<Insight>(
                Insight.CONTENT_TYPE_NAME, nameof(Insight), channelName, languageTag, topN, fieldName, ids, MapInsight, 2, cancellationToken));
        }
        return projections
            .GroupBy(x => x.WebPageItemId)
            .Select(group => group.First())
            .OrderByDescending(x => x.Date)
            .ThenByDescending(x => x.WebPageItemId)
            .Take(topN)
            .ToList();
    }

    private static List<(string FieldName, int[] Ids)> GetLinkingFields(IReadOnlyDictionary<string, int[]> subCategoryIdsByCategoryCode)
    {
        var fieldNameByCategoryCode = new (string CategoryCode, string FieldName)[]
        {
            ("BusinessIssue", nameof(BlogPost.MetadataBusinessIssues)),
            ("BusinessLine", nameof(BlogPost.MetadataBusinessLine)),
            ("ServiceArea", nameof(BlogPost.MetadataServiceAreas)),
            ("Industry", nameof(BlogPost.MetadataIndustries)),
        };

        return fieldNameByCategoryCode
            .Where(x => subCategoryIdsByCategoryCode.TryGetValue(x.CategoryCode, out var ids) && ids.Length != 0)
            .Select(x => (x.FieldName, subCategoryIdsByCategoryCode[x.CategoryCode]))
            .ToList();
    }

    private async Task<IEnumerable<InsightProjection>> GetPagesAsync<TPage>(
        string contentTypeName, string pageType, string channelName, LanguageTag languageTag, int topN,
        string linkingFieldName, int[] linkedItemIds, Func<TPage, string, InsightProjection> map,
        int linkedItemsDepth, CancellationToken cancellationToken)
        where TPage : class, IWebPageFieldsSource
    {
        async Task<List<TPage>> LoadPagesAsync()
        {
            var builder = new ContentItemQueryBuilder()
                .ForContentType(contentTypeName, config =>
                {
                    config.ForWebsite(channelName)
                        .WithLinkedItems(linkedItemsDepth);

                    if (!string.IsNullOrEmpty(linkingFieldName) && linkedItemIds.Length != 0)
                    {
                        config.Linking(linkingFieldName, linkedItemIds);
                    }

                    config
                        .Where(w => w.WhereEquals("IsSearchExcluded", false).Or().WhereNull("IsSearchExcluded"))
                        .OrderBy(
                            new OrderByColumn(nameof(ContentItemFields.ContentItemCommonDataFirstPublishedWhen), OrderDirection.Descending),
                            new OrderByColumn(nameof(WebPageFields.WebPageItemID), OrderDirection.Descending))
                        .TopN(topN);
                })
                .InLanguage(languageTag.Locale, useLanguageFallbacks: true);

            var result = await GetMappedPagesAsync<TPage>(builder, cancellationToken);
            return result.ToList();
        }

        List<TPage> pages;

        if (linkedItemIds.Length != 0)
        {
            pages = await LoadPagesAsync();
        }
        else
        {
            var cacheSettings = new CacheSettings(
                CacheMinutes,
                nameof(ContentCategoryService),
                nameof(GetPagesAsync),
                contentTypeName,
                channelName,
                languageTag.Locale,
                topN.ToString(CultureInfo.InvariantCulture))
            {
                GetCacheDependency = () => CacheHelper.GetCacheDependency(new[]
                {
                    $"webpageitem|bychannel|{channelName}|bycontenttype|{contentTypeName}",
                }),
            };

            pages = await _cache.LoadAsync(async _ => await LoadPagesAsync(), cacheSettings);
        }

        return pages.Select(page => map(page, pageType));
    }

    private async Task<List<InsightContentDto>> BuildInsightDtosAsync(
        IReadOnlyList<InsightProjection> projections, LanguageTag languageTag, CancellationToken cancellationToken)
    {
        if (projections.Count == 0)
        {
            return new List<InsightContentDto>();
        }

        var urlByWebPageItemId = await GetPageUrlsAsync(
            projections.Select(x => x.WebPageItemId).Distinct().ToList(), languageTag.Locale, cancellationToken);

        var dateCulture = GetCultureInfo(languageTag);
        var dateFormat = GetDisplayDateFormat();
        var fallbackDate = DateTime.Now;

        return projections
            .Select(projection =>
            {
                var pageUrl = urlByWebPageItemId.TryGetValue(projection.WebPageItemId, out var url) ? url : string.Empty;
                var date = projection.Date == default ? fallbackDate : projection.Date;
                var imageUrl = GetAbsoluteImageUrl(projection.ImageUrl, pageUrl);

                return new InsightContentDto
                {
                    GUID = projection.WebPageItemGuid,
                    PageType = projection.PageType,
                    PageSubType = projection.PageSubType,
                    TitleFull = projection.Title,
                    TitleTwoThirds = projection.Title,
                    TitleOneThird = projection.Title,
                    SummaryFull = projection.Summary,
                    SummaryTwoThirds = projection.Summary,
                    SummaryOneThird = projection.Summary,
                    ImageUrlFull = imageUrl,
                    ImageUrlTwoThirds = imageUrl,
                    ImageUrlOneThird = imageUrl,
                    Date = date,
                    DisplayDate = date.ToString(dateFormat, dateCulture),
                    URL = pageUrl,
                };
            })
            .ToList();
    }

    private async Task<Dictionary<int, string>> GetPageUrlsAsync(
        IReadOnlyCollection<int> webPageItemIds, string culture, CancellationToken cancellationToken)
    {
        var urlByWebPageItemId = new Dictionary<int, string>(webPageItemIds.Count);

        foreach (var webPageItemId in webPageItemIds)
        {
            var url = await _webPageUrlRetriever.Retrieve(webPageItemId, culture, false, cancellationToken);

            urlByWebPageItemId[webPageItemId] = ToHtmlCompatibleUrl(url?.AbsoluteUrl);
        }

        return urlByWebPageItemId;
    }

    private string GetDisplayDateFormat()
    {
        var format = _configuration.GetValue<string?>("Content:DisplayDateFormat");

        return string.IsNullOrWhiteSpace(format) ? FallbackDisplayDateFormat : format;
    }

    private CultureInfo GetCultureInfo(LanguageTag languageTag)
    {
        foreach (var name in new[] { languageTag.ToString(), languageTag.Locale })
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                continue;
            }

            try
            {
                return CultureInfo.GetCultureInfo(name);
            }
            catch (CultureNotFoundException)
            {
                // try next candidate
            }
        }

        _logger.LogWarning("Culture {Culture} is not available on the server, falling back to the invariant culture.", languageTag.Locale);

        return CultureInfo.InvariantCulture;
    }

    private static string ToHtmlCompatibleUrl(string? url)
    {
        return Link.TryParse(url, out var link) ? link.ToHtmlCompatibleString() : string.Empty;
    }

    private static string GetAbsoluteImageUrl(string? imageUrl, string pageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
        {
            return string.Empty;
        }

        var hasPageUrl = Uri.TryCreate(pageUrl, UriKind.Absolute, out var pageUri)
            && (pageUri.Scheme == Uri.UriSchemeHttp || pageUri.Scheme == Uri.UriSchemeHttps);

        if (imageUrl.StartsWith("//", StringComparison.Ordinal))
        {
            imageUrl = $"{(hasPageUrl ? pageUri!.Scheme : Uri.UriSchemeHttps)}:{imageUrl}";
        }
        else if (imageUrl.StartsWith("~/", StringComparison.Ordinal))
        {
            imageUrl = imageUrl[1..];
        }

        if (!Uri.TryCreate(imageUrl, UriKind.RelativeOrAbsolute, out var imageUri))
        {
            return string.Empty;
        }

        if (imageUri.IsAbsoluteUri)
        {
            return imageUri.Scheme == Uri.UriSchemeHttp || imageUri.Scheme == Uri.UriSchemeHttps
                ? ToHtmlCompatibleUrl(imageUri.AbsoluteUri)
                : string.Empty;
        }

        if (!hasPageUrl)
        {
            return string.Empty;
        }

        var origin = new Uri(pageUri!.GetLeftPart(UriPartial.Authority) + "/");

        return ToHtmlCompatibleUrl(new Uri(origin, imageUri).AbsoluteUri);
    }

    private async Task<string?> GetInsightSearchUrlAsync(string channelName, string culture, CancellationToken cancellationToken)
    {
        var builder = new ContentItemQueryBuilder()
            .ForContentType(Search.CONTENT_TYPE_NAME, config => config
                .ForWebsite(channelName)
                .Columns(nameof(WebPageFields.WebPageItemID))
                .TopN(1))
            .InLanguage(culture);

        var searchPages = await _contentQueryExecutor.GetResult(
            builder,
            row => row.GetValue<int>(nameof(WebPageFields.WebPageItemID)),
            cancellationToken: cancellationToken);

        var webPageItemId = searchPages.FirstOrDefault();

        if (webPageItemId == 0)
        {
            return null;
        }

        var url = await _webPageUrlRetriever.Retrieve(webPageItemId, culture, false, cancellationToken);

        var absoluteUrl = ToHtmlCompatibleUrl(url?.AbsoluteUrl);

        return string.IsNullOrEmpty(absoluteUrl) ? null : absoluteUrl;
    }

    private async Task<ChannelInfo> GetChannelAsync(int channelId, CancellationToken cancellationToken)
    {
        var channel = await _channelInfoProvider.GetAsync(channelId, cancellationToken);

        if (string.IsNullOrEmpty(channel?.ChannelName))
        {
            throw new NotFoundException($"Channel with ID {channelId} not found.");
        }

        return channel;
    }

    private async Task<LanguageTag> ResolveCultureAsync(int channelId, string culture, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(culture))
        {
            culture = await GetChannelDefaultCultureAsync(channelId, cancellationToken);
        }

        var languageTag = await _mappingHelper.ValidateAndNormalizeSiteCultureAsync(channelId, culture, cancellationToken);

        if (languageTag is null || string.IsNullOrEmpty(languageTag.Locale))
        {
            _validationState.AddError("culture", $"Incorrect language tag '{culture}' format.");
        }

        ThrowIfThereAreValidationErrors();

        return languageTag!;
    }

    private async Task<string> GetChannelDefaultCultureAsync(int channelId, CancellationToken cancellationToken)
    {
        var websiteChannels = await _websiteChannelInfoProvider.Get()
            .WhereEquals(nameof(WebsiteChannelInfo.WebsiteChannelChannelID), channelId)
            .TopN(1)
            .GetEnumerableTypedResultAsync(cancellationToken: cancellationToken);

        var websiteChannel = websiteChannels.FirstOrDefault()
            ?? throw new NotFoundException($"Website channel with ID {channelId} not found.");

        var language = await _contentLanguageInfoProvider.GetAsync(websiteChannel.WebsiteChannelPrimaryContentLanguageID, cancellationToken);

        if (string.IsNullOrEmpty(language?.ContentLanguageName))
        {
            throw new NotFoundException($"No primary content language is configured for channel with ID {channelId}.");
        }

        return language.ContentLanguageName;
    }

    private void ValidateContentCategoryFilters(ContentCategoryFilter[] filters)
    {
        foreach (var filter in filters)
        {
            if (filter.CategoryCode is null || !ValidCategoryCodes.Contains(filter.CategoryCode))
            {
                _validationState.AddUnprocessableEntityError(nameof(filter.CategoryCode), $"Not valid category code: {filter.CategoryCode ?? "null"}");
                continue;
            }

            if (filter.SubCategoryIds is null || filter.SubCategoryIds.Length == 0)
            {
                _validationState.AddUnprocessableEntityError(nameof(filter.SubCategoryIds), $"List of subcategory ids is empty on {filter.CategoryCode} category");
                continue;
            }

            if (filter.SubCategoryIds.Length > MaxSubCategoryIdsPerFilter)
            {
                _validationState.AddUnprocessableEntityError(nameof(filter.SubCategoryIds), $"List of subcategory ids on {filter.CategoryCode} category exceeds the maximum of {MaxSubCategoryIdsPerFilter}");
            }
        }

        ThrowIfThereAreValidationErrors();
    }

    private void ThrowIfThereAreValidationErrors()
    {
        if (!_validationState.HasErrors() && !_validationState.HasUnprocessableEntityErrors())
        {
            return;
        }
        var message = string.Join("; ", _validationState.GetErrors().Select(error => $"{error.Key}: {error.Value}"));

        if (_validationState.HasErrors())
        {
            throw new ValidationException(message);
        }

        throw new UnprocessableEntityException(message);
    }

    private static Dictionary<string, int[]> BuildSubCategoryIdsByCategoryCode(IEnumerable<ContentCategoryFilter> categoryFilters)
    {
        var map = new Dictionary<string, int[]>();

        foreach (var filter in categoryFilters)
        {
            if (filter.CategoryCode is null || filter.SubCategoryIds is null) continue;

            map[filter.CategoryCode] = filter.SubCategoryIds;
        }

        foreach (var code in ValidCategoryCodes)
        {
            map.TryAdd(code, Array.Empty<int>());
        }

        return map;
    }
    protected virtual Task<IEnumerable<TPage>> GetMappedPagesAsync<TPage>(
    ContentItemQueryBuilder builder, CancellationToken cancellationToken)
    where TPage : class, IWebPageFieldsSource =>
    _contentQueryExecutor.GetMappedResult<TPage>(builder, cancellationToken: cancellationToken);

    private sealed record InsightProjection(
        Guid WebPageItemGuid,
        int WebPageItemId,
        string PageType,
        string PageSubType,
        string Title,
        string Summary,
        string? ImageUrl,
        DateTime Date);

    private static InsightProjection MapBlogPost(BlogPost page, string pageType) => MapPage(
        page.SystemFields, pageType, page.MetadataTitle, page.MetadataDescription,
        page.ImageFileM?.FirstOrDefault()?.ContentAttachmentAsset?.Url, page.ContentTypeFilter);

    private static InsightProjection MapInsight(Insight page, string pageType) => MapPage(
     page.SystemFields, pageType, page.MetadataTitle, page.MetadataDescription,
     page.InsightContent?.FirstOrDefault()?.ImageFile?.FirstOrDefault()?.ContentAttachmentAsset?.Url, page.ContentTypeFilter);

    private static InsightProjection MapPage(
        WebPageFields systemFields, string pageType, string? title, string? description,
        string? imageUrl, IEnumerable<Contenttypes>? contentTypeFilter) => new(
            WebPageItemGuid: systemFields.WebPageItemGUID,
            WebPageItemId: systemFields.WebPageItemID,
            PageType: pageType,
            PageSubType: contentTypeFilter?.FirstOrDefault()?.Name ?? string.Empty,
            Title: title ?? string.Empty,
            Summary: HtmlHelper.ExtractTextFromHtml(description) ?? string.Empty,
            ImageUrl: imageUrl,
            Date: systemFields.ContentItemCommonDataFirstPublishedWhen ?? default);
}