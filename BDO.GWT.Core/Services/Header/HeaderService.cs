using BDO;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Header;
using BDO.GWT.Core.ValueObjects;
using CMS.Base;
using CMS.Helpers;
using CMS.Websites;
using CMS.Websites.Routing;
using Kentico.Content.Web.Mvc;
using Kentico.Content.Web.Mvc.Routing;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BDO.GWT.Core.Services.Header;

public interface IHeaderService
{
    Task<HeaderDto> GetAsync(IContentService contentservice, CancellationToken cancellationToken);
}

public class HeaderService : IHeaderService
{
    private readonly IContentService _contentService;
    private readonly IPreferredLanguageRetriever _currentLanguageRetriever;
    private readonly IProgressiveCache _progressiveCache;
    private readonly IContentRetriever _contentRetriever;
    public readonly IHomePageService _homePageService;
    private readonly IWebPageDataContextRetriever _webPageDataContextRetriever;
    private readonly IWebsiteCulturesService _websiteCulturesService;
    private readonly IWebsiteChannelContext _websiteChannel;
    public HeaderService(IContentService contentService, IPreferredLanguageRetriever currentLanguageRetriever, IProgressiveCache progressiveCache, IContentRetriever contentRetriever, IHomePageService homePageService, IWebPageDataContextRetriever webPageDataContextRetriever, IWebsiteCulturesService websiteCulturesService, IWebsiteChannelContext websiteChannel)
    {
        _contentService = contentService;
        _currentLanguageRetriever = currentLanguageRetriever;
        _progressiveCache = progressiveCache;
        _contentRetriever = contentRetriever;
        _homePageService = homePageService;
        _webPageDataContextRetriever = webPageDataContextRetriever;
        _websiteCulturesService = websiteCulturesService;
        _websiteChannel = websiteChannel;
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
            //LogoImageURL = logoImageUrl ?? string.Empty,
            //SearchPageURL = searchPage != null ? searchPage.UrlPath.ToHtmlCompatibleString() : string.Empty,
            MegaMenuItems = megaMenu,
        };

