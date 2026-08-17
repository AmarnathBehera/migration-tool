using NUnit.Framework;
using System.Linq;
using BDO.GWT.CustomReportingModule;

namespace BDO.GWT.CustomReportingModule.Tests.Generated
{
    public class EditQueryTests
    {
        [Test]
        public void Type_EditQuery_Exists()
        {
            var asm = typeof(ReportingApplicationPage).Assembly;
            var t = asm.GetTypes().FirstOrDefault(x => x.Name == "EditQuery");
            Assert.That(t, Is.Not.Null);
        }
    }
}
