
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.Services;
using Kentico.PageBuilder.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BDO.GWT.Web.Widgets.SocialMediaButtonsWidget;

public class SocialMediaButtonsWidget : ViewComponent
{
    private readonly ISocialMediaButtonsService _socialMediaButtonsService;
    private readonly IWidgetHelper _widgetHelper;

    public SocialMediaButtonsWidget(ISocialMediaButtonsService socialMediaButtonsService, IWidgetHelper widgetHelper)
    {
        _socialMediaButtonsService = socialMediaButtonsService;
        _widgetHelper = widgetHelper;
    }

    public async Task<IViewComponentResult> InvokeAsync(ComponentViewModel<SocialMediaButtonsWidgetProperties> model)
    {
        var currentPage = _widgetHelper.LinkWidgetPage(model);

        var dto = await _socialMediaButtonsService.GetSocialMediaButtonsDto(currentPage, HttpContext.RequestAborted);

        var widgetDto = new SocialMediaButtonsWidgetDto
        {
            EmailButtonUrl = dto.EmailButtonUrl,
            FacebookButtonUrl = dto.FacebookButtonUrl,
            TwitterButtonUrl = dto.TwitterButtonUrl,
            LinkedInUrl = dto.LinkedInUrl,
            ShowCopyLinkButton = dto.ShowCopyLinkButton,
            TopMargin = model.Properties.TopMargin,
            BottomMargin = model.Properties.BottomMargin,
            WidgetID = model.Properties.WidgetID,
        };

        return View("~/Components/Widget/SocialMediaButtonsWidget/_SocialMediaButtonsWidget.cshtml", widgetDto);
    }
}
