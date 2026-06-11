using BDO.GWT.Core.Constants;
using CMS.ContentEngine;
using Kentico.Forms.Web.Mvc;
using Kentico.PageBuilder.Web.Mvc;
using Kentico.Xperience.Admin.Base.FormAnnotations;
using Kentico.Xperience.Admin.Base.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.GLOBAL.Widgets.NewsCarouselWidget;

public class NewsCarouselWidgetProperties : IWidgetProperties
{
    [TextInputComponent(Label = "Title", ExplanationText = "Title that will be shown over news carousel widget.", Order = 0)]
    public string? Title { get; set; }

    [TextInputComponent(Label = "Description", ExplanationText = "Description that will be shown over news carousel widget but under title.", Order = 1)]
    public string? Description { get; set; }

    [CheckBoxComponent(Label = "Enable filtering", Order = 2)]
    public bool FilteringEnabled { get; set; } = true;

    [GeneralSelectorComponent(
        dataProviderType: typeof(FiltersDataProvider),
        Label = "Filters",
        ExplanationText = "Filters that visitor will be able to filter news by.",
        Order = 3)]
    [VisibleIfTrue(nameof(FilteringEnabled))]
    public List<string> Filters { get; set; } = NewsCarouselFilters.AllowedFilters.Where(x => x.IsDefault).Select(x => x.ParameterName).ToList();

    [ContentItemSelectorComponent(
        contentTypeNames: [NewsArticle.CONTENT_TYPE_NAME],
        Label = "Featured news",
        ExplanationText = "News that will be tagged as featured and that will appear in front of other news.",
        Order = 4)]
    public IEnumerable<ContentItemReference> FeaturedNews { get; set; } = Enumerable.Empty<ContentItemReference>();

    [ContentItemSelectorComponent(
        ContentAttachment.CONTENT_TYPE_NAME,
        Label = "Background image",
        ExplanationText = "Recommended size depends on usage.",
        Order = 5,
        MaximumItems = 1)]
    public IEnumerable<ContentItemReference> BackgroundImages { get; set; } = Enumerable.Empty<ContentItemReference>();

    [TextInputComponent(Label = "Background image alt text", Order = 6)]
    [VisibleIfNotEmpty(nameof(BackgroundImages))]
    public string BackgroundImageAltText { get; set; } = "";

    [TextInputComponent(Label = "See all link text", Order = 7)]
    public string? SeeAllLinkText { get; set; }

    [TextInputComponent(Label = "See all link URL", Order = 8)]
    public string? SeeAllLinkURL { get; set; }

    [DropDownComponent(Label = "See all link URL target", Order = 9, Options = "_self;Same tab\r\n_blank;New tab")]
    public string SeeAllLinkURLTarget { get; set; } = "_self";

    [DropDownComponent(Label = "Top margin", Order = 10, Options = "0px;None\r\n20px\r\n40px\r\n60px\r\n80px")]
    public string TopMargin { get; set; } = "0px";

    [DropDownComponent(Label = "Bottom margin", Order = 11, Options = "0px;None\r\n20px\r\n40px\r\n60px\r\n80px")]
    public string BottomMargin { get; set; } = "0px";

    [TextInputComponent( Order = 12, ExplanationText = "ID should be unique from any other ID on the page. If the ID contains multiple words, words must be joined with a '-' (e.g. three-word-id). No other special characters and no spaces.'", Label = "Widget ID")]
    [RegularExpression(
       @"^[a-z0-9'-]*$",
       ErrorMessage = "Characters are not allowed.")]
    public string WidgetID { get; set; }
}
public static class NewsCarouselFilters
{
    public static readonly (string ParameterName, bool IsDefault)[] AllowedFilters =
    [
        (FilterParameterName.ContentType,        true),
        (FilterParameterName.BusinessLine,       true),
        (FilterParameterName.ServiceArea,        false),
        (FilterParameterName.Service,            false),
        (FilterParameterName.IndustryCategory,   true),
        (FilterParameterName.IndustryPage,       false),
        (FilterParameterName.MicrositeMasterPage,false),
    ];
}
public class FiltersDataProvider : IGeneralSelectorDataProvider
{
    private static readonly ObjectSelectorListItem<string>[] Items =
        NewsCarouselFilters.AllowedFilters.Select(x => new ObjectSelectorListItem<string>
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