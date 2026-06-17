using System.Collections.Generic;

namespace BDO.GWT.Core.Data.Queries;

public class CardsQuery : IPaginationQuery
{
    public IDictionary<string, string> Filters { get; set; } = new Dictionary<string, string>();

    public Guid? CurrentPageGuid { get; set; }

    public string? PageType { get; set; }

    public int? CurrentPage { get; set; }

    public int? PageSize { get; set; }

    public string? Order { get; set; }

}