using BDO.GWT.Core.DTOs;
using CMS.Websites;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Core.services
{
    public interface IPageUtility
    {
            Task<SideBoxDto> GetSideboxDtoAsync<TPage>(
            string sideboxTitle,
            int parentWebPageItemId,
            Func<TPage, string> titleSelector,
            CancellationToken cancellationToken = default)
            where TPage : class, IWebPageFieldsSource;

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