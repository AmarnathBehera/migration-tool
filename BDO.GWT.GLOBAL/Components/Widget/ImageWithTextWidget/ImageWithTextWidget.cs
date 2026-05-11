using BDO.GWT.Common.Core.LiveSite.Data;
using BDO.GWT.Core.Entities;
using CMS.ContentEngine;
using Kentico.Content.Web.Mvc;
using Kentico.PageBuilder.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using System.Linq;
using System.Threading.Tasks;

namespace BDO.GWT.GLOBAL.Components.Widget.ImageWithTextWidget;

public class ImageWithTextWidget : ViewComponent
{
    private readonly IContentRetriever _contentRetriever;
    private readonly IContentRetrieverWrapper _wrapper;
    public ImageWithTextWidget(IContentRetriever contentRetriever, IContentRetrieverWrapper wrapper)
    {
        _contentRetriever = contentRetriever;
        _wrapper = wrapper;
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

        string url = null;
        if (model.Properties.UrlSource == "manual")
        {
            url = model.Properties.ManualUrl;
        }
        else
        {
            var contentTypes = new[] { ContentAttachment.CONTENT_TYPE_NAME, SocialLinks.CONTENT_TYPE_NAME };

            var buttonurl = model.Properties.ButtonURL?.Any() == true
                ? await _wrapper.RetrieveButtonUrlAsync(
                    model.Properties.ButtonURL.Select(i => i.Identifier).ToList(),
                    contentTypes)
                : Enumerable.Empty<IContentItemFieldsSource>();

            var item = buttonurl.FirstOrDefault();

            if (item is ContentAttachment attachment)
            {
                url = attachment.ContentAttachmentAsset?.Url;
            }
            else if (item is SocialLinks social)
            {
                url = social.Url;
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
            ImageURL = imageurl.FirstOrDefault()?.ContentAttachmentAsset?.Url,
            ImageAltText = model.Properties.ImageAltText,
            MediaPosition = model.Properties.MediaPosition,
            BackgroundImageURL = backgroundimageurl.FirstOrDefault()?.ContentAttachmentAsset?.Url,
            BackgroundImageAltText = string.IsNullOrEmpty(model.Properties.BackgroundImageAltText) ? null : model.Properties.BackgroundImageAltText,
            Theme = model.Properties.Theme,
            TopMargin = model.Properties.TopMargin,
            BottomMargin = model.Properties.BottomMargin,
            WidgetID = model.Properties.WidgetID,
        };

        return View("~/Components/Widget/ImageWithTextWidget/_ImageWithTextWidget.cshtml", dto);
    }
}