using BDO.GWT.Common.Exceptions;
using BDO.GWT.Core.Constants;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Pages;
using BDO.GWT.Core.Helpers;
using BDO.GWT.Core.Retrievers;
using BDO.GWT.Core.Utilities;
using CMS.Websites;
using HotChocolate.Resolvers;
using Kentico.Content.Web.Mvc;


namespace BDO.GWT.Core.Services;
public interface INewsArticleService{   
    Task<SectionNewsPageDto?> GetSectionNewsPageDtoAsync(NewsArticle contentPage, CancellationToken cancellationToken);
    //Task<ContentSidebarDto> GetContentPageSidebarDto<T>(T contentPage, CancellationToken cancellationToken) where T : NewsArticle;
}
public class NewsArticleService : INewsArticleService
{
    private readonly ISidebarService _sidebarService;
    private readonly IContentService _contentService;
    private readonly IExternalPeopleHelper _externalPeopleHelper;
    private readonly ITaxonomyTagRetriever _taxonomyTagRetriever;
    private readonly ICultureContext _cultureContext;

    public NewsArticleService(
        ISidebarService sidebarService,
        IContentService contentService,
        IExternalPeopleHelper externalPeopleHelper,
        ITaxonomyTagRetriever taxonomyTagRetriever,
        ICultureContext cultureContext)
    {
        _sidebarService = sidebarService;
        _contentService = contentService;
        _externalPeopleHelper = externalPeopleHelper;
        _taxonomyTagRetriever = taxonomyTagRetriever;
        _cultureContext = cultureContext;
    }
    public async Task<SectionNewsPageDto?> GetSectionNewsPageDtoAsync(NewsArticle contentPage, CancellationToken cancellationToken)
    {
        if(contentPage == null)            
            return null;

        List<Guid> relevantPersonGuids = new List<Guid>();
        var pageUrl = contentPage.GetUrl().AbsoluteUrl;
        ContentSidebarDto? sideBarDto = contentPage.ShowSidebar
                ? await _sidebarService.GetSidebarDtoAsync(contentPage, pageUrl, cancellationToken)
                : null;
        
        if(contentPage.RelevantContactPerson != null)
        {
           relevantPersonGuids = contentPage.RelevantContactPerson.Select(p => p.SystemFields.ContentItemGUID).ToList();
        }

        if (sideBarDto != null)
        {
            var publishedDate = contentPage.SystemFields.ContentItemCommonDataLastPublishedWhen ?? 
                contentPage.SystemFields.ContentItemCommonDataFirstPublishedWhen;
            var updatedDate = contentPage.UpdatedOn == DateTime.MinValue ? null : (DateTime?)contentPage.UpdatedOn;
            sideBarDto = await _sidebarService.GetContentPageSidebarDetails(sideBarDto,cancellationToken, 
                                                                          relevantPersonGuids, publishedDate, updatedDate);

            sideBarDto.ShowCopyLinkButton = await GetShowCopyLinkOptionFromSectionPageAsync(sideBarDto.ShowCopyLinkButton, cancellationToken);

            if(contentPage.RelatedExternalPeople != null)
            {
                sideBarDto.RelatedExternalPeople = await _externalPeopleHelper.GetSidebarExternalPeopleDtosAsync(
                             contentPage.RelatedExternalPeople.Select(p => p.SystemFields.ContentItemGUID).ToList(),
                             cancellationToken);
            }

           

        }

        var dto = new SectionNewsPageDto
        {
            Title = contentPage.ShowTitle ? contentPage.MetadataTitle : null,
            Description = contentPage.ShowDescription
                ? HtmlUtility.RemoveTildeFromUrlTypeHtmlAttributes(contentPage.MetadataDescription)
                : null,
            Content = HtmlUtility.RemoveTildeFromUrlTypeHtmlAttributes(contentPage.Content),
            Sidebar = contentPage.ShowSidebar
                ? sideBarDto
                : null,
            CallToAction = contentPage.ShowCallToAction
                ? PageHelper.GetCallToActionDto(contentPage)
                : null,
        };
        return dto;
    }

    //public async Task<ContentSidebarDto> GetContentPageSidebarDto<T>(T contentPage, CancellationToken cancellationToken) where T : NewsArticle
    //{
    //    var sidebarDto = await _sidebarService.GetSidebarDtoAsync(contentPage, cancellationToken);
               
