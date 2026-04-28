using BDO.GWT.Core.Data;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Pages;
using BDO.GWT.Core.DTOs.SchemaMarkups;
using BDO.GWT.Core.Utilities;
using CMS.Core;
using CMS.Websites;
using CMS.Websites.Routing;
using Kentico.Content.Web.Mvc;

namespace BDO.GWT.Core.Services;

public interface IPersonPageService
{
    Task<PersonDto> GetPersonPageDtoAsync(Person page, CancellationToken cancellationToken = default);

    Task<VCardData?> GeneratePersonVCardAsync(Guid personGuid, string? source = null, CancellationToken cancellationToken = default);

    Task<PersonSchemaMarkupDto> GetPersonSchemaMarkupDto(Person person, CancellationToken cancellationToken);

}

public class PersonPageService : IPersonPageService
{
    private readonly IHttpClientFactory _httpClientFactory;
    //private readonly IAuthoredContentService _authoredContentService;
    //private readonly IPageRetriever<IndustryCategory> _industryCategoryPageRetriever;
    //private readonly IPageRetriever<LocationCity> _locationCityPageRetriever;
    //private readonly IPageRetriever<LocationOffice> _locationOfficePageRetriever;
    //private readonly IPageRetriever<Person> _personPageRetriever;
    //private readonly IPageRetriever<ServiceArea> _serviceAreaPageRetriever;
    //private readonly IPageRetriever<SectionPeoplePage> _sectionPeoplePageRetriever;
    //private readonly IPageRetriever<SpecialtiesCategory> _specialtiesCategoryPageRetriever;
    //private readonly IPageUrlRetriever _pageUrlRetriever;
    //private readonly IRetriever<Credential> _credentialsRetriever;
    //private readonly IRetriever<Site> _siteRetriever;
    //private readonly ISettingsService _settingsService;
    private readonly IPageContext _pageContext;
    private readonly IContentService _contentService;
    //private readonly ICurrentSiteHelper _currentSiteHelper;

    private HttpClient? _httpClient;

    public PersonPageService(
        //IAuthoredContentService authoredContentService,
        IHttpClientFactory httpClientFactory,
        //IPageRetriever<IndustryCategory> industryCategoryPageRetriever,
        //IPageRetriever<LocationCity> locationCityPageRetriever,
        //IPageRetriever<LocationOffice> locationOfficePageRetriever,
        //IPageRetriever<Person> personPageRetriever,
        //IPageRetriever<ServiceArea> serviceAreaPageRetriever,
        //IPageRetriever<SectionPeoplePage> sectionPeoplePageRetriever,
        //IPageRetriever<SpecialtiesCategory> specialtiesCategoryPageRetriever,
        //IPageUrlRetriever pageUrlRetriever,
        //IRetriever<Credential> credentialsRetriever,
        //IRetriever<Site> siteRetriever,
        //ISettingsService settingsService,
        IPageContext pageContext,
        IContentService contentService)
       // ICurrentSiteHelper currentSiteHelper)
    {
        //_authoredContentService = authoredContentService;
        _httpClientFactory = httpClientFactory;
        //_industryCategoryPageRetriever = industryCategoryPageRetriever;
        //_locationCityPageRetriever = locationCityPageRetriever;
        //_locationOfficePageRetriever = locationOfficePageRetriever;
        //_personPageRetriever = personPageRetriever;
        //_serviceAreaPageRetriever = serviceAreaPageRetriever;
        //_sectionPeoplePageRetriever = sectionPeoplePageRetriever;
        //_specialtiesCategoryPageRetriever = specialtiesCategoryPageRetriever;
        //_pageUrlRetriever = pageUrlRetriever;
        //_credentialsRetriever = credentialsRetriever;
        //_siteRetriever = siteRetriever;
        //_settingsService = settingsService;
        _pageContext = pageContext;
        _contentService = contentService;
       // _currentSiteHelper = currentSiteHelper;
    }

    public async Task<PersonDto> GetPersonPageDtoAsync(Person page, CancellationToken cancellationToken = default)
    {
        var personDto = await MapBasicFieldsAsync(page, cancellationToken);

        //personDto.Credentials = await GetCredentialNamesAsync(page, cancellationToken);
        //personDto.IndustryCategoryNames = await GetIndustryCategoryNamesAsync(page, cancellationToken);
        //personDto.SpecialtiesCategoryNames = await GetSpecialtiesCategoriesNamesAsync(page, cancellationToken);
        //personDto.ServiceAreaNames = await GetServiceAreaNamesAsync(page, cancellationToken);
        //personDto.OfficeLocations = await GetLocationLinkItemsAsync(page, cancellationToken);
       // personDto.AuthoredContent = await _authoredContentService.GetAuthoredContentDtoAsync(page, cancellationToken);

        return personDto;
    }

