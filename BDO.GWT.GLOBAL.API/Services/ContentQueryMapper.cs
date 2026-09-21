using CMS.ContentEngine;

namespace BDO.GWT.GLOBAL.API.Services;

/// <summary>
/// Default <see cref="IContentQueryMapper"/> implementation delegating to <see cref="IContentQueryExecutor"/>.
/// </summary>
public class ContentQueryMapper : IContentQueryMapper
{
    private readonly IContentQueryExecutor _contentQueryExecutor;

    public ContentQueryMapper(IContentQueryExecutor contentQueryExecutor)
    {
        _contentQueryExecutor = contentQueryExecutor;
    }

    public async Task<IEnumerable<T>> GetMappedResultAsync<T>(ContentItemQueryBuilder query, CancellationToken cancellationToken)
        where T : class, IContentItemFieldsSource
    {
        return await _contentQueryExecutor.GetMappedResult<T>(query, cancellationToken: cancellationToken);
    }
}
