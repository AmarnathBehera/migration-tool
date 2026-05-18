using BDO.GWT.Core.Constants;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.Utilities;
using CMS.ContentEngine;
using CMS.Websites;
using Kentico.Content.Web.Mvc;

namespace BDO.GWT.Core.Services.Widget
{
    public interface ITileContentService
    {
        Task<List<TilePageDto>> GetTilePagesAsync(List<Guid> guids, CancellationToken cancellationToken);
    }

    public class TileContentService : ITileContentService
    {
        private readonly IContentRetriever _contentRetriever;
        private readonly IWebPageUrlRetriever _urlRetriever;

        public TileContentService(
            IContentRetriever contentRetriever,
            IWebPageUrlRetriever urlRetriever)
        {
            _contentRetriever = contentRetriever;
            _urlRetriever = urlRetriever;
        }

        public async Task<List<TilePageDto>> GetTilePagesAsync(List<Guid> guids, CancellationToken cancellationToken)
        {
            if (guids == null || !guids.Any())
                return new List<TilePageDto>();

            var contentTypes = new[]
            {
                SectionIndustries.CONTENT_TYPE_NAME,
                IndustryCategory.CONTENT_TYPE_NAME,
                IndustryService.CONTENT_TYPE_NAME,
                Industry.CONTENT_TYPE_NAME,
                SectionServices.CONTENT_TYPE_NAME,
                ServiceArea.CONTENT_TYPE_NAME,
                Sub_Service.CONTENT_TYPE_NAME,
                BusinessLine.CONTENT_TYPE_NAME,
                Service.CONTENT_TYPE_NAME
            };

            var contentItems = await _contentRetriever
                .RetrieveContentOfContentTypesByGuids<IContentItemFieldsSource>(
                    contentTypes,
                    guids,
                    cancellationToken
                );

            var tilePages = new List<TilePageDto>();

            foreach (var item in contentItems)
            {
                var title = item.GetType()
                                .GetProperty("MetadataTitle")
                                ?.GetValue(item)
                                ?.ToString()
                            ?? item.SystemFields.ContentItemName;
                var rawDescription = item.GetType()
                               .GetProperty("MetadataDescription")
                               ?.GetValue(item)
                               ?.ToString();
                var description = !string.IsNullOrEmpty(rawDescription)
              ? HtmlHelper.ExtractTextFromHtml(rawDescription) : null;


                string url = null;

                if (item is IWebPageFieldsSource webPage)
                {
                    var retrievedUrl = await _urlRetriever.Retrieve(webPage, cancellationToken);
                    url = retrievedUrl?.RelativePath;
                }

                tilePages.Add(new TilePageDto
                {
                    Title = title,
                    Description = description,
                    ReadMoreLink = new LinkItemDto
                    {
                        Title = ResourceStringKey.LiveSite.ReadMore,
                        URL = url,
                    },
                });
            }

            return tilePages;
        }
    }
}