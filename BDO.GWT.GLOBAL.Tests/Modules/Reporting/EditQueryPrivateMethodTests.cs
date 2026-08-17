using BDO.GWT.CustomReportingModule;
using System.Reflection;
using System.Text.RegularExpressions;

namespace BDO.GWT.GLOBAL.Tests.Modules.Reporting
{
    [TestFixture]
    public class EditQueryPrivateMethodTests
    {
        private Type editQueryType = null!;
        private MethodInfo generateCodeNameMethod = null!;

        [OneTimeSetUp]
        public void Setup()
        {
            editQueryType = typeof(ReportingApplicationPage)
                .Assembly
                .GetTypes()
                .First(x => x.Name == "EditQuery");

            generateCodeNameMethod = editQueryType.GetMethod(
                "GenerateQueryCodeName",
                BindingFlags.NonPublic | BindingFlags.Static)!;
        }

        [Test]
        public void EditQuery_Type_Exists()
        {
            Assert.That(editQueryType, Is.Not.Null);
        }

        [Test]
        public void GenerateQueryCodeName_WhenValidName_ReturnsNameWithGuid()
        {
            var result = (string)generateCodeNameMethod.Invoke(
                null,
                new object[] { "SalesReport" })!;

            Assert.That(result, Does.StartWith("SalesReport_"));
        }

        [Test]
        public void GenerateQueryCodeName_WhenEmptyName_ReturnsQueryPrefix()
        {
            var result = (string)generateCodeNameMethod.Invoke(
                null,
                new object[] { "" })!;

            Assert.That(result, Does.StartWith("Query_"));
        }

        [Test]
        public void GenerateQueryCodeName_WhenInvalidCharacters_ReplacesWithUnderscores()
        {
            var result = (string)generateCodeNameMethod.Invoke(
                null,
                new object[] { "Sales Report !@#" })!;

            Assert.That(result, Does.StartWith("Sales_Report____"));
        }

        [Test]
        public void GenerateQueryCodeName_WhenCalledTwice_ReturnsDifferentValues()
        {
            var first = (string)generateCodeNameMethod.Invoke(
                null,
                new object[] { "Report" })!;

            var second = (string)generateCodeNameMethod.Invoke(
                null,
                new object[] { "Report" })!;

            Assert.That(first, Is.Not.EqualTo(second));
        }

        [Test]
        public void GenerateQueryCodeName_Appends32CharacterGuid()
        {
            var result = (string)generateCodeNameMethod.Invoke(
                null,
                new object[] { "MyReport" })!;

            Assert.That(
                Regex.IsMatch(
                    result,
                    @"^MyReport_[A-Fa-f0-9]{32}$"),
                Is.True);
        }
    }
}