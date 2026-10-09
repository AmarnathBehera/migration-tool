using BDO.GWT.Domain.Entities;

namespace BDO.GWT.GLOBAL.Tests.Modules
{
    public class EntityInterfaceTests
    {
        [Test]
        public void IEntity_HasIdProperty()
        {
            var prop = typeof(IEntity).GetProperty("ID");
            Assert.That(prop, Is.Not.Null);
            Assert.That(prop!.PropertyType, Is.EqualTo(typeof(int)));
        }

        [Test]
        public void ICodeNameEntity_ExtendsIEntity()
        {
            Assert.That(typeof(IEntity).IsAssignableFrom(typeof(ICodeNameEntity)), Is.True);
        }

        [Test]
        public void ICodeNameEntity_HasCodeNameProperty()
        {
            var prop = typeof(ICodeNameEntity).GetProperty("CodeName");
            Assert.That(prop, Is.Not.Null);
            Assert.That(prop!.PropertyType, Is.EqualTo(typeof(string)));
        }

        [Test]
        public void IGuidEntity_ExtendsIEntity()
        {
            Assert.That(typeof(IEntity).IsAssignableFrom(typeof(IGuidEntity)), Is.True);
        }

        [Test]
        public void IGuidEntity_HasGuidProperty()
        {
            var prop = typeof(IGuidEntity).GetProperty("GUID");
            Assert.That(prop, Is.Not.Null);
            Assert.That(prop!.PropertyType, Is.EqualTo(typeof(Guid)));
        }

        [Test]
        public void IOptionalSiteEntity_ExtendsIEntity()
        {
            Assert.That(typeof(IEntity).IsAssignableFrom(typeof(IOptionalSiteEntity)), Is.True);
        }

        [Test]
        public void IOptionalSiteEntity_HasNullableSiteId()
        {
            var prop = typeof(IOptionalSiteEntity).GetProperty("SiteID");
            Assert.That(prop, Is.Not.Null);
            Assert.That(prop!.PropertyType, Is.EqualTo(typeof(int?)));
        }
    }
}
