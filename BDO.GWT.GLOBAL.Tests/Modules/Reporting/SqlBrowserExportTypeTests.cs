using NUnit.Framework;
using System.Linq;
using BDO.GWT.CustomReportingModule;

namespace BDO.GWT.GLOBAL.Tests.Modules.Reporting
{
    public class SqlBrowserExportTypeTests
    {
        [Test]
        public void Type_SqlBrowserExportType_Exists()
        {
            var asm = typeof(ReportingApplicationPage).Assembly;
            var t = asm.GetTypes().FirstOrDefault(x => x.Name == "SqlBrowserExportType");
            Assert.That(t, Is.Not.Null);
        }
    }
}
