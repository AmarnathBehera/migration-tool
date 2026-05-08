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
                Hero = _heroService.GetHeroDtoAsync(heroData, cancellationToken).GetAwaiter().GetResult(),
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
}
