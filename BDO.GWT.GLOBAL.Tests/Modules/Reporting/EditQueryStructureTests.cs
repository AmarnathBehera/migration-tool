using BDO.GWT.CustomReportingModule;

namespace BDO.GWT.GLOBAL.Tests.Modules.Reporting
{
    [TestFixture]
    public class EditQueryStructureTests
    {
        private Type editQueryType = null!;

        [OneTimeSetUp]
        public void Setup()
        {
            editQueryType = typeof(ReportingApplicationPage)
                .Assembly
                .GetTypes()
                .First(x => x.Name == "EditQuery");
        }

        [Test]
        public void RunSql_Method_Exists()
        {
            Assert.That(
                editQueryType.GetMethod("RunSql"),
                Is.Not.Null);
        }

        [Test]
        public void SaveQuery_Method_Exists()
        {
            Assert.That(
                editQueryType.GetMethod("SaveQuery"),
                Is.Not.Null);
        }

        [Test]
        public void RenameQuery_Method_Exists()
        {
            Assert.That(
                editQueryType.GetMethod("RenameQuery"),
                Is.Not.Null);
        }

        [Test]
        public void DeleteQuery_Method_Exists()
        {
            Assert.That(
                editQueryType.GetMethod("DeleteQuery"),
                Is.Not.Null);
        }

        [Test]
        public void ExportQuery_Method_Exists()
        {
            Assert.That(
                editQueryType.GetMethod("ExportQuery"),
                Is.Not.Null);
        }

        [Test]
        public void ConfigureTemplateProperties_Method_Exists()
        {
            Assert.That(
                editQueryType.GetMethod("ConfigureTemplateProperties"),
                Is.Not.Null);
        }
    }
}