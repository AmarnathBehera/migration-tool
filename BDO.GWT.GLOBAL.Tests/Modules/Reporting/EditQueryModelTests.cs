using NUnit.Framework;
using System.Linq;
using BDO.GWT.CustomReportingModule;

namespace BDO.GWT.CustomReportingModule.Tests.Generated
{
    public class EditQueryModelTests
    {
        [Test]
        public void Type_EditQueryModel_Exists()
        {
            var asm = typeof(ReportingApplicationPage).Assembly;
            var t = asm.GetTypes().FirstOrDefault(x => x.Name == "EditQueryModel");
            Assert.That(t, Is.Not.Null);
        }
    }
}
