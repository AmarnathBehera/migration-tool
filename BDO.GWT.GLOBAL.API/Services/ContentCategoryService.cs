using BDO.GWT.Common.Core.Exceptions;
using BDO.GWT.Common.Core.Services;
using BDO.GWT.Core;
using BDO.GWT.Core.Services;
using BDO.GWT.Core.ValueObjects;
using BDO.GWT.GLOBAL.API.DTOs;
using BDO.GWT.GLOBAL.API.Helpers;
using CMS.ContentEngine;
using CMS.DataEngine;
using CMS.Websites;
using CMS.Websites.Routing;
using Kentico.Content.Web.Mvc;
using Microsoft.AspNetCore.Mvc;

namespace BDO.GWT.GLOBAL.API.Services;

public interface IContentCategoryService
{

    Task<ActionResult<IEnumerable<ContentCategoryDto>>> GetContentCategoriesAsync(int channelId, string culture, CancellationToken cancellationToken);

    Task<ActionResult<ContentDto>> GetContentAsync(int channelId, string culture, int? insightCount, int? whatsNewCount, ContentCategoryFilter[] categoryFilters, CancellationToken cancellationToken);
}

public class ContentCategoryService : IContentCategoryService
{
    private static readonly HashSet<string> ValidCategoryCodes = new() { "BusinessIssue", "BusinessLine", "ServiceArea", "Industry" };
    private static readonly LanguageTag defaultCulture = LanguageTag.GB;

    private readonly IContentQueryExecutor _contentQueryExecutor;
    private readonly IContentService _contentRetriever;
    private readonly IMappingHelper _mappingHelper;
    private readonly IInfoProvider<ChannelInfo> _channelInfoProvider;
    private readonly IValidationState _validationState;
    private readonly IInfoProvider<WebsiteChannelInfo> _websiteChannelInfoProvider;
    private readonly IWebPageUrlRetriever _webPageUrlRetriever;

    public ContentCategoryService(
        IContentQueryExecutor contentQueryExecutor,
        IContentService contentRetriever,
        IMappingHelper mappingHelper,
        IInfoProvider<ChannelInfo> channelInfoProvider,
        IValidationState validationState,
        IInfoProvider<WebsiteChannelInfo> websiteChannelInfoProvider,
        IWebPageUrlRetriever webPageUrlRetriever)
    {
        _contentQueryExecutor = contentQueryExecutor;
        _contentRetriever = contentRetriever;
        _mappingHelper = mappingHelper;
        _channelInfoProvider = channelInfoProvider;
        _validationState = validationState;
        _websiteChannelInfoProvider = websiteChannelInfoProvider;
        _webPageUrlRetriever = webPageUrlRetriever;
    }


    public async Task<ActionResult<IEnumerable<ContentCategoryDto>>> GetContentCategoriesAsync(int channelId, string culture, CancellationToken cancellationToken)
    {
        try
        {
            var currentCulture = await _mappingHelper.ValidateAndNormalizeSiteCultureAsync(channelId, culture ?? defaultCulture.Language, cancellationToken) ?? defaultCulture;

            if (string.IsNullOrEmpty(currentCulture.Locale))
            {
                throw new NotFoundException("Invalid culture specified");
            }

            var channel = await GetChannelAsync(channelId);
            if (string.IsNullOrEmpty(channel?.ChannelName))
            {
                throw new NotFoundException("Channel not found");
            }

            var categories = new List<ContentCategoryDto>
            {
                await GetBusinessIssueCategoryDtoAsync(channel.ChannelName, currentCulture, cancellationToken),
                await GetContentCategoryDtoAsync("Business Line", "BusinessLine", BusinessLine.CONTENT_TYPE_NAME, channel.ChannelName, currentCulture, cancellationToken),
                await GetContentCategoryDtoAsync("Service Area", "ServiceArea", ServiceArea.CONTENT_TYPE_NAME, channel.ChannelName, currentCulture, cancellationToken),
                await GetContentCategoryDtoAsync("Industry", "Industry", Industry.CONTENT_TYPE_NAME, channel.ChannelName, currentCulture, cancellationToken),
            };

            return categories;
        }
        catch (UnprocessableEntityException)
        {
            return new UnprocessableEntityObjectResult(_validationState.GetErrors());
        }
        catch (NotFoundException ex)
        {
            return new NotFoundObjectResult(ex.Message);
        }
        catch (Exception ex)
        {
            return new ObjectResult(ex.Message) { StatusCode = StatusCodes.Status500InternalServerError };
        }
    }

