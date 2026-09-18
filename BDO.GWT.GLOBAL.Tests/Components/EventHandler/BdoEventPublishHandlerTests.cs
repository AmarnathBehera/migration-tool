using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using BDO.GWT.GLOBAL.Components.EventHandler;
using CMS.Base;
using CMS.ContentEngine;
using CMS.Core;
using CMS.DataEngine;
using CMS.Membership;
using CMS.OnlineForms;
using CMS.Websites;
using Microsoft.AspNetCore.Http;
using Moq;

namespace BDO.GWT.GLOBAL.Tests.Components.EventHandler;

public class BdoEventPublishHandlerTests
{
    private const BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;

    private Mock<IWebPageManagerFactory> _webPageManagerFactoryMock = null!;
    private Mock<IContentQueryExecutor> _contentQueryExecutorMock = null!;
    private Mock<IHttpContextAccessor> _httpContextAccessorMock = null!;
    private Mock<IContentItemCodeNameProvider> _contentItemCodeNameProviderMock = null!;
    private Mock<IInfoProvider<BizFormInfo>> _bizFormInfoProviderMock = null!;
    private Mock<IEventLogService> _eventLogServiceMock = null!;
    private Mock<IUserInfoProvider> _userInfoProviderMock = null!;
    private TestableHandler _handler = null!;

    [SetUp]
    public void Setup()
    {
        _webPageManagerFactoryMock = new Mock<IWebPageManagerFactory>(MockBehavior.Loose);
        _contentQueryExecutorMock = new Mock<IContentQueryExecutor>(MockBehavior.Loose);
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>(MockBehavior.Loose);
        _contentItemCodeNameProviderMock = new Mock<IContentItemCodeNameProvider>(MockBehavior.Loose);
        _bizFormInfoProviderMock = new Mock<IInfoProvider<BizFormInfo>>(MockBehavior.Loose);
        _eventLogServiceMock = new Mock<IEventLogService>(MockBehavior.Loose);
        _userInfoProviderMock = new Mock<IUserInfoProvider>(MockBehavior.Loose);

        _handler = CreateHandler();
    }

    [Test]
    public void Constructor_AssignsInjectedDependencies()
    {
        Assert.Multiple(() =>
        {
         Assert.That(GetFieldValue<IWebPageManagerFactory>("webPageManagerFactory"), Is.SameAs(_webPageManagerFactoryMock.Object));
        Assert.That(GetFieldValue<IContentQueryExecutor>("contentQueryExecutor"), Is.SameAs(_contentQueryExecutorMock.Object));
        Assert.That(GetFieldValue<IHttpContextAccessor>("httpContextAccessor"), Is.SameAs(_httpContextAccessorMock.Object));
        Assert.That(GetFieldValue<IContentItemCodeNameProvider>("contentItemCodeNameProvider"), Is.SameAs(_contentItemCodeNameProviderMock.Object));
        Assert.That(GetFieldValue<IInfoProvider<BizFormInfo>>("bizFormInfoProvider"), Is.SameAs(_bizFormInfoProviderMock.Object));
        Assert.That(GetFieldValue<IEventLogService>("eventLogService"), Is.SameAs(_eventLogServiceMock.Object));
        Assert.That(GetFieldValue<IUserInfoProvider>("userInfoProvider"), Is.SameAs(_userInfoProviderMock.Object));
        });
    }


    [Test]
    public async Task HandleAsync_WhenContentTypeIsUnsupported_ReturnsWithoutDoingWork()
    {
        var asyncEvent = CreateAfterPublishEvent(new PublishWebPageEventData
        {
            ContentTypeName = "BDO.Article",
            ID = 10,
            DisplayName = "Ignored"
        });

        await _handler.HandleAsync(asyncEvent, CancellationToken.None);

        Assert.That(_handler.GetChildIdCallCount, Is.Zero);
        _webPageManagerFactoryMock.VerifyNoOtherCalls();
        _contentItemCodeNameProviderMock.VerifyNoOtherCalls();
        _eventLogServiceMock.VerifyNoOtherCalls();
        _userInfoProviderMock.VerifyNoOtherCalls();
    }

