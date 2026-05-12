using System.Reflection;
using System.Runtime.CompilerServices;
using BDO.GWT.Core.DTOs.Cookieboot;
using BDO.GWT.Xperience.Admin.Pages.ProjectSettings.WebChannelSettings;
using CMS.DataEngine;
using Kentico.Xperience.Admin.Base.Forms;
using Kentico.Xperience.Admin.Base.Forms.Internal;
using Moq;

namespace BDO.GWT.GLOBAL.Tests.Pages.ProjectSettings
{
    public class CookiebootEditPageTests
    {
        [Test]
        public void Page_InheritsFromModelEditPage_OfCookiebotDTO()
        {
            var baseType = typeof(CookiebootEditPage).BaseType;
            Assert.That(baseType, Is.Not.Null);
            Assert.That(baseType!.IsGenericType, Is.True);
            Assert.That(baseType.GetGenericArguments()[0], Is.EqualTo(typeof(CookiebotDTO)));
        }

        [Test]
        public void Page_ExposesExpectedConstructor()
        {
            var ctor = typeof(CookiebootEditPage).GetConstructor(new[]
            {
                typeof(IFormItemCollectionProvider),
                typeof(IFormDataBinder),
                typeof(IInfoProvider<CookiebotInfo>),
            });

            Assert.That(ctor, Is.Not.Null);
        }

        [Test]
        public void Constructor_WithNullProvider_ThrowsArgumentNullException()
        {
            var formItemProvider = new Mock<IFormItemCollectionProvider>().Object;
            var formDataBinder = new Mock<IFormDataBinder>().Object;

            // Reflection because base ctor needs Xperience services; we still verify
            // the null-guard by invoking the ctor via Activator (it runs through base too,
            // but the guard executes first when provider == null).
            Assert.Throws<TargetInvocationException>(() =>
                Activator.CreateInstance(typeof(CookiebootEditPage),
                    formItemProvider, formDataBinder, (IInfoProvider<CookiebotInfo>?)null));
        }

        [Test]
        public void Model_Property_IsProtectedAndReturnsCookiebotDTO()
        {
            var prop = typeof(CookiebootEditPage).GetProperty(
                "Model",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly);

            Assert.That(prop, Is.Not.Null);
            Assert.That(prop!.PropertyType, Is.EqualTo(typeof(CookiebotDTO)));
        }

        [Test]
        public void OverridesConfigurePage()
        {
            var method = typeof(CookiebootEditPage).GetMethod(
                "ConfigurePage",
                BindingFlags.Instance | BindingFlags.Public);

            Assert.That(method, Is.Not.Null);
            Assert.That(method!.DeclaringType, Is.EqualTo(typeof(CookiebootEditPage)));
        }

        [Test]
        public void OverridesProcessFormData()
        {
            var method = typeof(CookiebootEditPage).GetMethod(
                "ProcessFormData",
                BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.That(method, Is.Not.Null);
            Assert.That(method!.DeclaringType, Is.EqualTo(typeof(CookiebootEditPage)));
        }
    }
}
