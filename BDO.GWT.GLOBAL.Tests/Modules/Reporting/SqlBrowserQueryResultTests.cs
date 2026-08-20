using NUnit.Framework;
using System.Linq;
using BDO.GWT.CustomReportingModule;

namespace BDO.GWT.GLOBAL.Tests.Modules.Reporting
{
    public class SqlBrowserQueryResultTests
    {
        [Test]
        public void Type_SqlBrowserQueryResult_Exists()
        {
            var asm = typeof(ReportingApplicationPage).Assembly;
            var t = asm.GetTypes().FirstOrDefault(x => x.Name == "SqlBrowserQueryResult");
            Assert.That(t, Is.Not.Null);
        }
    }
}
