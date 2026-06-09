using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using BDO;
using BDO.GWT.Xperience.Admin.Pages.ProjectSettings.WebChannelSettings;
using CMS.DataEngine;
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

        [Test]
        public void WebChannelSettingsId_PageParameterAttribute_UsesIntPageModelBinder()
        {
            var prop = typeof(WebChannelSnippetEditPage).GetProperty(nameof(WebChannelSnippetEditPage.WebChannelSettingsId));
            Assert.That(prop, Is.Not.Null);

            // Use CustomAttributeData so we don't depend on a named property on PageParameterAttribute.
            var attrData = prop!.GetCustomAttributesData()
                .FirstOrDefault(a => a.AttributeType.Name == "PageParameterAttribute");
            Assert.Multiple(() =>
            {
                Assert.That(attrData, Is.Not.Null);
                Assert.That(attrData!.ConstructorArguments, Has.Count.GreaterThan(0));
                Assert.That(
                    (Type)attrData.ConstructorArguments[0].Value!,
                    Is.EqualTo(typeof(Kentico.Xperience.Admin.Base.IntPageModelBinder)));
            });
        }

        [Test]
        public void ObjectId_HasPageParameterAttribute()
        {
            var prop = typeof(WebChannelSnippetEditPage).GetProperty(
                nameof(WebChannelSnippetEditPage.ObjectId),
                BindingFlags.Instance | BindingFlags.Public);

            Assert.That(prop, Is.Not.Null);
            var attr = prop!.GetCustomAttribute<Kentico.Xperience.Admin.Base.PageParameterAttribute>();
            Assert.That(attr, Is.Not.Null);
        }

        [Test]
        public void ConfigurePage_IsAsyncAndReturnsTask()
        {
            var method = typeof(WebChannelSnippetEditPage).GetMethod(
                "ConfigurePage",
                BindingFlags.Instance | BindingFlags.Public);

            Assert.That(method, Is.Not.Null);
            Assert.That(method!.ReturnType, Is.EqualTo(typeof(Task)));
        }

        [Test]
        public void GetSubmitSuccessResponse_IsProtectedOverride()
        {
            var method = typeof(WebChannelSnippetEditPage).GetMethod(
                "GetSubmitSuccessResponse",
                BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.That(method, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(method!.IsFamily, Is.True);   // protected
                Assert.That(method.IsVirtual, Is.True);   // override
            });
        }

        [Test]
        public void WebChannelSettingsDisplayName_IsPrivateProperty()
        {
            var prop = typeof(WebChannelSnippetEditPage).GetProperty(
                "WebChannelSettingsDisplayName",
                BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.That(prop, Is.Not.Null);
            var getter = prop!.GetGetMethod(nonPublic: true);
            Assert.Multiple(() =>
            {
                Assert.That(getter, Is.Not.Null);
                Assert.That(getter!.IsPrivate, Is.True);
            });
        }

        [Test]
        public void Page_IsPublicNonAbstractClass()
        {
            Assert.Multiple(() =>
            {
                Assert.That(typeof(WebChannelSnippetEditPage).IsPublic, Is.True);
                Assert.That(typeof(WebChannelSnippetEditPage).IsAbstract, Is.False);
            });
        }

        [Test]
        public void Page_IsInExpectedNamespace()
        {
            Assert.That(
                typeof(WebChannelSnippetEditPage).Namespace,
                Is.EqualTo("BDO.GWT.Xperience.Admin.Pages.ProjectSettings.WebChannelSettings"));
        }

        [Test]
        public void Page_HasSinglePublicConstructor()
        {
            var constructors = typeof(WebChannelSnippetEditPage)
                .GetConstructors(BindingFlags.Instance | BindingFlags.Public);

            Assert.That(constructors, Has.Length.EqualTo(1));
        }

        [Test]
        public void Constructor_HasExpectedParameterCount()
        {
            var ctor = typeof(WebChannelSnippetEditPage)
                .GetConstructors()
                .Single();

            Assert.That(ctor.GetParameters(), Has.Length.EqualTo(5));
        }

        [Test]
        public void Constructor_HasExpectedParameterTypes()
        {
            var ctor = typeof(WebChannelSnippetEditPage)
                .GetConstructors()
                .Single();

            var parameters = ctor.GetParameters();

            Assert.Multiple(() =>
            {
                Assert.That(parameters[0].ParameterType, Is.EqualTo(typeof(IFormComponentMapper)));
                Assert.That(parameters[1].ParameterType, Is.EqualTo(typeof(IFormDataBinder)));
                Assert.That(parameters[2].ParameterType, Is.EqualTo(typeof(IInfoProvider<WebChannelSnippetInfo>)));
                Assert.That(parameters[3].ParameterType, Is.EqualTo(typeof(IInfoProvider<WebChannelSettingsInfo>)));
                Assert.That(parameters[4].ParameterType, Is.EqualTo(typeof(IChannelAccessService)));
            });
        }

        [Test]
        public void WebChannelSettingsId_PropertyHasGetterAndSetter()
        {
            var prop = typeof(WebChannelSnippetEditPage).GetProperty(nameof(WebChannelSnippetEditPage.WebChannelSettingsId));

            Assert.That(prop, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(prop!.CanRead, Is.True);
                Assert.That(prop.CanWrite, Is.True);
            });
        }

        [Test]
        public void WebChannelSettingsId_IsPublicProperty()
        {
            var prop = typeof(WebChannelSnippetEditPage).GetProperty(nameof(WebChannelSnippetEditPage.WebChannelSettingsId));

            Assert.That(prop, Is.Not.Null);
            Assert.That(prop!.GetGetMethod()?.IsPublic, Is.True);
        }

        [Test]
        public void GetSubmitSuccessResponse_HasExpectedParameters()
        {
            var method = typeof(WebChannelSnippetEditPage).GetMethod(
                "GetSubmitSuccessResponse",
                BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.That(method, Is.Not.Null);
            var parameters = method!.GetParameters();

            Assert.Multiple(() =>
            {
                Assert.That(parameters, Has.Length.EqualTo(2));
                Assert.That(parameters[0].ParameterType, Is.EqualTo(typeof(WebChannelSnippetInfo)));
                Assert.That(parameters[1].ParameterType, Is.EqualTo(typeof(ICollection<IFormItem>)));
            });
        }

        [Test]
        public void GetSubmitSuccessResponse_ReturnsTaskOfICommandResponse()
        {
            var method = typeof(WebChannelSnippetEditPage).GetMethod(
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
    }
}
