using BDO.GWT.Core;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.DTOs.Pages;
using BDO.GWT.Core.ValueObjects;
using Kentico.Content.Web.Mvc;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Web.Core.Services;

public interface ICareerPageService{

    Task<CareerDto> GetCareerDtoAsync(CancellationToken cancellationToken);

}


public class CareerPageService  : ICareerPageService
{
    private readonly IHeroService _heroService;
    private readonly IContentService _contentRetriever;

    public CareerPageService(IHeroService heroService, IContentService contentService) { 
        _heroService = heroService;
        _contentRetriever = contentService;

    
    }

    public async Task<CareerDto> GetCareerDtoAsync(CancellationToken cancellationToken)
    {
        var page = await _contentRetriever.RetrieveCurrentPageAsync<SectionCareers>(new RetrieveCurrentPageParameters { LinkedItemsMaxLevel = 4 }, cancellationToken);

        var careerpage = page;

        if (careerpage == null)
        {
            return new CareerDto();
        }
        var bgImageUrl = page?.HeroBackgroundImage?.FirstOrDefault()?.ContentAttachmentAsset?.Url;
        var mobileImageUrl = page?.HeroBackgroundImage_Mobile?.FirstOrDefault()?.ContentAttachmentAsset?.Url;
        var ctaUrlPath = page?.HeroCallToActionButtonUrl?.FirstOrDefault()?.SystemFields?.WebPageUrlPath;

        var heroData = new HeroData
        {
            Layout = page?.HeroLayout,
            Title = page?.HeroTitle,
            Description = page?.HeroDescription,
            BackgroundImage = bgImageUrl != null ? Link.Parse(bgImageUrl) : null,
            BackgroundImageAltText = page?.HeroBackgroundImageAltText,
            MobileBackgroundImage = mobileImageUrl != null ? Link.Parse(mobileImageUrl) : null,
            MobileBackgroundImageAltText = page?.HeroBackgroundImage_MobileAltText,
            CallToActionButtonType = page?.HeroCallToActionButtonType,
            CallToActionButtonText = page?.HeroCallToActionButtonText,
            CallToActionButtonUrl = ctaUrlPath != null ? Link.Parse(ctaUrlPath) : null,
        };


        var dto = new CareerDto()
        {
            Hero = await _heroService.GetHeroDtoAsync(heroData, cancellationToken),
        };

        return dto;


    }
}

