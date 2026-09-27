using BDO.GWT.Core.Data;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.Utilities;
using CMS.ContentEngine;
using CMS.Websites;
using Kentico.Content.Web.Mvc;

namespace BDO.GWT.Core.Services.Widget;

public interface IFlexibleContentService
{
    Task<FlexibleContentDto> GetFlexibleContentAsync(FlexibleContentData data, CancellationToken cancellationToken);
}

public class FlexibleContentService : IFlexibleContentService
{
    private static readonly string[] AllowedContentTypes =
    [
        Blog.CONTENT_TYPE_NAME,
        BlogPost.CONTENT_TYPE_NAME,
        IndustryCategory.CONTENT_TYPE_NAME,
        Industry.CONTENT_TYPE_NAME,
        IndustryService.CONTENT_TYPE_NAME,
        Insight.CONTENT_TYPE_NAME,
        MicrositePage.CONTENT_TYPE_NAME,
        BusinessLine.CONTENT_TYPE_NAME,
        ServiceArea.CONTENT_TYPE_NAME,
        Service.CONTENT_TYPE_NAME,
        Sub_Service.CONTENT_TYPE_NAME,
        SpecialtiesPage.CONTENT_TYPE_NAME,
        SpecialtiesArea.CONTENT_TYPE_NAME,
    ];

    private readonly IContentRetriever _contentRetriever;
    private readonly IWebPageUrlRetriever _urlRetriever;

    public FlexibleContentService(IContentRetriever contentRetriever, IWebPageUrlRetriever urlRetriever)
    {
        _contentRetriever = contentRetriever;
        _urlRetriever = urlRetriever;
    }

    public async Task<FlexibleContentDto> GetFlexibleContentAsync(FlexibleContentData data, CancellationToken cancellationToken)
    {
        var items = await GetFlexibleContentItemDtosAsync(data.FlexibleContentGuids, data.FeaturedFlexibleContentGuids, cancellationToken);

        return new FlexibleContentDto
        {
            Title = data.Title,
            Description = data.Description,
            Items = items,
            TopMargin = data.TopMargin,
            BottomMargin = data.BottomMargin,
            WidgetID = data.WidgetID,
            LearnMoreButtonURLTarget = data.LearnMoreButtonURLTarget,
        };
    }

    private async Task<FlexibleContentItemDto[]> GetFlexibleContentItemDtosAsync(
        IEnumerable<Guid> pageGuids,
        IEnumerable<Guid> featuredPageGuids,
        CancellationToken cancellationToken)
    {
        var featuredGuids = featuredPageGuids.ToHashSet();
        var allGuids = featuredGuids.Concat(pageGuids).Distinct().ToList();

        if (allGuids.Count == 0)
        {
            return [];
        }

        var contentItems = await _contentRetriever
            .RetrieveContentOfContentTypesByGuids<IContentItemFieldsSource>(
                AllowedContentTypes,
                allGuids,
                new RetrieveContentOfContentTypesParameters
                {
                    LinkedItemsMaxLevel = 3
                },
                cancellationToken);

        var itemByGuid = contentItems.ToDictionary(
            x => x.SystemFields.ContentItemGUID,
            x => x);

        var result = new List<FlexibleContentItemDto>();

        foreach (var guid in allGuids)
        {
            if (!itemByGuid.TryGetValue(guid, out var contentItem))
            {
                continue;
            }

            var metadata = contentItem as IMetadata;
            string? url = null;

            if (contentItem is IWebPageFieldsSource webPage)
            {
                var retrievedUrl = await _urlRetriever.Retrieve(webPage, cancellationToken);
                url = retrievedUrl?.RelativePath;
            }

            result.Add(new FlexibleContentItemDto
            {
                Title = metadata?.MetadataTitle,
                Description = metadata?.MetadataDescription is string desc
                    ? HtmlHelper.ExtractTextFromHtml(desc)
                    : null,
                ReadMoreLink = url,
                ImageURL = metadata?.MetadataTeaserImage?.FirstOrDefault()?.ContentAttachmentAsset?.Url.TrimStart('~'),
                ImageAltText = metadata?.MetadataTeaserImageAltText,
                IsFeatured = featuredGuids.Contains(guid),
            });
        }

        return result.ToArray();
    }
}
