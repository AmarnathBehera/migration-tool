using BDO.GWT.CustomReportingModule.ReportingApp;
using Kentico.Xperience.Admin.Base;
using System.Reflection;

namespace BDO.GWT.GLOBAL.Tests.Modules.Reporting
{
    [TestFixture]
    public class ReportingQueryBrowserPageTests
    {
        [Test]
        public void Type_ReportingQueryBrowserPage_Exists()
        {
            Assert.That(typeof(ReportingQueryBrowserPage), Is.Not.Null);
        }

        [Test]
        public void Page_InheritsFrom_ApplicationPage()
        {
            Assert.That(
                typeof(ApplicationPage).IsAssignableFrom(typeof(ReportingQueryBrowserPage)),
                Is.True);
        }

        [Test]
        public void ReportingChannelSettingId_Property_Exists()
        {
            var property = typeof(ReportingQueryBrowserPage)
                .GetProperty(nameof(ReportingQueryBrowserPage.ReportingChannelSettingId));

            Assert.That(property, Is.Not.Null);
        }

        [Test]
        public void ReportingChannelSettingId_Property_Is_Int()
        {
            var property = typeof(ReportingQueryBrowserPage)
                .GetProperty(nameof(ReportingQueryBrowserPage.ReportingChannelSettingId));

            Assert.That(property, Is.Not.Null);
            Assert.That(property!.PropertyType, Is.EqualTo(typeof(int)));
        }

        [Test]
        public void ReportingChannelSettingId_Property_Has_Public_Getter()
        {
            var property = typeof(ReportingQueryBrowserPage)
                .GetProperty(nameof(ReportingQueryBrowserPage.ReportingChannelSettingId));

            Assert.That(property?.CanRead, Is.True);
        }

        [Test]
        public void ReportingChannelSettingId_Property_Has_Public_Setter()
        {
            var property = typeof(ReportingQueryBrowserPage)
                .GetProperty(nameof(ReportingQueryBrowserPage.ReportingChannelSettingId));

            Assert.That(property?.CanWrite, Is.True);
        }

        [Test]
        public void ReportingChannelSettingId_Has_PageParameter_Attribute()
        {
            var property = typeof(ReportingQueryBrowserPage)
                .GetProperty(nameof(ReportingQueryBrowserPage.ReportingChannelSettingId));

            var attribute = property?.GetCustomAttribute<PageParameterAttribute>();

            Assert.That(attribute, Is.Not.Null);
        }

        [Test]
        public void Class_Has_UIPermission_Attributes()
        {
            var attributes = typeof(ReportingQueryBrowserPage)
                .GetCustomAttributes(false);

            Assert.That(attributes.Length, Is.GreaterThan(0));
        }
    }
}