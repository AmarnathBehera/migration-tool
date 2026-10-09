using BDO.GWT.Core;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.ReusableFieldsDTOs;
using BDO.GWT.Web.Core.DTOs;

namespace BDO.GWT.Core.DTOs;

public class FormPageDto
{
    public MetaDataDto? MetaData { get; set; }
    public SearchAndSEODto? SearchAndSEO { get; set; }

    public OGMetaDataDto? OGMetaData { get; set; }

    public BaseContent? BaseContent { get; set; }

    public BaseWebPage? BaseWebPage { get; set; }
    public string Title { get; set; } = null!;

    public string Description { get; set; } = null!;

    public HeroDto Hero { get; set; } = null!;

    public string? FormType { get; set; }

    public string? Form { get; set; }

    public string? EmbedCode { get; set; }
}
