using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Footer;
using BDO.GWT.Core.ValueObjects;
using CMS.Websites;
using Kentico.Content.Web.Mvc;

namespace BDO.GWT.Core.Services.Footer;

public interface IFooterService
{
    Task<FooterDto?> GetAsync(CancellationToken cancellationToken);
}

public class FooterService : IFooterService
{
    private readonly IContentService _contentService;

    public FooterService(IContentService contentService)
    {
        _contentService = contentService;
    }

    public async Task<FooterDto?> GetAsync(CancellationToken cancellationToken)
    {
        var mandatoryFooterLinks = await GetMandatoryFooterLinkDtosAsync(cancellationToken);

        var footerLinks = await GetFooterLinks() ?? Enumerable.Empty<FooterLinkDto>();
        var socialLinks = await GetSocialLinks() ?? Enumerable.Empty<SocialButtonDto>();

        //var displaySubscriptionsSectionSetting = await _settingsService.GetCurrentSiteSettingAsync(SettingName.BDO.DisplaySubscriptionsSection, cancellationToken);

        var footerDto = new FooterDto
        {
            DisplaySubscriptionsSection = true,//(bool?)displaySubscriptionsSectionSetting?.Value ?? false,
            SubscriptionButtonLink = (await GetSubscribePageLinkAsync(cancellationToken))?.ToHtmlCompatibleString(),
            FooterLinks = mandatoryFooterLinks.Concat(footerLinks
                .Select(link => new FooterLinkDto
                {
                    OpenInNewTab = link?.OpenInNewTab ?? false,
                    Order = link?.Order ?? 0,
                    Title = link?.Title ?? string.Empty,
                    URL = link?.URL?.ToString() ?? string.Empty,
                })
                .OrderBy(o => o.Order)
            ).Take(12).ToList(),
            SocialButtons = socialLinks.Select(link => new SocialButtonDto
            {
                AltText = link?.AltText ?? string.Empty,
                AriaLabel = link?.AriaLabel ?? string.Empty,
                IconType = link?.IconType ?? string.Empty,
                //CustomIcon = link.IconType == SocialLinkIconType.Custom ? link.CustomIcon : null,
                Order = link?.Order ?? 0,
                URL = link?.URL?.ToString() ?? string.Empty,
            }).OrderBy(o => o.Order).Take(5).ToList(),
        };

        return footerDto;
    }
    private async Task<Link?> GetSubscribePageLinkAsync(CancellationToken cancellationToken)
    {
        var subscribePages = await _contentService.GetContentType<Subscribe>(cancellationToken);
        var subscribePage = subscribePages?.FirstOrDefault();
        if (subscribePage == null)
            return null;

        var url = subscribePage.GetUrl()?.AbsoluteUrl;
        if (string.IsNullOrEmpty(url))
            return null;

        return Link.TryParse(url, out var link) ? link : null;
    }

    private async Task<List<FooterLinkDto>> GetMandatoryFooterLinkDtosAsync(CancellationToken cancellationToken)
    {
        string rootPath = "/";

        var pages = (await _contentService.GetPagesAsync<FooterPages>(
        new RetrievePagesParameters { PathMatch = PathMatch.Children(rootPath, 1) },
        query => query.OrderBy(nameof(IWebPageContentQueryDataContainer.WebPageItemOrder)),
        RetrievalCacheSettings.CacheDisabled,
        cancellationToken
    ))?.ToList() ?? new List<FooterPages>();

        var allGuids = pages
            .Where(p => p.FooterLink != null)
            .SelectMany(p => p.FooterLink?.Select(i => i.WebPageGuid) ?? Enumerable.Empty<Guid>())
            .Distinct()
            .ToList();

        var selectedPages = allGuids.Any()
            ? await _contentService.GetItemsByGUIDAsync(allGuids)
            : Enumerable.Empty<IWebPageFieldsSource>();

        int order = 0;
        var result = selectedPages?.Select(page => new FooterLinkDto
        {
            Title = (page as IMetadata)?.MetadataTitle ?? string.Empty,
            OpenInNewTab = false,
            Order = order--,
            URL = page?.GetUrl()?.RelativePath?.ToString() ?? string.Empty
        })
        .OrderBy(x => x.Order)
        .ToList() ?? new List<FooterLinkDto>();

        return result;
    }

    private async Task<IEnumerable<FooterLinkDto?>> GetFooterLinks()
    {
        var footerLinksPages = (await _contentService.GetItemsChannelAsync<FooterLinks>())?.Take(8).ToList() ?? new List<FooterLinks>();
        var footerPages = new List<FooterLinkDto>();

        var allGuids = footerLinksPages
      .Where(x => x.TargetURL != null)
      .SelectMany(x => x.TargetURL?.Select(t => t.SystemFields?.WebPageItemGUID ?? Guid.Empty) ?? Enumerable.Empty<Guid>())
      .Where(guid => guid != Guid.Empty)
      .Distinct()
      .ToList();

        var selectedPages = allGuids.Any()
            ? await _contentService.GetItemsByGUIDAsync(allGuids)
            : Enumerable.Empty<IWebPageFieldsSource>();

        var pageUrlMap = selectedPages?.ToDictionary(
            p => p.SystemFields?.WebPageItemGUID ?? Guid.Empty,
            p => p.GetUrl()?.RelativePath ?? string.Empty
        ) ?? new Dictionary<Guid, string>();

        int order = 1;
        foreach (var link in footerLinksPages)
        {
            var guid = link?.TargetURL?.FirstOrDefault()?.SystemFields?.WebPageItemGUID ?? Guid.Empty;

            footerPages.Add(new FooterLinkDto
            {
                OpenInNewTab = link?.OpenInNewTab ?? false,
                Order = order++,
                Title = link?.Title ?? string.Empty,
                URL = guid != Guid.Empty && pageUrlMap.TryGetValue(guid, out var url) ? url : string.Empty
            });
        }
        return footerPages;
    }

    private async Task<IEnumerable<SocialButtonDto?>> GetSocialLinks()
    {
        var socialLinksPages = (await _contentService.GetItemsChannelAsync<SocialLinks>())?.Take(8) ?? Enumerable.Empty<SocialLinks>();
        int order = 1;

        return socialLinksPages
            .Select(link => new SocialButtonDto
            {
                AltText = link?.Alttext ?? string.Empty,
                AriaLabel = link?.AriaLabel ?? string.Empty,
                IconType = link?.Icontype ?? string.Empty,
                Order = order++,
                URL = link?.Url?.ToString() ?? "#"
            })
            .OrderBy(x => x.Order)
            .ToList();
    }

}
