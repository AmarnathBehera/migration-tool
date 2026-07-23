using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using BDO.GWT.Xperience.Admin.Pages.ProjectSettings.WebChannelSettings;
using CMS.ContentEngine;
using CMS.DataEngine;
using CMS.Membership;
using Microsoft.AspNetCore.Http;
using Moq;

namespace BDO.GWT.GLOBAL.Tests.Modules
{
    public class ChannelAccessServiceTests
    {
        private Mock<IInfoProvider<WebChannelSettingsInfo>> _webChannelSettingsProvider = null!;
        private Mock<IInfoProvider<ChannelInfo>> _channelProvider = null!;
        private Mock<IUserInfoProvider> _userInfoProvider = null!;
        private Mock<IInfoProvider<UserRoleInfo>> _userRoleProvider = null!;
        private Mock<IInfoProvider<RoleInfo>> _roleProvider = null!;
        private Mock<IHttpContextAccessor> _httpContextAccessor = null!;
        private ChannelAccessService _service = null!;

        [SetUp]
        public void Setup()
        {
            _webChannelSettingsProvider = new Mock<IInfoProvider<WebChannelSettingsInfo>>();
            _channelProvider = new Mock<IInfoProvider<ChannelInfo>>();
            _userInfoProvider = new Mock<IUserInfoProvider>();
            _userRoleProvider = new Mock<IInfoProvider<UserRoleInfo>>();
            _roleProvider = new Mock<IInfoProvider<RoleInfo>>();
            _httpContextAccessor = new Mock<IHttpContextAccessor>();

            _service = new ChannelAccessService(
                _webChannelSettingsProvider.Object,
                _channelProvider.Object,
                _userInfoProvider.Object,
                _userRoleProvider.Object,
                _roleProvider.Object,
                _httpContextAccessor.Object);
        }

        [Test]
        public void Service_ImplementsIChannelAccessService()
        {
            Assert.That(_service, Is.InstanceOf<IChannelAccessService>());
        }

        [Test]
        public void Service_ExposesExpectedConstructor()
        {
            var ctor = typeof(ChannelAccessService).GetConstructors().Single();

            Assert.That(ctor.GetParameters(), Has.Length.EqualTo(6));
            Assert.Multiple(() =>
            {
                Assert.That(ctor.GetParameters()[0].ParameterType, Is.EqualTo(typeof(IInfoProvider<WebChannelSettingsInfo>)));
                Assert.That(ctor.GetParameters()[1].ParameterType, Is.EqualTo(typeof(IInfoProvider<ChannelInfo>)));
                Assert.That(ctor.GetParameters()[2].ParameterType, Is.EqualTo(typeof(IUserInfoProvider)));
                Assert.That(ctor.GetParameters()[3].ParameterType, Is.EqualTo(typeof(IInfoProvider<UserRoleInfo>)));
                Assert.That(ctor.GetParameters()[4].ParameterType, Is.EqualTo(typeof(IInfoProvider<RoleInfo>)));
                Assert.That(ctor.GetParameters()[5].ParameterType, Is.EqualTo(typeof(IHttpContextAccessor)));
            });
        }

        [Test]
        public void GetAccessibleWebChannelSettingsIdsAsync_ReturnType_IsTaskOfCollectionOfInt()
        {
            var method = typeof(ChannelAccessService).GetMethod(nameof(ChannelAccessService.GetAccessibleWebChannelSettingsIdsAsync));

            Assert.That(method, Is.Not.Null);
            Assert.That(method!.ReturnType, Is.EqualTo(typeof(Task<ICollection<int>>)));
        }

        [Test]
        public void CanAccessAsync_ReturnType_IsTaskOfBoolean()
        {
            var method = typeof(ChannelAccessService).GetMethod(nameof(ChannelAccessService.CanAccessAsync));

            Assert.That(method, Is.Not.Null);
            Assert.That(method!.ReturnType, Is.EqualTo(typeof(Task<bool>)));
        }

        [Test]
        public void CanAccessAsync_DeclaresExpectedParameter()
        {
            var method = typeof(ChannelAccessService).GetMethod(nameof(ChannelAccessService.CanAccessAsync));

            Assert.Multiple(() =>
            {
                Assert.That(method, Is.Not.Null);
                Assert.That(method!.GetParameters(), Has.Length.EqualTo(1));
                Assert.That(method.GetParameters()[0].ParameterType, Is.EqualTo(typeof(int)));
            });
        }

