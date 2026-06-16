using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Header;
using CMS.Websites.Routing;
using Kentico.Content.Web.Mvc;
using Microsoft.AspNetCore.Http;

namespace BDO.GWT.Core.Services.Header;

public interface IMicrositeHeaderService
{
    Task<HeaderDto?> GetAsync(IContentService contentService, CancellationToken cancellationToken);
}

public class MicrositeHeaderService : IMicrositeHeaderService
{
    private readonly IContentService _contentService;
    private readonly IHomePageService _homePageService;
    private readonly IWebsiteCulturesService _websiteCulturesService;
    private readonly IWebsiteChannelContext _websiteChannel;
    private readonly ILocalizedUrlsBuilder _localizedUrlsBuilder;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IWebPageDataContextRetriever _dataContextRetriever;
    private readonly IWebPageUrlResolver _urlResolver;
    private readonly IPageContext _pageContext;
    private readonly IHeaderService _headerService;

    private const string BdoLogoImageUrl = "https://cdn.bdo.global/images/bdo_logo/1.0.0/bdo_logo_color.png";

    public MicrositeHeaderService(
        IContentService contentService,
        IHomePageService homePageService,
        IWebsiteCulturesService websiteCulturesService,
        IWebsiteChannelContext websiteChannel,
        ILocalizedUrlsBuilder localizedUrlsBuilder,
        IHttpContextAccessor httpContextAccessor,
        IWebPageDataContextRetriever dataContextRetriever,
        IWebPageUrlResolver urlResolver,
        IPageContext pageContext,
        IHeaderService headerService)
    {
        _contentService = contentService;
        _homePageService = homePageService;
        _websiteCulturesService = websiteCulturesService;
        _websiteChannel = websiteChannel;
        _localizedUrlsBuilder = localizedUrlsBuilder;
        _httpContextAccessor = httpContextAccessor;
        _dataContextRetriever = dataContextRetriever;
        _urlResolver = urlResolver;
        _pageContext = pageContext;
        _headerService = headerService;
    }

    public async Task<HeaderDto?> GetAsync(IContentService contentService, CancellationToken cancellationToken)
    {
        var masterPage = await GetMicrositeMasterPageAsync(cancellationToken);

        // If menu type is set to member firm menu, delegate to the standard header service
        if (masterPage is not null &&
            string.Equals(masterPage.SettingsMenuType, "mfmenu", StringComparison.OrdinalIgnoreCase))
        {
            return await _headerService.GetAsync(contentService, cancellationToken);
        }

        var cultures = await GetCultureDtosAsync();

        var siteName = masterPage is not null && !string.IsNullOrEmpty(masterPage.LogoText)
            ? masterPage.LogoText
            : _websiteCulturesService.GetDisplayName(_websiteChannel);

        var logoPageLink = await GetLogoPageLinkAsync(masterPage, cancellationToken);

        var megaMenu = await BuildMegaMenuAsync(masterPage, cancellationToken);

        return new HeaderDto
        {
            MenuItems = Enumerable.Empty<LinkItemDto>(),
            Cultures = cultures,
            Contact = null,
            SiteName = siteName,
            LogoURL = logoPageLink,
            LogoImageURL = BdoLogoImageUrl,
            SearchPageURL = string.Empty,
            MegaMenuItems = megaMenu,
        };
    }

    private async Task<string> GetLogoPageLinkAsync(
        MicrositeMasterPage? masterPage,
        CancellationToken cancellationToken)
    {
        if (masterPage is null || masterPage.LogoLinkTarget != "microsite")
            return await GetHomePageUrlAsync(cancellationToken);

        if (!_dataContextRetriever.TryRetrieve(out var ctx))
            return await GetHomePageUrlAsync(cancellationToken);

        var url = await _urlResolver.ResolvePageUrlAsync(
            masterPage.SystemFields.WebPageItemID,
            ctx.WebPage.LanguageName,
            cancellationToken);

        return url != "#" ? url : await GetHomePageUrlAsync(cancellationToken);
    }

    private async Task<string> GetHomePageUrlAsync(CancellationToken cancellationToken)
    {
        var homePage = await _homePageService.GetHomePageUrl(cancellationToken);
        return homePage ?? string.Empty;
    }

    // ── Mega menu ─────────────────────────────────────────────────────────────
 
