using System.Collections.Generic;
using System.Linq;
using BDO;
using BDO.GWT.GLOBAL.Services;
using NUnit.Framework;

namespace BDO.GWT.GLOBAL.Tests.Services
{
    public class PageBuilderSectionServiceTests
    {
        private PageBuilderSectionService _service = null!;

        [SetUp]
        public void Setup()
        {
            _service = new PageBuilderSectionService();
        }

        [Test]
        public void GetAllowedWidgets_ReturnsExplicitList_ForSectionWithExplicitConfiguration()
        {
            var expected = new List<string>
            {
                ComponentIdentifiers.Widgets.Button,
                ComponentIdentifiers.Widgets.SocialMediaButtons,
                ComponentIdentifiers.Widgets.HTML,
                ComponentIdentifiers.Widgets.RichText,
            };

            var result = _service.GetAllowedWidgets(ComponentIdentifiers.Sections.FourColumns);

            Assert.That(result, Is.EquivalentTo(expected));
        }

        [Test]
        public void GetAllowedWidgets_ReturnsExplicitList_ForFiveColumnsSection()
        {
            var expected = new List<string>
            {
                ComponentIdentifiers.Widgets.Button,
                ComponentIdentifiers.Widgets.SocialMediaButtons,
                ComponentIdentifiers.Widgets.HTML,
                ComponentIdentifiers.Widgets.RichText,
            };

            var result = _service.GetAllowedWidgets(ComponentIdentifiers.Sections.FiveColumns);

            Assert.That(result, Is.EquivalentTo(expected));
        }

        [Test]
        public void GetAllowedWidgets_ReturnsExactCopy_NotSharedReference()
        {
            var first = _service.GetAllowedWidgets(ComponentIdentifiers.Sections.FourColumns);
            first.Add("mutation");

            var second = _service.GetAllowedWidgets(ComponentIdentifiers.Sections.FourColumns);

            Assert.That(second, Does.Not.Contain("mutation"));
        }

        [Test]
        public void GetAllowedWidgets_IncludesSocialMediaButtons_ForDefaultSection()
        {
            var result = _service.GetAllowedWidgets(ComponentIdentifiers.Sections.DefaultSection);

            Assert.That(result, Does.Contain(ComponentIdentifiers.Widgets.SocialMediaButtons));
        }

        [Test]
        public void GetAllowedWidgets_IncludesSocialMediaButtons_ForContainerSection()
        {
            var result = _service.GetAllowedWidgets(ComponentIdentifiers.Sections.Container);

            Assert.That(result, Does.Contain(ComponentIdentifiers.Widgets.SocialMediaButtons));
        }

        [Test]
        public void GetAllowedWidgets_ExcludesSocialMediaButtons_ForSectionNotInItsAllowedList()
        {
            // SocialMediaButtons is only allowed in DefaultSection and Container.
            // Use a section identifier that has no explicit widget configuration.
            var result = _service.GetAllowedWidgets("BDO.Sections.SomeUnconfiguredSection");

            Assert.That(result, Does.Not.Contain(ComponentIdentifiers.Widgets.SocialMediaButtons));
        }

        [Test]
        public void GetAllowedWidgets_ReturnsRegisteredWidgets_ForSectionWithoutExplicitConfiguration()
        {
            var result = _service.GetAllowedWidgets("BDO.Sections.SomeUnconfiguredSection");

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Not.Empty);
        }

        [Test]
        public void GetAllowedWidgets_IsDeterministic_ForSameSection()
        {
            var first = _service.GetAllowedWidgets("BDO.Sections.SomeUnconfiguredSection");
            var second = _service.GetAllowedWidgets("BDO.Sections.SomeUnconfiguredSection");

            Assert.That(first, Is.EquivalentTo(second));
        }
    }
}
