using AutoMapper;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using BDO.GWT.Core.DTOs.AzureCognitiveSearch.Models;
using BDO.GWT.Common.Core.Helpers;
using BDO.GWT.Core.Extensions;
using BDO.GWT.Domain.Core.Helpers;
using BDO.GWT.Core.Retrievers.Search;
using BDO.GWT.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;
using SearchDocument = BDO.GWT.Core.Entities.Search.SearchDocument;

namespace BDO.GWT.Core.Helpers
{
    public interface ISearchQueryHelper
    {
        SuggestOptions MapSuggestSearchQueryToSuggestSearchOptions<TEntity>(ISuggestSearchQuery<TEntity> suggestSearchQuery)
            where TEntity : SearchDocument;

        SearchOptions MapSearchQueryToSearchOptions<TEntity>(ISearchQuery<TEntity> searchQuery)
            where TEntity : SearchDocument;
    }

    public class SearchQueryHelper : ISearchQueryHelper
    {
        private readonly IMapper _mapper;

        public SearchQueryHelper(IMapper mapper)
        {
            _mapper = mapper;
        }

        public SuggestOptions MapSuggestSearchQueryToSuggestSearchOptions<TEntity>(ISuggestSearchQuery<TEntity> suggestSearchQuery)
            where TEntity : SearchDocument
        {
            var typeMap = _mapper.GetTypeMap<TEntity, AzureSearchDocument>();

            var odataHelper = new ODataSearchHelper<TEntity>(typeMap);

            var suggestOptions = new SuggestOptions
            {
                Size = suggestSearchQuery.TakeCount,
                UseFuzzyMatching = true,
            };

            if (suggestSearchQuery.WhereConditions.Count > 0)
            {
                var odataFilter = odataHelper.ConvertToODataFilterString(suggestSearchQuery.WhereConditions);
                suggestOptions.Filter = odataFilter;
            }

            if (suggestSearchQuery.OrderByList is not null)
            {
                var orderList = odataHelper.ConvertToODataOrderStringList(suggestSearchQuery.OrderByList);
                AddRangeToICollection(suggestOptions.OrderBy, orderList);
            }

            if (suggestSearchQuery.SelectPropertyNames is not null)
            {
                var selectList = suggestSearchQuery.SelectPropertyNames.Select(x => MapToJsonPropertyName<TEntity>(typeMap, x));
                AddRangeToICollection(suggestOptions.Select, selectList);
            }

            return suggestOptions;
        }

        public SearchOptions MapSearchQueryToSearchOptions<TEntity>(ISearchQuery<TEntity> searchQuery)
            where TEntity : SearchDocument
        {
            var typeMap = _mapper.GetTypeMap<TEntity, AzureSearchDocument>();
            var odataHelper = new ODataSearchHelper<TEntity>(typeMap);

            var searchOptions = new SearchOptions
            {
                Skip = searchQuery.SkipCount,
                Size = searchQuery.TakeCount,
                IncludeTotalCount = searchQuery.IsTotalCountIncluded,
                QueryType = SearchQueryType.Simple,
            };

            if (searchQuery.WhereConditions.Count > 0)
            {
                var odataFilter = odataHelper.ConvertToODataFilterString(searchQuery.WhereConditions);
                searchOptions.Filter = odataFilter;
            }

            if (searchQuery.OrderByList is not null)
            {
                var orderList = odataHelper.ConvertToODataOrderStringList(searchQuery.OrderByList);
                AddRangeToICollection(searchOptions.OrderBy, orderList);
            }

            if (searchQuery.SelectPropertyNames is not null)
            {
                var selectList = searchQuery.SelectPropertyNames.Select(x => MapToJsonPropertyName<TEntity>(typeMap, x));
                AddRangeToICollection(searchOptions.Select, selectList);
            }

            if (searchQuery.SearchPropertyNames is not null)
            {
                var facetList = searchQuery.SearchPropertyNames.Select(x => MapToJsonPropertyName<TEntity>(typeMap, x));
                AddRangeToICollection(searchOptions.SearchFields, facetList);
            }

            if (searchQuery.Facets.Count > 0)
            {
                AddRangeToICollection(searchOptions.Facets, odataHelper.ConvertToODataSearchFacetList(searchQuery.Facets));
            }

            return searchOptions;
        }

        public static string MapToJsonPropertyName<TEntity>(TypeMap typeMap, string propertyName)
            where TEntity : SearchDocument
        {
            var mappedPropertyName = MapperHelper.MapToColumnName<TEntity, AzureSearchDocument>(typeMap, propertyName);
            var property = typeof(AzureSearchDocument).GetProperty(mappedPropertyName);

            if (property is null)
            {
                throw new InvalidOperationException($"The property '{mappedPropertyName}' does not exist in the {nameof(AzureSearchDocument)} type.");
            }

            var jsonPropertyName = AttributeHelper.GetAttributeField<JsonPropertyNameAttribute, string>(property, nameof(JsonPropertyNameAttribute.Name));

            return jsonPropertyName;
        }

        public static string MapToPropertyName<TEntity>(TypeMap typeMap, string jsonPropertyName)
            where TEntity : SearchDocument
        {
            var propertyName = typeof(AzureSearchDocument).GetProperties()
                .Where(x => x.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name == jsonPropertyName)
                .Select(x => x.Name)
                .FirstOrDefault();

            if (propertyName is null)
            {
                throw new InvalidOperationException($"The property '{jsonPropertyName}' doesn't exist in the {nameof(AzureSearchDocument)} type.");
            }

            var mappedPropertyName = MapperHelper.MapToColumnName<AzureSearchDocument, TEntity>(typeMap, propertyName);

            return mappedPropertyName;
        }

        private static void AddRangeToICollection<TEntity>(ICollection<TEntity> destination, IEnumerable<TEntity> source)
        {
            foreach (var item in source)
            {
                destination.Add(item);
            }
        }
    }
}
