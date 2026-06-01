using BDO.GWT.Common.Exceptions;
using BDO.GWT.Core.Constants;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.Services;

namespace BDO.GWT.Core.Helpers;

public static class PageHelper
{
   
    public static ImageDto? GetImageDto(String? imageLink, string? imageAltText)
    {
        if (imageLink == null)
        {
            return null;
        }

        var dto = new ImageDto
        {
            Url = imageLink,
            AltText = imageAltText ?? throw new InvalidStateException("Image alt text must be set when image link is present."),
        };

        return dto;
    }

    public static CallToActionDto? GetCallToActionDto(string? callToActionDescription, string? callToActionButtonText, String? callToActionButtonLink)
    {
        if (callToActionButtonText == null || callToActionButtonLink == null)
        {
            return null;
        }

        var dto = new CallToActionDto
        {
            Description = callToActionDescription,
            ButtonTitle = callToActionButtonText,
            ButtonUrl = callToActionButtonLink,
          
        };

        return dto;
    }

    public static CallToActionDto? GetCallToActionDto<T>(T contentPage) where T : ICallToAction, ISidebar
    {
        if (contentPage.CallToActionButtonText == null || contentPage.CallToActionButtonUrl == null)
        {
            return null;
        }

        var dto = new CallToActionDto
        {
            Description = contentPage.CallToActionDescription,
            ButtonTitle = contentPage.CallToActionButtonText,
            ButtonUrl = contentPage.CallToActionButtonUrl

        };

        return dto;

    }

    //public static async Task<ContentSidebarDto> GetContentPageSidebarDto<T>(T contentPage, CancellationToken cancellationToken) where T : ISidebar, IPageMetadata
    //{
    //    var sidebarDto = await _sidebarService.GetSidebarDtoAsync(contentPage, cancellationToken);

    //    var tags = await _taxonomyTagRetriever.GetTaxonomyTagByGUIDs(contentPage.ContentType, _cultureContext.CurrentLanguageTag);
    //    var type = tags?.FirstOrDefault()?.Title;

    //    sidebarDto.ShowCopyLinkButton = await GetShowCopyLinkOptionFromSectionNewsPageAsync(sidebarDto.ShowCopyLinkButton, cancellationToken);
    //    sidebarDto.TypeName = type;
    //    sidebarDto.PublishDate = contentPage.SystemFields.ContentItemCommonDataLastPublishedWhen ?? contentPage.SystemFields.ContentItemCommonDataFirstPublishedWhen;
    //    sidebarDto.UpdatedDate = contentPage.UpdatedOn == DateTime.MinValue ? null : contentPage.UpdatedOn;
    //    sidebarDto.RelevantContactPeopleTitle = ResourceStringKey.LiveSite.Insights.Authors;
    //    sidebarDto.RelevantContactPeople = await GetContentPageAuthorDtos(contentPage, cancellationToken);
    //    sidebarDto.RelatedExternalPeople = await _externalPeopleHelper.GetSidebarExternalPeopleDtosAsync(
    //        contentPage.RelatedExternalPeople.Select(p => p.SystemFields.ContentItemGUID).ToList(),
    //        cancellationToken);

    //    return sidebarDto;
    //}
}
