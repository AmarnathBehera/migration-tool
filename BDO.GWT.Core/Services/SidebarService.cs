using BDO.GWT.Core.Constants;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.Helpers;
using BDO.GWT.Web.Core.DTOs.Pages;
using CMS.Websites;
using Kentico.Content.Web.Mvc;

namespace BDO.GWT.Core.Services;
public interface ISidebarService
{
    Task<ContentSidebarDto> GetSidebarDtoAsync<T>(T page, string pageUrl, CancellationToken cancellationToken) where T : ISidebar, IPageMetadata, IWebPageFieldsSource;

    Task<SidebarPersonDto[]> GetContentPageAuthorDtosGen(List<Guid> relevantPersonGuids, CancellationToken cancellationToken);

    Task<ContentSidebarDto> GetContentPageSidebarDetails(ContentSidebarDto sidebarDto,
       CancellationToken cancellationToken, List<Guid>? relevantPersonGuidList = null, DateTime? publishedDate = null, DateTime? updatedDate = null);
    Task<DealSidebarDto> GetDealSidebarDtoAsync(Deal page, CancellationToken cancellationToken);
}
public class SidebarService : ISidebarService
{
   private readonly ISocialMediaButtonsService _socialMediaButtonsService;
   private readonly ISettingService _settingService;
    private readonly IContentService _contentService;
    public SidebarService(ISocialMediaButtonsService socialMediaButtonsService, ISettingService settingService,IContentService contentService)
    {
        _socialMediaButtonsService = socialMediaButtonsService;
        _settingService = settingService;
        _contentService = contentService;
    }

       public async Task<ContentSidebarDto> GetSidebarDtoAsync<T>(T page,string pageUrl, CancellationToken cancellationToken) where T : ISidebar, IPageMetadata, IWebPageFieldsSource
    {
        var settings = await _settingService.GetPrintorShareInfo();
        var settingInfo = settings?.FirstOrDefault();
        
        var sidebarDto = new ContentSidebarDto
        {
            ShowPrintButton = (settingInfo?.DisablePrint is not true) && page.ShowPrintButton,
            DownloadButtonAttachment = settingInfo?.DisableDownload is true
                                      ? null
                                      : page.DownloadButtonAttachment?.FirstOrDefault()?.ContentAttachmentAsset.Url,
            SocialMediaButtons = page.ShowSocialMediaButtons ? await _socialMediaButtonsService.GetSocialMediaButtonsDto(page, page.PageTitle, cancellationToken) : null,
            ShowCopyLinkButton = settingInfo?.DisableCopyLinkButton is not true,
        };

        if (sidebarDto.SocialMediaButtons is not null)
        {
            sidebarDto.SocialMediaButtons.ShowCopyLinkButton = null;
        }

        return sidebarDto;
    }

    public async Task<DealSidebarDto> GetDealSidebarDtoAsync(Deal page, CancellationToken cancellationToken)
    {
        var settings = await _settingService.GetPrintorShareInfo();
        var settingInfo = settings?.FirstOrDefault();

        var sidebarDto = new DealSidebarDto
        {
            ShowPrintButton = (settingInfo?.DisablePrint is not true) && page.ShowPrintButton,
            DownloadButtonAttachment = settingInfo?.DisableDownload is true
                                      ? null
                                      : page.DownloadButtonAttachment?.FirstOrDefault()?.ContentAttachmentAsset.Url,
            SocialMediaButtons = page.ShowSocialMediaButtons ? await _socialMediaButtonsService.GetSocialMediaButtonsDto(page, page.PageTitle, cancellationToken) : null,
            ShowCopyLinkButton = settingInfo?.DisableCopyLinkButton is not true,
            Date = page.DateField,
            CallToAction = page.ShowCallToAction ? new CallToActionDto
            {
                Description = page.CallToActionDescription,
                ButtonTitle = page.CallToActionButtonText,
                ButtonUrl = page.CallToActionButtonUrl,
            } : null,
            Contacts = await GetContentPageAuthorDtosGen(page.RelevantContactPersonDeal.Select(p => p.SystemFields.ContentItemGUID).ToList(), cancellationToken),
        };

        if (sidebarDto.SocialMediaButtons is not null)
        {
            sidebarDto.SocialMediaButtons.ShowCopyLinkButton = null;
        }

        return sidebarDto;
    }


