using System;
using System.Collections.Generic;

namespace BDO.GWT.Domain.Core.Retrievers;

public interface IMultiObjectQueryProperties
{
    ICollection<(string Name, JoinType JoinType)> IncludeProperties { get; set; }

    ICollection<(Type EntityType, string[] ColumnNames)> SelectList { get; set; }

    ICollection<(Type EntityType, IWhereCondition WhereCondition)> WhereList { get; set; }
}

public class MultiObjectQueryProperties : IMultiObjectQueryProperties
{
    public ICollection<(string Name, JoinType JoinType)> IncludeProperties { get; set; } = new List<(string PropertyName, JoinType JoinType)>();

    public ICollection<(Type EntityType, string[] ColumnNames)> SelectList { get; set; } = new List<(Type, string[])>();

    public ICollection<(Type EntityType, IWhereCondition WhereCondition)> WhereList { get; set; } = new List<(Type, IWhereCondition)>();
}
