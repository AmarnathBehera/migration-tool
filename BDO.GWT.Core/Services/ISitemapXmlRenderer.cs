using BDO.GWT.Core.DTOs;

namespace BDO.GWT.Core.Services;

public interface ISitemapXmlRenderer
{
    string RenderXmlSitemap(SitemapDto sitemapDto);
}
