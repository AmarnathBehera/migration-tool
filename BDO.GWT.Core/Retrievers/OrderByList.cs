using System.Collections.Generic;

namespace BDO.GWT.Domain.Core.Retrievers;

/// <summary>
/// Defines a contract for building a list of properties to order by in a query, allowing for the specification of sort order (ascending or descending) and optional collation settings for string comparisons. Implementing classes should provide methods to add properties to the order by list with the desired sorting direction and collation options.
/// </summary>
public interface IOrderByList
{
    IOrderByList Ascending(string propertyName);
    IOrderByList Ascending(string propertyName, string? collation);
    IOrderByList Descending(string propertyName);
    IOrderByList Descending(string propertyName, string? collation);
}

/// <summary>
/// Represents a list of properties to order by in a query, along with their corresponding sort order and optional collation settings. This class provides methods to add properties to the order by list in either ascending or descending order, with the ability to specify collation for string comparisons.
/// </summary>
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
