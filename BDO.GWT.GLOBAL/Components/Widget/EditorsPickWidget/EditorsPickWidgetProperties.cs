
using BDO.GWT.Core.Constants;
using BDO.GWT.Web.Widgets;
using CMS.ContentEngine;
using Kentico.Forms.Web.Mvc;
using Kentico.Xperience.Admin.Base.FormAnnotations;
using Kentico.Xperience.Admin.Base.Forms;
using Kentico.Xperience.Admin.Websites.FormAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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
    public string Title { get; set; }

    [TextInputComponent(Label = "Description", ExplanationText = "Description that will be shown over editor's pick widget but under title.", Order = 1)]
    public string Description { get; set; }

    [CheckBoxComponent(Label = "Enable filtering", Order = 2)]
    public bool FilteringEnabled { get; set; } = true;

    [GeneralSelectorComponent(dataProviderType: typeof(FiltersDataProvider), Label = "Filters", ExplanationText = "Filters that visitor will be able to filter editor's pick content by. Some filter values will not work for some page types.", Order = 3)]
    //[EditingComponentConfiguration(typeof(FiltersConfiguration))]
    [VisibleIfTrue(nameof(FilteringEnabled))]
    public List<string> Filters { get; set; } = EditorsPickFilters
        .Where(x => x.IsDefault)
        .Select(x => x.ParameterName)
        .ToList();

    //NOTE: Modify configuration of the OrderedPageSelector component to allow only specific page types (https://roadmap.kentico.com/c/75-page-selector-allowed-page-types)
    [ContentItemSelectorComponent(contentTypeNames: [BlogPost.CONTENT_TYPE_NAME, Event.CONTENT_TYPE_NAME, MicrositePage.CONTENT_TYPE_NAME, NewsArticle.CONTENT_TYPE_NAME, Webinar.CONTENT_TYPE_NAME,Insight.CONTENT_TYPE_NAME], Label = "Pages", ExplanationText = "Only pages from Blog Posts, Events, Insights, Microsites, News Articles and Webinars will be showcased.\nNote: All content is sorted in descending order - by \"Publish From\" for general content (newest first), and by start date for events (latest upcoming first).", Order = 4)]
    public IEnumerable<ContentItemReference> Pages { get; set; } = Enumerable.Empty<ContentItemReference>();

    //"insight" type missing
    [ContentItemSelectorComponent(contentTypeNames: [BlogPost.CONTENT_TYPE_NAME, Event.CONTENT_TYPE_NAME, MicrositePage.CONTENT_TYPE_NAME, NewsArticle.CONTENT_TYPE_NAME, Webinar.CONTENT_TYPE_NAME,Insight.CONTENT_TYPE_NAME],Label = "Featured pages", ExplanationText = "Only pages from Blog Posts, Events, Insights, Microsites, News Articles and Webinars will be showcased.", Order = 5)]
    public IEnumerable<ContentItemReference> FeaturedPages { get; set; } = Enumerable.Empty<ContentItemReference>();

    [ContentItemSelectorComponent(contentTypeName:ContentAttachment.CONTENT_TYPE_NAME, Label = "Background image", Order = 6,MaximumItems =1)]
    public IEnumerable<ContentItemReference> BackgroundImages { get; set; } = Enumerable.Empty<ContentItemReference>();

    [TextInputComponent( Label = "Background image alt text", Order = 7)]
    [VisibleIfNotEmpty(nameof(BackgroundImages))]
    [RequiredValidationRule]
    public string BackgroundImageAltText { get; set; } = "";

    [TextInputComponent( Label = "See all link text", Order = 8)]
    public string SeeAllLinkText { get; set; }

    [UrlSelectorComponent(Label = "See all link URL", Order = 9)]
    public string SeeAllLinkURL { get; set; }

    [DropDownComponent( Label = "See all link URL target", Order = 10,Options = "_self;Same tab\r\n_blank;New tab")]
    public string SeeAllLinkURLTarget { get; set; } = "_self";

    [DropDownComponent( Label = "Top margin", Order = 11,Options = "0px;None\r\n20px\r\n40px\r\n60px\r\n80px")]
    public string TopMargin { get; set; } = null!;

    [DropDownComponent( Label = "Bottom margin", Order = 12, Options = "0px;None\r\n20px\r\n40px\r\n60px\r\n80px")]
    public string BottomMargin { get; set; } = null!;


    public class FiltersDataProvider : IGeneralSelectorDataProvider
    {
        private static readonly ObjectSelectorListItem<string>[] Items =
            EditorsPickFilters.Select(x => new ObjectSelectorListItem<string>
            {
                Value = x.ParameterName,
                Text = x.ParameterName,
                IsValid = true
            }).ToArray();

        public Task<PagedSelectListItems<string>> GetItemsAsync(
            string searchTerm,
            int pageIndex,
            CancellationToken cancellationToken)
        {
            var results = string.IsNullOrEmpty(searchTerm)
                ? Items
                : Items.Where(x => x.Text.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).ToArray();

            return Task.FromResult(new PagedSelectListItems<string>
            {
                Items = results,
                NextPageAvailable = false
            });
        }

        public Task<IEnumerable<ObjectSelectorListItem<string>>> GetSelectedItemsAsync(
            IEnumerable<string> selectedValues,
            CancellationToken cancellationToken)
        {
            var selected = Items.Where(x => selectedValues.Contains(x.Value));
            return Task.FromResult<IEnumerable<ObjectSelectorListItem<string>>>(selected);
        }
    }
}
