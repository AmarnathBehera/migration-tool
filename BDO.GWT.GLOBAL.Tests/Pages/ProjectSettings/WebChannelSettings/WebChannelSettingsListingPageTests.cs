using System.Reflection;
using BDO;
using BDO.GWT.Xperience.Admin.Pages.ProjectSettings.WebChannelSettings;
using CMS.ContentEngine;
using CMS.DataEngine;
using Kentico.Xperience.Admin.Base;

namespace BDO.GWT.GLOBAL.Tests.Pages.ProjectSettings.WebChannelSettings
{
    [TestFixture]
    public class WebChannelSettingsListingPageTests
    {
        [Test]
        public void Page_InheritsFromListingPage()
        {
            Assert.That(typeof(WebChannelSettingsListingPage).BaseType, Is.EqualTo(typeof(ListingPage)));
        }

        [Test]
        public void Page_IsPublicClass()
        {
            Assert.That(typeof(WebChannelSettingsListingPage).IsPublic, Is.True);
        }

        [Test]
        public void Page_IsNotAbstract()
        {
            Assert.That(typeof(WebChannelSettingsListingPage).IsAbstract, Is.False);
        }

        [Test]
        public void Page_HasSinglePublicConstructor()
        {
            Assert.That(typeof(WebChannelSettingsListingPage).GetConstructors(), Has.Length.EqualTo(1));
        }

        [Test]
        public void Constructor_HasExpectedParameterCount()
        {
            var ctor = typeof(WebChannelSettingsListingPage).GetConstructors().Single();

            Assert.That(ctor.GetParameters(), Has.Length.EqualTo(3));
        }

        [Test]
        public void Constructor_HasExpectedParameterTypes()
        {
            var ctor = typeof(WebChannelSettingsListingPage).GetConstructors().Single();
            var parameters = ctor.GetParameters();

            Assert.Multiple(() =>
            {
                Assert.That(parameters[0].ParameterType, Is.EqualTo(typeof(IInfoProvider<ChannelInfo>)));
                Assert.That(parameters[1].ParameterType, Is.EqualTo(typeof(IInfoProvider<WebChannelSettingsInfo>)));
                Assert.That(parameters[2].ParameterType, Is.EqualTo(typeof(IChannelAccessService)));
            });
        }

        [Test]
        public void ObjectType_PropertyIsOverridden()
        {
            var property = typeof(WebChannelSettingsListingPage).GetProperty(
                "ObjectType",
                BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.That(property, Is.Not.Null);
            Assert.That(property!.DeclaringType, Is.EqualTo(typeof(WebChannelSettingsListingPage)));
        }

        [Test]
        public void ObjectType_ReturnsWebChannelSettingsObjectType()
        {
            var property = typeof(WebChannelSettingsListingPage).GetProperty(
                "ObjectType",
                BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.That(property, Is.Not.Null);
            Assert.That(property!.PropertyType, Is.EqualTo(typeof(string)));
        }

        [Test]
        public void ConfigurePage_IsOverridden()
        {
            var method = typeof(WebChannelSettingsListingPage).GetMethod(
                nameof(WebChannelSettingsListingPage.ConfigurePage),
                BindingFlags.Instance | BindingFlags.Public);

            Assert.That(method, Is.Not.Null);
            Assert.That(method!.DeclaringType, Is.EqualTo(typeof(WebChannelSettingsListingPage)));
        }

        [Test]
        public void ConfigurePage_ReturnsTask()
        {
            var method = typeof(WebChannelSettingsListingPage).GetMethod(
                nameof(WebChannelSettingsListingPage.ConfigurePage));

            Assert.That(method, Is.Not.Null);
            Assert.That(method!.ReturnType, Is.EqualTo(typeof(Task)));
        }

        [Test]
        public void ConfigurePage_HasNoParameters()
        {
            var method = typeof(WebChannelSettingsListingPage).GetMethod(
                nameof(WebChannelSettingsListingPage.ConfigurePage));

            Assert.That(method, Is.Not.Null);
            Assert.That(method!.GetParameters(), Has.Length.EqualTo(0));
        }

        [Test]
        public void EnsureSettingsListData_MethodExists()
        {
            var method = typeof(WebChannelSettingsListingPage).GetMethod(
                "EnsureSettingsListData",
                BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.That(method, Is.Not.Null);
        }

        [Test]
        public void EnsureChannelSetting_MethodExistsWithExpectedParameters()
        {
            var method = typeof(WebChannelSettingsListingPage).GetMethod(
                "EnsureChannelSetting",
                BindingFlags.Instance | BindingFlags.NonPublic);

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
        public void EnsureChannelSettingDisplayName_MethodExistsWithExpectedParameters()
        {
            var method = typeof(WebChannelSettingsListingPage).GetMethod(
                "EnsureChannelSettingDisplayName",
                BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.That(method, Is.Not.Null);

            var parameters = method!.GetParameters();

            Assert.Multiple(() =>
            {
                Assert.That(parameters, Has.Length.EqualTo(2));
                Assert.That(parameters[0].ParameterType, Is.EqualTo(typeof(ChannelInfo)));
                Assert.That(parameters[1].ParameterType, Is.EqualTo(typeof(WebChannelSettingsInfo)));
            });
        }
    }
}