using BDO.GWT.Core.Data;
using BDO.GWT.Core.DTOs.Pages;
using BDO.GWT.Core.ValueObjects;
using BDO.GWT.Web.Core.Services;

namespace BDO.GWT.Core.Services;

public interface IBusinessStrategyService
{
    Task<BusinessStrategyDto> GetBusinessStrategyDtoAsync(BusinessStrategy page, CancellationToken cancellationToken);
}

public class BusinessStrategyService : IBusinessStrategyService
{
    private readonly IHeroService _heroService;

    public BusinessStrategyService(IHeroService heroService)
    {
        _heroService = heroService;
    }

    public async Task<BusinessStrategyDto> GetBusinessStrategyDtoAsync(BusinessStrategy page, CancellationToken cancellationToken)
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
        var businessStrategy =  new BusinessStrategyDto
        {
            Hero = await _heroService.GetHeroDtoAsync(heroData, cancellationToken)
        };

        return businessStrategy;
    }
}