using BDO.GWT.CustomReportingModule;
using BDO.GWT.CustomReportingModule.ReportingApp;

namespace BDO.GWT.GLOBAL.Tests.Modules.Reporting
{
    [TestFixture]
    public class SavedQueryTests
    {
        [Test]
        public void Type_SavedQuery_Exists()
        {
            var asm = typeof(ReportingApplicationPage).Assembly;

            var type = asm.GetTypes()
                .FirstOrDefault(x => x.Name == "SavedQuery");

            Assert.That(type, Is.Not.Null);
        }

        [Test]
        public void DefaultConstructor_Sets_Default_Values()
        {
            var query = new SavedQuery();

            Assert.That(query.ID, Is.EqualTo(0));
            Assert.That(query.Name, Is.Null);
            Assert.That(query.Text, Is.Null);
            Assert.That(query.Order, Is.EqualTo(0));
        }

        [Test]
        public void ID_Property_Can_Be_Set()
        {
            var query = new SavedQuery();

            query.ID = 123;

            Assert.That(query.ID, Is.EqualTo(123));
        }

        [Test]
        public void Name_Property_Can_Be_Set()
        {
            var query = new SavedQuery();

            query.Name = "Test Query";

            Assert.That(query.Name, Is.EqualTo("Test Query"));
        }

        [Test]
        public void Text_Property_Can_Be_Set()
        {
            var query = new SavedQuery();

            query.Text = "SELECT * FROM CMS_User";

            Assert.That(query.Text, Is.EqualTo("SELECT * FROM CMS_User"));
        }

        [Test]
        public void Order_Property_Can_Be_Set()
        {
            var query = new SavedQuery();

            query.Order = 5;

            Assert.That(query.Order, Is.EqualTo(5));
        }

        [Test]
        public void Properties_Can_Be_Set_At_Same_Time()
        {
            var query = new SavedQuery
            {
                ID = 10,
                Name = "Users Report",
                Text = "SELECT * FROM CMS_User",
                Order = 2
            };

            Assert.That(query.ID, Is.EqualTo(10));
            Assert.That(query.Name, Is.EqualTo("Users Report"));
            Assert.That(query.Text, Is.EqualTo("SELECT * FROM CMS_User"));
            Assert.That(query.Order, Is.EqualTo(2));
        }
    }
}