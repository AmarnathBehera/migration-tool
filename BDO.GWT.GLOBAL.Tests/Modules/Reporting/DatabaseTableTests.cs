using NUnit.Framework;
using System.Linq;
using BDO.GWT.CustomReportingModule;

namespace BDO.GWT.CustomReportingModule.Tests.Generated
{
    public class DatabaseTableTests
    {
        [Test]
        public void Type_DatabaseTable_Exists()
        {
            var asm = typeof(ReportingApplicationPage).Assembly;
            var t = asm.GetTypes().FirstOrDefault(x => x.Name == "DatabaseTable");
            Assert.That(t, Is.Not.Null);
        }
    }
}
