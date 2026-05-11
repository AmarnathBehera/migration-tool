using BDO.GWT.Core;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.ValueObjects;
using BDO.GWT.Web.Core.Services;
using CMS.Websites;
using Kentico.Content.Web.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Core.Services
{
    public interface ISubServicesPageService
    {
        Task<SubServicePageDto> GetSubServicesPageDtoAsync(CancellationToken cancellationToken);
    }
    public class SubServicesPageService : ISubServicesPageService
    {
        private readonly IHeroService _heroService;
        private readonly IContentService _contentService;
        public SubServicesPageService(IHeroService heroService, IContentService contentService) 
        { 
            _heroService = heroService;
            _contentService = contentService;
        } 
        public async Task<SubServicePageDto> GetSubServicesPageDtoAsync(CancellationToken cancellationToken)
        {
            var page = await _contentService.RetrieveCurrentPageAsync<Sub_Service>(
           new RetrieveCurrentPageParameters { LinkedItemsMaxLevel = 4 },
           cancellationToken);

            if (page == null)
            {
                return new SubServicePageDto();
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

            return new SubServicePageDto
            {
                Hero = await _heroService.GetHeroDtoAsync(heroData, cancellationToken)
                
            };
        }
    }
}
