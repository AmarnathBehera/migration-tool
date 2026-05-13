using BDO.GWT.Core.Data;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Pages;
using BDO.GWT.Core.ValueObjects;
using BDO.GWT.Web.Core.Services;
using Kentico.Content.Web.Mvc;
using System;
using System.Collections.Generic;
using System.Text;

namespace BDO.GWT.Core.Services
{
    public interface IIndustryServicePageService
    {
        Task<IndustryServiceDto> GetIndustryServiceDtoAsync(CancellationToken cancellationToken = default);
    }
    public class IndustryServicePageService : IIndustryServicePageService
    {
        private readonly IContentService _contentService;
        private readonly IHeroService _heroService;

        public IndustryServicePageService(IContentService contentService, IHeroService heroService)
        {
            _contentService = contentService;
            _heroService = heroService;
        }

        public async Task<IndustryServiceDto> GetIndustryServiceDtoAsync(CancellationToken cancellationToken = default)
        {
            var page = await _contentService.RetrieveCurrentPageAsync<IndustryService>(
                new RetrieveCurrentPageParameters { LinkedItemsMaxLevel = 4 },
                cancellationToken);

            if (page is null)
            {
                return new IndustryServiceDto();
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
            return new IndustryServiceDto
            {
                Title = page.DescriptionTitle,
                Description = page.DescriptionBody,
                Hero = await _heroService.GetHeroDtoAsync(heroData, cancellationToken),
                BaseContent = new BaseContent
                {
                    ContentItemID = page.SystemFields.ContentItemID,
                    ContentItemGUID = page.SystemFields.ContentItemGUID,
                    ContentItemName = page.SystemFields.ContentItemName,
                    ContentItemContentTypeID = page.SystemFields.ContentItemContentTypeID,
                    ContentItemIsSecured = page.SystemFields.ContentItemIsSecured,
                    ContentItemCommonDataContentLanguageID = page.SystemFields.ContentItemCommonDataContentLanguageID,
                    ContentItemCommonDataFirstPublishedWhen = page.SystemFields.ContentItemCommonDataFirstPublishedWhen,
                    ContentItemCommonDataLastPublishedWhen = page.SystemFields.ContentItemCommonDataLastPublishedWhen,

                },
                BaseWebPage = new BaseWebPage
                {
                    WebPageItemGUID = page.SystemFields.WebPageItemGUID,
                    WebPageItemID = page.SystemFields.WebPageItemID,
                    WebPageItemName = page.SystemFields.WebPageItemName,
                    WebPageItemWebsiteChannelID = page.SystemFields.WebPageItemWebsiteChannelId,
                    WebPageUrlPath = page.SystemFields.WebPageUrlPath,
                    WebPageItemOrder = page.SystemFields.WebPageItemOrder,
                    WebPageItemTreePath = page.SystemFields.WebPageItemTreePath,
                    WebPageUrlPathContentLanguageID = page.SystemFields.WebPageUrlPathContentLanguageID,
                    WebPageItemParentID = page.SystemFields.WebPageItemParentID
                },

            };
        }
    }
}
