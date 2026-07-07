using BDO.GWT.Common.Exceptions;
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
public interface IInsightSectionCardListingPageService
{
    Task<InsightCardListingDto> GetContentSectionDtoAsync(SectionInsightsPage page, CancellationToken cancellationToken);

    Task<PaginationDto<ContentPageCardDto>> GetContentPaginationResultAsync(Guid pageGuid, CardsQuery cardsQuery, CancellationToken cancellationToken);
}

public class InsightSectionCardListingPageService : IInsightSectionCardListingPageService

{
    private readonly IFilteringHelper _filteringHelper;
    private readonly IHeroService _heroService;
    private readonly IContentRetriever _contentRetriever;
    private readonly IWebsiteChannelContext _websiteChannelContext;
    private readonly IInsightCardService _insightCardService;

    public InsightSectionCardListingPageService(
        IFilteringHelper filteringHelper,
        IHeroService heroService,
        IContentRetriever contentRetriever,
        IInsightCardService insightCardService,
        IWebsiteChannelContext websiteChannelContext)
    {
        _filteringHelper = filteringHelper;
        _heroService = heroService;
        _contentRetriever = contentRetriever;
        _websiteChannelContext = websiteChannelContext;
        _insightCardService = insightCardService;
    }

    public async Task<InsightCardListingDto> GetContentSectionDtoAsync(SectionInsightsPage page, CancellationToken cancellationToken)
    {
        var insightPage = await _contentRetriever.RetrieveCurrentPage<SectionInsightsPage>(
            new RetrieveCurrentPageParameters { LinkedItemsMaxLevel = 4 },
            cancellationToken);

        if (insightPage is null)
        {
            throw new ContentItemNotFoundException($"Current page of type '{nameof(SectionInsightsPage)}' was not found.");
        }

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
        var page = (await _contentRetriever.RetrieveAllPagesByGuids<IWebPageFieldsSource>(
            new[] { pageGuid },
            cancellationToken))
            .FirstOrDefault();

        if (page is null)
        {
            throw new ContentItemNotFoundException($"SectionInsightsPage page not found for guid '{pageGuid}'.");
        }

        var treePath = page.SystemFields.WebPageItemTreePath;
        Action<RetrievePagesQueryParameters>? decorateQuery = string.IsNullOrWhiteSpace(treePath)
            ? null
            : q => q.Where(w => w.WhereStartsWith(nameof(WebPageFields.WebPageItemTreePath), $"{treePath}/"));

        var featuredContentPageIds = await GetFeaturedContentPageIdsAsync(cancellationToken);

        return await _insightCardService.GetFilteredItemsAsync(
            cardsQuery,
            treePath,
            decorateQuery,
            contentPage => featuredContentPageIds.Contains(contentPage.SystemFields.WebPageItemID),
            cancellationToken);
    }

    private async Task<SectionInsightsPage> GetInsightSectionPage(SectionInsightsPage? currentPage, CancellationToken cancellationToken)
    {
        if (currentPage is not null)
        {
            return currentPage;
        }

        var insightSection = await _contentRetriever.RetrievePages<SectionInsightsPage>(
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

        var insightSectionPage = insightSection.FirstOrDefault();
        if (insightSectionPage is null)
        {
            throw new MissingSectionPageException($"No pages have been found of type '{nameof(SectionInsightsPage)}'.");
        }

        return insightSectionPage;
    }

    private async Task<ISet<int>> GetFeaturedContentPageIdsAsync(CancellationToken cancellationToken)
    {
        var insightSection = await GetInsightSectionPage(null, cancellationToken);

        return insightSection.InsightListFeaturedItems?
            .Select(x => x.SystemFields.WebPageItemID)
            .Where(id => id > 0)
            .ToHashSet()
            ?? new HashSet<int>();
    }

    private async Task<CardsGridDto> GetCardsGridDtoAsync(SectionInsightsPage page, CancellationToken cancellationToken)
    {
        var insightSection = await GetInsightSectionPage(page, cancellationToken);
        var filterNames = string.IsNullOrWhiteSpace(insightSection?.InsightListFilters)
        ? new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        : insightSection.InsightListFilters
        .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var createFilters = _insightCardService.CreateFiltersAsync(
                   filterNames,
                   null,
                   cancellationToken);
        var insightCardsGridDto = new CardsGridDto
        {
            Title = insightSection.ItemListTitle,
            Description = insightSection.ItemListDescription,
            Filters = insightSection.ItemListFilteringEnabled ? await createFilters : null,
            FilteringTooltipEnabled = await _filteringHelper.IsFilteringTooltipEnabledAsync(insightSection.ItemListFilteringEnabled, cancellationToken),
            PageSize = 20,
            ApiEndpointName = $"Insights/{page.SystemFields.WebPageItemGUID}",
            AdvancedSearchPrelinkText = insightSection.ItemListAdvancedSearchDescription,
            AdvancedSearchLinkText = insightSection.ItemListAdvancedSearchLinkText,
            AdvancedSearchLink = insightSection.ItemListAdvancedSearchLinkUrl?.FirstOrDefault()?.ToString(),
        };

        return insightCardsGridDto;
    }
}
