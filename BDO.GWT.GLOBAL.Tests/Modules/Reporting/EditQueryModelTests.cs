using BDO.GWT.CustomReportingModule.ReportingApp;

namespace BDO.GWT.GLOBAL.Tests.Modules.Reporting
{
    [TestFixture]
    public class EditQueryModelTests
    {
        [Test]
        public void QueryText_CanBeSetAndRetrieved()
        {
            // Arrange
            var model = new EditQueryModel();

            // Act
            model.QueryText = "SELECT * FROM CMS_User";

            // Assert
            Assert.That(model.QueryText, Is.EqualTo("SELECT * FROM CMS_User"));
        }

        [Test]
        public void QueryText_DefaultsToNull()
        {
            // Arrange
            var model = new EditQueryModel();

            // Assert
            Assert.That(model.QueryText, Is.Null);
        }
    }
}