using BDO.GWT.CustomReportingModule;
using System.Reflection;

namespace BDO.GWT.GLOBAL.Tests.Modules.Reporting
{
    [TestFixture]
    public class EditQueryTests
    {
        private Type? editQueryType;

        [OneTimeSetUp]
        public void Setup()
        {
            var asm = typeof(ReportingApplicationPage).Assembly;
            editQueryType = asm.GetTypes().FirstOrDefault(t => t.Name == "EditQuery");
        }

        [Test]
        public void Type_EditQuery_Exists()
        {
            Assert.That(editQueryType, Is.Not.Null);
        }

        [Test]
        public void GenerateQueryCodeName_Method_Exists()
        {
            var method = editQueryType?.GetMethod(
                "GenerateQueryCodeName",
                BindingFlags.NonPublic | BindingFlags.Static);

            Assert.That(method, Is.Not.Null);
        }

        [Test]
        public void GetSavedQueries_Method_Exists()
        {
            var method = editQueryType?.GetMethod(
                "GetSavedQueries",
                BindingFlags.NonPublic | BindingFlags.Instance);

            Assert.That(method, Is.Not.Null);
        }
    }
}
