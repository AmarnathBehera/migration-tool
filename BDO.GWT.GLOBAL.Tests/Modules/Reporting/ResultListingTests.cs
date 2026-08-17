using BDO.GWT.CustomReportingModule.ReportingApp;
using Kentico.Xperience.Admin.Base;

namespace BDO.GWT.GLOBAL.Tests.Modules.Reporting
{
    [TestFixture]
    public class ResultListingTests
    {
        [Test]
        public void Type_ResultListing_Exists()
        {
            Assert.That(typeof(ResultListing), Is.Not.Null);
        }

        [Test]
        public void Type_Inherits_From_DataContainerListingPage()
        {
            Assert.That(
                typeof(ResultListing).BaseType,
                Is.EqualTo(typeof(DataContainerListingPage)));
        }

        [Test]
        public void ReportingChannelSettingId_Property_Exists()
        {
            var property = typeof(ResultListing)
                .GetProperty(nameof(ResultListing.ReportingChannelSettingId));

            Assert.That(property, Is.Not.Null);
            Assert.That(property!.PropertyType, Is.EqualTo(typeof(int)));
        }

        [Test]
        public void Class_Has_UINavigation_Attribute()
        {
            var attribute = typeof(ResultListing)
                .GetCustomAttributes(typeof(UINavigationAttribute), false)
                .FirstOrDefault();

            Assert.That(attribute, Is.Not.Null);
        }

        [Test]
        public void Class_Has_UIEvaluatePermission_Attribute()
        {
            var attribute = typeof(ResultListing)
                .GetCustomAttributes(typeof(UIEvaluatePermissionAttribute), false)
                .FirstOrDefault();

            Assert.That(attribute, Is.Not.Null);
        }

        [Test]
        public void ConfigurePage_Method_Exists()
        {
            var method = typeof(ResultListing)
                .GetMethod(nameof(ResultListing.ConfigurePage));

            Assert.That(method, Is.Not.Null);
        }

        [Test]
        public void Export_Method_Exists()
        {
            var method = typeof(ResultListing)
                .GetMethod(nameof(ResultListing.Export));

            Assert.That(method, Is.Not.Null);
        }

        [Test]
        public void SubmitQuery_Method_Exists()
        {
            var method = typeof(ResultListing)
                .GetMethod(nameof(ResultListing.SubmitQuery));

            Assert.That(method, Is.Not.Null);
        }

        [Test]
        public void OpenEditQuery_Method_Exists()
        {
            var method = typeof(ResultListing)
                .GetMethod(nameof(ResultListing.OpenEditQuery));

            Assert.That(method, Is.Not.Null);
        }

        [Test]
        public void BackToChannel_Method_Exists()
        {
            var method = typeof(ResultListing)
                .GetMethod(nameof(ResultListing.BackToChannel));

            Assert.That(method, Is.Not.Null);
        }

        [Test]
        public void GetIdentifier_Method_Exists()
        {
            var method = typeof(ResultListing)
                .GetMethod(
                    "GetIdentifier",
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.NonPublic);

            Assert.That(method, Is.Not.Null);
        }

        [Test]
        public void LoadDataContainers_Method_Exists()
        {
            var method = typeof(ResultListing)
                .GetMethod(
                    "LoadDataContainers",
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.NonPublic);

            Assert.That(method, Is.Not.Null);
        }

        [Test]
        public void Export_Method_Has_PageCommand_Attribute()
        {
            var method = typeof(ResultListing)
                .GetMethod(nameof(ResultListing.Export));

            var attribute = method?
                .GetCustomAttributes(typeof(PageCommandAttribute), false)
                .FirstOrDefault();

            Assert.That(attribute, Is.Not.Null);
        }

        [Test]
        public void SubmitQuery_Method_Has_PageCommand_Attribute()
        {
            var method = typeof(ResultListing)
                .GetMethod(nameof(ResultListing.SubmitQuery));

            var attribute = method?
                .GetCustomAttributes(typeof(PageCommandAttribute), false)
                .FirstOrDefault();

            Assert.That(attribute, Is.Not.Null);
        }

        [Test]
        public void OpenEditQuery_Method_Has_PageCommand_Attribute()
        {
            var method = typeof(ResultListing)
                .GetMethod(nameof(ResultListing.OpenEditQuery));

            var attribute = method?
                .GetCustomAttributes(typeof(PageCommandAttribute), false)
                .FirstOrDefault();

            Assert.That(attribute, Is.Not.Null);
        }

        [Test]
        public void BackToChannel_Method_Has_PageCommand_Attribute()
        {
            var method = typeof(ResultListing)
                .GetMethod(nameof(ResultListing.BackToChannel));

            var attribute = method?
                .GetCustomAttributes(typeof(PageCommandAttribute), false)
                .FirstOrDefault();

            Assert.That(attribute, Is.Not.Null);
        }
    }
}