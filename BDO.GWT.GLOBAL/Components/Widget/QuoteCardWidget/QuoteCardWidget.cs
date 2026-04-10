using BDO;

using BDO.GWT.Common.Core.LiveSite.Data;
using BDO.GWT.Core;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Xperience.Core.LiveSite.Components.Widgets.ImageWithTextWidget;
using BDO.GWT.Xperience.Core.LiveSite.Components.Widgets.QuoteCardWidget;
using Kentico.PageBuilder.Web.Mvc;
using Microsoft.AspNetCore.Mvc;

namespace BDO.GWT.Xperience.Core.LiveSite.Components.Widgets.QuoteCardWidget;

public class QuoteCardWidget : ViewComponent
{
    public IViewComponentResult Invoke(ComponentViewModel<QuoteCardWidgetProperties> model)
    {
        var dto = new QuoteCardData
        {
            TopMargin = model.Properties.TopMargin,
            BottomMargin = model.Properties.BottomMargin,
            Data = new QuoteCard
            {
                AuthorsJobTitle = model.Properties.AuthorsJobTitle,
                Button = new LinkItemDto
                {
                    Title = model.Properties.ButtonText!,
                    URL = model.Properties.ButtonUrl!,
                    Target = model.Properties.PrimaryButtonLinkTarget,
                },
                Size = model.Properties.Size,
                Heading = model.Properties.Heading,
                ImageAltText = model.Properties.ImageAltText,
                ImageURL = model.Properties.Image,
                SubHeading = model.Properties.Subheading,
                QuoteText = model.Properties.QuoteText,
                QuotesAuthor = model.Properties.QuotesAuthor,
            }
        };

        return View("~/Components/Widget/QuoteCardWidget/_QuoteCardWidget.cshtml", dto);
    }
}