using System.Collections.Generic;

namespace BDO.GWT.Domain.Core.Retrievers;

public interface IQueryProperties
{
    int? OnSite { get; set; }

    OrderByList? OrderBy { get; set; }

    string[]? Select { get; set; }

    int? Skip { get; set; }

    int? Take { get; set; }

    List<IWhereCondition> Where { get; set; }
}

public class QueryProperties : IQueryProperties
{
    public int? OnSite { get; set; }

    public OrderByList? OrderBy { get; set; }

    public string[]? Select { get; set; }

    public int? Skip { get; set; }

    public int? Take { get; set; }

    public List<IWhereCondition> Where { get; set; } = new();
}
