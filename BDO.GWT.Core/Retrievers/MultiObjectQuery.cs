using BDO.GWT.Domain.Core.Entities;
using BDO.GWT.Domain.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace BDO.GWT.Domain.Core.Retrievers;

public interface IMultiObjectQuery<TMainEntity> : ICacheKeyPartsBuilder, ICacheKeyBuilder where TMainEntity : IEntity
{
    IMultiObjectQueryProperties Properties { get; }

    /// <summary>
    /// Includes the specified entity type in the query results, allowing for eager loading of related data. The expression parameter is a lambda expression that specifies the navigation property to include, and the joinType parameter determines the type of join to use (inner or left). This method enables the retrieval of related entities in a single query, reducing the number of database round trips and improving performance.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="expression"></param>
    /// <param name="joinType"></param>
    /// <returns></returns>
    IMultiObjectQuery<TMainEntity> Include<TEntity>(Expression<Func<TMainEntity, TEntity?>> expression, JoinType joinType = JoinType.Inner) where TEntity : DomainEntity;

    /// <summary>
    /// Selects specific properties from the specified entity type to be included in the query results. The expression parameter is a lambda expression that specifies the properties to select. This method allows for fine-grained control over which properties are retrieved from the database, potentially improving performance by reducing the amount of data transferred.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="expression"></param>
    /// <returns></returns>
    IMultiObjectQuery<TMainEntity> Select<TEntity>(Expression<Func<TEntity, object?>> expression) where TEntity : DomainEntity;

    IMultiObjectQuery<TMainEntity> Where<TEntity>(Expression<Func<TEntity, bool>> expression) where TEntity : DomainEntity;
}

public class MultiObjectQuery<TMainEntity> : IMultiObjectQuery<TMainEntity> where TMainEntity : IEntity
{
    public IMultiObjectQueryProperties Properties { get; } = new MultiObjectQueryProperties();

    public IMultiObjectQuery<TMainEntity> Select<TEntity>(Expression<Func<TEntity, object?>> expression) where TEntity : DomainEntity
    {
        Properties.SelectList.Add((typeof(TEntity), QueryHelper.GetPropertyNames(expression)));
        return this;
    }

    public IMultiObjectQuery<TMainEntity> Where<TEntity>(Expression<Func<TEntity, bool>> expression) where TEntity : DomainEntity
    {
        Properties.WhereList.Add((typeof(TEntity), new WhereConditionParser(expression.Parameters[0]).ParseExpression(expression.Body)));
        return this;
    }

    public IMultiObjectQuery<TMainEntity> Include<TEntity>(Expression<Func<TMainEntity, TEntity?>> expression, JoinType joinType = JoinType.Inner) where TEntity : DomainEntity
    {
        var columnName = QueryExpressionHelper.GetPropertyName(expression.Body);
        Properties.IncludeProperties.Add((columnName, joinType));
        return this;
    }

    public List<string> BuildCacheKeyParts()
    {
        var cacheKeyParts = new List<string>();
        if (Properties.IncludeProperties.Count > 0)
        {
            cacheKeyParts.Add($"{nameof(Properties.IncludeProperties)} = [{string.Join(", ", Properties.IncludeProperties.Select(x => $"{x.Name}.{x.JoinType}"))}]");
        }

        if (Properties.SelectList.Count > 0)
        {
            cacheKeyParts.Add($"{nameof(Properties.SelectList)} = [{string.Join(", ", Properties.SelectList.Select(x => $"\"{x.EntityType.Name}.{string.Join(", ", x.ColumnNames)}\""))}]");
        }

        if (Properties.WhereList.Count > 0)
        {
            cacheKeyParts.Add($"{nameof(Properties.WhereList)} = [{string.Join(", ", Properties.WhereList.Select(x => $"{x.EntityType.Name}.{x.WhereCondition.BuildCacheKey()}"))}]");
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
