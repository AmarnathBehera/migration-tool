using System.Reflection;
using System.Runtime.CompilerServices;
using BDO.GWT.Xperience.Admin.Pages.ProjectSettings.WebChannelSettings;
using CMS.DataEngine;
using Kentico.Xperience.Admin.Base;

namespace BDO.GWT.GLOBAL.Tests.Pages.ProjectSettings.WebChannelSettings
{
    public class WebChannelSnippetListingPageTests
    {
        [Test]
        public void Page_InheritsFromListingPage()
        {
            var baseType = typeof(WebChannelSnippetListingPage).BaseType;

            Assert.That(baseType, Is.Not.Null);
            Assert.That(baseType, Is.EqualTo(typeof(ListingPage)));
        }

        [Test]
        public void ObjectType_ReturnsWebChannelSnippetInfoObjectType()
        {
            var prop = typeof(WebChannelSnippetListingPage).GetProperty(
                "ObjectType",
                BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.That(prop, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(prop!.PropertyType, Is.EqualTo(typeof(string)));
                Assert.That(prop.DeclaringType, Is.EqualTo(typeof(WebChannelSnippetListingPage)));
            });

            // Validate the value via an uninitialized instance (avoids running base ctor).
            var instance = (WebChannelSnippetListingPage)RuntimeHelpers
                .GetUninitializedObject(typeof(WebChannelSnippetListingPage));

            var value = prop!.GetValue(instance) as string;
            Assert.That(value, Is.EqualTo(WebChannelSnippetInfo.OBJECT_TYPE));
        }

        [Test]
        public void WebChannelSettingsId_Property_HasPageParameterAttribute()
        {
            var prop = typeof(WebChannelSnippetListingPage).GetProperty(
                nameof(WebChannelSnippetListingPage.WebChannelSettingsId),
                BindingFlags.Instance | BindingFlags.Public);

            Assert.That(prop, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(prop!.PropertyType, Is.EqualTo(typeof(int)));
                Assert.That(prop.GetCustomAttribute<PageParameterAttribute>(), Is.Not.Null);
            });
        }

        [Test]
        public void OverridesConfigurePage()
        {
            var method = typeof(WebChannelSnippetListingPage).GetMethod(
                "ConfigurePage",
                BindingFlags.Instance | BindingFlags.Public);

            Assert.That(method, Is.Not.Null);
            Assert.That(method!.DeclaringType, Is.EqualTo(typeof(WebChannelSnippetListingPage)));
        }

        [Test]
        public void OverridesDelete_AndHasPageCommandAttributeWithDeletePermission()
        {
            var method = typeof(WebChannelSnippetListingPage).GetMethod(
                nameof(WebChannelSnippetListingPage.Delete),
                BindingFlags.Instance | BindingFlags.Public);

            Assert.That(method, Is.Not.Null);
            Assert.That(method!.DeclaringType, Is.EqualTo(typeof(WebChannelSnippetListingPage)));

            var pageCommand = method.GetCustomAttribute<PageCommandAttribute>();
            Assert.That(pageCommand, Is.Not.Null);
            Assert.That(pageCommand!.Permission, Is.EqualTo(CMS.Membership.SystemPermissions.DELETE));
        }
    }
}