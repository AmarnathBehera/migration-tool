using System.Collections.Generic;

namespace BDO.GWT.Domain.Core.Retrievers;

public interface IOrderByList
{
    IOrderByList Ascending(string propertyName);
    IOrderByList Ascending(string propertyName, string? collation);
    IOrderByList Descending(string propertyName);
    IOrderByList Descending(string propertyName, string? collation);
}

public class OrderByList : IOrderByList
{
    public IList<(string PropertyName, QueryOrder Order, string? Collation)> List { get; } = new List<(string PropertyName, QueryOrder Order, string? Collation)>();

    public IOrderByList Ascending(string propertyName)
    {
        List.Add((propertyName, QueryOrder.Ascending, null));
        return this;
    }

    public IOrderByList Ascending(string propertyName, string? collation)
    {
        List.Add((propertyName, QueryOrder.Ascending, collation));
        return this;
    }

    public IOrderByList Descending(string propertyName)
    {
        List.Add((propertyName, QueryOrder.Descending, null));
        return this;
    }

    public IOrderByList Descending(string propertyName, string? collation)
    {
        List.Add((propertyName, QueryOrder.Descending, collation));
        return this;
    }
}
