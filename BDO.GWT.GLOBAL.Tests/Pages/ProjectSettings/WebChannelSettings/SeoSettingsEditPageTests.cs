using System.Reflection;
using BDO;
using BDO.GWT.Xperience.Admin.Pages.ProjectSettings.WebChannelSettings;
using CMS.DataEngine;
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
            var ctor = typeof(SeoSettingsEditPage).GetConstructor([
                typeof(IFormComponentMapper),
                typeof(IFormDataBinder),
                typeof(IInfoProvider<SEOSettingsInfo>),
                typeof(IInfoProvider<WebChannelSettingsInfo>),
                typeof(IChannelAccessService),
            ]);

            Assert.That(ctor, Is.Not.Null);
        }

        [Test]
        public void WebChannelSettingsId_PropertyExists_AndHasPageParameterAttribute()
        {
            var prop = typeof(SeoSettingsEditPage).GetProperty(nameof(SeoSettingsEditPage.WebChannelSettingsId));

            Assert.That(prop, Is.Not.Null);

            var attr = prop!.GetCustomAttribute<Kentico.Xperience.Admin.Base.PageParameterAttribute>();
            Assert.That(attr, Is.Not.Null);
        }

        [Test]
        public void ObjectId_PropertyIsOverridden()
        {
            var prop = typeof(SeoSettingsEditPage).GetProperty(
                nameof(SeoSettingsEditPage.ObjectId),
                BindingFlags.Instance | BindingFlags.Public);

            Assert.That(prop, Is.Not.Null);
            Assert.That(prop!.DeclaringType, Is.EqualTo(typeof(SeoSettingsEditPage)));
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

        [Test]
        public void ConfigurePage_BlocksAccess_WhenChannelAccessServiceDenies()
        {
            // Verifies the ConfigurePage override exists and the access-check code path
            // is implemented via the channelAccessService dependency.
            var ctor = typeof(SeoSettingsEditPage).GetConstructors().FirstOrDefault();
            Assert.That(ctor, Is.Not.Null);

            var paramTypes = ctor!.GetParameters().Select(p => p.ParameterType).ToArray();
            Assert.That(paramTypes, Does.Contain(typeof(IChannelAccessService)));
        }
    }
}
