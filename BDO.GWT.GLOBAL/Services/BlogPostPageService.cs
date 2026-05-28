using BDO;
using BDO.GWT.Core;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.ReusableFieldsDTOs;
using Kentico.Content.Web.Mvc;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Web.Core.Services;

public interface IBlogPostPageService
{
    Task<BlogPostDto> GetBlogPostDtoAsync(CancellationToken cancellationToken);
}

public class BlogPostPageService : IBlogPostPageService
{
    private readonly IContentService _contentService;

    public BlogPostPageService(IContentService contentService)
    {
        _contentService = contentService;
    }

    public async Task<BlogPostDto> GetBlogPostDtoAsync(CancellationToken cancellationToken)
    {
        var page = await _contentService.RetrieveCurrentPageAsync<BlogPost>(
            new RetrieveCurrentPageParameters { LinkedItemsMaxLevel = 2 },
            cancellationToken);

        if (page == null)
        {
            return new BlogPostDto();
        }

       
        var imageFileUrl = page.ImageFile?.FirstOrDefault()?.ContentAttachmentAsset?.Url;

        return new BlogPostDto
        {
            ImageUrl = imageFileUrl ?? string.Empty,
            ImageAltText = page.ImageFileAltText ?? string.Empty,
            BaseContent = new BaseContent
            {
                ContentItemID = page.SystemFields.ContentItemID,
                ContentItemGUID = page.SystemFields.ContentItemGUID,
                ContentItemName = page.SystemFields.ContentItemName,
                ContentItemContentTypeID = page.SystemFields.ContentItemContentTypeID,
                ContentItemIsSecured = page.SystemFields.ContentItemIsSecured,
                ContentItemCommonDataContentLanguageID = page.SystemFields.ContentItemCommonDataContentLanguageID,
                ContentItemCommonDataFirstPublishedWhen = page.SystemFields.ContentItemCommonDataFirstPublishedWhen,
                ContentItemCommonDataLastPublishedWhen = page.SystemFields.ContentItemCommonDataLastPublishedWhen,
            },
            BaseWebPage = new BaseWebPage
            {
                WebPageItemGUID = page.SystemFields.WebPageItemGUID,
                WebPageItemID = page.SystemFields.WebPageItemID,
                WebPageItemName = page.SystemFields.WebPageItemName,
                WebPageItemWebsiteChannelID = page.SystemFields.WebPageItemWebsiteChannelId,
                WebPageUrlPath = page.SystemFields.WebPageUrlPath,
                WebPageItemOrder = page.SystemFields.WebPageItemOrder,
                WebPageItemTreePath = page.SystemFields.WebPageItemTreePath,
                WebPageUrlPathContentLanguageID = page.SystemFields.WebPageUrlPathContentLanguageID,
                WebPageItemParentID = page.SystemFields.WebPageItemParentID,
            },
        };
    }
}
