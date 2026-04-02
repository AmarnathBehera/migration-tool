using BDO.GWT.Common.LiveSite.Services;
using BDO.GWT.Domain.Entities;
using BDO.GWT.Domain.Services;
using Moq;

namespace BDO.GWT.GLOBAL.Tests.Components.Services
{
    public class ScriptsServiceTests
    {
        private Mock<ISettingsService> _settingsServiceMock;
        private ScriptsService _service;

        [SetUp]
        public void Setup()
        {
            _settingsServiceMock = new Mock<ISettingsService>();
            _service = new ScriptsService(_settingsServiceMock.Object);
        }

        [Test]
        public async Task GetHeadScriptsAsync_ReturnsScript_WhenSettingExists()
        {
            var settingsDict = new Dictionary<string, object?>
            {
                { "BDO_Global_HeadCode", "<script>head</script>" }
            };

            _settingsServiceMock
                .Setup(x => x.GetCurrentWebsiteSettingsAsync(
                    It.IsAny<string[]>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(settingsDict);

            var result = await _service.GetHeadScriptsAsync(CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.EqualTo("<script>head</script>"));
        }

        [Test]
        public async Task GetHeadScriptsAsync_ReturnsEmpty_WhenSettingIsNull()
        {
            var settingsDict = new Dictionary<string, object?>
            {
                { "BDO_Global_HeadCode", null }
            };

            _settingsServiceMock
                .Setup(x => x.GetCurrentWebsiteSettingsAsync(
                    It.IsAny<string[]>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(settingsDict);

            var result = await _service.GetHeadScriptsAsync(CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Empty);
        }      
       
        [Test]
        public async Task GetTopBodyScriptsAsync_ReturnsScript_WhenSettingExists()
        {
            var setting = new Setting
            {
                Value = "<script>top</script>"
            };

            _settingsServiceMock
                .Setup(x => x.GetCurrentWebsiteSettingAsync(
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(setting);

            var result = await _service.GetTopBodyScriptsAsync(CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.EqualTo("<script>top</script>"));
        }

        [Test]
        public async Task GetTopBodyScriptsAsync_ReturnsNull_WhenSettingIsNull()
        {
            _settingsServiceMock
                .Setup(x => x.GetCurrentWebsiteSettingAsync(
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((Setting)null);

            var result = await _service.GetTopBodyScriptsAsync(CancellationToken.None);

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetBottomBodyScriptsAsync_ReturnsScript_WhenSettingExists()
        {
            var setting = new Setting
            {
                Value = "<script>bottom</script>"
            };

            _settingsServiceMock
                .Setup(x => x.GetCurrentWebsiteSettingAsync(
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(setting);

            var result = await _service.GetBottomBodyScriptsAsync(CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.EqualTo("<script>bottom</script>"));
        }

        [Test]
        public async Task GetBottomBodyScriptsAsync_ReturnsNull_WhenSettingIsNull()
        {
            _settingsServiceMock
                .Setup(x => x.GetCurrentWebsiteSettingAsync(
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((Setting)null);

            var result = await _service.GetBottomBodyScriptsAsync(CancellationToken.None);

            Assert.That(result, Is.Null);
        }
        [Test]
        public async Task GetTopBodyScriptsAsync_ReturnsEmpty_WhenValueIsEmpty()
        {
            var setting = new Setting
            {
                Value = ""
            };

            _settingsServiceMock
                .Setup(x => x.GetCurrentWebsiteSettingAsync(
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(setting);

            var result = await _service.GetTopBodyScriptsAsync(CancellationToken.None);

            Assert.That(result, Is.Empty);
        }
    }
}