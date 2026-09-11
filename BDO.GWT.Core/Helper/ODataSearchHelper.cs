using AutoMapper;
using BDO.GWT.Common.Core.Extensions;
using BDO.GWT.Core.Helpers;
using BDO.GWT.Core.ValueObjects;
using BDO.GWT.Domain.Core.Retrievers;
using BDO.GWT.Core.Entities.Search;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace BDO.GWT.Core.Helpers
{
    public class ODataSearchHelper<TEntity>
    where TEntity : SearchDocument
    {
        private readonly TypeMap _typeMap;

        public ODataSearchHelper(TypeMap typeMap)
        {
            _typeMap = typeMap;
        }

        public string ConvertToODataFilterString(IWhereCondition whereCondition)
        {
            var odataFilter = ToOdataFilterCondition(whereCondition);
            return odataFilter;
        }

        public IEnumerable<string> ConvertToODataOrderStringList(OrderByList orderList)
        {
            var odataOrderList = new List<string>();

            foreach (var (propertyName, order, _) in orderList.List)
            {
                var jsonPropertyName = SearchQueryHelper.MapToJsonPropertyName<TEntity>(_typeMap, propertyName);

                var orderAsString = order switch
                {
                    QueryOrder.Ascending => "asc",
                    QueryOrder.Descending => "desc",
                    _ => throw new NotImplementedException($"QueryOrder of type '{order.GetType().Name}' is not implemented."),
                };

                odataOrderList.Add($"{jsonPropertyName} {orderAsString}");
            }

            return odataOrderList;
        }

        public IEnumerable<string> ConvertToODataSearchFacetList(IEnumerable<(string PropertyName, int FacetCount, IEnumerable<object>? Values)> facets)
        {
            var odataFacetList = new List<string>();

            foreach (var (propertyName, facetCount, values) in facets)
            {
                var jsonPropertyName = SearchQueryHelper.MapToJsonPropertyName<TEntity>(_typeMap, propertyName);

                if (values is null || !values.Any())
                {
                    odataFacetList.Add($"{jsonPropertyName},count:{facetCount}");
                    continue;
                }

                var valuesAsString = string.Join("|", values.Select(x => GetODataConstant(x, false)));

                odataFacetList.Add($"{jsonPropertyName},count:{facetCount},values:{valuesAsString}");
            }

            return odataFacetList;
        }

        private string ToOdataFilterCondition(IWhereCondition whereCondition)
        {
            switch (whereCondition)
            {
                case AndWhereCondition andWhereCondition:
                    {
                        var whereConditions = andWhereCondition.Select(ToOdataFilterCondition).ToArray();

                        var odataFilterCondition = string.Join(" and ", whereConditions);

                        return $"({odataFilterCondition})";
                    }

                case InWhereCondition inWhereCondition:
                    {
                        var columnName = SearchQueryHelper.MapToJsonPropertyName<TEntity>(_typeMap, inWhereCondition.PropertyName);

                        switch (inWhereCondition)
                        {
                            case InWhereCondition<bool>:
                            case InWhereCondition<string>:
                            case InWhereCondition<DateTime>:
                            case InWhereCondition<double>:
                            case InWhereCondition<float>:
                                {
                                    return ConvertTypedInWhereCondition(columnName, inWhereCondition);
                                }

                            default:
                                {
                                    throw new NotImplementedException($"InWhereCondition of type '{inWhereCondition.GetType().Name}' is not implemented.");
                                }
                        }
                    }

                case NotWhereCondition notWhereCondition:
                    {
                        var columnName = SearchQueryHelper.MapToJsonPropertyName<TEntity>(_typeMap, notWhereCondition.PropertyName);

                        var kenticoWhereCondition = $"not {columnName}";

                        return kenticoWhereCondition;
                    }

                case OrWhereCondition orWhereCondition:
                    {
                        var whereConditions = orWhereCondition.Select(ToOdataFilterCondition).ToArray();

                        var odataFilterCondition = string.Join(" or ", whereConditions);

                        return $"({odataFilterCondition})";
                    }

                case RelationalWhereCondition relationalWhereCondition:
                    {
                        var jsonPropertyName = SearchQueryHelper.MapToJsonPropertyName<TEntity>(_typeMap, relationalWhereCondition.PropertyName);

                        // Azure AI Search OData $filter has no LIKE/wildcard operator.
                        // Translate SQL-style LIKE patterns into a full-text search.ismatch() call.
                        if (relationalWhereCondition.Op == QueryOperator.Like)
                        {
                            return BuildIsMatchFilter(jsonPropertyName, relationalWhereCondition.Value?.ToString());
                        }

                        var op = ToODataLogicalOperator(relationalWhereCondition.Op);
                        var value = GetODataConstant(relationalWhereCondition.Value);

                        var odataFilterCondition = $"{jsonPropertyName} {op} {value}";

                        return odataFilterCondition;
                    }
            }

            throw new NotImplementedException($"Mapping between BDO '{whereCondition.GetType().FullName}' and OData filter is not implemented.");
        }

        private static string ConvertTypedInWhereCondition(string columnName, InWhereCondition inWhereCondition)
        {
            if (!(inWhereCondition.GetType().IsGenericType && inWhereCondition.GetType().GetGenericTypeDefinition() == typeof(InWhereCondition<>)))
            {
                throw new InvalidOperationException($"The provided InWhereCondition '{inWhereCondition.GetType().Name}' is not a generic type.");
            }

            var values = (IEnumerable<object>)inWhereCondition.GetPropertyValue(nameof(InWhereCondition<object>.Values))!;

            var odataValues = values.Select(x => GetODataConstant(x, false));

            var odataInOperation = $"search.in({columnName}, '{string.Join("|", odataValues)}', '|')";

            return odataInOperation;
        }

        private static string ToODataLogicalOperator(QueryOperator op)
        {
            return op switch
            {
                QueryOperator.NotEqual => "ne",
                QueryOperator.Equal => "eq",
                QueryOperator.GreaterThan => "gt",
                QueryOperator.GreaterThanOrEqual => "ge",
                QueryOperator.LessThan => "lt",
                QueryOperator.LessThanOrEqual => "le",
                _ => throw new NotImplementedException($"Mapping between {nameof(QueryOperator)}.{op} and OData logical operator is not implemented."),
            };
        }

        private static string GetODataConstant(object? value, bool encloseInQuotes = true)
        {
            var odataConstant = value switch
            {
                null => "null",
                int intValue => intValue.ToString(),
                long longValue => longValue.ToString(),
                bool boolValue => boolValue ? "true" : "false",
                string stringValue => encloseInQuotes ? $"'{stringValue}'" : stringValue,
                DateTime dateValue => dateValue.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture),
                DateTimeOffset dateTimeOffsetValue => dateTimeOffsetValue.UtcDateTime.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture),
                double doubleValue => doubleValue.ToString(CultureInfo.InvariantCulture),
                float floatValue => floatValue.ToString(CultureInfo.InvariantCulture),
                LanguageTag languageTagValue => GetODataConstant(languageTagValue.ToString().ToLowerInvariant(), encloseInQuotes),
                _ => throw new NotSupportedException($"The data type '{value.GetType()}' is not supported for OData constant conversion."),
            };

            return odataConstant;
        }

        private static string BuildIsMatchFilter(string fieldName, string? likePattern)
        {
            // Convert a SQL LIKE pattern ('en-%', '%foo', '%foo%') to a Lucene wildcard term.
            var term = (likePattern ?? string.Empty).Replace("%", "*");

            // Escape Lucene special characters that are not our wildcard.
            term = term
                .Replace("-", "\\-")
                .Replace(":", "\\:");

            // Single-quotes inside an OData string literal must be doubled.
            term = term.Replace("'", "''");

            // 'full' => Lucene query syntax (enables wildcards); 'any' => match any term.
            return $"search.ismatch('{term}', '{fieldName}', 'full', 'any')";
        }
    }
}
