using System.Reflection;
using BDO.GWT.Xperience.Admin.Pages.ProjectSettings.WebChannelSettings;
using CMS.DataEngine;
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.Forms;
using Moq;

namespace BDO.GWT.GLOBAL.Tests.Pages.ProjectSettings.WebChannelSettings
{
    public class PrintorShareEditPageTests
    {
        [Test]
        public void Page_InheritsFromInfoEditPage_OfPrintorShareInfo()
        {
            var baseType = typeof(PrintorShareEditPage).BaseType;
            Assert.That(baseType, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(baseType!.IsGenericType, Is.True);
                Assert.That(baseType.GetGenericArguments()[0], Is.EqualTo(typeof(PrintorShareInfo)));
            });
        }

        [Test]
        public void Page_ExposesExpectedConstructor()
        {
            var ctor = typeof(PrintorShareEditPage).GetConstructor(new[]
            {
                typeof(IFormComponentMapper),
                typeof(IFormDataBinder),
                typeof(IInfoProvider<PrintorShareInfo>),
                typeof(IInfoProvider<WebChannelSettingsInfo>),
                typeof(IChannelAccessService),
            });

            Assert.That(ctor, Is.Not.Null);
        }

        [Test]
        public void Constructor_WithNullPrintorShareInfoProvider_ThrowsArgumentNullException()
        {
            var formComponentMapper = new Mock<IFormComponentMapper>().Object;
            var formDataBinder = new Mock<IFormDataBinder>().Object;
            var webChannelSettingsProvider = new Mock<IInfoProvider<WebChannelSettingsInfo>>().Object;
            var channelAccessService = new Mock<IChannelAccessService>().Object;

            Assert.Throws<TargetInvocationException>(() =>
                Activator.CreateInstance(typeof(PrintorShareEditPage),
                    formComponentMapper,
                    formDataBinder,
                    (IInfoProvider<PrintorShareInfo>?)null,
                    webChannelSettingsProvider,
                    channelAccessService));
        }

        [Test]
        public void WebChannelSettingsId_Property_HasPageParameterAttribute()
        {
            var prop = typeof(PrintorShareEditPage).GetProperty(
                nameof(PrintorShareEditPage.WebChannelSettingsId),
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
            var prop = typeof(PrintorShareEditPage).GetProperty(
                nameof(PrintorShareEditPage.ObjectId),
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);

            Assert.That(prop, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(prop!.PropertyType, Is.EqualTo(typeof(int)));
                Assert.That(prop.GetCustomAttribute<PageParameterAttribute>(), Is.Not.Null);
                Assert.That(prop.GetGetMethod()!.GetBaseDefinition().DeclaringType,
                    Is.Not.EqualTo(typeof(PrintorShareEditPage)));
            });
        }

        [Test]
        public void OverridesConfigurePage()
        {
            var method = typeof(PrintorShareEditPage).GetMethod(
                "ConfigurePage",
                BindingFlags.Instance | BindingFlags.Public);

            Assert.That(method, Is.Not.Null);
            Assert.That(method!.DeclaringType, Is.EqualTo(typeof(PrintorShareEditPage)));
        }

        [Test]
        public void OverridesGetSubmitSuccessResponse()
        {
            var method = typeof(PrintorShareEditPage).GetMethod(
                "GetSubmitSuccessResponse",
                BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.That(method, Is.Not.Null);
            Assert.That(method!.DeclaringType, Is.EqualTo(typeof(PrintorShareEditPage)));
        }
    }
}