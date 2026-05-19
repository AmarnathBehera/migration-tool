using BDO;
using BDO.GWT.Core;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.Services.Widget;
using BDO.GWT.Web.Core.DTOs;
using BDO.GWT.Web.Core.LiveSite.Data;
using BDO.GWT.Xperience.Core.LiveSite.Components.Widgets.TiledLinkSectionWidget;
using CMS.ContentEngine;
using CMS.Websites;
using Kentico.Content.Web.Mvc;
using Kentico.PageBuilder.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Xperience.Core.LiveSite.Components.Widgets.TiledLinkSectionWidget
{
    public class TiledLinkSectionWidget : ViewComponent
    {
        private readonly IContentRetriever _contentRetriever;
        private readonly ITileContentService _tileContentService;

        public TiledLinkSectionWidget(
            IContentRetriever contentRetriever,
            ITileContentService tileContentService)
        {
            _contentRetriever = contentRetriever;
            _tileContentService = tileContentService;
        }

        public async Task<ViewViewComponentResult> InvokeAsync(
            ComponentViewModel<TiledLinkSectionWidgetProperties> model)
        {
            var cancellationToken = HttpContext?.RequestAborted ?? CancellationToken.None;

            var tilePages = new List<TilePageDto>();

            if (model?.Properties?.TilePagesIndustry != null && model.Properties.TilePagesIndustry.Any())
            {
                var guids = model.Properties.TilePagesIndustry
                    .Where(x => x.Identifier != Guid.Empty)
                    .Select(x => x.Identifier)
                    .ToList();

                tilePages = await _tileContentService
                    .GetTilePagesAsync(guids, cancellationToken);
            }
            if (model?.Properties?.TilePagesService != null && model.Properties.TilePagesService.Any())
            {
                var guids = model.Properties.TilePagesService
                    .Where(x => x.Identifier != Guid.Empty)
                    .Select(x => x.Identifier)
                    .ToList();

                tilePages = await _tileContentService
                    .GetTilePagesAsync(guids, cancellationToken);
            }


            var image = await GetImage(model.Properties, cancellationToken);
            var dto = new TiledLinkSectionDto
            {
                Title = model.Properties?.Title,
                Description = model.Properties?.Description,
                BackgroundImageURL = image?.ContentAttachmentAsset?.Url,
                BackgroundImageAltText = model.Properties?.BackgroundImageAltText,
                Items = tilePages,
                Theme = model.Properties?.Theme,
                SeeAllLink = model.Properties.SeeAllLinkText != null && model.Properties.SeeAllLinkURL != null ? new LinkItemDto
                {
                    Title = model.Properties.SeeAllLinkText,
                    URL = model.Properties.SeeAllLinkURL,
                    Target = model.Properties.SeeAllLinkURLTarget,
                } : null,
                TopMargin = model.Properties?.TopMargin,
                BottomMargin = model.Properties?.BottomMargin,
                WidgetID = model.Properties.WidgetID,
            };


            return View(
                "~/Components/Widget/TiledLinkSectionWidget/_TiledLinkSectionWidget.cshtml",
                dto
            );
        }

        private async Task<ContentAttachment> GetImage(
            TiledLinkSectionWidgetProperties properties,
            CancellationToken cancellationToken)
        {
            var imageGuid = properties?.BackgroundImages?.FirstOrDefault();

            if (imageGuid == null || imageGuid.Identifier == Guid.Empty)
            {
                return null;
            }

            var result = await _contentRetriever.RetrieveContentByGuids<ContentAttachment>(
                new[] { imageGuid.Identifier },
                cancellationToken
            );

            return result.FirstOrDefault();
        }
    }
}


