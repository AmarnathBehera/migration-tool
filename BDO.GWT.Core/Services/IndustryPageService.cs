using BDO.GWT.Core;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.services;
using BDO.GWT.Core.ValueObjects;
using CMS.Websites;
using Kentico.Content.Web.Mvc;

namespace BDO.GWT.Web.Core.Services;

public interface IIndustryPageService
{
    Task<IndustryDto> GetIndustryPageDtoAsync(CancellationToken cancellationToken = default);
}

public class IndustryPageService : IIndustryPageService
{
    private readonly IHeroService _heroService;
    private readonly IContentService _contentService;
    private readonly IPageUtility _pageUtility;

    public IndustryPageService(
        IHeroService heroService,
        IContentService contentService,
        IPageUtility pageUtility)
    {
        _heroService = heroService;
        _contentService = contentService;
        _pageUtility = pageUtility; 
    }

    public async Task<IndustryDto> GetIndustryPageDtoAsync(
        CancellationToken cancellationToken = default)
    {
        var page = await _contentService.RetrieveCurrentPageAsync<Industry>(
            new RetrieveCurrentPageParameters { LinkedItemsMaxLevel = 4 },
            cancellationToken);

        if (page == null)
        {
            return new IndustryDto();
        }

        var bgImageUrl = page.HeroBackgroundImage?.FirstOrDefault()?.ContentAttachmentAsset?.Url;
        var mobileImageUrl = page.HeroBackgroundImage_Mobile?.FirstOrDefault()?.ContentAttachmentAsset?.Url;
        var ctaUrlPath = page.HeroCallToActionButtonUrl?.FirstOrDefault()?.SystemFields?.WebPageUrlPath;

        var heroData = new HeroData
        {
            Layout = page.HeroLayout,
            Title = page.HeroTitle,
            Description = page.HeroDescription,
            BackgroundImage = bgImageUrl != null ? Link.Parse(bgImageUrl) : null,
            BackgroundImageAltText = page.HeroBackgroundImageAltText,
            MobileBackgroundImage = mobileImageUrl != null ? Link.Parse(mobileImageUrl) : null,
            MobileBackgroundImageAltText = page.HeroBackgroundImage_MobileAltText,
            CallToActionButtonType = page.HeroCallToActionButtonType,
            CallToActionButtonText = page.HeroCallToActionButtonText,
            CallToActionButtonUrl = ctaUrlPath != null ? Link.Parse(ctaUrlPath) : null,
        };

        SideBoxDto? sidebox = page.ShowSideNavigation
                ? await _pageUtility.GetSideboxDtoAsync<IndustryService>(
                    page.SideboxTitle,
                    page.SystemFields.WebPageItemID,
                    industryServices => industryServices.MetadataTitle,
                    cancellationToken)
                : null;

        return new IndustryDto
        {
            Hero = await _heroService.GetHeroDtoAsync(heroData, cancellationToken),
            Title = page.DescriptionTitle,
            Description = page.DescriptionBody,
            Sidebox = sidebox,
            BaseContent = new BaseContent
            {
                ContentItemID = page?.SystemFields?.ContentItemID ?? 0,
                ContentItemGUID = page?.SystemFields?.ContentItemGUID ?? Guid.Empty,
                ContentItemName = page?.SystemFields?.ContentItemName,
                ContentItemContentTypeID = page?.SystemFields?.ContentItemContentTypeID ?? 0,
                ContentItemIsSecured = page?.SystemFields?.ContentItemIsSecured ?? false,
                ContentItemCommonDataContentLanguageID = page?.SystemFields?.ContentItemCommonDataContentLanguageID ?? 0,
                ContentItemCommonDataFirstPublishedWhen = page?.SystemFields?.ContentItemCommonDataFirstPublishedWhen ?? default,
                ContentItemCommonDataLastPublishedWhen = page?.SystemFields?.ContentItemCommonDataLastPublishedWhen ?? default,

            },
            BaseWebPage = new BaseWebPage
            {
                WebPageItemGUID = page?.SystemFields?.WebPageItemGUID ?? Guid.Empty,
                WebPageItemID = page?.SystemFields?.WebPageItemID ?? 0,
                WebPageItemName = page?.SystemFields?.WebPageItemName,
                WebPageItemWebsiteChannelID = page?.SystemFields?.WebPageItemWebsiteChannelId ?? 0,
                WebPageUrlPath = page?.SystemFields?.WebPageUrlPath,
                WebPageItemOrder = page?.SystemFields?.WebPageItemOrder ?? 0,
                WebPageItemTreePath = page?.SystemFields?.WebPageItemTreePath,
                WebPageUrlPathContentLanguageID = page?.SystemFields?.WebPageUrlPathContentLanguageID ?? 0,
                WebPageItemParentID = page?.SystemFields?.WebPageItemParentID ?? 0
            },
        };
    }
}