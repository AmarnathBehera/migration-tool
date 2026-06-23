using BDO.GWT.Core;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.Helpers;
using BDO.GWT.Core.Services;
using BDO.GWT.Core.Utilities;
using BDO.GWT.Core.ValueObjects;
using BDO.GWT.Web.Core.DTOs.Pages;
using BDO.GWT.Web.Core.Services;
using CMS.Websites.Routing;
using Kentico.Content.Web.Mvc;

namespace BDO.GWT.Web.Core.LiveSite.Services;

public interface IDealPageService
{
    Task<DealDto> GetDealPageDtoAsync(CancellationToken cancellationToken);
}

public class DealPageService(
    IHeroService heroService,
    ISidebarService sidebarService,
    IContentService contentRetriever,
    IWebsiteChannelContext websiteChannelContext,
    IWebPageDataContextRetriever webPageDataContextRetriever) : IDealPageService
{
    private static readonly RetrievePagesParameters _retrieveParameters = new()
    {
        LinkedItemsMaxLevel = 5
    };

    public async Task<DealDto> GetDealPageDtoAsync(CancellationToken cancellationToken)
    {
        _ = websiteChannelContext;

        if (!webPageDataContextRetriever.TryRetrieve(out var webPageContext) || webPageContext.WebPage is null)
        {
            return new DealDto();
        }

        var currentPageGuid = webPageContext.WebPage.WebPageItemGUID;

        var results = await contentRetriever.GetPagesAsync<Deal>(
            _retrieveParameters,
            query => query.Where(w => w.WhereEquals("WebPageItemGUID", currentPageGuid)),
            RetrievalCacheSettings.CacheDisabled,
            cancellationToken);

        var dealPage = results.FirstOrDefault();
        if (dealPage is null)
        {
            return new DealDto();
        }

        var heroBackgroundImage = dealPage.HeroBackgroundImage?.FirstOrDefault();
        var heroMobileBackgroundImage = dealPage.HeroBackgroundImage_Mobile?.FirstOrDefault();
        var heroCallToActionUrl = dealPage.HeroCallToActionButtonUrl?.FirstOrDefault();

        var heroData = new HeroData
        {
            Layout = dealPage.HeroLayout,
            Title = dealPage.HeroTitle,
            Description = dealPage.HeroDescription,
            BackgroundImage = ParseLink(heroBackgroundImage?.ContentAttachmentAsset?.Url),
            BackgroundImageAltText = dealPage.HeroBackgroundImageAltText,
            MobileBackgroundImage = ParseLink(heroMobileBackgroundImage?.ContentAttachmentAsset?.Url),
            MobileBackgroundImageAltText = dealPage.HeroBackgroundImage_MobileAltText,
            CallToActionButtonType = dealPage.HeroCallToActionButtonType,
            CallToActionButtonText = dealPage.HeroCallToActionButtonText,
            CallToActionButtonUrl = ParseLink(heroCallToActionUrl?.SystemFields?.WebPageUrlPath),
        };

        var sidebarTask = dealPage.ShowSidebar
            ? sidebarService.GetDealSidebarDtoAsync(dealPage, cancellationToken)
            : null;

        var hero = await heroService.GetHeroDtoAsync(heroData, cancellationToken);
        var sidebar = sidebarTask is null ? null : await sidebarTask;

        return new DealDto
        {
            Hero = hero,
            Title = dealPage.MetadataShowTitle ? dealPage.MetadataTitle : null,
            DealTypeNames = MapNames(dealPage.DealType, cancellationToken),
            IndustryNames = MapNames(dealPage.Industry, cancellationToken),
            SubIndustryNames = MapNames(dealPage.SubIndustry, cancellationToken),
            Range = MapNames(dealPage.Size, cancellationToken),
            ClientName = dealPage.ClientName,
            Content = HtmlUtility.RemoveTildeFromUrlTypeHtmlAttributes(dealPage.DealContent),
            Sidebar = sidebar,
        };
    }

    private static Link? ParseLink(string? url) =>
        string.IsNullOrWhiteSpace(url) ? null : Link.Parse(url);

    private static List<string> MapNames<T>(IEnumerable<T>? items, CancellationToken cancellationToken)
        where T : class
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (items is null)
        {
            return [];
        }

        var result = items is ICollection<T> collection
            ? new List<string>(collection.Count)
            : [];

        foreach (var item in items)
        {
            if (item is null)
            {
                continue;
            }

            var name = ItemPropertyMapper.GetName(item);
            if (!string.IsNullOrWhiteSpace(name))
            {
                result.Add(name);
            }
        }

        return result;
    }
}
