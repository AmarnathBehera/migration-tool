//using BDO.GWT.Domain.Entities;
//using BDO.GWT.Domain.Entities.Pages;
//using BDO.GWT.Domain.Retrievers;
//using BDO.GWT.Domain.Services;
//using BDO.GWT.Domain.ValueObjects;
//using BDO.GWT.Web.DTOs;
//using BDO.GWT.Web.LiveSite.DTOs;
//using BDO.GWT.Web.LiveSite.Helpers;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading;
//using System.Threading.Tasks;

//namespace BDO.GWT.Web.LiveSite.Services;

//public abstract class HeaderServiceBase
//{
//    private readonly ICultureSwitchService _cultureSwitchService;
//    private readonly IDisplayNameService _displayNameService;
//    private readonly IMultiTypePageRetriever _multiTypePageRetriever;
//    private readonly IPageRetriever<HomePage> _homePageRetriever;
//    private readonly IPageRetriever<Search> _searchPageRetriever;
//    private readonly IShowInMenuService _showInMenuService;
//    private readonly IWebsiteContext _websiteContext;
//    private readonly IWebsiteCulturesService _websiteCulturesService;
//    private readonly ICurrentWebsiteHelper _currentWebsiteHelper;

//    protected HeaderServiceBase(
//        ICultureSwitchService cultureSwitchService,
//        IDisplayNameService displayNameService,
//        IMultiTypePageRetriever multiTypePageRetriever,
//        IPageRetriever<HomePage> homePageRetriever,
//        IPageRetriever<Search> searchPageRetriever,
//        IShowInMenuService showInMenuService,
//        IWebsiteContext websiteContext,
//        IWebsiteCulturesService websiteCulturesService,
//        ICurrentWebsiteHelper currentWebsiteHelper)
//    {
//        _cultureSwitchService = cultureSwitchService;
//        _displayNameService = displayNameService;
//        _multiTypePageRetriever = multiTypePageRetriever;
//        _homePageRetriever = homePageRetriever;
//        _searchPageRetriever = searchPageRetriever;
//        _showInMenuService = showInMenuService;
//        _websiteContext = websiteContext;
//        _websiteCulturesService = websiteCulturesService;
//        _currentWebsiteHelper = currentWebsiteHelper;
//    }

//    public async Task<HeaderDto> GetAsync(ICurrentPageData currentPageData, CancellationToken cancellationToken)
//    {
//        var topMenu = await BuildTopMenuAsync(cancellationToken);

//        var cultures = await GetCultureDtosAsync(currentPageData, cancellationToken);

//        var websiteDisplayName = await GetCurrentWebsiteDisplayNameAsync(cancellationToken);

//        var megaMenu = await BuildMegaMenuAsync(cancellationToken);

//        var logoImageUrl = await _currentWebsiteHelper.GetCurrentWebsiteLogoImageUrlAsync(cancellationToken);
//        var logoPageLink = await GetLogoPageLinkAsync(cancellationToken);

//        var searchPage = await _searchPageRetriever.RetrievePageAsync(query =>
//        {
//            query.IncludeUrlPath();

//        }, cancellationToken);

//        var headerDto = new HeaderDto
//        {
//            MenuItems = topMenu.TopMenu,
//            Cultures = cultures,
//            Contact = topMenu.Contact,
//            SiteName = websiteDisplayName,
//            LogoURL = logoPageLink?.ToHtmlCompatibleString() ?? string.Empty,
//            LogoImageURL = logoImageUrl ?? string.Empty,
//            SearchPageURL = searchPage != null ? searchPage.UrlPath.ToHtmlCompatibleString() : string.Empty,
//            MegaMenuItems = megaMenu,
//        };

//        return headerDto;
//    }

//    protected virtual async Task<Link?> GetLogoPageLinkAsync(CancellationToken cancellationToken)
//    {
//        var homePage = await _homePageRetriever.RetrievePageAsync(query =>
//        {
//            query.IncludeUrlPath();

//        }, cancellationToken);

//        return homePage?.UrlPath;
//    }

