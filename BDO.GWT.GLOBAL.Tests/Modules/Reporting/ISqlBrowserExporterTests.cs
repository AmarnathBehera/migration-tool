using NUnit.Framework;
using System.Linq;
using BDO.GWT.CustomReportingModule;

namespace BDO.GWT.CustomReportingModule.Tests.Generated
{
    public class ISqlBrowserExporterTests
    {
        [Test]
        public void Type_ISqlBrowserExporter_Exists()
        {
            var asm = typeof(ReportingApplicationPage).Assembly;
            var t = asm.GetTypes().FirstOrDefault(x => x.Name == "ISqlBrowserExporter");
            Assert.That(t, Is.Not.Null);
            Assert.That(t.IsInterface, Is.True);
        }
    }
}
