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

            return View(headerDto);
        }
    }
}