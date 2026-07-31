using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Footer;
using BDO.GWT.Core.ValueObjects;
using CMS.ContentEngine;
using CMS.DataEngine;
using CMS.Websites;
using Kentico.Content.Web.Mvc;
using Microsoft.AspNetCore.Http;
using System.Text.RegularExpressions;

namespace BDO.GWT.Core.Services.Footer;

public interface IFooterService
{
    Task<FooterDto?> GetAsync(CancellationToken cancellationToken);

    Task<FooterDto?> GetAsyncForAPI(int channelId, string? culture, CancellationToken cancellationToken);
}

public class FooterService : IFooterService
{
    private readonly IContentService _contentService;
    private readonly IInfoProvider<ChannelInfo> _channelInfoProvider;
    private readonly IWebPageUrlResolver _urlResolver;
    private static readonly LanguageTag defaultCulture = LanguageTag.GB;
    private const string MandatoryFooterRootPath = "/";
    public FooterService(IContentService contentService, IInfoProvider<ChannelInfo> channelInfoProvider, IWebPageUrlResolver urlResolver)
    {
        _contentService = contentService;
        _channelInfoProvider = channelInfoProvider;
        _urlResolver = urlResolver;
    }

    public async Task<FooterDto?> GetAsync(CancellationToken cancellationToken)
    {
        var mandatoryFooterLinks =
            await GetMandatoryFooterLinkDtosAsync(cancellationToken);

        var footerLinks =
            await GetFooterLinks() ?? Enumerable.Empty<FooterLinkDto>();

        var socialLinks =
            await GetSocialLinks() ?? Enumerable.Empty<SocialButtonDto>();

        var subscribeLink =
            (await GetSubscribePageLinkAsync(cancellationToken))
            ?.ToHtmlCompatibleString();

#pragma warning disable CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
        return await BuildFooterDtoAsync(
            mandatoryFooterLinks,
            footerLinks,
            socialLinks,
            subscribeLink);
#pragma warning restore CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
    }


    public async Task<FooterDto?> GetAsyncForAPI(
    int channelId,
    string? culture,
    CancellationToken cancellationToken)
    {
        var channel = await _channelInfoProvider.GetAsync(channelId);

        var mandatoryFooterLinks =
            await GetMandatoryFooterLinkDtosForAPIAsync(
                channelId,
                channel.ChannelName,
                culture,
                cancellationToken);

        culture ??= defaultCulture.ToString();

        var footerLinks =
            await GetFooterLinks(channelId, culture)
            ?? Enumerable.Empty<FooterLinkDto>();

        var socialLinks =
            await GetSocialLinks(channelId, culture)
            ?? Enumerable.Empty<SocialButtonDto>();

        var subscribeLink =
            (await GetSubscribePageLinkAsyncAPI(
                channel.ChannelName,
                culture,
                cancellationToken))
            ?.ToHtmlCompatibleString();
#pragma warning disable CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
        return await BuildFooterDtoAsync(
            mandatoryFooterLinks,
            footerLinks,
            socialLinks,
            subscribeLink);
#pragma warning restore CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
    }

    private async Task<Link?> GetSubscribePageLinkAsync(CancellationToken cancellationToken)
    {
        var subscribePages = await _contentService.GetContentType<Subscribe>(cancellationToken);
        return BuildSubscribeLink(subscribePages);
    }

    private async Task<Link?> GetSubscribePageLinkAsyncAPI(string channelName, string? cultureName, CancellationToken cancellationToken)
    {
        var subscribePages = await _contentService.GetContentTypeForAPI<Subscribe>(channelName, cultureName, cancellationToken);
        return BuildSubscribeLink(subscribePages);
    }

    private static Link? BuildSubscribeLink(IEnumerable<Subscribe>? subscribePages)
    {
        var subscribePage = subscribePages?.FirstOrDefault();
        if (subscribePage == null)
            return null;

        var url = subscribePage.GetUrl()?.AbsoluteUrl;
        if (string.IsNullOrEmpty(url))
            return null;

        return Link.TryParse(url, out var link) ? link : null;
    }

    private async Task<List<FooterLinkDto>> GetMandatoryFooterLinkDtosAsync(
    CancellationToken cancellationToken)
    {
        var pages = (await _contentService.GetPagesAsync<FooterPages>(
            new RetrievePagesParameters
            {
                PathMatch = PathMatch.Children(MandatoryFooterRootPath, 1),
                LinkedItemsMaxLevel = 2
            },
            query => query.OrderBy(
                nameof(IWebPageContentQueryDataContainer.WebPageItemOrder)),
            RetrievalCacheSettings.CacheDisabled,
            cancellationToken))
            ?.ToList() ?? [];

        return MapMandatoryFooterLinks(pages);
    }

    private async Task<List<FooterLinkDto>> GetMandatoryFooterLinkDtosForAPIAsync(
    int channelId,
    string channelName,
    string? culture,
    CancellationToken cancellationToken)
    {
        var pages = (await _contentService.GetPagesAsync<FooterPages>(
            channelId: channelId,
            new RetrievePagesParameters
            {
                LinkedItemsMaxLevel = 2,
                ChannelName = channelName,
                LanguageName = culture ?? defaultCulture.Language,
                IsForPreview = false,
                UseLanguageFallbacks = false,
                PathMatch = PathMatch.Children(MandatoryFooterRootPath, 1)
            },
            query => query.OrderBy(
                nameof(IWebPageContentQueryDataContainer.WebPageItemOrder)),
            RetrievalCacheSettings.CacheDisabled,
            cancellationToken))
            ?.ToList() ?? [];

        return MapMandatoryFooterLinks(pages);
    }    

