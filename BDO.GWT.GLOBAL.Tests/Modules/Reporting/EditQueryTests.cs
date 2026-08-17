using BDO.GWT.CustomReportingModule;
using System.Reflection;
using System.Text.RegularExpressions;

namespace BDO.GWT.GLOBAL.Tests.Modules.Reporting
{
    [TestFixture]
    public class EditQueryTests
    {
        private Type editQueryType = null!;
        private MethodInfo generateMethod = null!;

        [OneTimeSetUp]
        public void Setup()
        {
            var asm = typeof(ReportingApplicationPage).Assembly;

            editQueryType = asm.GetTypes()
                .First(x => x.Name == "EditQuery");

            generateMethod = editQueryType.GetMethod(
                "GenerateQueryCodeName",
                BindingFlags.NonPublic | BindingFlags.Static)!;
        }

        [Test]
        public void Type_EditQuery_Exists()
        {
            Assert.That(editQueryType, Is.Not.Null);
        }

        [Test]
        public void GenerateQueryCodeName_Replaces_Invalid_Characters()
        {
            var result = (string)generateMethod.Invoke(
                null,
                new object[] { "My Query!@#" })!;

            Assert.That(result, Does.StartWith("My_Query"));
        }

        [Test]
        public void GenerateQueryCodeName_Uses_Query_For_Empty_Name()
        {
            var result = (string)generateMethod.Invoke(
                null,
                new object[] { "" })!;

            Assert.That(result, Does.StartWith("Query_"));
        }

        [Test]
        public void GenerateQueryCodeName_Appends_Guid()
        {
            var result = (string)generateMethod.Invoke(
                null,
                new object[] { "Report" })!;

            Assert.That(
                Regex.IsMatch(
                    result,
                    @"^Report_[a-fA-F0-9]{32}$"),
                Is.True);
        }

        [Test]
        public void GenerateQueryCodeName_Returns_Unique_Value_Each_Time()
        {
            var first = (string)generateMethod.Invoke(
                null,
                new object[] { "Report" })!;

            var second = (string)generateMethod.Invoke(
                null,
                new object[] { "Report" })!;

            Assert.That(first, Is.Not.EqualTo(second));
        }
    }
}