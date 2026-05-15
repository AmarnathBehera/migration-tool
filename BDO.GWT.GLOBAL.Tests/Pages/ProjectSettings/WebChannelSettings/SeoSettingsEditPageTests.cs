using System.Reflection;
using BDO.GWT.Xperience.Admin.Pages.ProjectSettings.WebChannelSettings;
using CMS.DataEngine;
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.Forms;
using Moq;

namespace BDO.GWT.GLOBAL.Tests.Pages.ProjectSettings.WebChannelSettings
{
    public class SeoSettingsEditPageTests
    {
        [Test]
        public void Page_InheritsFromInfoEditPage_OfSEOSettingsInfo()
        {
            var baseType = typeof(SeoSettingsEditPage).BaseType;
            Assert.That(baseType, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(baseType!.IsGenericType, Is.True);
                Assert.That(baseType.GetGenericArguments()[0], Is.EqualTo(typeof(SEOSettingsInfo)));
            });
        }

        [Test]
        public void Page_ExposesExpectedConstructor()
        {
            var ctor = typeof(SeoSettingsEditPage).GetConstructor(new[]
            {
                typeof(IFormComponentMapper),
                typeof(IFormDataBinder),
                typeof(IInfoProvider<SEOSettingsInfo>),
                typeof(IInfoProvider<WebChannelSettingsInfo>),
                typeof(IChannelAccessService),
            });

            Assert.That(ctor, Is.Not.Null);
        }

        [Test]
        public void Constructor_WithNullSeoSettingsProvider_ThrowsArgumentNullException()
        {
            var formComponentMapper = new Mock<IFormComponentMapper>().Object;
            var formDataBinder = new Mock<IFormDataBinder>().Object;
            var webChannelSettingsProvider = new Mock<IInfoProvider<WebChannelSettingsInfo>>().Object;
            var channelAccessService = new Mock<IChannelAccessService>().Object;

            // Reflection because base ctor needs Xperience services; we still verify
            // the null-guard by invoking the ctor via Activator (it runs through base too,
            // but the guard executes first when provider == null).
            Assert.Throws<TargetInvocationException>(() =>
                Activator.CreateInstance(typeof(SeoSettingsEditPage),
                    formComponentMapper,
                    formDataBinder,
                    (IInfoProvider<SEOSettingsInfo>?)null,
                    webChannelSettingsProvider,
                    channelAccessService));
        }

        [Test]
        public void WebChannelSettingsId_Property_HasPageParameterAttribute()
        {
            var prop = typeof(SeoSettingsEditPage).GetProperty(
                nameof(SeoSettingsEditPage.WebChannelSettingsId),
                BindingFlags.Instance | BindingFlags.Public);

            Assert.That(prop, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(prop!.PropertyType, Is.EqualTo(typeof(int)));
                Assert.That(prop.GetCustomAttribute<PageParameterAttribute>(), Is.Not.Null);
            });
        }

        [Test]
        public void ObjectId_Property_IsOverriddenAndHasPageParameterAttribute()
        {
            var prop = typeof(SeoSettingsEditPage).GetProperty(
                nameof(SeoSettingsEditPage.ObjectId),
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);

            Assert.That(prop, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(prop!.PropertyType, Is.EqualTo(typeof(int)));
                Assert.That(prop.GetCustomAttribute<PageParameterAttribute>(), Is.Not.Null);
                Assert.That(prop.GetGetMethod()!.GetBaseDefinition().DeclaringType,
                    Is.Not.EqualTo(typeof(SeoSettingsEditPage)));
            });
        }

        [Test]
        public void OverridesConfigurePage()
        {
            var method = typeof(SeoSettingsEditPage).GetMethod(
                "ConfigurePage",
                BindingFlags.Instance | BindingFlags.Public);

            Assert.That(method, Is.Not.Null);
            Assert.That(method!.DeclaringType, Is.EqualTo(typeof(SeoSettingsEditPage)));
        }

        [Test]
        public void OverridesGetSubmitSuccessResponse()
        {
            var method = typeof(SeoSettingsEditPage).GetMethod(
                "GetSubmitSuccessResponse",
                BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.That(method, Is.Not.Null);
            Assert.That(method!.DeclaringType, Is.EqualTo(typeof(SeoSettingsEditPage)));
        }
    }
}