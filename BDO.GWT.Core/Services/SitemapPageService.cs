using BDO.GWT.Core;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.Services;
using BDO.GWT.Core.Utilities;
using BDO.GWT.Core.ValueObjects;
using CMS.ContentEngine;
using CMS.Websites;
using CMS.Websites.Routing;
using Kentico.Content.Web.Mvc;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace BDO.GWT.Web.Core.Services;

public sealed class SitemapPageNode
{
    public int WebPageItemID { get; init; }
    public string Name { get; init; } = null!;
    public string TreePath { get; init; } = null!;
}

public interface ISitemapPageService
{
    Task<SitemapPageDto> GetSitemapPageDtoAsync(CancellationToken cancellationToken);
}

public class SitemapPageService : ISitemapPageService
{

    private static readonly IReadOnlyList<string> _pageTypeRegistry =
    [
        Career.CONTENT_TYPE_NAME,
Careers.CONTENT_TYPE_NAME,
SectionCareers.CONTENT_TYPE_NAME,
Sitemap.CONTENT_TYPE_NAME,
SectionIndustries.CONTENT_TYPE_NAME,
Industry.CONTENT_TYPE_NAME,
IndustryService.CONTENT_TYPE_NAME,
IndustryCategory.CONTENT_TYPE_NAME,
SpecialtiesArea.CONTENT_TYPE_NAME,
SpecialtiesCategory.CONTENT_TYPE_NAME,
SpecialtiesPage.CONTENT_TYPE_NAME,
SectionSpecialties.CONTENT_TYPE_NAME,
ServiceArea.CONTENT_TYPE_NAME,
BusinessLine.CONTENT_TYPE_NAME,
Sub_Service.CONTENT_TYPE_NAME,
SectionServices.CONTENT_TYPE_NAME,
Service.CONTENT_TYPE_NAME
    ];

    private readonly IHeroService _heroService;
    private readonly IContentQueryExecutor _contentQueryExecutor;
    private readonly IWebPageUrlRetriever _webPageUrlRetriever;
    private readonly IWebsiteChannelContext _websiteChannelContext;
    private readonly IContentService _contentService;
    private readonly ILogger<SitemapPageService> _logger;
    private readonly string _languageName;

    public SitemapPageService(
        IHeroService heroService,
        IContentQueryExecutor contentQueryExecutor,
        IWebPageUrlRetriever webPageUrlRetriever,
        IWebsiteChannelContext websiteChannelContext,
        ICultureContext cultureContext,
        IContentService contentService,
        ILogger<SitemapPageService> logger)
    {
        _heroService = heroService;
        _contentQueryExecutor = contentQueryExecutor;
        _webPageUrlRetriever = webPageUrlRetriever;
        _websiteChannelContext = websiteChannelContext;
        _contentService = contentService;
        _logger = logger;
        _languageName = cultureContext.CurrentLanguageTag;
    }

    public async Task<SitemapPageDto> GetSitemapPageDtoAsync(CancellationToken cancellationToken)
    {
        var sitemap = await _contentService.RetrieveCurrentPageAsync<Sitemap>(
            new RetrieveCurrentPageParameters { LinkedItemsMaxLevel = 4 }, cancellationToken);

        if(sitemap == null) {
            throw new InvalidOperationException(
                "No Sitemap page found.");
        }

        var pages = await GetAllSitemapPagesAsync(cancellationToken);

        if (pages.Count == 0)
        {
            throw new InvalidOperationException(
                "[Sitemap] No pages returned from query. Check IncludeInSitemap flags and website channel name.");
        }

        // Step 2: Resolve URLs sequentially to avoid cold cache race conditions
        var urlByPageItemId = await GetUrlByPageItemIdAsync(pages, cancellationToken);

        // Step 3: Build nested DTO tree
        return new SitemapPageDto
        {
            Hero = await _heroService.GetHeroDtoAsync(new HeroData
            {
                Layout = sitemap.HeroLayout,
                Title = sitemap.HeroTitle,
                Description = sitemap.HeroDescription,
                BackgroundImage = sitemap.HeroBackgroundImage?.FirstOrDefault()?.ContentAttachmentAsset?.Url is string bgUrl
                    ? Link.Parse(bgUrl) : null,
                BackgroundImageAltText = sitemap.HeroBackgroundImageAltText,
                MobileBackgroundImage = sitemap.HeroBackgroundImage_Mobile?.FirstOrDefault()?.ContentAttachmentAsset?.Url is string mobileUrl
                    ? Link.Parse(mobileUrl) : null,
                MobileBackgroundImageAltText = sitemap.HeroBackgroundImage_MobileAltText,
                CallToActionButtonType = sitemap.HeroCallToActionButtonType,
                CallToActionButtonText = sitemap.HeroCallToActionButtonText,
                CallToActionButtonUrl = sitemap.HeroCallToActionButtonUrl?.FirstOrDefault()?.SystemFields?.WebPageUrlPath is string ctaPath
                    ? Link.Parse(ctaPath) : null,
            }, cancellationToken),
            Items = BuildSitemapItemDtos(pages, urlByPageItemId, parentPath: null),
        };
    }


