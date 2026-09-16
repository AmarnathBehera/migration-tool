using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Header;
using CMS.Helpers;
using CMS.Websites;
using CMS.Websites.Routing;
using Kentico.Content.Web.Mvc;
using Kentico.Content.Web.Mvc.Routing;
using Microsoft.AspNetCore.Http;

namespace BDO.GWT.Core.Services.Header;

public interface IHeaderService
{
    Task<HeaderDto?> GetAsync(IContentService contentservice, CancellationToken cancellationToken);
    Task<IEnumerable<CultureDto>> GetCultureDtosAsync(CancellationToken cancellationToken);
}

public class HeaderService : IHeaderService
{
    private readonly IContentService _contentService;
    private readonly IContentRetriever _contentRetriever;
    public readonly IHomePageService _homePageService;
    private readonly IWebsiteCulturesService _websiteCulturesService;
    private readonly IWebsiteChannelContext _websiteChannel;
    private readonly ILocalizedUrlsBuilder _localizedUrlsBuilder;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IProgressiveCache _cache;
    private readonly IPreferredLanguageRetriever _preferredLanguageRetriever;
    private readonly IWebPageUrlResolver _urlResolver;
    private readonly string navRootPath = "/";

    public HeaderService(
        IContentService contentService,
        IContentRetriever contentRetriever,
        IHomePageService homePageService,
        IWebsiteCulturesService websiteCulturesService,
        IWebsiteChannelContext websiteChannel,
        ILocalizedUrlsBuilder localizedUrlsBuilder,
        IHttpContextAccessor httpContextAccessor,
        IProgressiveCache cache,
        IPreferredLanguageRetriever preferredLanguageRetriever,
        IWebPageUrlResolver urlResolver)
    {
        _contentService = contentService;
        _contentRetriever = contentRetriever;
        _homePageService = homePageService;
        _websiteCulturesService = websiteCulturesService;
        _websiteChannel = websiteChannel;
        _localizedUrlsBuilder = localizedUrlsBuilder;
        _httpContextAccessor = httpContextAccessor;
        _cache = cache;
        _preferredLanguageRetriever = preferredLanguageRetriever;
        _urlResolver = urlResolver;
    }

    public async Task<HeaderDto?> GetAsync(IContentService contentservice, CancellationToken cancellationToken)
    {
        // The header (menus, contact, etc.) is language-specific, so the current
        // culture MUST be part of the cache key. Without it, the first language to
        // populate the cache is served to every other language on the channel.
        var languageName = _preferredLanguageRetriever.Get();
        var cacheSettings = new CacheSettings(
            cacheMinutes: 10,
            cacheItemNameParts:
            [
                nameof(HeaderService),
                _websiteChannel.WebsiteChannelName,
                languageName // language part – header varies by culture
            ])
        {
            GetCacheDependency = () => CacheHelper.GetCacheDependency(new[]
            {
                $"webpageitem|bychannel|{_websiteChannel.WebsiteChannelName}"
                // optionally narrow to the navigation content types
            })
        };

        var header = await _cache.LoadAsync(
            async _ => await BuildStaticHeaderAsync(cancellationToken),
            cacheSettings);

        if (header is null)
        {
            return null;
        }

        header.Cultures = await GetCultureDtosAsync(cancellationToken);
        return header;
    }

    private async Task<HeaderDto?> BuildStaticHeaderAsync(CancellationToken cancellationToken)
    {
        var topMenu = await BuildTopMenuAsync(cancellationToken);
        var websiteDisplayName = _websiteCulturesService.GetDisplayName(_websiteChannel);
        var megaMenu = await BuildMegaMenuAsync(cancellationToken);
        var logoPageLink = await GetLogoPageLinkAsync(cancellationToken);

        var searchPage = (await _contentRetriever.RetrievePages<Search>(
            new RetrievePagesParameters { PathMatch = PathMatch.Children(navRootPath, 1) },
            query => query.OrderBy(nameof(IWebPageContentQueryDataContainer.WebPageItemOrder)),
            RetrievalCacheSettings.CacheDisabled,
            cancellationToken
        ))?.FirstOrDefault();

        return new HeaderDto
        {
            MenuItems = topMenu.TopMenu,
            Contact = topMenu.Contact,
            SiteName = websiteDisplayName,
            LogoURL = logoPageLink ?? string.Empty,
            LogoImageURL = "https://cdn.bdo.global/images/bdo_logo/1.0.0/bdo_logo_color.png",
            SearchPageURL = searchPage != null ? _urlResolver.GetRelativePath(searchPage) : string.Empty,
            MegaMenuItems = megaMenu
            // Cultures intentionally NOT set here
        };
    }

