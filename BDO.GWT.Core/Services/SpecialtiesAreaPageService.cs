using BDO.GWT.Core;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Pages;
using BDO.GWT.Core.services;
using BDO.GWT.Core.Utilities;
using BDO.GWT.Core.ValueObjects;
using BDO.GWT.Web.Core.Services;
using Kentico.Content.Web.Mvc;

namespace BDO.GWT.Core.Services;


public interface ISpecialtiesAreaPageService {
    public  Task<SpecialtiesAreaPageDto> GetSpecialtiesAreaPageDtoAsync(CancellationToken cancellationToken);
        }
public class SpecialtiesAreaPageService : ISpecialtiesAreaPageService
{
    private readonly IHeroService _heroService;
    private readonly IPageUtility _pageUtility;
    private readonly IContentService _contentService;

    public SpecialtiesAreaPageService(IHeroService heroService, IPageUtility pageUtility, IContentService contentService)
    {
        _heroService = heroService;
        _pageUtility = pageUtility;
        _contentService = contentService;
    }

    public async Task<SpecialtiesAreaPageDto> GetSpecialtiesAreaPageDtoAsync(CancellationToken cancellationToken)
    {
        var currentPage = await _contentService.RetrieveCurrentPageAsync<SpecialtiesArea>(
            new RetrieveCurrentPageParameters
            {
                LinkedItemsMaxLevel = 4
            }, cancellationToken);

        if (currentPage == null)
        {
            return new SpecialtiesAreaPageDto();
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



        var dto = new SpecialtiesAreaPageDto
        {
            Title = currentPage.ContentTitle,
            BodyContent = HtmlUtility.RemoveTildeFromUrlTypeHtmlAttributes(currentPage.ContentBody),
            Hero = await _heroService.GetHeroDtoAsync(heroData, cancellationToken),
            SideBox =  currentPage.ShowSideNavigation
          ? await _pageUtility.GetSideboxDtoAsync<SpecialtiesPage, SpecialtiesArea>(
              sideboxTitle: currentPage.SideNavigationTitle,
              parentWebPageItemId: currentPage.SystemFields.WebPageItemID,
              siblingParentWebPageItemId: currentPage.SystemFields.WebPageItemParentID,
              childTitleSelector: child => child.MetadataTitle,
              siblingTitleSelector: sibling => sibling.MetadataTitle,
                     cancellationToken)
                 : null
        };

        return dto;
    }
}
