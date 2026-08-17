using NUnit.Framework;
using System.Linq;
using BDO.GWT.CustomReportingModule;

namespace BDO.GWT.CustomReportingModule.Tests.Generated
{
    public class ValidationResultTests
    {
        [Test]
        public void Type_ValidationResult_Exists()
        {
            var asm = typeof(ReportingApplicationPage).Assembly;
            var t = asm.GetTypes().FirstOrDefault(x => x.Name == "ValidationResult");
            Assert.That(t, Is.Not.Null);
        }
    }
}
