using BDO.GWT.GLOBAL.Components.FormComponents.CustomDropdown;
using Kentico.Xperience.Admin.Base.FormAnnotations;

namespace BDO.GWT.GLOBAL.Tests.Components.FormComponents.CustomDropdown
{
    public class CustomDropdownComponentAttributeTests
    {
        [Test]
        public void Attribute_InheritsFrom_FormComponentAttribute()
        {
            Assert.That(
                typeof(FormComponentAttribute).IsAssignableFrom(typeof(CustomDropdownComponentAttribute)),
                Is.True);
        }

        [Test]
        public void Attribute_CanBeInstantiated()
        {
            var attribute = new CustomDropdownComponentAttribute();

            Assert.That(attribute, Is.Not.Null);
            Assert.That(attribute, Is.InstanceOf<FormComponentAttribute>());
        }
    }
}
