using NUnit.Framework;
using System.Linq;
using BDO.GWT.CustomReportingModule;

namespace BDO.GWT.CustomReportingModule.Tests.Generated
{
    public class ReportingReportListingPageTests
    {
        [Test]
        public void Type_ReportingReportListingPage_Exists()
        {
            var asm = typeof(ReportingApplicationPage).Assembly;
            var t = asm.GetTypes().FirstOrDefault(x => x.Name == "ReportingReportListingPage");
            Assert.That(t, Is.Not.Null);
        }
    }
}
