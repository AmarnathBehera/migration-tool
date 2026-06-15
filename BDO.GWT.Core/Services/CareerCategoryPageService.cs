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
    private readonly IWebsiteChannelContext _websiteChannelContext;
    public CareerCategoryPageService(IHeroService heroService, IContentService contentService, IWebsiteChannelContext websiteChannelContext)
    {
        _heroService = heroService;
        _contentRetriever = contentService;
        _websiteChannelContext = websiteChannelContext;

    }

    public async Task<CareerCategoryDto> GetCareerCategoryDtoAsync(CancellationToken cancellationToken)
    {
        var pages = await _contentRetriever.GetPagesAsync<CareerCategory>(
            new RetrievePagesParameters
            {
                LinkedItemsMaxLevel = 5
            },
            query =>
            {
                query.Where(w =>
                    w.WhereEquals("ContentItemChannelID", _websiteChannelContext.WebsiteChannelID));
            },
            RetrievalCacheSettings.CacheDisabled,
            cancellationToken);

        var careercategorypage = pages?.FirstOrDefault();

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

