using CMS.ContentEngine;

namespace BDO.GWT.GLOBAL.API.Services;

public interface IContentQueryMapper
{
    /// <summary>
    /// Executes the given content item query and maps the results to <typeparamref name="T"/>.
    /// </summary>
    Task<IEnumerable<T>> GetMappedResultAsync<T>(ContentItemQueryBuilder query, CancellationToken cancellationToken)
        where T : class, IContentItemFieldsSource;
}
