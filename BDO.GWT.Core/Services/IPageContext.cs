using System;
using System.Collections.Generic;
using System.Text;
using BDO.GWT.Core.DTOs.ReusableFieldsDTOs;
namespace BDO.GWT.Core.Services
{
    public interface IPageContext
    {
        Task<T?> GetCurrentPageAsync<T>(CancellationToken cancellationToken = default) where T : class;
        Task<MetaDataDto> GetCurrentPageMetadataAsync(CancellationToken cancellationToken = default);
        bool IsInPreviewMode { get; }
        bool IsInPageBuilderMode { get; }
    }
    //public sealed class PageMetadataDto
    //{
    //    public string? Title { get; init; }
    //    public string? Description { get; init; }
    //    public string? Keywords { get; init; }
    //}
}
