using BDO.GWT.Core.Services;
using BDO.GWT.Web.Core.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
namespace BDO.GWT.Core.Controllers;

public class SitemapXmlController : Controller
{
    private readonly ISitemapService _sitemapService;
    private readonly ISitemapXmlRenderer _sitemapXmlRenderer;

    public SitemapXmlController(ISitemapService sitemapService, ISitemapXmlRenderer sitemapXmlRenderer)
    {
        _sitemapService = sitemapService;
        _sitemapXmlRenderer = sitemapXmlRenderer;
    }

    [HttpGet("/sitemap.xml")]
    [Produces(MediaTypeNames.Application.Xml)]
    public async Task<ActionResult> Index(CancellationToken cancellationToken)
    {
        var sitemapDto = await _sitemapService.GetSitemapDtoAsync(cancellationToken);

        var xmlContent = _sitemapXmlRenderer.RenderXmlSitemap(sitemapDto);
        return Content(xmlContent, "application/xml");
    }
}