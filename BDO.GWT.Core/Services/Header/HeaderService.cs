using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Header;
using CMS.Websites;
using CMS.Websites.Routing;
using Kentico.Content.Web.Mvc;
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

    public HeaderService(
        IContentService contentService,
        IContentRetriever contentRetriever,
        IHomePageService homePageService,
        IWebsiteCulturesService websiteCulturesService,
        IWebsiteChannelContext websiteChannel,
        ILocalizedUrlsBuilder localizedUrlsBuilder,
        IHttpContextAccessor httpContextAccessor)
    {
        _contentService = contentService;
        _contentRetriever = contentRetriever;
        _homePageService = homePageService;
        _websiteCulturesService = websiteCulturesService;
        _websiteChannel = websiteChannel;
        _localizedUrlsBuilder = localizedUrlsBuilder;
        _httpContextAccessor = httpContextAccessor;
    }
    public async Task<HeaderDto> GetAsync(IContentService contentservice, CancellationToken cancellationToken)
    {
        var topMenu = await BuildTopMenuAsync(cancellationToken);

        var cultures = await GetCultureDtosAsync(cancellationToken);

        var websiteDisplayName = _websiteCulturesService.GetDisplayName(_websiteChannel);

        var megaMenu = await BuildMegaMenuAsync(cancellationToken);

        //var logoImageUrl = await _currentWebsiteHelper.GetCurrentWebsiteLogoImageUrlAsync(cancellationToken);

        var logoPageLink = await GetLogoPageLinkAsync(cancellationToken);

        //var searchPage = await _searchPageRetriever.RetrievePageAsync(query =>
        //{
        //    query.IncludeUrlPath();

        //}, cancellationToken);

        var headerDto = new HeaderDto
        {
            MenuItems = topMenu.TopMenu,
            Cultures = cultures,
            Contact = topMenu.Contact,
            SiteName = websiteDisplayName,
            LogoURL = logoPageLink ?? string.Empty,
            LogoImageURL = "https://cdn.bdo.global/images/bdo_logo/1.0.0/bdo_logo_color.png" ?? string.Empty,//logoImageUrl ?? string.Empty,
            SearchPageURL = string.Empty,//searchPage != null ? searchPage.UrlPath.ToHtmlCompatibleString() : string.Empty,
            MegaMenuItems = megaMenu,
        };

        return headerDto;
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
        string navRootPath = "/Navigation";

        var pages = (await _contentRetriever.RetrievePages<Navigation>(
            new RetrievePagesParameters { PathMatch = PathMatch.Children(navRootPath,1) },
            query => query.OrderBy(nameof(IWebPageContentQueryDataContainer.WebPageItemOrder)),
            RetrievalCacheSettings.CacheDisabled,
            cancellationToken
        ))?.ToList() ?? new List<Navigation>();

        var allGuids = pages
            .Where(p => p.TopMenu != null)
            .SelectMany(p => p.TopMenu?.Select(i => i.WebPageGuid) ?? Enumerable.Empty<Guid>())
            .Distinct()
            .ToList();

        var selectedPages = allGuids.Any() ? await _contentService.GetItemsByGUIDAsync(allGuids) : Enumerable.Empty<IWebPageFieldsSource>();

        var topMenuItems = selectedPages
            .Select(item => new LinkItemDto
            {
                Title = (item as IMetadata)?.MetadataTitle ?? string.Empty,
                URL = item?.GetUrl()?.RelativePath?.ToString() ?? string.Empty
            })
            .ToList();

        var contactPage = (await _contentRetriever.RetrievePages<Contact>(
            new RetrievePagesParameters { },
            query => query.OrderBy(nameof(IWebPageContentQueryDataContainer.WebPageItemOrder)),
            RetrievalCacheSettings.CacheDisabled,
            cancellationToken
        ))?.FirstOrDefault();

        var linkItemDto = new LinkItemDto
        {
            Title = contactPage?.MetadataTitle ?? string.Empty,
            URL = contactPage?.GetUrl()?.RelativePath?.ToString() ?? string.Empty
        };

        return (topMenuItems, linkItemDto);
    }
    protected virtual async Task<IEnumerable<FirstLevelMenuItemDto>> BuildMegaMenuAsync(CancellationToken cancellationToken = default)
    {
        var firstLevelItems = (await _contentService?.GetContentType<NavigationFirstLevelItem>(cancellationToken))?.ToList()
                              ?? new List<NavigationFirstLevelItem>();

        var megaMenu = new List<FirstLevelMenuItemDto>();

        foreach (var first in firstLevelItems.Where(f => f != null))
        {
            var firstLevelUrls = first.TargetURL?.Select(u => u?.WebPageGuid ?? Guid.Empty)
                                    .Where(g => g != Guid.Empty)
                                    .ToList() ?? new List<Guid>();

            var firstSelectedPages = firstLevelUrls.Any()
                ? await _contentService.GetItemsByGUIDAsync(firstLevelUrls)
                : Enumerable.Empty<IWebPageFieldsSource>();

            var secondLevelDtos = await BuildSecondLevelAsync(first.SystemFields?.WebPageItemID ?? 0, cancellationToken);

            var featuredDtos = await BuildFeaturedLevelAsync(first.SystemFields?.WebPageItemID ?? 0, cancellationToken);

            megaMenu.Add(new FirstLevelMenuItemDto
            {
                Title = first.Title ?? string.Empty,
                Children = secondLevelDtos,
                Featured = featuredDtos,
                URL = firstSelectedPages.FirstOrDefault()?.GetUrl()?.RelativePath ?? string.Empty
            });
        }

        return megaMenu;
    }

    private async Task<List<SecondLevelMenuItemDto>> BuildSecondLevelAsync(int parentId, CancellationToken cancellationToken)
    {
        var secondLevelItems = (await _contentService.GetChildPagesByParentGuidAsync<NavigationSecondLevelItem>(parentId, cancellationToken))?.ToList()
                               ?? new List<NavigationSecondLevelItem>();

        var secondLevelDtos = new List<SecondLevelMenuItemDto>();

        foreach (var second in secondLevelItems.Where(s => s != null))
        {
            var secondUrls = second.TargetURL?.Select(u => u?.WebPageGuid ?? Guid.Empty).Where(g => g != Guid.Empty).ToList()
                             ?? new List<Guid>();
            var secondSelectedPages = secondUrls.Any()
                ? await _contentRetriever.RetrieveAllPagesByGuids<IWebPageFieldsSource>(secondUrls)
                : Enumerable.Empty<IWebPageFieldsSource>();

            var thirdLevelDtos = await BuildThirdLevelAsync(second.SystemFields?.WebPageItemID ?? 0);

            secondLevelDtos.Add(new SecondLevelMenuItemDto
            {
                Title = second.Title ?? string.Empty,
                URL = secondSelectedPages.FirstOrDefault()?.GetUrl()?.RelativePath ?? string.Empty,
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

        foreach (var third in thirdLevelItems.Where(t => t != null))
        {
            var thirdUrls = third.TargetURL?.Select(u => u?.WebPageGuid ?? Guid.Empty).Where(g => g != Guid.Empty).ToList()
                            ?? new List<Guid>();
            var thirdSelectedPages = thirdUrls.Any()
                ? await _contentRetriever.RetrieveAllPagesByGuids<IWebPageFieldsSource>(thirdUrls)
                : Enumerable.Empty<IWebPageFieldsSource>();

            thirdLevelDtos.Add(new ThirdLevelMenuItemDto
            {
                Title = third.Title ?? string.Empty,
                URL = thirdSelectedPages.FirstOrDefault()?.GetUrl()?.RelativePath ?? string.Empty
            });
        }

        return thirdLevelDtos;
    }

    private async Task<List<FeaturedMenuItemDto>> BuildFeaturedLevelAsync(int parentId, CancellationToken cancellationToken)
    {
        var featuredItems = (await _contentService.GetChildPagesByParentGuidAsync<NavigationFeaturedItem>(parentId))?.ToList()
                            ?? new List<NavigationFeaturedItem>();

        var featuredDtos = new List<FeaturedMenuItemDto>();

        foreach (var featured in featuredItems.Where(f => f != null))
        {
            var featuredUrls = featured.TargetURL?.Select(u => u?.WebPageGuid ?? Guid.Empty).Where(g => g != Guid.Empty).ToList()
                               ?? new List<Guid>();
            var selectedPagesFeatured = featuredUrls.Any()
                ? await _contentRetriever.RetrieveAllPagesByGuids<IWebPageFieldsSource>(featuredUrls)
                : Enumerable.Empty<IWebPageFieldsSource>();


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
                URL = selectedPagesFeatured.FirstOrDefault()?.GetUrl()?.RelativePath ?? string.Empty,
                ImageURL = imageUrl,
                ImageAltText = featured.ImageAltText,
            });
        }

        return featuredDtos;
    }
}


