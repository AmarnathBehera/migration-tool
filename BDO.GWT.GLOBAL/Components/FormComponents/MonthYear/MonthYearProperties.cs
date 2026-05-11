using CMS.DataEngine;
using Kentico.Forms.Web.Mvc;
using Kentico.Xperience.Admin.Base.FormAnnotations;
using System;

namespace BDO.GWT.Xperience.Core.LiveSite.Components.FormComponents;

public class MonthYearProperties : FormComponentProperties<DateTime?>
{
    public MonthYearProperties() : base(FieldDataType.DateTime, size: 0)
    {
    }

    [CheckBoxComponent(Label = "Edit time", Order = 0)]
    public bool EditTime { get; set; }

    [CheckBoxComponent(Label = "Display now", Order = 10)]
    public bool DisplayNow { get; set; }

    [CheckBoxComponent(Label = "Check range", Order = 20)]
    public bool CheckRange { get; set; }

    [TextInputComponent(Label = "Time zone type", Order = 30)]
    public string? TimeZoneType { get; set; }

    [TextInputComponent(Label = "Time zone", Order = 40)]
    public string? TimeZone { get; set; }

    [TextInputComponent(Label = "Default value", Order = EditingComponentOrder.DEFAULT_VALUE)]
    public override DateTime? DefaultValue { get; set; }
}