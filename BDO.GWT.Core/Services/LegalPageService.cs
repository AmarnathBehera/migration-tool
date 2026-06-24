using BDO.GWT.Core;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Pages;
using BDO.GWT.Core.Utilities;
using CMS.Websites;
using Kentico.Content.Web.Mvc;

namespace BDO.GWT.Web.Core.Services
{
    public interface ILegalPageService
    {
        Task<LegalPageDto> GetLegalPageDtoAsync(CancellationToken cancellationToken);
    }

    public class LegalPageService : ILegalPageService
    {
        private readonly IContentService _contentService;
        private readonly IWebPageUrlRetriever _webPageUrlRetriever;

        public LegalPageService(IContentService contentService, IWebPageUrlRetriever webPageUrlRetriever)
        {
            _contentService = contentService;
            _webPageUrlRetriever = webPageUrlRetriever;
        }

        public async Task<LegalPageDto> GetLegalPageDtoAsync(CancellationToken cancellationToken)
        {
            var currentPage = await _contentService.RetrieveCurrentPageAsync<LegalPage>(
                new RetrieveCurrentPageParameters(),
                cancellationToken);

            if (currentPage is null)
            {
                return new LegalPageDto();
            }

            return new LegalPageDto
            {
                Title = currentPage.MetadataTitle,
                BodyContent = HtmlUtility.RemoveTildeFromUrlTypeHtmlAttributes(currentPage.MetadataDescription) ?? string.Empty,
                Links = await GetLinksDtoAsync(currentPage, cancellationToken),
            };
        }

        private async Task<IEnumerable<LinkItemDto>> GetLinksDtoAsync(LegalPage legalPage, CancellationToken cancellationToken)
        {
            var sectionLegalWebPageItemId = legalPage.SystemFields.WebPageItemParentID;

            if (sectionLegalWebPageItemId <= 0)
            {
                return Enumerable.Empty<LinkItemDto>();
            }

            var siblingPages = (await _contentService.GetChildPagesByParentGuidAsync<LegalPage>(
                sectionLegalWebPageItemId,
                cancellationToken)).ToArray();

            if (siblingPages.Length == 0)
            {
                return Enumerable.Empty<LinkItemDto>();
            }

            var WebPageItemIDs = siblingPages.Select(p => p.SystemFields.WebPageItemID);

            var relativeUrlByWebPageItemId = await GetRelativeUrlsAsync(
                WebPageItemIDs, _contentService.GetCurrentLanguage(), cancellationToken);

            return siblingPages
                .Where(page => relativeUrlByWebPageItemId.ContainsKey(page.SystemFields.WebPageItemID))
                .Select(page => new LinkItemDto
                {
                    Title = page.MetadataTitle,
                    URL = relativeUrlByWebPageItemId[page.SystemFields.WebPageItemID],
                });
        }

        private async Task<IDictionary<int, string>> GetRelativeUrlsAsync(
            IEnumerable<int> webPageItemIds,
            string languageCode,
            CancellationToken cancellationToken)
        {
            var urlByWebPageItemId = new Dictionary<int, string>();

            foreach (var id in webPageItemIds.Distinct())
            {
                cancellationToken.ThrowIfCancellationRequested();
                    var url = await _webPageUrlRetriever.Retrieve(id, languageCode, cancellationToken: cancellationToken);
                    urlByWebPageItemId[id] = url.RelativePath;                               
            }

            return urlByWebPageItemId;
        }
    }
}