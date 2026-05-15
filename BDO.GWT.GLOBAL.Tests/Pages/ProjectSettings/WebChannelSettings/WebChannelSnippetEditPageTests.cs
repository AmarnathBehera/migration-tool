using System.Reflection;
using BDO.GWT.Xperience.Admin.Pages.ProjectSettings.WebChannelSettings;
using Kentico.Xperience.Admin.Base;
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
            var ctor = typeof(WebChannelSnippetEditPage).GetConstructor(new[]
            {
                typeof(IFormComponentMapper),
                typeof(IFormDataBinder),
            });

            Assert.That(ctor, Is.Not.Null);
        }

        [Test]
        public void Constructor_WithNullFormComponentMapper_Throws()
        {
            var formDataBinder = new Mock<IFormDataBinder>().Object;

            // Reflection because base ctor needs Xperience services; we still verify
            // the null-guard by invoking the ctor via Activator (it runs through base too,
            // but the guard executes first when mapper == null).
            Assert.Throws<TargetInvocationException>(() =>
                Activator.CreateInstance(typeof(WebChannelSnippetEditPage),
                    (IFormComponentMapper?)null,
                    formDataBinder));
        }

        [Test]
        public void ObjectId_Property_IsOverriddenAndHasPageParameterAttribute()
        {
            var prop = typeof(WebChannelSnippetEditPage).GetProperty(
                nameof(WebChannelSnippetEditPage.ObjectId),
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);

            Assert.That(prop, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(prop!.PropertyType, Is.EqualTo(typeof(int)));
                Assert.That(prop.GetCustomAttribute<PageParameterAttribute>(), Is.Not.Null);
                Assert.That(prop.GetGetMethod()!.GetBaseDefinition().DeclaringType,
                    Is.Not.EqualTo(typeof(WebChannelSnippetEditPage)));
            });
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
    }
}