using BDO.GWT.Core.DTOs;
using CMS.Websites;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Core.services
{
    /// <summary>
    /// Abstraction over common page utility operations,
    /// such as building sidebox DTOs from child page relationships.
    /// </summary>
    public interface IPageUtility
    {
        /// <summary>
        /// Builds a <see cref="SideBoxDto"/> by loading child pages of the specified type
        /// under the given parent page, using a caller-supplied title selector.
        /// </summary>
        /// <typeparam name="TPage">The XbyK generated content type of the child pages.</typeparam>
        /// <param name="sideboxTitle">The title displayed at the top of the sidebox.</param>
        /// <param name="parentWebPageItemId">The WebPageItemID of the parent page whose children to load.</param>
        /// <param name="titleSelector">A lambda that selects the link title from each child page.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A populated <see cref="SideBoxDto"/>.</returns>
        Task<SideBoxDto> GetSideboxDtoAsync<TPage>(
            string sideboxTitle,
            int parentWebPageItemId,
            Func<TPage, string> titleSelector,
            CancellationToken cancellationToken = default)
            where TPage : class, IWebPageFieldsSource;


        /// <summary>
        /// Builds a list of MultiLinkCardDto by loading parent category pages and
        /// their child pages, using caller-supplied field selectors.
        /// TCategory = the card-level page type (e.g. IndustryCategory)
        /// TChild    = the sub-link page type nested inside each category (e.g. Industry)
        /// </summary>
        Task<MultiLinkCardDto[]> GetMultiLinkCardsAsync<TCategory, TChild>(
            int parentWebPageItemId,
            Func<TCategory, string> cardTitleSelector,
            Func<TCategory, string> cardDescriptionSelector,
            Func<TCategory, string?> cardImageUrlSelector,
            Func<TCategory, string?> cardImageAltTextSelector,
            Func<TChild, string> childTitleSelector,
            CancellationToken cancellationToken = default)
            where TCategory : class, IWebPageFieldsSource
            where TChild : class, IWebPageFieldsSource;
    }
}