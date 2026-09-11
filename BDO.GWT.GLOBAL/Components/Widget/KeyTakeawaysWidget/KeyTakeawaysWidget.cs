using BDO.GWT.Core.DTOs;
using Kentico.PageBuilder.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BDO.GWT.GLOBAL.Components.Widget.KeyTakeawaysWidget;

public class KeyTakeawaysWidget : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync(ComponentViewModel<KeyTakeawaysWidgetProperties> model)
    {
        var data = new KeyTakeawaysData
        {
            Title = model.Properties.Title,
            Content = model.Properties.Content,
            BackgroundColour = model.Properties.BackgroundColour,
            ContentAlignment = model.Properties.ContentAlignment,
            ContentWidth = model.Properties.ContentWidth,
            TopMargin = model.Properties.TopMargin,
            BottomMargin = model.Properties.BottomMargin,
            WidgetID = model.Properties.WidgetID,
        };

        var dto = new KeyTakeawayDto
        {
            Title = data.Title,
            Content = data.Content,
            BackgroundColour = data.BackgroundColour,
            ContentAlignment = data.ContentAlignment,
            ContentWidth = data.ContentWidth,
            TopMargin = data.TopMargin,
            BottomMargin = data.BottomMargin,
            WidgetID = data.WidgetID,
        };

        return View("~/Components/Widget/KeyTakeawaysWidget/_KeyTakeawaysWidget.cshtml", dto);
    }
}