    public async Task<VCardData?> GeneratePersonVCardAsync(Guid personGuid, string? source = null, CancellationToken cancellationToken = default)
    {
        var person=(await _contentService.GetPagesAsync<Person>(
           new RetrievePagesParameters { },
           query => query
               .OrderBy(nameof(IWebPageContentQueryDataContainer.WebPageItemOrder))
               .Where(w => w.WhereEquals("ContentItemID", personGuid)),
           RetrievalCacheSettings.CacheDisabled,
           cancellationToken
       )).FirstOrDefault();

        if (person is null || !person.EnableVCard)
        {
            return null;
        }

        //var site = await _siteRetriever.RetrieveEntityAsync(person.NodeSiteID, cancellationToken);

        var photoLink = person.MetadataTeaserImage;

        var photoUrl = photoLink.FirstOrDefault()?.ContentAttachmentAsset.Url;
         //(photoLink is RelativeLink relativePhotoLink ? relativePhotoLink.ToAbsolute(site.PresentationURL) : photoLink)?.ToHtmlCompatibleString();

        var (photoMediaType, photoData) = await RetrieveImageBinaryAsync(photoUrl, cancellationToken);
        // TODO: After some time, when we will introduce setting key to change Logo for that Site, this will be needed to be changed
        var (logoMediaType, logoData) = await RetrieveImageBinaryAsync("https://cdn.bdo.global/images/bdo_logo/1.0.0/bdo_logo_color.png", cancellationToken);

        //var offices = await _locationOfficePageRetriever.RetrievePagesAsync(q =>
        //{
        //    q.Where(x => person.MetadataOfficeLocation.Contains(x.NodeID));
        //    q.OnSite(person.NodeSiteID);
        //    q.Culture(person.LanguageTag);
        //    q.Select(x => new { x.LocationAddressLine1, x.LocationAddressLine2, x.LocationZip, x.LocationCountry, x.NodeParentID });
        //}, cancellationToken);

        //var parentNodeIDsOfOffices = offices.Select(y => y.NodeParentID).OfType<int>().ToArray();

        //var cities = await _locationCityPageRetriever.RetrievePagesAsync(q =>
        //{
        //    q.Where(x => parentNodeIDsOfOffices.Contains(x.NodeID));
        //    q.OnSite(person.NodeSiteID);
        //    q.Culture(person.LanguageTag);
        //    q.Select(x => new { x.MetadataTitle, x.RegionName, x.NodeID });
        //}, cancellationToken);

        //var locations = offices.Join(cities, x => x.NodeParentID, x => x.NodeID, (office, city) => ("WORK", new VCardLocationData
        //{
        //    Address = office.LocationAddressLine1,
        //    SecondAddress = office.LocationAddressLine2,
        //    City = city.MetadataTitle,
        //    State = city.RegionName,
        //    Zip = office.LocationZip,
        //    Country = office.LocationCountry,
        //}));

        //var setting = await _settingsService.GetCurrentSiteSettingAsync(SettingName.BDO.DisplayEmailInvCard, cancellationToken);

        var showEmailInVCard = true;//(bool?)setting?.Value;

        var vCard = new VCardData
        {
            FullName = await GetPersonDisplayNameAsync(person),
            EMail = showEmailInVCard == true ? person.PersonEmail : string.Empty,
            Title = person.PersonJobTitle,
            TelephoneNumbers = new List<(string? Type, string? Number)>
            {
                ("WORK", person.PersonPhone),
                ("CELL", person.PersonMobilePhone),
                ("PREF", person.PersonDirectDialNumber),
            },
           // Locations = locations,
            URL = person.GetUrl().AbsoluteUrl,//(await _pageUrlRetriever.GetAbsolutePageUrlAsync(person, cancellationToken)).ToHtmlCompatibleString(),
            RevisionDate = DateTime.UtcNow,
            Source = source,
            Organization = "BDO",
        };

        if (photoMediaType is not null && photoData is not null)
        {
            vCard.PhotoFormat = photoMediaType.Split('/')[1];
            vCard.PhotoBase64 = Convert.ToBase64String(photoData);
        }

        if (logoMediaType is not null && logoData is not null)
        {
            vCard.LogoFormat = logoMediaType.Split('/')[1];
            vCard.LogoBase64 = Convert.ToBase64String(logoData);
        }

        return vCard;
    }

