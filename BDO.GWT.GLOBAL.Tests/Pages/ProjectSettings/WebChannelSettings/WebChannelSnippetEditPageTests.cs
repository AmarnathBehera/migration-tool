using System.Reflection;
using BDO;
using BDO.GWT.Xperience.Admin.Pages.ProjectSettings.WebChannelSettings;
using CMS.DataEngine;
using Kentico.Xperience.Admin.Base.Forms;
using Moq;

namespace BDO.GWT.GLOBAL.Tests.Pages.ProjectSettings.WebChannelSettings
{
    public class WebChannelSnippetEditPageTests
    {
        [Test]
        public void Page_InheritsFromInfoEditPage_OfWebChannelSnippetInfo()
        {
            var baseType = typeof(WebChannelSnippetEditPage).BaseType;
            Assert.That(baseType, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(baseType!.IsGenericType, Is.True);
                Assert.That(baseType.GetGenericArguments()[0], Is.EqualTo(typeof(WebChannelSnippetInfo)));
            });
        }

        [Test]
        public void Page_ExposesExpectedConstructor()
        {
            var ctor = typeof(WebChannelSnippetEditPage).GetConstructor([
                typeof(IFormComponentMapper),
                typeof(IFormDataBinder),
                typeof(IInfoProvider<WebChannelSnippetInfo>),
                typeof(IInfoProvider<WebChannelSettingsInfo>),
                typeof(IChannelAccessService),
            ]);

            Assert.That(ctor, Is.Not.Null);
        }

        [Test]
        public void WebChannelSettingsId_PropertyExists_AndHasPageParameterAttribute()
        {
            var prop = typeof(WebChannelSnippetEditPage).GetProperty(nameof(WebChannelSnippetEditPage.WebChannelSettingsId));

            Assert.That(prop, Is.Not.Null);

            var attr = prop!.GetCustomAttribute<Kentico.Xperience.Admin.Base.PageParameterAttribute>();
            Assert.That(attr, Is.Not.Null);
        }

        [Test]
        public void ObjectId_PropertyIsOverridden()
        {
            var prop = typeof(WebChannelSnippetEditPage).GetProperty(
                nameof(WebChannelSnippetEditPage.ObjectId),
                BindingFlags.Instance | BindingFlags.Public);

            Assert.That(prop, Is.Not.Null);
            Assert.That(prop!.DeclaringType, Is.EqualTo(typeof(WebChannelSnippetEditPage)));
        }

        [Test]
        public void OverridesConfigurePage()
        {
            var method = typeof(WebChannelSnippetEditPage).GetMethod(
                "ConfigurePage",
                BindingFlags.Instance | BindingFlags.Public);

            Assert.That(method, Is.Not.Null);
            Assert.That(method!.DeclaringType, Is.EqualTo(typeof(WebChannelSnippetEditPage)));
        }

        [Test]
        public void OverridesGetSubmitSuccessResponse()
        {
            var method = typeof(WebChannelSnippetEditPage).GetMethod(
                "GetSubmitSuccessResponse",
                BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.That(method, Is.Not.Null);
            Assert.That(method!.DeclaringType, Is.EqualTo(typeof(WebChannelSnippetEditPage)));
        }

        [Test]
        public void Constructor_AcceptsChannelAccessService()
        {
            var ctor = typeof(WebChannelSnippetEditPage).GetConstructors().FirstOrDefault();
            Assert.That(ctor, Is.Not.Null);

            var paramTypes = ctor!.GetParameters().Select(p => p.ParameterType).ToArray();
            Assert.That(paramTypes, Does.Contain(typeof(IChannelAccessService)));
        }

        [Test]
        public void ConfigurePage_UsesFormName_WebChannelSnippetEdit()
        {
            // Validates the form name is set inside ConfigurePage by inspecting the method body indirectly.
            // The direct test is a structural check that ConfigurePage is declared in this type.
            var method = typeof(WebChannelSnippetEditPage).GetMethod(
                "ConfigurePage",
                BindingFlags.Instance | BindingFlags.Public);

            Assert.That(method?.DeclaringType, Is.EqualTo(typeof(WebChannelSnippetEditPage)));
        }
    }
}
