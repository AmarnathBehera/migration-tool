namespace BDO.GWT.Core
{
    using CMS.ContentEngine;
    using CMS.Websites;
    using Kentico.Content.Web.Mvc;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IContentReadOnlyQuery
    {
        ContentItemQueryBuilder Builder { get; }
    }

    /// <summary>
    /// Abstraction over Xperience by Kentico content querying,
    /// replacing GenericPageRetriever / IPageRetriever from Kentico 13.
    /// </summary>
    public interface IContentService
    {
        /// <summary>
        /// Returns a list of content items of the specified type.
        /// T is usually a generated XbyK content model (e.g., HomePage, Insight, Deals).
        /// </summary>
        Task<IReadOnlyList<T>> GetItemsAsync<T>(
            Action<ContentItemQueryBuilder>? configure = null,
            CancellationToken cancellationToken = default)
            where T : class;

        Task<IReadOnlyList<T>> GetItemsChannelAsync<T>(
            Action<ContentItemQueryBuilder>? configure = null,
            CancellationToken cancellationToken = default)
            where T : class;

        /// <summary>
        /// Returns a single content item of the specified type by its GUID,
        /// or null if not found.
        /// </summary>
        Task<T?> GetItemAsync<T>(
            Guid contentItemGuid,
            CancellationToken cancellationToken = default)
            where T : class;

        /// <summary>
        /// Returns the total count of items matching the given configuration
        /// without loading all of them.
        /// </summary>
        Task<int> GetItemCountAsync<T>(
            Action<ContentItemQueryBuilder>? configure = null,
            CancellationToken cancellationToken = default)
            where T : class;

        /// <summary>
        /// Retrieves content items for multiple content types.
        /// </summary>
        Task<IReadOnlyList<object>> GetItemsByTypesAsync(
            Type[] types, int currentPageData,
            CancellationToken cancellationToken = default);

        /// <summary>
        //Retrieve page based on GUID
        /// </summary>
        Task<IEnumerable<IWebPageFieldsSource>> GetItemsByGUIDAsync(List<Guid> webPageItemGuidsfirst);

        /// <summary>
        //Retrieve page 
        /// </summary>
        Task<IEnumerable<T>> GetContentType<T>(CancellationToken cancellationToken = default) where T : class, IWebPageFieldsSource;

        Task<IEnumerable<TPage>> GetChildPagesByParentGuidAsync<TPage>(
       int parentGuid,
       CancellationToken cancellationToken = default)
       where TPage : IWebPageFieldsSource;

        Task<IEnumerable<TPage>> GetPagesAsync<TPage>(
      RetrievePagesParameters parameters,
      Action<RetrievePagesQueryParameters> queryAction,
      RetrievalCacheSettings cacheSettings,
      CancellationToken cancellationToken = default
  ) where TPage : class, IWebPageFieldsSource;


        public Task<IEnumerable<T?>?> RetrievePagesAsync<T>(CancellationToken cancellationToken) where T : class, IWebPageFieldsSource;
        public Task<IEnumerable<T?>?> RetrievePagesAsync<T>(RetrievePagesParameters parameters, CancellationToken cancellationToken) where T : class, IWebPageFieldsSource;
        
        public string GetFallbackMetadataDescription(string? metaDescription, string? metaOGDescription);
        public string GetCurrentSiteLogoImageUrlAsync();

        public string GetCurrentSiteLegalNameAsync();

        public int GetCurrentChannelID();
        Task<T?> RetrieveCurrentPageAsync<T>(RetrieveCurrentPageParameters parameters, CancellationToken cancellationToken = default) where T : class;

        Task<IWebPageFieldsSource?> GetPageByTypeAndNameAsync(
     Type type,
     string pageName,
     CancellationToken cancellationToken = default);

       public string? EnsureAbsoluteUrl(string? url);
    }
}
