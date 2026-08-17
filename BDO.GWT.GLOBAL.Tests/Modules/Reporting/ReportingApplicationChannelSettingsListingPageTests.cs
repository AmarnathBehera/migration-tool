using NUnit.Framework;
using System.Linq;
using BDO.GWT.CustomReportingModule;

namespace BDO.GWT.CustomReportingModule.Tests.Generated
{
    public class ReportingApplicationChannelSettingsListingPageTests
    {
        [Test]
        public void Type_ReportingApplicationChannelSettingsListingPage_Exists()
        {
            var asm = typeof(ReportingApplicationPage).Assembly;
            var t = asm.GetTypes().FirstOrDefault(x => x.Name == "ReportingApplicationChannelSettingsListingPage");
            Assert.That(t, Is.Not.Null);
        }
    }
}
