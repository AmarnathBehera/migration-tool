using BDO;
using BDO.GWT.Common.Exceptions;
using BDO.GWT.Core;
using BDO.GWT.Core.Constants;
using BDO.GWT.Core.Data;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Pages;
using BDO.GWT.Core.DTOs.ReusableFieldsDTOs;
using BDO.GWT.Core.Helpers;
using BDO.GWT.Core.Retrievers;
using BDO.GWT.Core.Services;
using BDO.GWT.Core.Utilities;
using BDO.GWT.Core.ValueObjects;
using CMS.Websites;
using Kentico.Content.Web.Mvc;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Web.Core.Services;

public interface IBlogPostPageService
{
    Task<BlogPostDto> GetBlogPostDtoAsync(BlogPost blogpostPage,CancellationToken cancellationToken);
}

public class BlogPostPageService : IBlogPostPageService
{
    private readonly ISidebarService _sidebarService;
    private readonly IExternalPeopleHelper _externalPeopleHelper;

    public BlogPostPageService(ISidebarService sidebarService, IExternalPeopleHelper externalPeopleHelper)
    {
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
        var pageUrl = blogpostPage.SystemFields.WebPageUrlPath;

        

        var imageFileUrl = blogpostPage.ImageFileM?.FirstOrDefault()?.ContentAttachmentAsset?.Url;


        BlogPostDto BlogPostDtoObj = new BlogPostDto
        {
           
            ImageDto = new ImageDto
            {
                Url = imageFileUrl ?? string.Empty,
                AltText = blogpostPage.ImageFileAltTextM ?? string.Empty,
            },
            ShowSidebar = blogpostPage.ShowSidebar,
            ShowTitle = blogpostPage.ShowTitle,
            ShowDescription = blogpostPage.ShowDescription,
            ShowCallToAction = blogpostPage.ShowCallToAction,
            Content = HtmlUtility.RemoveTildeFromUrlTypeHtmlAttributes(blogpostPage.Content) ?? string.Empty,

        };

            //getting sidebar data if ShowSidebar is true
        ContentSidebarDto? sideBarObj = BlogPostDtoObj.ShowSidebar
            ? await _sidebarService.GetSidebarDtoAsync(blogpostPage, pageUrl, cancellationToken)
            : null;

        if (BlogPostDtoObj.ShowCallToAction)
        {
            BlogPostDtoObj.CallToAction = PageHelper.GetCallToActionDto(blogpostPage);
        }

        if (blogpostPage.RelevantContactPerson != null)
        {
            relevantPersonGuids = blogpostPage.RelevantContactPerson.Select(p => p.SystemFields.ContentItemGUID).ToList();
        }

        //check if ShowTitle and ShowDescription are true before setting the values in DTO
        if (BlogPostDtoObj.ShowTitle)
        {
            BlogPostDtoObj.Title = blogpostPage.MetadataTitle ?? string.Empty;
        }
        if(BlogPostDtoObj.ShowDescription)
        {
            BlogPostDtoObj.Description = HtmlUtility.RemoveTildeFromUrlTypeHtmlAttributes(blogpostPage.MetadataDescription) ?? string.Empty;
        }

        //check if Sidebar DTO is not null before setting the values in DTO
        if (sideBarObj != null)
        {
            var publishedDate = blogpostPage.SystemFields.ContentItemCommonDataLastPublishedWhen ??
                                blogpostPage.SystemFields.ContentItemCommonDataFirstPublishedWhen;
            var updatedDate = blogpostPage.UpdatedOn == DateTime.MinValue ? null : (DateTime?)blogpostPage.UpdatedOn;


            sideBarObj = await _sidebarService.GetContentPageSidebarDetails(sideBarObj, cancellationToken,
                                                                          relevantPersonGuids, publishedDate, updatedDate);

            if (blogpostPage.RelatedExternalPeople != null)
            {
                sideBarObj.RelatedExternalPeople = await _externalPeopleHelper.GetSidebarExternalPeopleDtosAsync(
                             blogpostPage.RelatedExternalPeople.Select(p => p.SystemFields.ContentItemGUID).ToList(),
                        cancellationToken);
            }
        }

        if (sideBarObj != null) {
            BlogPostDtoObj.Sidebar = sideBarObj;
        }

        return BlogPostDtoObj;
    }
}