    protected virtual async Task<string> GetLogoPageLinkAsync(CancellationToken cancellationToken)
    {
        var homePage = await _homePageService.GetHomePageUrl(cancellationToken);
        return homePage ?? string.Empty;
    }

    public async Task<IEnumerable<CultureDto>> GetCultureDtosAsync(CancellationToken cancellationToken)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext is null)
        {
            return Enumerable.Empty<CultureDto>();
        }

        var localizedUrls = await _localizedUrlsBuilder.BuildAsync(httpContext);

        return localizedUrls.Select(u => new CultureDto
        {
            Name = u.DisplayName,
            ShortName = u.ShortCode,
            Url = u.RelativeUrl,
        }).ToList();
    }

    private async Task<(IEnumerable<LinkItemDto> TopMenu, LinkItemDto? Contact)> BuildTopMenuAsync(CancellationToken cancellationToken)
    {
        var pages = (await _contentRetriever.RetrievePages<Navigation>(
            new RetrievePagesParameters
            {
                PathMatch = PathMatch.Children(navRootPath, 1),
                LinkedItemsMaxLevel = 2   // <-- required so p.TopMenu (linked pages) is populated
            },
            query => query.OrderBy(nameof(IWebPageContentQueryDataContainer.WebPageItemOrder)),
            RetrievalCacheSettings.CacheDisabled,
            cancellationToken
        ))?.ToList() ?? new List<Navigation>();

        if (pages.Count == 0)
        {
            return (Enumerable.Empty<LinkItemDto>(), null);
        }

        var topMenuItems = MapTopMenuLinks(pages);

        if (topMenuItems.Count == 0)
        {
            return (Enumerable.Empty<LinkItemDto>(), null);
        }

        var contactPage = (await _contentRetriever.RetrievePages<Contact>(
            new RetrievePagesParameters { PathMatch = PathMatch.Children(navRootPath, 1) },
            query => query.OrderBy(nameof(IWebPageContentQueryDataContainer.WebPageItemOrder)),
            RetrievalCacheSettings.CacheDisabled,
            cancellationToken
        ))?.FirstOrDefault();

        var linkItemDto = new LinkItemDto
        {
            Title = contactPage?.MetadataTitle ?? string.Empty,
            URL = contactPage?.GetUrl()?.RelativePath ?? string.Empty
        };
        return (topMenuItems, linkItemDto);
    }

    private List<LinkItemDto> MapTopMenuLinks(IEnumerable<Navigation> pages)
    {
        return pages
            .Where(p => p.TopMenu != null)
            .SelectMany(p => p.TopMenu ?? Enumerable.Empty<IWebPageFieldsSource>())
            .Select(page => new LinkItemDto
            {
                Title = (page as IMetadata)?.MetadataTitle ?? string.Empty,
                URL = _urlResolver.GetRelativePath(page) ?? string.Empty
            })
            .ToList();
    }

    protected virtual async Task<IEnumerable<FirstLevelMenuItemDto>> BuildMegaMenuAsync(CancellationToken cancellationToken = default)
    {
        var firstLevelItems = (await _contentRetriever.RetrievePages<NavigationFirstLevelItem>(
            new RetrievePagesParameters
            {
                PathMatch = PathMatch.Children(navRootPath, 1),
                LinkedItemsMaxLevel = 2   // <-- required so p.TopMenu (linked pages) is populated
            },
            query => query.OrderBy(nameof(IWebPageContentQueryDataContainer.WebPageItemOrder)),
            RetrievalCacheSettings.CacheDisabled,
            cancellationToken
        ))?.ToList() ?? new List<NavigationFirstLevelItem>();

        var megaMenu = new List<FirstLevelMenuItemDto>();

        if (firstLevelItems.Count == 0)
        {
            return megaMenu;
        }

        foreach (var first in firstLevelItems.Where(f => f != null))
        {
            var secondLevelDtos = await BuildSecondLevelAsync(first.SystemFields?.WebPageItemID ?? 0, cancellationToken);
            var featuredDtos = await BuildFeaturedLevelAsync(first.SystemFields?.WebPageItemID ?? 0, cancellationToken);
            var targetPage = first.TargetURL?
                .OfType<IWebPageFieldsSource>()
                .FirstOrDefault();

            megaMenu.Add(new FirstLevelMenuItemDto
            {
                Title = first.Title ?? string.Empty,
                Children = secondLevelDtos,
                Featured = featuredDtos,
                URL = targetPage != null
                    ? _urlResolver.GetRelativePath(targetPage) ?? string.Empty
                    : string.Empty
            });
        }

        return megaMenu;
    }

    private async Task<List<SecondLevelMenuItemDto>> BuildSecondLevelAsync(int parentId, CancellationToken cancellationToken)
    {
        var secondLevelItems = (await _contentService.GetChildPagesByParentGuidAsync<NavigationSecondLevelItem>(parentId, cancellationToken))?.ToList()
                               ?? new List<NavigationSecondLevelItem>();

        var secondLevelDtos = new List<SecondLevelMenuItemDto>();

        if (secondLevelItems.Count == 0)
        {
            return secondLevelDtos;
        }

        foreach (var second in secondLevelItems.Where(s => s != null))
        {
            var thirdLevelDtos = await BuildThirdLevelAsync(second.SystemFields?.WebPageItemID ?? 0);

            secondLevelDtos.Add(new SecondLevelMenuItemDto
            {
                Title = second.Title ?? string.Empty,
                URL = second.TargetURL?.FirstOrDefault()?.GetUrl()?.RelativePath ?? string.Empty,
                Children = thirdLevelDtos
            });
        }

        return secondLevelDtos;
    }

    private async Task<List<ThirdLevelMenuItemDto>> BuildThirdLevelAsync(int parentId)
    {
        var thirdLevelItems = (await _contentService.GetChildPagesByParentGuidAsync<NavigationThirdLevelItem>(parentId))?.ToList()
                              ?? new List<NavigationThirdLevelItem>();

        var thirdLevelDtos = new List<ThirdLevelMenuItemDto>();

        if (thirdLevelItems.Count == 0)
        {
            return thirdLevelDtos;
        }

        foreach (var third in thirdLevelItems.Where(t => t != null))
        {
            thirdLevelDtos.Add(new ThirdLevelMenuItemDto
            {
                Title = third.Title ?? string.Empty,
                URL = third.TargetURL?.FirstOrDefault()?.GetUrl()?.RelativePath ?? string.Empty
            });
        }

        return thirdLevelDtos;
    }

    private async Task<List<FeaturedMenuItemDto>> BuildFeaturedLevelAsync(int parentId, CancellationToken cancellationToken)
    {
        var featuredItems = (await _contentService.GetChildPagesByParentGuidAsync<NavigationFeaturedItem>(parentId))?.ToList()
                            ?? new List<NavigationFeaturedItem>();

        if (featuredItems.Count == 0)
        {
            return new List<FeaturedMenuItemDto>();
        }

        var featuredDtos = new List<FeaturedMenuItemDto>();

        foreach (var featured in featuredItems.Where(f => f != null))
        {
            string imageUrl = string.Empty;
            if (featured.Image != null)
            {
                var firstAttachment = featured.Image.FirstOrDefault();
                if (firstAttachment?.ContentAttachmentAsset != null)
                {
                    imageUrl = firstAttachment.ContentAttachmentAsset.Url ?? string.Empty;
                }
            }

            featuredDtos.Add(new FeaturedMenuItemDto
            {
                Title = featured.Title ?? string.Empty,
                Description = featured.Description ?? string.Empty,
                URL = featured.TargetURL?.FirstOrDefault()?.GetUrl()?.RelativePath ?? string.Empty,
                ImageURL = imageUrl.TrimStart('~'),
                ImageAltText = featured.ImageAltText,
            });
        }

        return featuredDtos;
    }
}