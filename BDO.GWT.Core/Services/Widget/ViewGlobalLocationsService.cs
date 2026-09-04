using BDO.GWT.Core.Constants;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.Helper;
using BDO.GWT.Core.ValueObjects;
using BDO.GWT.Web.Core.DTOs;
using CMS.Core;
using CMS.Websites;
using Google.Protobuf.WellKnownTypes;
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

    private readonly ISettingService _settingService;

    public ViewGlobalLocationsService(IContentService contentService, IWebPageUrlResolver webPageUrlResolver, ILocalizationHelper localizationHelper, ICultureContext cultureContext, ISettingService settingService)
    {
        _contentService = contentService;
        _webPageUrlResolver = webPageUrlResolver;
        _localizationHelper = localizationHelper;
        _cultureContext = cultureContext;
        _settingService = settingService;
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
            BackgroundImage = await GetGlobalLocationsBackgroundImageAsync(cancellationToken),
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
    private async Task<string?> GetGlobalLocationsBackgroundImageAsync(CancellationToken cancellationToken)
    {
        var globalLocationInfo = await _settingService.GetGlobalSettingAsync();
        var setting = globalLocationInfo?.GlobalLocationsBackgroundImage.FirstOrDefault();

        if (setting is null)
        {
            return "";
        }

        var attachments = await _contentService.GetItemsAsync<ContentAttachment>(
            builder => builder.Parameters(p => p
                .Where(w => w
                    .WhereEquals("ContentItemGUID", setting.Identifier) 
                    .And()
                    .WhereNotNull("ContentAttachmentAsset"))),
            cancellationToken);
        if (attachments == null)
        {
            return "";
        }
        var attachment = attachments?.FirstOrDefault();      
        return attachment?.ContentAttachmentAsset?.Url;
    }
}
