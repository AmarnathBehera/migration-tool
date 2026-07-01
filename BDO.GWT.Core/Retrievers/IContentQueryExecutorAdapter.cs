using CMS.ContentEngine;
using CMS.Websites;

namespace BDO.GWT.Xperience.Core.Retrievers;

public interface IContentQueryExecutorAdapter
{
    Task<IEnumerable<TContentItem>> GetMappedWebPageResultAsync<TContentItem>(
        ContentItemQueryBuilder queryBuilder,
        ContentQueryExecutionOptions executionOptions,
        CancellationToken cancellationToken = default)
        where TContentItem : class, IWebPageFieldsSource, new();

    Task<IEnumerable<TResult>> GetWebPageResultAsync<TResult>(
        ContentItemQueryBuilder queryBuilder,
        Func<IWebPageContentQueryDataContainer, TResult> mapper,
        ContentQueryExecutionOptions executionOptions,
        CancellationToken cancellationToken = default);
}

public class ContentQueryExecutorAdapter : IContentQueryExecutorAdapter
{
    private readonly IContentQueryExecutor _executor;

    public ContentQueryExecutorAdapter(IContentQueryExecutor executor)
    {
        _executor = executor;
    }

    public Task<IEnumerable<TContentItem>> GetMappedWebPageResultAsync<TContentItem>(
        ContentItemQueryBuilder queryBuilder,
        ContentQueryExecutionOptions executionOptions,
        CancellationToken cancellationToken = default)
        where TContentItem : class, IWebPageFieldsSource, new()
        => _executor.GetMappedWebPageResult<TContentItem>(queryBuilder, executionOptions, cancellationToken);

    public Task<IEnumerable<TResult>> GetWebPageResultAsync<TResult>(
        ContentItemQueryBuilder queryBuilder,
        Func<IWebPageContentQueryDataContainer, TResult> mapper,
        ContentQueryExecutionOptions executionOptions,
        CancellationToken cancellationToken = default)
        => _executor.GetWebPageResult(queryBuilder, mapper, executionOptions, cancellationToken);
}
