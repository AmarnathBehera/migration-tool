using BDO;
using BDO.GWT.Common.Core.LiveSite.Data;
using BDO.GWT.Xperience.Core.LiveSite.Components.Widgets.ExternalNavigationWidget;
using BDO.GWT.Xperience.Core.LiveSite.Components.Widgets.ImageWithTextWidget;
using Kentico.PageBuilder.Web.Mvc;
using Microsoft.AspNetCore.Mvc;

[assembly: RegisterWidget(
    ComponentIdentifiers.Imagewithtext,
    typeof(ImageWithTextWidget),
    "ImageWithText",
    typeof(ImageWithTextWidgetProperties),
    AllowCache = true,
    IconClass = "icon-xml-tag"
)]

namespace BDO.GWT.Xperience.Core.LiveSite.Components.Widgets.ImageWithTextWidget;

public class ImageWithTextWidget : ViewComponent
{
    public IViewComponentResult Invoke(ComponentViewModel<ImageWithTextWidgetProperties> model)
    {
        var dto = new ImageWithTextData
        {
            Title = model.Properties.Title,
            Description = model.Properties.Description,
            ButtonTitle = model.Properties.ButtonText,
            ButtonURL = model.Properties.ButtonURL,
            PrimaryButtonLinkTarget = model.Properties.PrimaryButtonLinkTarget,
            ButtonType = model.Properties.ButtonType,
            MediaType = model.Properties.MediaType,
            EmbedCode = model.Properties.EmbedCode,
            ImageURL = model.Properties.ImageURL,
            ImageAltText = model.Properties.ImageAltText,
            MediaPosition = model.Properties.MediaPosition,
            BackgroundImageURL = model.Properties.BackgroundImageURL,
            BackgroundImageAltText = string.IsNullOrEmpty(model.Properties.BackgroundImageAltText) ? null : model.Properties.BackgroundImageAltText,
            Theme = model.Properties.Theme,
            TopMargin = model.Properties.TopMargin,
            BottomMargin = model.Properties.BottomMargin,
        };
        return View("~/Components/Widget/ImageWithTextWidget/_ImageWithTextWidget.cshtml", dto);
    }
}