    //    return sidebarDto;
    //}

   
    

    //public async Task<SidebarPersonDto[]> GetContentPageAuthorDtos(NewsArticle contentPage, CancellationToken cancellationToken)
    //{
    //    var relevantPersonGuids = contentPage.RelevantContactPerson.Select(p => p.SystemFields.ContentItemGUID).ToList();

    //    var people = (await _contentService.GetPagesAsync<Person>(
    //       new RetrievePagesParameters { LinkedItemsMaxLevel =2},
    //       query => query   
    //           .OrderBy(nameof(IWebPageContentQueryDataContainer.WebPageItemOrder))
    //           .Where(w => w.WhereIn("ContentItemGUID", relevantPersonGuids)),
    //       RetrievalCacheSettings.CacheDisabled,
    //       cancellationToken
    //   ))
    //   .DistinctBy(x => x.SystemFields.ContentItemGUID).ToArray();

    //    var internalPeople = (await _contentService.GetPagesAsync<InternalPerson>(
    //       new RetrievePagesParameters { LinkedItemsMaxLevel = 2 },
    //       query => query
    //           .OrderBy(nameof(IWebPageContentQueryDataContainer.WebPageItemOrder))
    //           .Where(w => w.WhereIn("ContentItemGUID", relevantPersonGuids)),
    //       RetrievalCacheSettings.CacheDisabled,
    //       cancellationToken
    //   ))
    //   .DistinctBy(x => x.SystemFields.ContentItemGUID).ToArray();

    //    var dtsByGuid = new Dictionary<Guid, SidebarPersonDto>();

    //    foreach (var person in people)
    //    {
    //        dtsByGuid[person.SystemFields.ContentItemGUID] = new SidebarPersonDto
    //        {
    //            Image = PageHelper.GetImageDto(person.MetadataTeaserImage?.FirstOrDefault()?.ContentAttachmentAsset.Url, person.MetadataTeaserImageAltText),
    //            FullName = GetPersonDisplayName(person),
    //            JobTitle = person.PersonJobTitle,
    //            Url = person.GetUrl().RelativePath,
    //        };
    //    }

    //    foreach (var internalPerson in internalPeople)
    //    {
    //        dtsByGuid[internalPerson.SystemFields.ContentItemGUID] = new SidebarPersonDto
    //        {
    //            Image = PageHelper.GetImageDto(internalPerson.ImageFile?.FirstOrDefault()?.ContentAttachmentAsset.Url, internalPerson.ImageFileAltText),
    //            FullName = GetInternalPersonDisplayName(internalPerson),
    //            JobTitle = internalPerson.JobTitle,
    //            Url = _contentService.EnsureAbsoluteUrl(internalPerson.Url)
    //        };
    //    }
    //    return relevantPersonGuids.Select(guid => dtsByGuid.TryGetValue(guid, out var dto) ? dto : null)
    //                              .OfType<SidebarPersonDto>().ToArray();
    //}



    private static string GetPersonDisplayName(Person person)
    {
        var name = person.PersonFirstName ?? string.Empty;

        if (!string.IsNullOrWhiteSpace(person.PersonSurnamePrefixes))
            name += " " + person.PersonSurnamePrefixes;

        if (!string.IsNullOrWhiteSpace(person.PersonLastName))
            name += " " + person.PersonLastName;

        return name;
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

   

    private async Task<bool> GetShowCopyLinkOptionFromSectionPageAsync( bool showCopyLinkButton, CancellationToken cancellationToken)
    {
        

        var newsSectionPage = (await _contentService.RetrievePagesAsync<SectionNews>(cancellationToken))?.FirstOrDefault()
            ?? throw new MissingSectionPageException($"No pages have been found of type '{nameof(SectionNews)}'.");

        return newsSectionPage.ShowCopyUrlButtonInNewsArticle && showCopyLinkButton;
    }
}

//public interface ISidebarFields
//{
//    bool ShowSidebar { get; }
//}

//public interface ICallToActionFields
//{
//    string? CallToActionDescription { get; }
//    string? CallToActionButtonText { get; }
//    string? CallToActionButtonUrl { get; }

//    string? CallToActionPosition { get; }
//}