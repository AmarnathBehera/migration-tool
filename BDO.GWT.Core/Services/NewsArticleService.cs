using BDO.GWT.Common.Exceptions;
using BDO.GWT.Core.Constants;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Pages;
using BDO.GWT.Core.Helpers;
using BDO.GWT.Core.Utilities;
using CMS.Websites;
using Kentico.Content.Web.Mvc;

namespace BDO.GWT.Core.Services;

public interface INewsArticleService{   
    Task<ContentPageDto> GetContentPageDtoAsync(NewsArticle contentPage, CancellationToken cancellationToken);
    Task<ContentSidebarDto> GetContentPageSidebarDto(NewsArticle contentPage, CancellationToken cancellationToken);
}

public class NewsArticleService : INewsArticleService
{
    private readonly ISidebarService _sidebarService;
    private readonly IContentService _contentService;
    private readonly IExternalPeopleHelper _externalPeopleHelper;

    public NewsArticleService(
        ISidebarService sidebarService,
        IContentService contentService,
        IExternalPeopleHelper externalPeopleHelper
        )
    {
        _sidebarService = sidebarService;
        _contentService = contentService;
        _externalPeopleHelper = externalPeopleHelper;
    }

    public async Task<ContentPageDto> GetContentPageDtoAsync(NewsArticle contentPage, CancellationToken cancellationToken)
    {
        var dto = new ContentPageDto
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
        var sidebarDto = await _sidebarService.GetSidebarDtoAsync(contentPage, cancellationToken);

        sidebarDto.ShowCopyLinkButton = await GetShowCopyLinkOptionFromSectionPageAsync(contentPage, sidebarDto.ShowCopyLinkButton, cancellationToken);
        //sidebarDto.TypeName = await contentPage.GetTypeNameAsync(_contentTypeRetriever, cancellationToken);
        //sidebarDto.PublishDate = contentPage.PublishFrom ?? contentPage.CreationDate;
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
           new RetrievePagesParameters { LinkedItemsMaxLevel =2},
           query => query   
               .OrderBy(nameof(IWebPageContentQueryDataContainer.WebPageItemOrder))
               .Where(w => w.WhereIn("ContentItemGUID", relevantPersonGuids)),
           RetrievalCacheSettings.CacheDisabled,
           cancellationToken
       ))
       .DistinctBy(x => x.SystemFields.ContentItemGUID).ToArray();

        var internalPeople = (await _contentService.GetPagesAsync<MicrositePlaceholder>(
           new RetrievePagesParameters { LinkedItemsMaxLevel = 2 },
           query => query
               .OrderBy(nameof(IWebPageContentQueryDataContainer.WebPageItemOrder))
               .Where(w => w.WhereIn("ContentItemGUID", relevantPersonGuids)),
           RetrievalCacheSettings.CacheDisabled,
           cancellationToken
       ))
       .DistinctBy(x => x.SystemFields.ContentItemGUID).ToArray();

        var internalPeopleAsPerson = internalPeople.Select(internalPerson => new Person
        {
            SystemFields = internalPerson.SystemFields,
            //PersonFirstName = internalPerson.FirstName,
            //PersonLastName = internalPerson.LastName,
            //PersonSurnamePrefixes = internalPerson.SurnamePrefixes,
            //PersonJobTitle = internalPerson.JobTitle,
            //MetadataTeaserImage = internalPerson.Image,
            //MetadataTeaserImageAltText = internalPerson.ImageAltText,
        });

        var unorderedPeople = people.Concat(internalPeopleAsPerson).ToArray();
        var orderedPeople = OrderingUtility.OrderPeopleByPeopleSort(unorderedPeople, relevantPersonGuids).ToArray();

        var dtos = new List<SidebarPersonDto>();
        foreach (var person in orderedPeople)
        {
            dtos.Add(new SidebarPersonDto
            {
                Image = PageHelper.GetImageDto(person.MetadataTeaserImage?.FirstOrDefault()?.ContentAttachmentAsset.Url, person.MetadataTeaserImageAltText),
                FullName = GetPersonDisplayName(person),
                JobTitle = person.PersonJobTitle,
                Url = person.GetUrl().RelativePath,
            });
        }

        return dtos.ToArray();
    }

    private string GetPersonDisplayName(Person person)
    {
        var name = person.PersonFirstName ?? string.Empty;

        if (!string.IsNullOrWhiteSpace(person.PersonSurnamePrefixes))
            name += " " + person.PersonSurnamePrefixes;

        if (!string.IsNullOrWhiteSpace(person.PersonLastName))
            name += " " + person.PersonLastName;

        return name;
    }

    private static CallToActionDto? GetCallToActionDto(NewsArticle contentPage)
    {
        //if (contentPage is Insight insight)
        //{
        //    return PageHelper.GetCallToActionDto(
        //        insight.CallToActionDescription,
        //        insight.CallToActionButtonText,
        //        insight.CallToActionButtonUrl,
        //        insight.CallToActionPosition
        //    );
        //}

        return contentPage.ShowSidebar
            ? PageHelper.GetCallToActionDto(contentPage.CallToActionDescription, contentPage.CallToActionButtonText, contentPage.CallToActionButtonUrl, null)
            : null;
    }

    private async Task<bool> GetShowCopyLinkOptionFromSectionPageAsync(NewsArticle contentPage, bool showCopyLinkButton, CancellationToken cancellationToken)
    {
        if (contentPage is NewsArticle)
        {
            var newsSectionPage = (await _contentService.RetrievePagesAsync<SectionNews>(cancellationToken))?.FirstOrDefault()
                ?? throw new MissingSectionPageException($"No pages have been found of type '{nameof(SectionNews)}'.");

            return newsSectionPage.ShowCopyUrlButtonInNewsArticle && showCopyLinkButton;
        }

        //if (contentPage is BlogPost)
        //{
        //    var blogSectionPage = (await _multiTypePageRetriever.RetrievePagesAsync<SectionBlogs>(query =>
        //    {
        //        query.Types(typeof(SectionBlogs));
        //        query.Take(1);
        //    }, cancellationToken)).FirstOrDefault()
        //        ?? throw new MissingSectionPageException($"No pages have been found of type '{nameof(SectionBlogs)}'.");

        //    return blogSectionPage.ShowCopyUrlButtonInBlogPage && showCopyLinkButton;
        //}

        return showCopyLinkButton;
    }
}