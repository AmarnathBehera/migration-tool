using BDO;
using BDO.GWT.Core;
using BDO.GWT.Core.DTOs;
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

[assembly: RegisterWidget(
    ComponentIdentifiers.TiledLink,
    typeof(TiledLinkSectionWidget),
    "Tiled Link Widget",
    typeof(TiledLinkSectionWidgetProperties),
    AllowCache = true,
    IconClass = "icon-xml-tag"
)]

namespace BDO.GWT.Xperience.Core.LiveSite.Components.Widgets.TiledLinkSectionWidget
{
    public class TiledLinkSectionWidget : ViewComponent
    {
        private readonly IContentRetriever _contentRetriever;

        public TiledLinkSectionWidget(IContentRetriever contentRetriever)
        {
            _contentRetriever = contentRetriever;
        }

        public async Task<ViewViewComponentResult> InvokeAsync(ComponentViewModel<TiledLinkSectionWidgetProperties> model)
        {
            var cancellationToken = HttpContext?.RequestAborted ?? CancellationToken.None;

            var tilePages = new List<TilePageDto>();

            // ✅ Background Image
            var image = await GetImage(model.Properties, cancellationToken);

            // ✅ Content Retrieval
            if (model.Properties?.TilePages != null && model.Properties.TilePages.Any())
            {
                var multipleContentTypes = new[]
                {
                    HomePage.CONTENT_TYPE_NAME,
                    Navigation.CONTENT_TYPE_NAME
                };

                var guids = model.Properties.TilePages
                    .Where(x => x.Identifier != Guid.Empty)
                    .Select(x => x.Identifier)
                    .ToList();

                if (guids.Any())
                {
                    var contentItems = await _contentRetriever
                        .RetrieveContentOfContentTypesByGuids<IContentItemFieldsSource>(
                            multipleContentTypes,
                            guids,
                            cancellationToken
                        );

                    tilePages = contentItems.Select(x => new TilePageDto
                    {
                        Title = x.SystemFields.ContentItemName,
                    }).ToList();
                }
            }

            // ✅ Final Model
            var data = new TiledLinkSectionData
            {
                Title = model.Properties?.Title,
                Description = model.Properties?.Description,
                BackgroundImageMediaFileGuid = image?.MediaFile?.Url,
                BackgroundImageAltText = model.Properties?.BackgroundImageAltText,
                Theme = model.Properties?.Theme,
                SeeAllLinkText = model.Properties?.SeeAllLinkText,
                SeeAllLinkUrl = model.Properties?.SeeAllLinkURL,
                SeeAllLinkURLTarget = model.Properties?.SeeAllLinkURLTarget,
                TopMargin = model.Properties?.TopMargin,
                BottomMargin = model.Properties?.BottomMargin,

                TileNodeGuids = tilePages
            }; 
            return View("~/Components/Widget/TiledLinkSectionWidget/_TiledLinkSectionWidget.cshtml", data);
        }

        // =========================
        // ✅ IMAGE RETRIEVAL
        // =========================
        private async Task<MigratedMediaFile> GetImage(TiledLinkSectionWidgetProperties properties, CancellationToken cancellationToken)
        {
            var imageGuid = properties?.BackgroundImages?.FirstOrDefault();

            if (imageGuid == null || imageGuid.Identifier == Guid.Empty)
            {
                return null;
            }

            var result = await _contentRetriever.RetrieveContentByGuids<MigratedMediaFile>(
                new[] { imageGuid.Identifier },
                cancellationToken
            );

            return result.FirstOrDefault();
        }
    }
}