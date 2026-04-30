using BDO.GWT.Core;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.ValueObjects;
using Kentico.Content.Web.Mvc;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Web.Core.Services;

public interface ISectionAboutPageService
{
    public Task<AboutDto> GetAboutDtoAsync(CancellationToken cancellationToken);
}

public class SectionAboutPageService : ISectionAboutPageService   
{  
    private readonly IHeroService _heroService;    
    private readonly IContentService _contentService; 
    public SectionAboutPageService(IHeroService heroService, IContentService contentService)
    {
        _heroService = heroService;       
        _contentService = contentService;       
    }

    public async Task<AboutDto> GetAboutDtoAsync(CancellationToken cancellationToken)
    {
        var page = await _contentService.RetrieveCurrentPageAsync<AboutPage>(
            new RetrieveCurrentPageParameters { LinkedItemsMaxLevel = 4 }, cancellationToken);


        if (page == null)
        {
            return new AboutDto();
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

        return new AboutDto
        {
            Hero = await _heroService.GetHeroDtoAsync(heroData, cancellationToken)
        };
    }
}
