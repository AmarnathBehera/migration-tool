using CMS.ContentEngine;

namespace BDO.GWT.GLOBAL.API.Services;

/// <summary>
/// Thin abstraction over <see cref="IContentQueryExecutor.GetMappedResult{T}(IContentQueryBuilder, CMS.DataEngine.ContentQueryExecutionOptions?, CancellationToken)"/>
/// so consumers can be unit tested without relying on Kentico's automapper validation, which cannot be
/// satisfied by ordinary test doubles.
/// </summary>
public interface IContentQueryMapper
{
    /// <summary>
    /// Executes the given content item query and maps the results to <typeparamref name="T"/>.
    /// </summary>
    Task<IEnumerable<T>> GetMappedResultAsync<T>(ContentItemQueryBuilder query, CancellationToken cancellationToken)
        where T : class, IContentItemFieldsSource;
}