        return headerDto;
    }

    protected virtual async Task<string> GetLogoPageLinkAsync(CancellationToken cancellationToken)
    {
        var homePage = await _homePageService.GetHomePageUrl(cancellationToken);

        return homePage ?? string.Empty;
    }

    private async Task<IEnumerable<CultureDto>> GetCultureDtosAsync(CancellationToken cancellationToken)
    {
        return await _websiteCulturesService.GetWebsiteCulturesAsync();
    }

    private async Task<(IEnumerable<LinkItemDto> TopMenu, LinkItemDto? Contact)> BuildTopMenuAsync(CancellationToken cancellationToken)
    {
        var homePages = await _contentRetriever.RetrievePages<HomePage>(cancellationToken); 
        var microsites = await _contentRetriever.RetrievePages<MicrositePlaceholder>(cancellationToken); 

        var topMenuItems = new List<LinkItemDto>();
        topMenuItems.AddRange(homePages.Select(p => new LinkItemDto
        {
            Title = p.MetadataTitle,
            URL = p.GetUrl().AbsoluteUrl
        }));

        topMenuItems.AddRange(microsites.Select(p => new LinkItemDto
        {
            Title = p.MicrositeTitle,
            URL = p.GetUrl().AbsoluteUrl.ToString() ?? string.Empty
        }));
        var contactPage = (await _contentRetriever.RetrievePages<HomePage>(
           new RetrievePagesParameters
           { },
           query => query.OrderBy(nameof(IWebPageContentQueryDataContainer.WebPageItemOrder)),
           RetrievalCacheSettings.CacheDisabled,
           cancellationToken
       )).FirstOrDefault();
        var linkItemDto = new LinkItemDto
        {
            Title = contactPage?.MetadataTitle,
            URL = contactPage?.GetUrl().AbsoluteUrl.ToString() ?? string.Empty
        };

        return (topMenuItems, linkItemDto);
    }
    protected virtual async Task<IEnumerable<FirstLevelMenuItemDto>> BuildMegaMenuAsync(CancellationToken cancellationToken = default)
    {
        string rootPath = "/";

        var firstLevelItems = (await _contentRetriever.RetrievePages<NavigationFirstLevelItem>(
            new RetrievePagesParameters
            {
                PathMatch = PathMatch.Children(rootPath, 1)
            },
            query => query.OrderBy(nameof(IWebPageContentQueryDataContainer.WebPageItemOrder)),
            RetrievalCacheSettings.CacheDisabled,
            cancellationToken
        )).ToList();

        var megaMenu = new List<FirstLevelMenuItemDto>();

        foreach (var first in firstLevelItems)
        {
            var firstPath = first.SystemFields.WebPageUrlPath;

            List<Guid> webPageItemGuidsfirst = first.TargetURL.Select(i => i.WebPageGuid).ToList();

            IEnumerable<IWebPageFieldsSource> selectedPagesfirst =
                await _contentService.GetItemsByGUIDAsync(webPageItemGuidsfirst);

            var secondLevelItems = (await _contentRetriever.RetrievePages<NavigationSecondLevelItem>(
                new RetrievePagesParameters
                {
                    PathMatch = PathMatch.Children(firstPath, 1)
                },
                query => query.OrderBy(nameof(IWebPageContentQueryDataContainer.WebPageItemOrder)),
                RetrievalCacheSettings.CacheDisabled,
                cancellationToken
            )).ToList();

            var featuredLevelItems = (await _contentRetriever.RetrievePages<NavigationFeaturedItem>(
                new RetrievePagesParameters
                {
                    PathMatch = PathMatch.Children(firstPath, 1)
                },
                query => query.OrderBy(nameof(IWebPageContentQueryDataContainer.WebPageItemOrder)),
                RetrievalCacheSettings.CacheDisabled,
                cancellationToken
            )).ToList();

            var secondLevelDtos = new List<SecondLevelMenuItemDto>();

            foreach (var second in secondLevelItems)
            {
                var secondPath = second.SystemFields.WebPageUrlPath;

                List<Guid> webPageItemGuidssecond = second.TargetURL.Select(i => i.WebPageGuid).ToList();

                IEnumerable<IWebPageFieldsSource> selectedPagesSecond =
                    await _contentRetriever.RetrieveAllPagesByGuids<IWebPageFieldsSource>(webPageItemGuidssecond);

                var thirdLevelItems = (await _contentRetriever.RetrievePages<NavigationThirdLevelItem>(
                    new RetrievePagesParameters
                    {
                        PathMatch = PathMatch.Children(secondPath, 1)
                    },
                    query => query.OrderBy(nameof(IWebPageContentQueryDataContainer.WebPageItemOrder)),
                    RetrievalCacheSettings.CacheDisabled,
                    cancellationToken
                )).ToList();

                var thirdLevelDtos = new List<ThirdLevelMenuItemDto>();

                foreach (var third in thirdLevelItems)
                {
                    List<Guid> webPageItemGuidsthird = third.TargetURL.Select(i => i.WebPageGuid).ToList();

                    IEnumerable<IWebPageFieldsSource> selectedPagesThird =
                        await _contentRetriever.RetrieveAllPagesByGuids<IWebPageFieldsSource>(webPageItemGuidsthird);

                    thirdLevelDtos.Add(new ThirdLevelMenuItemDto
                    {
                        Title = third.Title,
                        URL = selectedPagesThird.FirstOrDefault()?.GetUrl()?.AbsoluteUrl ?? string.Empty
                    });
                }

                secondLevelDtos.Add(new SecondLevelMenuItemDto
                {
                    Title = second.Title,
                    Children = thirdLevelDtos,
                    URL = selectedPagesSecond.FirstOrDefault()?.GetUrl()?.AbsoluteUrl ?? string.Empty
                });
            }

            var featuredDtos = featuredLevelItems.Select(featured => new FeaturedMenuItemDto
            {
                Title = featured.Title
            }).ToList();

            megaMenu.Add(new FirstLevelMenuItemDto
            {
                Title = first.Title,
                Children = secondLevelDtos,
                Featured = featuredDtos,
                Target = selectedPagesfirst.FirstOrDefault()?.GetUrl()?.AbsoluteUrl ?? string.Empty
            });
        }

        return megaMenu;
    }

}
