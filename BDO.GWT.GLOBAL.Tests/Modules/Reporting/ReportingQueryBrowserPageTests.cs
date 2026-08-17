using BDO.GWT.CustomReportingModule.ReportingApp;

namespace BDO.GWT.GLOBAL.Tests.Modules.Reporting
{
    [TestFixture]
    public class ReportingQueryBrowserPageTests
    {
        [Test]
        public void ReportingChannelSettingId_CanBeSetAndRead()
        {
            var page = new ReportingQueryBrowserPage();

            page.ReportingChannelSettingId = 123;

            Assert.That(page.ReportingChannelSettingId, Is.EqualTo(123));
        }

        [Test]
        public void ReportingChannelSettingId_DefaultsToZero()
        {
            var page = new ReportingQueryBrowserPage();

            Assert.That(page.ReportingChannelSettingId, Is.EqualTo(0));
        }
    }
}