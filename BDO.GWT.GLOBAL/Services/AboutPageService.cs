using BDO.GWT.Core.Data;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.ValueObjects;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Web.Core.Services;

public interface IAboutPageService<in TPage> where TPage : class, IHero
{
    Task<AboutDto> GetAboutDtoAsync(TPage page, CancellationToken cancellationToken);
}

public class AboutPageService<TPage> : IAboutPageService<TPage>
    where TPage : class, IHero
{
    private readonly IHeroService _heroService;

    public AboutPageService(IHeroService heroService)
    {
        _heroService = heroService;
    }

    public async Task<AboutDto> GetAboutDtoAsync(TPage page, CancellationToken cancellationToken)
    {
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
