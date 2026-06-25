using BDO.GWT.Core.Data;
using BDO.GWT.Core.Services;
using Kentico.PageBuilder.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace BDO.GWT.GLOBAL.Widgets;

public class KeyContactsWidget : ViewComponent
{
    private readonly IKeyContactsService _keyContactsService;

    public KeyContactsWidget(IKeyContactsService keyContactsService)
    {
        _keyContactsService = keyContactsService;
    }

    public async Task<IViewComponentResult> InvokeAsync(ComponentViewModel<KeyContactsWidgetProperties> model)
    {
        var cancellationToken = HttpContext.RequestAborted;

        var data = new KeyContactsData
        {
            Title = model.Properties.Title,
            Description = model.Properties.Description,
            OrderedPeopleNodeGuids = model.Properties.People ?? [],
            OrderedContributorsNodeGuids = model.Properties.Contributors ?? [],
            IsOrderOverriden = model.Properties.IsOrderOverriden,
            OrderCriteria = model.Properties.OrderCriteria,
            IsDescendingOrder = model.Properties.IsDescendingOrder,
            ShowOffice = model.Properties.ShowOffice,
            SeeAllLinkText = model.Properties.SeeAllLinkText,
            SeeAllLinkUrl = model.Properties.SeeAllLinkURL,
            SeeAllLinkURLTarget = model.Properties.SeeAllLinkURLTarget,
            TopMargin = model.Properties.TopMargin,
            BottomMargin = model.Properties.BottomMargin,
            WidgetID = model.Properties.WidgetID,
        };

        var dto = await _keyContactsService.GetKeyContactsAsync(data, cancellationToken);

        return View("~/Components/Widget/KeyContactsWidget/_KeyContactsWidget.cshtml", dto);
    }
}
