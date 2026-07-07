using BDO.GWT.Core.DTOs.ReusableFieldsDTOs;
using BDO.GWT.Core.DTOs.SchemaMarkups;
using BDO.GWT.Web.Core.DTOs;
using BDO.GWT.Web.Core.DTOs.Pages;
using BDO.GWT.Web.Core.DTOs.SchemaMarkups;

namespace BDO.GWT.Core.DTOs.Pages;

public class InsightPageDto : ContentPageDto
{
    public MetaDataDto? MetaData { get; set; }
    public SearchAndSEODto? SearchAndSEO { get; set; }

    public OGMetaDataDto? OGMetaData { get; set; }

    public BaseContent? BaseContent { get; set; }

    public BaseWebPage? BaseWebPage { get; set; }
    public required ArticleSchemaMarkupDto SchemaMarkup { get; set; }

    public bool? IsLayoutNarrow { get; set; }

    public PageConfigurationDto? PageConfiguration { get; set; }

    public InsightSourceDto? InsightSource { get; set; }

    public PeopleGridDto? Authors { get; set; }

    public bool? ShowPeopleInline { get; set; }

    public InsightPageDto(ContentPageDto contentPageDto) : base(contentPageDto)
    {
    }
}

