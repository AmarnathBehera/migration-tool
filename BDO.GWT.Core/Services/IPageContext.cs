using System;
using System.Collections.Generic;
using System.Text;
using BDO.GWT.Core.DTOs.ReusableFieldsDTOs;
using BDO.GWT.Core.DTOs;
namespace BDO.GWT.Core.Services
{
    public interface IPageContext
    {
        Task<T?> GetCurrentPageAsync<T>(CancellationToken cancellationToken = default) where T : class;
        Task<MetaDataSEOItemsDto> GetCurrentPageMetadataAsync(CancellationToken cancellationToken = default);
        bool IsInPreviewMode { get; }
        bool IsInPageBuilderMode { get; }
    }
   
}