//    private async Task<(IEnumerable<LinkItemDto> TopMenu, LinkItemDto? Contact)> BuildTopMenuAsync(CancellationToken cancellationToken)
//    {
//        var retrievedPages = (await _multiTypePageRetriever.RetrievePagesAsync(query =>
//        {
//            query.Types(
//                typeof(Contact),
//                typeof(Locations),
//                typeof(SectionCareers),
//                typeof(SectionEvents),
//                typeof(SectionNews),
//                typeof(SectionPeoplePage)
//            );

//            query.IncludeUrlPath();

//        }, cancellationToken))
//            .ToArray();

//        var displayNameByPage = await _displayNameService.GetDisplayNameByContentItemAsync(retrievedPages, cancellationToken);

//        var contactPage = retrievedPages.OfType<Contact>().FirstOrDefault();

//        var showInMenuByPage = await _showInMenuService.GetShowInMenuByPageAsync(retrievedPages, cancellationToken);

//        var otherPages = retrievedPages
//            .Where(x => x is not Contact && showInMenuByPage[x])
//            .OrderBy(x => x.Order)
//            .ToArray();

//        var topMenuItems = otherPages.Select(page => GetLinkItemDto(page, displayNameByPage));

//        var contactUsMenuItem = contactPage is not null ? GetLinkItemDto(contactPage, displayNameByPage) : null;

//        return (topMenuItems, contactUsMenuItem);
//    }

//    private async Task<IEnumerable<CultureDto>> GetCultureDtosAsync(ICurrentPageData currentPageData, CancellationToken cancellationToken)
//    {
//        var websiteCultures = (await _websiteCulturesService.GetWebsiteCulturesAsync(_websiteContext.CurrentWebsiteID, cancellationToken)).ToArray();

//        if (websiteCultures.Length <= 1)
//        {
//            return Enumerable.Empty<CultureDto>();
//        }

//        var languageTags = websiteCultures.Select(x => x.LanguageTag).ToArray();

//        var linkByLanguageTag = await _cultureSwitchService.GetLinkByLanguageTagAsync(languageTags, currentPageData.PageID, cancellationToken);

//        var cultureDtos = websiteCultures
//            .OrderBy(x => x.Order ?? int.MaxValue)
//            .Select(websiteCulture =>
//            {
//                if (websiteCulture is { Visible: false })
//                {
//                    return null;
//                }

//                var shortName = string.IsNullOrEmpty(websiteCulture.SelectorAbbreviation) ? websiteCulture.LanguageTag.Language : websiteCulture.SelectorAbbreviation;

//                var name = string.IsNullOrEmpty(websiteCulture.SelectorLongForm) ? shortName : websiteCulture.SelectorLongForm;

//                var cultureDto = new CultureDto
//                {
//                    Name = name,
//                    ShortName = shortName,
//                    Url = linkByLanguageTag.TryGetValue(websiteCulture.LanguageTag, out var link) ? link.ToHtmlCompatibleString() : $"/{websiteCulture.LanguageTag}".ToLowerInvariant(),
//                };

//                return cultureDto;
//            }).OfType<CultureDto>();

//        return cultureDtos;
//    }

//    protected virtual async Task<string> GetCurrentWebsiteDisplayNameAsync(CancellationToken cancellationToken)
//    {
//        return await _currentWebsiteHelper.GetCurrentWebsiteDisplayNameAsync(cancellationToken);
//    }

//    protected virtual async Task<IEnumerable<FirstLevelMenuItemDto>> BuildMegaMenuAsync(CancellationToken cancellationToken)
//    {
//        var navigationItems = (await _multiTypePageRetriever.RetrievePagesAsync(query =>
//        {
//            query.Types(typeof(NavigationFeaturedItem), typeof(NavigationFirstLevelItem), typeof(NavigationSecondLevelItem), typeof(NavigationThirdLevelItem));

//            query.IncludePageTypeProperties();

//        }, cancellationToken))
//            .ToArray();

//        var displayNameByPage = await _displayNameService.GetDisplayNameByContentItemAsync(navigationItems, cancellationToken);

//        var firstLevelItems = navigationItems.OfType<NavigationFirstLevelItem>();

//        var secondLevelItems = navigationItems.OfType<NavigationSecondLevelItem>();

//        var thirdLevelItems = navigationItems.OfType<NavigationThirdLevelItem>();

//        var featuredItems = navigationItems.OfType<NavigationFeaturedItem>();

//        var firstLevelItemDtos = BuildFirstLevelMenuItems(firstLevelItems, secondLevelItems, thirdLevelItems, featuredItems, displayNameByPage);

