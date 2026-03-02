namespace BDO.GWT.Core
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using CMS.ContentEngine;
    using CMS.Websites;    

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
    }
}
