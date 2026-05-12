using BDO.GWT.Core;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.services;
using BDO.GWT.Core.ValueObjects;
using BDO.GWT.Web.Core.Services;
using CMS.Websites;
using Kentico.Content.Web.Mvc;

namespace BDO.GWT.Core.Services
{
    public interface IBusinessLinePageService
    {
        Task<BusinessLinePageDto> GetBusinessLinePageDtoAsync(CancellationToken cancellationToken = default);
    }

    public class BusinessLinePageService : IBusinessLinePageService
    {
        private readonly IHeroService _heroService;
        private readonly IContentService _contentService;
        private readonly IPageUtility _pageUtility;

        public BusinessLinePageService(IHeroService heroService,
            IContentService contentService, IPageUtility pageUtility)
        {
            _heroService = heroService;
            _contentService = contentService;
            _pageUtility = pageUtility;
        }

        public async Task<BusinessLinePageDto> GetBusinessLinePageDtoAsync(CancellationToken cancellationToken = default)
        {
            var page = await _contentService.RetrieveCurrentPageAsync<BusinessLine>(
                new RetrieveCurrentPageParameters { LinkedItemsMaxLevel = 4 },
                cancellationToken);

            if (page == null)
            {
                return new BusinessLinePageDto();
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
                       ? await _pageUtility.GetSideboxDtoAsync<ServiceArea>(
                           page.SideNavigationTitle,
                           page.SystemFields.WebPageItemID,
                           serviceArea => serviceArea.MetadataTitle,
                           cancellationToken)
                       : null;

            return new BusinessLinePageDto
            {
                Hero = await _heroService.GetHeroDtoAsync(heroData, cancellationToken),
                ShowSideNavigation = page.ShowSideNavigation,
                Sidebox = sidebox,
            };
        }
    }
}