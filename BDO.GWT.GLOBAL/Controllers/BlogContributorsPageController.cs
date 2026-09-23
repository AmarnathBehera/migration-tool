using BDO.GWT.Core;
using BDO.GWT.Core.Services;
using BDO.GWT.Web.Core.Services;
using CMS.Core;
using CMS.Websites;
using CMS.Websites.Routing;
using Kentico.Content.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Web.Controllers
{
    public class BlogContributorsPageController : Controller
    {
        private readonly IBlogContributorsPageService _blogContributorsPageService;
        private readonly IContentService _contentService;
        private readonly IWebsiteChannelContext _websiteChannelContext;

        public BlogContributorsPageController(IBlogContributorsPageService blogContributorsPageService, IContentService contentService, IWebsiteChannelContext websiteChannelContext)
        {
            _blogContributorsPageService = blogContributorsPageService;
            _contentService = contentService;
            _websiteChannelContext = websiteChannelContext;
        }

        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var currentPage = await _contentService.RetrieveCurrentPageAsync<BlogContributors>(
            new RetrieveCurrentPageParameters { LinkedItemsMaxLevel = 4 }, cancellationToken);

            if (currentPage is null)
                return NotFound();

            var channelID = _websiteChannelContext.WebsiteChannelID;
            if (channelID == 0)
            {
                CMS.Core.Service.Resolve<IEventLogService>().LogWarning("SiteConfigurationService", "GetSiteConfigurationDtoAsync", $"WebsiteChannelID is 0 for site {_siteContext.CurrentSiteName}.");
            }
            var channelname = _websiteChannelContext.WebsiteChannelName;


            var websiteChannels = WebsiteChannelInfo.Provider.Get().ToList();

            var websitedomain = websiteChannels.FirstOrDefault(x => x.WebsiteChannelID == channelID)?.WebsiteChannelDomain;

            CMS.Core.Service.Resolve<CMS.Core.IEventLogService>()
                .LogInformation(
                    source: "CustomCode",
                    eventCode: "DOMAIN_LOG",
                    eventDescription: $"Website domain: {websitedomain}, Website channel name: {channelname}"
                );

            var pageDto = await _blogContributorsPageService.GetSectionPeoplePageDtoAsync(currentPage, cancellationToken);

            return View("~/Views/BlogContributorsPage/Index.cshtml", pageDto);
            
        }
    }
    }
