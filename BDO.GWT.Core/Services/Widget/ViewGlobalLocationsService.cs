using BDO.GWT.Core.Constants;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.Helper;
using BDO.GWT.Core.ValueObjects;
using BDO.GWT.Web.Core.DTOs;
using CMS.Core;
using CMS.Websites;
using Kentico.Content.Web.Mvc;

namespace BDO.GWT.Core.Services.Widget;
public interface IViewGlobalLocationsService
{
    Task<ViewGlobalLocationsDto> GetGlobalLocationsBannerAsync(ViewGlobalLocationsData data, CancellationToken cancellationToken);
}

public class ViewGlobalLocationsService : IViewGlobalLocationsService
{
    private readonly IContentService _contentService;
    private readonly IWebPageUrlResolver _webPageUrlResolver;
    private readonly ICultureContext _cultureContext;
    private readonly ILocalizationHelper _localizationHelper;

    public ViewGlobalLocationsService(IContentService contentService, IWebPageUrlResolver webPageUrlResolver, ILocalizationHelper localizationHelper, ICultureContext cultureContext)
    {
        _contentService = contentService;
        _webPageUrlResolver = webPageUrlResolver;
        _localizationHelper = localizationHelper;
        _cultureContext = cultureContext;
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

        var buttonTitle = await _localizationHelper.GetTranslationAsync(_cultureContext.CurrentLanguageTag,ResourceStringKey.LiveSite.ViewGlobalLocations.Button, cancellationToken);
        var descriptionText = await _localizationHelper.GetTranslationAsync(_cultureContext.CurrentLanguageTag, ResourceStringKey.LiveSite.ViewGlobalLocations.Description, cancellationToken);
        var titleText = await _localizationHelper.GetTranslationAsync(_cultureContext.CurrentLanguageTag, ResourceStringKey.LiveSite.ViewGlobalLocations.Title, cancellationToken);

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
}
