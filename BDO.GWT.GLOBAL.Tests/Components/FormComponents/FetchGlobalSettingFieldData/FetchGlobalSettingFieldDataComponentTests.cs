using System.Runtime.CompilerServices;
using BDO.GWT.GLOBAL.Components.FormComponents.FetchGlobalSettingFieldData;

namespace BDO.GWT.GLOBAL.Tests.Components.FormComponents.FetchGlobalSettingFieldData;

public class FetchGlobalSettingFieldDataComponentTests
{
    [Test]
    public void Identifier_HasExpectedValue()
    {
        Assert.That(FetchGlobalSettingFieldDataComponent.IDENTIFIER, Is.EqualTo("BDO.FetchGlobalSettingFieldData"));
    }

    [Test]
    public void ClientComponentName_HasExpectedValue()
    {
        var component = (FetchGlobalSettingFieldDataComponent)RuntimeHelpers.GetUninitializedObject(
            typeof(FetchGlobalSettingFieldDataComponent));

        Assert.That(component.ClientComponentName, Is.EqualTo("@bdo/custom-components/BDOFetchGlobalSettingFieldData"));
    }
}
