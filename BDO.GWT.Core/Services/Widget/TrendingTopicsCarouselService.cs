using BDO.GWT.Core;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.Utilities;
using CMS.ContentEngine;
using CMS.Websites;
using Kentico.Content.Web.Mvc;
using MailKit;

namespace BDO.GWT.Core.Services;

public interface ITrendingTopicsCarouselService
{
    Task<TrendingTopicsCarouselDto> GetTrendingTopicsCarouselAsync(TrendingTopicsCarouselData data, CancellationToken cancellationToken);
}

public class TrendingTopicsCarouselService : ITrendingTopicsCarouselService
{
    private readonly ICultureContext _cultureContext;
    private readonly IWebPageUrlRetriever _webPageUrlRetriever;
    private readonly IContentRetriever _contentRetriever;


    private static readonly string[] BlogContentTypes = [SectionBlogs.CONTENT_TYPE_NAME,
Blog.CONTENT_TYPE_NAME,
BlogPost.CONTENT_TYPE_NAME];

    private static readonly string[] ServiceContentTypes = [SectionServices.CONTENT_TYPE_NAME,
Service.CONTENT_TYPE_NAME,
BusinessLine.CONTENT_TYPE_NAME,
ServiceArea.CONTENT_TYPE_NAME,
Sub_Service.CONTENT_TYPE_NAME];

    private static readonly string[] IndustryContentTypes = [SectionIndustries.CONTENT_TYPE_NAME,
Industry.CONTENT_TYPE_NAME,
IndustryCategory.CONTENT_TYPE_NAME,
IndustryService.CONTENT_TYPE_NAME];

    private static readonly string[] MicrositeContentTypes = [SectionMicrosites.CONTENT_TYPE_NAME,
MicrositeMasterPage.CONTENT_TYPE_NAME,
MicrositePage.CONTENT_TYPE_NAME];

    private static readonly string[] SpecialtyContentTypes = [SectionSpecialties.CONTENT_TYPE_NAME,
SpecialtiesArea.CONTENT_TYPE_NAME,
SpecialtiesCategory.CONTENT_TYPE_NAME,
SpecialtiesPage.CONTENT_TYPE_NAME];

    private static readonly string[] InsightContentTypes = [SectionInsightsPage.CONTENT_TYPE_NAME,
Insight.CONTENT_TYPE_NAME,
InsightCategory.CONTENT_TYPE_NAME];



    private readonly Func<IEnumerable<string>, IEnumerable<Guid>, RetrieveContentOfContentTypesParameters, CancellationToken, Task<IEnumerable<IWebPageFieldsSource>>> _retrieveContentByGuidsAsync;
    public TrendingTopicsCarouselService(
        IContentRetriever contentRetriever,
        ICultureContext cultureContext,
        IWebPageUrlRetriever webPageUrlRetriever)
    {
        _contentRetriever = contentRetriever;
        _cultureContext = cultureContext;
        _webPageUrlRetriever = webPageUrlRetriever;

        _retrieveContentByGuidsAsync = (contentTypes, guids, parameters, ct) =>
            _contentRetriever.RetrieveContentOfContentTypesByGuids<IWebPageFieldsSource>(
                contentTypes, guids.ToList(), parameters, ct);
    }

    public async Task<TrendingTopicsCarouselDto> GetTrendingTopicsCarouselAsync(
        TrendingTopicsCarouselData data,
        CancellationToken cancellationToken)
    {
        var itemDtos = await GetTrendingTopicsCarouselItemDtosAsync(
            data.TrendingTopicsPages,data.TrendingTopicsPageType, cancellationToken);

        return new TrendingTopicsCarouselDto
        {
            Title = data.Title,
            Description = data.Description,
            Items = itemDtos,
            TopMargin = data.TopMargin,
            BottomMargin = data.BottomMargin,
            WidgetID = data.WidgetID,
            LearnMoreButtonURLTarget = data.LearnMoreButtonURLTarget,
        };
    }

    private async Task<TrendingTopicsCarouselItemDto[]> GetTrendingTopicsCarouselItemDtosAsync(
        IEnumerable<ContentItemReference> trendingTopicsPages,string trendingTopicsPageType,
        CancellationToken cancellationToken)
    {
        // Extract GUIDs from ContentItemReference — IContentService.GetItemsByGUIDAsync takes List<Guid>
        var guids = trendingTopicsPages
            .Select(p => p.Identifier)
            .ToList();

        if (guids.Count == 0)
        {
            return Array.Empty<TrendingTopicsCarouselItemDto>();
        }



        var task = trendingTopicsPageType switch
        {
            "Blog" => _retrieveContentByGuidsAsync(BlogContentTypes, guids, new RetrieveContentOfContentTypesParameters { LinkedItemsMaxLevel = 2 }, cancellationToken),
            "Service" => _retrieveContentByGuidsAsync(ServiceContentTypes, guids, new RetrieveContentOfContentTypesParameters { LinkedItemsMaxLevel = 2 }, cancellationToken),
            "Industry" => _retrieveContentByGuidsAsync(IndustryContentTypes, guids, new RetrieveContentOfContentTypesParameters { LinkedItemsMaxLevel = 2 }, cancellationToken),
            "Microsite" => _retrieveContentByGuidsAsync(MicrositeContentTypes, guids, new RetrieveContentOfContentTypesParameters { LinkedItemsMaxLevel = 2 }, cancellationToken),
            "Specialty" => _retrieveContentByGuidsAsync(SpecialtyContentTypes, guids, new RetrieveContentOfContentTypesParameters { LinkedItemsMaxLevel = 2 }, cancellationToken),
            "Insight" => _retrieveContentByGuidsAsync(InsightContentTypes, guids, new RetrieveContentOfContentTypesParameters { LinkedItemsMaxLevel = 2 }, cancellationToken),
            _ => throw new ArgumentException($"Invalid trendingTopicsPageType: {trendingTopicsPageType}")
        };

        var pages = await task;

        var itemDtos = new List<TrendingTopicsCarouselItemDto>();

        foreach (var page in pages)
        {
            // XbyK: cast to IWebPageFieldsSource to get tree/url fields
            if (page is not IWebPageFieldsSource webPage)
            {
                continue;
            }

            // Metadata interface — same as KX13 but now on the content model
            if (page is not IMetadata metadata)
            {
                continue;
            }

            // Resolve URL via IWebPageUrlRetriever using WebPageItemGUID (replaces NodeID url lookup)
            var url = await _webPageUrlRetriever.Retrieve(
                webPage,
                _cultureContext.CurrentLanguageTag,
                cancellationToken: cancellationToken);

            itemDtos.Add(new TrendingTopicsCarouselItemDto
            {
                Title = metadata.MetadataTitle,
                Description = HtmlHelper.ExtractTextFromHtml(metadata.MetadataDescription),
                ReadMoreLink = url.AbsoluteUrl,
                ImageURL = metadata.MetadataTeaserImage?.FirstOrDefault()?.ContentAttachmentAsset.Url.TrimStart('~'),
                ImageAltText = metadata.MetadataTeaserImageAltText,
                IsFeatured = false,
            });
        }

        return itemDtos.ToArray();
    }
}