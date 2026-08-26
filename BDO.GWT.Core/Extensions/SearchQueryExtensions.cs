using Azure.Search.Documents.Models;
using BDO.GWT.Core.Retrievers.Search;
using System;
using System.Collections.Generic;
using System.Text;
using BDO.GWT.Core.Helpers;
using BDO.GWT.Core.Entities.Search;

namespace BDO.GWT.Core.Extensions
{
    public static class SearchQueryExtensions
    {
        public static void OnlyPublishedPages(this ISearchQuery<BDO.GWT.Core.Entities.Search.SearchDocument> query, IDateTimeProviderSearch dateTimeProvider)
        {
            query.Where(x => x.PublishTo == null || x.PublishTo >= dateTimeProvider.Now);
            query.Where(x => x.PublishFrom == null || x.PublishFrom <= dateTimeProvider.Now);
        }
    }
}
