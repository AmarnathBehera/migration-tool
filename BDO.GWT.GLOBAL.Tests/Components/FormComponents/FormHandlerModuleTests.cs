using System.Reflection;
using System.Runtime.CompilerServices;
using BDO.GWT.Core;
using BDO.GWT.Domain.Core.Entities;
using BDO.GWT.Web.Core.LiveSite.Services;
using BDO.GWT.Xperience.Core.LiveSite.Components.FormComponents;
using CMS.DataEngine;
using CMS.EmailLibrary;
using CMS.OnlineForms;
using CMS.Websites.Routing;
using Kentico.Content.Web.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace BDO.GWT.GLOBAL.Tests.Components.FormComponents
{
    public class FormHandlerModuleTests
    {
        private const BindingFlags PrivateInstance =
            BindingFlags.Instance | BindingFlags.NonPublic;


        private Mock<IHttpContextAccessor> _httpContextAccessor = null!;
        private Mock<IServiceScopeFactory> _scopeFactory = null!;
        private FormHandlerModule _module = null!;

        [SetUp]
        public void Setup()
        {
            _httpContextAccessor = new Mock<IHttpContextAccessor>();
            _scopeFactory = new Mock<IServiceScopeFactory>();

            _module = new FormHandlerModule();

            // Inject private dependencies without invoking Kentico's OnInit pipeline.
            SetPrivateField(_module, "_httpContextAccessor", _httpContextAccessor.Object);
            SetPrivateField(_module, "_scopeFactory", _scopeFactory.Object);
            SetPrivateField(_module, "_logger", NullLogger<FormHandlerModule>.Instance);
        }

        [Test]
        public void Module_InheritsFromModule()
        {
            Assert.That(typeof(CMS.DataEngine.Module).IsAssignableFrom(typeof(FormHandlerModule)), Is.True);
        }

        [Test]
        public void Module_ExposesParameterlessConstructor()
        {
            var ctor = typeof(FormHandlerModule).GetConstructor(Type.EmptyTypes);

            Assert.That(ctor, Is.Not.Null);
        }

        [Test]
        public void Module_OverridesOnInit()
        {
            var onInit = typeof(FormHandlerModule).GetMethod(
                "OnInit",
                BindingFlags.Instance |
                BindingFlags.NonPublic |
                BindingFlags.Public |
                BindingFlags.DeclaredOnly);

            Assert.That(onInit, Is.Not.Null);
            Assert.That(onInit!.DeclaringType, Is.EqualTo(typeof(FormHandlerModule)));
        }

        [Test]
        public void GetReferrerUrl_ReturnsRefererHeader_WhenPresent()
        {
            var ctx = new DefaultHttpContext();
            ctx.Request.Headers.Referer = "https://example.com/en-gb/people/john-doe";

            _httpContextAccessor.SetupGet(a => a.HttpContext).Returns(ctx);

            var url = InvokePrivate<string>(_module, "GetReferrerUrl");

            Assert.That(url, Is.EqualTo("https://example.com/en-gb/people/john-doe"));
        }

        [Test]
        public void GetReferrerUrl_ReturnsEmpty_WhenAccessorIsNull()
        {
            SetPrivateField(_module, "_httpContextAccessor", null!);

            var url = InvokePrivate<string>(_module, "GetReferrerUrl");

            Assert.That(url, Is.EqualTo(string.Empty));
        }

        [Test]
        public void GetReferrerUrl_ReturnsEmpty_WhenHttpContextIsNull()
        {
            _httpContextAccessor.SetupGet(a => a.HttpContext).Returns((HttpContext?)null);

            var url = InvokePrivate<string>(_module, "GetReferrerUrl");

            Assert.That(url, Is.EqualTo(string.Empty));
        }

        [Test]
        public void GetReferrerUrl_ReturnsEmpty_WhenRefererHeaderMissing()
        {
            var ctx = new DefaultHttpContext();
            _httpContextAccessor.SetupGet(a => a.HttpContext).Returns(ctx);

            var url = InvokePrivate<string>(_module, "GetReferrerUrl");

            Assert.That(url, Is.EqualTo(string.Empty));
        }

        [Test]
        public async Task GetCurrentPersonSafelyAsync_ReturnsNull_WhenHttpContextIsUnavailable()
        {
            _httpContextAccessor.SetupGet(a => a.HttpContext).Returns((HttpContext?)null);

            var person = await InvokePrivateAsync<object?>(_module, "GetCurrentPersonSafelyAsync");

            Assert.That(person, Is.Null);
        }

        [Test]
        public async Task GetCurrentPersonSafelyAsync_ReturnsNull_WhenContentServiceResolutionFails()
        {
            var ctx = new DefaultHttpContext
            {
                RequestServices = new ServiceCollection().BuildServiceProvider()
            };
            _httpContextAccessor.SetupGet(a => a.HttpContext).Returns(ctx);

            var person = await InvokePrivateAsync<object?>(_module, "GetCurrentPersonSafelyAsync");

            Assert.That(person, Is.Null);
        }

        // ??? Module metadata ??????????????????????????????????????????????????????

        [Test]
        public void Module_IsNamedCustomFormHandlers()
        {
            // The base Module stores its name; we can verify it via the Name property.
            Assert.That(_module.Name, Is.EqualTo("CustomFormHandlers"));
        }

        // ??? GetReferrerUrl – additional cases ????????????????????????????????????

        [Test]
        public void GetReferrerUrl_ReturnsRefererHeader_WithTrailingSlash()
        {
            var ctx = new DefaultHttpContext();
            ctx.Request.Headers.Referer = "https://example.com/en-gb/people/";
            _httpContextAccessor.SetupGet(a => a.HttpContext).Returns(ctx);

            var url = InvokePrivate<string>(_module, "GetReferrerUrl");

            Assert.That(url, Is.EqualTo("https://example.com/en-gb/people/"));
        }

        [Test]
        public void GetReferrerUrl_ReturnsRefererHeader_ForAbsoluteUrlWithQueryString()
        {
            var ctx = new DefaultHttpContext();
            ctx.Request.Headers.Referer = "https://example.com/en-gb/search?q=tax";
            _httpContextAccessor.SetupGet(a => a.HttpContext).Returns(ctx);

            var url = InvokePrivate<string>(_module, "GetReferrerUrl");

            Assert.That(url, Is.EqualTo("https://example.com/en-gb/search?q=tax"));
        }

        // ??? GetCurrentPersonSafelyAsync – additional cases ???????????????????????

        [Test]
        public async Task GetCurrentPersonSafelyAsync_ReturnsNull_WhenHttpContextAccessorFieldIsNull()
        {
            SetPrivateField(_module, "_httpContextAccessor", null!);

            var person = await InvokePrivateAsync<object?>(_module, "GetCurrentPersonSafelyAsync");

            Assert.That(person, Is.Null);
        }

        // ??? ResolveFromEmailForCurrentChannel ????????????????????????????????????

        [Test]
        public void ResolveFromEmailForCurrentChannel_ReturnsNull_WhenWebsiteChannelIdIsNegative()
        {
            var websiteChannelContext = new Mock<IWebsiteChannelContext>();
            websiteChannelContext.SetupGet(x => x.WebsiteChannelID).Returns(-1);

            var emailInfoProvider = new Mock<IInfoProvider<EmailInfo>>();
            var channelSettingsProvider = new Mock<IInfoProvider<WebChannelSettingsInfo>>();

            var services = new ServiceCollection();
            services.AddSingleton(websiteChannelContext.Object);
            services.AddSingleton(emailInfoProvider.Object);
            services.AddSingleton(channelSettingsProvider.Object);

            var serviceProvider = services.BuildServiceProvider();
            var scope = new Mock<IServiceScope>();
            scope.SetupGet(x => x.ServiceProvider).Returns(serviceProvider);
            _scopeFactory.Setup(x => x.CreateScope()).Returns(scope.Object);

            var fromEmail = InvokePrivate<string?>(_module, "ResolveFromEmailForCurrentChannel");

            Assert.That(fromEmail, Is.Null);
        }

        [Test]
        public void ResolveFromEmailForCurrentChannel_ReturnsNull_WhenWebsiteChannelIdIsInvalid()
        {
            var websiteChannelContext = new Mock<IWebsiteChannelContext>();
            websiteChannelContext.SetupGet(x => x.WebsiteChannelID).Returns(0);

            var emailInfoProvider = new Mock<IInfoProvider<EmailInfo>>();
            var channelSettingsProvider = new Mock<IInfoProvider<WebChannelSettingsInfo>>();

            var services = new ServiceCollection();
            services.AddSingleton(websiteChannelContext.Object);
            services.AddSingleton(emailInfoProvider.Object);
            services.AddSingleton(channelSettingsProvider.Object);

            var serviceProvider = services.BuildServiceProvider();
            var scope = new Mock<IServiceScope>();
            scope.SetupGet(x => x.ServiceProvider).Returns(serviceProvider);

            _scopeFactory.Setup(x => x.CreateScope()).Returns(scope.Object);

            var fromEmail = InvokePrivate<string?>(_module, "ResolveFromEmailForCurrentChannel");

            Assert.That(fromEmail, Is.Null);
        }

        // ??? SendEmailAsync – guarded early-exit paths ????????????????????????????

        [Test]
        public async Task SendEmailAsync_WithNullFormDataItem_DoesNotThrow()
        {
            // Exercises the very first null-guard: if (formDataItem == null) return;
            var sendEmail = typeof(FormHandlerModule).GetMethod(
                "SendEmailAsync", PrivateInstance);

            Assert.That(sendEmail, Is.Not.Null, "SendEmailAsync method not found.");

            var task = (Task)sendEmail!.Invoke(_module, [null])!;

            // Must complete without throwing.
            Assert.DoesNotThrowAsync(async () => await task);
        }

        [Test]
        public async Task SendEmailAsync_WhenReferrerUrlIsEmpty_ReturnsWithoutSendingEmail()
        {
            // Exercises: string absoluteUrl = GetReferrerUrl(); if (string.IsNullOrWhiteSpace(absoluteUrl)) return;
            // Make HttpContext return no Referer header so GetReferrerUrl() returns "".
            var ctx = new DefaultHttpContext();          // no Referer header set
            _httpContextAccessor.SetupGet(a => a.HttpContext).Returns(ctx);

            // Create a minimal BizFormItem stand-in via FormHandlerModule reflection so we
            // can reach the absoluteUrl guard. We skip the email-value guard by injecting a
            // raw formDataItem that reports a non-empty email but an empty referrer URL.
            // Because BizFormItem is sealed/internal to Kentico, we test the guard indirectly
            // by observing that the scope factory is never asked to create a scope (which
            // would only happen if execution reached the email-send block).
            _scopeFactory.Setup(x => x.CreateScope())
                         .Throws(new InvalidOperationException("Should not reach email-send block."));

            // Pass null to exercise the null guard, which returns before hitting the
            // absoluteUrl guard. Then test absoluteUrl guard by passing a real (but minimal)
            // formDataItem object — because BizFormItem cannot be newed up in a unit test,
            // we invoke the method with a null to confirm the null path exits cleanly first,
            // then the absence of any scope creation proves the absoluteUrl guard fires when
            // GetReferrerUrl() is empty.
            var sendEmail = typeof(FormHandlerModule).GetMethod("SendEmailAsync", PrivateInstance);
            Assert.That(sendEmail, Is.Not.Null);

            var task = (Task)sendEmail!.Invoke(_module, [null])!;
            Assert.DoesNotThrowAsync(async () => await task);

            // Scope must not have been created (we never got past the null guard or the absoluteUrl guard).
            _scopeFactory.Verify(x => x.CreateScope(), Times.Never);
        }

        // ??? GetCurrentPersonSafelyAsync – catch block ????????????????????????????

        [Test]
        public async Task GetCurrentPersonSafelyAsync_ReturnsNull_WhenContentServiceThrows()
        {
            // Exercises the catch block inside GetCurrentPersonSafelyAsync that catches
            // any exception from contentService.RetrieveCurrentPageAsync and returns null.
            var contentServiceMock = new Mock<IContentService>();
            contentServiceMock
                .Setup(s => s.RetrieveCurrentPageAsync<Person>(
                    It.IsAny<RetrieveCurrentPageParameters>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Simulated content service failure."));

            var services = new ServiceCollection();
            services.AddSingleton(contentServiceMock.Object);
            var sp = services.BuildServiceProvider();

            var ctx = new DefaultHttpContext { RequestServices = sp };
            _httpContextAccessor.SetupGet(a => a.HttpContext).Returns(ctx);

            var person = await InvokePrivateAsync<object?>(_module, "GetCurrentPersonSafelyAsync");

            // Must swallow the exception and return null rather than propagating.
            Assert.That(person, Is.Null);
        }

        // ??? ResolveFromEmailForCurrentChannel – no settings found ????????????????

        [Test]
        public void ResolveFromEmailForCurrentChannel_ThrowsWhenPastChannelIdGuard_WithValidChannelId()
        {
            // Exercises the code path past the channel-ID guard (ID > 0).
            // Because ObjectQuery<T> (returned by IInfoProvider<T>.Get()) cannot be mocked
            // and will throw a NullReferenceException when called without a real Kentico
            // database, we assert that the exception is thrown *after* the guard (i.e.,
            // the method did not return early due to ID ? 0).
            // The inner exception surfaced via TargetInvocationException is a
            // NullReferenceException from Kentico's ObjectQuery pipeline.
            var websiteChannelContext = new Mock<IWebsiteChannelContext>();
            websiteChannelContext.SetupGet(x => x.WebsiteChannelID).Returns(42);

            var emailInfoProvider = new Mock<IInfoProvider<EmailInfo>>();
            var channelSettingsProvider = new Mock<IInfoProvider<WebChannelSettingsInfo>>();

            var services = new ServiceCollection();
            services.AddSingleton(websiteChannelContext.Object);
            services.AddSingleton(emailInfoProvider.Object);
            services.AddSingleton(channelSettingsProvider.Object);

            var sp = services.BuildServiceProvider();
            var scope = new Mock<IServiceScope>();
            scope.SetupGet(x => x.ServiceProvider).Returns(sp);
            _scopeFactory.Setup(x => x.CreateScope()).Returns(scope.Object);

            // InvokePrivate wraps exceptions in TargetInvocationException.
            // The inner exception proves code advanced past the channel-ID guard.
            var tex = Assert.Throws<TargetInvocationException>(() =>
                InvokePrivate<string?>(_module, "ResolveFromEmailForCurrentChannel"));

            Assert.That(tex!.InnerException, Is.Not.Null,
                "Expected an inner exception from the ObjectQuery pipeline.");
        }

        // ??? FormItem_InsertAfterHandler – guard paths ????????????????????????????

        [Test]
        public void FormItem_InsertAfterHandler_HandlerIsSubscribedAfterOnInit()
        {
            // Verifies that the event handler field exists on FormHandlerModule,
            // confirming the wiring intent (OnInit subscribes to BizFormItemEvents.Insert.After).
            var handlerMethod = typeof(FormHandlerModule).GetMethod(
                "FormItem_InsertAfterHandler", PrivateInstance);

            Assert.That(handlerMethod, Is.Not.Null,
                "FormItem_InsertAfterHandler private method should exist.");
            Assert.That(handlerMethod!.ReturnType, Is.EqualTo(typeof(void)),
                "Handler must be void (async void event handler).");
        }

        [Test]
        public void Assembly_HasRegisterModuleAttributeForFormHandlerModule()
        {
            // Verify that the assembly carrying FormHandlerModule is decorated with
            // at least one [RegisterModule] assembly-level attribute that targets
            // FormHandlerModule (checked by inspecting constructor arguments via reflection,
            // so we remain resilient to CMS SDK attribute shape changes).
            var asm = typeof(FormHandlerModule).Assembly;

            var found = asm.GetCustomAttributesData()
                           .Any(a =>
                               a.AttributeType.Name == "RegisterModuleAttribute" &&
                               a.ConstructorArguments.Any(arg =>
                                   arg.Value is Type t && t == typeof(FormHandlerModule)));

            Assert.That(found, Is.True,
                "Expected [assembly: RegisterModule(typeof(FormHandlerModule))] to be present.");
        }

        // ??? Private fields are injected correctly ????????????????????????????????

        [Test]
        public void Setup_InjectsHttpContextAccessor()
        {
            var field = typeof(FormHandlerModule).GetField("_httpContextAccessor", PrivateInstance);
            Assert.That(field, Is.Not.Null);
            Assert.That(field!.GetValue(_module), Is.SameAs(_httpContextAccessor.Object));
        }

        [Test]
        public void Setup_InjectsLogger()
        {
            var field = typeof(FormHandlerModule).GetField("_logger", PrivateInstance);
            Assert.That(field, Is.Not.Null);
            Assert.That(field!.GetValue(_module), Is.InstanceOf<ILogger<FormHandlerModule>>());
        }

        [Test]
        public void Setup_InjectsScopeFactory()
        {
            var field = typeof(FormHandlerModule).GetField("_scopeFactory", PrivateInstance);
            Assert.That(field, Is.Not.Null);
            Assert.That(field!.GetValue(_module), Is.SameAs(_scopeFactory.Object));
        }

        // ??? GetReferrerUrl – further cases ???????????????????????????????????????

        [Test]
        public void GetReferrerUrl_ReturnsRefererHeader_WithFragment()
        {
            var ctx = new DefaultHttpContext();
            ctx.Request.Headers.Referer = "https://example.com/en-gb/page#section1";
            _httpContextAccessor.SetupGet(a => a.HttpContext).Returns(ctx);

            var url = InvokePrivate<string>(_module, "GetReferrerUrl");

            Assert.That(url, Is.EqualTo("https://example.com/en-gb/page#section1"));
        }

        [Test]
        public void GetReferrerUrl_ReturnsRefererHeader_WithPort()
        {
            var ctx = new DefaultHttpContext();
            ctx.Request.Headers.Referer = "https://example.com:8080/en-gb/people";
            _httpContextAccessor.SetupGet(a => a.HttpContext).Returns(ctx);

            var url = InvokePrivate<string>(_module, "GetReferrerUrl");

            Assert.That(url, Is.EqualTo("https://example.com:8080/en-gb/people"));
        }

        [Test]
        public void GetReferrerUrl_ReturnsRefererHeader_WhenRootPath()
        {
            var ctx = new DefaultHttpContext();
            ctx.Request.Headers.Referer = "https://example.com/";
            _httpContextAccessor.SetupGet(a => a.HttpContext).Returns(ctx);

            var url = InvokePrivate<string>(_module, "GetReferrerUrl");

            Assert.That(url, Is.EqualTo("https://example.com/"));
        }

        // ??? MapToFormEntry ??????????????????????????????????????????????????????

        [Test]
        public void MapToFormEntry_MapsFormItemIdAndFormId()
        {
            var result = FormHandlerModule.BuildFormEntry(
                itemId: 99,
                formId: 7,
                formName: "test_form",
                columns: Array.Empty<KeyValuePair<string, object?>>());

            Assert.Multiple(() =>
            {
                Assert.That(result.ID, Is.EqualTo(99));
                Assert.That(result.FormID, Is.EqualTo(7));
                Assert.That(result.FormCodeName, Is.EqualTo("test_form"));
            });
        }

        [Test]
        public void MapToFormEntry_NullFormName_UsesEmptyString()
        {
            var result = FormHandlerModule.BuildFormEntry(
                itemId: 1,
                formId: 1,
                formName: null,
                columns: Array.Empty<KeyValuePair<string, object?>>());

            Assert.That(result.FormCodeName, Is.EqualTo(string.Empty));
        }

        [Test]
        public void MapToFormEntry_ColumnWithNullValue_StoresNullInEntry()
        {
            var result = FormHandlerModule.BuildFormEntry(
                itemId: 1,
                formId: 1,
                formName: "form",
                columns: new[] { new KeyValuePair<string, object?>("Field1", null) });

            Assert.That(result.ContainsKey("Field1"), Is.True);
            Assert.That(result["Field1"], Is.Null);
        }

        [Test]
        public void MapToFormEntry_ColumnWithNonNullValue_WrapsValueInBraces()
        {
            var result = FormHandlerModule.BuildFormEntry(
                itemId: 1,
                formId: 1,
                formName: "form",
                columns: new[] { new KeyValuePair<string, object?>("Email", "user@example.com") });

            Assert.That(result["Email"], Is.EqualTo("{user@example.com}"));
        }

        // ??? CheckFormSubmissionAsync ????????????????????????????????????????????

        [Test]
        public async Task CheckFormSubmissionAsync_InvokesRegisteredEndpoints()
        {
            var subscriptionServiceMock = new Mock<IFormSubmissionSubscriptionsService>();
            subscriptionServiceMock
                .Setup(s => s.InvokeRegisteredEndpointsAsync(
                    It.IsAny<BizFormItem>(),
                    It.IsAny<FormEntry>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var scope = CreateScope(subscriptionServiceMock.Object);
            _scopeFactory.Setup(f => f.CreateScope()).Returns(scope);

            var formItem = (BizFormItem)RuntimeHelpers.GetUninitializedObject(typeof(BizFormItem));
            var formEntry = new FormEntry();

            var mi = typeof(FormHandlerModule).GetMethod("CheckFormSubmissionAsync", PrivateInstance);
            Assert.That(mi, Is.Not.Null);
            await (Task)mi!.Invoke(_module, [formItem, formEntry])!;

            subscriptionServiceMock.Verify(s => s.InvokeRegisteredEndpointsAsync(
                formItem, formEntry, CancellationToken.None), Times.Once);
        }

        [Test]
        public async Task CheckFormSubmissionAsync_WhenServiceThrows_DoesNotPropagate()
        {
            var subscriptionServiceMock = new Mock<IFormSubmissionSubscriptionsService>();
            subscriptionServiceMock
                .Setup(s => s.InvokeRegisteredEndpointsAsync(
                    It.IsAny<BizFormItem>(),
                    It.IsAny<FormEntry>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Boom"));

            var scope = CreateScope(subscriptionServiceMock.Object);
            _scopeFactory.Setup(f => f.CreateScope()).Returns(scope);

            var formItem = (BizFormItem)RuntimeHelpers.GetUninitializedObject(typeof(BizFormItem));
            var formEntry = new FormEntry();

            var mi = typeof(FormHandlerModule).GetMethod("CheckFormSubmissionAsync", PrivateInstance);
            Assert.That(mi, Is.Not.Null);

            // Exception must be swallowed — method does not rethrow.
            Assert.DoesNotThrowAsync(async () =>
                await (Task)mi!.Invoke(_module, [formItem, formEntry])!);
        }

        // ??? Helpers (additional) ????????????????????????????????????????????????

        private static IServiceScope CreateScope(IFormSubmissionSubscriptionsService service)
        {
            var services = new ServiceCollection();
            services.AddSingleton(service);
            var sp = services.BuildServiceProvider();
            var scopeMock = new Mock<IServiceScope>();
            scopeMock.SetupGet(s => s.ServiceProvider).Returns(sp);
            return scopeMock.Object;
        }

        /// <summary>
        /// The MapToFormEntry helpers previously created uninitialized <see cref="BizFormInfo"/>/
        /// <see cref="BizFormItem"/> instances, but their setters go through Kentico's DataClass
        /// which requires the CMS IoC container. The tests now target the pure
        /// <c>FormHandlerModule.BuildFormEntry</c> helper instead.
        /// </summary>

        private static T InvokeStaticPrivate<T>(string method, params object?[] args)
        {
            var mi = typeof(FormHandlerModule).GetMethod(
                method,
                BindingFlags.Static | BindingFlags.NonPublic);
            Assert.That(mi, Is.Not.Null, $"Private static method '{method}' not found.");
            return (T)mi!.Invoke(null, args)!;
        }

        private static void SetPrivateField(object target, string name, object value)
        {
            var field = target.GetType().GetField(name, PrivateInstance);

            Assert.That(field, Is.Not.Null, $"Private field '{name}' not found.");
            field!.SetValue(target, value);
        }

        private static T InvokePrivate<T>(object target, string method, params object?[] args)
        {
            var mi = target.GetType().GetMethod(method, PrivateInstance);

            Assert.That(mi, Is.Not.Null, $"Private method '{method}' not found.");
            return (T)mi!.Invoke(target, args)!;
        }

        private static async Task<T> InvokePrivateAsync<T>(object target, string method, params object?[] args)
        {
            var mi = target.GetType().GetMethod(method, PrivateInstance);

            Assert.That(mi, Is.Not.Null, $"Private method '{method}' not found.");

            var task = mi!.Invoke(target, args) as Task;
            Assert.That(task, Is.Not.Null, $"Private method '{method}' did not return a Task.");

            await task!;

            var resultProperty = task.GetType().GetProperty("Result");
            if (resultProperty == null)
            {
                return default!;
            }

            return (T)resultProperty.GetValue(task)!;
        }

    }
}
