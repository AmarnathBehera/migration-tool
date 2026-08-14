using BDO.GWT.CustomReportingModule.ReportingApp;

namespace BDO.GWT.GLOBAL.Tests.Modules.Reporting;

public class EditSqlTemplateClientPropertiesTests
{
    [Test]
    public void Has_Tables_Property()
    {
        var prop = typeof(EditSqlTemplateClientProperties).GetProperty("Tables");
        Assert.That(prop, Is.Not.Null);
        Assert.That(prop!.PropertyType, Is.AssignableTo(typeof(IEnumerable<DatabaseTable>)));
    }

    [Test]
    public void Has_Query_Property()
    {
        var prop = typeof(EditSqlTemplateClientProperties).GetProperty("Query");
        Assert.That(prop, Is.Not.Null);
        Assert.That(prop!.PropertyType, Is.EqualTo(typeof(string)));
    }

    [Test]
    public void Has_SavedQueries_Property()
    {
        var prop = typeof(EditSqlTemplateClientProperties).GetProperty("SavedQueries");
        Assert.That(prop, Is.Not.Null);
        Assert.That(prop!.PropertyType, Is.AssignableTo(typeof(IEnumerable<SavedQuery>)));
    }

    [Test]
    public void Has_ReportingChannelSettingId_Property()
    {
        var prop = typeof(EditSqlTemplateClientProperties).GetProperty("ReportingChannelSettingId");
        Assert.That(prop, Is.Not.Null);
        Assert.That(prop!.PropertyType, Is.EqualTo(typeof(int)));
    }
}
