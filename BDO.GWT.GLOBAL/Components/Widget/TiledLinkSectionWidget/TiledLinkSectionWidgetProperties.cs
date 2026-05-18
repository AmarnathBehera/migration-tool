using CMS.ContentEngine;
using Kentico.Components.Web.Mvc.FormComponents;
using Kentico.Forms.Web.Mvc;
using Kentico.PageBuilder.Web.Mvc;
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.FormAnnotations;
using Kentico.Xperience.Admin.Base.Forms;
using Kentico.Xperience.Admin.Websites.FormAnnotations;
using System.Collections.Generic;

namespace BDO.GWT.Xperience.Core.LiveSite.Components.Widgets.TiledLinkSectionWidget;

public class TiledLinkSectionWidgetProperties : IWidgetProperties
{

    [TextInputComponent(Label = "Title", ExplanationText = "Title that will be shown over tiled link section widget.", Order = 0)]
    public string? Title { get; set; }

    [TextInputComponent(Label = "Description", ExplanationText = "Description that will be shown over tiled link section widget but under title.", Order = 1)]
    public string? Description { get; set; }


    [ContentItemSelectorComponent(ContentAttachment.CONTENT_TYPE_NAME, Label = "Background Image", Order = 2)]
    public IEnumerable<ContentItemReference> BackgroundImages { get; set; } = new List<ContentItemReference>();


    [TextInputComponent(Label = "Background image alt text", Order = 3)]
    public string BackgroundImageAltText { get; set; } = "";

    [DropDownComponent(Label = "Tile page type", Order = 4, Options = "Industry;Industry\r\nService;Service")]
    public string TilePageType { get; set; }

    [ContentItemSelectorComponent(new string[] { SectionIndustries.CONTENT_TYPE_NAME, Industry.CONTENT_TYPE_NAME, IndustryCategory.CONTENT_TYPE_NAME, IndustryService.CONTENT_TYPE_NAME }, Label = "Tile pages", Order = 5)]
    [VisibleIfEqualTo(nameof(TilePageType), "Industry")]
    public IEnumerable<ContentItemReference> TilePagesIndustry { get; set; } = new List<ContentItemReference>();
    [ContentItemSelectorComponent(new string[] { SectionServices.CONTENT_TYPE_NAME, ServiceArea.CONTENT_TYPE_NAME, Sub_Service.CONTENT_TYPE_NAME, BusinessLine.CONTENT_TYPE_NAME, Service.CONTENT_TYPE_NAME }, Label = "Tile pages", Order = 6)]

    [VisibleIfEqualTo(nameof(TilePageType), "Service")]
    public IEnumerable<ContentItemReference> TilePagesService { get; set; } = new List<ContentItemReference>();



    [RadioGroupComponent(Label = "Light / Dark theme", Order = 7, Options = "light;Light theme\r\ndark;Dark theme")]
    public string Theme { get; set; } = "light";

    [TextInputComponent(Label = "See all link text", Order = 8)]
    public string? SeeAllLinkText { get; set; }

    [TextInputComponent(Label = "See all link URL", Order = 9)]
    [UrlValidationRule(AllowRelativeUrl = true, AllowFragmentUrl = true)]
    [ExcludeFromAiraTranslation]
    public string? SeeAllLinkURL { get; set; }

    [DropDownComponent(Label = "See all link URL target", Order = 10, Options = "_self;Same tab\n_blank;New tab")]
    public string SeeAllLinkURLTarget { get; set; } = "_self";

    [DropDownComponent(Label = "Top margin", Order = 11, Options = "0px;None\n20px\n40px\n60px\n80px")]
    public string? TopMargin { get; set; }

    [DropDownComponent(Label = "Bottom margin", Order = 12, Options = "0px;None\n20px\n40px\n60px\n80px")]
    public string? BottomMargin { get; set; }

    [TextInputComponent(Label = "Widget ID", Order = 13, ExplanationText = "ID should be unique from any other ID on the page. If the ID contains multiple words, words must be joined with a '-' (e.g. three-word-id). No other special characters and no spaces.'")]

    public string WidgetID { get; set; }
}