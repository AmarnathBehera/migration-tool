using BDO.GWT.CustomReportingModule;
using System.Reflection;

namespace BDO.GWT.GLOBAL.Tests.Modules.Reporting
{
    [TestFixture]
    public class ViewRecordTests
    {
        private Type viewRecordType = null!;

        [OneTimeSetUp]
        public void Setup()
        {
            var assembly = typeof(ReportingApplicationPage).Assembly;

            viewRecordType = assembly.GetTypes()
                .First(t => t.Name == "ViewRecord");
        }

        [Test]
        public void Type_ViewRecord_Exists()
        {
            Assert.That(viewRecordType, Is.Not.Null);
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
                BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.That(method, Is.Not.Null);
        }

        [Test]
        public void GetFormItemsClientProperties_Method_Exists()
        {
            var method = viewRecordType.GetMethod(
                "GetFormItemsClientProperties",
                BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.That(method, Is.Not.Null);
        }

        [Test]
        public void SubmitInternal_Method_Exists()
        {
            var method = viewRecordType.GetMethod(
                "SubmitInternal",
                BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.That(method, Is.Not.Null);
        }
    }
}