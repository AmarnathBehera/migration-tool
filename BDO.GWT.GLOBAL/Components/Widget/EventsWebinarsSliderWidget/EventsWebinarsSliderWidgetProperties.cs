using BDO.GWT.Core.Constants;
using BDO.GWT.Web.Widgets;
using CMS.ContentEngine;
using Kentico.Forms.Web.Mvc;
using Kentico.PageBuilder.Web.Mvc;
using Kentico.Xperience.Admin.Base.FormAnnotations;
using Kentico.Xperience.Admin.Base.Forms;
using Kentico.Xperience.Admin.Websites.FormAnnotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Xperience.Core.LiveSite.Components.Widgets.EventsWebinarsSliderWidget;

public class EventsWebinarsSliderWidgetProperties : BaseWidgetProperties
{
    [TextInputComponent(Label = "Title", ExplanationText = "Title that will be shown over the widget.", Order = 0)]
    public string Title { get; set; }

    [TextInputComponent(Label = "Description", ExplanationText = "Description that will be shown over the widget but under the title.", Order = 1)]
    public string Description { get; set; }

    [CheckBoxComponent(Label = "Enable filtering", Order = 2)]
    public bool FilteringEnabled { get; set; } = true;

    [GeneralSelectorComponent(
        dataProviderType: typeof(FiltersDataProvider),
        Label = "Filters",
        ExplanationText = "Filters that visitor will be able to filter news by.",
        Order = 3)]
    [VisibleIfTrue(nameof(FilteringEnabled))]
    public List<string> Filters { get; set; } = EventWebinarFilters.AllowedFilters
        .Where(x => x.IsDefault)
        .Select(x => x.ParameterName)
        .ToList();

    [ContentItemSelectorComponent(
        contentTypeNames: [Event.CONTENT_TYPE_NAME, Webinar.CONTENT_TYPE_NAME],
        Label = "Featured events & webinars",
        ExplanationText = "Events & webinars that will be tagged as featured and that will appear in front of other events & webinars.",
        Order = 4)]
    public IEnumerable<ContentItemReference> FeaturedEventsAndWebinars { get; set; } = new List<ContentItemReference>();

    [ContentItemSelectorComponent(ContentAttachment.CONTENT_TYPE_NAME, Label = "Background image", Order = 5)]
    public IEnumerable<ContentItemReference> BackgroundImages { get; set; }

    [TextInputComponent(Label = "Background image alt text", Order = 6)]
    public string BackgroundImageAltText { get; set; } = "";

    [TextInputComponent(Label = "See all link text", Order = 7)]
    public string SeeAllLinkText { get; set; }

    [UrlSelectorComponent(Label = "See all link URL", Order = 8)]
    public string SeeAllLinkURL { get; set; }

    [DropDownComponent(Label = "See all link URL target", Options = "_self;Same tab\r\n_blank;New tab", Order = 9)]
    public string SeeAllLinkURLTarget { get; set; } = "_self";

    [DropDownComponent(Label = "Top margin", Options = "0px;None\r\n20px\r\n40px\r\n60px\r\n80px", Order = 10)]
    public string TopMargin { get; set; } = null!;

    [DropDownComponent(Label = "Bottom margin", Options = "0px;None\r\n20px\r\n40px\r\n60px\r\n80px", Order = 11)]
    public string BottomMargin { get; set; } = null!;


}

public static class EventWebinarFilters
{
    public static readonly (string ParameterName, bool IsDefault)[] AllowedFilters =
    [
        (FilterParameterName.EventType, true),
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
        EventWebinarFilters.AllowedFilters.Select(x => new ObjectSelectorListItem<string>
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