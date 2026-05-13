using BDO.GWT.Xperience.Core.LiveSite.Components.FormComponents;

namespace BDO.GWT.GLOBAL.Tests.Components.FormComponents.ReadOnlyText
{
    public class ReadOnlyTextComponentTests
    {
        private ReadOnlyTextComponent _component;

        [SetUp]
        public void Setup()
        {
            _component = new ReadOnlyTextComponent();
        }

        [Test]
        public void Identifier_HasExpectedValue()
        {
            Assert.That(ReadOnlyTextComponent.IDENTIFIER, Is.EqualTo("ReadOnlyTextComponent"));
        }

        [Test]
        public void GetValue_WhenValueNotSet_ReturnsNull()
        {
            Assert.That(_component.GetValue(), Is.Null);
        }

        [Test]
        public void GetValue_WhenValueSet_ReturnsSameValue()
        {
            _component.Value = "read-only";

            Assert.That(_component.GetValue(), Is.EqualTo("read-only"));
        }

        [Test]
        public void SetValue_AssignsToValueProperty()
        {
            _component.SetValue("static-text");

            Assert.That(_component.Value, Is.EqualTo("static-text"));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("plain text")]
        public void SetValue_ThenGetValue_RoundTrips(string? input)
        {
            _component.SetValue(input!);

            Assert.That(_component.GetValue(), Is.EqualTo(input));
        }
    }
}