using BDO.GWT.Domain.Core.Entities;
using BDO.GWT.Domain.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace BDO.GWT.Domain.Core.Retrievers;

public interface IQuery<TEntity> : ICacheKeyPartsBuilder, ICacheKeyBuilder where TEntity : IEntity
{
    IQueryProperties Properties { get; }

    IQuery<TEntity> Global();

    IQuery<TEntity> OnSite(int siteID);

    IQuery<TEntity> OrderBy(Action<IOrderByList> list);

    IQuery<TEntity> Select(Expression<Func<TEntity, object?>> expression);

    IQuery<TEntity> Skip(int count);

    IQuery<TEntity> Take(int count);

    IQuery<TEntity> Where(Expression<Func<TEntity, bool>> expression);
}

public class Query<TEntity> : IQuery<TEntity> where TEntity : IEntity
{
    public IQueryProperties Properties { get; } = new QueryProperties();

    public IQuery<TEntity> Global()
    {
        Properties.OnSite = null;
        return this;
    }

    public IQuery<TEntity> OnSite(int siteID)
    {
        Properties.OnSite = siteID == 0 ? null : siteID;
        return this;
    }

    public IQuery<TEntity> OrderBy(Action<IOrderByList> list)
    {
        Properties.OrderBy = new OrderByList();
        list.Invoke(Properties.OrderBy);
        return this;

    }

    public IQuery<TEntity> Select(Expression<Func<TEntity, object?>> expression)
    {
        Properties.Select = QueryHelper.GetPropertyNames(expression);
        return this;
    }

    public IQuery<TEntity> Skip(int count)
    {
        Properties.Skip = count;
        return this;
    }

    public IQuery<TEntity> Take(int count)
    {
        Properties.Take = count;
        return this;
    }

    public IQuery<TEntity> Where(Expression<Func<TEntity, bool>> expression)
    {
        var whereCondition = new WhereConditionParser(expression.Parameters[0]).ParseExpression(expression.Body);

        Properties.Where.Add(whereCondition);
        return this;
    }

    public List<string> BuildCacheKeyParts()
    {
        var cacheKeyParts = new List<string>();

        if (Properties.OnSite != null)
        {
            cacheKeyParts.Add($"{nameof(Properties.OnSite)} = {Properties.OnSite}");
        }

        if (Properties.OrderBy != null)
        {
            cacheKeyParts.Add($"{nameof(Properties.OrderBy)} = [{string.Join(", ", Properties.OrderBy.List.Select(x => $"\"{x.PropertyName} {(x.Order == QueryOrder.Ascending ? "ASC" : "DESC")}\""))}]");
        }

        if (Properties.Select != null)
        {
            cacheKeyParts.Add($"{nameof(Properties.Select)} = [{string.Join(", ", Properties.Select.Select(x => $"\"{x}\""))}]");
        }

        if (Properties.Skip != null)
        {
            cacheKeyParts.Add($"{nameof(Properties.Skip)} = {Properties.Skip}");
        }

        if (Properties.Take != null)
        {
            cacheKeyParts.Add($"{nameof(Properties.Take)} = {Properties.Take}");
        }

        if (Properties.Where.Count > 0)
        {
            cacheKeyParts.Add($"{nameof(Properties.Where)} = ({string.Join(" AND ", Properties.Where.Select(x => x.BuildCacheKey()))})");
        }

        return cacheKeyParts;
    }

    public string BuildCacheKey()
    {
        var cacheKeyParts = BuildCacheKeyParts();

        var cacheKey = $"{{ {string.Join(", ", cacheKeyParts)} }}";

        return cacheKey;
    }
}
