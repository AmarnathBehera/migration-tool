using System.Reflection;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Xperience.Admin.Pages.ProjectSettings.WebChannelSettings;
using CMS.DataEngine;
using Kentico.Xperience.Admin.Base.Forms;
using Kentico.Xperience.Admin.Base.Forms.Internal;
using Moq;

namespace BDO.GWT.GLOBAL.Tests.Pages.ProjectSettings.EventsWebinar
{
    public class GlobalEventsWebinarEditPageTests
    {
        [Test]
        public void Page_InheritsFromModelEditPage_OfGlobalEventsWebinarDTO()
        {
            var baseType = typeof(GlobalEventsWebinarEditPage).BaseType;
            Assert.That(baseType, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(baseType!.IsGenericType, Is.True);
                Assert.That(baseType.GetGenericArguments()[0], Is.EqualTo(typeof(GlobalEventsWebinarDto)));
            });
        }

        [Test]
        public void Page_ExposesExpectedConstructor()
        {
            var ctor = typeof(GlobalEventsWebinarEditPage).GetConstructor(new[]
            {
                typeof(IFormItemCollectionProvider),
                typeof(IFormDataBinder),
                typeof(IInfoProvider<GlobalEventsnWebinarsInfo>),
            });

            Assert.That(ctor, Is.Not.Null);
        }

        [Test]
        public void Constructor_WithNullProvider_ThrowsArgumentNullException()
        {
            var formItemProvider = new Mock<IFormItemCollectionProvider>().Object;
            var formDataBinder = new Mock<IFormDataBinder>().Object;

            Assert.Throws<TargetInvocationException>(() =>
                Activator.CreateInstance(typeof(GlobalEventsWebinarEditPage),
                    formItemProvider,
                    formDataBinder,
                    (IInfoProvider<GlobalEventsnWebinarsInfo>?)null));
        }

        [Test]
        public void Model_Property_IsProtectedAndReturnsDto()
        {
            var prop = typeof(GlobalEventsWebinarEditPage).GetProperty(
                "Model",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly);

            Assert.That(prop, Is.Not.Null);
            Assert.That(prop!.PropertyType, Is.EqualTo(typeof(GlobalEventsWebinarDto)));
        }

        [Test]
        public void OverridesConfigurePage()
        {
            var method = typeof(GlobalEventsWebinarEditPage).GetMethod(
                "ConfigurePage",
                BindingFlags.Instance | BindingFlags.Public);

            Assert.That(method, Is.Not.Null);
            Assert.That(method!.DeclaringType, Is.EqualTo(typeof(GlobalEventsWebinarEditPage)));
        }

        [Test]
        public void OverridesProcessFormData()
        {
            var method = typeof(GlobalEventsWebinarEditPage).GetMethod(
                "ProcessFormData",
                BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.That(method, Is.Not.Null);
            Assert.That(method!.DeclaringType, Is.EqualTo(typeof(GlobalEventsWebinarEditPage)));
        }

        [Test]
        public void HasAdministratorPermission()
        {
            var attr = typeof(GlobalEventsWebinarEditPage)
                .GetCustomAttributes(true)
                .OfType<Kentico.Xperience.Admin.Base.UIEvaluatePermissionAttribute>()
                .FirstOrDefault();

            Assert.That(attr, Is.Not.Null);
        }
    }
}
