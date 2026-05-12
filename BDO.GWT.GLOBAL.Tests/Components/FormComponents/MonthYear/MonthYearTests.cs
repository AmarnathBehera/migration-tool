using BDO.GWT.Xperience.Core.LiveSite.Components.FormComponents;

namespace BDO.GWT.GLOBAL.Tests.Components.FormComponents.MonthYear
{
    public class MonthYearTests
    {
        private BDO.GWT.Xperience.Core.LiveSite.Components.FormComponents.MonthYear _component;

        [SetUp]
        public void Setup()
        {
            _component = new BDO.GWT.Xperience.Core.LiveSite.Components.FormComponents.MonthYear();
        }

        [Test]
        public void Identifier_HasExpectedValue()
        {
            Assert.That(
                BDO.GWT.Xperience.Core.LiveSite.Components.FormComponents.MonthYear.IDENTIFIER,
                Is.EqualTo("BDO.FormComponent.MonthYear"));
        }

        [Test]
        public void GetValue_WhenMonthAndYearUninitialized_ReturnsNull()
        {
            Assert.That(_component.GetValue(), Is.Null);
        }

        [TestCase(0, 2024)]
        [TestCase(13, 2024)]
        [TestCase(-1, 2024)]
        [TestCase(6, 0)]
        [TestCase(6, -5)]
        public void GetValue_WhenMonthOrYearInvalid_ReturnsNull(int month, int year)
        {
            _component.Month = month;
            _component.Year = year;

            Assert.That(_component.GetValue(), Is.Null);
        }

        [TestCase(1, 2024)]
        [TestCase(12, 1999)]
        [TestCase(6, 2026)]
        public void GetValue_WhenMonthAndYearValid_ReturnsFirstDayOfMonth(int month, int year)
        {
            _component.Month = month;
            _component.Year = year;

            var result = _component.GetValue();

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Value, Is.EqualTo(new DateTime(year, month, 1)));
        }

        [Test]
        public void SetValue_WithNull_ResetsMonthAndYearToZero()
        {
            _component.Month = 5;
            _component.Year = 2020;

            _component.SetValue(null);

            Assert.That(_component.Month, Is.EqualTo(0));
            Assert.That(_component.Year, Is.EqualTo(0));
        }

        [Test]
        public void SetValue_WithDate_SetsMonthAndYear()
        {
            _component.SetValue(new DateTime(2026, 5, 12));

            Assert.That(_component.Month, Is.EqualTo(5));
            Assert.That(_component.Year, Is.EqualTo(2026));
        }

        [Test]
        public void SetValue_ThenGetValue_RoundTripsToFirstOfMonth()
        {
            _component.SetValue(new DateTime(2026, 5, 12));

            var result = _component.GetValue();

            Assert.That(result, Is.EqualTo(new DateTime(2026, 5, 1)));
        }
    }
}