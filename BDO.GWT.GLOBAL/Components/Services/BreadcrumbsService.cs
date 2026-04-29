using AngleSharp.Attributes;
using BDO.GWT.Core;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.Services;
using CMS.ContentEngine;
using CMS.DataEngine;
using CMS.Websites;
using Kentico.Content.Web.Mvc;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Core.Services
{
    public interface IBreadcrumbsService
    {
        Task<BreadcrumbsDto> GetAsync(CancellationToken cancellationToken = default);
    }

    public class BreadcrumbsService : IBreadcrumbsService
    {
        private readonly IWebPageDataContextRetriever _dataContextRetriever;
        private readonly IWebPageUrlRetriever _pageUrlRetriever;
        private readonly IHomePageService _homePageService;
        private readonly IPageContext _pageContext;

        public BreadcrumbsService(
            IWebPageDataContextRetriever dataContextRetriever,
            IWebPageUrlRetriever pageUrlRetriever,
            IHomePageService homePageService,
            IPageContext pageContext)
        {
            _dataContextRetriever = dataContextRetriever;
            _pageUrlRetriever = pageUrlRetriever;
            _homePageService = homePageService;
            _pageContext = pageContext;
        }

        public async Task<BreadcrumbsDto> GetAsync(CancellationToken cancellationToken = default)
        {
            if (!_dataContextRetriever.TryRetrieve(out var ctx))
                return new BreadcrumbsDto();

            var currentPage = ctx.WebPage;

            // Home page doesn't need breadcrumbs
            if (currentPage.ContentTypeName == HomePage.CONTENT_TYPE_NAME)
                return new BreadcrumbsDto { Links = new List<LinkItemDto>() };

            var homeDto = await _homePageService.GetHomePageDtoAsync(null, cancellationToken);
            var homeTitle = homeDto?.MetaData?.MetaTitle ?? "Home";

            var page = await _pageContext.GetCurrentPageBasePropertiesAsync(cancellationToken);
            var parentPageID = page.WebPageItemParentID;

            var ancestors = new List<(string Title, int WebPageItemId)>();

            ancestors.Add((page.WebPageItemName, currentPage.WebPageItemID));

            // Walk up the content tree until we hit the root or HomePage
            var current = await _pageContext.GetAnyPageByWebPageItemId(parentPageID, cancellationToken);
            while (parentPageID > 0 && current != null)
            {
                ancestors.Insert(0, (
                    current.SystemFields.WebPageItemName,
                    current.SystemFields.WebPageItemID
                ));

                parentPageID = current.SystemFields.WebPageItemParentID;
                if (parentPageID > 0)
                {
                    current = await _pageContext.GetAnyPageByWebPageItemId(parentPageID, cancellationToken);
                }
            }
            // Home is always the first breadcrumb link
            var links = new List<LinkItemDto>
    {
        new LinkItemDto { Title = homeTitle, URL = "/", Target = "_self" }
    };

            foreach (var (_, webPageItemId) in ancestors)
            {
                var url = await _pageUrlRetriever.Retrieve(webPageItemId, ctx.WebPage.LanguageName, cancellationToken: cancellationToken);
                var title = await _pageContext.GetPageTitleByWebPageItemId(webPageItemId, cancellationToken);

                links.Add(new LinkItemDto
                {
                    Title = title ?? "Page",
                    URL = url?.RelativePath?.Replace("~", "") ?? "#",
                    Target = "_self"
                });
            }

            return new BreadcrumbsDto { Links = links };
        }
    }
}