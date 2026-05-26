using BDO.GWT.Core.Data;
using BDO.GWT.Core.Extensions;
using BDO.GWT.Core.Services.Widget;
using BDO.GWT.GLOBAL.Components.Widget.FaqWidget;
using Kentico.PageBuilder.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Linq;
using System.Threading.Tasks;

namespace BDO.GWT.Web.Widgets.AccordionWidget;

public class FaqWidget : ViewComponent
{
    private readonly IFaqService _faqService;

    public FaqWidget(IFaqService faqService)
    {
        _faqService = faqService;
    }

    public async Task<IViewComponentResult> InvokeAsync(ComponentViewModel<FaqWidgetProperties> model)
    {
        var cancellationToken = HttpContext.RequestAborted;

        var data = new FaqData
        {
            Title = model.Properties.Title,
            Layout = model.Properties.Layout,
            PageGuids = model.Properties.Pages?.Select(x => x.Identifier) ?? [],
            HoverColour = model.Properties.HoverColour,
            TopMargin = model.Properties.TopMargin,
            BottomMargin = model.Properties.BottomMargin,
            WidgetID = model.Properties.WidgetID,
        };


        var dto = await _faqService.GetFaqAsync(data, cancellationToken);
        var faqSchema = _faqService.GetFaqSchemaOrgAsync(dto);

        HttpContext.AddLdJsonSchema(faqSchema);


        return View("~/Components/Widget/FaqWidget/_FaqWidget.cshtml", dto);
    }
}
