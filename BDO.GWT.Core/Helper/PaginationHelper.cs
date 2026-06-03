using BDO.GWT.Core.Data.Queries;
using BDO.GWT.Domain.Core.Entities;
using System.Collections.Generic;
using System.Linq;

namespace BDO.GWT.Web.Core.Helpers;

public static class PaginationHelper
{
    public static IEnumerable<TPage> PaginatePages<TPage>(IEnumerable<TPage> pages, IPaginationQuery paginationQuery)
        where TPage : IPage
    {
        return Paginate(pages, paginationQuery);
    }

    public static IEnumerable<T> Paginate<T>(IEnumerable<T> items, IPaginationQuery paginationQuery)
    {
        if (!paginationQuery.CurrentPage.HasValue || !paginationQuery.PageSize.HasValue)
        {
            return items;
        }

        return items
            .Skip((paginationQuery.CurrentPage.Value - 1) * paginationQuery.PageSize.Value)
            .Take(paginationQuery.PageSize.Value);
    }
}
