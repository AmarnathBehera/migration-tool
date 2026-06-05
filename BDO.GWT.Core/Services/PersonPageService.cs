using BDO.GWT.Core.Constants;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Pages;
using BDO.GWT.Core.DTOs.SchemaMarkups;
using BDO.GWT.Core.Retrievers;
using BDO.GWT.Core.Utilities;
using CMS.Core;
using CMS.DataEngine;
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
    private readonly IContentService _contentService;
    private readonly ICultureContext _cultureContext;
    private readonly ISettingService _settingsService;

    private HttpClient? _httpClient;

    public PersonPageService(
        IHttpClientFactory httpClientFactory,
        IContentService contentService,
        ICultureContext cultureContext,
        ISettingService settingService
        )
    {
        _httpClientFactory = httpClientFactory;
        _contentService = contentService;
        _cultureContext = cultureContext;
        _settingsService = settingService;
    }

    public async Task<PersonDto> GetPersonPageDtoAsync(Person page, CancellationToken cancellationToken = default)
    {
        var personDto = await MapBasicFieldsAsync(page, cancellationToken);

        personDto.Credentials = page.PersonCredentials?
            .Select(x => x.Name)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Select(name => name!)
            ?? Enumerable.Empty<string>();

        personDto.OfficeLocations = await GetLocationLinkItemsAsync(page, cancellationToken);

        return personDto;
    }

    public async Task<VCardData?> GeneratePersonVCardAsync(Guid personGuid, string? source = null, CancellationToken cancellationToken = default)
    {
        var person = (await _contentService.GetPagesAsync<Person>(
           new RetrievePagesParameters
           {
               LinkedItemsMaxLevel = 1
           },
           query => query
               .OrderBy(nameof(IWebPageContentQueryDataContainer.WebPageItemOrder))
               .Where(w => w.WhereEquals("ContentItemGUID", personGuid)),
           RetrievalCacheSettings.CacheDisabled,
           cancellationToken
       )).FirstOrDefault();

        if (person is null || !person.EnableVCard)
        {
            return null;
        }
        var photoLink = person.MetadataTeaserImage;

        var photoUrl = photoLink.FirstOrDefault()?.ContentAttachmentAsset.Url;
        var (photoMediaType, photoData) = await RetrieveImageBinaryAsync(photoUrl, cancellationToken);
        var (logoMediaType, logoData) = await RetrieveImageBinaryAsync(_contentService.GetCurrentSiteLogoImageUrlAsync(), cancellationToken);

        var offices = await _contentService.GetPagesAsync<LocationOffice>(
        new RetrievePagesParameters
        {
            LanguageName = _cultureContext.CurrentLanguageTag,
            UseLanguageFallbacks = false
        },
        query =>
        {
            query.Where(w =>
            {
                w.WhereEquals("ContentItemChannelID", _contentService.GetCurrentChannelID());
                w.WhereIn(nameof(LocationOffice.SystemFields.ContentItemGUID), person.MetadataOfficeLocation.Select(x => x.SystemFields.ContentItemGUID));
            });
            query.Columns(
                nameof(LocationOffice.LocationAddressLine1),
                nameof(LocationOffice.LocationAddressLine2),
                nameof(LocationOffice.LocationZip),
                nameof(LocationOffice.LocationCountry),
                nameof(LocationOffice.SystemFields.WebPageItemParentID)
            );
        },
        RetrievalCacheSettings.CacheDisabled,
        cancellationToken
        );
        var parentNodeIDsOfOffices = offices?.Where(y => y is not null).Select(y => y!.SystemFields.WebPageItemParentID).Where(id => id > 0).ToArray();

        var cities = await _contentService.GetPagesAsync<LocationCity>(
        new RetrievePagesParameters
        {
            LanguageName = _cultureContext.CurrentLanguageTag,
            UseLanguageFallbacks = false
        },
        query =>
        {
            query.Where(w =>
            {
                w.WhereEquals("ContentItemChannelID", _contentService.GetCurrentChannelID());
                w.WhereIn(nameof(LocationCity.SystemFields.WebPageItemID), parentNodeIDsOfOffices);
            });
            query.Columns(
                nameof(LocationCity.MetadataTitle),
                //nameof(LocationCity.RegionName),
                nameof(LocationCity.SystemFields.WebPageItemID)
            );
        },
        RetrievalCacheSettings.CacheDisabled,
        cancellationToken
    );
        var locations = offices?.Join(
    cities ?? Enumerable.Empty<LocationCity?>(),
    x => x?.SystemFields.WebPageItemParentID ?? 0,
    x => x?.SystemFields.WebPageItemID ?? 0,
    (office, city) => ("WORK", new VCardLocationData
    {
        Address = office?.LocationAddressLine1,
        SecondAddress = office?.LocationAddressLine2,
        City = city?.MetadataTitle,
        //State = city?.RegionName,
        Zip = office?.LocationZip,
        Country = office?.LocationCountry,
    }));


        var settings = await _settingsService.GetUsePersonPageTitleInfo();
        var showEmailInVCard = settings?.FirstOrDefault()?.DisplayEmailinvCard;

        var vCard = new VCardData
        {
            FullName = GetPersonDisplayNameAsync(person),
            EMail = showEmailInVCard == true ? person.PersonEmail : string.Empty,
            Title = person.PersonJobTitle,
            TelephoneNumbers = new List<(string? Type, string? Number)>
            {
                ("WORK", person.PersonPhone),
                ("CELL", person.PersonMobilePhone),
                ("PREF", person.PersonDirectDialNumber),
            },
            Locations = locations,
            URL = person.GetUrl().AbsoluteUrl,
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

        return new PersonSchemaMarkupDto()
        {
            Url = person.GetUrl().AbsoluteUrl,
            Name = person.MetadataTitle,
            JobTitle = person.PersonJobTitle,
            Description = _contentService.GetFallbackMetadataDescription(person.MetadataDescription, person.MetadataOGDescription),
            Image = person.MetadataTeaserImage?.FirstOrDefault()?.ContentAttachmentAsset.Url ?? string.Empty,
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
           new RetrievePagesParameters
           {
               LanguageName = _cultureContext.CurrentLanguageTag,
               UseLanguageFallbacks = false,
           },
           query => query
               .OrderBy(nameof(IWebPageContentQueryDataContainer.WebPageItemOrder))
               .Where(w => w.WhereEquals("WebPageItemID", person.SystemFields.WebPageItemParentID)),
           RetrievalCacheSettings.CacheDisabled,
           cancellationToken
       )).FirstOrDefault();

        return new PersonDto
        {
            DisplayName = GetPersonDisplayNameAsync(person),
            TeaserImageUrl = person.MetadataTeaserImage?.FirstOrDefault()?.ContentAttachmentAsset.Url ?? string.Empty,
            TeaserImageAltText = person.MetadataTeaserImageAltText,
            Summary = HtmlUtility.RemoveTildeFromUrlTypeHtmlAttributes(person.MetadataDescription),
            JobTitle = person.PersonJobTitle,
            VCardUrl = person.EnableVCard ? $"/vcard/{person.SystemFields.ContentItemGUID}" : null,
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
    public static string GetPersonDisplayNameAsync(Person person)
    {
        return string.IsNullOrEmpty(person.PersonSurnamePrefixes) ? $"{person.PersonFirstName} {person.PersonLastName}" : $"{person.PersonFirstName} {person.PersonSurnamePrefixes} {person.PersonLastName}";
    }
    private async Task<IEnumerable<LinkItemDto>> GetLocationLinkItemsAsync(Person person, CancellationToken cancellationToken = default)
    {
        var locationOffices = await _contentService.GetPagesAsync<LocationOffice>(
             new RetrievePagesParameters
             {
                 LanguageName = _cultureContext.CurrentLanguageTag,
                 UseLanguageFallbacks = false
             },
            query =>
            {
                query.Where(w =>
                {
                    w.WhereEquals("ContentItemChannelID", _contentService.GetCurrentChannelID());
                    w.WhereIn(nameof(LocationOffice.SystemFields.ContentItemGUID), person.MetadataOfficeLocation.Select(x => x.SystemFields.ContentItemGUID));
                });
                query.Columns(
                    nameof(LocationOffice.MetadataTitle),
                    nameof(LocationOffice.SystemFields.WebPageItemParentID)
                );
            }, RetrievalCacheSettings.CacheDisabled,
            cancellationToken
        );
        var locationOfficesNodeParentIds = locationOffices?
            .Where(x => x is not null && x.SystemFields.WebPageItemParentID > 0)
            .DistinctBy(x => x!.SystemFields.WebPageItemParentID)
            .Select(x => x!.SystemFields.WebPageItemParentID)
            .ToArray();

        var locationCities = await _contentService.GetPagesAsync<LocationCity>(
             new RetrievePagesParameters
             {
                 LanguageName = _cultureContext.CurrentLanguageTag,
                 UseLanguageFallbacks = false
             },
            query =>
            {
                query.Where(w =>
                {
                    w.WhereEquals("ContentItemChannelID", _contentService.GetCurrentChannelID());
                    w.WhereIn(nameof(LocationCity.SystemFields.WebPageItemID), locationOfficesNodeParentIds);
                });
            }, RetrievalCacheSettings.CacheDisabled,
            cancellationToken
        );
        var locationCityPageByNodeId = (locationCities ?? Enumerable.Empty<LocationCity?>())
           .Where(x => x is not null)
           .ToDictionary(x => x!.SystemFields.WebPageItemID, x => x!);

        var result = new List<LinkItemDto>();

        foreach (var locationOffice in locationOffices ?? Enumerable.Empty<LocationOffice?>())
        {
            if (locationOffice is null)
            {
                continue;
            }

            if (locationCityPageByNodeId.TryGetValue(locationOffice.SystemFields.WebPageItemParentID, out var city))
            {
                result.Add(new LinkItemDto
                {
                    Title = locationOffice.MetadataTitle ?? string.Empty,
                    URL = city.GetUrl().AbsoluteUrl,
                });
            }
        }

        return result;
    }

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

            if (response?.Content is null)
            {
                return (null, null);
            }

            var format = response.Content.Headers?.ContentType?.MediaType;

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
