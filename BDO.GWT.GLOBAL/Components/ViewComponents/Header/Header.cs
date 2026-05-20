using BDO.GWT.Core;
using BDO.GWT.Core.Services.Header;
using BDO.GWT.Core.ValueObjects;
using CMS.Core;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace BDO.GWT.Global.Components.ViewComponents
{
    public class Header : ViewComponent
    {
        private readonly IHeaderService _headerService;
        private readonly IContentService _contentService;
        private readonly IEventLogService _eventLogService;

        public Header(IHeaderService headerService, IContentService contentService,IEventLogService eventLogService)
        {
            _headerService = headerService;
            _contentService = contentService;
            _eventLogService = eventLogService;
        }

        public async Task<IViewComponentResult> InvokeAsync(CancellationToken cancellationToken)
        {
            var headerDto = await _headerService.GetAsync(_contentService, cancellationToken);

            if (headerDto == null)
            {
                return View(headerDto);
            }

            // Log any featured items with missing ImageURL 
            //this needs to be removed after we fix the issue with missing image urls for featured items in mega menu
            var megaMenuItems = headerDto.MegaMenuItems;
            if (megaMenuItems != null && _eventLogService != null)
            {
                var firstLevelItems = (from item in megaMenuItems where item.Featured != null && item.Featured.Any() select item).ToList();
                var featuredItems = firstLevelItems.SelectMany(item => item.Featured).ToList();

                foreach (var featured in featuredItems)
                {
                    _eventLogService.LogInformation(
                       source: "Header.cs class",
                       eventCode: "Featured image url", eventDescription: featured.ImageURL);
                }
            }

            return View(headerDto);
        }
    }
}