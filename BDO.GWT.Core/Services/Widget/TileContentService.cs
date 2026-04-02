using BDO.GWT.Core.DTOs;
using CMS.ContentEngine;
using Kentico.Content.Web.Mvc;
using CMS.ContentEngine;
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

        public TileContentService(IContentRetriever contentRetriever)
        {
            _contentRetriever = contentRetriever;
        }

        public async Task<List<TilePageDto>> GetTilePagesAsync(List<Guid> guids, CancellationToken cancellationToken)
        {
            if (guids == null || !guids.Any())
            {
                return new List<TilePageDto>();
            }

            var contentTypes = new[]
            {
            HomePage.CONTENT_TYPE_NAME,
            Navigation.CONTENT_TYPE_NAME
            };

            var contentItems = await _contentRetriever
                .RetrieveContentOfContentTypesByGuids<IContentItemFieldsSource>(
                    contentTypes,
                    guids,
                    cancellationToken
                );

            return contentItems.Select(x => new TilePageDto
            {
                Title = x.SystemFields.ContentItemName
            }).ToList();
        }
    }
}