    private async Task<ContentCategoryDto> GetBusinessIssueCategoryDtoAsync(string channelName, LanguageTag languageTag, CancellationToken cancellationToken)
    {
        var builder = new ContentItemQueryBuilder()
            .ForContentType(Businessissue.CONTENT_TYPE_NAME, config => config
                .ForWebsite(channelName)
                .Columns(nameof(Businessissue.SystemFields.ContentItemID), nameof(Businessissue.Name)))
            .InLanguage(languageTag.Locale)
            .Parameters(q => q.OrderBy(new OrderByColumn(nameof(Businessissue.Name))));

        var businessIssues = await _contentQueryExecutor.GetMappedResult<Businessissue>(
            builder, cancellationToken: cancellationToken);

        return new ContentCategoryDto
        {
            Name = "Business Issue",
            Code = "BusinessIssue",
            SubCategories = businessIssues
                .Select(businessIssue => new ContentSubCategoryDto
                {
                    Id = businessIssue.SystemFields.ContentItemID,
                    Name = businessIssue.Name,
                })
                .ToList(),
        };
    }

    private async Task<ContentCategoryDto> GetContentCategoryDtoAsync(
        string name, string codeName, string contentTypeName, string channelName, LanguageTag languageTag, CancellationToken cancellationToken)
    {
        var builder = new ContentItemQueryBuilder()
            .ForContentType(contentTypeName, config => config
                .ForWebsite(channelName)
                .Columns("ContentItemID", "MetadataTitle"))
            .InLanguage(languageTag.Locale)
            .Parameters(q => q.OrderBy(new OrderByColumn("MetadataTitle")));

        var pages = await _contentQueryExecutor.GetResult(builder, row => new
        {
            ContentItemID = row.GetValue<int>("ContentItemID"),
            MetadataTitle = row.GetValue<string>("MetadataTitle"),
        }, cancellationToken: cancellationToken);

        return new ContentCategoryDto
        {
            Name = name,
            Code = codeName,
            SubCategories = pages
                .Select(page => new ContentSubCategoryDto
                {
                    Id = page.ContentItemID,
                    Name = page.MetadataTitle,
                })
                .ToList(),
        };
    }


