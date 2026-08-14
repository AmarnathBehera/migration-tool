using BDO.GWT.CustomReportingModule.ReportingApp;
using Kentico.Xperience.Admin.Base.UIPages;
using Kentico.Xperience.Admin.Base;

namespace BDO.GWT.GLOBAL.Tests.Modules.Reporting;

public class ReportingQueryBrowserPageTests
{
    [Test]
    public void Page_InheritsFromApplicationPage()
    {
        Assert.That(typeof(ApplicationPage).IsAssignableFrom(typeof(ReportingQueryBrowserPage)), Is.True);
    }

    [Test]
    public void Page_HasPageParameterProperty()
    {
        var prop = typeof(ReportingQueryBrowserPage).GetProperty("ReportingChannelSettingId");
        Assert.That(prop, Is.Not.Null);
        Assert.That(prop!.PropertyType, Is.EqualTo(typeof(int)));
    }
}
