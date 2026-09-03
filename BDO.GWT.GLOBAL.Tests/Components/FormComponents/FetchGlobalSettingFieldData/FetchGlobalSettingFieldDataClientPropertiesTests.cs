using BDO.GWT.GLOBAL.Components.FormComponents.FetchGlobalSettingFieldData;

namespace BDO.GWT.GLOBAL.Tests.Components.FormComponents.FetchGlobalSettingFieldData;

public class FetchGlobalSettingFieldDataClientPropertiesTests
{
    [Test]
    public void FieldLabel_DefaultsToEmptyString()
    {
        var clientProps = new FetchGlobalSettingFieldDataClientProperties();

        Assert.That(clientProps.FieldLabel, Is.EqualTo(string.Empty));
    }

    [Test]
    public void FieldLabel_CanBeSet()
    {
        var clientProps = new FetchGlobalSettingFieldDataClientProperties
        {
            FieldLabel = "Global locations title"
        };

        Assert.That(clientProps.FieldLabel, Is.EqualTo("Global locations title"));
    }
}
