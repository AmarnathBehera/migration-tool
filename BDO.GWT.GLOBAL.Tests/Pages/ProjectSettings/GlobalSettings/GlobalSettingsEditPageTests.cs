using System.Reflection;
using BDO;
using BDO.GWT.Xperience.Admin.Pages.ProjectSettings.GlobalSettings;
using CMS.DataEngine;
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.Forms;

namespace BDO.GWT.GLOBAL.Tests.Pages.ProjectSettings.GlobalSettings
{
    [TestFixture]
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
                Assert.That(
                    baseType.GetGenericArguments()[0],
                    Is.EqualTo(typeof(GlobalsettingskeyInfo)));
            });
        }

        [Test]
        public void Page_ExposesExpectedConstructor()
        {
            var ctor = typeof(GlobalSettingsEditPage).GetConstructor(
            [
                typeof(IFormComponentMapper),
                typeof(IFormDataBinder),
                typeof(IInfoProvider<GlobalsettingskeyInfo>)
            ]);

            Assert.That(ctor, Is.Not.Null);
        }

        [Test]
        public void Constructor_HasExpectedParameterCount()
        {
            var ctor = typeof(GlobalSettingsEditPage)
                .GetConstructors()
                .Single();

            Assert.That(ctor.GetParameters().Length, Is.EqualTo(3));
        }

        [Test]
        public void Constructor_HasExpectedParameterTypes()
        {
            var ctor = typeof(GlobalSettingsEditPage)
                .GetConstructors()
                .Single();

            var parameters = ctor.GetParameters();

            Assert.Multiple(() =>
            {
                Assert.That(parameters[0].ParameterType, Is.EqualTo(typeof(IFormComponentMapper)));
                Assert.That(parameters[1].ParameterType, Is.EqualTo(typeof(IFormDataBinder)));
                Assert.That(parameters[2].ParameterType, Is.EqualTo(typeof(IInfoProvider<GlobalsettingskeyInfo>)));
            });
        }

        [Test]
        public void Constructor_ContainsProviderParameter()
        {
            var ctor = typeof(GlobalSettingsEditPage)
                .GetConstructors()
                .Single();

            var providerParameter = ctor.GetParameters()
                .Single(x => x.ParameterType == typeof(IInfoProvider<GlobalsettingskeyInfo>));

            Assert.That(providerParameter.Name, Is.EqualTo("provider"));
        }

        [Test]
        public void ObjectId_PropertyExists()
        {
            var property = typeof(GlobalSettingsEditPage)
                .GetProperty(nameof(GlobalSettingsEditPage.ObjectId));

            Assert.That(property, Is.Not.Null);
        }

        [Test]
        public void ObjectId_PropertyIsOverridden()
        {
            var property = typeof(GlobalSettingsEditPage)
                .GetProperty(
                    nameof(GlobalSettingsEditPage.ObjectId),
                    BindingFlags.Instance | BindingFlags.Public);

            Assert.That(property, Is.Not.Null);
            Assert.That(
                property!.DeclaringType,
                Is.EqualTo(typeof(GlobalSettingsEditPage)));
        }

        [Test]
        public void ObjectId_HasGetterAndSetter()
        {
            var property = typeof(GlobalSettingsEditPage)
                .GetProperty(nameof(GlobalSettingsEditPage.ObjectId));

            Assert.That(property, Is.Not.Null);

            Assert.Multiple(() =>
            {
                Assert.That(property!.CanRead, Is.True);
                Assert.That(property.CanWrite, Is.True);
            });
        }

        [Test]
        public void OverridesConfigurePage()
        {
            var method = typeof(GlobalSettingsEditPage).GetMethod(
                nameof(GlobalSettingsEditPage.ConfigurePage),
                BindingFlags.Instance | BindingFlags.Public);

            Assert.That(method, Is.Not.Null);
            Assert.That(
                method!.DeclaringType,
                Is.EqualTo(typeof(GlobalSettingsEditPage)));
        }

        [Test]
        public void ConfigurePage_ReturnsTask()
        {
            var method = typeof(GlobalSettingsEditPage).GetMethod(
                nameof(GlobalSettingsEditPage.ConfigurePage));

            Assert.That(method, Is.Not.Null);
            Assert.That(method!.ReturnType, Is.EqualTo(typeof(Task)));
        }

        [Test]
        public void OverridesGetSubmitSuccessResponse()
        {
            var method = typeof(GlobalSettingsEditPage).GetMethod(
                "GetSubmitSuccessResponse",
                BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.That(method, Is.Not.Null);
            Assert.That(
                method!.DeclaringType,
                Is.EqualTo(typeof(GlobalSettingsEditPage)));
        }

        [Test]
        public void GetSubmitSuccessResponse_ReturnsTaskOfCommandResponse()
        {
            var method = typeof(GlobalSettingsEditPage).GetMethod(
                "GetSubmitSuccessResponse",
                BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.That(method, Is.Not.Null);

            var returnType = method!.ReturnType;

            Assert.Multiple(() =>
            {
                Assert.That(returnType.IsGenericType, Is.True);
                Assert.That(returnType.GetGenericTypeDefinition(), Is.EqualTo(typeof(Task<>)));
                Assert.That(returnType.GetGenericArguments()[0], Is.EqualTo(typeof(ICommandResponse)));
            });
        }

        [Test]
        public void Page_HasUIEvaluatePermissionAttribute()
        {
            var attribute = typeof(GlobalSettingsEditPage)
                .GetCustomAttributes(true)
                .OfType<UIEvaluatePermissionAttribute>()
                .FirstOrDefault();

            Assert.That(attribute, Is.Not.Null);
        }

        [Test]
        public void Page_HasSingleUIEvaluatePermissionAttribute()
        {
            var attributes = typeof(GlobalSettingsEditPage)
                .GetCustomAttributes(true)
                .OfType<UIEvaluatePermissionAttribute>()
                .ToList();

            Assert.That(attributes, Has.Count.EqualTo(1));
        }

        [Test]
        public void Page_IsPublicClass()
        {
            Assert.That(typeof(GlobalSettingsEditPage).IsPublic, Is.True);
        }

        [Test]
        public void Page_IsNotAbstract()
        {
            Assert.That(typeof(GlobalSettingsEditPage).IsAbstract, Is.False);
        }
    }
}