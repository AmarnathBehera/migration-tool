
using BDO.GWT.Core;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.ValueObjects;
using CMS.Websites.Routing;
using Kentico.Content.Web.Mvc;

namespace BDO.GWT.Web.Core.Services;

public interface IEventRegistrationFormPageService
{

    Task<FormPageDto> GetEventRegistrationFormPageDtoAsync(EventRegistrationForm page, CancellationToken cancellationToken);
}

public class EventRegistrationFormPageService : IEventRegistrationFormPageService
{
    private readonly IHeroService _heroService;
    private readonly IContentService _contentRetriever;
    private readonly IWebsiteChannelContext _websiteChannelContext;
    public EventRegistrationFormPageService(IContentService pageRetriever,
        IHeroService heroService,
        IWebsiteChannelContext websiteChannelContext)
    {
        _contentRetriever = pageRetriever;
        _heroService = heroService;
        _websiteChannelContext = websiteChannelContext;
    }

    public async Task<FormPageDto> GetEventRegistrationFormPageDtoAsync(EventRegistrationForm page, CancellationToken cancellationToken)
    {
        var results = await _contentRetriever.GetPagesAsync<EventRegistrationForm>(
          new RetrievePagesParameters
          {
              LinkedItemsMaxLevel = 5
          },
          query =>
          {
              query.Where(w =>
                  w.WhereEquals("ContentItemChannelID", _websiteChannelContext.WebsiteChannelID));
          },
          RetrievalCacheSettings.CacheDisabled,
          cancellationToken);

        var EventRPage = results?.FirstOrDefault();

        if (EventRPage == null)
        {
            return new FormPageDto(); // or throw exception if required
        }

        var sys = EventRPage.SystemFields;

        return new FormPageDto
        {
            Title = EventRPage.PageTitle,
            Description = EventRPage.PageDescription,
            FormType = EventRPage.FormType,
            Form = EventRPage.Form,
            EmbedCode = EventRPage.EmbedCode,

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
                Layout = EventRPage.HeroLayout,
                Title = EventRPage.HeroTitle,
                Description = EventRPage.HeroDescription,
                BackgroundImage = EventRPage.HeroBackgroundImage?.FirstOrDefault()?.ContentAttachmentAsset?.Url is string bgUrl ? Link.Parse(bgUrl) : null,
                BackgroundImageAltText = EventRPage.HeroBackgroundImageAltText,
                MobileBackgroundImage = EventRPage.HeroBackgroundImage_Mobile?.FirstOrDefault()?.ContentAttachmentAsset?.Url is string mobileUrl ? Link.Parse(mobileUrl) : null,
                MobileBackgroundImageAltText = EventRPage.HeroBackgroundImage_MobileAltText,
                CallToActionButtonType = EventRPage.HeroCallToActionButtonType,
                CallToActionButtonText = EventRPage.HeroCallToActionButtonText,
                CallToActionButtonUrl = EventRPage.HeroCallToActionButtonUrl?.FirstOrDefault()?.SystemFields?.WebPageUrlPath is string ctaPath ? Link.Parse(ctaPath) : null,
            }, cancellationToken),
        };
    }
}