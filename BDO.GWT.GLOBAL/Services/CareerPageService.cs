using BDO.GWT.Core;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.ValueObjects;
using BDO.GWT.Web.Core.DTOs;
using CMS.Websites.Routing;
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
    private readonly IWebsiteChannelContext _websiteChannelContext;
    public CareerPageService(IHeroService heroService, IContentService contentService, IWebsiteChannelContext websiteChannelContext) { 
        _heroService = heroService;
        _contentRetriever = contentService;
        _websiteChannelContext = websiteChannelContext;
    
    }

    public async Task<CareerDto> GetCareerDtoAsync(CancellationToken cancellationToken)
    {
        var pages = await _contentRetriever.GetPagesAsync<SectionCareers>(
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

        var careerpage = pages?.FirstOrDefault();

        if (careerpage == null)
        {
            return new CareerDto();
        }
        

        var dto = new CareerDto()
        {
            Hero = await _heroService.GetHeroDtoAsync(new HeroData
            {
                Layout = careerpage.HeroLayout,
                Title = careerpage.HeroTitle,
                Description = careerpage.HeroDescription,
                BackgroundImage = careerpage.HeroBackgroundImage?.FirstOrDefault()?.ContentAttachmentAsset?.Url is string bgUrl ? Link.Parse(bgUrl) : null,
                BackgroundImageAltText = careerpage.HeroBackgroundImageAltText,
                MobileBackgroundImage = careerpage.HeroBackgroundImage_Mobile?.FirstOrDefault()?.ContentAttachmentAsset?.Url is string mobileUrl ? Link.Parse(mobileUrl) : null,
                MobileBackgroundImageAltText = careerpage.HeroBackgroundImage_MobileAltText,
                CallToActionButtonType = careerpage.HeroCallToActionButtonType,
                CallToActionButtonText = careerpage.HeroCallToActionButtonText,
                CallToActionButtonUrl = careerpage.HeroCallToActionButtonUrl?.FirstOrDefault()?.SystemFields?.WebPageUrlPath is string ctaPath ? Link.Parse(ctaPath) : null,
            }, cancellationToken),
        };

        return dto;


    }
}

