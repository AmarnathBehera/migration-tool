using BDO.GWT.Core.Data;
using BDO.GWT.Web.Core.DTOs;
using Kentico.Content.Web.Mvc;
using Kentico.PageBuilder.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace BDO.GWT.Web.Widgets.FlexibleBannerWidget;

public class FlexibleBannerWidget : ViewComponent
{
    private readonly IContentRetriever _contentRetriever;

    public FlexibleBannerWidget(IContentRetriever contentRetriever)
    {
        _contentRetriever = contentRetriever;
    }
    public async Task<IViewComponentResult> InvokeAsync(ComponentViewModel<FlexibleBannerWidgetProperties> model)
    {

        var imageurl = model.Properties?.BackgroundImage?.Any() == true
            ? await _contentRetriever.RetrieveContentByGuids<ContentAttachment>(
                model.Properties.BackgroundImage.Select(i => i.Identifier).ToList(),
                new RetrieveContentParameters { LinkedItemsMaxLevel = 3 })
            : Enumerable.Empty<ContentAttachment>();
        var dto = new FlexibleBannerData
        {
            Layout = model.Properties.Layout,
            Description = model.Properties.Description,
            BackgroundImage = imageurl.FirstOrDefault()?.ContentAttachmentAsset?.Url,
            BackgroundImageAltText = model.Properties.BackgroundImageAltText,
            Theme = model.Properties.Theme,
            ButtonSide = model.Properties.ButtonSide,
            TopMargin = model.Properties.TopMargin,
            BottomMargin = model.Properties.BottomMargin,
            PrimaryButton = new ButtonDto
            {
                Title = model.Properties.PrimaryButtonText,
                URL = model.Properties.PrimaryButtonUrl,
                Type = model.Properties.PrimaryButtonType,
                Target = model.Properties.PrimaryButtonLinkTarget,
            },
            SecondaryButton = string.IsNullOrEmpty(model.Properties.SecondaryButtonText) || string.IsNullOrEmpty(model.Properties.SecondaryButtonUrl) ? null : new ButtonDto
            {
                Title = model.Properties.SecondaryButtonText,
                URL = model.Properties.SecondaryButtonUrl,
                Type = model.Properties.SecondaryButtonType,
                Target = model.Properties.SecondaryButtonLinkTarget,
            },
            WidgetID = model.Properties.WidgetID,
        };

        return View("~/Components/Widget/FlexibleBanner/_FlexibleBanner.cshtml", dto);
    }
}
