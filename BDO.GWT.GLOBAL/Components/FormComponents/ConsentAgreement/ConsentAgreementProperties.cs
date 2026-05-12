using CMS.DataEngine;
using Kentico.Forms.Web.Mvc;
using Kentico.Xperience.Admin.Base.FormAnnotations;
using System.ComponentModel.DataAnnotations;

namespace BDO.GWT.Xperience.Core.LiveSite.Components.FormComponents;

public class ConsentAgreementProperties : FormComponentProperties<string>
{
    public ConsentAgreementProperties() : base(FieldDataType.Text, size: 100)
    {
    }

    [TextInputComponent(Label = "Options", Order = 0)]
    public string Options { get; set; }

    [CheckBoxComponent(Label = "Sort items", Order = 10)]
    public bool SortItems { get; set; }

    [TextInputComponent(Label = "Text format", Order = 20)]
    public string TextFormat { get; set; }

    [TextInputComponent(Label = "Value format", Order = 30)]
    public string ValueFormat { get; set; }

    [DropDownComponent(
        Label = "Repeat direction",
        Order = 40,
        Options = "vertical;Vertical\r\nhorizontal;Horizontal"
    )]
    [Required]
    public string RepeatDirection { get; set; } = "vertical";

    [DropDownComponent(
        Label = "Repeat layout",
        Order = 50,
        Options = "Flow;Flow\r\nTable;Table"
    )]
    public string RepeatLayout { get; set; }

    [TextInputComponent(Label = "Repeat columns", Order = 60)]
    public string RepeatColumns { get; set; }

    [TextInputComponent(Label = "Separator", Order = 70)]
    public string Separator { get; set; }

    [TextInputComponent(Label = "Default value", Order = EditingComponentOrder.DEFAULT_VALUE)]
    public override string DefaultValue { get; set; }
}