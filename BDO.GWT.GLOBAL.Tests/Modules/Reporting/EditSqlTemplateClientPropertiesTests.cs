using BDO.GWT.CustomReportingModule.ReportingApp;

namespace BDO.GWT.GLOBAL.Tests.Modules.Reporting
{
    [TestFixture]
    public class EditSqlTemplateClientPropertiesTests
    {
        [Test]
        public void Constructor_InitializesTablesToEmptyCollection()
        {
            var properties = new EditSqlTemplateClientProperties();

            Assert.That(properties.Tables, Is.Not.Null);
            Assert.That(properties.Tables, Is.Empty);
        }

        [Test]
        public void Constructor_InitializesSavedQueriesToEmptyCollection()
        {
            var properties = new EditSqlTemplateClientProperties();

            Assert.That(properties.SavedQueries, Is.Not.Null);
            Assert.That(properties.SavedQueries, Is.Empty);
        }

        [Test]
        public void Query_DefaultValue_IsNull()
        {
            var properties = new EditSqlTemplateClientProperties();

            Assert.That(properties.Query, Is.Null);
        }

        [Test]
        public void ReportingChannelSettingId_DefaultValue_IsZero()
        {
            var properties = new EditSqlTemplateClientProperties();

            Assert.That(properties.ReportingChannelSettingId, Is.EqualTo(0));
        }

        [Test]
        public void Query_CanBeAssigned()
        {
            var properties = new EditSqlTemplateClientProperties();

            properties.Query = "SELECT * FROM CMS_User";

            Assert.That(properties.Query, Is.EqualTo("SELECT * FROM CMS_User"));
        }

        [Test]
        public void ReportingChannelSettingId_CanBeAssigned()
        {
            var properties = new EditSqlTemplateClientProperties();

            properties.ReportingChannelSettingId = 100;

            Assert.That(properties.ReportingChannelSettingId, Is.EqualTo(100));
        }

        [Test]
        public void Tables_CanBeAssigned()
        {
            var properties = new EditSqlTemplateClientProperties();

            var tables = new[]
            {
                new DatabaseTable
                {
                    Name = "CMS_User",
                    Columns = new[] { "UserID", "UserName" }
                }
            };

            properties.Tables = tables;

            Assert.That(properties.Tables, Is.EqualTo(tables));
            Assert.That(properties.Tables.Count(), Is.EqualTo(1));
        }

        [Test]
        public void SavedQueries_CanBeAssigned()
        {
            var properties = new EditSqlTemplateClientProperties();

            var queries = new[]
            {
                new SavedQuery
                {
                    ID = 1,
                    Name = "Sample Query"
                }
            };

            properties.SavedQueries = queries;

            Assert.That(properties.SavedQueries, Is.EqualTo(queries));
            Assert.That(properties.SavedQueries.Count(), Is.EqualTo(1));
        }
    }
}