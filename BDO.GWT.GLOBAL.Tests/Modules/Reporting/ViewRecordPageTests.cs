using BDO.GWT.CustomReportingModule.ReportingApp;
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.Forms;

namespace BDO.GWT.GLOBAL.Tests.Modules.Reporting;

public class ViewRecordPageTests
{
    [Test]
    public void Page_InheritsFromEditPageBase()
    {
        Assert.That(typeof(EditPageBase).IsAssignableFrom(typeof(ViewRecord)), Is.True);
    }

    [Test]
    public void Constructor_HasTwoParameters()
    {
        var ctors = typeof(ViewRecord).GetConstructors();
        Assert.That(ctors, Has.Length.EqualTo(1));
        var parameters = ctors.Single().GetParameters();
        Assert.That(parameters, Has.Length.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(parameters[0].ParameterType, Is.EqualTo(typeof(IFormDataBinder)));
            Assert.That(parameters[1].ParameterType, Is.EqualTo(typeof(ISqlBrowserResultProvider)));
        });
    }

    [Test]
    public void ExposesConfigureTemplateProperties()
    {
        var method = typeof(ViewRecord).GetMethod(nameof(ViewRecord.ConfigureTemplateProperties));
        Assert.That(method, Is.Not.Null);
        Assert.That(method!.ReturnType, Is.EqualTo(typeof(Task<EditTemplateClientProperties>)));
    }
}
