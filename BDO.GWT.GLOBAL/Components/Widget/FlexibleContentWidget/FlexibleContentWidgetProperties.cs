using CMS.ContentEngine;
using Kentico.Xperience.Admin.Base.FormAnnotations;
using System.Collections.Generic;

namespace BDO.GWT.Web.Widgets.FlexibleContentWidget;

public class FlexibleContentWidgetProperties : BaseWidgetProperties
{
    [TextInputComponent(Label = "Title", ExplanationText = "Title that will be shown over flexible content widget.", Order = 0)]
    public string Title { get; set; }

    [TextInputComponent(Label = "Description", ExplanationText = "Description that will be shown over flexible content widget but under title.", Order = 1)]
    public string Description { get; set; }

    [ContentItemSelectorComponent(contentTypeNames: [SectionIndustries.CONTENT_TYPE_NAME,IndustryCategory.CONTENT_TYPE_NAME,IndustryService.CONTENT_TYPE_NAME,Industry.CONTENT_TYPE_NAME,Blog.CONTENT_TYPE_NAME,BlogPost.CONTENT_TYPE_NAME,BlogContributors.CONTENT_TYPE_NAME,MicrositeMasterPage.CONTENT_TYPE_NAME,MicrositePage.CONTENT_TYPE_NAME,Service.CONTENT_TYPE_NAME,BusinessLine.CONTENT_TYPE_NAME,ServiceArea.CONTENT_TYPE_NAME,Sub_Service.CONTENT_TYPE_NAME,SpecialtiesArea.CONTENT_TYPE_NAME,SpecialtiesCategory.CONTENT_TYPE_NAME,SpecialtiesPage.CONTENT_TYPE_NAME], Label = "Pages", ExplanationText = "Only pages from Blogs, Industries, Insights, Microsites, Services and Specialties will be showcased.", Order = 2)]
    public IEnumerable<ContentItemReference> Pages { get; set; } = [];

    [DropDownComponent(Label = "Redirect to Target Page", Order = 3, Options = "_self;Same tab\r\n_blank;New tab")]
    public string LearnMoreButtonURLTarget { get; set; } = "_self";

    [ContentItemSelectorComponent(contentTypeNames: [SectionIndustries.CONTENT_TYPE_NAME, IndustryCategory.CONTENT_TYPE_NAME, IndustryService.CONTENT_TYPE_NAME, Industry.CONTENT_TYPE_NAME, Blog.CONTENT_TYPE_NAME, BlogPost.CONTENT_TYPE_NAME, BlogContributors.CONTENT_TYPE_NAME, MicrositeMasterPage.CONTENT_TYPE_NAME, MicrositePage.CONTENT_TYPE_NAME, Service.CONTENT_TYPE_NAME, BusinessLine.CONTENT_TYPE_NAME, ServiceArea.CONTENT_TYPE_NAME, Sub_Service.CONTENT_TYPE_NAME, SpecialtiesArea.CONTENT_TYPE_NAME, SpecialtiesCategory.CONTENT_TYPE_NAME, SpecialtiesPage.CONTENT_TYPE_NAME], Label = "Featured pages", ExplanationText = "Only pages from Blogs, Industries, Insights, Microsites, Services and Specialties will be showcased.", Order = 4)]
    public IEnumerable<ContentItemReference> FeaturedPages { get; set; } = [];

    [DropDownComponent(Label = "Top margin", Order = 5, Options = "0px;None\r\n20px\r\n40px\r\n60px\r\n80px")]
    public string TopMargin { get; set; } = "0px";

    [DropDownComponent(Label = "Bottom margin", Order = 6, Options = "0px;None\r\n20px\r\n40px\r\n60px\r\n80px")]
    public string BottomMargin { get; set; } = "0px";
}
