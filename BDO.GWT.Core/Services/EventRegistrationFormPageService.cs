using BDO.GWT.Core;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.ReusableFieldsDTOs;
using BDO.GWT.Core.ValueObjects;
using CMS.Websites.Routing;
using Kentico.Content.Web.Mvc;

namespace BDO.GWT.Web.Core.Services;

public interface IEventRegistrationFormPageService
{
    Task<FormPageDto> GetEventRegistrationFormPageDtoAsync(
        EventRegistrationForm? page,
        CancellationToken cancellationToken);
}

public class EventRegistrationFormPageService : IEventRegistrationFormPageService
{
    private readonly IHeroService _heroService;
    private readonly IContentService _contentService;
    private readonly IWebsiteChannelContext _websiteChannelContext;

    public EventRegistrationFormPageService(
        IContentService contentService,
        IHeroService heroService,
        IWebsiteChannelContext websiteChannelContext)
    {
        _contentService = contentService;
        _heroService = heroService;
        _websiteChannelContext = websiteChannelContext;
    }

    public async Task<FormPageDto> GetEventRegistrationFormPageDtoAsync(
        EventRegistrationForm? page,
        CancellationToken cancellationToken)
    {
        var eventRegistrationPage = page ?? await _contentService.RetrieveCurrentPageAsync<EventRegistrationForm>(
            new RetrieveCurrentPageParameters
            {
                LinkedItemsMaxLevel = 4
            },
            cancellationToken);

        if (eventRegistrationPage is null)
        {
            return new FormPageDto();
        }

        var sys = eventRegistrationPage.SystemFields;

        return new FormPageDto
        {
            Title = eventRegistrationPage.PageTitle,
            Description = eventRegistrationPage.PageDescription,
            FormType = eventRegistrationPage.FormType,
            Form = eventRegistrationPage.Form,
            EmbedCode = eventRegistrationPage.EmbedCode,

            BaseContent = new BaseContent
            {
                ContentItemID = sys?.ContentItemID ?? 0,
                ContentItemGUID = sys?.ContentItemGUID ?? Guid.Empty,
                ContentItemName = sys?.ContentItemName,
                ContentItemContentTypeID = sys?.ContentItemContentTypeID ?? 0,
                ContentItemIsSecured = sys?.ContentItemIsSecured ?? false,
                ContentItemCommonDataContentLanguageID = sys?.ContentItemCommonDataContentLanguageID ?? 0,
                ContentItemCommonDataFirstPublishedWhen = sys?.ContentItemCommonDataFirstPublishedWhen ?? default,
                ContentItemCommonDataLastPublishedWhen = sys?.ContentItemCommonDataLastPublishedWhen ?? default
            },

            BaseWebPage = new BaseWebPage
            {
                WebPageItemGUID = sys?.WebPageItemGUID ?? Guid.Empty,
                WebPageItemID = sys?.WebPageItemID ?? 0,
                WebPageItemName = sys?.WebPageItemName,
                WebPageItemWebsiteChannelID = sys?.WebPageItemWebsiteChannelId ?? 0,
                WebPageUrlPath = sys?.WebPageUrlPath,
                WebPageItemOrder = sys?.WebPageItemOrder ?? 0,
                WebPageItemTreePath = sys?.WebPageItemTreePath,
                WebPageUrlPathContentLanguageID = sys?.WebPageUrlPathContentLanguageID ?? 0,
                WebPageItemParentID = sys?.WebPageItemParentID ?? 0
            },

            Hero = await _heroService.GetHeroDtoAsync(new HeroData
            {
                Layout = eventRegistrationPage.HeroLayout,
                Title = eventRegistrationPage.HeroTitle,
                Description = eventRegistrationPage.HeroDescription,

                BackgroundImage = eventRegistrationPage.HeroBackgroundImage?
                    .FirstOrDefault()?
                    .ContentAttachmentAsset?
                    .Url is string bgUrl
                        ? Link.Parse(bgUrl)
                        : null,

                BackgroundImageAltText = eventRegistrationPage.HeroBackgroundImageAltText,

                MobileBackgroundImage = eventRegistrationPage.HeroBackgroundImage_Mobile?
                    .FirstOrDefault()?
                    .ContentAttachmentAsset?
                    .Url is string mobileUrl
                        ? Link.Parse(mobileUrl)
                        : null,

                MobileBackgroundImageAltText = eventRegistrationPage.HeroBackgroundImage_MobileAltText,

                CallToActionButtonType = eventRegistrationPage.HeroCallToActionButtonType,
                CallToActionButtonText = eventRegistrationPage.HeroCallToActionButtonText,

                CallToActionButtonUrl = eventRegistrationPage.HeroCallToActionButtonUrl?
                    .FirstOrDefault()?
                    .SystemFields?
                    .WebPageUrlPath is string ctaPath
                        ? Link.Parse(ctaPath)
                        : null
            }, cancellationToken)
        };
    }
}