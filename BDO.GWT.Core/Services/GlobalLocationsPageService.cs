using BDO.GWT.Core.Data;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Pages;
using BDO.GWT.Core.Utilities;
using BDO.GWT.Core.ValueObjects;
using BDO.GWT.Web.Core.Services;

namespace BDO.GWT.Core.Services;

public interface IGlobalLocationsPageService
{
    Task<GlobalLocationsDto> GetGlobalLocationsPageDtoAsync(GlobalLocations globalLocations, CancellationToken cancellationToken = default);
}

public class GlobalLocationsPageService : IGlobalLocationsPageService
{
    private static Dictionary<string, string> _mapRegion = new()
    {
        ["1"] = "AMERICAS",
        ["2"] = "EMEA",
        ["3"] = "ASIA PACIFIC"
    };

    private readonly IGlobalLocationCustomTableDataService _globalLocationCustomTableDataService;
    private readonly IHeroService _heroService;
    private readonly ISettingService _settingService;

    public GlobalLocationsPageService(IGlobalLocationCustomTableDataService globalLocationCustomTableDataService, IHeroService heroService, ISettingService settingService)
    {
        _globalLocationCustomTableDataService = globalLocationCustomTableDataService;
        _heroService = heroService;
        _settingService = settingService;
    }

    public async Task<GlobalLocationsDto> GetGlobalLocationsPageDtoAsync(GlobalLocations globalLocations, CancellationToken cancellationToken = default)
    {
        var bgImageUrl = globalLocations.HeroBackgroundImage?.FirstOrDefault()?.ContentAttachmentAsset?.Url;
        var mobileImageUrl = globalLocations.HeroBackgroundImage_Mobile?.FirstOrDefault()?.ContentAttachmentAsset?.Url;
        var ctaUrlPath = globalLocations.HeroCallToActionButtonUrl?.FirstOrDefault()?.SystemFields?.WebPageUrlPath;

        var heroData = new HeroData
        {
            Layout = globalLocations.HeroLayout,
            Title = globalLocations.HeroTitle,
            Description = globalLocations.HeroDescription,
            BackgroundImage = bgImageUrl != null ? Link.Parse(bgImageUrl) : null,
            BackgroundImageAltText = globalLocations.HeroBackgroundImageAltText,
            MobileBackgroundImage = mobileImageUrl != null ? Link.Parse(mobileImageUrl) : null,
            MobileBackgroundImageAltText = globalLocations.HeroBackgroundImage_MobileAltText,
            CallToActionButtonType = globalLocations.HeroCallToActionButtonType,
            CallToActionButtonText = globalLocations.HeroCallToActionButtonText,
            CallToActionButtonUrl = ctaUrlPath != null ? Link.Parse(ctaUrlPath) : null,
        };

        var globalLocationInfo = await _settingService.GetGlobalSettingAsync();
        return new GlobalLocationsDto
        {

            Hero = await _heroService.GetHeroDtoAsync(heroData, cancellationToken),
            MainGrid = new GlobalLocationsGridDto
            {
                Title = globalLocationInfo?.GlobalLocationsTitle ?? string.Empty,
                Description = HtmlUtility.RemoveTildeFromUrlTypeHtmlAttributes(globalLocationInfo?.GlobalLocationsDescription ?? string.Empty),
                Locations = await GetGlobalLocationsDtoAsync(cancellationToken),
            }
        };
    }

    private async Task<List<GlobalLocationsRegionDto>> GetGlobalLocationsDtoAsync(CancellationToken cancellationToken = default)
    {
        var locations = await _globalLocationCustomTableDataService.GetAllGlobalLocationsAsync();

        var regionalList = locations
            .GroupBy(x => x.Region?.ToUpperInvariant() ?? string.Empty)
            .Select(g =>
            new GlobalLocationsRegionDto
            {
                Region = _mapRegion[g.Key],
                RegionalLocations = g.Select(v => new LinkItemDto()
                {
                    Title = v.Title,
                    URL = v.Url
                })
                .OrderBy(x => x.Title)
                .ToList()

            }).ToList();

        return regionalList;
    }
}
