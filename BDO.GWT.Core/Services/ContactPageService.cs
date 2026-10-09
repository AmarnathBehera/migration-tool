using BDO.GWT.Core;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.ValueObjects;
using Kentico.Content.Web.Mvc;

namespace BDO.GWT.Web.Core.Services;

public interface IContactPageService
{
    Task<FormPageDto> GetContactPageDtoAsync(Contact? page, CancellationToken cancellationToken);
}

public class ContactPageService : IContactPageService
{
    private readonly IContentService _contentService;
    private readonly IHeroService _heroService;

    public ContactPageService(
        IContentService contentService,
        IHeroService heroService)
    {
        _contentService = contentService;
        _heroService = heroService;
    }

    public async Task<FormPageDto> GetContactPageDtoAsync(Contact? page, CancellationToken cancellationToken)
    {
        var contactPage = page ?? await _contentService.RetrieveCurrentPageAsync<Contact>(
            new RetrieveCurrentPageParameters
            {
                LinkedItemsMaxLevel = 4
            },
            cancellationToken);

        if (contactPage == null)
        {
            return new FormPageDto();
        }

        var sys = contactPage.SystemFields;

        return new FormPageDto
        {
            Title = contactPage.DescriptionTitle,
            Description = contactPage.DescriptionBody,
            FormType = contactPage.FormType,
            Form = contactPage.Form,
            EmbedCode = contactPage.EmbedCode,

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
                Layout = contactPage.HeroLayout,
                Title = contactPage.HeroTitle,
                Description = contactPage.HeroDescription,

                BackgroundImage = contactPage.HeroBackgroundImage?
                    .FirstOrDefault()?
                    .ContentAttachmentAsset?
                    .Url is string bgUrl
                        ? Link.Parse(bgUrl)
                        : null,

                BackgroundImageAltText = contactPage.HeroBackgroundImageAltText,

                MobileBackgroundImage = contactPage.HeroBackgroundImage_Mobile?
                    .FirstOrDefault()?
                    .ContentAttachmentAsset?
                    .Url is string mobileUrl
                        ? Link.Parse(mobileUrl)
                        : null,

                MobileBackgroundImageAltText = contactPage.HeroBackgroundImage_MobileAltText,

                CallToActionButtonType = contactPage.HeroCallToActionButtonType,
                CallToActionButtonText = contactPage.HeroCallToActionButtonText,

                CallToActionButtonUrl = contactPage.HeroCallToActionButtonUrl?
                    .FirstOrDefault()?
                    .SystemFields?
                    .WebPageUrlPath is string ctaPath
                        ? Link.Parse(ctaPath)
                        : null
            }, cancellationToken)
        };
    }
}