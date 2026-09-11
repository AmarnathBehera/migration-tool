using BDO.GWT.Core.Constants;
using BDO.GWT.Web.Widgets;
using CMS.ContentEngine;
using Kentico.PageBuilder.Web.Mvc;
using Kentico.Xperience.Admin.Base.FormAnnotations;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace BDO.GWT.GLOBAL.Widgets;

public class KeyContactsWidgetProperties : BaseWidgetProperties
{
    [TextInputComponent(Label = "Title", Order = 0)]
    public string Title { get; set; }

    [TextInputComponent(Label = "Description", Order = 1)]
    public string Description { get; set; }

    [ContentItemSelectorComponent(
        Person.CONTENT_TYPE_NAME,
        Label = "Contacts",
        ExplanationText = "List of people. Adding people from the list will display them in the same order as they are added.",
        Order = 2)]
    public IEnumerable<ContentItemReference> People { get; set; } = Enumerable.Empty<ContentItemReference>();

    [ContentItemSelectorComponent(
        contentTypeNames: [ExternalPerson.CONTENT_TYPE_NAME, InternalPerson.CONTENT_TYPE_NAME],        
        Label = "Contributors",
        ExplanationText = "List of contributors. Adding contributors from the list will display them in the same order as they are added.",
        Order = 3)]
    public IEnumerable<ContentItemReference> Contributors { get; set; } = Enumerable.Empty<ContentItemReference>();

    [CheckBoxComponent(Label = "Override contact ordering", ExplanationText = "If checked, contacts will be ordered alphabetically by the selected field.", Order = 4)]
    public bool IsOrderOverriden { get; set; } = false;

    [DropDownComponent(Label = "Order contacts by", Order = 5,
        Options = $"{PeopleOrderCriteria.FirstName};First name\r\n{PeopleOrderCriteria.LastName};Last name\r\n{PeopleOrderCriteria.JobTitle};Job title")]
    [VisibleIfTrue(nameof(IsOrderOverriden))]
    public string OrderCriteria { get; set; }

    [CheckBoxComponent(Label = "Use descending alphabetical order", Order = 6)]
    [VisibleIfTrue(nameof(IsOrderOverriden))]
    public bool IsDescendingOrder { get; set; } = false;

    [CheckBoxComponent(Label = "Show office", ExplanationText = "Location can be shown for individuals from the Our People section.", Order = 7)]
    public bool ShowOffice { get; set; } = false;

    [TextInputComponent(Label = "See all link text", Order = 8)]
    public string SeeAllLinkText { get; set; }

    [TextInputComponent(Label = "See all link URL", Order = 9)]
    public string SeeAllLinkURL { get; set; }

    [DropDownComponent(Label = "See all link URL target", Order = 10, Options = "_self;Same tab\r\n_blank;New tab")]
    public string SeeAllLinkURLTarget { get; set; } = "_self";

    [DropDownComponent(Label = "Top margin", Order = 11, Options = "0px;None\r\n20px\r\n40px\r\n60px\r\n80px")]
    public string TopMargin { get; set; } = "0px";

    [DropDownComponent(Label = "Bottom margin", Order = 12, Options = "0px;None\r\n20px\r\n40px\r\n60px\r\n80px")]
    public string BottomMargin { get; set; } = "0px";


}