    public async Task<PersonSchemaMarkupDto> GetPersonSchemaMarkupDto(Person person, CancellationToken cancellationToken)
    {
        var socialProfiles = new List<string>();

        if (!string.IsNullOrWhiteSpace(person.Facebook))
        {
            socialProfiles.Add(person.Facebook);
        }

        if (!string.IsNullOrWhiteSpace(person.Twitter))
        {
            socialProfiles.Add(person.Twitter);
        }

        if (!string.IsNullOrWhiteSpace(person.LinkedIn))
        {
            socialProfiles.Add(person.LinkedIn);
        }


        //name = !name.Contains(SiteConstants.CompanyName, System.StringComparison.InvariantCultureIgnoreCase)
        //    ? $"{SiteConstants.CompanyName} {name}"
        //    : name;

        return new PersonSchemaMarkupDto()
        {
            Url = person.GetUrl().AbsoluteUrl,
            Name = person.MetadataTitle,
            JobTitle = person.PersonJobTitle,
            Description = _contentService.GetFallbackMetadataDescription(person.MetadataDescription,person.MetadataOGDescription),
            Image = person.MetadataTeaserImage?.FirstOrDefault()?.ContentAttachmentAsset.Url ?? string.Empty,//.ToHtmlCompatibleString(),
            SameAs = socialProfiles,
            WorksFor = new OrganizationSchemaMarkupDto
            {
                Name = _contentService.GetCurrentSiteLegalNameAsync() ?? string.Empty,
                Logo = _contentService.GetCurrentSiteLogoImageUrlAsync() ?? string.Empty,
            },
        };
    }
    
    private async Task<PersonDto> MapBasicFieldsAsync(Person person, CancellationToken cancellationToken)
    {        
        var sectionPeople = (await _contentService.GetPagesAsync<SectionPeoplePage>(
           new RetrievePagesParameters { },
           query => query
               .OrderBy(nameof(IWebPageContentQueryDataContainer.WebPageItemOrder))
               .Where(w => w.WhereEquals("WebPageItemID", person.SystemFields.WebPageItemParentID)),
           RetrievalCacheSettings.CacheDisabled,
           cancellationToken
       )).FirstOrDefault();

        return new PersonDto
        {
            DisplayName = await GetPersonDisplayNameAsync(person),
            TeaserImageUrl = person.MetadataTeaserImage?.FirstOrDefault()?.ContentAttachmentAsset.Url ?? string.Empty,//.ToHtmlCompatibleString(),
            TeaserImageAltText = person.MetadataTeaserImageAltText,
            Summary = HtmlUtility.RemoveTildeFromUrlTypeHtmlAttributes(person.MetadataDescription),
            JobTitle = person.PersonJobTitle,
            VCardUrl = person.EnableVCard ? $"/vcard/{person.SystemFields.WebPageItemGUID}":null,//.PageGUID}" : null,
            FacebookUrl = person.Facebook,
            TwitterUrl = person.Twitter,
            LinkedInUrl = person.LinkedIn,
            OfficePhoneNumber = person.PersonPhone,
            MobilePhoneNumber = person.PersonMobilePhone,
            DirectDialNumber = person.PersonDirectDialNumber,
            Email = sectionPeople?.DisplayEmailAddressInPersonPage ?? false ? person.PersonEmail : null,
            DisplayFullEmailAddress = sectionPeople?.DisplayFullEmailAddress ?? false,
            EnableForm = person.EnableForm,
            ParentEnableForm = sectionPeople?.EnableForm ?? false
        };
    }
    public async Task<string> GetPersonDisplayNameAsync(Person person)
    {
        return string.IsNullOrEmpty(person.PersonSurnamePrefixes) ? $"{person.PersonFirstName} {person.PersonLastName}" : $"{person.PersonFirstName} {person.PersonSurnamePrefixes} {person.PersonLastName}";
    }
    //private async Task<IEnumerable<string>> GetCredentialNamesAsync(Person person, CancellationToken cancellationToken = default)
    //{
    //    return (await _credentialsRetriever.RetrieveEntitiesAsync(q =>
    //    {
    //        q.Where(x => person.PersonCredentials.Contains(x.ID) && x.LanguageTag == person.LanguageTag);
    //        q.OnSite(person.NodeSiteID);
    //        q.Select(x => x.Name);
    //    }, cancellationToken)).Select(x => x.Name);
    //}

