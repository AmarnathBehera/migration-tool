using BDO.GWT.Core.Data.ImageWithTextData;
using BDO.GWT.Core;
using CMS.ContentEngine;
using Kentico.Content.Web.Mvc;
using Kentico.PageBuilder.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using System.Linq;
using System.Threading.Tasks;
using CMS.Websites;
using CMS.Websites.Routing;

namespace BDO.GWT.Web.Widgets.ImageWithTextWidget;

public class ImageWithTextWidget : ViewComponent
{
    private readonly IContentRetriever _contentRetriever;
    private readonly IContentService _contentService;
    private readonly IContentQueryExecutor _queryExecutor;
    private readonly IWebsiteChannelContext _channelContext;

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
            return View("~/Components/Widget/ImageWithTextWidget/_ImageWithTextWidget.cshtml", new ImageWithTextData());
        }

        var imageurl = model.Properties?.ImageURL?.Any() == true
            ? await _contentRetriever.RetrieveContentByGuids<ContentAttachment>(
                model.Properties.ImageURL.Select(i => i.Identifier).ToList(),
                new RetrieveContentParameters { LinkedItemsMaxLevel = 3 })
            : Enumerable.Empty<ContentAttachment>();

        var backgroundimageurl = model.Properties?.BackgroundImageURL?.Any() == true
            ? await _contentRetriever.RetrieveContentByGuids<ContentAttachment>(
                model.Properties.BackgroundImageURL.Select(i => i.Identifier).ToList(),
                new RetrieveContentParameters { LinkedItemsMaxLevel = 3 })
            : Enumerable.Empty<ContentAttachment>();

        string? url = null;

        if (model.Properties.UrlSource == "manual")
        {
            url = model.Properties.ManualUrl;
        }
        else
        {
            var buttonGuids = model.Properties.ButtonURL?.Select(i => i.Identifier).ToList() ?? [];

            if (buttonGuids.Count > 0)
            {
                var contentItemGuid = buttonGuids.FirstOrDefault();

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

                if (!string.IsNullOrEmpty(urlPath))
                {
                    url = _contentService.EnsureAbsoluteUrl("/" + urlPath.TrimStart('/'));
                }

                if (url is null)
                {
                    var attachments = model.Properties.ButtonURL?.Any() == true
                        ? await _contentService.RetrieveButtonUrlAsync(
                            model.Properties.ButtonURL.Select(i => i.Identifier).ToList(),
                            [ContentAttachment.CONTENT_TYPE_NAME])
                        : Enumerable.Empty<IContentItemFieldsSource>();

                    var attachment = attachments.FirstOrDefault();

                    if (attachment is ContentAttachment contentAttachment)
                    {
                        url = contentAttachment?.ContentAttachmentAsset?.Url;
                    }
                }
            }
        }
        var dto = new ImageWithTextData
        {
            Title = model.Properties.Title,
            Description = model.Properties.Description,
            ButtonTitle = model.Properties.ButtonText,
            ButtonURL = url,
            PrimaryButtonLinkTarget = model.Properties.PrimaryButtonLinkTarget,
            ButtonType = model.Properties.ButtonType,
            MediaType = model.Properties.MediaType,
            EmbedCode = model.Properties.EmbedCode,
            ImageURL = imageurl.FirstOrDefault()?.ContentAttachmentAsset?.Url.TrimStart('~'),
            ImageAltText = model.Properties.ImageAltText,
            MediaPosition = model.Properties.MediaPosition,
            BackgroundImageURL = backgroundimageurl.FirstOrDefault()?.ContentAttachmentAsset?.Url.TrimStart('~'),
            BackgroundImageAltText = string.IsNullOrEmpty(model.Properties.BackgroundImageAltText) ? null : model.Properties.BackgroundImageAltText,
            Theme = model.Properties.Theme,
            TopMargin = model.Properties.TopMargin,
            BottomMargin = model.Properties.BottomMargin,
            WidgetID = model.Properties.WidgetID,
        };

        return View("~/Components/Widget/ImageWithTextWidget/_ImageWithTextWidget.cshtml", dto);
    }

}