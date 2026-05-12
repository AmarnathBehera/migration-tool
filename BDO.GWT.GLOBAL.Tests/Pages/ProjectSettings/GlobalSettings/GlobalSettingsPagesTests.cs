using System.Reflection;
using BDO.GWT.Xperience.Admin.Pages.ProjectSettings.GlobalSettings;
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.Forms;

namespace BDO.GWT.GLOBAL.Tests.Pages.ProjectSettings.GlobalSettings
{
    public class GlobalSettingsCreatePageTests
    {
        [Test]
        public void Page_InheritsFromCreatePage()
        {
            var baseType = typeof(GlobalSettingsCreatePage).BaseType;
            Assert.That(baseType, Is.Not.Null);
            Assert.That(baseType!.IsGenericType, Is.True);
            Assert.Multiple(() =>
            {
                Assert.That(baseType.GetGenericArguments()[0], Is.EqualTo(typeof(GlobalsettingskeyInfo)));
                Assert.That(baseType.GetGenericArguments()[1], Is.EqualTo(typeof(GlobalSettingsEditPage)));
            });
        }

        [Test]
        public void Page_ExposesExpectedConstructor()
        {
            var ctor = typeof(GlobalSettingsCreatePage).GetConstructor(new[]
            {
                typeof(IFormComponentMapper),
                typeof(IFormDataBinder),
                typeof(IPageLinkGenerator),
            });

            Assert.That(ctor, Is.Not.Null);
        }

        [Test]
        public void OverridesConfigurePage()
        {
            var method = typeof(GlobalSettingsCreatePage).GetMethod(
                "ConfigurePage",
                BindingFlags.Instance | BindingFlags.Public);

            Assert.That(method, Is.Not.Null);
            Assert.That(method!.DeclaringType, Is.EqualTo(typeof(GlobalSettingsCreatePage)));
        }
    }

    public class GlobalSettingsEditPageTests
    {
        [Test]
        public void Page_InheritsFromInfoEditPage()
        {
            var baseType = typeof(GlobalSettingsEditPage).BaseType;
            Assert.That(baseType, Is.Not.Null);
            Assert.That(baseType!.IsGenericType, Is.True);
            Assert.That(baseType.GetGenericArguments()[0], Is.EqualTo(typeof(GlobalsettingskeyInfo)));
        }

        [Test]
        public void Page_ExposesExpectedConstructor()
        {
            var ctor = typeof(GlobalSettingsEditPage).GetConstructor(new[]
            {
                typeof(IFormComponentMapper),
                typeof(IFormDataBinder),
            });

            Assert.That(ctor, Is.Not.Null);
        }

        [Test]
        public void ObjectId_HasPageParameterAttribute()
        {
            var prop = typeof(GlobalSettingsEditPage).GetProperty(nameof(GlobalSettingsEditPage.ObjectId));
            Assert.That(prop, Is.Not.Null);

            var attr = prop!.GetCustomAttribute<PageParameterAttribute>();
            Assert.That(attr, Is.Not.Null);
        }

        [Test]
        public void OverridesConfigurePage()
        {
            var method = typeof(GlobalSettingsEditPage).GetMethod(
                "ConfigurePage",
                BindingFlags.Instance | BindingFlags.Public);

            Assert.That(method, Is.Not.Null);
            Assert.That(method!.DeclaringType, Is.EqualTo(typeof(GlobalSettingsEditPage)));
        }
    }

    public class GlobalSettingsEditSectionTests
    {
        [Test]
        public void Section_InheritsFromEditSectionPage()
        {
            var baseType = typeof(GlobalSettingsEditSection).BaseType;
            Assert.That(baseType, Is.Not.Null);
            Assert.That(baseType!.IsGenericType, Is.True);
            Assert.That(baseType.GetGenericArguments()[0], Is.EqualTo(typeof(GlobalsettingskeyInfo)));
        }
    }

    public class GlobalSettingsListingPageTests
    {
        [Test]
        public void Page_InheritsFromListingPage()
        {
            Assert.That(typeof(ListingPage).IsAssignableFrom(typeof(GlobalSettingsListingPage)), Is.True);
        }

        [Test]
        public void OverridesConfigurePage()
        {
            var method = typeof(GlobalSettingsListingPage).GetMethod(
                "ConfigurePage",
                BindingFlags.Instance | BindingFlags.Public);

            Assert.That(method, Is.Not.Null);
            Assert.That(method!.DeclaringType, Is.EqualTo(typeof(GlobalSettingsListingPage)));
        }

        [Test]
        public void OverridesDelete()
        {
            var method = typeof(GlobalSettingsListingPage).GetMethod(
                "Delete",
                BindingFlags.Instance | BindingFlags.Public);

            Assert.That(method, Is.Not.Null);
            Assert.That(method!.DeclaringType, Is.EqualTo(typeof(GlobalSettingsListingPage)));

            var attr = method.GetCustomAttribute<PageCommandAttribute>();
            Assert.That(attr, Is.Not.Null,
                "Delete must be decorated with PageCommandAttribute for permission enforcement.");
        }
    }
}
