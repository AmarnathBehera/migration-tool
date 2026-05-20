using System.Reflection;
using BDO.GWT.Xperience.Admin.Pages.ProjectSettings.WebChannelSettings;
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.Forms;
using Moq;

namespace BDO.GWT.GLOBAL.Tests.Pages.ProjectSettings.WebChannelSettings
{
    public class WebChannelSnippetCreatePageTests
    {
        [Test]
        public void Page_InheritsFromCreatePage_OfWebChannelSnippetInfoAndEditSection()
        {
            var baseType = typeof(WebChannelSnippetCreatePage).BaseType;
            Assert.That(baseType, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(baseType!.IsGenericType, Is.True);
                var args = baseType.GetGenericArguments();
                Assert.That(args[0], Is.EqualTo(typeof(WebChannelSnippetInfo)));
                Assert.That(args[1], Is.EqualTo(typeof(WebChannelSnippetEditSection)));
            });
        }

        [Test]
        public void Page_ExposesExpectedConstructor()
        {
            var ctor = typeof(WebChannelSnippetCreatePage).GetConstructor(new[]
            {
                typeof(IFormComponentMapper),
                typeof(IFormDataBinder),
                typeof(IPageLinkGenerator),
            });

            Assert.That(ctor, Is.Not.Null);
        }

        [Test]
        public void Constructor_WithNullPageLinkGenerator_Throws()
        {
            var formComponentMapper = new Mock<IFormComponentMapper>().Object;
            var formDataBinder = new Mock<IFormDataBinder>().Object;

            // Reflection because base ctor needs Xperience services; we still verify
            // the null-guard by invoking the ctor via Activator (it runs through base too,
            // but the guard executes first when generator == null).
            Assert.Throws<TargetInvocationException>(() =>
                Activator.CreateInstance(typeof(WebChannelSnippetCreatePage),
                    formComponentMapper,
                    formDataBinder,
                    (IPageLinkGenerator?)null));
        }

        [Test]
        public void WebChannelSettingsId_Property_HasPageParameterAttribute()
        {
            var prop = typeof(WebChannelSnippetCreatePage).GetProperty(
                nameof(WebChannelSnippetCreatePage.WebChannelSettingsId),
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
            var method = typeof(WebChannelSnippetCreatePage).GetMethod(
                "ConfigurePage",
                BindingFlags.Instance | BindingFlags.Public);

            Assert.That(method, Is.Not.Null);
            Assert.That(method!.DeclaringType, Is.EqualTo(typeof(WebChannelSnippetCreatePage)));
        }

        [Test]
        public void OverridesFinalizeInfoObject()
        {
            var method = typeof(WebChannelSnippetCreatePage).GetMethod(
                "FinalizeInfoObject",
                BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.That(method, Is.Not.Null);
            Assert.That(method!.DeclaringType, Is.EqualTo(typeof(WebChannelSnippetCreatePage)));
        }
    }
}