using BDO.GWT.GLOBAL.Components.FormComponents.CustomDropdown;

namespace BDO.GWT.GLOBAL.Tests.Components.FormComponents.CustomDropdown
{
    public class CustomDropdownClientPropertiesTests
    {
        [Test]
        public void Options_DefaultsToEmptyCollection()
        {
            var clientProps = new CustomDropdownClientProperties();

            Assert.That(clientProps.Options, Is.Not.Null);
            Assert.That(clientProps.Options, Is.Empty);
        }

        [Test]
        public void Options_CanBeAssigned()
        {
            var clientProps = new CustomDropdownClientProperties
            {
                Options =
                [
                    new CustomDropdownOption { Value = "1", Text = "One" },
                    new CustomDropdownOption { Value = "2", Text = "Two" }
                ]
            };

            Assert.That(clientProps.Options.Count(), Is.EqualTo(2));
        }

        [Test]
        public void FieldLabel_CanBeSet()
        {
            var clientProps = new CustomDropdownClientProperties
            {
                FieldLabel = "My Label"
            };

            Assert.That(clientProps.FieldLabel, Is.EqualTo("My Label"));
        }
    }

    public class CustomDropdownOptionTests
    {
        [Test]
        public void Defaults_AreEmptyStrings()
        {
            var option = new CustomDropdownOption();

            Assert.Multiple(() =>
            {
                Assert.That(option.Value, Is.EqualTo(string.Empty));
                Assert.That(option.Text, Is.EqualTo(string.Empty));
            });
        }

        [Test]
        public void Properties_CanBeSetAndRetrieved()
        {
            var option = new CustomDropdownOption
            {
                Value = "v1",
                Text = "Display"
            };

            Assert.Multiple(() =>
            {
                Assert.That(option.Value, Is.EqualTo("v1"));
                Assert.That(option.Text, Is.EqualTo("Display"));
            });
        }
    }
}
