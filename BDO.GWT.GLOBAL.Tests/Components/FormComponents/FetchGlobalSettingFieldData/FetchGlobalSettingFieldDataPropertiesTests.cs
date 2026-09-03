using BDO.GWT.GLOBAL.Components.FormComponents.FetchGlobalSettingFieldData;

namespace BDO.GWT.GLOBAL.Tests.Components.FormComponents.FetchGlobalSettingFieldData;

public class FetchGlobalSettingFieldDataPropertiesTests
{
    [Test]
    public void FieldName_DefaultsToEmptyString()
    {
        var properties = new FetchGlobalSettingFieldDataProperties();

        Assert.That(properties.FieldName, Is.EqualTo(string.Empty));
    }

    [Test]
    public void FieldName_CanBeSet()
    {
        var properties = new FetchGlobalSettingFieldDataProperties
        {
            FieldName = "GlobalLocationsDescription"
        };

        Assert.That(properties.FieldName, Is.EqualTo("GlobalLocationsDescription"));
    }
}
