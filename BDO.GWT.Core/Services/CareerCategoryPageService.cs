using BDO.GWT.Core;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.DTOs.Pages;
using BDO.GWT.Core.ValueObjects;
using CMS.Websites.Routing;
using Kentico.Content.Web.Mvc;

namespace BDO.GWT.Web.Core.Services;

public interface ICareerCategoryPageService
{

    Task<CareerCategoryDto> GetCareerCategoryDtoAsync(CancellationToken cancellationToken);

}


public class CareerCategoryPageService : ICareerCategoryPageService
{
    private readonly IHeroService _heroService;
    private readonly IContentService _contentRetriever;
    public CareerCategoryPageService(IHeroService heroService, IContentService contentService)
    {
        _heroService = heroService;
        _contentRetriever = contentService;

    }

    public async Task<CareerCategoryDto> GetCareerCategoryDtoAsync(CancellationToken cancellationToken)
    {
        var careercategorypage = await _contentRetriever.RetrieveCurrentPageAsync<CareerContent>(
            new RetrieveCurrentPageParameters()
            {

                LinkedItemsMaxLevel = 3
            }, cancellationToken);

        if (careercategorypage == null)
        {
            return new CareerCategoryDto();
        }


        var dto = new CareerCategoryDto()
        {
            Hero = await _heroService.GetHeroDtoAsync(new HeroData
            {
                Layout = careercategorypage.HeroLayout,
                Title = careercategorypage.HeroTitle,
                Description = careercategorypage.HeroDescription,
                BackgroundImage = careercategorypage.HeroBackgroundImage?.FirstOrDefault()?.ContentAttachmentAsset?.Url is string bgUrl ? Link.Parse(bgUrl) : null,
                BackgroundImageAltText = careercategorypage.HeroBackgroundImageAltText,
                MobileBackgroundImage = careercategorypage.HeroBackgroundImage_Mobile?.FirstOrDefault()?.ContentAttachmentAsset?.Url is string mobileUrl ? Link.Parse(mobileUrl) : null,
                MobileBackgroundImageAltText = careercategorypage.HeroBackgroundImage_MobileAltText,
                CallToActionButtonType = careercategorypage.HeroCallToActionButtonType,
                CallToActionButtonText = careercategorypage.HeroCallToActionButtonText,
                CallToActionButtonUrl = careercategorypage.HeroCallToActionButtonUrl?.FirstOrDefault()?.SystemFields?.WebPageUrlPath is string ctaPath ? Link.Parse(ctaPath) : null,
            }, cancellationToken),
        };

        return dto;


    }
}

