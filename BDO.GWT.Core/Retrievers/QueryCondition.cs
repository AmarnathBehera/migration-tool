using System;
using System.Collections.Generic;
using System.Linq;

namespace BDO.GWT.Domain.Core.Retrievers;

public interface IWhereCondition : ICacheKeyBuilder { }

public class AndWhereCondition : List<IWhereCondition>, IWhereCondition
{
    public string BuildCacheKey()
    {
        return $"{string.Join(" AND ", this.Select(x => $"({x.BuildCacheKey()})"))}";
    }
}

public abstract class ContainsAnyWhereCondition : IWhereCondition
{
    public string PropertyName { get; set; }

    public Type Type { get; set; }

    protected ContainsAnyWhereCondition(string propertyName, Type type)
    {
        PropertyName = propertyName;
        Type = type;
    }

    public abstract string BuildCacheKey();
}

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

    public abstract string BuildCacheKey();
}

public class InWhereCondition<TItem> : InWhereCondition
{
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
