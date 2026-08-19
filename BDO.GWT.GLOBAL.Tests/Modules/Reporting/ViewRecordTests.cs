using BDO.GWT.CustomReportingModule.ReportingApp;
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.Forms;

namespace BDO.GWT.GLOBAL.Tests.Modules.Reporting;

[TestFixture]
public class ViewRecordTests
{
    [Test]
    public void Page_InheritsFromEditPageBase()
    {
        Assert.That(
            typeof(EditPageBase).IsAssignableFrom(typeof(ViewRecord)),
            Is.True);
    }

    [Test]
    public void Constructor_HasExpectedParameters()
    {
        var ctor = typeof(ViewRecord).GetConstructors().Single();

        var parameters = ctor.GetParameters();

        Assert.That(parameters.Length, Is.EqualTo(2));
        Assert.That(parameters[0].ParameterType, Is.EqualTo(typeof(IFormDataBinder)));
        Assert.That(parameters[1].ParameterType, Is.EqualTo(typeof(ISqlBrowserResultProvider)));
    }

    [Test]
    public void ConfigureTemplateProperties_Method_Exists()
    {
        var method = typeof(ViewRecord)
            .GetMethod(nameof(ViewRecord.ConfigureTemplateProperties));

        Assert.That(method, Is.Not.Null);
        Assert.That(
            method!.ReturnType,
            Is.EqualTo(typeof(Task<EditTemplateClientProperties>)));
    }

    [Test]
    public void ConfigurePage_Method_Exists()
    {
        var method = typeof(ViewRecord)
            .GetMethod(nameof(ViewRecord.ConfigurePage));

        Assert.That(method, Is.Not.Null);
        Assert.That(
            method!.ReturnType,
            Is.EqualTo(typeof(Task)));
    }

    [Test]
    public void RecordId_Property_Exists()
    {
        var property = typeof(ViewRecord)
            .GetProperty(nameof(ViewRecord.RecordId));

        Assert.That(property, Is.Not.Null);
        Assert.That(property!.CanRead, Is.True);
        Assert.That(property.CanWrite, Is.True);
    }

    [Test]
    public void ReportingChannelSettingId_Property_Exists()
    {
        var property = typeof(ViewRecord)
            .GetProperty(nameof(ViewRecord.ReportingChannelSettingId));

        Assert.That(property, Is.Not.Null);
        Assert.That(property!.CanRead, Is.True);
        Assert.That(property.CanWrite, Is.True);
    }
}