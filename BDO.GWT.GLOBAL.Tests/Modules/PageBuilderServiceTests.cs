using System.Reflection;
using BDO.GWT.Core.Services;
using BDO.GWT.Xperience.LiveSite.Services;
using Moq;

namespace BDO.GWT.GLOBAL.Tests.Modules
{
    public class PageBuilderServiceTests
    {
        private static readonly string[] WidgetsW1W2 = ["w1", "w2"];
        private static readonly string[] AdvancedWidgetsAw1 = ["aw1"];
        private static readonly string[] SectionsS1S2 = ["s1", "s2"];
        private static readonly string[] AdvancedSectionsAs1 = ["as1"];
        private static readonly string[] SingleA = ["a"];
        private static readonly string[] SingleS = ["s"];

        private Mock<ISettingService> _settingsService = null!;
        private PageBuilderService _service = null!;

        [SetUp]
        public void Setup()
        {
            _settingsService = new Mock<ISettingService>();
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
            var returned = _service.AddWidgets(WidgetsW1W2);

            Assert.Multiple(() =>
            {
                Assert.That(returned, Is.SameAs(_service));
                Assert.That(GetPrivateList(_service, "_widgets"), Is.EqualTo(WidgetsW1W2));
            });
        }

        [Test]
        public void AddAdvancedWidgets_ReturnsSameInstanceAndAppendsWidgets()
        {
            var returned = _service.AddAdvancedWidgets(AdvancedWidgetsAw1);

            Assert.Multiple(() =>
            {
                Assert.That(returned, Is.SameAs(_service));
                Assert.That(GetPrivateList(_service, "_advancedWidgets"), Is.EqualTo(AdvancedWidgetsAw1));
            });
        }

        [Test]
        public void AddSections_ReturnsSameInstanceAndAppendsSections()
        {
            var returned = _service.AddSections(SectionsS1S2);

            Assert.Multiple(() =>
            {
                Assert.That(returned, Is.SameAs(_service));
                Assert.That(GetPrivateList(_service, "_sections"), Is.EqualTo(SectionsS1S2));
            });
        }

        [Test]
        public void AddAdvancedSections_ReturnsSameInstanceAndAppendsSections()
        {
            var returned = _service.AddAdvancedSections(AdvancedSectionsAs1);

            Assert.Multiple(() =>
            {
                Assert.That(returned, Is.SameAs(_service));
                Assert.That(GetPrivateList(_service, "_advancedSections"), Is.EqualTo(AdvancedSectionsAs1));
            });
        }

        [Test]
        public void AddIdentifier_SetsIdentifier()
        {
            var returned = _service.AddIdentifier("custom-area");

            Assert.Multiple(() =>
            {
                Assert.That(returned, Is.SameAs(_service));
                Assert.That(GetIdentifier(_service), Is.EqualTo("custom-area"));
            });
        }

        [Test]
        public void AddDefaultWidgets_AddsDefaultWidgets()
        {
            var returned = _service.AddDefaultWidgets();

            Assert.Multiple(() =>
            {
                Assert.That(returned, Is.SameAs(_service));
                Assert.That(GetPrivateList(_service, "_widgets"), Has.Count.EqualTo(9));
            });
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
                .AddWidgets(SingleA)
                .AddSections(SingleS)
                .AddIdentifier("zone1");

            Assert.Multiple(() =>
            {
                Assert.That(GetPrivateList(_service, "_widgets"), Is.EqualTo(SingleA));
                Assert.That(GetPrivateList(_service, "_sections"), Is.EqualTo(SingleS));
                Assert.That(GetIdentifier(_service), Is.EqualTo("zone1"));
            });
        }
    }
}
