using BDO.GWT.CustomReportingModule;
using System.Reflection;
using System.Text.RegularExpressions;

namespace BDO.GWT.GLOBAL.Tests.Modules.Reporting
{
    [TestFixture]
    public class EditQueryCodeNameTests
    {
        private MethodInfo? generateMethod;

        [SetUp]
        public void Setup()
        {
            var type = typeof(ReportingApplicationPage)
                .Assembly
                .GetTypes()
                .First(x => x.Name == "EditQuery");

            generateMethod = type.GetMethod(
                "GenerateQueryCodeName",
                BindingFlags.NonPublic | BindingFlags.Static);
        }

        [Test]
        public void GenerateQueryCodeName_Replaces_Invalid_Characters()
        {
            var result = (string)generateMethod!.Invoke(
                null,
                new object[] { "Test Query!@#" })!;

            Assert.That(result, Does.StartWith("Test_Query"));
        }

        [Test]
        public void GenerateQueryCodeName_Empty_Name_Returns_Query_Prefix()
        {
            var result = (string)generateMethod!.Invoke(
                null,
                new object[] { "" })!;

            Assert.That(result, Does.StartWith("Query_"));
        }

        [Test]
        public void GenerateQueryCodeName_Appends_Guid()
        {
            var result = (string)generateMethod!.Invoke(
                null,
                new object[] { "MyQuery" })!;

            Assert.That(
                Regex.IsMatch(
                    result,
                    @"^MyQuery_[a-fA-F0-9]{32}$"),
                Is.True);
        }

        [Test]
        public void GenerateQueryCodeName_Generates_Unique_Values()
        {
            var first = (string)generateMethod!.Invoke(
                null,
                new object[] { "Report" })!;

            var second = (string)generateMethod!.Invoke(
                null,
                new object[] { "Report" })!;

            Assert.That(first, Is.Not.EqualTo(second));
        }
    }
}