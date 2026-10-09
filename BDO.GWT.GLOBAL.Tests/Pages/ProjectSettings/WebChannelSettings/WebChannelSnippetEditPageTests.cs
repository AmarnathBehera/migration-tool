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
        private Mock<IFormComponentMapper> _formComponentMapper = null!;
        private Mock<IFormDataBinder> _formDataBinder = null!;
        private Mock<IInfoProvider<WebChannelSnippetInfo>> _snippetInfoProvider = null!;
        private Mock<IInfoProvider<WebChannelSettingsInfo>> _webChannelSettingsInfoProvider = null!;
        private Mock<IChannelAccessService> _channelAccessService = null!;

        [SetUp]
        public void SetUp()
        {
            _formComponentMapper = new Mock<IFormComponentMapper>();
            _formDataBinder = new Mock<IFormDataBinder>();
            _snippetInfoProvider = new Mock<IInfoProvider<WebChannelSnippetInfo>>();
            _webChannelSettingsInfoProvider = new Mock<IInfoProvider<WebChannelSettingsInfo>>();
            _channelAccessService = new Mock<IChannelAccessService>();
        }

        private WebChannelSnippetEditPage CreatePage() =>
            new(
                _formComponentMapper.Object,
                _formDataBinder.Object,
                _snippetInfoProvider.Object,
                _webChannelSettingsInfoProvider.Object,
                _channelAccessService.Object);

        /// <summary>
        /// base.ConfigurePage() reaches into real Kentico module infrastructure
        /// (GetDataClassInfo -> InfoProviderLoader -> ModuleEntryManager) that only
        /// works inside a bootstrapped CMS application. Unit tests here don't have
        /// that context, so this swallows exactly that expected failure so we can
        /// still assert on state our own code set *before* the base call.
        /// </summary>
        private static async Task RunIgnoringKenticoBootstrapFailure(Func<Task> action)
        {
            try
            {
                await action();
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("pre-initialization"))
            {
                // Expected outside a bootstrapped Kentico app context.
            }
        }

        // ---------- structural checks ----------

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

            var attr = prop!.GetCustomAttribute<PageParameterAttribute>();
            Assert.That(attr, Is.Not.Null);
        }

        [Test]
        public void ObjectId_PropertyIsOverridden_AndHasPageParameterAttribute()
        {
            var prop = typeof(WebChannelSnippetEditPage).GetProperty(
                nameof(WebChannelSnippetEditPage.ObjectId),
                BindingFlags.Instance | BindingFlags.Public);

            Assert.That(prop, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(prop!.DeclaringType, Is.EqualTo(typeof(WebChannelSnippetEditPage)));
                Assert.That(prop.GetCustomAttribute<PageParameterAttribute>(), Is.Not.Null);
            });
        }

        [Test]
        public void OverridesConfigurePage_AsyncReturningTask()
        {
            var method = typeof(WebChannelSnippetEditPage).GetMethod(
                "ConfigurePage", BindingFlags.Instance | BindingFlags.Public);

            Assert.That(method, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(method!.DeclaringType, Is.EqualTo(typeof(WebChannelSnippetEditPage)));
                Assert.That(method.ReturnType, Is.EqualTo(typeof(Task)));
            });
        }

        [Test]
        public void GetSubmitSuccessResponse_IsProtectedOverride_WithExpectedSignature()
        {
            var method = typeof(WebChannelSnippetEditPage).GetMethod(
                "GetSubmitSuccessResponse", BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.That(method, Is.Not.Null);
            var parameters = method!.GetParameters();
            var returnType = method.ReturnType;

            Assert.Multiple(() =>
            {
                Assert.That(method.IsFamily, Is.True);
                Assert.That(method.IsVirtual, Is.True);
                Assert.That(parameters, Has.Length.EqualTo(2));
                Assert.That(parameters[0].ParameterType, Is.EqualTo(typeof(WebChannelSnippetInfo)));
                Assert.That(parameters[1].ParameterType, Is.EqualTo(typeof(ICollection<IFormItem>)));
                Assert.That(returnType.IsGenericType, Is.True);
                Assert.That(returnType.GetGenericTypeDefinition(), Is.EqualTo(typeof(Task<>)));
                Assert.That(returnType.GetGenericArguments()[0], Is.EqualTo(typeof(ICommandResponse)));
            });
        }

        [Test]
        public void WebChannelSettingsDisplayName_IsPrivateProperty()
        {
            var prop = typeof(WebChannelSnippetEditPage).GetProperty(
                "WebChannelSettingsDisplayName", BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.That(prop, Is.Not.Null);
            var getter = prop!.GetGetMethod(nonPublic: true);
            Assert.Multiple(() =>
            {
                Assert.That(getter, Is.Not.Null);
                Assert.That(getter!.IsPrivate, Is.True);
            });
        }

        [Test]
        public void Page_HasSinglePublicConstructor_WithExpectedParameterTypes()
        {
            var ctors = typeof(WebChannelSnippetEditPage).GetConstructors(BindingFlags.Instance | BindingFlags.Public);
            Assert.That(ctors, Has.Length.EqualTo(1));

            var parameters = ctors.Single().GetParameters();
            Assert.Multiple(() =>
            {
                Assert.That(parameters, Has.Length.EqualTo(5));
                Assert.That(parameters[0].ParameterType, Is.EqualTo(typeof(IFormComponentMapper)));
                Assert.That(parameters[1].ParameterType, Is.EqualTo(typeof(IFormDataBinder)));
                Assert.That(parameters[2].ParameterType, Is.EqualTo(typeof(IInfoProvider<WebChannelSnippetInfo>)));
                Assert.That(parameters[3].ParameterType, Is.EqualTo(typeof(IInfoProvider<WebChannelSettingsInfo>)));
                Assert.That(parameters[4].ParameterType, Is.EqualTo(typeof(IChannelAccessService)));
            });
        }

        // ---------- behavioral tests ----------

        [Test]
        public async Task ConfigurePage_WhenAccessDenied_SetsAccessDeniedHeadline_AndHidesSubmit_AndSkipsBaseConfigure()
        {
            // This path returns before base.ConfigurePage() is ever called,
            // so it needs no Kentico bootstrap workaround.
            _channelAccessService
                .Setup(s => s.CanAccessAsync(It.IsAny<int>()))
                .ReturnsAsync(false);

            var page = CreatePage();
            page.WebChannelSettingsId = 42;

            await page.ConfigurePage();

            Assert.Multiple(() =>
            {
                Assert.That(page.PageConfiguration.Headline, Is.EqualTo("Access Denied"));
                Assert.That(page.PageConfiguration.SubmitConfiguration.Visible, Is.False);
                Assert.That(page.PageConfiguration.UIFormName, Is.Not.EqualTo("WebChannelSnippetEdit"));
            });
        }

        [Test]
        public async Task ConfigurePage_WhenAccessGranted_SetsUIFormName_BeforeDelegatingToBase()
        {
            _channelAccessService
                .Setup(s => s.CanAccessAsync(It.IsAny<int>()))
                .ReturnsAsync(true);

            var page = CreatePage();
            page.WebChannelSettingsId = 42;

            await RunIgnoringKenticoBootstrapFailure(page.ConfigurePage);

            // UIFormName is assigned immediately before the base call, so it's
            // still set even though base.ConfigurePage() itself can't complete here.
            Assert.That(page.PageConfiguration.UIFormName, Is.EqualTo("WebChannelSnippetEdit"));
        }

        [Test]
        public async Task ConfigurePage_PassesWebChannelSettingsId_ToAccessCheck()
        {
            int? capturedId = null;
            _channelAccessService
                .Setup(s => s.CanAccessAsync(It.IsAny<int>()))
                .Callback<int>(id => capturedId = id)
                .ReturnsAsync(true);

            var page = CreatePage();
            page.WebChannelSettingsId = 99;

            await RunIgnoringKenticoBootstrapFailure(page.ConfigurePage);

            Assert.That(capturedId, Is.EqualTo(99));
        }
    }
}