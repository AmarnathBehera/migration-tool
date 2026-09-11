namespace BDO.GWT.Domain.Core.Retrievers;

/// <summary>
/// Defines the operators that can be used in query conditions for filtering data. These operators allow for various types of comparisons, such as equality, inequality, and pattern matching, enabling flexible and precise data retrieval based on specified criteria.
/// </summary>
public enum QueryOperator
{
    /// <summary>
    /// Represents an "EQUAL" operator for filtering queries. This operator is used to retrieve items that match a specified value, allowing for precise comparisons in query conditions.
    /// </summary>
    Equal,
    GreaterThan,
    GreaterThanOrEqual,
    LessThan,
    LessThanOrEqual,
    /// <summary>
    /// Represents a "LIKE" operator for pattern matching in queries. This operator is typically used to search for a specified pattern in a column, allowing for wildcard characters. It is commonly used in SQL queries to filter results based on partial matches.
    /// </summary>
    Like,
    /// <summary>
    /// Represents a "NOT EQUAL" operator for filtering queries. This operator is used to exclude items that match a specified value, allowing for the retrieval of items that do not meet the given condition.
    /// </summary>
    NotEqual,
}
