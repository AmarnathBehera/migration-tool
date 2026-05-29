using BDO.GWT.Core;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.Services;
using BDO.GWT.Core.Utilities;
using BDO.GWT.Core.ValueObjects;
using BDO.GWT.Web.Core.DTOs.Pages;
using BDO.GWT.Web.Core.Services;
using CMS.ContentEngine;
using CMS.Websites.Routing;
using Kentico.Content.Web.Mvc;

namespace BDO.GWT.Web.Core.LiveSite.Services;

public interface IDealPageService
{
    Task<DealDto> GetDealPageDtoAsync(CancellationToken cancellationToken);
}

public class DealPageService : IDealPageService
{
    private readonly IHeroService _heroService;
    private readonly ISidebarService _sidebarService;
    private readonly IContentService _contentRetriever;
    private readonly IWebsiteChannelContext _websiteChannelContext;

    public DealPageService(ICultureContext cultureContext, IHeroService heroService, ISidebarService sidebarService, IContentService contentRetriever, IWebsiteChannelContext websiteChannelContext)
    {
        _heroService = heroService;
        _sidebarService = sidebarService;
        _contentRetriever = contentRetriever;
        _websiteChannelContext = websiteChannelContext;
    }

    public async Task<DealDto> GetDealPageDtoAsync(CancellationToken cancellationToken)
    {
        var results = await _contentRetriever.GetPagesAsync<Deal>(
           new RetrievePagesParameters
           {
               LinkedItemsMaxLevel = 5
           },
           query =>
           {
               query.Where(w =>
                   w.WhereEquals("ContentItemChannelID", _websiteChannelContext.WebsiteChannelID));
           },
           RetrievalCacheSettings.CacheDisabled,
           cancellationToken);

        var dealPage = results?.FirstOrDefault();

        if (dealPage == null)
        {
            return new DealDto();
        }

        // Map hero data
        var heroData = new HeroData
        {
            Layout = dealPage.HeroLayout,
            Title = dealPage.HeroTitle,
            Description = dealPage.HeroDescription,
            BackgroundImage = dealPage.HeroBackgroundImage?.FirstOrDefault()?.ContentAttachmentAsset?.Url is string bgUrl ? Link.Parse(bgUrl) : null,
            BackgroundImageAltText = dealPage.HeroBackgroundImageAltText,
            MobileBackgroundImage = dealPage.HeroBackgroundImage_Mobile?.FirstOrDefault()?.ContentAttachmentAsset?.Url is string mobileUrl ? Link.Parse(mobileUrl) : null,
            MobileBackgroundImageAltText = dealPage.HeroBackgroundImage_MobileAltText,
            CallToActionButtonType = dealPage.HeroCallToActionButtonType,
            CallToActionButtonText = dealPage.HeroCallToActionButtonText,
            CallToActionButtonUrl = dealPage.HeroCallToActionButtonUrl?.FirstOrDefault()?.SystemFields?.WebPageUrlPath is string ctaPath ? Link.Parse(ctaPath) : null,
        };

        var dto = new DealDto
        {
            Hero = await _heroService.GetHeroDtoAsync(heroData, cancellationToken),
            Title = dealPage.MetadataShowTitle ? dealPage.MetadataTitle : null,
            DealTypeNames = await GetDealTypeNames(dealPage.DealType, cancellationToken),
            IndustryNames = await GetIndustryNames(dealPage.Industry, cancellationToken),
            SubIndustryNames = await GetSubIndustryNames(dealPage.SubIndustry, cancellationToken),
            ClientName = dealPage.ClientName,
            Content = HtmlUtility.RemoveTildeFromUrlTypeHtmlAttributes(dealPage.DealContent),
            Sidebar = dealPage.ShowSidebar ? await _sidebarService.GetDealSidebarDtoAsync(dealPage, cancellationToken) : null,
        };

        return dto;
    }

    private Task<List<string>> GetDealTypeNames(IEnumerable<TagReference> dealTypes, CancellationToken cancellationToken)
        => GetTagNames(dealTypes, cancellationToken);

    private Task<List<string>> GetIndustryNames(IEnumerable<TagReference> industries, CancellationToken cancellationToken)
        => GetTagNames(industries, cancellationToken);

    private Task<List<string>> GetSubIndustryNames(IEnumerable<TagReference> subIndustries, CancellationToken cancellationToken)
        => GetTagNames(subIndustries, cancellationToken);

    private static Task<List<string>> GetTagNames(IEnumerable<TagReference> tags, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var names = tags
            .Select(tag => tag.ToString())
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Distinct()
            .ToList();

        return Task.FromResult(names);
    }
}
