using NUnit.Framework;
using System.Linq;
using BDO.GWT.CustomReportingModule;

namespace BDO.GWT.CustomReportingModule.Tests.Generated
{
    public class ReportingApplicationChannelSettingsEditSectionTests
    {
        [Test]
        public void Type_ReportingApplicationChannelSettingsEditSection_Exists()
        {
            var asm = typeof(ReportingApplicationPage).Assembly;
            var t = asm.GetTypes().FirstOrDefault(x => x.Name == "ReportingApplicationChannelSettingsEditSection");
            Assert.That(t, Is.Not.Null);
        }
    }
}
