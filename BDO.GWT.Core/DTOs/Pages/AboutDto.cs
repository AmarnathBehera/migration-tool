using BDO.GWT.Core;
using BDO.GWT.Core.DTOs.ReusableFieldsDTOs;
using BDO.GWT.Web.Core.DTOs;

namespace BDO.GWT.Core.DTOs;

public class AboutDto
{
    public HeroDto Hero { get; set; } = null!;

    public MetaDataDto? MetaData { get; set; }

    public SearchAndSEODto? SearchAndSEO { get; set; }

    public OGMetaDataDto? OGMetaData { get; set; }

    public BaseContent? BaseContent { get; set; }

    public BaseWebPage? BaseWebPage { get; set; }
}
