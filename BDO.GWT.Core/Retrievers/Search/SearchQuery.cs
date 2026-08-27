using BDO.GWT.Core.Entities.Search;
using BDO.GWT.Core.Helpers;
using BDO.GWT.Domain.Core.Helpers;
using BDO.GWT.Domain.Core.Retrievers;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;


namespace BDO.GWT.Core.Retrievers.Search
{
    public interface ISearchQuery<T> where T : SearchDocument
    {
        AndWhereCondition WhereConditions { get; }

        string[]? SelectPropertyNames { get; }

        IList<(string PropertyName, int FacetCount, IEnumerable<object>? Values)> Facets { get; }

        int? SkipCount { get; }

        int? TakeCount { get; }

        bool? IsTotalCountIncluded { get; }

        OrderByList? OrderByList { get; }

        string[]? SearchPropertyNames { get; }

        ISearchQuery<T> Where(Expression<Func<T, bool>> predicate);

        ISearchQuery<T> Select(Expression<Func<T, object?>> selector);

        ISearchQuery<T> Skip(int count);

        ISearchQuery<T> Take(int count);

        ISearchQuery<T> IncludeFacet<TProperty>(Expression<Func<T, TProperty?>> selector, int facetCount = 10, IEnumerable<TProperty>? values = null)
            where TProperty : class;

        ISearchQuery<T> IncludeTotalCount();

        ISearchQuery<T> OrderBy(Action<IOrderByList> list);

        ISearchQuery<T> SearchBy(Expression<Func<T, object?>> selector);
    }

    public class SearchQuery<T> : ISearchQuery<T> where T : SearchDocument
    {
        public AndWhereCondition WhereConditions { get; private set; } = new();

        public string[]? SelectPropertyNames { get; private set; }

        public IList<(string PropertyName, int FacetCount, IEnumerable<object>? Values)> Facets { get; private set; } = new List<(string PropertyName, int FacetCount, IEnumerable<object>? Values)>();

        public string[]? SearchPropertyNames { get; private set; }

        public int? SkipCount { get; private set; }

        public int? TakeCount { get; private set; }

        public OrderByList? OrderByList { get; private set; }

        public bool? IsTotalCountIncluded { get; private set; }

        public ISearchQuery<T> Where(Expression<Func<T, bool>> predicate)
        {
            WhereConditions.Add(new WhereConditionParser(predicate.Parameters[0]).ParseExpression(predicate.Body));
            return this;
        }

        public ISearchQuery<T> Select(Expression<Func<T, object?>> selector)
        {
            SelectPropertyNames = QueryHelper.GetPropertyNames(selector);
            return this;
        }

        public ISearchQuery<T> Skip(int count)
        {
            SkipCount = count;
            return this;
        }

        public ISearchQuery<T> Take(int count)
        {
            TakeCount = count;
            return this;
        }

        public ISearchQuery<T> IncludeFacet<TProperty>(Expression<Func<T, TProperty?>> selector, int facetCount = 10, IEnumerable<TProperty>? values = null)
            where TProperty : class
        {
            Facets.Add((QueryHelper.GetPropertyNames(selector)[0], facetCount, values));
            return this;
        }

        public ISearchQuery<T> IncludeTotalCount()
        {
            IsTotalCountIncluded = true;
            return this;
        }

        public ISearchQuery<T> SearchBy(Expression<Func<T, object?>> selector)
        {
            SearchPropertyNames = QueryHelper.GetPropertyNames(selector);
            return this;
        }

        public ISearchQuery<T> OrderBy(Action<IOrderByList> list)
        {
            OrderByList = new OrderByList();
            list.Invoke(OrderByList);
            return this;
        }
    }
}
