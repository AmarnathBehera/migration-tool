//using BDO.GWT.Core.Bases;
//using BDO.GWT.Core.Services;
//using Microsoft.AspNetCore.Mvc;
//using System.Net.Mime;
//using System.Threading;
//using System.Threading.Tasks;

//namespace BDO.GWT.Web.Controllers;

//public class RssController : PageController
//{

//    private readonly IRssPageService _rssPageService;

//    public RssController( IRssPageService rssPageService)
//    {

//        _rssPageService = rssPageService;
//    }

//    public async Task<IActionResult> Index(CancellationToken cancellationToken)
//    {
//        var rssFeedXml = await _rssPageService.GetRssFeedXmlAsync(cancellationToken);

//        return Content(rssFeedXml, MediaTypeNames.Application.Xml);
//    }
//}
