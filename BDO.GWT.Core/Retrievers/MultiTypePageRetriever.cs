using CMS.ContentEngine;
using CMS.Helpers;
using CMS.Websites;
using CMS.Websites.Routing;
using Kentico.Content.Web.Mvc.Routing;

namespace BDO.GWT.Xperience.Core.Retrievers;

public interface IMultiTypePageRetriever
{
    Task<IEnumerable<TContentItem>> RetrievePagesAsync<TContentItem>(
        string contentTypeName,
        Action<ContentTypesQueryParameters>? decorateQuery = null,
        CancellationToken cancellationToken = default)
        where TContentItem : class, IWebPageFieldsSource, new();

    Task<IEnumerable<TAbstractItem>> RetrievePagesAsync<TAbstractItem>(
        IEnumerable<string> contentTypeNames,
        Func<IWebPageContentQueryDataContainer, TAbstractItem> mapper,
        Action<ContentTypesQueryParameters>? decorateQuery = null,
        Action<ContentQueryParameters>? decorateParameters = null,
        CancellationToken cancellationToken = default)
        where TAbstractItem : class;

    Task<TContentItem?> RetrievePageAsync<TContentItem>(
        string contentTypeName,
        Action<ContentTypesQueryParameters>? decorateQuery = null,
        CancellationToken cancellationToken = default)
        where TContentItem : class, IWebPageFieldsSource, new();

    Task<TAbstractItem?> RetrievePageAsync<TAbstractItem>(
        IEnumerable<string> contentTypeNames,
        Func<IWebPageContentQueryDataContainer, TAbstractItem> mapper,
        Action<ContentTypesQueryParameters>? decorateQuery = null,
        Action<ContentQueryParameters>? decorateParameters = null,
        CancellationToken cancellationToken = default)
        where TAbstractItem : class;

    Task<int> RetrievePageCountAsync(
        string contentTypeName,
        Action<ContentTypesQueryParameters>? decorateQuery = null,
        CancellationToken cancellationToken = default);

    Task<int> RetrievePageCountAsync(
        IEnumerable<string> contentTypeNames,
        Action<ContentTypesQueryParameters>? decorateQuery = null,
        Action<ContentQueryParameters>? decorateParameters = null,
        CancellationToken cancellationToken = default);
}

public class MultiTypePageRetriever : IMultiTypePageRetriever
{
    private readonly IContentQueryExecutor _contentQueryExecutor;
    private readonly IWebsiteChannelContext _websiteChannelContext;
    private readonly IProgressiveCache _progressiveCache;
    private readonly IPreferredLanguageRetriever _preferredLanguageRetriever;

    public MultiTypePageRetriever(
        IContentQueryExecutor contentQueryExecutor,
        IWebsiteChannelContext websiteChannelContext,
        IProgressiveCache progressiveCache,
        IPreferredLanguageRetriever preferredLanguageRetriever)
    {
        _contentQueryExecutor = contentQueryExecutor;
        _websiteChannelContext = websiteChannelContext;
        _progressiveCache = progressiveCache;
        _preferredLanguageRetriever = preferredLanguageRetriever;
    }

    public async Task<IEnumerable<TContentItem>> RetrievePagesAsync<TContentItem>(
        string contentTypeName,
        Action<ContentTypesQueryParameters>? decorateQuery = null,
        CancellationToken cancellationToken = default)
        where TContentItem : class, IWebPageFieldsSource, new()
    {
        var queryBuilder = BuildSingleTypeQuery(contentTypeName, decorateQuery);

        var executionOptions = new ContentQueryExecutionOptions
        {
            ForPreview = _websiteChannelContext.IsPreview,
        };

        var cacheSettings = new CacheSettings(
            cacheMinutes: 10,
            cacheItemNameParts: [nameof(MultiTypePageRetriever), contentTypeName, _websiteChannelContext.WebsiteChannelName]
        )
        {
            GetCacheDependency = () => CacheHelper.GetCacheDependency(
                $"webpageitem|bychannel|{_websiteChannelContext.WebsiteChannelName}|bycontenttype|{contentTypeName}"
            )
        };

        return await _progressiveCache.LoadAsync(
            async _ => (await _contentQueryExecutor.GetMappedWebPageResult<TContentItem>(
                queryBuilder,
                executionOptions,
                cancellationToken: cancellationToken
            )).ToArray(),
            cacheSettings
        );
    }

