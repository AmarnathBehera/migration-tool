using System.Reflection;
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
    }
}
