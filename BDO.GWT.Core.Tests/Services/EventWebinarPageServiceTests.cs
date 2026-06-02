using System.Reflection;
using BDO;
using BDO.GWT.Web.Core.Services;

namespace BDO.GWT.Core.Tests.Services
{
    public class EventWebinarPageServiceTests
    {
        [Test]
        public void Service_ImplementsInterface()
        {
            Assert.That(typeof(IEventWebinarPageService).IsAssignableFrom(typeof(EventWebinarPageService)), Is.True);
        }

        [Test]
        public void Interface_DeclaresExpectedMembers()
        {
            var t = typeof(IEventWebinarPageService);
            Assert.Multiple(() =>
            {
                Assert.That(t.GetMethod(nameof(IEventWebinarPageService.GetEventPageDtoAsync)), Is.Not.Null);
                Assert.That(t.GetMethod(nameof(IEventWebinarPageService.GetWebinarPageDtoAsync)), Is.Not.Null);
                Assert.That(t.GetMethod(nameof(IEventWebinarPageService.GetEventSchemaMarkupDto)), Is.Not.Null);
                Assert.That(t.GetMethod(nameof(IEventWebinarPageService.GetWebinarSchemaMarkupDto)), Is.Not.Null);
                Assert.That(t.GetMethod(nameof(IEventWebinarPageService.GenerateOutlookEventCardContent)), Is.Not.Null);
                Assert.That(t.GetMethod(nameof(IEventWebinarPageService.GenerateOutlookWebinarCardContent)), Is.Not.Null);
            });
        }

        [Test]
        public void Service_HasExpectedConstructor()
        {
            var ctor = typeof(EventWebinarPageService).GetConstructors(BindingFlags.Public | BindingFlags.Instance).FirstOrDefault();
            Assert.That(ctor, Is.Not.Null);
            var parameters = ctor!.GetParameters();
            Assert.That(parameters, Has.Length.GreaterThan(0));
        }

        [Test]
        public void Event_ImplementsIEventBase()
        {
            Assert.That(typeof(Event).IsAssignableFrom(typeof(Event)), Is.True);
        }

        [Test]
        public void Webinar_ImplementsIEventBase()
        {
            Assert.That(typeof(Webinar).IsAssignableFrom(typeof(Webinar)), Is.True);
        }
    }
}
