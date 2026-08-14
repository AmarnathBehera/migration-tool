using BDO.GWT.CustomReportingModule.ReportingApp;
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.UIPages;
using CMS.Core;
using CMS.DataEngine;

namespace BDO.GWT.GLOBAL.Tests.Modules.Reporting;

public class EditQueryPageTests
{
    [Test]
    public void Page_IsPublicNonAbstractClass()
    {
        Assert.Multiple(() =>
        {
            Assert.That(typeof(EditQuery).IsPublic, Is.True);
            Assert.That(typeof(EditQuery).IsAbstract, Is.False);
        });
    }

    [Test]
    public void Page_Constructor_HasFourParameters()
    {
        var ctors = typeof(EditQuery).GetConstructors();
        Assert.That(ctors, Has.Length.EqualTo(1));

        var parameters = ctors.Single().GetParameters();
        Assert.That(parameters, Has.Length.EqualTo(4));
        Assert.That(parameters[0].ParameterType, Is.EqualTo(typeof(IEventLogService)));
        Assert.That(parameters[1].ParameterType, Is.EqualTo(typeof(ISqlBrowserResultProvider)));
        Assert.That(parameters[2].ParameterType, Is.EqualTo(typeof(ISqlBrowserExporter)));
        Assert.That(parameters[3].ParameterType.GetGenericTypeDefinition(), Is.EqualTo(typeof(IInfoProvider<>)));
    }

    [Test]
    public void Page_InheritsFromPage_Templated()
    {
        // Avoid calling GetGenericTypeDefinition on a non-generic type which throws InvalidOperationException
        Assert.That(typeof(Page<>).IsAssignableFrom(typeof(EditQuery)), Is.False);
        // Instead assert that EditQuery inherits from Page<EditSqlTemplateClientProperties>
        Assert.That(typeof(Page<EditSqlTemplateClientProperties>).IsAssignableFrom(typeof(EditQuery)), Is.True);
    }

    [Test]
    public void Page_ExposesConfigureTemplateProperties()
    {
        var method = typeof(EditQuery).GetMethod(nameof(EditQuery.ConfigureTemplateProperties));
        Assert.That(method, Is.Not.Null);
        Assert.That(method!.ReturnType, Is.EqualTo(typeof(Task<EditSqlTemplateClientProperties>)));
    }

    [Test]
    public void Page_ExposesRunSqlAndNotifyCommands()
    {
        var notify = typeof(EditQuery).GetMethod("Notify");
        var runSql = typeof(EditQuery).GetMethod("RunSql");

        Assert.That(notify, Is.Not.Null);
        Assert.That(runSql, Is.Not.Null);

        Assert.That(notify!.ReturnType, Is.EqualTo(typeof(Task<ICommandResponse>)));
        Assert.That(runSql!.ReturnType, Is.EqualTo(typeof(Task<ICommandResponse<SqlBrowserQueryResult>>)));
    }
}
