using BDO.GWT.Core.Entities.Search;
using BDO.GWT.Core.Helpers;
using BDO.GWT.Domain.Core.Helpers;
using BDO.GWT.Domain.Core.Retrievers;
using System;
using System.Linq.Expressions;

namespace BDO.GWT.Core.Retrievers.Search
{
    public interface ISuggestSearchQuery<T> where T : SearchDocument
    {
        AndWhereCondition WhereConditions { get; }

        string[]? SelectPropertyNames { get; }

        int? TakeCount { get; }

        OrderByList? OrderByList { get; }

        ISuggestSearchQuery<T> Where(Expression<Func<T, bool>> predicate);

        ISuggestSearchQuery<T> Select(Expression<Func<T, object?>> selector);

        ISuggestSearchQuery<T> Take(int count);

        ISuggestSearchQuery<T> OrderBy(Action<IOrderByList> list);
    }

    public class SuggestSearchQuery<T> : ISuggestSearchQuery<T> where T : SearchDocument
    {
        public AndWhereCondition WhereConditions { get; } = new();

        public string[]? SelectPropertyNames { get; private set; }

        public OrderByList? OrderByList { get; private set; }

        public int? TakeCount { get; private set; }

        public ISuggestSearchQuery<T> Where(Expression<Func<T, bool>> predicate)
        {
            WhereConditions.Add(new WhereConditionParser(predicate.Parameters[0]).ParseExpression(predicate.Body));
            return this;
        }

        public ISuggestSearchQuery<T> Select(Expression<Func<T, object?>> selector)
        {
            SelectPropertyNames = QueryHelper.GetPropertyNames(selector);
            return this;
        }

        public ISuggestSearchQuery<T> Take(int count)
        {
            TakeCount = count;
            return this;
        }

        public ISuggestSearchQuery<T> OrderBy(Action<IOrderByList> list)
        {
            OrderByList = new OrderByList();
            list.Invoke(OrderByList);
            return this;
        }
    }
}
