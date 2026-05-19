using BDO.GWT.Core.DTOs;
using CMS.Websites;
using Kentico.Content.Web.Mvc;

namespace BDO.GWT.Core.Helpers;

public interface IExternalPeopleHelper
{
    Task<SidebarExternalPersonDto[]> GetSidebarExternalPeopleDtosAsync(List<Guid> externalPeopleNodeGuids, CancellationToken cancellationToken);
    Task<List<string>> GetExternalPeopleDisplayNames(List<Guid> externalPeopleNodeGuids, CancellationToken cancellationToken);
}

public class ExternalPeopleHelper : IExternalPeopleHelper
{
    private readonly IContentService _contentService;

    public ExternalPeopleHelper(IContentService contentService)
    {
        _contentService = contentService;
    }

    public async Task<SidebarExternalPersonDto[]> GetSidebarExternalPeopleDtosAsync(List<Guid> externalPeopleNodeGuids, CancellationToken cancellationToken)
    {
        var externalPeople = (await _contentService.GetPagesAsync<MicrositePlaceholder>(
           new RetrievePagesParameters { LinkedItemsMaxLevel = 2 },
           query => query
               .OrderBy(nameof(IWebPageContentQueryDataContainer.WebPageItemOrder))
               .Where(w => w.WhereIn("ContentItemGUID", externalPeopleNodeGuids)),
           RetrievalCacheSettings.CacheDisabled,
           cancellationToken
       ));

        var dtos = await Task.WhenAll(externalPeople.Select(async externalPerson => new SidebarExternalPersonDto
        {
            Image = null, // TODO: Map from Person properties
            FullName = "name",//await GetDisplayNameAsync(externalPerson),
            JobTitle = "tesxt",//externalPerson.PersonJobTitle,
            Url = null, // TODO: Map from Person properties
        }));

        return dtos;
    }

    public async Task<List<string>> GetExternalPeopleDisplayNames(List<Guid> externalPeopleNodeGuids, CancellationToken cancellationToken)
    {
        var externalPeople = (await _contentService.GetPagesAsync<Person>(
           new RetrievePagesParameters { LinkedItemsMaxLevel = 2 },
           query => query
               .OrderBy(nameof(IWebPageContentQueryDataContainer.WebPageItemOrder))
               .Where(w => w.WhereIn("ContentItemGUID", externalPeopleNodeGuids)),
           RetrievalCacheSettings.CacheDisabled,
           cancellationToken
       ));
        var names = await Task.WhenAll(externalPeople.Select(async externalPerson => await GetDisplayNameAsync(externalPerson)));

        return names.ToList();
    }
    public async Task<string> GetDisplayNameAsync(Person externalPerson)
    {
        //var setting = await settingsService.GetCurrentSiteSettingAsync(SettingName.BDO.UsePersonPageTitle, cancellationToken);

        //var usePersonPageTitle = (bool?)setting?.Value;

        //if (usePersonPageTitle is true)
        //{
        //    return Title;
        //}

       // return string.IsNullOrEmpty(SurnamePrefixes) ? $"{FirstName} {LastName}" : $"{FirstName} {SurnamePrefixes} {LastName}";
        return string.IsNullOrEmpty(externalPerson.PersonSurnamePrefixes) 
            ? $"{externalPerson.PersonFirstName} {externalPerson.PersonLastName}" 
            : $"{externalPerson.PersonFirstName} {externalPerson.PersonSurnamePrefixes} {externalPerson.PersonLastName}";
    }
}
