using BDO.GWT.GLOBAL.Components.FormComponents.CustomDropdown;
using Kentico.Xperience.Admin.Base.Forms;

namespace BDO.GWT.GLOBAL.Tests.Components.FormComponents.CustomDropdown
{
    public class CustomDropdownPropertiesTests
    {
        [Test]
        public void Properties_InheritsFrom_FormComponentProperties()
        {
            Assert.That(typeof(FormComponentProperties).IsAssignableFrom(typeof(CustomDropdownProperties)), Is.True);
        }

        [Test]
        public void Script_DefaultsToEmptyString()
        {
            var props = new CustomDropdownProperties();

            Assert.That(props.Script, Is.EqualTo(string.Empty));
        }

        [Test]
        public void Script_CanBeSetAndRetrieved()
        {
            var props = new CustomDropdownProperties
            {
                Script = "SELECT Id, Name FROM Table"
            };

            Assert.That(props.Script, Is.EqualTo("SELECT Id, Name FROM Table"));
        }
    }
}
