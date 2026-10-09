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

namespace BDO.GWT.GLOBAL.Components.Widget.InsightCarouselWidget;

public class InsightCarouselWidgetProperties : BaseWidgetProperties
{
    private const string MarginOptions = "0px;None\r\n20px\r\n40px\r\n60px\r\n80px";

    [TextInputComponent(Label = "Title", ExplanationText = "Title that will be shown over insight carousel widget.", Order = 0)]
    public string Title { get; set; } = string.Empty;

    [TextInputComponent(Label = "Description", ExplanationText = "Description that will be shown over insight carousel widget but under title.", Order = 1)]
    public string Description { get; set; } = string.Empty;

    [CheckBoxComponent(Label = "Enable filtering", Order = 2)]
    public bool FilteringEnabled { get; set; } = true;

    [GeneralSelectorComponent(dataProviderType: typeof(FiltersDataProvider), Label = "Filters", ExplanationText = "Filters that visitor will be able to filter insights by.", Order = 3)]
    [VisibleIfTrue(nameof(FilteringEnabled))]
    public List<string> Filters { get; set; } = InsightCarouselWidgetFilter.AllowedFilters
        .Where(x => x.IsDefault)
        .Select(x => x.ParameterName)
        .ToList();

    [ContentItemSelectorComponent(contentTypeNames: [Insight.CONTENT_TYPE_NAME], Label = "Featured insights", ExplanationText = "Insights that will be tagged as featured and that will appear in front of other insights.", Order = 4)]
    public IEnumerable<ContentItemReference> FeaturedInsights { get; set; } = Enumerable.Empty<ContentItemReference>();

    [ContentItemSelectorComponent(ContentAttachment.CONTENT_TYPE_NAME, Label = "Background image", ExplanationText = "Recommended size depends on usage.", Order = 5, MaximumItems = 1)]
    public IEnumerable<ContentItemReference> BackgroundImages { get; set; } = Enumerable.Empty<ContentItemReference>();

    [TextInputComponent(Label = "Background image alt text", Order = 6)]
    [VisibleIfNotEmpty(nameof(BackgroundImages))]
    [RequiredValidationRule]
    public string BackgroundImageAltText { get; set; } = string.Empty;

    [TextInputComponent(Label = "See all link text", Order = 7)]
    public string SeeAllLinkText { get; set; } = string.Empty;

    [UrlSelectorComponent(Label = "See all link URL", Order = 8)]
    public string SeeAllLinkURL { get; set; } = string.Empty;

    [DropDownComponent(Label = "See all link URL target", Order = 9, Options = "_self;Same tab\r\n_blank;New tab")]
    public string SeeAllLinkURLTarget { get; set; } = "_self";

    [DropDownComponent(Label = "Top margin", Order = 10, Options = MarginOptions)]
    public string TopMargin { get; set; } = "0px";

    [DropDownComponent(Label = "Bottom margin", Order = 11, Options = MarginOptions)]
    public string BottomMargin { get; set; } = "0px";
}

public static class InsightCarouselWidgetFilter
{
    public static readonly (string ParameterName, bool IsDefault)[] AllowedFilters =
    [
        (FilterParameterName.ContentType, true),
        (FilterParameterName.BusinessIssue, true),
        (FilterParameterName.BusinessLine, true),
        (FilterParameterName.ServiceArea, false),
        (FilterParameterName.IndustryCategory, true),
        (FilterParameterName.SpecialtyCategory, false),
        (FilterParameterName.MicrositeMasterPage, false),
        (FilterParameterName.OfficeLocation, false),
    ];
}

public class FiltersDataProvider : IGeneralSelectorDataProvider
{
    private static readonly ObjectSelectorListItem<string>[] Items =
        InsightCarouselWidgetFilter.AllowedFilters.Select(x => new ObjectSelectorListItem<string>
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