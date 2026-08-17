using NUnit.Framework;
using System.Linq;
using BDO.GWT.CustomReportingModule;

namespace BDO.GWT.CustomReportingModule.Tests.Generated
{
    public class ExportConfirmationDialogModelTests
    {
        [Test]
        public void Type_ExportConfirmationDialogModel_Exists()
        {
            var asm = typeof(ReportingApplicationPage).Assembly;
            var t = asm.GetTypes().FirstOrDefault(x => x.Name == "ExportConfirmationDialogModel");
            Assert.That(t, Is.Not.Null);
        }
    }
}
