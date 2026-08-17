using NUnit.Framework;
using System.Linq;
using BDO.GWT.CustomReportingModule;

namespace BDO.GWT.CustomReportingModule.Tests.Generated
{
    public class SavedQueryTests
    {
        [Test]
        public void Type_SavedQuery_Exists()
        {
            var asm = typeof(ReportingApplicationPage).Assembly;
            var t = asm.GetTypes().FirstOrDefault(x => x.Name == "SavedQuery");
            Assert.That(t, Is.Not.Null);
        }
    }
}
