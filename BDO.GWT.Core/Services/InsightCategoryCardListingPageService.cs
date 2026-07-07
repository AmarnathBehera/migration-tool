using BDO.GWT.Core;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.Data.Queries;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Cards;
using BDO.GWT.Core.DTOs.ReusableFieldsDTOs;
using BDO.GWT.Core.Helpers;
using BDO.GWT.Core.Services;
using BDO.GWT.Core.ValueObjects;
using CMS.ContentEngine;
using CMS.Websites;
using CMS.Websites.Routing;
using Kentico.Content.Web.Mvc;

namespace BDO.GWT.Web.Core.Services;

public interface IInsightCategoryCardListingPageService
{
    Task<InsightCardListingDto> GetInsightPageDtoAsync(InsightCategory page, CancellationToken cancellationToken);

    Task<PaginationDto<ContentPageCardDto>> GetContentPaginationResultAsync(Guid pageGuid, CardsQuery cardsQuery, CancellationToken cancellationToken);
}

public class InsightCategoryCardListingPageService : IInsightCategoryCardListingPageService
{
    private readonly IFilteringHelper _filteringHelper;
    private readonly IHeroService _heroService;
    private readonly IContentRetriever _contentRetriever;
    private readonly IWebsiteChannelContext _websiteChannelContext;
    private readonly IInsightCardService _insightCardService;

    public InsightCategoryCardListingPageService(IInsightCardService insightCardService, IWebsiteChannelContext websiteChannelContext, IContentRetriever contentRetriever, IFilteringHelper filteringHelper, IHeroService heroService)
    {
        _filteringHelper = filteringHelper;
        _heroService = heroService;
        _contentRetriever = contentRetriever;
        _websiteChannelContext = websiteChannelContext;
        _insightCardService = insightCardService;
    }

    public async Task<InsightCardListingDto> GetInsightPageDtoAsync(InsightCategory insightCategory, CancellationToken cancellationToken)
    {
        var insightPage = await _contentRetriever.RetrieveCurrentPage<InsightCategory>(new RetrieveCurrentPageParameters { LinkedItemsMaxLevel = 4 }, cancellationToken);
        var sys = insightPage.SystemFields;

        var dto = new InsightCardListingDto
        {
            Title = insightPage.MetadataTitle,
            Hero = await _heroService.GetHeroDtoAsync(new HeroData
            {
                Layout = insightPage.HeroLayout,
                Title = insightPage.HeroTitle,
                Description = insightPage.HeroDescription,
                BackgroundImage = insightPage.HeroBackgroundImage?.FirstOrDefault()?.ContentAttachmentAsset?.Url is string bgUrl ? Link.Parse(bgUrl) : null,
                BackgroundImageAltText = insightPage.HeroBackgroundImageAltText,
                MobileBackgroundImage = insightPage.HeroBackgroundImage_Mobile?.FirstOrDefault()?.ContentAttachmentAsset?.Url is string mobileUrl ? Link.Parse(mobileUrl) : null,
                MobileBackgroundImageAltText = insightPage.HeroBackgroundImage_MobileAltText,
                CallToActionButtonType = insightPage.HeroCallToActionButtonType,
                CallToActionButtonText = insightPage.HeroCallToActionButtonText,
                CallToActionButtonUrl = insightPage.HeroCallToActionButtonUrl?.FirstOrDefault()?.SystemFields?.WebPageUrlPath is string ctaPath ? Link.Parse(ctaPath) : null,
            }, cancellationToken),
            BaseContent = new BaseContent
            {
                ContentItemID = sys?.ContentItemID ?? 0,
                ContentItemGUID = sys?.ContentItemGUID ?? Guid.Empty,
                ContentItemName = sys?.ContentItemName,
                ContentItemContentTypeID = sys?.ContentItemContentTypeID ?? 0,
                ContentItemIsSecured = sys?.ContentItemIsSecured ?? false,
                ContentItemCommonDataContentLanguageID = sys?.ContentItemCommonDataContentLanguageID ?? 0,
                ContentItemCommonDataFirstPublishedWhen = sys?.ContentItemCommonDataFirstPublishedWhen ?? default,
                ContentItemCommonDataLastPublishedWhen = sys?.ContentItemCommonDataLastPublishedWhen ?? default
            },
            BaseWebPage = new BaseWebPage
            {
                WebPageItemGUID = sys?.WebPageItemGUID ?? Guid.Empty,
                WebPageItemID = sys?.WebPageItemID ?? 0,
                WebPageItemName = sys?.WebPageItemName,
                WebPageItemWebsiteChannelID = sys?.WebPageItemWebsiteChannelId ?? 0,
                WebPageUrlPath = sys?.WebPageUrlPath,
                WebPageItemOrder = sys?.WebPageItemOrder ?? 0,
                WebPageItemTreePath = sys?.WebPageItemTreePath,
                WebPageUrlPathContentLanguageID = sys?.WebPageUrlPathContentLanguageID ?? 0,
                WebPageItemParentID = sys?.WebPageItemParentID ?? 0
            },
            MetaData = new MetaDataDto
            {
                MetaTitle = insightPage.MetadataTitle,
                MetaDescription = insightPage.MetadataDescription,
                MetadataTeaserImage = insightPage.MetadataTeaserImage.FirstOrDefault()?.ContentAttachmentAsset?.Url,
                MetaTeaserImageAltText = insightPage.MetadataTeaserImageAltText,
                MetaCanonical = insightPage.MetadataCanonical,
            },
            SearchAndSEO = new SearchAndSEODto
            {
                MetadataNofollow = insightPage.MetadataNofollow,
                MetadataNoindex = insightPage.MetadataNoindex,
                IncludeInSitemap = insightPage.IncludeInSitemap,
            },
            OGMetaData = new OGMetaDataDto
            {
                MetadataOGTitle = insightPage.MetadataOGTitle,
                MetadataOGDescription = insightPage.MetadataOGDescription,
                MetadataOGImage = insightPage.MetadataOGImage.FirstOrDefault()?.ContentAttachmentAsset?.Url,
                MetadataOGTwitterImage = insightPage.MetadataOGTwitterImage.FirstOrDefault()?.ContentAttachmentAsset?.Url,
                MetadataOGTwitterImageSizeCheckbox = insightPage.MetadataOGTwitterImageSizeCheckbox,
                Url = insightPage.MetadataCanonical,
            },
            CardsGrid = await GetCardsGridDtoAsync(insightPage, cancellationToken),

        };

        return dto;
    }


