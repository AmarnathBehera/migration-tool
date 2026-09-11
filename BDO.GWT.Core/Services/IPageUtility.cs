using BDO.GWT.Core.DTOs;
using CMS.Websites;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Core.services
{
    public interface IPageUtility
    {
        Task<SideBoxDto> GetSideboxDtoAsync<TChild, TSibling>(
            string sideboxTitle,
            int parentWebPageItemId,
     int siblingParentWebPageItemId,
     Func<TChild, string> childTitleSelector,
     Func<TSibling, string> siblingTitleSelector,
            CancellationToken cancellationToken = default)
     where TChild : class, IWebPageFieldsSource
     where TSibling : class, IWebPageFieldsSource;


        Task<SideBoxDto> GetSideboxDtoAsync<TSibling>(
            string sideboxTitle,
            int parentWebPageItemId,
     int siblingParentWebPageItemId,
     Func<TSibling, string> siblingTitleSelector,
            CancellationToken cancellationToken = default)
     where TSibling : class, IWebPageFieldsSource;

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

        Task<MultiLinkCardDto[]> GetMultiLinkCardsAsync2<TCategory, TChild1,TChild2>(
            int parentWebPageItemId,
            Func<TCategory, string> cardTitleSelector,
            Func<TCategory, string> cardDescriptionSelector,
            Func<TCategory, string?> cardImageUrlSelector,
            Func<TCategory, string?> cardImageAltTextSelector,
            Func<TChild1, string> childTitleSelector,
            Func<TChild2, string> child2TitleSelector,
            CancellationToken cancellationToken = default)
            where TCategory : class, IWebPageFieldsSource
            where TChild2 : class, IWebPageFieldsSource
            where TChild1: class, IWebPageFieldsSource;
    }
}