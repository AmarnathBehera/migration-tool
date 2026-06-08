using System.Reflection;
using BDO;
using BDO.GWT.Xperience.Admin.Pages.ProjectSettings.GlobalSettings;
using CMS.DataEngine;
using Kentico.Xperience.Admin.Base.Forms;
using Moq;

namespace BDO.GWT.GLOBAL.Tests.Pages.ProjectSettings.GlobalSettings
{
    public class GlobalSettingsEditPageTests
    {
        [Test]
        public void Page_InheritsFromInfoEditPage_OfGlobalsettingskeyInfo()
        {
            var baseType = typeof(GlobalSettingsEditPage).BaseType;
            Assert.That(baseType, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(baseType!.IsGenericType, Is.True);
                Assert.That(baseType.GetGenericArguments()[0], Is.EqualTo(typeof(GlobalsettingskeyInfo)));
            });
        }

        [Test]
        public void Page_ExposesExpectedConstructor()
        {
            var ctor = typeof(GlobalSettingsEditPage).GetConstructor([
                typeof(IFormComponentMapper),
                typeof(IFormDataBinder),
                typeof(IInfoProvider<GlobalsettingskeyInfo>),
            ]);

            Assert.That(ctor, Is.Not.Null);
        }

        [Test]
        public void Constructor_WithNullProvider_ThrowsArgumentNullException()
        {
            var formComponentMapper = new Mock<IFormComponentMapper>().Object;
            var formDataBinder = new Mock<IFormDataBinder>().Object;

            Assert.Throws<System.Reflection.TargetInvocationException>(() =>
                Activator.CreateInstance(
                    typeof(GlobalSettingsEditPage),
                    formComponentMapper,
                    formDataBinder,
                    (IInfoProvider<GlobalsettingskeyInfo>?)null));
        }

        [Test]
        public void ObjectId_PropertyIsOverridden()
        {
            var prop = typeof(GlobalSettingsEditPage).GetProperty(
                nameof(GlobalSettingsEditPage.ObjectId),
                BindingFlags.Instance | BindingFlags.Public);

            Assert.That(prop, Is.Not.Null);
            Assert.That(prop!.DeclaringType, Is.EqualTo(typeof(GlobalSettingsEditPage)));
        }

        [Test]
        public void OverridesConfigurePage()
        {
            var method = typeof(GlobalSettingsEditPage).GetMethod(
                "ConfigurePage",
                BindingFlags.Instance | BindingFlags.Public);

            Assert.That(method, Is.Not.Null);
            Assert.That(method!.DeclaringType, Is.EqualTo(typeof(GlobalSettingsEditPage)));
        }

        [Test]
        public void OverridesGetSubmitSuccessResponse()
        {
            var method = typeof(GlobalSettingsEditPage).GetMethod(
                "GetSubmitSuccessResponse",
                BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.That(method, Is.Not.Null);
            Assert.That(method!.DeclaringType, Is.EqualTo(typeof(GlobalSettingsEditPage)));
        }

        [Test]
        public void Page_HasUIEvaluatePermissionAttribute_WithAdministrator()
        {
            var attr = typeof(GlobalSettingsEditPage)
                .GetCustomAttributes(true)
                .OfType<Kentico.Xperience.Admin.Base.UIEvaluatePermissionAttribute>()
                .FirstOrDefault();

            Assert.That(attr, Is.Not.Null);
        }
    }
}
