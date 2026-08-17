using NUnit.Framework;
using System.Linq;
using BDO.GWT.CustomReportingModule;

namespace BDO.GWT.CustomReportingModule.Tests.Generated
{
    public class ISqlQueryValidatorTests
    {
        [Test]
        public void Type_ISqlQueryValidator_Exists()
        {
            var asm = typeof(ReportingApplicationPage).Assembly;
            var t = asm.GetTypes().FirstOrDefault(x => x.Name == "ISqlQueryValidator");
            Assert.That(t, Is.Not.Null);
            Assert.That(t.IsInterface, Is.True);
        }
    }
}
