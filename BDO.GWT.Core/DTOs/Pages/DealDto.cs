using BDO.GWT.Core;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.ReusableFieldsDTOs;
using BDO.GWT.Web.Core.DTOs;

namespace BDO.GWT.Web.Core.DTOs.Pages;

public class DealDto
{
    public MetaDataDto? MetaData { get; set; }

    public SearchAndSEODto? SearchAndSEO { get; set; }

    public OGMetaDataDto? OGMetaData { get; set; }

    public BaseContent? BaseContent { get; set; }

    public BaseWebPage? BaseWebPage { get; set; }

    public HeroDto Hero { get; set; } = null!;

    public string? Title { get; set; }

    public string? Content { get; set; }

    public string? ClientName { get; set; }

    public DateTime? Date { get; set; }

    public string? OfficeLocation { get; set; }

    public string? Country { get; set; }

    public bool SharingOptIn { get; set; }

    public List<string> DealTypeNames { get; set; } = [];

    public List<string> IndustryNames { get; set; } = [];

    public List<string> SubIndustryNames { get; set; } = [];

    public List<string> SizeNames { get; set; } = [];

    public ContentSidebarDto? Sidebar { get; set; }

    public CallToActionDto? CallToAction { get; set; }
}