    private async Task<List<SitemapPageNode>> GetAllSitemapPagesAsync(CancellationToken cancellationToken)
    {
        var tasks = _pageTypeRegistry.Select(contentTypeName =>
            QuerySitemapPagesAsync(contentTypeName, cancellationToken));

        var results = await Task.WhenAll(tasks);

        return results.SelectMany(x => x).ToList();
    }

    private async Task<IEnumerable<SitemapPageNode>> QuerySitemapPagesAsync(
        string contentTypeName,
        CancellationToken cancellationToken)
    {
        var query = new ContentItemQueryBuilder()
            .ForContentType(
                contentTypeName,
                config => config
                    .ForWebsite(_websiteChannelContext.WebsiteChannelName)
                    .Where(where => where
                        .WhereTrue(nameof(ISearchAndSEO.IncludeInSitemap)))
                    .WithLinkedItems(0))
            .InLanguage(_languageName);


        var options = new ContentQueryExecutionOptions
        {
            ForPreview = false,
            IncludeSecuredItems = false,
        };

        return await _contentQueryExecutor.GetWebPageResult(
            query,
            container => new SitemapPageNode
            {
                WebPageItemID = container.WebPageItemID,
                Name = GetCleanPageName(container.WebPageItemTreePath),
                TreePath = container.WebPageItemTreePath,
            },
            options,
            cancellationToken);
    }


    private async Task<Dictionary<int, string>> GetUrlByPageItemIdAsync(
        IEnumerable<SitemapPageNode> pages,
        CancellationToken cancellationToken)
    {
        var urlMap = new Dictionary<int, string>();

        foreach (var page in pages)
        {
            try
            {
                var url = await _webPageUrlRetriever.Retrieve(
                    page.WebPageItemID,
                    _languageName,
                    cancellationToken: cancellationToken);

                if (!string.IsNullOrEmpty(url?.RelativePath))
                {
                    urlMap[page.WebPageItemID] = url.RelativePath;
                }
            }
            catch (Exception ex)
            {
                
                _logger.LogWarning(ex,
                    "[Sitemap] Failed to retrieve URL for WebPageItemID: {WebPageItemID} | Name: {Name} | Path: {TreePath}",
                    page.WebPageItemID, page.Name, page.TreePath);
            }
        }

        return urlMap;
    }


    private static List<SitemapItemDto> BuildSitemapItemDtos(
        List<SitemapPageNode> allPages,
        Dictionary<int, string> urlByPageItemId,
        string? parentPath)
    {
        return allPages
            .Where(p => GetParentPath(p.TreePath) == parentPath)
            .Select(page =>
            {
                var children = BuildSitemapItemDtos(allPages, urlByPageItemId, page.TreePath);

                // Drop pages with no URL and no children — mirrors KX13 original behaviour
                if (!urlByPageItemId.ContainsKey(page.WebPageItemID) && children.Count == 0)
                    return null;

                return new SitemapItemDto
                {
                    Title = page.Name,
                    Link = urlByPageItemId.GetValueOrDefault(page.WebPageItemID),
                    Items = children,
                };
            })
            .OfType<SitemapItemDto>()
            .ToList();
    }


    private static string GetCleanPageName(string treePath)
    {
        var lastSegment = treePath.TrimEnd('/').Split('/').Last();

        // Strip XbyK's 8-char alphanumeric suffix e.g. "-2cihogce"
        lastSegment = SitemapRegex.SuffixRegex().Replace(lastSegment, string.Empty);

        // "graduate-programmes" → "Graduate Programmes"
        return System.Globalization.CultureInfo.CurrentCulture.TextInfo
            .ToTitleCase(lastSegment.Replace('-', ' '));
    }


    private static string? GetParentPath(string treePath)
    {
        var lastSlash = treePath.TrimEnd('/').LastIndexOf('/');
        return lastSlash <= 0 ? null : treePath[..lastSlash];
    }
}