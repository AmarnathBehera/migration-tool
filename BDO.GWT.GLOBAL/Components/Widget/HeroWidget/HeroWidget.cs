using BDO.GWT.Core;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.ValueObjects;
using BDO.GWT.Web.Core.Data;
using BDO.GWT.Web.Core.Services;
using CMS.ContentEngine;
using Kentico.Content.Web.Mvc;
using Kentico.PageBuilder.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BDO.GWT.Web.Components.Widget.HeroWidget;

public class HeroWidget : ViewComponent
{
    public readonly IHeroService _heroService;
    public readonly IContentService _contentService;

    public HeroWidget(IHeroService heroService, IContentService contentService)
    {
        _heroService = heroService;
        _contentService = contentService;
    }

    public async Task<IViewComponentResult> InvokeAsync(ComponentViewModel<HeroWidgetProperties> model)
    {
        var cancellationToken = HttpContext.RequestAborted;

        // Resolve background image URL
        Link backgroundImageUrl = null;
        if (model.Properties?.BackgroundImage.Any() == true)
        {

            var attachment = await _contentService.GetItemAsync<ContentAttachment>(
                model.Properties.BackgroundImage.FirstOrDefault().Identifier,
                cancellationToken);

            backgroundImageUrl = attachment?.ContentAttachmentAsset?.Url is string url
                ? Link.Parse(url)
                : null;
        }

        // Resolve mobile background image URL
        Link mobileBackgroundImageUrl = null;
        if (model.Properties?.MobileBackgroundImage.Any() == true)
        {
            var mobileAttachment = await _contentService.GetItemAsync<ContentAttachment>(
                model.Properties.MobileBackgroundImage.FirstOrDefault().Identifier,
                cancellationToken);

            mobileBackgroundImageUrl = mobileAttachment?.ContentAttachmentAsset?.Url is string mobileUrl
                ? Link.Parse(mobileUrl)
                : null;
        }

        var data = new HeroData
        {
            Layout = model.Properties.Layout,
            Title = model.Properties.Title,
            Description = model.Properties.Description,
            BackgroundImage = backgroundImageUrl,
            BackgroundImageAltText = string.IsNullOrEmpty(model.Properties.BackgroundImageAltText) ? null : model.Properties.BackgroundImageAltText,
            MobileBackgroundImage = mobileBackgroundImageUrl,
            MobileBackgroundImageAltText = string.IsNullOrEmpty(model.Properties.MobileBackgroundImageAltText) ? null : model.Properties.MobileBackgroundImageAltText,
            CallToActionButtonType = model.Properties.CtaButtonType,
            CallToActionButtonText = string.IsNullOrEmpty(model.Properties.CtaButtonText) ? null : model.Properties.CtaButtonText,
            CallToActionButtonUrl = string.IsNullOrEmpty(model.Properties.CtaButtonUrl) ? null : Link.Parse(model.Properties.CtaButtonUrl),
            CtaButtonUrlTarget = model.Properties.CtaButtonUrlTarget,
            WidgetID = model.Properties.WidgetID,
        };

        var dto = await _heroService.GetHeroDtoAsync(data, cancellationToken);
        return View("~/Components/Widget/HeroWidget/_HeroWidget.cshtml", dto);
    }
}