//        return firstLevelItemDtos;
//    }

//    private static IEnumerable<FirstLevelMenuItemDto> BuildFirstLevelMenuItems(IEnumerable<NavigationFirstLevelItem> firstLevelItems, IEnumerable<NavigationSecondLevelItem> secondLevelItems, IEnumerable<NavigationThirdLevelItem> thirdLevelItems, IEnumerable<NavigationFeaturedItem> featuredItems, IDictionary<IContentItem, string> displayNameByPage)
//    {
//        var firstLevelMenuItemDtos = firstLevelItems
//            .OrderBy(x => x.Order)
//            .Select(firstLevelItem => new FirstLevelMenuItemDto
//            {
//                Title = displayNameByPage[firstLevelItem],
//                URL = firstLevelItem.TargetURL.ToHtmlCompatibleString(),
//                Featured = BuildFeaturedMenuItems(featuredItems, firstLevelItem.PageID, displayNameByPage),
//                Children = BuildSecondLevelMenuItems(secondLevelItems, thirdLevelItems, firstLevelItem.PageID, displayNameByPage),
//            });

//        return firstLevelMenuItemDtos;
//    }

//    private static IEnumerable<FeaturedMenuItemDto> BuildFeaturedMenuItems(IEnumerable<NavigationFeaturedItem> featuredItems, int parentId, IDictionary<IContentItem, string> displayNameByPage)
//    {
//        var featuredMenuItemDtos = featuredItems
//            .Where(featuredItem => featuredItem.PageParentID == parentId)
//            .OrderBy(x => x.Order)
//            .Select(featuredItem => new FeaturedMenuItemDto
//            {
//                Title = displayNameByPage[featuredItem],
//                URL = featuredItem.TargetURL.ToHtmlCompatibleString(),
//                Description = featuredItem.Description,
//                ImageURL = featuredItem.Image.ToHtmlCompatibleString(),
//                ImageAltText = featuredItem.ImageAltText,
//            })
//            .Take(3);

//        return featuredMenuItemDtos;
//    }

//    private static IEnumerable<SecondLevelMenuItemDto> BuildSecondLevelMenuItems(IEnumerable<NavigationSecondLevelItem> secondLevelItems, IEnumerable<NavigationThirdLevelItem> thirdLevelItems, int parentId, IDictionary<IContentItem, string> displayNameByPage)
//    {
//        var secondLevelItemsArray = secondLevelItems.ToArray();

//        var secondLevelMenuItemDtos = secondLevelItemsArray
//            .Where(secondLevelItem => secondLevelItem.PageParentID == parentId)
//            .OrderBy(x => x.Order)
//            .Select(secondLevelItem => new SecondLevelMenuItemDto
//            {
//                Title = displayNameByPage[secondLevelItem],
//                URL = secondLevelItem.TargetURL.ToHtmlCompatibleString(),
//                Description = secondLevelItem.Description,
//                Children = BuildThirdLevelMenuItems(thirdLevelItems, secondLevelItem.PageID, displayNameByPage),
//            });

//        return secondLevelMenuItemDtos;
//    }

//    private static IEnumerable<ThirdLevelMenuItemDto> BuildThirdLevelMenuItems(IEnumerable<NavigationThirdLevelItem> thirdLevelItems, int pageParentID, IDictionary<IContentItem, string> displayNameByPage)
//    {
//        var thirdLevelMenuItemDtos = thirdLevelItems
//            .Where(thirdLevelItem => thirdLevelItem.PageParentID == pageParentID)
//            .OrderBy(x => x.Order)
//            .Select(thirdLevelItem => new ThirdLevelMenuItemDto
//            {
//                Title = displayNameByPage[thirdLevelItem],
//                URL = thirdLevelItem.TargetURL.ToHtmlCompatibleString(),
//            });

//        return thirdLevelMenuItemDtos;
//    }

//    private static LinkItemDto GetLinkItemDto(IPage page, IDictionary<IContentItem, string> displayNameByPage)
//    {
//        var linkItemDto = new LinkItemDto
//        {
//            Title = displayNameByPage[page],
//            URL = page.UrlPath.ToHtmlCompatibleString(),
//        };

//        return linkItemDto;
//    }
//}
