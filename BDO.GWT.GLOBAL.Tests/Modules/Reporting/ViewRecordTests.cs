using BDO.GWT.CustomReportingModule;
using System.Reflection;

namespace BDO.GWT.GLOBAL.Tests.Modules.Reporting
{
    [TestFixture]
    public class ViewRecordTests
    {
        private System.Type viewRecordType = null!;

        [OneTimeSetUp]
        public void Setup()
        {
            var asm = typeof(ReportingApplicationPage).Assembly;

            viewRecordType = asm.GetTypes()
                .First(x => x.Name == "ViewRecord");
        }

        [Test]
        public void Type_ViewRecord_Exists()
        {
            Assert.That(viewRecordType, Is.Not.Null);
        }

        [Test]
        public void ViewRecord_Inherits_From_EditPageBase()
        {
            Assert.That(
                viewRecordType.BaseType?.Name,
                Is.EqualTo("EditPageBase"));
        }

        [Test]
        public void ReportingChannelSettingId_Property_Exists()
        {
            var property = viewRecordType.GetProperty("ReportingChannelSettingId");

            Assert.That(property, Is.Not.Null);
            Assert.That(property!.PropertyType, Is.EqualTo(typeof(int)));
        }

        [Test]
        public void RecordId_Property_Exists()
        {
            var property = viewRecordType.GetProperty("RecordId");

            Assert.That(property, Is.Not.Null);
            Assert.That(property!.PropertyType, Is.EqualTo(typeof(int)));
        }

        [Test]
        public void ReportingChannelSettingId_Has_PageParameter_Attribute()
        {
            var property = viewRecordType.GetProperty("ReportingChannelSettingId");

            var attributes = property!.GetCustomAttributes(false);

            Assert.That(
                attributes.Any(a => a.GetType().Name == "PageParameterAttribute"),
                Is.True);
        }

        [Test]
        public void RecordId_Has_PageParameter_Attribute()
        {
            var property = viewRecordType.GetProperty("RecordId");

            var attributes = property!.GetCustomAttributes(false);

            Assert.That(
                attributes.Any(a => a.GetType().Name == "PageParameterAttribute"),
                Is.True);
        }

        [Test]
        public void ConfigurePage_Method_Exists()
        {
            var method = viewRecordType.GetMethod("ConfigurePage");

            Assert.That(method, Is.Not.Null);
        }

        [Test]
        public void ConfigureTemplateProperties_Method_Exists()
        {
            var method = viewRecordType.GetMethod("ConfigureTemplateProperties");

            Assert.That(method, Is.Not.Null);
        }

        [Test]
        public void GetFormItems_Method_Exists()
        {
            var method = viewRecordType.GetMethod(
                "GetFormItems",
                BindingFlags.NonPublic | BindingFlags.Instance);

            Assert.That(method, Is.Not.Null);
        }

        [Test]
        public void GetFormItemsClientProperties_Method_Exists()
        {
            var method = viewRecordType.GetMethod(
                "GetFormItemsClientProperties",
                BindingFlags.NonPublic | BindingFlags.Instance);

            Assert.That(method, Is.Not.Null);
        }

        [Test]
        public void SubmitInternal_Method_Exists()
        {
            var method = viewRecordType.GetMethod(
                "SubmitInternal",
                BindingFlags.NonPublic | BindingFlags.Instance);

            Assert.That(method, Is.Not.Null);
        }

        [Test]
        public void Class_Contains_EolReplacement_Field()
        {
            var field = viewRecordType.GetField(
                "EOL_REPLACEMENT",
                BindingFlags.NonPublic | BindingFlags.Static);

            Assert.That(field, Is.Not.Null);
        }

        [Test]
        public void Class_Contains_NewLineRegex_Field()
        {
            var field = viewRecordType.GetField(
                "newLineRegex",
                BindingFlags.NonPublic | BindingFlags.Instance);

            Assert.That(field, Is.Not.Null);
        }
    }
}