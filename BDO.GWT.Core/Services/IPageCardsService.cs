using BDO.GWT.Core.Data.Queries;
using BDO.GWT.Core.DTOs;
using CMS.ContentEngine;
using CMS.Websites;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Web.Core.Services;

/// <summary>
/// Service abstraction for retrieving and filtering page-based card listings.
/// Migrated from Kentico 13 (IPage / IPageQuery / IMultiTypePageQuery) to
/// Xperience by Kentico (IWebPageFieldsSource / ContentItemQueryBuilder).
/// </summary>
/// <typeparam name="TPage">The Xperience by Kentico web page content type.</typeparam>
/// <typeparam name="TItemDto">The DTO type produced for each card.</typeparam>
public interface IPageCardsService<TPage, TItemDto>
    where TPage : class, IWebPageFieldsSource
{
    /// <summary>
    /// Adds a decorator that is applied to the underlying multi-type content query
    /// used when building the filter options.
    /// </summary>
    void AddFilterQueryDecorator<TPageFilter>(Action<ContentItemQueryBuilder> decorateFilterQuery)
        where TPageFilter : class, IWebPageFieldsSource;

    /// <summary>
    /// Creates the list of available filters for a card listing.
    /// </summary>
    /// <param name="filters">Names of the filters to build.</param>
    /// <param name="treePath">Optional web page tree path used to scope the filter values.</param>
    Task<FilterItemDto<OptionDto>[]> CreateFiltersAsync(HashSet<string> filters, string? treePath, CancellationToken cancellationToken);

    /// <summary>
    /// Returns a paginated, filtered list of card items.
    /// </summary>
    /// <param name="cardsQuery">Pagination, ordering and filter values supplied by the client.</param>
    /// <param name="decorateQuery">Optional decorator applied to the underlying content query.</param>
    /// <param name="isFeatured">Predicate used to identify featured pages.</param>
    Task<PaginationDto<TItemDto>> GetFilteredItemsAsync(
        CardsQuery cardsQuery,
        Action<ContentItemQueryBuilder>? decorateQuery,
        Func<TPage, bool> isFeatured,
        CancellationToken cancellationToken);
}
