using BDO.GWT.Core.Constants;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.ValueObjects;
using BDO.GWT.Web.Core.DTOs;
using CMS.Core;
using CMS.Websites;
using Kentico.Content.Web.Mvc;

namespace BDO.GWT.Core.Services.Widget;
public interface IViewGlobalLocationsService
{
    Task<ViewGlobalLocationsDto> GetGlobalLocationsBannerAsync(ViewGlobalLocationsData data, CancellationToken cancellationToken);

   // Task<string> GetTranslationAsync(int siteId, LanguageTag languageTag, string key, CancellationToken cancellationToken);
}

public class ViewGlobalLocationsService : IViewGlobalLocationsService
{
    private readonly IContentService _contentService;
    private readonly IWebPageUrlResolver _webPageUrlResolver;

    public ViewGlobalLocationsService(IContentService contentService, IWebPageUrlResolver webPageUrlResolver)
    {
        _contentService = contentService;
        _webPageUrlResolver = webPageUrlResolver;
    }

    public async Task<ViewGlobalLocationsDto> GetGlobalLocationsBannerAsync(ViewGlobalLocationsData data, CancellationToken cancellationToken)
    {
        var globalLocations = (await _contentService.GetPagesAsync<GlobalLocations>(
           new RetrievePagesParameters
           {
               LinkedItemsMaxLevel = 1
           },
           query => query
               .OrderBy(nameof(IWebPageContentQueryDataContainer.WebPageItemOrder)),
           RetrievalCacheSettings.CacheDisabled,
           cancellationToken
       )).FirstOrDefault();

        var globalLocationsURL = string.Empty;

        if (globalLocations is not null)
        {
            var relativeLink = _webPageUrlResolver.GetRelativePath(globalLocations);
            globalLocationsURL = relativeLink;
        }

        //var buttonTitle = await GetTranslationAsync(_siteContext.CurrentSiteID, _pageContext.CurrentPage.LanguageTag, ResourceStringKey.LiveSite.ViewGlobalLocations.Button, cancellationToken);
        //var descriptionText = await GetTranslationAsync(_siteContext.CurrentSiteID, _pageContext.CurrentPage.LanguageTag, ResourceStringKey.LiveSite.ViewGlobalLocations.Description, cancellationToken);
        //var titleText = await GetTranslationAsync(_siteContext.CurrentSiteID, _pageContext.CurrentPage.LanguageTag, ResourceStringKey.LiveSite.ViewGlobalLocations.Title, cancellationToken);
        var buttonTitle = ResourceStringKey.LiveSite.ViewGlobalLocations.Button;
        var descriptionText = ResourceStringKey.LiveSite.ViewGlobalLocations.Description;
        var titleText = ResourceStringKey.LiveSite.ViewGlobalLocations.Title;

        var dto = new ViewGlobalLocationsDto
        {
            Title = titleText,
            Description = descriptionText,
            BackgroundImage = await GetGlobalLoctionsBackgroundImageAsync(cancellationToken),
            Theme = data.Theme,
            Layout = data.Layout,
            TopMargin = data.TopMargin,
            BottomMargin = data.BottomMargin,
            Button = new ButtonDto
            {
                Title = buttonTitle,
                URL = string.IsNullOrWhiteSpace(data.ButtonURL) ? globalLocationsURL : data.ButtonURL,
                Target = data.ButtonTarget,
                Type = "secondary",
            },
            WidgetID = data.WidgetID,
        };

        return dto;
    }

    private async Task<string?> GetGlobalLoctionsBackgroundImageAsync(CancellationToken cancellationToken)
    {
        //var setting = await _settingsService.GetCurrentSiteSettingAsync(SettingName.BDO.ViewGlobalLocationsBackgroundImage, cancellationToken);

        // return (string?)setting?.Value;
        return "";
    }

    //public async Task<string> GetTranslationAsync(int siteId, LanguageTag languageTag, string key, CancellationToken cancellationToken)
    //{
    //    if (languageTag.Language == Language.English)
    //    {
    //        return await _localizationService.GetGlobalTranslationAsync(languageTag, key, cancellationToken) ?? key;
    //    }
    //    else
    //    {
    //        return await _localizationService.GetTranslation(key, languageTag, siteId, cancellationToken) ?? key;
    //    }
    //}
}
