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
    public interface IServiceAreaAndServicePageService
    {
        Task<ServicesAreaPageDto> GetServiceAreaPageDtoAsync(CancellationToken cancellationToken = default);
        Task<ServicePageDto> GetServicePageDtoAsync(CancellationToken cancellationToken = default);
    }
    public class ServiceAreaAndServicePageService : IServiceAreaAndServicePageService
    {
        private readonly IHeroService _heroService;
        private readonly IContentService _contentService;
        private readonly IPageUtility _pageUtility;

        public ServiceAreaAndServicePageService(IHeroService heroService,
        IContentService contentService, IPageUtility pageUtility)
        {
            _heroService = heroService;
            _contentService = contentService;
            _pageUtility = pageUtility;
        }
        public async Task<ServicesAreaPageDto> GetServiceAreaPageDtoAsync(CancellationToken cancellationToken = default)
        {
            var page = await _contentService.RetrieveCurrentPageAsync<ServiceArea>(
                new RetrieveCurrentPageParameters { LinkedItemsMaxLevel = 4 },
                cancellationToken);

            if (page == null)
            {
                return new ServicesAreaPageDto();
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
                       ? await _pageUtility.GetSideboxDtoAsync<Service>(
                           page.SideNavigationTitle,
                           page.SystemFields.WebPageItemID,
                           service => service.MetadataTitle,
                           cancellationToken)
                       : null;

            return new ServicesAreaPageDto
            {
                Hero = await _heroService.GetHeroDtoAsync(heroData, cancellationToken),
                Title = page.ContentTitle,
                Description = page.ContentBody,
                ShowSideNavigation = page.ShowSideNavigation,
                Sidebox = sidebox,
            };
        }

        public async Task<ServicePageDto> GetServicePageDtoAsync(CancellationToken cancellationToken = default)
        {
            var page = await _contentService.RetrieveCurrentPageAsync<Service>(
                new RetrieveCurrentPageParameters { LinkedItemsMaxLevel = 4 },
                cancellationToken);

            if (page == null)
            {
                return new ServicePageDto();
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
                       ? await _pageUtility.GetSideboxDtoAsync<Sub_Service>(
                           page.SideNavigationTitle,
                           page.SystemFields.WebPageItemID,
                           service => service.MetadataTitle,
                           cancellationToken)
                       : null;

            return new ServicePageDto
            {
                Hero = await _heroService.GetHeroDtoAsync(heroData, cancellationToken),
                ShowSideNavigation = page.ShowSideNavigation,
                Sidebox = sidebox,
            };
        }
    }


}
