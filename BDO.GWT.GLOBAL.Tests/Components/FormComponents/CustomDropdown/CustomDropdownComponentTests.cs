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
            // The Kentico FormComponent<,,> base constructor resolves services from
            // the Xperience IoC container, which is not initialized in unit tests.
            // Create an uninitialized instance so we can assert the overridden
            // ClientComponentName (a literal value) without invoking the base ctor.
            var component = (CustomDropdownComponent)RuntimeHelpers.GetUninitializedObject(
                typeof(CustomDropdownComponent));

            Assert.That(component.ClientComponentName, Is.EqualTo("@bdo/custom-dropdown/BDOSQLDropdown"));
        }
    }
}