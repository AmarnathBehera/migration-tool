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
            InsightCards = model.Properties.InsightCards.Select(x => x.Identifier).ToList(),
            openInNewTab = model.Properties.openInNewTab,
            SeeAllLinkText = model.Properties.SeeAllLinkText,
            SeeAllLinkUrl = model.Properties.SeeAllLinkURL,
            TopMargin = model.Properties.TopMargin,
            BottomMargin = model.Properties.BottomMargin,
            WidgetID = model.Properties.WidgetID,
        };


      IEnumerable<InsightAnimationPage> insightanimationpages = Enumerable.Empty<InsightAnimationPage>();

        if (data.InsightCards.Any())
        {
            insightanimationpages = await _contentRetriever.RetrievePagesByGuids<InsightAnimationPage>(data.InsightCards,cancellationToken);
        }

        var dto = new InsightAnimationCardDto
        {
            Title = data.Title,
            Description = data.Description,
            Items = insightanimationpages.Select(x => new InsightAnimationPageDto
            {
                Title = x.Title,
                Description = HtmlHelper.ExtractTextFromHtml(x.Description),
                ReadMoreIcon = x.Url,
                ImageURL = x.Image?.FirstOrDefault()?.ContentAttachmentAsset.Url,
                OpenInNewTab = data.openInNewTab
            }),
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