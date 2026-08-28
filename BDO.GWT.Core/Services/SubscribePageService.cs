using BDO.GWT.Core.Data;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Pages;
using BDO.GWT.Core.ValueObjects;
using BDO.GWT.Web.Core.DTOs.Pages;
using BDO.GWT.Web.Core.Services;
using CMS.ContactManagement;
using CMS.ContentEngine;
using CMS.DataEngine;
using CMS.Globalization;
using CMS.Websites;
using CMS.Websites.Routing;
using Kentico.Content.Web.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BDO.GWT.Core.Services;

public interface ISubscribePageService
{
    Task<SubscribeDto> GetSubscribeDtoAsync(Subscribe? subscribePage, CancellationToken cancellationToken);
}

public class SubscribePageService : ISubscribePageService
{
    private readonly IHeroService _heroService;
    private readonly IContentRetriever _contentRetriever;
    private readonly IWebsiteChannelContext _websiteChannelContext;
    private readonly IInfoProvider<WebsiteCaptchaSettingsInfo> _captchaSettingsProvider;
    private readonly IInfoProvider<CountryInfo> _countryProvider;
    private readonly IInfoProvider<ContactGroupInfo> _recipientListInfoProvider;

    public SubscribePageService(IInfoProvider<CountryInfo> countryProvider,IInfoProvider<ContactGroupInfo> recipientListInfoProvider,IWebsiteChannelContext websiteChannelContext, IHeroService heroService,IContentRetriever contentRetriever, IInfoProvider<WebsiteCaptchaSettingsInfo> captchaSettingsProvider)
    {
        _heroService = heroService;
        _contentRetriever = contentRetriever;
        _websiteChannelContext = websiteChannelContext;
        _captchaSettingsProvider = captchaSettingsProvider;
        _recipientListInfoProvider = recipientListInfoProvider;
        _countryProvider = countryProvider;
    }

    public async Task<SubscribeDto> GetSubscribeDtoAsync(Subscribe? subscribePage, CancellationToken cancellationToken)
    {
    if (subscribePage is null)
    {
        return new SubscribeDto
        {
            Hero = new BDO.GWT.Web.Core.DTOs.HeroDto { Layout = string.Empty },
            FormType = null,
            EmbedCode = null,
            SubscribeFormDto = null,
        };
    }

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
            SiteKey = GetRecaptchaPublicKey() ?? string.Empty,
            EndpointRoute = $"newsletter/subscribe/{subscribePage.SystemFields.WebPageItemGUID}",
            RedirectUrl = redirectLink ?? string.Empty,
            Newsletters = await GetAllSiteNewsletterOptionDtos(),
            Countries = await GetAllCountriesOptionDtosAsync(cancellationToken),
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
    public async Task<BDO.GWT.Core.DTOs.OptionDto[]> GetAllCountriesOptionDtosAsync(CancellationToken cancellationToken)
    {
        var countries = await _countryProvider.Get()
                                              .Columns(nameof(CountryInfo.CountryGUID), nameof(CountryInfo.CountryDisplayName))
                                              .OrderBy(nameof(CountryInfo.CountryDisplayName))
                                              .GetEnumerableTypedResultAsync(cancellationToken: cancellationToken);

        return countries.Select(c => new BDO.GWT.Core.DTOs.OptionDto
        {
            Value = c.CountryGUID.ToString(),
            Label = c.CountryDisplayName
        }).ToArray();
    }

    private Task<BDO.GWT.Core.DTOs.OptionDto[]> GetAllSiteNewsletterOptionDtos()
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

        return Task.FromResult(recipientLists
            .Select(list => new BDO.GWT.Core.DTOs.OptionDto
            {
                Value = list.ContactGroupGUID.ToString(),
                Label = string.IsNullOrWhiteSpace(list.ContactGroupDisplayName)
                    ? list.ContactGroupName
                    : list.ContactGroupDisplayName
            })
            .ToArray());
    }
    private string? GetRecaptchaPublicKey()
    {
        int channelId = _websiteChannelContext.WebsiteChannelID;
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
