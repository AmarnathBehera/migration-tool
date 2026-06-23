using BDO.GWT.Core.DTOs;
using CMS.Websites;
using Kentico.Content.Web.Mvc;
using System.Collections.Generic;
using System.Linq;
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
        private readonly IPageContext _pageContext;
        private readonly IContentService _contentService;


        public BreadcrumbsService(
            IWebPageDataContextRetriever dataContextRetriever,
            IWebPageUrlRetriever pageUrlRetriever,
            IPageContext pageContext,
            IContentService contentService
            )
        {
            _dataContextRetriever = dataContextRetriever;
            _pageUrlRetriever = pageUrlRetriever;
            _pageContext = pageContext;
            _contentService = contentService;
        }

        public async Task<BreadcrumbsDto> GetAsync(CancellationToken cancellationToken = default)
        {
            if (!_dataContextRetriever.TryRetrieve(out var ctx))
                return new BreadcrumbsDto();

            var currentPage = ctx.WebPage;

            // Home page doesn't need breadcrumbs
            if (currentPage.ContentTypeName == HomePage.CONTENT_TYPE_NAME)
                return new BreadcrumbsDto { Links = new List<LinkItemDto>() };

            var homepages = await _contentService.RetrievePagesAsync<HomePage>(cancellationToken);
            var homepage = homepages.FirstOrDefault();

            var homeTitle = homepage?.MetadataTitle
                ?? homepage?.PageTitle
                ?? "Home";

            var homeUrl = "/";
            if (homepage != null)
            {
                homeUrl = homepage.GetUrl().RelativePath;
            }

            var page = await _pageContext.GetCurrentPageBasePropertiesAsync(cancellationToken);
            var parentPageID = page.WebPageItemParentID;

            var ancestors = new List<(string Title, int WebPageItemId)>();

            ancestors.Add((page.WebPageItemName, currentPage.WebPageItemID));

            // Walk up the content tree until we hit the root or HomePage
            var current = await _pageContext.GetAnyPageByWebPageItemId(parentPageID, cancellationToken);
            while (parentPageID > 0 && current != null)
            {
                // Skip SectionMicrosites — breadcrumb starts from MicrositeMasterPage
                if (current is not SectionMicrosites)
                {
                    ancestors.Insert(0, (
                        current.SystemFields.WebPageItemName,
                        current.SystemFields.WebPageItemID
                    ));
                }

                parentPageID = current.SystemFields.WebPageItemParentID;
                if (parentPageID > 0)
                {
                    current = await _pageContext.GetAnyPageByWebPageItemId(parentPageID, cancellationToken);
                }
            }
            var isMicrositeContentType = currentPage.ContentTypeName == MicrositeMasterPage.CONTENT_TYPE_NAME
                || currentPage.ContentTypeName == MicrositePage.CONTENT_TYPE_NAME
                || currentPage.ContentTypeName == SectionMicrosites.CONTENT_TYPE_NAME;

            var links = new List<LinkItemDto>();
            if (!isMicrositeContentType)
            {
                links.Add(new LinkItemDto { Title = homeTitle, URL = homeUrl, Target = "_self" });
            }

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