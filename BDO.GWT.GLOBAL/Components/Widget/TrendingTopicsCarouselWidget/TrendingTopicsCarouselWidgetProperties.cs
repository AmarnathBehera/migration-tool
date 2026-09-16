using BDO.GWT.Web.Widgets;
using CMS.ContentEngine;
using CMS.DataEngine;
using Kentico.Xperience.Admin.Base.FormAnnotations;
using Kentico.Xperience.Admin.Base.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Core.Components.Widget.TrendingTopicsCarouselWidget;

public class TrendingTopicsCarouselWidgetProperties : BaseWidgetProperties
{
    [TextInputComponent(Label = "Title", ExplanationText = "Title that will be shown over trending topics widget.", Order = 0)]
    public string Title { get; set; }

    [TextInputComponent(Label = "Description", ExplanationText = "Description that will be shown over trending topics widget but under title.", Order = 1)]
    public string Description { get; set; }

    [DropDownComponent(Label = "Topics page type", ExplanationText = "Type of pages that will be shown for this widget.", Order = 2,
        Options = "Blog\r\nIndustry\r\nInsight\r\nMicrosite\r\nService\r\nSpecialty")]
    public string TrendingTopicsPageType { get; set; } = null;

    [ContentItemSelectorComponent(contentTypeNames: [SectionBlogs.CONTENT_TYPE_NAME,Blog.CONTENT_TYPE_NAME,BlogPost.CONTENT_TYPE_NAME], Label = "Pages", Order = 3)]
    [VisibleIfEqualTo(nameof(TrendingTopicsPageType), "Blog")]
    public IEnumerable<ContentItemReference> Pages { get; set; } = Enumerable.Empty<ContentItemReference>();

    [ContentItemSelectorComponent(contentTypeNames: [SectionServices.CONTENT_TYPE_NAME, Service.CONTENT_TYPE_NAME, BusinessLine.CONTENT_TYPE_NAME, ServiceArea.CONTENT_TYPE_NAME, Sub_Service.CONTENT_TYPE_NAME], Order = 3, Label = "Pages")]
    [VisibleIfEqualTo(nameof(TrendingTopicsPageType), "Service")]
    public IEnumerable<ContentItemReference> ServicePages { get; set; } = new List<ContentItemReference>();

    [ContentItemSelectorComponent(contentTypeNames: [SectionIndustries.CONTENT_TYPE_NAME, Industry.CONTENT_TYPE_NAME, IndustryCategory.CONTENT_TYPE_NAME, IndustryService.CONTENT_TYPE_NAME], Order = 3, Label = "Pages")]
    [VisibleIfEqualTo(nameof(TrendingTopicsPageType), "Industry")]
    public IEnumerable<ContentItemReference> IndustryPages { get; set; } = new List<ContentItemReference>();

    [ContentItemSelectorComponent(contentTypeNames: [SectionMicrosites.CONTENT_TYPE_NAME, MicrositeMasterPage.CONTENT_TYPE_NAME, MicrositePage.CONTENT_TYPE_NAME], Order = 3, Label = "Pages")]
    [VisibleIfEqualTo(nameof(TrendingTopicsPageType), "Microsite")]
    public IEnumerable<ContentItemReference> MicrositePages { get; set; } = new List<ContentItemReference>();

    [ContentItemSelectorComponent(contentTypeNames: [SectionSpecialties.CONTENT_TYPE_NAME, SpecialtiesArea.CONTENT_TYPE_NAME, SpecialtiesCategory.CONTENT_TYPE_NAME, SpecialtiesPage.CONTENT_TYPE_NAME], Order = 3, Label = "Pages")]
    [VisibleIfEqualTo(nameof(TrendingTopicsPageType), "Specialty")]
    public IEnumerable<ContentItemReference> SpecialtyPages { get; set; } = new List<ContentItemReference>();

    [ContentItemSelectorComponent(contentTypeNames: [SectionInsightsPage.CONTENT_TYPE_NAME, Insight.CONTENT_TYPE_NAME, InsightCategory.CONTENT_TYPE_NAME], Order = 3, Label = "Pages")]
    [VisibleIfEqualTo(nameof(TrendingTopicsPageType), "Insight")]
    public IEnumerable<ContentItemReference> InsightPages { get; set; } = new List<ContentItemReference>();

    [DropDownComponent(Label = "Redirect to Target Page", Order = 4,
        Options = "_self;Same tab\r\n_blank;New tab")]
    public string LearnMoreButtonURLTarget { get; set; } = "_self";

    [DropDownComponent(Label = "Top margin", Order = 5,
        Options = "0px;None\r\n20px\r\n40px\r\n60px\r\n80px")]
    public string TopMargin { get; set; } = "0px";

    [DropDownComponent(Label = "Bottom margin", Order = 6,
        Options = "0px;None\r\n20px\r\n40px\r\n60px\r\n80px")]
    public string BottomMargin { get; set; } = "0px";

}