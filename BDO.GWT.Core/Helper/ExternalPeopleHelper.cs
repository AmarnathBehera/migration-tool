using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.Services;
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
    private readonly ISettingService _settingsService;

    public ExternalPeopleHelper(IContentService contentService, ISettingService settingsService)
    {
        _contentService = contentService;
        _settingsService = settingsService;
    }

    public async Task<SidebarExternalPersonDto[]> GetSidebarExternalPeopleDtosAsync(List<Guid> externalPeopleNodeGuids, CancellationToken cancellationToken)
    {
        var externalPeople = await _contentService.GetPagesAsync<ExternalPerson>(
           new RetrievePagesParameters { LinkedItemsMaxLevel = 2 },
           query => query
               .OrderBy(nameof(IWebPageContentQueryDataContainer.WebPageItemOrder))
               .Where(w => w.WhereIn("ContentItemGUID", externalPeopleNodeGuids)),
           RetrievalCacheSettings.CacheDisabled,
           cancellationToken
       );

        var dtos = await Task.WhenAll(externalPeople.Select(async externalPerson =>
        {
           var imageFile = externalPerson.ImageFile.FirstOrDefault();

            return new SidebarExternalPersonDto
            {
                Image = imageFile != null ? new ImageDto
                {
                    Url = imageFile.ContentAttachmentAsset.Url,
                    AltText = externalPerson.ImageFileAltText
                } : null,
                FullName = await GetDisplayNameAsync(externalPerson),
                JobTitle = externalPerson.JobTitle,
                Url = _contentService.EnsureAbsoluteUrl(externalPerson.Url)
            };
        }));

        return dtos;
    }

    public async Task<List<string>> GetExternalPeopleDisplayNames(List<Guid> externalPeopleNodeGuids, CancellationToken cancellationToken)
    {
        var externalPeople = (await _contentService.GetPagesAsync<ExternalPerson>(
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
    public async Task<string> GetDisplayNameAsync(ExternalPerson externalPerson)
    {
        //var setting = await _settingsService.GetUsePersonPageTitleInfo();

        //var usePersonPageTitle = setting?.FirstOrDefault()?.UsePageTitleToDisplayPersonName ?? false;

        //if (usePersonPageTitle)
        //{
        //    return externalPerson.MetadataTitle;
        //}

        return string.IsNullOrEmpty(externalPerson.SurnamePrefixes) ? $"{externalPerson.FirstName} {externalPerson.LastName}" : $"{externalPerson.FirstName} {externalPerson.SurnamePrefixes} {externalPerson.LastName}";
    }
}
