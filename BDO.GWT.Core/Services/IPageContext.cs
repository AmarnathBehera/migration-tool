using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.ReusableFieldsDTOs;
using CMS.Websites;
using System;
using System.Collections.Generic;
using System.Text;
namespace BDO.GWT.Core.Services
{
    public interface IPageContext
    {
        Task<T?> GetCurrentPageAsync<T>(CancellationToken cancellationToken = default) where T : class;
        Task<MetaDataSEOItemsDto> GetCurrentPageMetadataAsync(CancellationToken cancellationToken = default);
        bool IsInPreviewMode { get; }
        bool IsInPageBuilderMode { get; }
        Task<BaseWebPage> GetCurrentPageBasePropertiesAsync(CancellationToken cancellationToken = default);
        Task<IWebPageFieldsSource> GetAnyPageByWebPageItemId(int webPageItemId, string languageName, CancellationToken cancellationToken);
        Task<string?> GetPageTitleByWebPageItemId(int webPageItemId, string languageName, CancellationToken cancellationToken);
    }

}
