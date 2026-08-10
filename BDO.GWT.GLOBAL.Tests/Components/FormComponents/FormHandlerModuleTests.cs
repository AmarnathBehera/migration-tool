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

        // ??? Module metadata ??????????????????????????????????????????????????????

        [Test]
        public void Module_IsNamedCustomFormHandlers()
        {
            // The base Module stores its name; we can verify it via the Name property.
            Assert.That(_module.Name, Is.EqualTo("CustomFormHandlers"));
        }

        // ??? RemoveCultureFromPath – additional cases ?????????????????????????????

        [Test]
        [TestCase("/de-de/news/article-1", "/news/article-1")]
        [TestCase("/zh-cn/a/b/c/d", "/a/b/c/d")]
        [TestCase("/en-GB/", "/")]
        [TestCase("/en-gb", "")]
        public void RemoveCultureFromPath_StripsLeadingCulture_Extended(string input, string expected)
        {
            var result = InvokePrivateStatic<string>("RemoveCultureFromPath", input);

            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        [TestCase("/123-ab/people")]   // segment starts with digit – not a culture
        [TestCase("/english/people")]  // more than 2 chars per part – not a culture
        public void RemoveCultureFromPath_DoesNotStripNonCultureSegments(string input)
        {
            var result = InvokePrivateStatic<string>("RemoveCultureFromPath", input);

            // Path should remain unchanged when the leading segment is not a culture tag.
            Assert.That(result, Is.EqualTo(input));
        }

        // ??? NormalizeCulture – additional cases ??????????????????????????????????

        [Test]
        public void NormalizeCulture_WithThreePartCulture_OnlySplitsOnFirstHyphen()
        {
            // "zh-hans-cn" has multiple hyphens; only the first split matters.
            var result = InvokePrivateStatic<string>("NormalizeCulture", "zh-hans-cn");

            // The implementation splits on '-' and uses parts[0]/parts[1], so the
            // result is "zh-HANS" (the third segment is ignored by the current logic).
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Does.StartWith("zh-"));
        }

        [Test]
        [TestCase("AR-SA", "ar-SA")]
        [TestCase("PT-BR", "pt-BR")]
        [TestCase("ZH-TW", "zh-TW")]
        public void NormalizeCulture_UpperCaseInput_NormalizedCorrectly(string input, string expected)
        {
            var result = InvokePrivateStatic<string>("NormalizeCulture", input);

            Assert.That(result, Is.EqualTo(expected));
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

        // ??? Assembly attribute ???????????????????????????????????????????????????

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

        // ??? CultureRegex ?????????????????????????????????????????????????????????

        [Test]
        [TestCase("en-gb", true)]
        [TestCase("EN-GB", true)]
        [TestCase("fr-FR", true)]
        [TestCase("zh-CN", true)]
        [TestCase("ar-SA", true)]
        public void CultureRegex_MatchesValidCultureTags(string input, bool expectedMatch)
        {
            var method = typeof(FormHandlerModule).GetMethod("CultureRegex", PrivateStatic);
            Assert.That(method, Is.Not.Null, "CultureRegex() method not found.");

            var regex = (System.Text.RegularExpressions.Regex)method!.Invoke(null, null)!;

            Assert.That(regex.IsMatch(input), Is.EqualTo(expectedMatch));
        }

        [Test]
        [TestCase("english")]           // no hyphen
        [TestCase("en")]                // no hyphen, single part
        [TestCase("en-gb-extra")]       // three parts
        [TestCase("e-gb")]              // first part only 1 char
        [TestCase("en-g")]              // second part only 1 char
        [TestCase("123-gb")]            // digits
        [TestCase("")]                  // empty
        public void CultureRegex_DoesNotMatchInvalidCultureTags(string input)
        {
            var method = typeof(FormHandlerModule).GetMethod("CultureRegex", PrivateStatic);
            Assert.That(method, Is.Not.Null, "CultureRegex() method not found.");

            var regex = (System.Text.RegularExpressions.Regex)method!.Invoke(null, null)!;

            Assert.That(regex.IsMatch(input), Is.False);
        }

        // ??? CulturePrefixRegex ???????????????????????????????????????????????????

        [Test]
        [TestCase("/en-gb/people")]
        [TestCase("/EN-GB/people")]
        [TestCase("/fr-fr/contact")]
        [TestCase("/zh-cn")]
        public void CulturePrefixRegex_MatchesPathsWithCulturePrefix(string input)
        {
            var method = typeof(FormHandlerModule).GetMethod("CulturePrefixRegex", PrivateStatic);
            Assert.That(method, Is.Not.Null, "CulturePrefixRegex() method not found.");

            var regex = (System.Text.RegularExpressions.Regex)method!.Invoke(null, null)!;

            Assert.That(regex.IsMatch(input), Is.True);
        }

        [Test]
        [TestCase("/people/en-gb")]     // culture not at start
        [TestCase("/english/contact")]  // not a 2-letter code
        [TestCase("en-gb/contact")]     // no leading slash
        [TestCase("")]                  // empty
        public void CulturePrefixRegex_DoesNotMatchPathsWithoutCulturePrefix(string input)
        {
            var method = typeof(FormHandlerModule).GetMethod("CulturePrefixRegex", PrivateStatic);
            Assert.That(method, Is.Not.Null, "CulturePrefixRegex() method not found.");

            var regex = (System.Text.RegularExpressions.Regex)method!.Invoke(null, null)!;

            Assert.That(regex.IsMatch(input), Is.False);
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

        // ??? NormalizeCulture – further cases ????????????????????????????????????

        [Test]
        public void NormalizeCulture_SingleHyphen_ReturnsNormalizedTwoParts()
        {
            // Input is "-" ? parts[0]="" parts[1]=""
            var result = InvokePrivateStatic<string>("NormalizeCulture", "-");

            // Both parts are empty; result should be "-"
            Assert.That(result, Is.EqualTo("-"));
        }

        [Test]
        [TestCase("JA-JP", "ja-JP")]
        [TestCase("ko-KR", "ko-KR")]
        [TestCase("RU-RU", "ru-RU")]
        public void NormalizeCulture_CoversMiscLocales(string input, string expected)
        {
            var result = InvokePrivateStatic<string>("NormalizeCulture", input);

            Assert.That(result, Is.EqualTo(expected));
        }

        // ??? RemoveCultureFromPath – further boundary cases ???????????????????????

        [Test]
        public void RemoveCultureFromPath_OnlyCultureSegmentWithTrailingSlash_ReturnsSlash()
        {
            // "/en-gb/" ? the culture prefix "/en-gb" is stripped, leaving "/"
            var result = InvokePrivateStatic<string>("RemoveCultureFromPath", "/en-gb/");

            Assert.That(result, Is.EqualTo("/"));
        }

        [Test]
        public void RemoveCultureFromPath_PathWithMultipleSegmentsAfterCulture_StripsOnlyPrefix()
        {
            var result = InvokePrivateStatic<string>("RemoveCultureFromPath", "/fr-fr/a/b/c/d/e");

            Assert.That(result, Is.EqualTo("/a/b/c/d/e"));
        }

        [Test]
        public void RemoveCultureFromPath_CultureTagInMiddleOfPath_IsNotStripped()
        {
            // Only the leading culture prefix should be removed.
            var result = InvokePrivateStatic<string>("RemoveCultureFromPath", "/people/en-gb/profile");

            Assert.That(result, Is.EqualTo("/people/en-gb/profile"));
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