    public async Task<IEnumerable<TAbstractItem>> RetrievePagesAsync<TAbstractItem>(
        IEnumerable<string> contentTypeNames,
        Func<IWebPageContentQueryDataContainer, TAbstractItem> mapper,
        Action<ContentTypesQueryParameters>? decorateQuery = null,
        Action<ContentQueryParameters>? decorateParameters = null,
        CancellationToken cancellationToken = default)
        where TAbstractItem : class
    {
        var contentTypeNamesList = contentTypeNames.ToList();
        var queryBuilder = BuildMultiTypeQuery(contentTypeNamesList, decorateQuery, decorateParameters);

        var executionOptions = new ContentQueryExecutionOptions
        {
            ForPreview = _websiteChannelContext.IsPreview,
        };

        var cacheKey = string.Join("|", contentTypeNamesList);

        var cacheSettings = new CacheSettings(
            cacheMinutes: 10,
            cacheItemNameParts: [nameof(MultiTypePageRetriever), "multitype", cacheKey, _websiteChannelContext.WebsiteChannelName]
        )
        {
            GetCacheDependency = () =>
            {
                var dependencies = contentTypeNamesList
                    .Select(ct => $"webpageitem|bychannel|{_websiteChannelContext.WebsiteChannelName}|bycontenttype|{ct}")
                    .ToArray();

                return CacheHelper.GetCacheDependency(dependencies);
            }
        };

        return await _progressiveCache.LoadAsync(
            async _ =>
            {
                var results = await _contentQueryExecutor.GetWebPageResult(
                    queryBuilder,
                    mapper,
                    executionOptions,
                    cancellationToken: cancellationToken
                );

                return results.ToArray();
            },
            cacheSettings
        );
    }

    public async Task<TContentItem?> RetrievePageAsync<TContentItem>(
        string contentTypeName,
        Action<ContentTypesQueryParameters>? decorateQuery = null,
        CancellationToken cancellationToken = default)
        where TContentItem : class, IWebPageFieldsSource, new()
    {
        var queryBuilder = BuildSingleTypeQuery(contentTypeName, decorateQuery);
        queryBuilder.Parameters(q => q.TopN(1));

        var executionOptions = new ContentQueryExecutionOptions
        {
            ForPreview = _websiteChannelContext.IsPreview,
        };

        var cacheSettings = new CacheSettings(
            cacheMinutes: 10,
            cacheItemNameParts: [nameof(MultiTypePageRetriever), contentTypeName, "single", _websiteChannelContext.WebsiteChannelName]
        )
        {
            GetCacheDependency = () => CacheHelper.GetCacheDependency(
                $"webpageitem|bychannel|{_websiteChannelContext.WebsiteChannelName}|bycontenttype|{contentTypeName}"
            )
        };

        return await _progressiveCache.LoadAsync(
            async _ => (await _contentQueryExecutor.GetMappedWebPageResult<TContentItem>(
                queryBuilder,
                executionOptions,
                cancellationToken: cancellationToken
            )).FirstOrDefault(),
            cacheSettings
        );
    }

    public async Task<TAbstractItem?> RetrievePageAsync<TAbstractItem>(
        IEnumerable<string> contentTypeNames,
        Func<IWebPageContentQueryDataContainer, TAbstractItem> mapper,
        Action<ContentTypesQueryParameters>? decorateQuery = null,
        Action<ContentQueryParameters>? decorateParameters = null,
        CancellationToken cancellationToken = default)
        where TAbstractItem : class
    {
        var contentTypeNamesList = contentTypeNames.ToList();
        var queryBuilder = BuildMultiTypeQuery(contentTypeNamesList, decorateQuery, decorateParameters);
        queryBuilder.Parameters(q => q.TopN(1));

        var executionOptions = new ContentQueryExecutionOptions
        {
            ForPreview = _websiteChannelContext.IsPreview,
        };

        var cacheKey = string.Join("|", contentTypeNamesList);

        var cacheSettings = new CacheSettings(
            cacheMinutes: 10,
            cacheItemNameParts: [nameof(MultiTypePageRetriever), "multitype", cacheKey, "single", _websiteChannelContext.WebsiteChannelName]
        )
        {
            GetCacheDependency = () =>
            {
                var dependencies = contentTypeNamesList
                    .Select(ct => $"webpageitem|bychannel|{_websiteChannelContext.WebsiteChannelName}|bycontenttype|{ct}")
                    .ToArray();

                return CacheHelper.GetCacheDependency(dependencies);
            }
        };

        return await _progressiveCache.LoadAsync(
            async _ =>
            {
                var results = await _contentQueryExecutor.GetWebPageResult(
                    queryBuilder,
                    mapper,
                    executionOptions,
                    cancellationToken: cancellationToken
                );

                return results.FirstOrDefault();
            },
            cacheSettings
        );
    }

