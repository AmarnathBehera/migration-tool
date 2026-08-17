using NUnit.Framework;
using System.Linq;
using BDO.GWT.CustomReportingModule;

namespace BDO.GWT.CustomReportingModule.Tests.Generated
{
    public class ViewRecordTests
    {
        [Test]
        public void Type_ViewRecord_Exists()
        {
            var asm = typeof(ReportingApplicationPage).Assembly;
            var t = asm.GetTypes().FirstOrDefault(x => x.Name == "ViewRecord");
            Assert.That(t, Is.Not.Null);
        }
    }
}
