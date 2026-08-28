using AutoMapper;
using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using BDO.GWT.Core.DTOs.AzureCognitiveSearch.Models;
using BDO.GWT.Core.Entities.Search;
using BDO.GWT.Core.Extensions;
using BDO.GWT.Core.Helpers;
using BDO.GWT.Core.Retrievers.Search;
using BDO.GWT.Xperience.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SearchDocument = BDO.GWT.Core.Entities.Search.SearchDocument;

namespace BDO.GWT.AzureCognitiveSearch.Services
{
    public interface ISearchService
    {
        public Task<SuggestSearchResponse<T>> SuggestAsync<T>(string searchText, string suggesterName, Action<SuggestSearchQuery<T>>? decorateQuery = null, CancellationToken cancellationToken = default)
            where T : SearchDocument;

        Task<SearchResponse<T>> SearchAsync<T>(string searchText, Action<SearchQuery<T>>? decorateQuery = null, CancellationToken cancellationToken = default)
            where T : SearchDocument;
    }
    public class SearchService : ISearchService
    {
        private readonly IMapper _mapper;
        private readonly ISearchClientBuilder _searchClientBuilder;
        private readonly ISearchQueryHelper _searchQueryHelper;
        private readonly ISiteContext _siteContext;

        public SearchService(IMapper mapper, ISearchClientBuilder searchClientBuilder, ISearchQueryHelper searchQueryHelper, ISiteContext siteContext)
        {
            _mapper = mapper;
            _searchClientBuilder = searchClientBuilder;
            _searchQueryHelper = searchQueryHelper;
            _siteContext = siteContext;
        }

        public async Task<SearchResponse<TEntity>> SearchAsync<TEntity>(string searchText, Action<SearchQuery<TEntity>>? decorateQuery = null, CancellationToken cancellationToken = default)
            where TEntity : SearchDocument
        {
            var query = new SearchQuery<TEntity>();

            decorateQuery?.Invoke(query);

            var client = _searchClientBuilder.CreateSearchClient(_siteContext.CurrentSiteName);

            var searchOptions = _searchQueryHelper.MapSearchQueryToSearchOptions(query);

            var response = await client.SearchAsync<AzureSearchDocument>(searchText, searchOptions, cancellationToken);

            var mappedResponse = await MapSearchResponseAsync<TEntity>(response);

            return mappedResponse;
        }

        public async Task<SuggestSearchResponse<TEntity>> SuggestAsync<TEntity>(string searchText, string suggesterName, Action<SuggestSearchQuery<TEntity>>? decorateQuery = null, CancellationToken cancellationToken = default)
            where TEntity : SearchDocument
        {
            var query = new SuggestSearchQuery<TEntity>();

            decorateQuery?.Invoke(query);

            var client = _searchClientBuilder.CreateSearchClient(_siteContext.CurrentSiteName);

            var suggestOptions = _searchQueryHelper.MapSuggestSearchQueryToSuggestSearchOptions(query);

            var response = await client.SuggestAsync<AzureSearchDocument>(searchText, suggesterName, suggestOptions, cancellationToken);

            var mappedResponse = MapSuggestResponse<TEntity>(response);

            return mappedResponse;
        }

        private async Task<SearchResponse<TEntity>> MapSearchResponseAsync<TEntity>(Response<SearchResults<AzureSearchDocument>> response)
            where TEntity : SearchDocument
        {
            var typeMap = _mapper.GetTypeMap<AzureSearchDocument, TEntity>();
            
            var mappedFacetResults = response.Value.Facets?.ToDictionary(x => SearchQueryHelper.MapToPropertyName<TEntity>(typeMap, x.Key), x => x.Value.Select(y => new SearchFacet
            {
                Value = y.Value,
                Count = y.Count,
            }));

            var mappedDocuments = new List<TEntity>();

            //Can be performance issue due to possible multiple requests to Azure API
            await foreach (var searchResult in response.Value.GetResultsAsync())
            {
                try
                {                
                    var mappedDocument = _mapper.Map<TEntity>(searchResult.Document);
                    mappedDocuments.Add(mappedDocument);
                }
                catch (Exception ex)
                {
                    // Handle the exception (e.g., log it, rethrow it, etc.)
                    throw new Exception($"Error mapping search result for document with key '{searchResult.Document}': {ex.Message}", ex);
                }
                
            }

            var suggestSearchResponse = new SearchResponse<TEntity>
            {
                Documents = mappedDocuments,
                Facets = mappedFacetResults ?? new Dictionary<string, IEnumerable<SearchFacet>>(),
                Count = response.Value.TotalCount,
            };

            return suggestSearchResponse;
        }

        private SuggestSearchResponse<TEntity> MapSuggestResponse<TEntity>(Response<SuggestResults<AzureSearchDocument>> response)
            where TEntity : SearchDocument
        {
            var mappedSuggestResults = response.Value.Results.Select(x => new SuggestResult<TEntity>
            {
                Text = x.Text,
                Document = _mapper.Map<TEntity>(x.Document),
            }).ToList();

            var suggestSearchResponse = new SuggestSearchResponse<TEntity>
            {
                Results = mappedSuggestResults,
            };

            return suggestSearchResponse;
        }
    }
}
