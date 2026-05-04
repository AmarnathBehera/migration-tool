
using BDO.GWT.Core.Data;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Pages;
using BDO.GWT.Core.DTOs.SchemaMarkups;
using BDO.GWT.Core.Utilities;
using BDO.GWT.Core.ValueObjects;
using BDO.GWT.Web.Core.DTOs;
using BDO.GWT.Web.Core.Services;
using CMS.Websites;
using CMS.Websites.Routing;
using Kentico.Content.Web.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Core.Services;

public interface ILocationsPageService
{
    Task<LocationsDto> GetLocationsDtoAsync(Locations page, CancellationToken cancellationToken);
    Task<OrganizationSchemaMarkupDto> GetLocationsSchemaMarkupDto(Locations page, CancellationToken cancellationToken);
}

public class LocationsPageService : ILocationsPageService
{
    //private readonly IPageRetriever<LocationCity> _locationCityPageRetriever;
    //private readonly IPageUrlService _pageUrlService;
    private readonly IHeroService _heroService;
    private readonly IContentService _contentService;
    //private readonly ICurrentSiteHelper _currentSiteHelper;
    //private readonly IRetriever<Site> _siteRetriever;
    //private readonly ISiteContext _siteContext;

    public LocationsPageService(
        //IPageRetriever<LocationCity> locationCityPageRetriever,
        IHeroService heroService,
        IContentService contentService
        //IPageUrlService pageUrlService,
        //ICurrentSiteHelper currentSiteHelper,
        //IRetriever<Site> siteRetriever,
        //ISiteContext siteContext
        )
    {
        //_locationCityPageRetriever = locationCityPageRetriever;
        //_pageUrlService = pageUrlService;
        _heroService = heroService;
        _contentService = contentService;
        //_currentSiteHelper = currentSiteHelper;
        //_siteRetriever = siteRetriever;
        //_siteContext = siteContext;
    }

    public async Task<LocationsDto> GetLocationsDtoAsync(Locations page, CancellationToken cancellationToken)
    {
        var locationsDto = MapBasicFields(page);
        locationsDto.MainContent.Locations = await RetrieveLocationList(page, cancellationToken);
        var bgImageUrl = page.HeroBackgroundImage?.FirstOrDefault()?.ContentAttachmentAsset?.Url;
        var mobileImageUrl = page.HeroBackgroundImage_Mobile?.FirstOrDefault()?.ContentAttachmentAsset?.Url;
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
        };
        locationsDto.Hero = await _heroService.GetHeroDtoAsync(heroData, cancellationToken);
        return locationsDto;
    }

    public async Task<OrganizationSchemaMarkupDto> GetLocationsSchemaMarkupDto(Locations page, CancellationToken cancellationToken)
    {       
        return new OrganizationSchemaMarkupDto()
        {
            Url = page.GetUrl().AbsoluteUrl,
            Name = page.MetadataTitle,
            Description = page.MetadataDescription,
            Image = page.MetadataTeaserImage?.FirstOrDefault()?.ContentAttachmentAsset.Url,//ToHtmlCompatibleString(),
            Logo = _contentService.GetCurrentSiteLogoImageUrlAsync() ?? string.Empty,
            LegalName = _contentService.GetCurrentSiteLegalNameAsync() ?? string.Empty
        };
    }

    private async Task<IEnumerable<LocalLocationsDto>> RetrieveLocationList(Locations page, CancellationToken cancellationToken)
    {
        var pages = (await _contentService.GetPagesAsync<LocationCity>(
           new RetrievePagesParameters { },
           query => query
               .OrderBy(nameof(IWebPageContentQueryDataContainer.WebPageItemOrder))
               .Where(w => w.WhereEquals("WebPageItemParentID", page.SystemFields.WebPageItemID)),
           RetrievalCacheSettings.CacheDisabled,
           cancellationToken
       )).ToList();

        var locationsList = new List<LocalLocationsDto>
        {new LocalLocationsDto
        {
            Title = page.LocationsTitle,
            Items = pages
                .Select(v => new LinkItemDto()
                {
                    Title = v.MetadataTitle,
                    URL = v.GetUrl().AbsoluteUrl
                })
                .OrderBy(x => x.Title)
                .ToList()
        }
        };

        return locationsList;
    }

    private static LocationsDto MapBasicFields(Locations page)
    {
        return new LocationsDto
        {
            MainContent = new LocationsMainDto
            {
                Title = page.DescriptionTitle,
                Description = HtmlUtility.RemoveTildeFromUrlTypeHtmlAttributes(page.DescriptionBody),
                ListTitle = page.LocationsTitle,
            },
            Banner = new LocationsBannerDto
            {
                Description = HtmlUtility.RemoveTildeFromUrlTypeHtmlAttributes(page.BannerContent),
                BackgroundImage = page.BannerBackgroundImage?.FirstOrDefault()?.ContentAttachmentAsset.Url,//.ToHtmlCompatibleString(),
                Theme = page.BannerTheme,
                PrimaryButton = page.BannerButtonURL is not null && page.BannerButtonText is not null ? new ButtonDto
                {
                    Title = page.BannerButtonText,
                   // URL = page.BannerButtonURL.FirstOrDefault(),//ToHtmlCompatibleString(),
                    Target = page.BannerButtonLinkTarget,
                    Type = page.BannerButtonType,
                } : null,
            },
        };
    }
}