    public async Task<PaginationDto<ContentPageCardDto>> GetContentPaginationResultAsync(
     Guid pageGuid,
     CardsQuery cardsQuery,
     CancellationToken cancellationToken)
    {
        var page = (await _contentRetriever.RetrievePagesByGuids<InsightCategory>(
            new[] { pageGuid },
            cancellationToken)).FirstOrDefault();

        if (page is null)
        {
            throw new ContentItemNotFoundException($"InsightCategory page not found for guid '{pageGuid}'.");
        }

        var treePath = page.SystemFields.WebPageItemTreePath;
        Action<RetrievePagesQueryParameters>? decorateQuery = string.IsNullOrWhiteSpace(treePath)
            ? null
            : q => q.Where(w => w.WhereStartsWith(nameof(WebPageFields.WebPageItemTreePath), $"{treePath}/"));

        var featuredContentPageIds = new HashSet<int>();

        return await _insightCardService.GetFilteredItemsAsync(
            cardsQuery,
            treePath,
            decorateQuery,
            contentPage => featuredContentPageIds.Contains(contentPage.SystemFields.WebPageItemID),
            cancellationToken);
    }


    private async Task<CardsGridDto> GetCardsGridDtoAsync(InsightCategory page, CancellationToken cancellationToken)
    {
        var results = await _contentRetriever.RetrievePages<SectionInsightsPage>(
                    new RetrievePagesParameters
                    {
                        LinkedItemsMaxLevel = 5
                    },
                    query =>
                    {
                        query.Where(w =>
                            w.WhereEquals("ContentItemChannelID", _websiteChannelContext.WebsiteChannelID));
                    },
                    RetrievalCacheSettings.CacheDisabled,
                    cancellationToken);

        var insightSection = results.FirstOrDefault();
        var filterNames = string.IsNullOrWhiteSpace(insightSection?.InsightListFilters)
            ? new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            : insightSection.InsightListFilters
                .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var filteringEnabled = insightSection?.ItemListFilteringEnabled == true;
        var createFilters = _insightCardService.CreateFiltersAsync(
            filterNames,
            null,
            cancellationToken);

        var insightCardsGridDto = new CardsGridDto
        {
            Filters = filteringEnabled ? await createFilters : null,
            FilteringTooltipEnabled = await _filteringHelper.IsFilteringTooltipEnabledAsync(filteringEnabled, cancellationToken),
            PageSize = 20,
            ApiEndpointName = $"Insights/{page.SystemFields.WebPageItemGUID}",
            AdvancedSearchPrelinkText = insightSection?.ItemListAdvancedSearchDescription,
            AdvancedSearchLinkText = insightSection?.ItemListAdvancedSearchLinkText,
            AdvancedSearchLink = insightSection?.ItemListAdvancedSearchLinkUrl?.FirstOrDefault()?.ToString(),
        };

        return insightCardsGridDto;
    }
}