    public async Task<int> RetrievePageCountAsync(
        string contentTypeName,
        Action<ContentTypesQueryParameters>? decorateQuery = null,
        CancellationToken cancellationToken = default)
    {
        var queryBuilder = BuildSingleTypeQuery(contentTypeName, decorateQuery);

        var executionOptions = new ContentQueryExecutionOptions
        {
            ForPreview = _websiteChannelContext.IsPreview,
        };

        var cacheSettings = new CacheSettings(
            cacheMinutes: 10,
            cacheItemNameParts: [nameof(MultiTypePageRetriever), contentTypeName, "count", _websiteChannelContext.WebsiteChannelName]
        )
        {
            GetCacheDependency = () => CacheHelper.GetCacheDependency(
                $"webpageitem|bychannel|{_websiteChannelContext.WebsiteChannelName}|bycontenttype|{contentTypeName}"
            )
        };

        return await _progressiveCache.LoadAsync(
            async _ =>
            {
                var results = await _contentQueryExecutor.GetWebPageResult(
                    queryBuilder,
                    container => container.WebPageItemID,
                    executionOptions,
                    cancellationToken: cancellationToken
                );

                return results.Count();
            },
            cacheSettings
        );
    }

    public async Task<int> RetrievePageCountAsync(
        IEnumerable<string> contentTypeNames,
        Action<ContentTypesQueryParameters>? decorateQuery = null,
        Action<ContentQueryParameters>? decorateParameters = null,
        CancellationToken cancellationToken = default)
    {
        var contentTypeNamesList = contentTypeNames.ToList();
        var queryBuilder = BuildMultiTypeQuery(contentTypeNamesList, decorateQuery, decorateParameters);

        var executionOptions = new ContentQueryExecutionOptions
        {
            ForPreview = _websiteChannelContext.IsPreview,
        };

        var cacheKey = string.Join("|", contentTypeNamesList);

        var cacheSettings = new CacheSettings(
            cacheMinutes: 10,
            cacheItemNameParts: [nameof(MultiTypePageRetriever), "multitype", cacheKey, "count", _websiteChannelContext.WebsiteChannelName]
        )
        {
            GetCacheDependency = () =>
            {
                var dependencies = contentTypeNamesList
                    .Select(ct => $"webpageitem|bychannel|{_websiteChannelContext.WebsiteChannelName}|bycontenttype|{ct}")
                    .ToArray();

                return CacheHelper.GetCacheDependency(dependencies);
            }
        };

        return await _progressiveCache.LoadAsync(
            async _ =>
            {
                var results = await _contentQueryExecutor.GetWebPageResult(
                    queryBuilder,
                    container => container.WebPageItemID,
                    executionOptions,
                    cancellationToken: cancellationToken
                );

                return results.Count();
            },
            cacheSettings
        );
    }

    private ContentItemQueryBuilder BuildSingleTypeQuery(
        string contentTypeName,
        Action<ContentTypesQueryParameters>? decorateQuery)
    {
        return new ContentItemQueryBuilder()
            .ForContentTypes(q =>
            {
                q.OfContentType(contentTypeName);
                decorateQuery?.Invoke(q);
            })
            .InLanguage(_preferredLanguageRetriever.Get());
    }

    private ContentItemQueryBuilder BuildMultiTypeQuery(
        List<string> contentTypeNames,
        Action<ContentTypesQueryParameters>? decorateQuery,
        Action<ContentQueryParameters>? decorateParameters)
    {
        var builder = new ContentItemQueryBuilder()
            .ForContentTypes(q =>
            {
                q.OfContentType([.. contentTypeNames]);
                decorateQuery?.Invoke(q);
            })
            .InLanguage(_preferredLanguageRetriever.Get());

        if (decorateParameters is not null)
        {
            builder.Parameters(decorateParameters);
        }

        return builder;
    }
}