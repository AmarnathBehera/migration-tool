using BDO.GWT.Core.Data;
using BDO.GWT.Core.Services;
using Kentico.Content.Web.Mvc;
using Kentico.PageBuilder.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace BDO.GWT.GLOBAL.Widgets;

public class InFocusWidget : ViewComponent
{
    private readonly IInFocusService _inFocusService;
    private readonly IContentRetriever _contentRetriever;

    public InFocusWidget(IInFocusService inFocusService, IContentRetriever contentRetriever)
    {
        _inFocusService = inFocusService;
        _contentRetriever = contentRetriever;
    }

    public async Task<IViewComponentResult> InvokeAsync(ComponentViewModel<InFocusWidgetProperties> model)
    {
        var cancellationToken = HttpContext.RequestAborted;

        var imageurl = model.Properties?.ImageURL?.Any() == true
          ? await _contentRetriever.RetrieveContentByGuids<ContentAttachment>(
              model.Properties.ImageURL.Select(i => i.Identifier).ToList(),
              new RetrieveContentParameters { LinkedItemsMaxLevel = 3 })
          : Enumerable.Empty<ContentAttachment>();

        var data = new InFocusData
        {
            Title = model.Properties.Title,
            Subtitle = model.Properties.Subtitle,
            ImageURL = imageurl.FirstOrDefault()?.ContentAttachmentAsset?.Url,
            ImageAltText = model.Properties.ImageAltText,
            PageGuids = model.Properties.Pages?.Select(p => p.Identifier) ?? [],
            TopMargin = model.Properties.TopMargin,
            BottomMargin = model.Properties.BottomMargin,
            WidgetID = model.Properties.WidgetID,
        };

        var dto = await _inFocusService.GetInFocusAsync(data, cancellationToken);

        return View("~/Components/Widget/InFocusWidget/_InFocusWidget.cshtml", dto);
    }
}
