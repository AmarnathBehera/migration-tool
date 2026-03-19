//using BDO.GWT.Core;
//using Kentico.PageBuilder.Web.Mvc;
//using Microsoft.AspNetCore.Mvc;
//using System.Linq;
//using System.Threading.Tasks;

//namespace BDO.GWT.Xperience.Core.LiveSite.Components.Widgets.TiledLinkSectionWidget;

//public class TiledLinkSectionWidget : ViewComponent
//{
//    public async Task<IViewComponentResult> InvokeAsync(ComponentViewModel<TiledLinkSectionWidgetProperties> model)
//    {
//        var cancellationToken = HttpContext.RequestAborted;

//        var data = new TiledLinkSectionData
//        {
//            Title = model.Properties.Title,
//            Description = model.Properties.Description,
//            BackgroundImageMediaFileGuid = model.Properties.BackgroundImages.Select(x => x.FileGuid).FirstOrDefault(),
//            BackgroundImageAltText = string.IsNullOrEmpty(model.Properties.BackgroundImageAltText) ? null : model.Properties.BackgroundImageAltText,
//            TileNodeGuids = model.Properties.TilePages.Select(x => x.NodeGuid),
//            Theme = model.Properties.Theme,
//            SeeAllLinkText = model.Properties.SeeAllLinkText,
//            SeeAllLinkUrl = model.Properties.SeeAllLinkURL,
//            SeeAllLinkURLTarget = model.Properties.SeeAllLinkURLTarget,
//            TopMargin = model.Properties.TopMargin,
//            BottomMargin = model.Properties.BottomMargin,
//            WidgetID = model.Properties.WidgetID,
//        };


//        return View("~/Components/Widgets/TiledLinkSectionWidget/_TiledLinkSectionWidget.cshtml", dto);
//    }
//}
