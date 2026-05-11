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

    public override DateTime? GetValue() => new (Year, Month, 1);

    public override void SetValue(DateTime? value)
    {
        if (value == null)
        {
            return;
        }   

        Month = value.Value.Month;
        Year = value.Value.Year;
    }
}
