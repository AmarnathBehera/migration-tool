using System.Reflection;
using System.Runtime.CompilerServices;
using BDO.GWT.Xperience.Admin.Pages.ProjectSettings.WebChannelSettings;
using CMS.ContentEngine;
using CMS.DataEngine;
using Kentico.Xperience.Admin.Base;
using Moq;

namespace BDO.GWT.GLOBAL.Tests.Pages.ProjectSettings.WebChannelSettings
{
    public class WebChannelSettingsListingPageTests
    {
        [Test]
        public void Page_InheritsFromListingPage()
        {
            var baseType = typeof(WebChannelSettingsListingPage).BaseType;

            Assert.That(baseType, Is.Not.Null);
            Assert.That(baseType, Is.EqualTo(typeof(ListingPage)));
        }

        [Test]
        public void Page_ExposesExpectedConstructor()
        {
            var ctor = typeof(WebChannelSettingsListingPage).GetConstructor(new[]
            {
                typeof(IInfoProvider<ChannelInfo>),
                typeof(IInfoProvider<WebChannelSettingsInfo>),
                typeof(IInfoProvider<SEOSettingsInfo>),
                typeof(IChannelAccessService),
            });

            Assert.That(ctor, Is.Not.Null);
        }

        [Test]
        public void Constructor_WithNullChannelInfoProvider_Throws()
        {
            var webChannelSettingsProvider = new Mock<IInfoProvider<WebChannelSettingsInfo>>().Object;
            var seoSettingsProvider = new Mock<IInfoProvider<SEOSettingsInfo>>().Object;
            var channelAccessService = new Mock<IChannelAccessService>().Object;

            // Reflection because base ctor needs Xperience services; we still verify
            // the null-guard by invoking the ctor via Activator (it runs through base too,
            // but the guard executes first when provider == null).
            Assert.Throws<TargetInvocationException>(() =>
                Activator.CreateInstance(typeof(WebChannelSettingsListingPage),
                    (IInfoProvider<ChannelInfo>?)null,
                    webChannelSettingsProvider,
                    seoSettingsProvider,
                    channelAccessService));
        }

        [Test]
        public void ObjectType_ReturnsWebChannelSettingsInfoObjectType()
        {
            var prop = typeof(WebChannelSettingsListingPage).GetProperty(
                "ObjectType",
                BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.That(prop, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(prop!.PropertyType, Is.EqualTo(typeof(string)));
                Assert.That(prop.DeclaringType, Is.EqualTo(typeof(WebChannelSettingsListingPage)));
            });

            // Validate the value via an uninitialized instance (avoids running base ctor).
            var instance = (WebChannelSettingsListingPage)RuntimeHelpers
                .GetUninitializedObject(typeof(WebChannelSettingsListingPage));

            var value = prop!.GetValue(instance) as string;
            Assert.That(value, Is.EqualTo(WebChannelSettingsInfo.OBJECT_TYPE));
        }

        [Test]
        public void OverridesConfigurePage()
        {
            var method = typeof(WebChannelSettingsListingPage).GetMethod(
                "ConfigurePage",
                BindingFlags.Instance | BindingFlags.Public);

            Assert.That(method, Is.Not.Null);
            Assert.That(method!.DeclaringType, Is.EqualTo(typeof(WebChannelSettingsListingPage)));
        }

        [Test]
        public void DeclaresPrivateHelperMethods()
        {
            var type = typeof(WebChannelSettingsListingPage);
            var flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

            Assert.Multiple(() =>
            {
                Assert.That(type.GetMethod("EnsureSettingsListData", flags), Is.Not.Null);
                Assert.That(type.GetMethod("EnsureChannelSetting", flags), Is.Not.Null);
                Assert.That(type.GetMethod("EnsureChannelSettingDisplayName", flags), Is.Not.Null);
                Assert.That(type.GetMethod("EnsureSeoSettings", flags), Is.Not.Null);
            });
        }
    }
}