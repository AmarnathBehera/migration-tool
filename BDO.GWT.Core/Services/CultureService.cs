using BDO.GWT.Core.ValueObjects;
using CMS.Websites.Routing;

namespace BDO.GWT.Core.Services;

public class CultureService : ICultureService
{
    private readonly IWebsiteCulturesService _websiteCulturesService;
    private readonly IWebsiteChannelContext _siteContext;

    public CultureService(IWebsiteCulturesService websiteCulturesService, IWebsiteChannelContext siteContext)
    {
        _websiteCulturesService = websiteCulturesService;
        _siteContext = siteContext;
    }

    public async Task<IEnumerable<LanguageTag>> GetCurrentSiteCulturesAsync(CancellationToken cancellationToken)
    {
        return await GetSiteCulturesAsync(_siteContext.WebsiteChannelID, cancellationToken);
    }

    public async Task<IEnumerable<LanguageTag>> GetSiteCulturesAsync(int siteID, CancellationToken cancellationToken)
    {
        var siteCultures = await _websiteCulturesService.GetWebsiteCulturesAsync();

        var languageTags = siteCultures.Select(x => LanguageTag.Parse(x.ShortName));

        return languageTags;
    }

    public async Task<IEnumerable<LanguageTag>> GetSiteCulturesLanguageCodeAsync(int siteID, CancellationToken cancellationToken)
    {
        var siteCultures = await _websiteCulturesService.GetWebsiteCulturesAsync();

        var languageTags = siteCultures.Select(x => LanguageTag.Parse(x.CultureCode));

        return languageTags;
    }
}