    [Test]
    public async Task HandleAsync_WhenChildRegistrationFormAlreadyExists_ReturnsEarly()
    {
        var asyncEvent = CreateAfterPublishEvent(CreateEventData());

        _handler.GetChildIdImpl = (_, _, _, _) => Task.FromResult(987);

        await _handler.HandleAsync(asyncEvent, CancellationToken.None);

        Assert.That(_handler.GetChildIdCallCount, Is.EqualTo(1));
        _webPageManagerFactoryMock.VerifyNoOtherCalls();
        _contentItemCodeNameProviderMock.VerifyNoOtherCalls();
        _eventLogServiceMock.VerifyNoOtherCalls();
        _userInfoProviderMock.VerifyNoOtherCalls();
    }

    [Test]
    public async Task HandleAsync_WhenCurrentUserCannotBeResolved_LogsCreateFailure()
    {
        var asyncEvent = CreateAfterPublishEvent(CreateEventData());

        _handler.GetChildIdImpl = (_, _, _, _) => Task.FromResult(0);
        _httpContextAccessorMock.SetupGet(x => x.HttpContext).Returns((HttpContext?)null);

        await _handler.HandleAsync(asyncEvent, CancellationToken.None);

        // The handler calls the LogException extension, which delegates to IEventLogService.LogEvent.
        // Verify the underlying interface call carries the REGFORM_CREATE_FAILED event code.
        _eventLogServiceMock.Verify(x => x.LogEvent(It.Is<EventLogData>(d =>
            d.EventCode == "REGFORM_CREATE_FAILED")), Times.Once);
    }

    [Test]
    public void GetCurrentUser_ReturnsNull_WhenHttpContextIsNull()
    {
        _httpContextAccessorMock.SetupGet(x => x.HttpContext).Returns((HttpContext?)null);

        var result = InvokePrivate<UserInfo?>("GetCurrentUser");

        Assert.That(result, Is.Null);
        _userInfoProviderMock.VerifyNoOtherCalls();
    }

