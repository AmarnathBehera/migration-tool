using AngleSharp.Text;
using BDO.GWT.Common.Core.Exceptions;
using BDO.GWT.Core;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.Utilities;
using CMS.Websites.Routing;
using Kentico.Content.Web.Mvc;
using Kentico.PageBuilder.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Xperience.Core.LiveSite.Components.Widgets.InsightAnimationCardWidget;
public class InsightAnimationCardWidget: ViewComponent
{
    private readonly IContentService _contentService;
    private readonly IWebsiteChannelContext _websiteChannelContext;
    private readonly IContentRetriever _contentRetriever;

    public InsightAnimationCardWidget(IContentService contentService,IWebsiteChannelContext websiteChannelContext,IContentRetriever contentRetriever)
    {
        _contentService = contentService;
        _websiteChannelContext = websiteChannelContext;
        _contentRetriever = contentRetriever;
    }

    public async Task<IViewComponentResult> InvokeAsync(ComponentViewModel<InsightAnimationCardWidgetProperties> model)
    {
        var cancellationToken = HttpContext?.RequestAborted ?? CancellationToken.None;
        var channel = _websiteChannelContext;
        if (channel== null)
        {
            throw new NotFoundException($"Site with ID {channel?.WebsiteChannelID} not found.");
        }

        var data = new InsightAnimationCardData
        {
            Title = model.Properties.Title,
            Description = model.Properties.Description,
            InsightCards = model.Properties.InsightCards.Select(x => x.Identifier),
            openInNewTab = model.Properties.openInNewTab,
            SeeAllLinkText = model.Properties.SeeAllLinkText,
            SeeAllLinkUrl = model.Properties.SeeAllLinkURL,
            TopMargin = model.Properties.TopMargin,
            BottomMargin = model.Properties.BottomMargin,
            WidgetID = model.Properties.WidgetID,
        };


      IEnumerable<Insight> insightanimationpages = Enumerable.Empty<Insight>();

        //if (data.InsightCards.Any())
        //{
        //    insightanimationpages = _contentService.RetrievePagesAsync<Insight>(new RetrievePagesParameters { },cancellationToken);
        //}

        var dto = new InsightAnimationCardDto
        {
            Title = data.Title,
            Description = data.Description,
            //Items = insightanimationpages.Select(x => new InsightAnimationPageDto
            //{                
            //    //Title = x.Title,
            //    //Description = HtmlHelper.ExtractTextFromHtml(x.Description),
            //    //ReadMoreIcon = x.Url,
            //    //ImageURL = channel?.WebsiteChannelName + x.Image?.TrimStart('~'),
            //    OpenInNewTab = data.openInNewTab
            //}),            
            SeeAllLink = new LinkItemDto
            {
                Title = data.SeeAllLinkText ?? string.Empty,
                URL = data.SeeAllLinkUrl ?? string.Empty,
                Target = data.openInNewTab
            },
            TopMargin = data.TopMargin ?? "0px",
            BottomMargin = data.BottomMargin ?? "0px",
            WidgetID = data.WidgetID,
        };
        return View("~/Components/Widget/InsightAnimationCardWidget/_InsightAnimationCardWidget.cshtml", dto);
    }
}