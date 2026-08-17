using BDO.GWT.CustomReportingModule;

namespace BDO.GWT.GLOBAL.Tests.Modules.Reporting
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