    public async Task<ActionResult<ContentDto>> GetContentAsync(
        int channelId, string culture, int? insightCount, int? whatsNewCount,
        ContentCategoryFilter[] categoryFilters, CancellationToken cancellationToken)
    {
        try
        {
            var currentCulture = await _mappingHelper.ValidateAndNormalizeSiteCultureAsync(channelId, culture ?? defaultCulture.Language, cancellationToken) ?? defaultCulture;

            if (string.IsNullOrEmpty(currentCulture.Locale))
            {
                throw new NotFoundException("Invalid culture specified");
            }

            var channel = await GetChannelAsync(channelId);
            if (string.IsNullOrEmpty(channel?.ChannelName))
            {
                throw new NotFoundException("Channel not found");
            }

            categoryFilters ??= Array.Empty<ContentCategoryFilter>();
            ValidateContentCategoryFilters(categoryFilters);

            if (insightCount is null or <= 0)
            {
                insightCount = 15;
            }
            var subCategoryIdsByCategoryCode = BuildSubCategoryIdsByCategoryCode(categoryFilters);

            var retrieveParameters = new RetrievePagesParameters
            {
                LinkedItemsMaxLevel = 1,
                ChannelName = channel.ChannelName,
                LanguageName = currentCulture.Locale,
                IsForPreview = false,
            };

            var blogPosts = await _contentRetriever.GetPagesAsync<BlogPost>(
                retrieveParameters,
                query => query.Where(w => w.WhereEquals("IsSearchExcluded", false)),
                RetrievalCacheSettings.CacheDisabled,
                cancellationToken);

            var insights = await _contentRetriever.GetPagesAsync<Insight>(
                retrieveParameters,
                query => query.Where(w => w.WhereEquals("IsSearchExcluded", false)),
                RetrievalCacheSettings.CacheDisabled,
                cancellationToken);

            var allInsights = (blogPosts ?? Enumerable.Empty<BlogPost>()).Select(MapBlogPostToProjection)
                .Concat((insights ?? Enumerable.Empty<Insight>()).Select(MapInsightToProjection));

            if (categoryFilters.Length != 0)
            {
                allInsights = allInsights.Where(item =>
                    item.BusinessIssueIds.Intersect(subCategoryIdsByCategoryCode["BusinessIssue"]).Any() ||
                    item.BusinessLineIds.Intersect(subCategoryIdsByCategoryCode["BusinessLine"]).Any() ||
                    item.ServiceAreaIds.Intersect(subCategoryIdsByCategoryCode["ServiceArea"]).Any() ||
                    item.IndustryIds.Intersect(subCategoryIdsByCategoryCode["Industry"]).Any());
            }

            var dto = new ContentDto
            {
                InsightsSearchURL = await GetInsightSearchUrlAsync(channelId, channel.ChannelName, currentCulture.Locale, cancellationToken),
                Insights = allInsights
                    .OrderByDescending(i => i.Date)
                    .Take(insightCount.Value)
                    .Select(i => i.ToDto())
                    .ToList(),
            };

            return dto;
        }
        catch (UnprocessableEntityException)
        {
            return new UnprocessableEntityObjectResult(_validationState.GetErrors());
        }
        catch (NotFoundException ex)
        {
            return new NotFoundObjectResult(ex.Message);
        }
        catch (Exception ex)
        {
            return new ObjectResult(ex.Message) { StatusCode = StatusCodes.Status500InternalServerError };
        }
    }

    private async Task<string?> GetInsightSearchUrlAsync(int channelId, string channelName, string culture, CancellationToken cancellationToken)
    {
        var pageUrl = await GetInsightSearchUrlByPageAsync(channelName, culture, cancellationToken);
        if (!string.IsNullOrWhiteSpace(pageUrl))
        {
            return pageUrl;
        }

        return await GetInsightSearchUrlByDomainAsync(channelId, culture);
    }

    private async Task<string?> GetInsightSearchUrlByPageAsync(string channelName, string culture, CancellationToken cancellationToken)
    {
        var builder = new ContentItemQueryBuilder()
            .ForContentType(Search.CONTENT_TYPE_NAME, config => config
                .ForWebsite(channelName)
                .Columns("WebPageItemID"))
            .InLanguage(culture)
            .Parameters(q => q.TopN(1));

        var searchPages = await _contentQueryExecutor.GetResult(builder, row => new
        {
            WebPageItemID = row.GetValue<int>("WebPageItemID"),
        }, cancellationToken: cancellationToken);

        var searchPage = searchPages.FirstOrDefault();

        if (searchPage is null)
        {
            return null;
        }

        var url = await _webPageUrlRetriever.Retrieve(searchPage.WebPageItemID, culture, cancellationToken: cancellationToken);

        return url?.AbsoluteUrl;
    }

