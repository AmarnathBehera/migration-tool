using BDO.GWT.CustomReportingModule.ReportingApp;

namespace BDO.GWT.GLOBAL.Tests.Modules.Reporting
{
    [TestFixture]
    public class DatabaseTableTests
    {
        [Test]
        public void Constructor_ShouldInitializeColumnsToEmptyCollection()
        {
            var table = new DatabaseTable();

            Assert.That(table.Columns, Is.Not.Null);
            Assert.That(table.Columns, Is.Empty);
        }

        [Test]
        public void Name_CanBeSetAndRetrieved()
        {
            var table = new DatabaseTable();

            table.Name = "CMS_User";

            Assert.That(table.Name, Is.EqualTo("CMS_User"));
        }

        [Test]
        public void Columns_CanBeSetAndRetrieved()
        {
            var table = new DatabaseTable();

            var columns = new[] { "UserID", "UserName" };

            table.Columns = columns;

            Assert.That(table.Columns, Is.EqualTo(columns));
        }

        [Test]
        public void Name_DefaultsToNull()
        {
            var table = new DatabaseTable();

            Assert.That(table.Name, Is.Null);
        }
    }
}