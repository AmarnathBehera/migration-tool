using BDO.GWT.CustomReportingModule;
using System.Reflection;

namespace BDO.GWT.GLOBAL.Tests.Modules.Reporting
{
    [TestFixture]
    public class ReportingApplicationChannelSettingsListingPageTests
    {
        private System.Type pageType = null!;

        [OneTimeSetUp]
        public void Setup()
        {
            var asm = typeof(ReportingApplicationPage).Assembly;

            pageType = asm.GetTypes()
                .First(t => t.Name == "ReportingApplicationChannelSettingsListingPage");
        }

        [Test]
        public void Type_Exists()
        {
            Assert.That(pageType, Is.Not.Null);
        }

        [Test]
        public void Inherits_From_ListingPage()
        {
            Assert.That(
                pageType.BaseType?.Name,
                Is.EqualTo("ListingPage"));
        }

        [Test]
        public void Has_EnsureSettingsListData_Method()
        {
            var method = pageType.GetMethod(
                "EnsureSettingsListData",
                BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.That(method, Is.Not.Null);
        }

        [Test]
        public void Has_EnsureChannelSetting_Method()
        {
            var method = pageType.GetMethod(
                "EnsureChannelSetting",
                BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.That(method, Is.Not.Null);
        }

        [Test]
        public void Has_EnsureChannelSettingDisplayName_Method()
        {
            var method = pageType.GetMethod(
                "EnsureChannelSettingDisplayName",
                BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.That(method, Is.Not.Null);
        }

        [Test]
        public void Has_ConfigurePage_Method()
        {
            var method = pageType.GetMethod(
                "ConfigurePage",
                BindingFlags.Instance | BindingFlags.Public);

            Assert.That(method, Is.Not.Null);
        }

        [Test]
        public void Constructor_Has_Three_Parameters()
        {
            var ctor = pageType.GetConstructors().Single();

            Assert.That(
                ctor.GetParameters().Length,
                Is.EqualTo(3));
        }
    }
}