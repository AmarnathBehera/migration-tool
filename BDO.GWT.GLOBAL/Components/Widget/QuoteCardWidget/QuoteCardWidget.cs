using BDO;

using BDO.GWT.Common.Core.LiveSite.Data;
using BDO.GWT.Core;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.Services.Widget;
using BDO.GWT.Xperience.Core.LiveSite.Components.Widgets.QuoteCardWidget;
using CMS.ContentEngine;
using Kentico.Content.Web.Mvc;
using Kentico.PageBuilder.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Xperience.Core.LiveSite.Components.Widgets.QuoteCardWidget;

public class QuoteCardWidget : ViewComponent
{
    private readonly IContentRetriever _contentRetriever;
    private readonly IButtonUrlRetriever _buttonUrlRetriever;

    public QuoteCardWidget(IContentRetriever contentRetriever, IButtonUrlRetriever buttonUrlRetriever)
    {
        _contentRetriever = contentRetriever;
        _buttonUrlRetriever = buttonUrlRetriever;
    }

    public async Task<IViewComponentResult> InvokeAsync(ComponentViewModel<QuoteCardWidgetProperties> model)
    {
        var cancellationToken = HttpContext.RequestAborted;

        var buttonUrl = new List<LinkItemDto>();

        if (model?.Properties?.ButtonUrl != null && model.Properties.ButtonUrl.Any())
        {
            var guids = model.Properties.ButtonUrl
                .Where(x => x.Identifier != Guid.Empty)
                .Select(x => x.Identifier)
                .ToList();

            buttonUrl = await _buttonUrlRetriever
                .GetTilePagesAsync(guids, cancellationToken);
        }


        var imageUrl = await GetImageUrlAsync(model.Properties, cancellationToken);

        var dto = new QuoteCardData
        {
            TopMargin = model.Properties.TopMargin,
            BottomMargin = model.Properties.BottomMargin,
            Data = new QuoteCard
            {
                AuthorsJobTitle = model.Properties.AuthorsJobTitle,
                Button = new LinkItemDto
                {
                    Title = model.Properties.ButtonText!,
                    URL = buttonUrl.FirstOrDefault()?.URL,
                    Target = model.Properties.PrimaryButtonLinkTarget,
                },
                Size = model.Properties.Size,
                Heading = model.Properties.Heading,
                ImageAltText = model.Properties.ImageAltText,
                ImageURL = imageUrl,
                SubHeading = model.Properties.Subheading,
                QuoteText = model.Properties.QuoteText,
                QuotesAuthor = model.Properties.QuotesAuthor,
            }
        };

        return View("~/Components/Widget/QuoteCardWidget/_QuoteCardWidget.cshtml", dto);
    }

    private async Task<string?> GetImageUrlAsync(
        QuoteCardWidgetProperties properties,
        CancellationToken cancellationToken)
    {
        var imageRef = properties.Image?.FirstOrDefault();

        if (imageRef == null || imageRef.Identifier == Guid.Empty)
        {
            return null;
        }

        var result = await _contentRetriever.RetrieveContentByGuids<MigratedMediaFile>(
            new[] { imageRef.Identifier },
            cancellationToken
        );

        return result.FirstOrDefault()?.MediaFile?.Url;
    }
}