    [Test]
    public void GetCurrentUser_ReturnsNull_WhenIdentityNameIsMissing()
    {
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity())
        };

        _httpContextAccessorMock.SetupGet(x => x.HttpContext).Returns(httpContext);

        var result = InvokePrivate<UserInfo?>("GetCurrentUser");

        Assert.That(result, Is.Null);
        _userInfoProviderMock.VerifyNoOtherCalls();
    }

    [Test]
    public void GetCurrentUser_ReturnsUser_WhenIdentityNameExists()
    {
        // Construct UserInfo without touching Kentico services (setting properties triggers service resolution).
        var expectedUser = (UserInfo)RuntimeHelpers.GetUninitializedObject(typeof(UserInfo));
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Name, "event.user")], "Test"))
        };

        _httpContextAccessorMock.SetupGet(x => x.HttpContext).Returns(httpContext);
        _userInfoProviderMock.Setup(x => x.Get("event.user")).Returns(expectedUser);

        var result = InvokePrivate<UserInfo?>("GetCurrentUser");

        Assert.That(result, Is.SameAs(expectedUser));
        _userInfoProviderMock.VerifyAll();
    }

    [Test]
    public void CreateEventRegistrationForm_WhenTemplateFormIsMissing_ThrowsInvalidOperationException()
    {
        _handler.GetTemplateImpl = () => null;

        var exception = Assert.Throws<TargetInvocationException>(() =>
            InvokePrivate<object>("CreateEventRegistrationForm", "Tax Event", new DateTime(2025, 1, 10)));
        Assert.Multiple(() =>
        {
            Assert.That(exception!.InnerException, Is.TypeOf<InvalidOperationException>());
            Assert.That(exception.InnerException!.Message, Does.Contain("Template form 'EventRegistrationTemplate' was not found."));
        });
    }

    private TestableHandler CreateHandler()
    {
        return new TestableHandler(
            _webPageManagerFactoryMock.Object,
            _contentQueryExecutorMock.Object,
            _httpContextAccessorMock.Object,
            _contentItemCodeNameProviderMock.Object,
            _bizFormInfoProviderMock.Object,
            _eventLogServiceMock.Object,
            _userInfoProviderMock.Object);
    }

    private T GetFieldValue<T>(string fieldName) where T : class
    {
        return (T)typeof(BdoEventPublishHandler)
            .GetField(fieldName, PrivateInstance)!
            .GetValue(_handler)!;
    }

    private static AfterPublishWebPageEvent CreateAfterPublishEvent(PublishWebPageEventData data)
    {
        var instance = (AfterPublishWebPageEvent)RuntimeHelpers.GetUninitializedObject(typeof(AfterPublishWebPageEvent));
        SetPropertyOrBackingField(instance, nameof(AfterPublishWebPageEvent.Data), data);
        return instance;
    }

    private static PublishWebPageEventData CreateEventData()
    {
        return new PublishWebPageEventData
        {
            ContentTypeName = "BDO.Event",
            ContentLanguageName = "en-US",
            DisplayName = "Tax Event",
            ID = 123,
            WebsiteChannelID = 7,
            WebsiteChannelName = "Global"
        };
    }

    private T InvokePrivate<T>(string methodName, params object?[] args)
    {
        var method = typeof(BdoEventPublishHandler).GetMethod(methodName, PrivateInstance)!;
        return (T)method.Invoke(_handler, args)!;
    }

    private static void SetPropertyOrBackingField(object instance, string propertyName, object? value)
    {
        // Try a writable property anywhere in the hierarchy.
        for (var t = instance.GetType(); t != null && t != typeof(object); t = t.BaseType)
        {
            var property = t.GetProperty(propertyName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
            if (property?.CanWrite == true)
            {
                property.SetValue(instance, value);
                return;
            }
        }

        // Fall back to the auto-property backing field, searching the entire base-type chain.
        var backingFieldName = $"<{propertyName}>k__BackingField";
        for (var t = instance.GetType(); t != null && t != typeof(object); t = t.BaseType)
        {
            var field = t.GetField(backingFieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
                ?? t.GetField(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
            if (field != null)
            {
                field.SetValue(instance, value);
                return;
            }
        }

        throw new InvalidOperationException($"Could not set '{propertyName}' on '{instance.GetType().FullName}'.");
    }

    /// <summary>
    /// Test subclass that overrides the virtual seams so tests avoid mocking extension methods
    /// (<see cref="ContentQueryExecutorExtensions"/> and <see cref="InfoProviderExtensions"/>).
    /// </summary>
    private sealed class TestableHandler : BdoEventPublishHandler
    {
        public Func<int, string, string, CancellationToken, Task<int>>? GetChildIdImpl { get; set; }
        public Func<BizFormInfo?>? GetTemplateImpl { get; set; }
        public int GetChildIdCallCount { get; private set; }

        public TestableHandler(
            IWebPageManagerFactory webPageManagerFactory,
            IContentQueryExecutor contentQueryExecutor,
            IHttpContextAccessor httpContextAccessor,
            IContentItemCodeNameProvider contentItemCodeNameProvider,
            IInfoProvider<BizFormInfo> bizFormInfoProvider,
            IEventLogService eventLogService,
            IUserInfoProvider userInfoProvider)
            : base(webPageManagerFactory, contentQueryExecutor, httpContextAccessor,
                   contentItemCodeNameProvider, bizFormInfoProvider, eventLogService, userInfoProvider)
        {
        }

        protected override Task<int> GetChildRegistrationFormIdAsync(
            int parentPageId, string websiteChannelName, string languageName, CancellationToken cancellationToken)
        {
            GetChildIdCallCount++;
            return GetChildIdImpl?.Invoke(parentPageId, websiteChannelName, languageName, cancellationToken)
                ?? Task.FromResult(0);
        }

        protected override BizFormInfo GetTemplateForm()
     => (GetTemplateImpl != null ? GetTemplateImpl() : null)!;
    }
}
