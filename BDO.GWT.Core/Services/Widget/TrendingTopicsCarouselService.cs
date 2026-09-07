using BDO.GWT.Core;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.Utilities;
using CMS.ContentEngine;
using CMS.Websites;
using MailKit;

namespace BDO.GWT.Core.Services;

public interface ITrendingTopicsCarouselService
{
    Task<TrendingTopicsCarouselDto> GetTrendingTopicsCarouselAsync(TrendingTopicsCarouselData data, CancellationToken cancellationToken);
}

public class TrendingTopicsCarouselService : ITrendingTopicsCarouselService
{
    private readonly IContentService _contentService;
    private readonly ICultureContext _cultureContext;
    private readonly IWebPageUrlRetriever _webPageUrlRetriever;

    public TrendingTopicsCarouselService(
        IContentService contentService,
        ICultureContext cultureContext,
        IWebPageUrlRetriever webPageUrlRetriever)
    {
        _contentService = contentService;
        _cultureContext = cultureContext;
        _webPageUrlRetriever = webPageUrlRetriever;
    }

    public async Task<TrendingTopicsCarouselDto> GetTrendingTopicsCarouselAsync(
        TrendingTopicsCarouselData data,
        CancellationToken cancellationToken)
    {
        var itemDtos = await GetTrendingTopicsCarouselItemDtosAsync(
            data.TrendingTopicsPages, cancellationToken);

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
        IEnumerable<ContentItemReference> trendingTopicsPages,
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

        var pages = await _contentService.GetItemsByGUIDAsync(guids);

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
                webPage.SystemFields.WebPageItemGUID,
                _cultureContext.CurrentLanguageTag,
                cancellationToken: cancellationToken);

            itemDtos.Add(new TrendingTopicsCarouselItemDto
            {
                Title = webPage.SystemFields.WebPageItemName,
                Description = HtmlHelper.ExtractTextFromHtml(metadata.MetadataDescription),
                ReadMoreLink = url.RelativePath,
                ImageURL = metadata.MetadataTeaserImage?.FirstOrDefault()?.ContentAttachmentAsset.Url,
                ImageAltText = metadata.MetadataTeaserImageAltText,
                IsFeatured = false,
            });
        }

        return itemDtos.ToArray();
    }
}