    public async Task<ContentSidebarDto> GetContentPageSidebarDetails(ContentSidebarDto sidebarDto,
       CancellationToken cancellationToken, List<Guid>? relevantPersonGuidList = null, DateTime? publishedDate = null, DateTime? updatedDate = null)
    {

        sidebarDto.PublishDate = publishedDate;
        sidebarDto.UpdatedDate = updatedDate;
        sidebarDto.RelevantContactPeopleTitle = ResourceStringKey.LiveSite.Insights.Authors;

      
        if (relevantPersonGuidList != null && relevantPersonGuidList.Count > 0)
        {
            sidebarDto.RelevantContactPeople = await GetContentPageAuthorDtosGen(relevantPersonGuidList, cancellationToken);
        }

        return sidebarDto;
    }


    public async Task<SidebarPersonDto[]> GetContentPageAuthorDtosGen(List<Guid> relevantPersonGuids, CancellationToken cancellationToken)
    {
        
        var people = (await _contentService.GetPagesAsync<Person>(
           new RetrievePagesParameters { LinkedItemsMaxLevel = 2 },
           query => query
               .OrderBy(nameof(IWebPageContentQueryDataContainer.WebPageItemOrder))
               .Where(w => w.WhereIn("ContentItemGUID", relevantPersonGuids)),
           RetrievalCacheSettings.CacheDisabled,
           cancellationToken
       ))
       .DistinctBy(x => x.SystemFields.ContentItemGUID).ToArray();

        var internalPeople = (await _contentService.GetPagesAsync<InternalPerson>(
           new RetrievePagesParameters { LinkedItemsMaxLevel = 2 },
           query => query
               .OrderBy(nameof(IWebPageContentQueryDataContainer.WebPageItemOrder))
               .Where(w => w.WhereIn("ContentItemGUID", relevantPersonGuids)),
           RetrievalCacheSettings.CacheDisabled,
           cancellationToken
       ))
       .DistinctBy(x => x.SystemFields.ContentItemGUID).ToArray();

        var dtsByGuid = new Dictionary<Guid, SidebarPersonDto>();

        foreach (var person in people)
        {
            dtsByGuid[person.SystemFields.ContentItemGUID] = new SidebarPersonDto
            {
                Image = PageHelper.GetImageDto(person.MetadataTeaserImage?.FirstOrDefault()?.ContentAttachmentAsset.Url.TrimStart('~'), person.MetadataTeaserImageAltText),
                FullName = GetPersonDisplayName(person),
                JobTitle = person.PersonJobTitle,
                Url = person.GetUrl().RelativePath,
            };
        }

        foreach (var internalPerson in internalPeople)
        {
            dtsByGuid[internalPerson.SystemFields.ContentItemGUID] = new SidebarPersonDto
            {
                Image = PageHelper.GetImageDto(internalPerson.ImageFile?.FirstOrDefault()?.ContentAttachmentAsset.Url.TrimStart('~'), internalPerson.ImageFileAltText),
                FullName = GetInternalPersonDisplayName(internalPerson),
                JobTitle = internalPerson.JobTitle,
                Url = _contentService.EnsureAbsoluteUrl(internalPerson.Url)
            };
        }
        return relevantPersonGuids.Select(guid => dtsByGuid.TryGetValue(guid, out var dto) ? dto : null)
                                  .OfType<SidebarPersonDto>().ToArray();
    }

    private static string GetInternalPersonDisplayName(InternalPerson person)
    {
        var name = person.FirstName ?? string.Empty;

        if (!string.IsNullOrWhiteSpace(person.SurnamePrefixes))
            name += " " + person.SurnamePrefixes;

        if (!string.IsNullOrWhiteSpace(person.LastName))
            name += " " + person.LastName;

        return name;
    }

    private static string GetPersonDisplayName(Person person)
    {
        var name = person.PersonFirstName ?? string.Empty;

        if (!string.IsNullOrWhiteSpace(person.PersonSurnamePrefixes))
            name += " " + person.PersonSurnamePrefixes;

        if (!string.IsNullOrWhiteSpace(person.PersonLastName))
            name += " " + person.PersonLastName;

        return name;
    }
}
