using BDO.GWT.Core.Data;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Pages;
using BDO.GWT.Core.ValueObjects;
using BDO.GWT.Web.Core.DTOs.Pages;
using BDO.GWT.Web.Core.Services;
using CMS.ContactManagement;
using CMS.ContentEngine;
using CMS.DataEngine;
using CMS.Websites;
using CMS.Websites.Routing;
using Kentico.Content.Web.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BDO.GWT.Core.Services;

public interface ISubscribePageService
{
    Task<SubscribeDto> GetSubscribeDtoAsync(Subscribe subscribePage, CancellationToken cancellationToken);
}

public class SubscribePageService : ISubscribePageService
{
    private readonly IHeroService _heroService;
    private readonly IContentRetriever _contentRetriever;
    private readonly IWebsiteChannelContext _websiteChannelContext;
    private readonly IInfoProvider<WebsiteCaptchaSettingsInfo> _captchaSettingsProvider;

    // IMPORTANT: keep these aligned with values stored in WebsiteCaptchaSettingsReCaptchaVersion
    private const int RecaptchaVersionV2 = 2;
    private const int RecaptchaVersionV3 = 3;

    private readonly IInfoProvider<ContactGroupInfo> _recipientListInfoProvider;

    public SubscribePageService(IInfoProvider<ContactGroupInfo> recipientListInfoProvider,IWebsiteChannelContext websiteChannelContext, IHeroService heroService,IContentRetriever contentRetriever, IInfoProvider<WebsiteCaptchaSettingsInfo> captchaSettingsProvider)
    {
        _heroService = heroService;
        _contentRetriever = contentRetriever;
        _websiteChannelContext = websiteChannelContext;
        _captchaSettingsProvider = captchaSettingsProvider;
        _recipientListInfoProvider = recipientListInfoProvider;

    }

    public async Task<SubscribeDto> GetSubscribeDtoAsync(Subscribe subscribePage, CancellationToken cancellationToken)
    {
        var redirectPageGuid = subscribePage.RedirectPageOnFormSubmission?.FirstOrDefault()?.WebPageGuid;
        var redirectPages = redirectPageGuid.HasValue && redirectPageGuid.Value != Guid.Empty
            ? await _contentRetriever.RetrieveAllPagesByGuids<IWebPageFieldsSource>([redirectPageGuid.Value], cancellationToken: cancellationToken)
            : Enumerable.Empty<IWebPageFieldsSource>();

        var redirectLink = redirectPages.FirstOrDefault()?.GetUrl()?.RelativePath;

        var dto = new SubscribeDto
        {
            Hero = await _heroService.GetHeroDtoAsync(new HeroData
            {
                Layout = subscribePage.HeroLayout,
                Title = subscribePage.HeroTitle,
                Description = subscribePage.HeroDescription,

                BackgroundImage = subscribePage.HeroBackgroundImage?
                    .FirstOrDefault()?
                    .ContentAttachmentAsset?
                    .Url is string bgUrl
                        ? Link.Parse(bgUrl)
                        : null,

                BackgroundImageAltText = subscribePage.HeroBackgroundImageAltText,

                MobileBackgroundImage = subscribePage.HeroBackgroundImage_Mobile?
                    .FirstOrDefault()?
                    .ContentAttachmentAsset?
                    .Url is string mobileUrl
                        ? Link.Parse(mobileUrl)
                        : null,

                MobileBackgroundImageAltText = subscribePage.HeroBackgroundImage_MobileAltText,

                CallToActionButtonType = subscribePage.HeroCallToActionButtonType,
                CallToActionButtonText = subscribePage.HeroCallToActionButtonText,

                CallToActionButtonUrl = subscribePage.HeroCallToActionButtonUrl?
                    .FirstOrDefault()?
                    .SystemFields?
                    .WebPageUrlPath is string ctaPath
                        ? Link.Parse(ctaPath)
                        : null
            }, cancellationToken),
            FormType = subscribePage.FormType,
            EmbedCode = subscribePage.EmbedCode,
            SubscribeFormDto = subscribePage.FormType == "form" ? new SubscribeFormDto
            {
                IsV3 = subscribePage.RecaptchaUseV3,
                SiteKey = GetRecaptchaPublicKey(subscribePage, cancellationToken) ?? string.Empty,
                EndpointRoute = $"newsletter/subscribe/{subscribePage.SystemFields.WebPageItemGUID}",
                RedirectUrl = redirectLink ?? string.Empty,
                Newsletters = await GetAllSiteNewsletterOptionDtos(cancellationToken),
               // Countries = await GetAllCountriesOptionDtos(cancellationToken),
                ShowCompanyField = subscribePage.ShowCompanyField,
                IsCompanyRequired = subscribePage.IsCompanyRequired,
                ShowJobTitleField = subscribePage.ShowJobTitleField,
                IsJobTitleRequired = subscribePage.IsJobTitleRequired,
                ShowCountryField = subscribePage.ShowCountryField,
                IsCountryRequired = subscribePage.IsCountryRequired,
                AddConsentToForm = subscribePage.AddConsentToForm,
            } : null,
        };

        return dto;
    }



    private async Task<BDO.GWT.Core.DTOs.OptionDto[]> GetAllSiteNewsletterOptionDtos(
     CancellationToken cancellationToken)
    {
        var recipientLists = _recipientListInfoProvider.Get()
            .Columns(
                nameof(ContactGroupInfo.ContactGroupName),
                nameof(ContactGroupInfo.ContactGroupDisplayName),
                nameof(ContactGroupInfo.ContactGroupGUID))
            .OrderBy(nameof(ContactGroupInfo.ContactGroupDisplayName))
            .GetEnumerableTypedResult()
            .Where(list => !string.IsNullOrWhiteSpace(list.ContactGroupName))
            .ToList();

        return recipientLists
            .Select(list => new BDO.GWT.Core.DTOs.OptionDto
            {
                Value = list.ContactGroupGUID.ToString(),
                Label = string.IsNullOrWhiteSpace(list.ContactGroupDisplayName)
                    ? list.ContactGroupName
                    : list.ContactGroupDisplayName
            })
            .ToArray();
    }
    private string? GetRecaptchaPublicKey(Subscribe subscribePage, CancellationToken cancellationToken)
    {
        int channelId = _websiteChannelContext.WebsiteChannelID;
        int expectedVersion = subscribePage.RecaptchaUseV3 ? RecaptchaVersionV3 : RecaptchaVersionV2;
        var captchaSettings = _captchaSettingsProvider
                .Get()
                .WhereEquals(
                    nameof(WebsiteCaptchaSettingsInfo.WebsiteCaptchaSettingsWebsiteChannelID),
                    channelId)
                .TopN(1)
                .FirstOrDefault();

         return captchaSettings?.WebsiteCaptchaSettingsReCaptchaSiteKey;
    }
}
