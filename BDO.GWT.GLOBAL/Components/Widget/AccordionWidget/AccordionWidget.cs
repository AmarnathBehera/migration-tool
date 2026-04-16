using BDO.GWT.Core.Data;
using BDO.GWT.Web.Core.LiveSite.Data;
using Kentico.PageBuilder.Web.Mvc;
using Microsoft.AspNetCore.Mvc;

namespace BDO.GWT.Web.Widgets.AccordionWidget;

public class AccordionWidget : ViewComponent
{
    public IViewComponentResult Invoke(ComponentViewModel<AccordionWidgetProperties> model)
    {
        var dto = new AccordionData
        {
            TopMargin = model.Properties.TopMargin,
            BottomMargin = model.Properties.BottomMargin,
            WidgetID = model.Properties.WidgetID,
            AccordionItems = new AccordionItem[] { new() { Content = model.Properties.Content, Id = model.Properties.DeepLinkID, Title = model.Properties.Heading } }
        };

        return View("~/Components/Widget/AccordionWidget/_AccordionWidget.cshtml", dto);
    }
}
