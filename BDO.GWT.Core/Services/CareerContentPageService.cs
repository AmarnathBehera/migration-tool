using BDO.GWT.Core;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.DTOs.Pages;
using BDO.GWT.Core.ValueObjects;
using CMS.Websites.Routing;
using Kentico.Content.Web.Mvc;

namespace BDO.GWT.Web.Core.Services;

public interface ICareerContentPageService
{

    Task<CareerDto> GetCareerContentDtoAsync(CancellationToken cancellationToken);

}


public class CareerContentPageService : ICareerContentPageService
{
    private readonly IHeroService _heroService;
    private readonly IContentService _contentRetriever;

    public CareerContentPageService(IHeroService heroService, IContentService contentService)
    {
        _heroService = heroService;
        _contentRetriever = contentService;

    }

    public async Task<CareerDto> GetCareerContentDtoAsync(CancellationToken cancellationToken)
    {
        var careercontentpage = await _contentRetriever.RetrieveCurrentPageAsync<CareerContent>(
            new RetrieveCurrentPageParameters()
            {

                LinkedItemsMaxLevel = 5
            }, cancellationToken);

        if (careercontentpage == null)
        {
            return new CareerDto();
        }


        var dto = new CareerDto()
        {
            Hero = await _heroService.GetHeroDtoAsync(new HeroData
            {
                Layout = careercontentpage.HeroLayout,
                Title = careercontentpage.HeroTitle,
                Description = careercontentpage.HeroDescription,
                BackgroundImage = careercontentpage.HeroBackgroundImage?.FirstOrDefault()?.ContentAttachmentAsset?.Url is string bgUrl ? Link.Parse(bgUrl) : null,
                BackgroundImageAltText = careercontentpage.HeroBackgroundImageAltText,
                MobileBackgroundImage = careercontentpage.HeroBackgroundImage_Mobile?.FirstOrDefault()?.ContentAttachmentAsset?.Url is string mobileUrl ? Link.Parse(mobileUrl) : null,
                MobileBackgroundImageAltText = careercontentpage.HeroBackgroundImage_MobileAltText,
                CallToActionButtonType = careercontentpage.HeroCallToActionButtonType,
                CallToActionButtonText = careercontentpage.HeroCallToActionButtonText,
                CallToActionButtonUrl = careercontentpage.HeroCallToActionButtonUrl?.FirstOrDefault()?.SystemFields?.WebPageUrlPath is string ctaPath ? Link.Parse(ctaPath) : null,
            }, cancellationToken),
        };

        return dto;


    }
}

