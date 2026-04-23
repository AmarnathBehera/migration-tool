using BDO.GWT.Core.DTOs;
using CMS.ContentEngine;
using CMS.Websites;
using Kentico.Content.Web.Mvc;

namespace BDO.GWT.Core.Services.Widget
{
    public interface IButtonUrlRetriever
    {
        Task<List<LinkItemDto>> GetTilePagesAsync(List<Guid> guids, CancellationToken cancellationToken);
    }


    public class ButtonUrlRetriever : IButtonUrlRetriever
    {
        private readonly IContentRetriever _contentRetriever;

        public ButtonUrlRetriever(IContentRetriever contentRetriever)
        {
            _contentRetriever = contentRetriever;
        }

        public async Task<List<LinkItemDto>> GetTilePagesAsync(List<Guid> guids, CancellationToken cancellationToken)
        {
            if (guids == null || guids.Count == 0)
            {
                return new List<LinkItemDto>();
            }

            var contentTypes = new[]
            {
            HomePage.CONTENT_TYPE_NAME,
            Navigation.CONTENT_TYPE_NAME
            };

            var contentItems = await _contentRetriever
                .RetrieveContentOfContentTypesByGuids<IWebPageFieldsSource>(
                    contentTypes,
                    guids,
                    cancellationToken
                );

            return contentItems.Select(x => new LinkItemDto
            {
                Title = x.SystemFields.WebPageItemName,
                URL = x.SystemFields.WebPageUrlPath,
            }).ToList();
        }
    }
}
