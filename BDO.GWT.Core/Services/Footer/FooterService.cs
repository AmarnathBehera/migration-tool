using BDO.GWT.Core.DTOs.Footer;
using BDO.GWT.Core.ValueObjects;
using CMS.Websites;

namespace BDO.GWT.Core.Services.Footer;

public interface IFooterService
{
    Task<FooterDto> GetAsync(CancellationToken cancellationToken);
}

public class FooterService : IFooterService
{
    private static readonly (Type PageType, int Order)[] MandatoryPageTypesWithOrders = {
        (typeof(Contact), -3),
        (typeof(Locations), -2),
        (typeof(LegalPage), -1),
        (typeof(Sitemap), 0),
    };

    private readonly IContentService _contentService;

    public FooterService(IContentService contentService)
    {
        _contentService = contentService;
    }

    public async Task<FooterDto> GetAsync(CancellationToken cancellationToken)
    {
        var mandatoryFooterLinks = await GetMandatoryFooterLinkDtosAsync(cancellationToken);

        var footerLinks = await GetFooterLinks();
        var socialLinks = await GetSocialLinks();

        //var displaySubscriptionsSectionSetting = await _settingsService.GetCurrentSiteSettingAsync(SettingName.BDO.DisplaySubscriptionsSection, cancellationToken);

        var footerDto = new FooterDto
        {
            DisplaySubscriptionsSection = true,//(bool?)displaySubscriptionsSectionSetting?.Value ?? false,
            SubscriptionButtonLink = (await GetSubscribePageLinkAsync(cancellationToken))?.ToHtmlCompatibleString(),
            FooterLinks = mandatoryFooterLinks.Concat(footerLinks
                .Select(link => new FooterLinkDto
                {
                    OpenInNewTab = link.OpenInNewTab,
                    Order = link.Order,
                    Title = link.Title,
                    URL = link.URL.ToString(),
                })
                .OrderBy(o => o.Order)
            ).Take(12).ToList(),
            SocialButtons = socialLinks.Select(link => new SocialButtonDto
            {
                AltText = link.AltText,
                AriaLabel = link.AriaLabel,
                IconType = link.IconType,
                //     CustomIcon = link.IconType == SocialLinkIconType.Custom ? link.CustomIcon : null,
                Order = link.Order,
                URL = link.URL.ToString(),
            }).OrderBy(o => o.Order).Take(5).ToList(),
        };

        return footerDto;
    }

    private async Task<Link?> GetSubscribePageLinkAsync(CancellationToken cancellationToken)
    {
        var subscribePages = await _contentService.GetContentType<Subscribe>(cancellationToken);
        var subscribePage = subscribePages.FirstOrDefault();
        if (subscribePage is null)
        {
            return null;
        }
        var url = subscribePage.GetUrl()?.AbsoluteUrl;
        if (string.IsNullOrEmpty(url))
        {
            return null;
        }
        if (Link.TryParse(url, out var link))
        {
            return link;
        }
        return null;
    }

    private async Task<List<FooterLinkDto>> GetMandatoryFooterLinkDtosAsync(CancellationToken cancellationToken)
    {
        var typeList = MandatoryPageTypesWithOrders.ToList();

        var pages = (await _contentService
            .RetrieveContentItemsOfContentTypes(typeList.Select(t => t.PageType).ToArray()))
            .ToList();
        var typeOrderMap = MandatoryPageTypesWithOrders.ToDictionary(t => t.PageType.GetField("CONTENT_TYPE_NAME")?
                                                                                    .GetValue(null)?.ToString(),
                                                                                     t => t.Order);
        var result = pages.Select(page =>
        {
            return new FooterLinkDto
            {
                Title = page.MetadataTitle ?? string.Empty,
                OpenInNewTab = false,
                Order = page.ContentTypeName != null && typeOrderMap.TryGetValue(page.ContentTypeName, out var order) ? order : 0,
                URL = page.SystemFields.WebPageUrlPath?.ToString() ?? string.Empty
            };
        })
        .OrderBy(x => x.Order)
        .ToList();

        return result;
    }
    private async Task<IEnumerable<FooterLinkDto>> GetFooterLinks()
    {
        var footerLinksPages = (await _contentService.GetItemsAsync<FooterLinks>())
            .Take(8)
            .ToList();

        var footerPages = new List<FooterLinkDto>();

        var allGuids = footerLinksPages
            .Where(x => x.TargetURL != null)
            .SelectMany(x => x.TargetURL.Select(t => t.WebPageGuid))
            .Distinct()
            .ToList();

        var selectedPages = await _contentService.GetItemsByGUIDAsync(allGuids);

        var pageUrlMap = selectedPages.ToDictionary(
            p => p.SystemFields.WebPageItemGUID,
            p => p.GetUrl()?.RelativePath ?? string.Empty
        );
        int order = -4;
        foreach (var link in footerLinksPages)
        {
            var guid = link.TargetURL?.FirstOrDefault()?.WebPageGuid;

            footerPages.Add(new FooterLinkDto
            {
                OpenInNewTab = link.OpenInNewTab,
                Order = order--,
                Title = link.Title,
                URL = guid != null && pageUrlMap.ContainsKey(guid.Value)
                    ? pageUrlMap[guid.Value]
                    : string.Empty
            });
        }
        return footerPages;
    }
    private async Task<IEnumerable<SocialButtonDto>> GetSocialLinks()
    {
        var socialLinksPages = (await _contentService.GetItemsAsync<SocialLinks>())
            .Take(8);
        int order = 1;
        return socialLinksPages
            .Select(link => new SocialButtonDto
            {
                AltText = link.Alttext,
                AriaLabel = link.AriaLabel,
                IconType = link.Icontype,
                //CustomIcon = link.IconType == "Custom" ? link.CustomIcon : null,
                Order = order++,
                URL = link.Url?.ToString() ?? "#"
            })
            .OrderBy(x => x.Order)
            .ToList();
    }
}
