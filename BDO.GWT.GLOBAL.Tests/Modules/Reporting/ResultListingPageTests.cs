using BDO.GWT.CustomReportingModule.ReportingApp;
using CMS.Core;
using Kentico.Xperience.Admin.Base;

namespace BDO.GWT.GLOBAL.Tests.Modules.Reporting;

public class ResultListingPageTests
{
    [Test]
    public void Page_InheritsFromDataContainerListingPage()
    {
        Assert.That(typeof(DataContainerListingPage).IsAssignableFrom(typeof(ResultListing)), Is.True);
    }

    [Test]
    public void Constructor_HasFourParameters()
    {
        var ctors = typeof(ResultListing).GetConstructors();
        Assert.That(ctors, Has.Length.EqualTo(1));
        var parameters = ctors.Single().GetParameters();
        Assert.That(parameters, Has.Length.EqualTo(4));
        Assert.Multiple(() =>
        {
            Assert.That(parameters[0].ParameterType, Is.EqualTo(typeof(ISqlBrowserResultProvider)));
            Assert.That(parameters[1].ParameterType, Is.EqualTo(typeof(ISqlBrowserExporter)));
            Assert.That(parameters[2].ParameterType, Is.EqualTo(typeof(IEventLogService)));
            Assert.That(parameters[3].ParameterType, Is.EqualTo(typeof(IPageLinkGenerator)));
        });
    }

    [Test]
    public void ExposesExport_BackToChannel_SubmitQuery_OpenEditQuery()
    {
        var export = typeof(ResultListing).GetMethod("Export");
        var back = typeof(ResultListing).GetMethod("BackToChannel");
        var submit = typeof(ResultListing).GetMethod("SubmitQuery");
        var open = typeof(ResultListing).GetMethod("OpenEditQuery");

        Assert.Multiple(() =>
        {
            Assert.That(export, Is.Not.Null);
            Assert.That(back, Is.Not.Null);
            Assert.That(submit, Is.Not.Null);
            Assert.That(open, Is.Not.Null);
        });

        // Export signature
        Assert.That(export!.ReturnType, Is.EqualTo(typeof(Task<ICommandResponse>)));
    }
}
