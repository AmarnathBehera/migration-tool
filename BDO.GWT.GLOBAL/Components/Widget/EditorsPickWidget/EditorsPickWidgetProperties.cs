
using BDO.GWT.Core.Constants;
using BDO.GWT.Core.Utilities;
using BDO.GWT.Web.Widgets;
using CMS.ContentEngine;
using Kentico.Components.Web.Mvc.FormComponents;
using Kentico.Content.Web.Mvc;
using Kentico.Forms.Web.Mvc;
using Kentico.Xperience.Admin.Base.FormAnnotations;
using Kentico.Xperience.Admin.Base.Forms;
using Kentico.Xperience.Admin.Websites.FormAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using XperienceCommunity.MultiChoiceFormControl.MultiChoiceFormComponent;

namespace BDO.GWT.Xperience.Core.LiveSite.Components.Widgets.EditorsPickWidget;

public class EditorsPickWidgetProperties : BaseWidgetProperties
{
    private static readonly (string ParameterName, bool IsDefault)[] EditorsPickFilters =
    [
        (FilterParameterName.ContentType, true),
        (FilterParameterName.BusinessLine, true),
        (FilterParameterName.ServiceArea, false),
        (FilterParameterName.IndustryCategory, true),
    ];

    [TextInputComponent(Label = "Title", ExplanationText = "Title that will be shown over editor's pick widget.", Order = 0)]
    public string? Title { get; set; }

    [TextInputComponent(Label = "Description", ExplanationText = "Description that will be shown over editor's pick widget but under title.", Order = 1)]
    public string? Description { get; set; }

    [CheckBoxComponent(Label = "Enable filtering", Order = 2)]
    public bool FilteringEnabled { get; set; } = true;

    //[MultipleChoiceComponent(Label = "Filters", ExplanationText = "Filters that visitor will be able to filter editor's pick content by. Some filter values will not work for some page types.", Order = 3)]
    ////[EditingComponentConfiguration(typeof(FiltersConfiguration))]
    //[VisibleIfTrue(nameof(FilteringEnabled))]
    public string? Filters { get; set; } = string.Join(';', EditorsPickFilters.Where(x => x.IsDefault).Select(x => x.ParameterName));

    //NOTE: Modify configuration of the OrderedPageSelector component to allow only specific page types (https://roadmap.kentico.com/c/75-page-selector-allowed-page-types)
    [ContentItemSelectorComponent(contentTypeNames: ["blogPost", "event", "insight", "microsite", "newsArticle", "webinar"], Label = "Pages", ExplanationText = "Only pages from Blog Posts, Events, Insights, Microsites, News Articles and Webinars will be showcased.\nNote: All content is sorted in descending order - by \"Publish From\" for general content (newest first), and by start date for events (latest upcoming first).", Order = 4)]
    public IEnumerable<ContentItemReference> Pages { get; set; } = Enumerable.Empty<ContentItemReference>();

    [ContentItemSelectorComponent(contentTypeNames: [],Label = "Featured pages", ExplanationText = "Only pages from Blog Posts, Events, Insights, Microsites, News Articles and Webinars will be showcased.", Order = 5)]
    public IEnumerable<ContentItemReference> FeaturedPages { get; set; } = Enumerable.Empty<ContentItemReference>();

    [ContentItemSelectorComponent(contentTypeName:ContentAttachment.CONTENT_TYPE_NAME, Label = "Background image", Order = 6)]
    public IEnumerable<ContentItemReference> BackgroundImages { get; set; } = Enumerable.Empty<ContentItemReference>();

    [TextInputComponent( Label = "Background image alt text", Order = 7)]
    [VisibleIfNotEmpty(nameof(BackgroundImages))]
    public string BackgroundImageAltText { get; set; } = "";

    [TextInputComponent( Label = "See all link text", Order = 8)]
    public string? SeeAllLinkText { get; set; }

    [UrlSelectorComponent(Label = "See all link URL", Order = 9)]
    public string? SeeAllLinkURL { get; set; }

    [DropDownComponent( Label = "See all link URL target", Order = 10,Options = "_self;Same tab\r\n_blank;New tab")]
    public string SeeAllLinkURLTarget { get; set; } = "_self";

    [DropDownComponent( Label = "Top margin", Order = 11,Options = "0px;None\r\n20px\r\n40px\r\n60px\r\n80px")]
    public string TopMargin { get; set; } = null!;

    [DropDownComponent( Label = "Bottom margin", Order = 12, Options = "0px;None\r\n20px\r\n40px\r\n60px\r\n80px")]
    public string BottomMargin { get; set; } = null!;

#pragma warning disable CA1812 // Avoid uninstantiated internal classes - this class is instantiated by Kentico
//    private class FiltersConfiguration : FormComponentConfigurator<MultipleChoiceComponent>
//#pragma warning restore CA1812 // Avoid uninstantiated internal classes - this class is instantiated by Kentico
//    {
//        public override void Configure(MultipleChoiceComponent formComponent, IFormFieldValueProvider formFieldValueProvider)
//        {
//            var sourceValues = EditorsPickFilters.Select(x =>
//            {
//                var filterParameterData = FilterUtility.GetFilterParameterData(x.ParameterName) ?? throw new NotSupportedException($"Filter parameter '{x.ParameterName}' is not supported.");

//                var sourceValue = $"{x.ParameterName};{filterParameterData.AdminDisplayName}";

//                return sourceValue;
//            });

//            formComponent.Properties.DataSource = string.Join(Environment.NewLine, sourceValues);
//            if (formFieldValueProvider.TryGet<bool>(nameof(FilteringEnabled), out var filteringEnabled) && filteringEnabled)
//            {
//                formComponent.Properties.Required = true;
//            }
//        }
    //}
}
