using Kentico.Forms.Web.Mvc;
using System;

namespace BDO.GWT.Xperience.Core.LiveSite.Components.FormComponents;

public class MonthYear : FormComponent<MonthYearProperties, DateTime?>
{
    public const string IDENTIFIER = "BDO.FormComponent.MonthYear";

    [BindableProperty]
    public int Month { get; set; }

    [BindableProperty]
    public int Year { get; set; }

    public override DateTime? GetValue()
    {
        // ✅ Guard: return null if Month or Year are uninitialized/invalid
        if (Month < 1 || Month > 12 || Year < 1)
        {
            return null;
        }

        return new DateTime(Year, Month, 1);
    }

    public override void SetValue(DateTime? value)
    {
        if (value == null)
        {
            // ✅ Reset to safe defaults when null
            Month = 0;
            Year = 0;
            return;
        }

        Month = value.Value.Month;
        Year = value.Value.Year;
    }
}