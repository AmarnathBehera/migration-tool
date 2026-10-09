
using BDO.GWT.Core.Constants;
using BDO.GWT.Web.Widgets;
using CMS.ContentEngine;

using Kentico.Forms.Web.Mvc;
using Kentico.Xperience.Admin.Base.FormAnnotations;
using Kentico.Xperience.Admin.Base.Forms;
using Kentico.Xperience.Admin.Websites.FormAnnotations;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace BDO.GWT.Core.Components.Widgets.RelatedContentCarouselWidget;

public class RelatedContentCarouselWidgetProperties : BaseWidgetProperties
{
    private static readonly (string ParameterName, string AdminDisplayName, bool IsDefault)[] RelatedContentFilters =
    [
        (FilterParameterName.ContentType,"Content Type", true),
        (FilterParameterName.BusinessLine, "Business Line", true),
        (FilterParameterName.ServiceArea, "Service Area", false),
        (FilterParameterName.IndustryCategory, "Industry Category", true),
    ];

    private static readonly (string ParameterName, string AdminDisplayName, bool IsDefault)[] AvailablePageTypes =
    [
        (typeof(BlogPost).Name, "Blog Post", true),
        (typeof(Event).Name, "Event", true),
        (typeof(Insight).Name, "Insight", true),
        (typeof(MicrositePage).Name, "Microsite Page", true),
        (typeof(NewsArticle).Name, "News Article", true),
        (typeof(Webinar).Name, "Webinar", true),
    ];

    [TextInputComponent(Label = "Title", ExplanationText = "Title that will be shown over related content carousel widget.", Order = 0)]
    public string Title { get; set; }

    [TextInputComponent(Label = "Description", ExplanationText = "Description that will be shown over related content carousel widget but under title.", Order = 1)]
    public string Description { get; set; }

    [CheckBoxComponent(Label = "Enable filtering", Order = 2)]
    public bool FilteringEnabled { get; set; } = true;

    [GeneralSelectorComponent(dataProviderType: typeof(FiltersDataProvider), Label = "Filters", ExplanationText = "Filters that visitor will be able to filter related content by. Some filter values will not work for some page types.", Order = 3)]
    [VisibleIfTrue(nameof(FilteringEnabled))]
    // ObjectCreationHandling.Replace prevents the stored values from being appended to the
    // default list below, which would otherwise duplicate every entry.
    [JsonProperty(ObjectCreationHandling = ObjectCreationHandling.Replace)]
    public List<string> Filters { get; set; } = RelatedContentFilters
        .Where(x => x.IsDefault)
        .Select(x => x.ParameterName)
        .ToList();

    [GeneralSelectorComponent(dataProviderType: typeof(SelectedPageTypesConfiguration), Label = "Related content page types", ExplanationText = "Note: All content is sorted in descending order - by \"Publish From\" for general content (newest first), and by start date for events (latest upcoming first).", Order = 4)]
    [JsonProperty(ObjectCreationHandling = ObjectCreationHandling.Replace)]
    public List<string> SelectedPageTypes { get; set; } = AvailablePageTypes
        .Where(x => x.IsDefault)
        .Select(x => x.ParameterName)
        .ToList();

    [ContentItemSelectorComponent(contentTypeNames: [BlogPost.CONTENT_TYPE_NAME, Event.CONTENT_TYPE_NAME, Insight.CONTENT_TYPE_NAME, MicrositePage.CONTENT_TYPE_NAME, NewsArticle.CONTENT_TYPE_NAME, Webinar.CONTENT_TYPE_NAME], Label = "Featured related content", ExplanationText = "Related content that will be tagged as featured and that will appear in front of other related content.", Order = 5)]
    public IEnumerable<ContentItemReference> FeaturedPages { get; set; }

    [ContentItemSelectorComponent(contentTypeName: ContentAttachment.CONTENT_TYPE_NAME, Label = "Background image", Order = 6,MaximumItems = 1)]
    public IEnumerable<ContentItemReference> BackgroundImages { get; set; } = Enumerable.Empty<ContentItemReference>();

    [TextInputComponent(Label = "Background image alt text", Order = 7)]
    [VisibleIfNotEmpty(nameof(BackgroundImages))]
    [RequiredValidationRule]
    public string BackgroundImageAltText { get; set; } = "";

    [TextInputComponent(Label = "See all link text", Order = 8)]
    public string SeeAllLinkText { get; set; }

    [UrlSelectorComponent(Label = "See all link URL", Order = 9)]
    public string SeeAllLinkURL { get; set; }

    [DropDownComponent(Label = "See all link URL target", Order = 10, Options = "_self;Same tab\r\n_blank;New tab")]
    public string SeeAllLinkURLTarget { get; set; } = "_self";

    [DropDownComponent(Label = "Top margin", Order = 11, Options = "0px;None\r\n20px\r\n40px\r\n60px\r\n80px")]
    public string TopMargin { get; set; } = null!;

    [DropDownComponent(Label = "Bottom margin", Order = 12, Options = "0px;None\r\n20px\r\n40px\r\n60px\r\n80px")]
    public string BottomMargin { get; set; } = null!;


    public class FiltersDataProvider : Kentico.Xperience.Admin.Base.FormAnnotations.IGeneralSelectorDataProvider
    {
        private static readonly ObjectSelectorListItem<string>[] Items =
            RelatedContentFilters.Select(x => new ObjectSelectorListItem<string>
            {
                Value = x.ParameterName,
                Text = x.AdminDisplayName,
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
            if (selectedValues == null)
            {
                return Task.FromResult<IEnumerable<ObjectSelectorListItem<string>>>(Array.Empty<ObjectSelectorListItem<string>>());
            }

            var selected = Items.Where(x => selectedValues.Contains(x.Value));
            return Task.FromResult<IEnumerable<ObjectSelectorListItem<string>>>(selected);
        }
    }




    public class SelectedPageTypesConfiguration : Kentico.Xperience.Admin.Base.FormAnnotations.IGeneralSelectorDataProvider
    {
        private static readonly ObjectSelectorListItem<string>[] Items =
            AvailablePageTypes.Select(x => new ObjectSelectorListItem<string>
            {
                Value = x.ParameterName,
                Text = x.AdminDisplayName,
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
            if (selectedValues == null)
            {
                return Task.FromResult<IEnumerable<ObjectSelectorListItem<string>>>(Array.Empty<ObjectSelectorListItem<string>>());
            }

            var selected = Items.Where(x => selectedValues.Contains(x.Value));
            return Task.FromResult<IEnumerable<ObjectSelectorListItem<string>>>(selected);
        }
    }
}






