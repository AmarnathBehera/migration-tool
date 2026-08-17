using NUnit.Framework;
using System.Linq;
using BDO.GWT.CustomReportingModule;

namespace BDO.GWT.GLOBAL.Tests.Modules.Reporting
{
    public class SharedResourcesTests
    {
        [Test]
        public void Type_SharedResources_Exists()
        {
            var asm = typeof(ReportingApplicationPage).Assembly;
            var t = asm.GetTypes().FirstOrDefault(x => x.Name == "SharedResources");
            Assert.That(t, Is.Not.Null);
        }
    }
}
