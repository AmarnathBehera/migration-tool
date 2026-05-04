namespace BDO.GWT.Core
{
    using CMS.ContentEngine;
    using CMS.Helpers;
    using CMS.Websites;
    using CMS.Websites.Routing;
    using Kentico.Content.Web.Mvc;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// XbyK implementation of <see cref="IContentService"/>.
    /// Wraps <see cref="IContentQueryExecutor"/> and <see cref="IWebsiteChannelContext"/>
    /// to provide a clean, reusable querying abstraction.
    /// </summary>
    public class ContentService : IContentService
    {
        private readonly IContentQueryExecutor _executor;
        private readonly IWebsiteChannelContext _websiteChannelContext;
        private readonly IProgressiveCache _cache;
        private readonly IContentRetriever _contentRetriever;
        private readonly IContentQueryExecutor _contentQueryExecutor;
        private readonly IContentQueryModelTypeMapper _contentQueryModelTypeMapper;

        // Default cache duration – override via constructor / options if needed.
        private static readonly TimeSpan DefaultCacheDuration = TimeSpan.FromMinutes(10);

        public ContentService(
            IContentQueryExecutor executor,
            IWebsiteChannelContext websiteChannelContext,
            IProgressiveCache cache,
            IContentRetriever contentRetriever,
            IContentQueryExecutor contentQueryExecutor,
            IContentQueryModelTypeMapper contentQueryModelTypeMapper
            )
        {
            _executor = executor ?? throw new ArgumentNullException(nameof(executor));
            _websiteChannelContext = websiteChannelContext ?? throw new ArgumentNullException(nameof(websiteChannelContext));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _contentRetriever = contentRetriever ?? throw new ArgumentNullException(nameof(contentRetriever));
            _contentQueryExecutor = contentQueryExecutor ?? throw new ArgumentNullException(nameof(contentQueryExecutor));
            _contentQueryModelTypeMapper = contentQueryModelTypeMapper ?? throw new ArgumentNullException(nameof(contentQueryModelTypeMapper));
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<T>> GetItemsAsync<T>(
            Action<ContentItemQueryBuilder>? configure = null,
            CancellationToken cancellationToken = default)
            where T : class
        {
            var builder = BuildQuery<T>(configure);

            var queryOptions = BuildQueryExecutionOptions();

            var results = await _executor.GetMappedResult<T>(builder, queryOptions, cancellationToken);

            return results.ToList().AsReadOnly();
        }
        public async Task<IReadOnlyList<T>> GetItemsChannelAsync<T>(
     Action<ContentItemQueryBuilder>? configure = null,
     CancellationToken cancellationToken = default)
     where T : class
        {
            var builder = new ContentItemQueryBuilder();

            builder.ForContentType(GetContentTypeName<T>(), subquery =>
            {
                subquery.Where(w =>
                    w.WhereEquals("ContentItemChannelID", _websiteChannelContext.WebsiteChannelID));
            });
            configure?.Invoke(builder);

            var queryOptions = BuildQueryExecutionOptions();

            var results = await _executor.GetMappedResult<T>(builder, queryOptions, cancellationToken);

            return results.ToList().AsReadOnly();
        }
        /// <inheritdoc />
        public async Task<T?> GetItemAsync<T>(
            Guid contentItemGuid,
            CancellationToken cancellationToken = default)
            where T : class
        {
            var builder = new ContentItemQueryBuilder()
                .ForContentType(
                    // XbyK resolves the content type name from the mapped type via the generated
                    // IContentQueryDataContainer registration; we filter by GUID below.
                    GetContentTypeName<T>(),
                    q => q.Where(w => w.WhereEquals(nameof(IContentQueryDataContainer.ContentItemGUID), contentItemGuid))
                          .TopN(1));

            var queryOptions = BuildQueryExecutionOptions();

            var results = await _executor.GetMappedResult<T>(builder, queryOptions, cancellationToken);

            return results.FirstOrDefault();
        }

        /// <inheritdoc />
        public async Task<int> GetItemCountAsync<T>(
     Action<ContentItemQueryBuilder>? configure = null,
     CancellationToken cancellationToken = default)
     where T : class
        {
            var builder = BuildQuery<T>(configure);

            // Strip ordering/paging that would conflict with COUNT, then select only
            // the system GUID column — we just need row count, not data.
            builder.Parameters(p => p
                .TopN(int.MaxValue)   // reset any TopN the caller may have set
            );

            var queryOptions = BuildQueryExecutionOptions();

            // GetResult<int> maps each raw IContentQueryDataContainer row to an int (1),
            // then we just take the count of returned rows.
            // This is the correct XbyK pattern — there is no GetCount() method.
            var results = await _executor.GetResult<int>(
                builder,
                _ => 1,              // map each row to a dummy scalar
                queryOptions,
                cancellationToken);

            return results.Count();
        }

        // -------------------------------------------------------------------------
        // Helpers
        // -------------------------------------------------------------------------

        /// <summary>
        /// Constructs a <see cref="ContentItemQueryBuilder"/> pre-scoped to the
        /// content type inferred from <typeparamref name="T"/>, then applies any
        /// additional caller-supplied configuration.
        /// </summary>
        private static ContentItemQueryBuilder BuildQuery<T>(Action<ContentItemQueryBuilder>? configure)
            where T : class
        {
            var builder = new ContentItemQueryBuilder()
                .ForContentType(GetContentTypeName<T>());

            configure?.Invoke(builder);

            return builder;
        }

        /// <summary>
        /// Builds execution options that scope queries to the current website channel
        /// and language. Adjust as needed for preview / multi-language scenarios.
        /// </summary>
        private ContentQueryExecutionOptions BuildQueryExecutionOptions()
        {
            return new ContentQueryExecutionOptions
            {
                // ForPreview can be wired to IPreviewService / a feature flag if needed.
                ForPreview = _websiteChannelContext.IsPreview,
            };
        }

        /// <summary>
        /// Resolves the XbyK content type code name for the given CLR type.
        /// XbyK generated models expose a static <c>CONTENT_TYPE_NAME</c> constant.
        /// We read it via reflection so callers never have to pass it explicitly.
        /// </summary>
        private static string GetContentTypeName<T>() where T : class
        {
            var type = typeof(T);

            // Convention used by the XbyK code generator.
            const string FieldName = "CONTENT_TYPE_NAME";

            var field = type.GetField(FieldName,
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.Static |
                System.Reflection.BindingFlags.FlattenHierarchy);

            if (field?.GetValue(null) is string name && !string.IsNullOrWhiteSpace(name))
            {
                return name;
            }

            // Fallback: use the class name as-is (may need namespace prefix in real projects).
            throw new InvalidOperationException(
                $"Could not resolve the XbyK content type name for '{type.FullName}'. " +
                $"Ensure the class exposes a public static string field named '{FieldName}', " +
                $"as generated by the Xperience by Kentico code generator.");
        }

        public async Task<IReadOnlyList<object>> GetItemsByTypesAsync(
     Type[] types,
     int WebsiteChannelID,
     CancellationToken cancellationToken = default)
        {
            var results = new List<object>();

            foreach (var type in types)
            {
                var method = typeof(ContentService)
                    .GetMethod(nameof(GetItemsAsync))
                    ?.MakeGenericMethod(type);

                if (method == null)
                {
                    throw new InvalidOperationException($"Cannot execute query for type {type.Name}");
                }

                Action<ContentItemQueryBuilder> configure = builder =>
                {
                    builder.Parameters(p =>
                    {
                        p.Where(w => w.WhereEquals("ContentItemChannelID", WebsiteChannelID));
                    });
                };

                var task = (Task)method.Invoke(this, new object?[] { configure, cancellationToken })!;
                await task.ConfigureAwait(false);

                var resultProperty = task.GetType().GetProperty("Result");

                var items = resultProperty?.GetValue(task) as System.Collections.IEnumerable;

                if (items != null)
                {
                    foreach (var item in items)
                    {
                        results.Add(item);
                    }
                }
            }

            return results.AsReadOnly();
        }

        public async Task<IEnumerable<IWebPageFieldsSource>> GetItemsByGUIDAsync(List<Guid> webPageItemGuidsfirst)
        {
            return await _contentRetriever.RetrieveAllPagesByGuids<IWebPageFieldsSource>(webPageItemGuidsfirst);
        }

        public async Task<IEnumerable<T>> GetContentType<T>(CancellationToken cancellationToken = default) where T : class, IWebPageFieldsSource
        {
            return await _contentRetriever.RetrievePages<T>(cancellationToken);
        }
        public async Task<IEnumerable<TPage>> GetChildPagesByParentGuidAsync<TPage>(
       int parentGuid,
       CancellationToken cancellationToken = default)
       where TPage : IWebPageFieldsSource
        {
            if (parentGuid == 0)
                throw new ArgumentException("Parent GUID must be provided", nameof(parentGuid));

            var childPages = await _contentRetriever.RetrievePages<TPage>(
                new RetrievePagesParameters { LinkedItemsMaxLevel = 1 },
            query =>
                {
                    // Filter by parent GUID
                    query.Where(w => w.WhereEquals("WebPageItemParentID", parentGuid))
                         .OrderBy(nameof(IWebPageContentQueryDataContainer.WebPageItemOrder));
                },
                cacheSettings: RetrievalCacheSettings.CacheDisabled, // Use the predefined disabled cache
                cancellationToken: cancellationToken
            );

            return childPages;
        }

        public Task<IEnumerable<TPage>> GetPagesAsync<TPage>(
      RetrievePagesParameters parameters,
      Action<RetrievePagesQueryParameters> queryAction,
      RetrievalCacheSettings cacheSettings,
      CancellationToken cancellationToken
  ) where TPage : class, IWebPageFieldsSource
        {
            // Call the extension method here
            return _contentRetriever.RetrievePages<TPage>(
                parameters,
                queryAction,
                cacheSettings,
                cancellationToken
            );
        }


        public Task<IEnumerable<T?>?> RetrievePagesAsync<T>(CancellationToken cancellationToken) where T : class, IWebPageFieldsSource
        {
            return _contentRetriever.RetrievePages<T?>(cancellationToken);
        }

        public Task<IEnumerable<T?>?> RetrievePagesAsync<T>(RetrievePagesParameters parameters, CancellationToken cancellationToken) where T : class, IWebPageFieldsSource
        {
            return _contentRetriever.RetrievePages<T?>(parameters, cancellationToken);
        }
        public string GetFallbackMetadataDescription(string? metaDescription, string? metaOGDescription)
        {
            var metaDesc = metaDescription;

            if (string.IsNullOrEmpty(metaDesc))
            {
                metaDesc = metaOGDescription;
            }

            return metaDesc ?? string.Empty;
        }
        public string GetCurrentSiteLogoImageUrlAsync()
        {
            return "https://cdn.bdo.global/images/bdo_logo/1.0.0/bdo_logo_color.png";
        }

        public string GetCurrentSiteLegalNameAsync()
        {
            return _websiteChannelContext.WebsiteChannelName;
        }
        public int GetCurrentChannelID()
        {
            return _websiteChannelContext.WebsiteChannelID;
        }

        public async Task<T?> RetrieveCurrentPageAsync<T>(RetrieveCurrentPageParameters parameters, CancellationToken cancellationToken = default) where T : class
        {
            return await _contentRetriever.RetrieveCurrentPage<T>(parameters, cancellationToken);
        }
    }
}


