namespace BDO.GWT.Core.Data.Queries;

public interface IPaginationQuery
{
    public int? CurrentPage { get; set; }

    public int? PageSize { get; set; }
}
