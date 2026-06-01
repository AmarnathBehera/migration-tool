using BDO;
using BDO.GWT.Common.Exceptions;
using BDO.GWT.Core;
using BDO.GWT.Core.Constants;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Pages;
using BDO.GWT.Core.DTOs.ReusableFieldsDTOs;
using BDO.GWT.Core.Helpers;
using BDO.GWT.Core.Retrievers;
using BDO.GWT.Core.Services;
using BDO.GWT.Core.Utilities;
using CMS.Websites;
using Kentico.Content.Web.Mvc;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Web.Core.Services;

public interface IBlogPostPageService
{
    Task<BlogPostDto> GetBlogPostDtoAsync(BlogPost contentPage,CancellationToken cancellationToken);
}

public class BlogPostPageService : IBlogPostPageService
{
    private readonly IContentService _contentService;
    private readonly ISidebarService _sidebarService;
    private readonly IExternalPeopleHelper _externalPeopleHelper;

    public BlogPostPageService(IContentService contentService, ISidebarService sidebarService, IExternalPeopleHelper externalPeopleHelper)
    {
        _contentService = contentService;
        _sidebarService = sidebarService;
        _externalPeopleHelper = externalPeopleHelper;   
    }

    public async Task<BlogPostDto> GetBlogPostDtoAsync(BlogPost blogpostPage, CancellationToken cancellationToken)
    {
        if (blogpostPage == null)
        {
            return new BlogPostDto();
        }

        List<Guid> relevantPersonGuids = new List<Guid>();
        var pageUrl = blogpostPage.GetUrl().AbsoluteUrl;
        
        //getting sidebar data if ShowSidebar is true
        ContentSidebarDto? sideBarDto = blogpostPage.ShowSidebar
                ? await _sidebarService.GetSidebarDtoAsync(blogpostPage, pageUrl, cancellationToken)
                : null;

        var imageFileUrl = blogpostPage.ImageFile?.FirstOrDefault()?.ContentAttachmentAsset?.Url;

        return new BlogPostDto
        {
            MetaData = new MetaDataDto
            {
                MetaTitle = blogpostPage.MetadataTitle ?? string.Empty,
                MetaDescription = blogpostPage.MetadataDescription ?? string.Empty,
                
            },
            ImageDto = new ImageDto
            {
                 Url = imageFileUrl ?? string.Empty,
                 AltText = blogpostPage.ImageFileAltText ?? string.Empty,
            },
            ShowSidebar = blogpostPage.ShowSidebar,
            ContentSidebarDto = sideBarDto,

        };
    }
}
