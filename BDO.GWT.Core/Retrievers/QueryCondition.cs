using System;
using System.Collections.Generic;
using System.Linq;

namespace BDO.GWT.Domain.Core.Retrievers;

public interface IWhereCondition : ICacheKeyBuilder { }

/// <summary>
/// Represents a condition that combines multiple where conditions using a logical AND operator. This class implements the IWhereCondition interface and allows for the construction of complex query conditions by combining multiple individual conditions. The BuildCacheKey method generates a cache key string that represents the combined condition for caching purposes.
/// </summary>
public class AndWhereCondition : List<IWhereCondition>, IWhereCondition
{
    public string BuildCacheKey()
    {
        return $"{string.Join(" AND ", this.Select(x => $"({x.BuildCacheKey()})"))}";
    }
}

public abstract class ContainsAnyWhereCondition : IWhereCondition
{
    /// <summary>
    /// string propertyname
    /// </summary>
    public string PropertyName { get; set; }

    public Type Type { get; set; }

    protected ContainsAnyWhereCondition(string propertyName, Type type)
    {
        PropertyName = propertyName;
        Type = type;
    }

    /// <summary>
    /// Builds a cache key string that represents the condition for checking if any of the specified values are contained within a property. The implementation of this method should return a string that can be used as a unique identifier for caching purposes, based on the property name and its values.
    /// </summary>
    /// <returns></returns>
    public abstract string BuildCacheKey();
}

/// <summary>
/// Represents a condition that checks if any of the specified values are contained within a property. This class is generic and can be used with any type of item. It builds a cache key string that represents the condition for caching purposes.
/// </summary>
/// <typeparam name="TItem"></typeparam>
public class ContainsAnyWhereCondition<TItem> : ContainsAnyWhereCondition
{
    public ICollection<TItem> Values { get; set; }

    public ContainsAnyWhereCondition(string propertyName, Type type, ICollection<TItem> values) : base(propertyName, type)
    {
        Values = values;
    }

    public override string BuildCacheKey()
    {
        return $"EXISTS (SELECT * FROM STRING_SPLIT({PropertyName}, '|') WHERE value IN ({string.Join(", ", Values)}))";
    }
}

public abstract class InWhereCondition : IWhereCondition
{
    public string PropertyName { get; set; }

    public Type Type { get; set; }

    public bool PropertyIsArray { get; set; }

    protected InWhereCondition(string propertyName, bool propertyIsArray, Type type)
    {
        PropertyName = propertyName;
        PropertyIsArray = propertyIsArray;
        Type = type;
    }

    /// <summary>
    /// Builds a cache key string that represents the IN condition for the specified property. The implementation of this method should return a string that can be used as a unique identifier for caching purposes, based on the property name and its values.
    /// </summary>
    /// <returns></returns>
    public abstract string BuildCacheKey();
}

public class InWhereCondition<TItem> : InWhereCondition
{
    /// <summary>
    /// Gets or sets the collection of values to be used in the IN condition.
    /// </summary>
    public ICollection<TItem> Values { get; set; }

    public InWhereCondition(string propertyName, bool propertyIsArray, ICollection<TItem> values) : base(propertyName, propertyIsArray, typeof(TItem))
    {
        Values = values;
    }

    public override string ToString()
    {
        return BuildCacheKey();
    }

    public override string BuildCacheKey()
    {
        return PropertyIsArray
            ? $"'{Values.First()}' IN (SELECT value FROM STRING_SPLIT({PropertyName}, '|'))"
            : $"{PropertyName} IN ({string.Join(", ", Values.Select(x => $"'{x}'"))})";
    }
}

public class NotWhereCondition : IWhereCondition
{
    /// <summary>
    /// Gets or sets the name of the property to which the NOT condition applies.
    /// </summary>
    public string PropertyName { get; set; }

    public NotWhereCondition(string propertyName)
    {
        PropertyName = propertyName;
    }

    public override string ToString()
    {
        return BuildCacheKey();
    }

    public string BuildCacheKey()
    {
        return $"NOT {PropertyName}";
    }
}

public class OrWhereCondition : List<IWhereCondition>, IWhereCondition
{
    public string BuildCacheKey()
    {
        return $"{string.Join(" OR ", this.Select(x => $"({x.BuildCacheKey()})"))}";
    }
}

public class RelationalWhereCondition : IWhereCondition
{
    public string PropertyName { get; }

    public QueryOperator Op { get; }

    public object? Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RelationalWhereCondition"/> class with the specified property name, operator, and value.
    /// </summary>
    /// <param name="propertyName"></param>
    /// <param name="op"></param>
    /// <param name="value"></param>
    public RelationalWhereCondition(string propertyName, QueryOperator op, object? value)
    {
        PropertyName = propertyName;
        Op = op;
        Value = value;
    }

    public override string ToString()
    {
        return BuildCacheKey();
    }

    public string BuildCacheKey()
    {
        return $"{PropertyName} {Op} '{Value}'";
    }
}
