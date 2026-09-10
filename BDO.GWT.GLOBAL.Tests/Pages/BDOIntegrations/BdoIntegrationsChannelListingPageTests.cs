using BDO.GWT.Xperience.Admin.Pages.ProjectSettings.WebChannelSettings;
using CMS.ContentEngine;
using CMS.DataEngine;
using GWTXbyKWeb.Pages.BDOIntegrations;
using Kentico.Xperience.Admin.Base;
using System.Reflection;

namespace BDO.GWT.GLOBAL.Tests.Pages.BDOIntegrations
{
    [TestFixture]
    public class BdoIntegrationsChannelListingPageTests
    {
        private const BindingFlags InstanceNonPublic = BindingFlags.Instance | BindingFlags.NonPublic;

        [Test]
        public void InheritsFromListingPage()
        {
            Assert.That(typeof(BdoIntegrationsChannelListingPage).BaseType, Is.EqualTo(typeof(ListingPage)));
        }

        [Test]
        public void ExposesExpectedConstructor()
        {
            var constructor = typeof(BdoIntegrationsChannelListingPage).GetConstructor(new[]
            {
                typeof(IInfoProvider<ChannelInfo>),
                typeof(IInfoProvider<WebChannelSettingsInfo>),
                typeof(IChannelAccessService),
            });

            Assert.That(constructor, Is.Not.Null);
        }

        [Test]
        public void DeclaresExpectedInjectedDependencies()
        {
            var type = typeof(BdoIntegrationsChannelListingPage);

            Assert.Multiple(() =>
            {
                Assert.That(type.GetField("channelInfoProvider", InstanceNonPublic)?.FieldType, Is.EqualTo(typeof(IInfoProvider<ChannelInfo>)));
                Assert.That(type.GetField("webChannelSettingsInfoProvider", InstanceNonPublic)?.FieldType, Is.EqualTo(typeof(IInfoProvider<WebChannelSettingsInfo>)));
                Assert.That(type.GetField("channelAccessService", InstanceNonPublic)?.FieldType, Is.EqualTo(typeof(IChannelAccessService)));
            });
        }

        [Test]
        public void ObjectType_ReturnsWebChannelSettingsObjectType()
        {
            // The constructor performs a database sync, so the instance is created without running it.
            var page = BdoIntegrationsPageTestHelper.CreateUninitialized<BdoIntegrationsChannelListingPage>();

            var objectType = typeof(BdoIntegrationsChannelListingPage)
                .GetProperty("ObjectType", InstanceNonPublic)
                ?.GetValue(page) as string;

            Assert.That(objectType, Is.EqualTo(WebChannelSettingsInfo.OBJECT_TYPE));
        }

        [Test]
        public void ConfigurePage_IsOverridden()
        {
            var method = typeof(BdoIntegrationsChannelListingPage).GetMethod(
                nameof(BdoIntegrationsChannelListingPage.ConfigurePage),
                BindingFlags.Instance | BindingFlags.Public);

            Assert.That(method, Is.Not.Null);
            Assert.That(method!.DeclaringType, Is.EqualTo(typeof(BdoIntegrationsChannelListingPage)));
        }

        [Test]
        public void DeclaresSelfHealingHelperMethods()
        {
            var type = typeof(BdoIntegrationsChannelListingPage);

            Assert.Multiple(() =>
            {
                Assert.That(type.GetMethod("EnsureSettingsListData", InstanceNonPublic), Is.Not.Null);
                Assert.That(type.GetMethod("EnsureChannelSetting", InstanceNonPublic), Is.Not.Null);
                Assert.That(type.GetMethod("EnsureChannelSettingDisplayName", InstanceNonPublic), Is.Not.Null);
            });
        }

        [Test]
        public void EnsureChannelSetting_HasExpectedSignature()
        {
            var method = typeof(BdoIntegrationsChannelListingPage).GetMethod("EnsureChannelSetting", InstanceNonPublic);

            Assert.That(method, Is.Not.Null);

            var parameters = method!.GetParameters();

            Assert.Multiple(() =>
            {
                Assert.That(parameters, Has.Length.EqualTo(2));
                Assert.That(parameters[0].ParameterType, Is.EqualTo(typeof(List<WebChannelSettingsInfo>)));
                Assert.That(parameters[1].ParameterType, Is.EqualTo(typeof(ChannelInfo)));
            });
        }

        [Test]
        public void EnsureChannelSettingDisplayName_HasExpectedSignature()
        {
            var method = typeof(BdoIntegrationsChannelListingPage).GetMethod("EnsureChannelSettingDisplayName", InstanceNonPublic);

            Assert.That(method, Is.Not.Null);

            var parameters = method!.GetParameters();

            Assert.Multiple(() =>
            {
                Assert.That(parameters, Has.Length.EqualTo(2));
                Assert.That(parameters[0].ParameterType, Is.EqualTo(typeof(ChannelInfo)));
                Assert.That(parameters[1].ParameterType, Is.EqualTo(typeof(WebChannelSettingsInfo)));
            });
        }

        [Test]
        public void ConfigurePage_ReturnsTask()
        {
            var method = typeof(BdoIntegrationsChannelListingPage).GetMethod(
                nameof(BdoIntegrationsChannelListingPage.ConfigurePage),
                BindingFlags.Instance | BindingFlags.Public);

            Assert.That(method!.ReturnType, Is.EqualTo(typeof(Task)));
        }
    }
}
