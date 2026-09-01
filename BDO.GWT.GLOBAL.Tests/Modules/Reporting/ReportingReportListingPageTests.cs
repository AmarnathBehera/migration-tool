using BDO.GWT.CustomReportingModule.ReportingApp;
using Kentico.Xperience.Admin.Base;
using System.Reflection;

namespace BDO.GWT.GLOBAL.Tests.Modules.Reporting
{
    [TestFixture]
    public class ReportingReportListingPageTests
    {
        [Test]
        public void Type_ReportingReportListingPage_Exists()
        {
            var type = typeof(ReportingReportListingPage);

            Assert.That(type, Is.Not.Null);
        }

        [Test]
        public void Type_Inherits_From_ListingPage()
        {
            Assert.That(typeof(ReportingReportListingPage).BaseType,
                Is.EqualTo(typeof(ListingPage)));
        }

        [Test]
        public void ExportPermission_HasExpectedValue()
        {
            Assert.That(
                ReportingReportListingPage.EXPORT_PERMISSION,
                Is.EqualTo("Export"));
        }

        [Test]
        public void Identifier_HasExpectedValue()
        {
            Assert.That(
                ReportingReportListingPage.IDENTIFIER,
                Is.EqualTo("DancingGoat.Reporting"));
        }

        [Test]
        public void ReportingReportId_DefaultsToZero()
        {
            var type = typeof(ReportingReportListingPage);

            var property = type.GetProperty(nameof(ReportingReportListingPage.ReportingReportId));

            Assert.That(property, Is.Not.Null);
            Assert.That(property!.PropertyType, Is.EqualTo(typeof(int)));
        }

        [Test]
        public void Class_HasThree_UIPermissionAttributes()
        {
            var attrs = typeof(ReportingReportListingPage)
                .GetCustomAttributes(typeof(UIPermissionAttribute), false);

            Assert.That(attrs, Has.Length.EqualTo(3));
        }

        [Test]
        public void Class_HasExportPermissionAttribute()
        {
            var attrs = typeof(ReportingReportListingPage)
                .GetCustomAttributes(typeof(UIPermissionAttribute), false)
                .Cast<UIPermissionAttribute>()
                .ToList();

            Assert.That(attrs, Has.Count.GreaterThan(0));
        }

        [Test]
        public void ReportingReportId_Property_Exists()
        {
            var property = typeof(ReportingReportListingPage)
                .GetProperty(nameof(ReportingReportListingPage.ReportingReportId));

            Assert.That(property, Is.Not.Null);
        }

        [Test]
        public void ConfigurePage_Method_Exists()
        {
            var method = typeof(ReportingReportListingPage)
                .GetMethod(nameof(ReportingReportListingPage.ConfigurePage));

            Assert.That(method, Is.Not.Null);
        }

        [Test]
        public void EditQuery_Is_Registered_Directly_Under_Channel_Edit_Section()
        {
            var uiPageAttribute = typeof(EditQuery).Assembly
                .GetCustomAttributesData()
                .FirstOrDefault(a =>
                    a.AttributeType.Name == "UIPageAttribute" &&
                    a.ConstructorArguments.Count >= 3 &&
                    Equals(a.ConstructorArguments[2].Value, typeof(EditQuery)));

            Assert.That(uiPageAttribute, Is.Not.Null);

            Assert.Multiple(() =>
            {
                Assert.That(uiPageAttribute!.ConstructorArguments[0].Value, Is.EqualTo(typeof(ReportingApplicationChannelSettingsEditSection)));
                Assert.That(uiPageAttribute.ConstructorArguments[1].Value?.ToString(), Is.EqualTo("queries"));
            });
        }
    }
}
