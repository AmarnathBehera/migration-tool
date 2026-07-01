using System.Collections.Generic;

namespace BDO.GWT.Domain.Core.Retrievers;

/// <summary>
/// Represents the properties used for querying data from a data source. This interface defines the structure for specifying query parameters such as filtering, ordering, selection, and pagination. Implementing classes can use these properties to construct queries that retrieve specific subsets of data based on the defined criteria.
/// </summary>
public interface IQueryProperties
{
    /// <summary>
    /// Gets or sets the number of items to retrieve from the data source that are located on-site. This property allows filtering the query results based on the location of the items, specifically those that are available on-site. If this property is null, no filtering based on location will be applied, and items from all locations will be included in the results.
    /// </summary>
    int? OnSite { get; set; }

    OrderByList? OrderBy { get; set; }

    string[]? Select { get; set; }

    int? Skip { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of items to return in the query results. This property allows limiting the number of items retrieved from the data source, which can be useful for pagination or controlling the size of the result set. If this property is null, no limit will be applied, and all matching items will be returned.
    /// </summary>
    int? Take { get; set; }

    /// <summary>
    /// Gets or sets the list of where conditions for the query. This property allows specifying filtering criteria for the query results based on one or more conditions. Each condition in the list defines a specific filter that must be satisfied for an item to be included in the results. If this property is null, no filtering will be applied to the query results.
    /// </summary>
    List<IWhereCondition> Where { get; set; }
}

public class QueryProperties : IQueryProperties
{
    public int? OnSite { get; set; }

    /// <summary>
    /// Gets or sets the list of order by conditions for the query. This property allows specifying how the results should be sorted based on one or more fields. Each condition in the list defines a field and the direction (ascending or descending) for sorting. If this property is null, no specific ordering will be applied to the query results.
    /// </summary>
    public OrderByList? OrderBy { get; set; }

    public string[]? Select { get; set; }

    public int? Skip { get; set; }

    public int? Take { get; set; }

    public List<IWhereCondition> Where { get; set; } = new();
}
