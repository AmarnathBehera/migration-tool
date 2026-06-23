using BDO.GWT.Common.Exceptions;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Pages;
using BDO.GWT.Core.Helpers;
using BDO.GWT.Core.Utilities;

namespace BDO.GWT.Core.Services;
public interface INewsArticleService
{
    Task<SectionNewsPageDto?> GetSectionNewsPageDtoAsync(NewsArticle contentPage, CancellationToken cancellationToken);
    
}
public class NewsArticleService : INewsArticleService
{
    private readonly ISidebarService _sidebarService;
    private readonly IContentService _contentService;
    private readonly IExternalPeopleHelper _externalPeopleHelper;
    private readonly IWebPageUrlResolver _webPageUrlResolver;
    public NewsArticleService(
        ISidebarService sidebarService,
        IContentService contentService,
        IExternalPeopleHelper externalPeopleHelper,
        IWebPageUrlResolver webPageUrlResolver)
    {
        _sidebarService = sidebarService;
        _contentService = contentService;
        _externalPeopleHelper = externalPeopleHelper;
        _webPageUrlResolver = webPageUrlResolver;

    }
    public async Task<SectionNewsPageDto?> GetSectionNewsPageDtoAsync(NewsArticle contentPage, CancellationToken cancellationToken)
    {
        if(contentPage == null)            
            return null;

        List<Guid> relevantPersonGuids = new List<Guid>();
        var pageUrl = _webPageUrlResolver.GetAbsoluteUrl(contentPage);
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
   

    private async Task<bool> GetShowCopyLinkOptionFromSectionPageAsync( bool showCopyLinkButton, CancellationToken cancellationToken)
    {
        

        var newsSectionPage = (await _contentService.RetrievePagesAsync<SectionNews>(cancellationToken))?.FirstOrDefault()
            ?? throw new MissingSectionPageException($"No pages have been found of type '{nameof(SectionNews)}'.");

        return newsSectionPage.ShowCopyUrlButtonInNewsArticle && showCopyLinkButton;
    }
}

