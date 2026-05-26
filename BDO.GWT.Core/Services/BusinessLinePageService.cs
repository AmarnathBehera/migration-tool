using BDO.GWT.Core.Data;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.SchemaMarkups;
using BDO.GWT.Core.services;
using BDO.GWT.Core.Utilities;
using BDO.GWT.Core.ValueObjects;
using BDO.GWT.Web.Core.Services;
using CMS.Websites;
using Kentico.Content.Web.Mvc;

namespace BDO.GWT.Core.Services
{
    public interface IBusinessLinePageService
    {
        Task<BusinessLinePageDto> GetBusinessLinePageDtoAsync(CancellationToken cancellationToken = default);

        Task<ServiceSchemaMarkupDto> GetServicesPrimarySchemaMarkupDto(BusinessLinePageDto pageDto, CancellationToken cancellationToken = default);
    }

    public class BusinessLinePageService : IBusinessLinePageService
    {
        private readonly IHeroService _heroService;
        private readonly IContentService _contentService;
        private readonly IPageUtility _pageUtility;
        private readonly IWebPageUrlResolver _urlResolver;

        public BusinessLinePageService(IHeroService heroService,
            IContentService contentService, IPageUtility pageUtility,
            IWebPageUrlResolver urlResolver)
        {
            _heroService = heroService;
            _contentService = contentService;
            _pageUtility = pageUtility;
            _urlResolver = urlResolver;
        }

        public Task<ServiceSchemaMarkupDto> GetServicesPrimarySchemaMarkupDto(BusinessLinePageDto pageDto, CancellationToken cancellationToken = default)
        {
            var schemaMarkupDto = new ServiceSchemaMarkupDto
            {
                Name = _contentService.GetCurrentSiteLegalNameAsync(),
                AlternateName = "BDO",
                Url = pageDto.Url,
                Logo = _contentService.GetCurrentSiteLogoImageUrlAsync(),
                Description = pageDto.MetaData?.MetaDescription,
                DisambiguatingDescription = pageDto.MetaData?.MetaTitle,
            };

            return Task.FromResult(schemaMarkupDto);
        }

        public async Task<BusinessLinePageDto> GetBusinessLinePageDtoAsync(CancellationToken cancellationToken = default)
        {
            var currentPage = await _contentService.RetrieveCurrentPageAsync<BusinessLine>(
                new RetrieveCurrentPageParameters { LinkedItemsMaxLevel = 4 },
                cancellationToken);

            if (currentPage == null)
            {
                return new BusinessLinePageDto();
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
                           ? await _pageUtility.GetSideboxDtoAsync<ServiceArea, BusinessLine>(
                               currentPage.SideNavigationTitle,
                               currentPage.SystemFields.WebPageItemID,
                               currentPage.SystemFields.WebPageItemParentID,
                           serviceArea => serviceArea.MetadataTitle,
                               sibling => sibling.MetadataTitle,
                           cancellationToken)
                       : null;

            return new BusinessLinePageDto
            {
                Title = currentPage.ContentTitle,
                BodyContent = HtmlUtility.RemoveTildeFromUrlTypeHtmlAttributes(currentPage.ContentBody),
                Hero = await _heroService.GetHeroDtoAsync(heroData, cancellationToken),
                ShowSideNavigation = currentPage.ShowSideNavigation,
                Sidebox = sidebox,
                Url = _urlResolver.GetAbsoluteUrl(currentPage),
                MetaData = new DTOs.ReusableFieldsDTOs.MetaDataDto
                {
                    MetaTitle = currentPage.MetadataTitle,
                    MetaDescription = currentPage.MetadataDescription,
                }
            };
        }
    }
}