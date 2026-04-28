using BDO.GWT.Core.Data;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Pages;
using BDO.GWT.Core.DTOs.SchemaMarkups;
using BDO.GWT.Core.Utilities;
using BDO.GWT.Core.ValueObjects;
using BDO.GWT.Web.Core.Services;
using CMS.Websites;
using Kentico.Content.Web.Mvc;
using System.Collections.Generic;
using System.Linq;  
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Core.Services;

public interface ILocationCityPageService
{
    Task<LocationCityDto> GetLocationCityDtoAsync(LocationCity page, CancellationToken cancellationToken);
    Task<FinancialServiceSchemaMarkupDto> GetLocationCitySchemaMarkupDto(LocationCity page, CancellationToken cancellationToken);
}

public class LocationCityPageService : ILocationCityPageService
{
    private readonly IHeroService _heroService;
    private readonly IContentService _contentService;
    //private readonly IPageRetriever<LocationOffice> _locationOfficePageRetriever;
    //private readonly ICurrentSiteHelper _currentSiteHelper;
    //private readonly IPageUrlService _pageUrlService;
    //private readonly IRetriever<Site> _siteRetriever;
    //private readonly ISiteContext _siteContext;
    private readonly IPageContext _pageContext;

    public LocationCityPageService(
        IHeroService heroService,
        IContentService contentService,
        //IPageRetriever<LocationOffice> locationOfficePageRetriever,
        //ICurrentSiteHelper currentSiteHelper,
        //IPageUrlService pageUrlService,
        //IRetriever<Site> siteRetriever,
        //ISiteContext siteContext,
        IPageContext pageContext)
    {
        //_locationOfficePageRetriever = locationOfficePageRetriever;
        _heroService = heroService;
        _contentService = contentService;
        //_currentSiteHelper = currentSiteHelper;
        //_pageUrlService = pageUrlService;
        //_siteRetriever = siteRetriever;
        //_siteContext = siteContext;
        _pageContext = pageContext;
    }

    public async Task<LocationCityDto> GetLocationCityDtoAsync(LocationCity page, CancellationToken cancellationToken)
    {
        var offices = await RetrieveOfficesAsync(page, cancellationToken);
        var locationCityDto = MapBasicFields(page);
        locationCityDto.MainContent.OfficeCards = ToOfficeCards(offices, page);
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
        locationCityDto.Hero = await _heroService.GetHeroDtoAsync(heroData, cancellationToken);
        return locationCityDto;
    }

    public async Task<FinancialServiceSchemaMarkupDto> GetLocationCitySchemaMarkupDto(LocationCity page, CancellationToken cancellationToken)
    {
        var offices = await RetrieveOfficesAsync(page, cancellationToken);

        var office = offices.FirstOrDefault();

        var name = _contentService.GetCurrentSiteLegalNameAsync();
        var logo = _contentService.GetCurrentSiteLogoImageUrlAsync();

        return new FinancialServiceSchemaMarkupDto
        {
            Url = page.GetUrl().AbsoluteUrl,
            //Name = page.RegionName,
            Description = _contentService.GetFallbackMetadataDescription(page.MetadataDescription, page.MetadataOGDescription),
            Image = page.MetadataTeaserImage?.FirstOrDefault()?.ContentAttachmentAsset.Url,//.ToHtmlCompatibleString(),
            Logo = logo,
            LegalName = name,
            Longitude = office?.LocationLongitude,
            Latitude = office?.LocationLatitude,
            Brand = new OrganizationSchemaMarkupDto
            {
                Name = name,
                Logo = logo,
            },
            Address = new PostalAddressSchemaMarkupDto
            {
                StreetAddress = office != null ? $"{office.LocationAddressLine1} {office.LocationAddressLine2}".Trim() : null,
                AddressLocality = page.MetadataTitle,
                PostalCode = office?.LocationZip,
                AddressCountry = office?.LocationCountry, 
                //AddressRegion = page?.RegionName,
            },
        };
    }

    private async Task<IEnumerable<LocationOffice>> RetrieveOfficesAsync(LocationCity page, CancellationToken cancellationToken)
    {
        var offices = (await _contentService.GetPagesAsync<LocationOffice>(
           new RetrievePagesParameters { },
           query => query
               .OrderBy(nameof(IWebPageContentQueryDataContainer.WebPageItemOrder))
               .Where(w => w.WhereEquals("WebPageItemParentID", page.SystemFields.WebPageItemID)),
           RetrievalCacheSettings.CacheDisabled,
           cancellationToken
       ));

        return offices;
    }

    private static IEnumerable<OfficeCardDto> ToOfficeCards(IEnumerable<LocationOffice> offices, LocationCity page)
    {
        var officeCards = offices?.Select(x => new OfficeCardDto
        {
            ImageURL = x.OfficeImage?.FirstOrDefault()?.ContentAttachmentAsset?.Url,//ToHtmlCompatibleString(),
            ImageAltText = x.OfficeImageAltText,
            Name = x.MetadataTitle,
            AddressLine1 = x.LocationAddressLine1,
            AddressLine2 = x.LocationAddressLine2,
            City = page.MetadataTitle,
            PostalCode = x.LocationZip,
            PostalCodeDisplayType = x.LocationZipDisplayType,
            Country = x.LocationCountry,
            Email = x.LocationEmail,
            PhoneNumber = x.LocationPhone,
            Fax = x.LocationFax,
            OpeningHours = x.OpeningHours,
           // GetDirectionsUrl = GoogleMapsHelper.GenerateGoogleMapsLink(x.LocationLatitude, x.LocationLongitude),
            PoBoxNumber = x.LocationPOBoxNumber,
            PoCity = x.LocationPOCity,
            PoPostalCode = x.LocationPOZip,
            PoPostalCodeDisplayType = x.LocationPOZipDisplayType,
            PoCountry = x.LocationPOCountry,
        });

        return officeCards ?? new List<OfficeCardDto>();
    }

    private static LocationCityDto MapBasicFields(LocationCity page)
    {
        return new LocationCityDto
        {
            MainContent = new LocationCityMainDto
            {
                Title = page.ContentTitle,
                Content = HtmlUtility.RemoveTildeFromUrlTypeHtmlAttributes(page.MetadataDescription),
                SectionTitle = page.SectionTitle,
            },
        };
    }
}
