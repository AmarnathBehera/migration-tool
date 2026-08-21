using BDO.GWT.CustomReportingModule;

namespace BDO.GWT.GLOBAL.Tests.Modules.Reporting
{
    [TestFixture]
    public class ExportConfirmationDialogModelTests
    {
        [Test]
        public void Type_ExportConfirmationDialogModel_Exists()
        {
            var asm = typeof(ReportingApplicationPage).Assembly;
            var t = asm.GetTypes().FirstOrDefault(x => x.Name == "ExportConfirmationDialogModel");

            Assert.That(t, Is.Not.Null);
        }

        [Test]
        public void ExportType_Defaults_To_CSV()
        {
            var model = new CustomReportingModule.ReportingApp.ExportConfirmationDialogModel();

            Assert.That(model.ExportType, Is.EqualTo("CSV"));
        }

        [Test]
        public void FileName_Defaults_To_Null()
        {
            var model = new CustomReportingModule.ReportingApp.ExportConfirmationDialogModel();

            Assert.That(model.FileName, Is.Null);
        }

        [Test]
        public void ExportType_Can_Be_Set()
        {
            var model = new CustomReportingModule.ReportingApp.ExportConfirmationDialogModel
            {
                ExportType = "Excel"
            };

            Assert.That(model.ExportType, Is.EqualTo("Excel"));
        }

        [Test]
        public void FileName_Can_Be_Set()
        {
            var model = new CustomReportingModule.ReportingApp.ExportConfirmationDialogModel
            {
                FileName = "SalesReport"
            };

            Assert.That(model.FileName, Is.EqualTo("SalesReport"));
        }
    }
}