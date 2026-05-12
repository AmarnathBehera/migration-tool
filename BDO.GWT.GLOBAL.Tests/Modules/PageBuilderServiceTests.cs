using System.Reflection;
using BDO.GWT.Domain.Services;
using BDO.GWT.Xperience.LiveSite.Services;
using Moq;

namespace BDO.GWT.GLOBAL.Tests.Modules
{
    public class PageBuilderServiceTests
    {
        private Mock<ISettingsService> _settingsService = null!;
        private PageBuilderService _service = null!;

        [SetUp]
        public void Setup()
        {
            _settingsService = new Mock<ISettingsService>();
            _service = new PageBuilderService(_settingsService.Object);
        }

        private static List<string> GetPrivateList(PageBuilderService service, string fieldName)
        {
            var field = typeof(PageBuilderService).GetField(fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            return (List<string>)field!.GetValue(service)!;
        }

        private static string GetIdentifier(PageBuilderService service)
        {
            var field = typeof(PageBuilderService).GetField("_identifier",
                BindingFlags.Instance | BindingFlags.NonPublic);
            return (string)field!.GetValue(service)!;
        }

        [Test]
        public void Service_ImplementsIPageBuilderService()
        {
            Assert.That(_service, Is.InstanceOf<IPageBuilderService>());
        }

        [Test]
        public void AddWidgets_ReturnsSameInstanceAndAppendsWidgets()
        {
            var returned = _service.AddWidgets(new[] { "w1", "w2" });

            Assert.That(returned, Is.SameAs(_service));
            var widgets = GetPrivateList(_service, "_widgets");
            Assert.That(widgets, Is.EqualTo(new[] { "w1", "w2" }));
        }

        [Test]
        public void AddAdvancedWidgets_ReturnsSameInstanceAndAppendsWidgets()
        {
            var returned = _service.AddAdvancedWidgets(new[] { "aw1" });

            Assert.That(returned, Is.SameAs(_service));
            var widgets = GetPrivateList(_service, "_advancedWidgets");
            Assert.That(widgets, Is.EqualTo(new[] { "aw1" }));
        }

        [Test]
        public void AddSections_ReturnsSameInstanceAndAppendsSections()
        {
            var returned = _service.AddSections(new[] { "s1", "s2" });

            Assert.That(returned, Is.SameAs(_service));
            var sections = GetPrivateList(_service, "_sections");
            Assert.That(sections, Is.EqualTo(new[] { "s1", "s2" }));
        }

        [Test]
        public void AddAdvancedSections_ReturnsSameInstanceAndAppendsSections()
        {
            var returned = _service.AddAdvancedSections(new[] { "as1" });

            Assert.That(returned, Is.SameAs(_service));
            var sections = GetPrivateList(_service, "_advancedSections");
            Assert.That(sections, Is.EqualTo(new[] { "as1" }));
        }

        [Test]
        public void AddIdentifier_SetsIdentifier()
        {
            var returned = _service.AddIdentifier("custom-area");

            Assert.That(returned, Is.SameAs(_service));
            Assert.That(GetIdentifier(_service), Is.EqualTo("custom-area"));
        }

        [Test]
        public void AddDefaultWidgets_AddsTwoWidgets()
        {
            var returned = _service.AddDefaultWidgets();

            Assert.That(returned, Is.SameAs(_service));
            var widgets = GetPrivateList(_service, "_widgets");
            Assert.That(widgets, Has.Count.EqualTo(2));
        }

        [Test]
        public void Default_IdentifierIsMainArea()
        {
            Assert.That(GetIdentifier(_service), Is.EqualTo("mainArea"));
        }

        [Test]
        public void AddWidgets_CanBeChained()
        {
            _service
                .AddWidgets(new[] { "a" })
                .AddSections(new[] { "s" })
                .AddIdentifier("zone1");

            Assert.Multiple(() =>
            {
                Assert.That(GetPrivateList(_service, "_widgets"), Is.EqualTo(new[] { "a" }));
                Assert.That(GetPrivateList(_service, "_sections"), Is.EqualTo(new[] { "s" }));
                Assert.That(GetIdentifier(_service), Is.EqualTo("zone1"));
            });
        }
    }
}
