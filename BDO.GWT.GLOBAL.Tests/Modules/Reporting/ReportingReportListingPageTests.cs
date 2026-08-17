using BDO.GWT.CustomReportingModule;
using BDO.GWT.CustomReportingModule.ReportingApp;
using Microsoft.Extensions.Localization;
using Moq;

namespace BDO.GWT.GLOBAL.Tests.Modules.Reporting
{
    [TestFixture]
    public class ReportingReportListingPageTests
    {
        [Test]
        public void Constructor_Creates_Instance()
        {
            var localizer =
                new Mock<IStringLocalizer<SharedResources>>();

            var page = new ReportingReportListingPage(
                localizer.Object);

            Assert.That(page, Is.Not.Null);
        }

        [Test]
        public void ReportingReportId_CanBeSetAndRetrieved()
        {
            var localizer =
                new Mock<IStringLocalizer<SharedResources>>();

            var page = new ReportingReportListingPage(
                localizer.Object);

            page.ReportingReportId = 25;

            Assert.That(page.ReportingReportId,
                Is.EqualTo(25));
        }
    }
}