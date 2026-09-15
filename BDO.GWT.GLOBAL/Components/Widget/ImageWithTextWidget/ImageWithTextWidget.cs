using BDO.GWT.Core;
using BDO.GWT.Core.Data.ImageWithTextData;
using CMS.ContentEngine;
using CMS.Websites;
using CMS.Websites.Routing;
using Kentico.Content.Web.Mvc;
using Kentico.PageBuilder.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BDO.GWT.Web.Widgets.ImageWithTextWidget;

public class ImageWithTextWidget : ViewComponent
{
    private readonly IContentRetriever _contentRetriever;
    private readonly IContentService _contentService;
    private readonly IContentQueryExecutor _queryExecutor;
    private readonly IWebsiteChannelContext _channelContext;

    private const string ViewPath = "~/Components/Widget/ImageWithTextWidget/_ImageWithTextWidget.cshtml";

    public ImageWithTextWidget(
        IContentRetriever contentRetriever,
        IContentService contentService,
        IContentQueryExecutor queryExecutor,
        IWebsiteChannelContext channelContext)
    {
        _contentRetriever = contentRetriever;
        _contentService = contentService;
        _queryExecutor = queryExecutor;
        _channelContext = channelContext;
    }

    public async Task<ViewViewComponentResult> InvokeAsync(ComponentViewModel<ImageWithTextWidgetProperties> model)
    {
        if (model is null)
        {
            return View(ViewPath, new ImageWithTextData());
        }

        var imageUrl = await RetrieveAttachmentUrlAsync(model.Properties.ImageURL);
        var backgroundImageUrl = await RetrieveAttachmentUrlAsync(model.Properties.BackgroundImageURL);
        var buttonUrl = await ResolveButtonUrlAsync(model.Properties);

        var dto = BuildDto(model.Properties, buttonUrl, imageUrl, backgroundImageUrl);

        return View(ViewPath, dto);
    }

    // Retrieves the URL of a ContentAttachment from a list of ContentItemReferences.
    private async Task<string> RetrieveAttachmentUrlAsync(IEnumerable<ContentItemReference> references)
    {
        if (references?.Any() != true)
        {
            return null;
        }

        var attachments = await _contentRetriever.RetrieveContentByGuids<ContentAttachment>(
            references.Select(i => i.Identifier).ToList(),
            new RetrieveContentParameters { LinkedItemsMaxLevel = 3 });

        return attachments.FirstOrDefault()?.ContentAttachmentAsset?.Url.TrimStart('~');
    }

    // Resolves the button URL from either a manual URL or a content item GUID.
    private async Task<string> ResolveButtonUrlAsync(ImageWithTextWidgetProperties properties)
    {
        if (properties.UrlSource == "manual")
        {
            return properties.ManualUrl;
        }

        var buttonGuids = properties.ButtonURL?.Select(i => i.Identifier).ToList() ?? [];

        if (buttonGuids.Count == 0)
        {
            return null;
        }

        return await ResolvePageUrlAsync(buttonGuids.First())
            ?? await ResolveAttachmentUrlAsync(properties.ButtonURL);
    }

    // Queries WebPageUrlPath by ContentItemGUID — single lightweight indexed lookup.
    private async Task<string> ResolvePageUrlAsync(Guid contentItemGuid)
    {
        var builder = new ContentItemQueryBuilder()
            .ForContentTypes(q => q
                .ForWebsite(_channelContext.WebsiteChannelName, includeUrlPath: true))
            .Parameters(q => q
                .Where(w => w.WhereEquals(
                    nameof(IContentQueryDataContainer.ContentItemGUID),
                    contentItemGuid))
                .TopN(1))
            .InLanguage(_contentService.GetCurrentLanguage());

        var options = new ContentQueryExecutionOptions
        {
            ForPreview = _channelContext.IsPreview
        };

        var results = await _queryExecutor.GetResult<string>(
            builder,
            container => container.GetValue<string>("WebPageUrlPath"),
            options);

        var urlPath = results.FirstOrDefault();

        return string.IsNullOrEmpty(urlPath)
            ? null
            : _contentService.EnsureAbsoluteUrl(urlPath);
    }

    // Fallback: resolves URL from a ContentAttachment content item.
    private async Task<string> ResolveAttachmentUrlAsync(IEnumerable<ContentItemReference> references)
    {
        if (references?.Any() != true)
        {
            return null;
        }

        var attachments = await _contentService.RetrieveButtonUrlAsync(
            references.Select(i => i.Identifier).ToList(),
            [ContentAttachment.CONTENT_TYPE_NAME]);

        return attachments.FirstOrDefault() is ContentAttachment contentAttachment
            ? contentAttachment.ContentAttachmentAsset?.Url
            : null;
    }

    // Maps widget properties and resolved URLs to the view DTO.
    private static ImageWithTextData BuildDto(
        ImageWithTextWidgetProperties properties,
        string buttonUrl,
        string imageUrl,
        string backgroundImageUrl)
    {
        return new ImageWithTextData
        {
            Title = properties.Title,
            Description = properties.Description,
            ButtonTitle = properties.ButtonText,
            ButtonURL = buttonUrl,
            PrimaryButtonLinkTarget = properties.PrimaryButtonLinkTarget,
            ButtonType = properties.ButtonType,
            MediaType = properties.MediaType,
            EmbedCode = properties.EmbedCode,
            ImageURL = imageUrl,
            ImageAltText = properties.ImageAltText,
            MediaPosition = properties.MediaPosition,
            BackgroundImageURL = backgroundImageUrl,
            BackgroundImageAltText = string.IsNullOrEmpty(properties.BackgroundImageAltText) ? null : properties.BackgroundImageAltText,
            Theme = properties.Theme,
            TopMargin = properties.TopMargin,
            BottomMargin = properties.BottomMargin,
            WidgetID = properties.WidgetID,
        };
    }
}