    private async Task<IEnumerable<FirstLevelMenuItemDto>> BuildMegaMenuAsync(
        MicrositeMasterPage? masterPage,
        CancellationToken cancellationToken)
    {
        if (masterPage is null)
            return Enumerable.Empty<FirstLevelMenuItemDto>();

        if (!_dataContextRetriever.TryRetrieve(out var ctx))
            return Enumerable.Empty<FirstLevelMenuItemDto>();

        var masterPageId = masterPage.SystemFields.WebPageItemID;
        var language = ctx.WebPage.LanguageName;

        // Level 1 — direct children of MicrositeMasterPage
        var level1Pages = (await _contentService.GetChildPagesByParentGuidAsync<MicrositePage>(
            masterPageId, cancellationToken)).ToList();

        if (level1Pages.Count == 0)
            return Enumerable.Empty<FirstLevelMenuItemDto>();

        var menuItems = new List<FirstLevelMenuItemDto>();

        foreach (var l1Page in level1Pages)
        {
            var l1Id = l1Page.SystemFields.WebPageItemID;

            // Level 2 — children of each Level 1 page
            var level2Pages = (await _contentService.GetChildPagesByParentGuidAsync<MicrositePage>(
                l1Id, cancellationToken)).ToList();

            var l2Items = new List<SecondLevelMenuItemDto>();

            foreach (var l2Page in level2Pages)
            {
                var l2Id = l2Page.SystemFields.WebPageItemID;

                // Level 3 — children of each Level 2 page
                var level3Pages = (await _contentService.GetChildPagesByParentGuidAsync<MicrositePage>(
                    l2Id, cancellationToken)).ToList();

                var l3Items = new List<ThirdLevelMenuItemDto>();
                foreach (var l3Page in level3Pages)
                {
                    l3Items.Add(new ThirdLevelMenuItemDto
                    {
                        Title = ResolveMicrositePageTitle(l3Page),
                        URL = await _urlResolver.ResolvePageUrlAsync(l3Page.SystemFields.WebPageItemID, language, cancellationToken),
                    });
                }

                l2Items.Add(new SecondLevelMenuItemDto
                {
                    Title = ResolveMicrositePageTitle(l2Page),
                    URL = await _urlResolver.ResolvePageUrlAsync(l2Id, language, cancellationToken),
                    Children = l3Items.ToArray(),
                });
            }

            menuItems.Add(new FirstLevelMenuItemDto
            {
                Title = ResolveMicrositePageTitle(l1Page),
                URL = await _urlResolver.ResolvePageUrlAsync(l1Id, language, cancellationToken),
                Featured = Enumerable.Empty<FeaturedMenuItemDto>(),
                Children = l2Items.ToArray(),
            });
        }

        return menuItems;
    }


    private static string ResolveMicrositePageTitle(MicrositePage page)
    {
        if (!string.IsNullOrEmpty(page.PageTitle)) return page.PageTitle;
        if (!string.IsNullOrEmpty(page.MetadataTitle)) return page.MetadataTitle;
        return page.SystemFields.WebPageItemName;
    }

    // ── Cultures 

    private async Task<IEnumerable<CultureDto>> GetCultureDtosAsync()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext is null)
            return Enumerable.Empty<CultureDto>();

        var localizedUrls = await _localizedUrlsBuilder.BuildAsync(httpContext);

        return localizedUrls.Select(u => new CultureDto
        {
            Name = u.DisplayName,
            ShortName = u.ShortCode,
            Url = u.RelativeUrl,
        }).ToList();
    }

    // ── Master page 
    // Walks up the ancestor tree from the current page until it finds a MicrositeMasterPage.
    // This ensures the header is the same on the master page itself AND all its child pages.
    private async Task<MicrositeMasterPage?> GetMicrositeMasterPageAsync(CancellationToken cancellationToken)
    {
        if (!_dataContextRetriever.TryRetrieve(out var ctx))
            return null;

        if (ctx.WebPage.ContentTypeName == MicrositeMasterPage.CONTENT_TYPE_NAME)
            return await _contentService.RetrieveCurrentPageAsync<MicrositeMasterPage>(
                new RetrieveCurrentPageParameters { LinkedItemsMaxLevel = 1 },
                cancellationToken);

        var masterPages = (await _contentService.RetrievePagesAsync<MicrositeMasterPage>(cancellationToken))?
            .OfType<MicrositeMasterPage>()
            .ToDictionary(p => p.SystemFields.WebPageItemID) ?? new Dictionary<int, MicrositeMasterPage>();

        var page = await _pageContext.GetCurrentPageBasePropertiesAsync(cancellationToken);
        var parentId = page.WebPageItemParentID;

        while (parentId > 0)
        {
            if (masterPages.TryGetValue(parentId, out var masterPage))
                return masterPage;

            var ancestor = await _pageContext.GetAnyPageByWebPageItemId(parentId, cancellationToken);
            parentId = ancestor?.SystemFields.WebPageItemParentID ?? 0;
        }

        return null;
    }

    }