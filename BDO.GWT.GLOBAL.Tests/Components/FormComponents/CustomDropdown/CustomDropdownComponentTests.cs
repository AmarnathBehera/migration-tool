using System.Runtime.CompilerServices;
using BDO.GWT.GLOBAL.Components.FormComponents.CustomDropdown;

namespace BDO.GWT.GLOBAL.Tests.Components.FormComponents.CustomDropdown
{
    public class CustomDropdownComponentTests
    {
        [Test]
        public void Identifier_HasExpectedValue()
        {
            Assert.That(CustomDropdownComponent.IDENTIFIER, Is.EqualTo("BDO.SQLDropdown"));
        }

        [Test]
        public void ClientComponentName_HasExpectedValue()
        {
            var component = (CustomDropdownComponent)RuntimeHelpers.GetUninitializedObject(
                typeof(CustomDropdownComponent));

            Assert.That(component.ClientComponentName, Is.EqualTo("@bdo/custom-components/BDOSQLDropdown"));
        }
    }
}