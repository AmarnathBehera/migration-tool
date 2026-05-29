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
            new RetrieveCurrentPageParameters { LinkedItemsMaxLevel = 3 },
            cancellationToken);

        if (page == null)
        {
            return new BlogPostDto();
        }

       
        var imageFileUrl = page.ImageFile?.FirstOrDefault()?.ContentAttachmentAsset?.Url;

        return new BlogPostDto
        {
            MetaData = new MetaDataDto
            {
                MetaTitle = page.MetadataTitle ?? string.Empty,
                MetaDescription = page.MetadataDescription ?? string.Empty,
                
            },
            ImageDto = new ImageDto
            {
                 Url = imageFileUrl ?? string.Empty,
                 AltText = page.ImageFileAltText ?? string.Empty,
            },
            

        };
    }
}
