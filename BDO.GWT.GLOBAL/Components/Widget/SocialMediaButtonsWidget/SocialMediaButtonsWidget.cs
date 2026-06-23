
using BDO.GWT.Core;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.Services;
using CMS.Websites;
using Kentico.Content.Web.Mvc;
using Kentico.PageBuilder.Web.Mvc;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BDO.GWT.Web.Widgets.SocialMediaButtonsWidget;

public class SocialMediaButtonsWidget : ViewComponent
{
    private readonly ISocialMediaButtonsService _socialMediaButtonsService;
    private readonly IContentService _contentService;
    private readonly IWebPageUrlRetriever _webPageUrlRetriever;
    private readonly ICultureContext _cultureContext;

    public SocialMediaButtonsWidget(ISocialMediaButtonsService socialMediaButtonsService, IContentService contentService, IWebPageUrlRetriever webPageUrlRetriever, ICultureContext cultureContext)
    {
        _socialMediaButtonsService = socialMediaButtonsService;
        _contentService = contentService;
        _webPageUrlRetriever = webPageUrlRetriever;
        _cultureContext = cultureContext;
    }

    public async Task<IViewComponentResult> InvokeAsync(ComponentViewModel<SocialMediaButtonsWidgetProperties> model)
    {
        var currentPage = await _contentService.RetrieveCurrentPageAsync<IWebPageFieldsSource>(
             new RetrieveCurrentPageParameters { LinkedItemsMaxLevel = 4 },
             HttpContext.RequestAborted);

        if (currentPage == null)
        {
            return View("~/Components/Widget/SocialMediaButtonsWidget/_SocialMediaButtonsWidget.cshtml",
                new SocialMediaButtonsWidgetDto
                {
                    TwitterButtonUrl = string.Empty,
                    LinkedInUrl = string.Empty,
                    ShowCopyLinkButton = false,
                });
        }

        string pageAbsoluteUrl = string.Empty;
        
        string pageTitle = string.Empty;


            var url = await _webPageUrlRetriever.Retrieve(
                currentPage,
                _cultureContext.CurrentLanguageTag,
                HttpContext.RequestAborted);

            pageAbsoluteUrl = url?.AbsoluteUrl ?? string.Empty;

            pageTitle = currentPage.GetType()
                .GetProperty("MetadataTitle")
                ?.GetValue(currentPage) as string ?? string.Empty;
        

        var dto = await _socialMediaButtonsService.GetSocialMediaButtonsDtoNB(pageAbsoluteUrl,pageTitle, HttpContext.RequestAborted);

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
