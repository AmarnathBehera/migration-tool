using CMS.ContentEngine;

namespace BDO.GWT.GLOBAL.API.Services;

public class ContentQueryMapper : IContentQueryMapper
{
    private readonly IContentQueryExecutor _contentQueryExecutor;
    private readonly IContentQueryModelTypeMapper _contentQueryModelTypeMapper;

    public ContentQueryMapper(
        IContentQueryExecutor contentQueryExecutor,
        IContentQueryModelTypeMapper contentQueryModelTypeMapper)
    {
        ArgumentNullException.ThrowIfNull(contentQueryExecutor);
        ArgumentNullException.ThrowIfNull(contentQueryModelTypeMapper);
        _contentQueryExecutor = contentQueryExecutor;
        _contentQueryModelTypeMapper = contentQueryModelTypeMapper;
    }

    public async Task<IEnumerable<T>> GetMappedResultAsync<T>(ContentItemQueryBuilder query, CancellationToken cancellationToken)
        where T : class, IContentItemFieldsSource
    {
        // Use the mockable GetResult overload with an explicit mapper (equivalent to the
        // GetMappedResult extension, but testable) so the injected IContentQueryModelTypeMapper
        // performs the automatic content-type-model mapping.
        return await _contentQueryExecutor.GetResult<T>(
            query,
            container => _contentQueryModelTypeMapper.Map<T>(container),
            cancellationToken: cancellationToken);
    }
}
