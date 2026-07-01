namespace BDO.GWT.Domain.Core.Retrievers;

public enum JoinType
{
    Inner = 1,
    LeftOuter = 2,
    /// <summary>
    /// Represents a right outer join, which returns all records from the right table and the matched records from the left table. If there is no match, NULL values are returned for columns from the left table.
    /// </summary>
    RightOuter = 3,
}