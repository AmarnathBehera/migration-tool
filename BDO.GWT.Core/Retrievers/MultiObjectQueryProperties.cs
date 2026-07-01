using System;
using System.Collections.Generic;

namespace BDO.GWT.Domain.Core.Retrievers;

public interface IMultiObjectQueryProperties
{
    ICollection<(string Name, JoinType JoinType)> IncludeProperties { get; set; }

    /// <summary>
    /// Gets or sets a collection of tuples representing the entity types and their corresponding column names to be selected in the query. Each tuple contains the entity type and an array of column names to be included in the query results.
    /// </summary>
    ICollection<(Type EntityType, string[] ColumnNames)> SelectList { get; set; }

    ICollection<(Type EntityType, IWhereCondition WhereCondition)> WhereList { get; set; }
}

public class MultiObjectQueryProperties : IMultiObjectQueryProperties
{
    public ICollection<(string Name, JoinType JoinType)> IncludeProperties { get; set; } = new List<(string PropertyName, JoinType JoinType)>();

    public ICollection<(Type EntityType, string[] ColumnNames)> SelectList { get; set; } = new List<(Type, string[])>();

    public ICollection<(Type EntityType, IWhereCondition WhereCondition)> WhereList { get; set; } = new List<(Type, IWhereCondition)>();
}
