using AutoMapper;
using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using BDO.GWT.AzureCognitiveSearch.Services;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.AzureCognitiveSearch.Models;
using BDO.GWT.Core.Entities.Search;
using BDO.GWT.Core.Extensions;
using BDO.GWT.Core.Helpers;
using BDO.GWT.Core.Retrievers.Search;
using BDO.GWT.Xperience.Core.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SearchDocument = BDO.GWT.Core.Entities.Search.SearchDocument;


namespace BDO.GWT.Core.Services
{
    public interface ITypeAheadSearchService
    {
        Task<IEnumerable<TypeAheadSearchResultDto>> GetSuggestions(string searchText, CancellationToken cancellationToken);
    }
    public class TypeAheadSearchService : ITypeAheadSearchService
    {
        private readonly ICultureContext _cultureContext;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IPageUrlService _pageUrlService;
        private readonly ISearchService _searchService;
        public TypeAheadSearchService(ICultureContext cultureContext, IDateTimeProvider dateTimeProvider, IPageUrlService pageUrlService, ISearchService searchService)
        {
            _cultureContext = cultureContext;
            _dateTimeProvider = dateTimeProvider;
            _pageUrlService = pageUrlService;
            _searchService = searchService;
        }
        public async Task<IEnumerable<TypeAheadSearchResultDto>> GetSuggestions(string searchText, CancellationToken cancellationToken)
        {
            var response = await _searchService.SearchAsync<SearchDocument>(searchText, q =>
            {
                //q.OnlyPublishedPages(_dateTimeProvider);
                q.Select(x => new
                {
                    x.NodeId,
                    x.MetadataTitle,
                });
                q.Where(x => x.LanguageTag == _cultureContext.CurrentLanguageTag);
                q.Take(10);
            }, cancellationToken);

            var urlByPageNodeId = await _pageUrlService.GetPageRelativeUrlByNodeIdAsync(response.Documents.Select(x => x.NodeId), _cultureContext.CurrentLanguageTag, cancellationToken);

            var pageTitleWithUrl = response.Documents
                .Where(x => urlByPageNodeId.ContainsKey(x.NodeId))
                .Select(x => new TypeAheadSearchResultDto
                {
                    Title = x.MetadataTitle ?? string.Empty,
                    Url = urlByPageNodeId[x.NodeId].ToHtmlCompatibleString(),
                });

            return pageTitleWithUrl;
        }
    }
}