        [Test]
        public async Task GetAccessibleWebChannelSettingsIdsAsync_WithNoHttpContext_ReturnsEmpty()
        {
            _httpContextAccessor.SetupGet(a => a.HttpContext).Returns((HttpContext?)null);

            var result = await _service.GetAccessibleWebChannelSettingsIdsAsync();

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task GetAccessibleWebChannelSettingsIdsAsync_WithUnauthenticatedUser_ReturnsEmpty()
        {
            var httpContext = new DefaultHttpContext();
            _httpContextAccessor.SetupGet(a => a.HttpContext).Returns(httpContext);

            var result = await _service.GetAccessibleWebChannelSettingsIdsAsync();

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task CanAccessAsync_WhenAccessibleIdsEmpty_ReturnsFalse()
        {
            _httpContextAccessor.SetupGet(a => a.HttpContext).Returns((HttpContext?)null);

            var canAccess = await _service.CanAccessAsync(123);

            Assert.That(canAccess, Is.False);
        }

        // ── structural ────────────────────────────────────────────────────────

        [Test]
        public void Service_IsPublicNonAbstractClass()
        {
            Assert.Multiple(() =>
            {
                Assert.That(typeof(ChannelAccessService).IsPublic, Is.True);
                Assert.That(typeof(ChannelAccessService).IsAbstract, Is.False);
            });
        }

        [Test]
        public void IChannelAccessService_IsPublicInterface()
        {
            Assert.Multiple(() =>
            {
                Assert.That(typeof(IChannelAccessService).IsPublic, Is.True);
                Assert.That(typeof(IChannelAccessService).IsInterface, Is.True);
            });
        }

        [Test]
        public void IChannelAccessService_ExposesGetAccessibleWebChannelSettingsIdsAsync()
        {
            Assert.That(typeof(IChannelAccessService)
                .GetMethod(nameof(IChannelAccessService.GetAccessibleWebChannelSettingsIdsAsync)), Is.Not.Null);
        }

        [Test]
        public void IChannelAccessService_ExposesCanAccessAsync()
        {
            Assert.That(typeof(IChannelAccessService)
                .GetMethod(nameof(IChannelAccessService.CanAccessAsync)), Is.Not.Null);
        }

        [Test]
        public void GetAccessibleWebChannelSettingsIdsAsync_ReturnType_IsTaskOfICollectionOfInt()
        {
            var method = typeof(ChannelAccessService)
                .GetMethod(nameof(ChannelAccessService.GetAccessibleWebChannelSettingsIdsAsync))!;

            Assert.That(method.ReturnType, Is.EqualTo(typeof(Task<ICollection<int>>)));
        }

        [Test]
        public void CanAccessAsync_ReturnType_IsTaskOfBool()
        {
            var method = typeof(ChannelAccessService)
                .GetMethod(nameof(ChannelAccessService.CanAccessAsync))!;

            Assert.That(method.ReturnType, Is.EqualTo(typeof(Task<bool>)));
        }

        [Test]
        public void Service_HasSixPrivateReadonlyFields()
        {
            var fields = typeof(ChannelAccessService)
                .GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
                .ToList();

            Assert.That(fields, Has.Count.GreaterThanOrEqualTo(6));
        }

        [Test]
        public void Service_HasPrivateField_ForHttpContextAccessor()
        {
            var field = typeof(ChannelAccessService)
                .GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
                .FirstOrDefault(f => f.FieldType == typeof(IHttpContextAccessor));

            Assert.That(field, Is.Not.Null);
        }

        // ── GetAccessibleWebChannelSettingsIdsAsync – extra cases ─────────────

        [Test]
        public async Task GetAccessibleWebChannelSettingsIdsAsync_WithNullUserName_ReturnsEmpty()
        {
            var ctx = new DefaultHttpContext();
            // No Identity / User set → Name is null
            _httpContextAccessor.SetupGet(a => a.HttpContext).Returns(ctx);

            var result = await _service.GetAccessibleWebChannelSettingsIdsAsync();

            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task CanAccessAsync_WithNoHttpContext_ReturnsFalse()
        {
            _httpContextAccessor.SetupGet(a => a.HttpContext).Returns((HttpContext?)null);

            var result = await _service.CanAccessAsync(42);

            Assert.That(result, Is.False);
        }

        // ── behavioural: GetAccessibleWebChannelSettingsIdsAsync ──────────────

        [Test]
        public async Task GetAccessibleWebChannelSettingsIdsAsync_WhenUserIsAdministrator_ReturnsNull()
        {
            AuthenticateUser("admin", MakeUser(userId: 1, admin: true));

            var result = await _service.GetAccessibleWebChannelSettingsIdsAsync();

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetAccessibleWebChannelSettingsIdsAsync_WhenUserHasNoRoles_ReturnsEmpty()
        {
            AuthenticateUser("editor", MakeUser(userId: 2, admin: false));
            SetupUserRoleIds();

            var result = await _service.GetAccessibleWebChannelSettingsIdsAsync();

            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task GetAccessibleWebChannelSettingsIdsAsync_WhenRolesDoNotMatchAnyChannel_ReturnsEmpty()
        {
            AuthenticateUser("editor", MakeUser(userId: 3, admin: false));
            SetupUserRoleIds(10);
            SetupRoleNames("SomeUnrelatedRole");
            SetupChannels(("bdoglobal", 5));

            var result = await _service.GetAccessibleWebChannelSettingsIdsAsync();

            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task GetAccessibleWebChannelSettingsIdsAsync_WhenChannelMatchesButNoWebChannelSettings_ReturnsEmpty()
        {
            AuthenticateUser("editor", MakeUser(userId: 4, admin: false));
            SetupUserRoleIds(10);
            SetupRoleNames("bdoglobal");
            SetupChannels(("bdoglobal", 5));
            SetupWebChannelSettings(/* none */);

            var result = await _service.GetAccessibleWebChannelSettingsIdsAsync();

            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task GetAccessibleWebChannelSettingsIdsAsync_WhenRoleEqualsChannelName_ReturnsSettingsIds()
        {
            AuthenticateUser("editor", MakeUser(userId: 5, admin: false));
            SetupUserRoleIds(10);
            SetupRoleNames("bdoglobal");
            SetupChannels(("bdoglobal", 5), ("other", 6));
            SetupWebChannelSettings(77);

            var result = await _service.GetAccessibleWebChannelSettingsIdsAsync();

            Assert.That(result, Is.EqualTo(new[] { 77 }));
        }

        [Test]
        public async Task GetAccessibleWebChannelSettingsIdsAsync_WhenRoleStartsWithChannelPrefix_ReturnsSettingsIds()
        {
            AuthenticateUser("editor", MakeUser(userId: 6, admin: false));
            SetupUserRoleIds(11);
            SetupRoleNames("bdoglobal-editor");
            SetupChannels(("bdoglobal", 5));
            SetupWebChannelSettings(88, 99);

            var result = await _service.GetAccessibleWebChannelSettingsIdsAsync();

            Assert.That(result, Is.EqualTo(new[] { 88, 99 }));
        }

        // ── behavioural: CanAccessAsync ───────────────────────────────────────

        [Test]
        public async Task CanAccessAsync_WhenUserIsAdministrator_ReturnsTrue()
        {
            AuthenticateUser("admin", MakeUser(userId: 7, admin: true));

            var result = await _service.CanAccessAsync(123);

            Assert.That(result, Is.True);
        }

        [Test]
        public async Task CanAccessAsync_WhenIdIsAccessible_ReturnsTrue()
        {
            AuthenticateUser("editor", MakeUser(userId: 8, admin: false));
            SetupUserRoleIds(10);
            SetupRoleNames("bdoglobal");
            SetupChannels(("bdoglobal", 5));
            SetupWebChannelSettings(77);

            var result = await _service.CanAccessAsync(77);

            Assert.That(result, Is.True);
        }

        [Test]
        public async Task CanAccessAsync_WhenIdIsNotAccessible_ReturnsFalse()
        {
            AuthenticateUser("editor", MakeUser(userId: 9, admin: false));
            SetupUserRoleIds(10);
            SetupRoleNames("bdoglobal");
            SetupChannels(("bdoglobal", 5));
            SetupWebChannelSettings(77);

            var result = await _service.CanAccessAsync(999);

            Assert.That(result, Is.False);
        }

        // ── helpers ───────────────────────────────────────────────────────────

        private void AuthenticateUser(string userName, UserInfo user)
        {
            var ctx = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(
                    new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, userName) }, "TestAuth"))
            };
            _httpContextAccessor.SetupGet(a => a.HttpContext).Returns(ctx);
            _userInfoProvider.Setup(p => p.Get(userName)).Returns(user);
        }

        private void SetupUserRoleIds(params int[] roleIds)
        {
            var query = new Mock<ObjectQuery<UserRoleInfo>>();
            _userRoleProvider.Setup(p => p.Get()).Returns(query.Object);
            query.Setup(q => q.WhereEquals(It.IsAny<string>(), It.IsAny<object>())).Returns(query.Object);
            query.Setup(q => q.Column(It.IsAny<string>())).Returns(query.Object);
            query.Setup(q => q.GetEnumerableTypedResult(It.IsAny<System.Data.CommandBehavior>(), It.IsAny<bool>()))
                .Returns(roleIds.Select(id =>
                {
                    var ur = Make<TestUserRoleInfo>();
                    ur.RoleID = id;
                    return (UserRoleInfo)ur;
                }).ToList());
        }

        private void SetupRoleNames(params string[] roleNames)
        {
            var query = new Mock<ObjectQuery<RoleInfo>>();
            _roleProvider.Setup(p => p.Get()).Returns(query.Object);
            query.Setup(q => q.WhereIn(It.IsAny<string>(), It.IsAny<ICollection<int>>())).Returns(query.Object);
            query.Setup(q => q.Column(It.IsAny<string>())).Returns(query.Object);
            query.Setup(q => q.GetEnumerableTypedResult(It.IsAny<System.Data.CommandBehavior>(), It.IsAny<bool>()))
                .Returns(roleNames.Select(name =>
                {
                    var r = Make<TestRoleInfo>();
                    r.RoleName = name;
                    return (RoleInfo)r;
                }).ToList());
        }

        private void SetupChannels(params (string name, int id)[] channels)
        {
            var query = new Mock<ObjectQuery<ChannelInfo>>();
            _channelProvider.Setup(p => p.Get()).Returns(query.Object);
            query.Setup(q => q.GetEnumerableTypedResult(It.IsAny<System.Data.CommandBehavior>(), It.IsAny<bool>()))
                .Returns(channels.Select(c =>
                {
                    var ch = Make<TestChannelInfo>();
                    ch.ChannelName = c.name;
                    ch.ChannelID = c.id;
                    return (ChannelInfo)ch;
                }).ToList());
        }

        private void SetupWebChannelSettings(params int[] settingsIds)
        {
            var query = new Mock<ObjectQuery<WebChannelSettingsInfo>>();
            _webChannelSettingsProvider.Setup(p => p.Get()).Returns(query.Object);
            query.Setup(q => q.WhereIn(It.IsAny<string>(), It.IsAny<ICollection<int>>())).Returns(query.Object);
            query.Setup(q => q.GetEnumerableTypedResult(It.IsAny<System.Data.CommandBehavior>(), It.IsAny<bool>()))
                .Returns(settingsIds.Select(id =>
                {
                    var s = Make<TestWebChannelSettingsInfo>();
                    s.WebChannelSettingsID = id;
                    return (WebChannelSettingsInfo)s;
                }).ToList());
        }

        private static UserInfo MakeUser(int userId, bool admin)
        {
            var user = Make<TestUserInfo>();
            user.UserID = userId;
            user.Admin = admin;
            return user;
        }

        private static T Make<T>() => (T)RuntimeHelpers.GetUninitializedObject(typeof(T));

        // Test subclasses override virtual members so reads/writes bypass Kentico's
        // AbstractInfoBase.GetValue/SetValue, which require an initialised CMS IoC container.
        private sealed class TestUserInfo : UserInfo
        {
            public bool Admin;
            public override int UserID { get; set; }
            public override bool IsInRole(string roleName) => Admin;
        }

        private sealed class TestUserRoleInfo : UserRoleInfo
        {
            public override int RoleID { get; set; }
        }

        private sealed class TestRoleInfo : RoleInfo
        {
            public override string RoleName { get; set; } = string.Empty;
        }

        private sealed class TestChannelInfo : ChannelInfo
        {
            public override string ChannelName { get; set; } = string.Empty;
            public override int ChannelID { get; set; }
        }

        private sealed class TestWebChannelSettingsInfo : WebChannelSettingsInfo
        {
            public override int WebChannelSettingsID { get; set; }
        }
    }
}
