using BDO.GWT.Web.Core.LiveSite.Data;
using BDO.GWT.Web.Core.LiveSite.Services;
using BDO.GWT.Xperience.Core.LiveSite.Helpers;
using Kentico.PageBuilder.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BDO.GWT.Xperience.Core.LiveSite.Components.Widgets.EditorsPickWidget;

public class EditorsPickWidget : ViewComponent
{
    private readonly IEditorsPickService _editorsPickService;
    private readonly IWidgetHelper _widgetHelper;

    public EditorsPickWidget(IEditorsPickService editorsPickService, IWidgetHelper widgetHelper)
    {
        _editorsPickService = editorsPickService;
        _widgetHelper = widgetHelper;
    }

    public async Task<IViewComponentResult> InvokeAsync(ComponentViewModel<EditorsPickWidgetProperties> model)
    {
        var currentPage = _widgetHelper.LinkWidgetPage(model);

        var data = new EditorsPickData
        {
            Title = model.Properties.Title,
            Description = model.Properties.Description,
            FilteringEnabled = model.Properties.FilteringEnabled,
            Filters = model.Properties.Filters?.Split(';').ToHashSet() ?? new HashSet<string>(),
            EditorsPickNodeGuids = model.Properties.Pages.OrderBy(x => x.Order).Select(x => x.NodeGUID),
            FeaturedPageNodeGuids = model.Properties.FeaturedPages.OrderBy(x => x.Order).Select(x => x.NodeGUID),
            BackgroundImageMediaFileGuid = model.Properties.BackgroundImages.Select(x => x.FileGuid).FirstOrDefault(),
            BackgroundImageAltText = string.IsNullOrEmpty(model.Properties.BackgroundImageAltText) ? null : model.Properties.BackgroundImageAltText,
            SeeAllLinkText = model.Properties.SeeAllLinkText,
            SeeAllLinkUrl = model.Properties.SeeAllLinkURL,
            SeeAllLinkURLTarget = model.Properties.SeeAllLinkURLTarget,
            TopMargin = model.Properties.TopMargin,
            BottomMargin = model.Properties.BottomMargin,
            WidgetID = model.Properties.WidgetID,
        };

        var dto = await _editorsPickService.GetEditorsPickDtoAsync(currentPage, data, HttpContext.RequestAborted);

        return View("~/Components/Widget/EditorsPickWidget/_EditorsPickWidget.cshtml", dto);
    }
}