    //private async Task<IEnumerable<string>> GetIndustryCategoryNamesAsync(Person person, CancellationToken cancellationToken = default)
    //{
    //    return (await _industryCategoryPageRetriever.RetrievePagesAsync(q =>
    //    {
    //        q.Where(x => person.MetadataIndustryCategories.Contains(x.NodeID));
    //        q.OnSite(person.NodeSiteID);
    //        q.Culture(person.LanguageTag);
    //        q.Select(x => x.MetadataTitle);
    //    }, cancellationToken)).Select(x => x.MetadataTitle);
    //}

    //private async Task<IEnumerable<string>> GetSpecialtiesCategoriesNamesAsync(Person person, CancellationToken cancellationToken = default)
    //{
    //    return (await _specialtiesCategoryPageRetriever.RetrievePagesAsync(q =>
    //    {
    //        q.Where(x => person.MetadataSpecialtiesCategories.Contains(x.NodeID));
    //        q.OnSite(person.NodeSiteID);
    //        q.Culture(person.LanguageTag);
    //        q.Select(x => x.MetadataTitle);
    //    }, cancellationToken)).Select(x => x.MetadataTitle);
    //}

    //private async Task<IEnumerable<string>> GetServiceAreaNamesAsync(Person person, CancellationToken cancellationToken = default)
    //{
    //    return (await _serviceAreaPageRetriever.RetrievePagesAsync(q =>
    //    {
    //        q.Where(x => person.MetadataServiceAreas.Contains(x.NodeID));
    //        q.OnSite(person.NodeSiteID);
    //        q.Culture(person.LanguageTag);
    //        q.Select(x => x.MetadataTitle);
    //    }, cancellationToken)).Select(x => x.MetadataTitle);
    //}

    //private async Task<IEnumerable<LinkItemDto>> GetLocationLinkItemsAsync(Person person, CancellationToken cancellationToken = default)
    //{
    //    var locationOffices = (await _locationOfficePageRetriever.RetrievePagesAsync(q =>
    //    {
    //        q.Where(x => person.MetadataOfficeLocation.Contains(x.NodeID));
    //        q.OnSite(person.NodeSiteID);
    //        q.Culture(person.LanguageTag);
    //        q.Select(x => new
    //        {
    //            x.MetadataTitle,
    //            x.NodeParentID,
    //        });
    //    }, cancellationToken)).ToArray();

    //    var locationOfficesNodeParentIds = locationOffices
    //        .Where(x => x.NodeParentID is not null)
    //        .DistinctBy(x => x.NodeParentID!.Value)
    //        .Select(x => x.NodeParentID!.Value)
    //        .ToArray();

    //    var locationCities = await _locationCityPageRetriever.RetrievePagesAsync(q =>
    //    {
    //        q.Where(x => locationOfficesNodeParentIds.Contains(x.NodeID));
    //        q.OnSite(person.NodeSiteID);
    //        q.Culture(person.LanguageTag);
    //        q.Select(x => new
    //        {
    //            x.LanguageTag,
    //            x.NodeID,
    //        });
    //    }, cancellationToken);

    //    var locationCityPageByNodeId = locationCities
    //        .ToDictionary(x => x.NodeID, x => x);

    //    var result = new List<LinkItemDto>();

    //    foreach (var locationOffice in locationOffices)
    //    {
    //        result.Add(new LinkItemDto
    //        {
    //            Title = locationOffice.MetadataTitle,
    //            URL = (await _pageUrlRetriever.GetPageUrlAsync(locationCityPageByNodeId[locationOffice.NodeParentID!.Value], cancellationToken)).ToHtmlCompatibleString(),
    //        });
    //    }

    //    return result;
    //}

    private async Task<(string? imageMediaType, byte[]? data)> RetrieveImageBinaryAsync(string? url, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return (null, null);
        }

        var client = CreateHttpClient();

        try
        {
            var response = await client.GetAsync(url, cancellationToken);

            var format = response.Content.Headers.ContentType.MediaType;

            return (format, await response.Content.ReadAsByteArrayAsync(cancellationToken));
        }
        catch
        {
            return (null, null);
        }
    }

    private HttpClient CreateHttpClient()
    {
        if (_httpClient is not null)
        {
            return _httpClient;
        }

        _httpClient = _httpClientFactory.CreateClient("ImageRetriever");

        return _httpClient;
    }
}
