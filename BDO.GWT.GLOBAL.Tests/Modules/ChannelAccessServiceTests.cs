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
    }
}
