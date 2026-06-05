using BDO.GWT.Common.Exceptions;
using BDO.GWT.Core.Constants;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Pages;
using BDO.GWT.Core.Helpers;
using BDO.GWT.Core.Retrievers;
using BDO.GWT.Core.Utilities;
using CMS.Websites;
using Kentico.Content.Web.Mvc;

namespace BDO.GWT.Core.Services;
public interface INewsArticleService
{
    Task<SectionNewsPageDto> GetSectionNewsPageDtoAsync(NewsArticle contentPage, CancellationToken cancellationToken);
    Task<ContentSidebarDto> GetContentPageSidebarDto(NewsArticle contentPage, CancellationToken cancellationToken);
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
    public async Task<SectionNewsPageDto> GetSectionNewsPageDtoAsync(NewsArticle contentPage, CancellationToken cancellationToken)
    {
        var dto = new SectionNewsPageDto
        {
            Title = contentPage.ShowTitle ? contentPage.MetadataTitle : null,
            Description = contentPage.ShowDescription
                ? HtmlUtility.RemoveTildeFromUrlTypeHtmlAttributes(contentPage.MetadataDescription)
                : null,
            Content = HtmlUtility.RemoveTildeFromUrlTypeHtmlAttributes(contentPage.Content),
            Sidebar = contentPage.ShowSidebar
                ? await GetContentPageSidebarDto(contentPage, cancellationToken)
                : null,
            CallToAction = contentPage.ShowCallToAction
                ? GetCallToActionDto(contentPage)
                : null,
        };
        return dto;
    }

    public async Task<ContentSidebarDto> GetContentPageSidebarDto(NewsArticle contentPage, CancellationToken cancellationToken)
    {
        var sidebarDto = await _sidebarService.GetNewsSidebarDtoAsync(contentPage, cancellationToken);

        sidebarDto.ShowCopyLinkButton = await GetShowCopyLinkOptionFromSectionPageAsync(contentPage, sidebarDto.ShowCopyLinkButton, cancellationToken);
        sidebarDto.TypeName = contentPage.ContentType?.FirstOrDefault()?.Name;
        sidebarDto.PublishDate = contentPage.SystemFields.ContentItemCommonDataLastPublishedWhen ?? contentPage.SystemFields.ContentItemCommonDataFirstPublishedWhen;
        sidebarDto.UpdatedDate = contentPage.UpdatedOn == DateTime.MinValue ? null : contentPage.UpdatedOn;
        sidebarDto.RelevantContactPeopleTitle = ResourceStringKey.LiveSite.Insights.Authors;
        sidebarDto.RelevantContactPeople = await GetContentPageAuthorDtos(contentPage, cancellationToken);
        sidebarDto.RelatedExternalPeople = await _externalPeopleHelper.GetSidebarExternalPeopleDtosAsync(
            contentPage.RelatedExternalPeople.Select(p => p.SystemFields.ContentItemGUID).ToList(),
            cancellationToken);

        return sidebarDto;
    }
    private async Task<SidebarPersonDto[]> GetContentPageAuthorDtos(NewsArticle contentPage, CancellationToken cancellationToken)
    {
        var relevantPersonGuids = contentPage.RelevantContactPerson.Select(p => p.SystemFields.ContentItemGUID).ToList();

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
                Image = PageHelper.GetImageDto(person.MetadataTeaserImage?.FirstOrDefault()?.ContentAttachmentAsset.Url, person.MetadataTeaserImageAltText),
                FullName = GetPersonDisplayName(person),
                JobTitle = person.PersonJobTitle,
                Url = person.GetUrl().RelativePath,
            };
        }

        foreach (var internalPerson in internalPeople)
        {
            dtsByGuid[internalPerson.SystemFields.ContentItemGUID] = new SidebarPersonDto
            {
                Image = PageHelper.GetImageDto(internalPerson.ImageFile?.FirstOrDefault()?.ContentAttachmentAsset.Url, internalPerson.ImageFileAltText),
                FullName = GetInternalPersonDisplayName(internalPerson),
                JobTitle = internalPerson.JobTitle,
                Url = _contentService.EnsureAbsoluteUrl(internalPerson.Url)
            };
        }
        return relevantPersonGuids.Select(guid => dtsByGuid.TryGetValue(guid, out var dto) ? dto : null)
                                  .OfType<SidebarPersonDto>().ToArray();
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

    private static string GetInternalPersonDisplayName(InternalPerson person)
    {
        var name = person.FirstName ?? string.Empty;

        if (!string.IsNullOrWhiteSpace(person.SurnamePrefixes))
            name += " " + person.SurnamePrefixes;

        if (!string.IsNullOrWhiteSpace(person.LastName))
            name += " " + person.LastName;

        return name;
    }

    private static CallToActionDto? GetCallToActionDto(NewsArticle contentPage)
    {
        return contentPage.ShowSidebar
            ? PageHelper.GetCallToActionDto(contentPage.CallToActionDescription, contentPage.CallToActionButtonText, contentPage.CallToActionButtonUrl, null)
            : null;
    }

    private async Task<bool> GetShowCopyLinkOptionFromSectionPageAsync(NewsArticle contentPage, bool showCopyLinkButton, CancellationToken cancellationToken)
    {
        if (contentPage == null)
            return showCopyLinkButton;

        var newsSectionPage = (await _contentService.RetrievePagesAsync<SectionNews>(cancellationToken))?.FirstOrDefault()
            ?? throw new MissingSectionPageException($"No pages have been found of type '{nameof(SectionNews)}'.");

        return newsSectionPage.ShowCopyUrlButtonInNewsArticle && showCopyLinkButton;
    }
}