    private async Task<string?> GetInsightSearchUrlByDomainAsync(int channelId, string culture)
    {
        var websiteChannel = await GetWebsiteChannelAsync(channelId);

        var domain = websiteChannel?.WebsiteChannelDomain;

        if (string.IsNullOrWhiteSpace(domain))
        {
            return null;
        }

        var normalizedDomain = domain.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            || domain.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
                ? domain
                : $"https://{domain}";

        return $"{normalizedDomain.TrimEnd('/')}/{culture}/search";
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
            }
        }

        if (_validationState.HasUnprocessableEntityErrors() || _validationState.HasErrors())
        {
            throw new UnprocessableEntityException();
        }
    }

    private Task<ChannelInfo?> GetChannelAsync(int channelId)
    {
        var provider = _channelInfoProvider as IInfoByIdProvider<ChannelInfo>;
        return Task.FromResult(provider?.Get(channelId));
    }

    private Task<WebsiteChannelInfo?> GetWebsiteChannelAsync(int channelId)
    {
        var provider = _websiteChannelInfoProvider as IInfoByIdProvider<WebsiteChannelInfo>;
        return Task.FromResult(provider?.Get(channelId));
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

    private sealed record InsightProjection(
        Guid Guid, string PageType, string Title, string Summary, string? ImageUrl,
        DateTime Date, string Url,
        int[] BusinessIssueIds, int[] BusinessLineIds, int[] ServiceAreaIds, int[] IndustryIds)
    {
        public InsightContentDto ToDto() => new()
        {
            GUID = Guid,
            PageType = PageType,
            PageSubType = string.Empty,
            TitleFull = Title,
            TitleTwoThirds = Title,
            TitleOneThird = Title,
            SummaryFull = Summary,
            SummaryTwoThirds = Summary,
            SummaryOneThird = Summary,
            ImageUrlFull = ImageUrl ?? string.Empty,
            ImageUrlTwoThirds = ImageUrl ?? string.Empty,
            ImageUrlOneThird = ImageUrl ?? string.Empty,
            Date = Date,
            DisplayDate = Date == default ? string.Empty : Date.ToString("d MMMM yyyy"),
            URL = Url,
        };
    }

    private static InsightProjection MapBlogPostToProjection(BlogPost page) => new(
        Guid: page.SystemFields.ContentItemGUID,
        PageType: nameof(BlogPost),
        Title: page.MetadataTitle ?? string.Empty,
        Summary: page.MetadataDescription ?? string.Empty,
        ImageUrl: page.MetadataTeaserImage?.FirstOrDefault()?.ContentAttachmentAsset?.Url,
        Date: page.SystemFields.ContentItemCommonDataLastPublishedWhen ?? default,
        Url: page.SystemFields.WebPageUrlPath ?? string.Empty,
        BusinessIssueIds: page.MetadataBusinessIssues?.Select(x => x.SystemFields.ContentItemID).ToArray() ?? Array.Empty<int>(),
        BusinessLineIds: page.MetadataBusinessLine?.Select(x => x.SystemFields.ContentItemID).ToArray() ?? Array.Empty<int>(),
        ServiceAreaIds: page.MetadataServiceAreas?.Select(x => x.SystemFields.ContentItemID).ToArray() ?? Array.Empty<int>(),
        IndustryIds: page.MetadataIndustries?.Select(x => x.SystemFields.ContentItemID).ToArray() ?? Array.Empty<int>());

    private static InsightProjection MapInsightToProjection(Insight page) => new(
        Guid: page.SystemFields.ContentItemGUID,
        PageType: nameof(Insight),
        Title: page.MetadataTitle ?? string.Empty,
        Summary: page.MetadataDescription ?? string.Empty,
        ImageUrl: page.MetadataTeaserImage?.FirstOrDefault()?.ContentAttachmentAsset?.Url,
        Date: page.SystemFields.ContentItemCommonDataLastPublishedWhen ?? default,
        Url: page.SystemFields.WebPageUrlPath ?? string.Empty,
        BusinessIssueIds: page.MetadataBusinessIssues?.Select(x => x.SystemFields.ContentItemID).ToArray() ?? Array.Empty<int>(),
        BusinessLineIds: page.MetadataBusinessLine?.Select(x => x.SystemFields.ContentItemID).ToArray() ?? Array.Empty<int>(),
        ServiceAreaIds: page.MetadataServiceAreas?.Select(x => x.SystemFields.ContentItemID).ToArray() ?? Array.Empty<int>(),
        IndustryIds: page.MetadataIndustries?.Select(x => x.SystemFields.ContentItemID).ToArray() ?? Array.Empty<int>());
}