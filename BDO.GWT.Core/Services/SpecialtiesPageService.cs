using BDO;
using BDO.GWT.Core;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Pages;
using BDO.GWT.Core.services;
using BDO.GWT.Core.Services;
using BDO.GWT.Core.ValueObjects;
using BDO.GWT.Web.Core.Services;
using Kentico.Content.Web.Mvc;
using System;
using System.Collections.Generic;
using System.Text;

namespace BDO.GWT.Core.Services
{
    public interface ISpecialtiesPageService{
        Task<SpecialtiesPageDto> GetSpecialtiesPageDtoAsync( CancellationToken cancellationToken);
    }

}
    public class SpecialtiesPageService: ISpecialtiesPageService
    {
        private readonly IHeroService _heroService;
        private readonly IPageUtility _pageUtility;
        private readonly IContentService _contentService;
        public SpecialtiesPageService(IHeroService heroService, IPageUtility pageUtility, IContentService contentService)
        {
            _heroService = heroService;
            _pageUtility = pageUtility;
            _contentService = contentService;
        }





        public async Task<SpecialtiesPageDto> GetSpecialtiesPageDtoAsync(CancellationToken cancellationToken)
        {
            var currentPage = await _contentService.RetrieveCurrentPageAsync<SpecialtiesPage>(
            new RetrieveCurrentPageParameters
            {
                LinkedItemsMaxLevel = 4
            }, cancellationToken);

            if (currentPage == null)
            {
                return new SpecialtiesPageDto();
            }

            var bgImageUrl = currentPage.HeroBackgroundImage?.FirstOrDefault()?.ContentAttachmentAsset?.Url;
            var mobileImageUrl = currentPage.HeroBackgroundImage_Mobile?.FirstOrDefault()?.ContentAttachmentAsset?.Url;
            var ctaUrlPath = currentPage.HeroCallToActionButtonUrl?.FirstOrDefault()?.SystemFields?.WebPageUrlPath;

            var heroData = new HeroData
            {
                Layout = currentPage.HeroLayout,
                Title = currentPage.HeroTitle,
                Description = currentPage.HeroDescription,
                BackgroundImage = bgImageUrl != null ? Link.Parse(bgImageUrl) : null,
                BackgroundImageAltText = currentPage.HeroBackgroundImageAltText,
                MobileBackgroundImage = mobileImageUrl != null ? Link.Parse(mobileImageUrl) : null,
                MobileBackgroundImageAltText = currentPage.HeroBackgroundImage_MobileAltText,
                CallToActionButtonType = currentPage.HeroCallToActionButtonType,
                CallToActionButtonText = currentPage.HeroCallToActionButtonText,
                CallToActionButtonUrl = ctaUrlPath != null ? Link.Parse(ctaUrlPath) : null,
            };

            SideBoxDto? sidebox = currentPage.ShowSideNavigation
          ? await _pageUtility.GetSideboxDtoAsync<SpecialtiesPage>(
              sideboxTitle: currentPage.SideNavigationTitle,
              parentWebPageItemId: currentPage.SystemFields.WebPageItemID,       
              siblingParentWebPageItemId: currentPage.SystemFields.WebPageItemParentID, 
              siblingTitleSelector: sibling => sibling.MetadataTitle,
                     cancellationToken)
                 : null;

            return new SpecialtiesPageDto
            {
                Hero = await _heroService.GetHeroDtoAsync(heroData, cancellationToken),
                SideBox = sidebox,
            };

            
        }
    
}
