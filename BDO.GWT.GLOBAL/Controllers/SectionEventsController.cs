using BDO;
using BDO.GWT.Core;
using BDO.GWT.Core.Services;
using Kentico.Content.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace GWTXbyKWeb.Controllers
{
    public class SectionEventsController : Controller
    {
        private readonly IContentService _contentService;
        private readonly ISectionEventsService _sectionEventsService;

        public SectionEventsController(IContentService contentService, ISectionEventsService sectionEventsService)
        {
            _contentService = contentService;
            _sectionEventsService = sectionEventsService;
        }
        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var page = await _contentService.RetrieveCurrentPageAsync<SectionEvents>(
                new RetrieveCurrentPageParameters { LinkedItemsMaxLevel = 4 }, cancellationToken);

            if (page is null)
                return NotFound();
            var pageDto = await _sectionEventsService.GetSectionEventsDtoAsync(page, cancellationToken);
            return View("~/Views/SectionEvent/Index.cshtml", pageDto);
        }

    }
}
