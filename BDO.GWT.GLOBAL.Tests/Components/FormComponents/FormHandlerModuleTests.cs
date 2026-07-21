using System.Reflection;
using BDO.GWT.Xperience.Core.LiveSite.Components.FormComponents;
using CMS.DataEngine;
using CMS.EmailLibrary;
using CMS.Websites.Routing;
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

        private const BindingFlags PrivateStatic =
            BindingFlags.Static | BindingFlags.NonPublic;

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
        [TestCase("/en-gb/people/john-doe", "/people/john-doe")]
        [TestCase("/EN-GB/people/john-doe", "/people/john-doe")]
        [TestCase("/fr-FR/contact", "/contact")]
        [TestCase("/people/john-doe", "/people/john-doe")]
        [TestCase("", "")]
        public void RemoveCultureFromPath_StripsLeadingCulture(string input, string expected)
        {
            var result = InvokePrivateStatic<string>("RemoveCultureFromPath", input);

            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        [TestCase("/en-gb", "")]
        [TestCase("/EN-GB", "")]
        [TestCase("/engb/people", "/engb/people")]
        [TestCase(" /en-gb/people", " /en-gb/people")]
        public void RemoveCultureFromPath_HandlesEdgeCases(string input, string expected)
        {
            var result = InvokePrivateStatic<string>("RemoveCultureFromPath", input);

            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void RemoveCultureFromPath_WithNullInput_ReturnsEmpty()
        {
            var result = InvokePrivateStatic<string>("RemoveCultureFromPath", (string?)null!);

            Assert.That(result, Is.EqualTo(string.Empty));
        }

        [Test]
        [TestCase("en-gb", "en-GB")]
        [TestCase("EN-GB", "en-GB")]
        [TestCase("fr-fr", "fr-FR")]
        [TestCase("en", "en")]
        [TestCase("", "")]
        public void NormalizeCulture_NormalizesCasing(string input, string expected)
        {
            var result = InvokePrivateStatic<string>("NormalizeCulture", input);

            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        [TestCase("   ")]
        [TestCase("en_GB")]
        public void NormalizeCulture_LeavesUnexpectedFormatsUntouched(string input)
        {
            var result = InvokePrivateStatic<string>("NormalizeCulture", input);

            Assert.That(result, Is.EqualTo(input));
        }

        [Test]
        public void NormalizeCulture_WithNullInput_ReturnsNull()
        {
            var result = InvokePrivateStatic<string?>("NormalizeCulture", (string?)null!);

            Assert.That(result, Is.Null);
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

        private static T InvokePrivateStatic<T>(string method, params object?[] args)
        {
            var mi = typeof(FormHandlerModule).GetMethod(method, PrivateStatic);

            Assert.That(mi, Is.Not.Null, $"Private static method '{method}' not found.");
            return (T)mi!.Invoke(null, args)!;
        }
    }
}
