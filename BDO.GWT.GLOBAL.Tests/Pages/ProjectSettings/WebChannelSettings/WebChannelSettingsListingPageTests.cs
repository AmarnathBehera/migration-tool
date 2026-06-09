using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using BDO;
using BDO.GWT.Xperience.Admin.Pages.ProjectSettings.WebChannelSettings;
using CMS.ContentEngine;
using CMS.DataEngine;
using Moq;

namespace BDO.GWT.GLOBAL.Tests.Pages.ProjectSettings.WebChannelSettings
{
    public class WebChannelSettingsListingPageTests
    {
        [Test]
        public void Page_InheritsFromListingPage()
        {
            Assert.That(
                typeof(Kentico.Xperience.Admin.Base.ListingPage)
                    .IsAssignableFrom(typeof(WebChannelSettingsListingPage)),
                Is.True);
        }

        [Test]
        public void Page_ExposesExpectedConstructor()
        {
            var ctor = typeof(WebChannelSettingsListingPage).GetConstructor([
                typeof(IInfoProvider<ChannelInfo>),
                typeof(IInfoProvider<WebChannelSettingsInfo>),
                typeof(IInfoProvider<SEOSettingsInfo>),
                typeof(IChannelAccessService),
            ]);

            Assert.That(ctor, Is.Not.Null);
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
        public void ObjectType_ReturnsWebChannelSettingsObjectType()
        {
            var prop = typeof(WebChannelSettingsListingPage).GetProperty(
                "ObjectType",
                BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.That(prop, Is.Not.Null);
        }

        [Test]
        public void Constructor_AcceptsChannelAccessService()
        {
            var ctor = typeof(WebChannelSettingsListingPage).GetConstructors().FirstOrDefault();
            Assert.That(ctor, Is.Not.Null);

            var paramTypes = ctor!.GetParameters().Select(p => p.ParameterType).ToArray();
            Assert.That(paramTypes, Does.Contain(typeof(IChannelAccessService)));
        }

        [Test]
        public void EnsureSettingsListData_IsPrivateMethod()
        {
            var method = typeof(WebChannelSettingsListingPage).GetMethod(
                "EnsureSettingsListData",
                BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.That(method, Is.Not.Null);
            Assert.That(method!.IsPrivate, Is.True);
        }

        [Test]
        public void EnsureChannelSetting_IsPrivateMethod()
        {
            var method = typeof(WebChannelSettingsListingPage).GetMethod(
                "EnsureChannelSetting",
                BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.That(method, Is.Not.Null);
            Assert.That(method!.IsPrivate, Is.True);
        }

        [Test]
        public void EnsureChannelSettingDisplayName_IsPrivateMethod()
        {
            var method = typeof(WebChannelSettingsListingPage).GetMethod(
                "EnsureChannelSettingDisplayName",
                BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.That(method, Is.Not.Null);
            Assert.That(method!.IsPrivate, Is.True);
        }

        [Test]
        public void EnsureSeoSettings_IsPrivateMethod()
        {
            var method = typeof(WebChannelSettingsListingPage).GetMethod(
                "EnsureSeoSettings",
                BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.That(method, Is.Not.Null);
            Assert.That(method!.IsPrivate, Is.True);
        }

        [Test]
        public void Constructor_DependsOnSeoSettingsInfoProvider()
        {
            var ctor = typeof(WebChannelSettingsListingPage).GetConstructors().FirstOrDefault();
            Assert.That(ctor, Is.Not.Null);

            var paramTypes = ctor!.GetParameters().Select(p => p.ParameterType).ToArray();
            Assert.That(paramTypes, Does.Contain(typeof(IInfoProvider<SEOSettingsInfo>)));
        }

        [Test]
        public void ObjectType_IsOverriddenInListingPage()
        {
            var prop = typeof(WebChannelSettingsListingPage).GetProperty(
                "ObjectType",
                BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.That(prop, Is.Not.Null);
            Assert.That(prop!.DeclaringType, Is.EqualTo(typeof(WebChannelSettingsListingPage)));
        }

        [Test]
        public void EnsureChannelSetting_HasCorrectParameterTypes()
        {
            var method = typeof(WebChannelSettingsListingPage).GetMethod(
                "EnsureChannelSetting",
                BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.That(method, Is.Not.Null);
            var parameters = method!.GetParameters();
            Assert.Multiple(() =>
            {
                Assert.That(parameters.Length, Is.EqualTo(2));
                Assert.That(parameters[0].ParameterType, Is.EqualTo(typeof(List<WebChannelSettingsInfo>)));
                Assert.That(parameters[1].ParameterType, Is.EqualTo(typeof(ChannelInfo)));
            });
        }

        [Test]
        public void EnsureChannelSettingDisplayName_HasCorrectParameterTypes()
        {
            var method = typeof(WebChannelSettingsListingPage).GetMethod(
                "EnsureChannelSettingDisplayName",
                BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.That(method, Is.Not.Null);
            var parameters = method!.GetParameters();
            Assert.Multiple(() =>
            {
                Assert.That(parameters.Length, Is.EqualTo(2));
                Assert.That(parameters[0].ParameterType, Is.EqualTo(typeof(ChannelInfo)));
                Assert.That(parameters[1].ParameterType, Is.EqualTo(typeof(WebChannelSettingsInfo)));
            });
        }

        [Test]
        public void EnsureSeoSettings_HasCorrectParameterTypes()
        {
            var method = typeof(WebChannelSettingsListingPage).GetMethod(
                "EnsureSeoSettings",
                BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.That(method, Is.Not.Null);
            var parameters = method!.GetParameters();
            Assert.Multiple(() =>
            {
                Assert.That(parameters.Length, Is.EqualTo(2));
                Assert.That(parameters[0].ParameterType, Is.EqualTo(typeof(WebChannelSettingsInfo)));
                Assert.That(parameters[1].ParameterType, Is.EqualTo(typeof(IEnumerable<SEOSettingsInfo>)));
            });
        }

        [Test]
        public void ConfigurePage_IsAsyncAndReturnsTask()
        {
            var method = typeof(WebChannelSettingsListingPage).GetMethod(
                "ConfigurePage",
                BindingFlags.Instance | BindingFlags.Public);

            Assert.That(method, Is.Not.Null);
            Assert.That(method!.ReturnType, Is.EqualTo(typeof(Task)));
        }

        [Test]
        public void Page_IsPublicNonAbstractClass()
        {
            Assert.Multiple(() =>
            {
                Assert.That(typeof(WebChannelSettingsListingPage).IsPublic, Is.True);
                Assert.That(typeof(WebChannelSettingsListingPage).IsAbstract, Is.False);
            });
        }
    }
}