    private List<FooterLinkDto> MapMandatoryFooterLinks(IEnumerable<FooterPages> pages)
    {
        return pages
            .Where(p => p.FooterLink != null)
            .SelectMany(p => p.FooterLink ?? Enumerable.Empty<IContentItemFieldsSource>())
            .OfType<IWebPageFieldsSource>()   // keep only web pages (they have a URL)
            .Select(page => new FooterLinkDto
            {
                Title = (page as IMetadata)?.MetadataTitle ?? string.Empty,
                URL = _urlResolver.GetRelativePath(page) ?? string.Empty
            })
            .ToList();
    }

    private async Task<IEnumerable<FooterLinkDto?>> GetFooterLinks()
    {
        var footerLinksPages =
            await _contentService.GetItemsChannelAsync<FooterLinks>()
            ?? Enumerable.Empty<FooterLinks>();

        return await BuildFooterLinksAsync(footerLinksPages);
    }

    private async Task<IEnumerable<FooterLinkDto?>> GetFooterLinks(
    int channelId,
    string cultureName)
    {
        var footerLinksPages =
            await _contentService.GetItemsChannelOnChannelIdAsync<FooterLinks>(
                cultureName: cultureName,
                channelId: channelId)
            ?? Enumerable.Empty<FooterLinks>();

        return await BuildFooterLinksAsync(footerLinksPages);
    }

    private async Task<IEnumerable<SocialButtonDto?>> GetSocialLinks()
    {
        var socialLinksPages =
            (await _contentService.GetItemsChannelAsync<SocialLinks>())?.Take(8)
            ?? Enumerable.Empty<SocialLinks>();

        return MapSocialButtons(socialLinksPages);
    }

    private async Task<IEnumerable<SocialButtonDto?>> GetSocialLinks(int channelId, string cultureName)
    {
        var socialLinksPages =
            (await _contentService.GetItemsChannelOnChannelIdAsync<SocialLinks>(cultureName: cultureName, channelId: channelId))?.Take(8)
            ?? Enumerable.Empty<SocialLinks>();

        return MapSocialButtons(socialLinksPages);
    }

    private static List<SocialButtonDto> MapSocialButtons(IEnumerable<SocialLinks> socialLinksPages)
    {
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

    private static Task<FooterDto> BuildFooterDtoAsync(
    IEnumerable<FooterLinkDto> mandatoryFooterLinks,
    IEnumerable<FooterLinkDto>? footerLinks,
    IEnumerable<SocialButtonDto>? socialLinks,
    string? subscriptionButtonLink)
    {
        return Task.FromResult(new FooterDto
        {
            DisplaySubscriptionsSection = true,
            SubscriptionButtonLink = subscriptionButtonLink,
            FooterLinks = mandatoryFooterLinks
                .Concat((footerLinks ?? Enumerable.Empty<FooterLinkDto>())
                    .Select(link => new FooterLinkDto
                    {
                        OpenInNewTab = link?.OpenInNewTab ?? false,
                        Order = link?.Order ?? 0,
                        Title = link?.Title ?? string.Empty,
                        URL = link?.URL?.ToString() ?? string.Empty,
                    })
                    .OrderBy(o => o.Order))
                .Take(12)
                .ToList(),

            SocialButtons = (socialLinks ?? Enumerable.Empty<SocialButtonDto>())
                .Select(link => new SocialButtonDto
                {
                    AltText = link?.AltText ?? string.Empty,
                    AriaLabel = link?.AriaLabel ?? string.Empty,
                    IconType = link?.IconType ?? string.Empty,
                    Order = link?.Order ?? 0,
                    URL = link?.URL?.ToString() ?? string.Empty,
                })
                .OrderBy(o => o.Order)
                .Take(5)
                .ToList()
        });
    }

    private async Task<IEnumerable<FooterLinkDto?>> BuildFooterLinksAsync(
    IEnumerable<FooterLinks> footerLinksPages)
    {
        var pages = footerLinksPages.Take(8).ToList();

        var allGuids = pages
            .Where(x => x.TargetURL != null)
            .SelectMany(x =>
                x.TargetURL?.Select(t => t.SystemFields?.WebPageItemGUID ?? Guid.Empty)
                ?? Enumerable.Empty<Guid>())
            .Where(guid => guid != Guid.Empty)
            .Distinct()
            .ToList();

        var selectedPages = allGuids.Any()
            ? await _contentService.GetItemsByGUIDAsync(allGuids)
            : Enumerable.Empty<IWebPageFieldsSource>();

        var pageUrlMap = selectedPages.ToDictionary(
            p => p.SystemFields?.WebPageItemGUID ?? Guid.Empty,
            p => p.GetUrl()?.RelativePath ?? string.Empty);

        int order = 1;

        return pages.Select(link =>
        {
            var guid = link?.TargetURL?.FirstOrDefault()?.SystemFields?.WebPageItemGUID
                       ?? Guid.Empty;

            return new FooterLinkDto
            {
                OpenInNewTab = link?.OpenInNewTab ?? false,
                Order = order++,
                Title = link?.Title ?? string.Empty,
                URL = guid != Guid.Empty &&
                      pageUrlMap.TryGetValue(guid, out var url)
                        ? url
                        : string.Empty
            };
        }).ToList();
    }
}
