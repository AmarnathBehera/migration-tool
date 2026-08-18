using BDO.GWT.CustomReportingModule;
using BDO.GWT.CustomReportingModule.ReportingApp;
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
                .First(x => x.Name == "ReportingApplicationChannelSettingsListingPage");
        }

        [Test]
        public void Type_ReportingApplicationChannelSettingsListingPage_Exists()
        {
            Assert.That(pageType, Is.Not.Null);
        }

        [Test]
        public void EnsureSettingsListData_Method_Exists()
        {
            var method = pageType.GetMethod(
                "EnsureSettingsListData",
                BindingFlags.NonPublic | BindingFlags.Instance);

            Assert.That(method, Is.Not.Null);
        }

        [Test]
        public void EnsureChannelSetting_Method_Exists()
        {
            var method = pageType.GetMethod(
                "EnsureChannelSetting",
                BindingFlags.NonPublic | BindingFlags.Instance);

            Assert.That(method, Is.Not.Null);
        }

        [Test]
        public void EnsureChannelSettingDisplayName_Method_Exists()
        {
            var method = pageType.GetMethod(
                "EnsureChannelSettingDisplayName",
                BindingFlags.NonPublic | BindingFlags.Instance);

            Assert.That(method, Is.Not.Null);
        }

        [Test]
        public void ConfigurePage_Method_Exists()
        {
            var method = pageType.GetMethod("ConfigurePage");

            Assert.That(method, Is.Not.Null);
        }

        [Test]
        public void Class_Inherits_From_ListingPage()
        {
            Assert.That(
                pageType.BaseType?.Name,
                Is.EqualTo("ListingPage"));
        }

        [Test]
        public void Class_Has_Three_Constructor_Parameters()
        {
            var ctor = pageType.GetConstructors().Single();

            Assert.That(ctor.GetParameters().Length, Is.EqualTo(3));
        }

        [Test]
        public void ObjectType_Returns_ReportingChannelSettingInfo_ObjectType()
        {
            var page = (ReportingApplicationChannelSettingsListingPage)
                System.Runtime.Serialization.FormatterServices
                    .GetUninitializedObject(typeof(ReportingApplicationChannelSettingsListingPage));

            var property = typeof(ReportingApplicationChannelSettingsListingPage)
                .GetProperty(
                    "ObjectType",
                    BindingFlags.Instance |
                    BindingFlags.NonPublic);

            Assert.That(property, Is.Not.Null);

            var result = property!.GetValue(page);

            Assert.That(
                result,
                Is.EqualTo(ReportingChannelSettingInfo.OBJECT_TYPE));
        }

    }
}