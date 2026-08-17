using NUnit.Framework;
using System.Linq;
using BDO.GWT.CustomReportingModule;

namespace BDO.GWT.CustomReportingModule.Tests.Generated
{
    public class ResultListingTests
    {
        [Test]
        public void Type_ResultListing_Exists()
        {
            var asm = typeof(ReportingApplicationPage).Assembly;
            var t = asm.GetTypes().FirstOrDefault(x => x.Name == "ResultListing");
            Assert.That(t, Is.Not.Null);
        }
    }
}
