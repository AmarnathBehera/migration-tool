using BDO.GWT.Domain.Entities;

namespace BDO.GWT.GLOBAL.Tests.Modules
{
    public class DomainEntityTests
    {
        private sealed class TestEntity : DomainEntity
        {
        }

        [Test]
        public void DomainEntity_ImplementsIEntity()
        {
            Assert.That(typeof(IEntity).IsAssignableFrom(typeof(DomainEntity)), Is.True);
        }

        [Test]
        public void DomainEntity_IsAbstract()
        {
            Assert.That(typeof(DomainEntity).IsAbstract, Is.True);
        }

        [Test]
        public void ID_DefaultsToZero()
        {
            var entity = new TestEntity();

            Assert.That(entity.ID, Is.EqualTo(0));
        }

        [Test]
        public void ID_CanBeSetAndRetrieved()
        {
            var entity = new TestEntity { ID = 42 };

            Assert.That(entity.ID, Is.EqualTo(42));
        }

        [Test]
        public void ID_IsVirtual()
        {
            var prop = typeof(DomainEntity).GetProperty(nameof(DomainEntity.ID));
            Assert.That(prop, Is.Not.Null);
            Assert.That(prop!.GetGetMethod()!.IsVirtual, Is.True);
        }
    }
}
