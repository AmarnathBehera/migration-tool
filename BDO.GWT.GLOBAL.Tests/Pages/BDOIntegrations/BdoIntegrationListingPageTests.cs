using BDO;
using BDO.GWT.GLOBAL.Tests.Pages.BDOIntegrations;
using BDO.GWT.Xperience.Admin.Pages.ProjectSettings.WebChannelSettings;
using CMS.Membership;
using GWTXbyKWeb.Pages.BDOIntegrations;
using Kentico.Xperience.Admin.Base;
using Moq;
using NUnit.Framework;
using System.Threading.Tasks;

namespace GWTXbyKWeb.Tests.Pages.BDOIntegrations
{
    // Kentico's ListingPage.ConfigurePage() sets up PageConfiguration internals that
    // are largely framework plumbing rather than logic worth asserting directly.
    // These tests focus on the actual decision logic in this class: whether
    // channelAccessService is consulted, with what ID, and what is configured for
    // users without access. The page is created without running its constructor and the
    // base call is allowed to fail, because the Kentico admin runtime (IoC container,
    // page invoker) is not bootstrapped in a unit test host.
    [TestFixture]
    public class BdoIntegrationListingPageTests
    {
        private Mock<IChannelAccessService> channelAccessServiceMock;
        private BdoIntegrationListingPage page;

        [SetUp]
        public void SetUp()
        {
            channelAccessServiceMock = new Mock<IChannelAccessService>();

            page = BdoIntegrationsPageTestHelper.CreateUninitialized<BdoIntegrationListingPage>();
            page.WebChannelSettingsId = 42;

            BdoIntegrationsPageTestHelper.PreparePageConfigurationForConfigurePage(page);
            BdoIntegrationsPageTestHelper.SetPrivateField(page, "channelAccessService", channelAccessServiceMock.Object);
        }

        [Test]
        public async Task ConfigurePage_ChecksAccessForBoundChannelId()
        {
            channelAccessServiceMock
                .Setup(s => s.CanAccessAsync(42))
                .ReturnsAsync(true);

            await ConfigurePageIgnoringAdminRuntimeAsync();

            channelAccessServiceMock.Verify(s => s.CanAccessAsync(42), Times.Once);
        }

        [Test]
        public async Task ConfigurePage_ConfiguresExpectedListingColumns()
        {
            channelAccessServiceMock
                .Setup(s => s.CanAccessAsync(It.IsAny<int>()))
                .ReturnsAsync(false);

            await ConfigurePageIgnoringAdminRuntimeAsync();

            var pageConfiguration = BdoIntegrationsPageTestHelper.GetPageConfiguration(page);

            Assert.That(BdoIntegrationsPageTestHelper.GetEnumerableCount(pageConfiguration, "ColumnConfigurations"), Is.EqualTo(5));
        }

        [Test]
        public async Task ConfigurePage_NoAccess_DoesNotAddHeaderOrTableActions()
        {
            channelAccessServiceMock
                .Setup(s => s.CanAccessAsync(It.IsAny<int>()))
                .ReturnsAsync(false);

            await ConfigurePageIgnoringAdminRuntimeAsync();

            var pageConfiguration = BdoIntegrationsPageTestHelper.GetPageConfiguration(page);

            Assert.Multiple(() =>
            {
                Assert.That(BdoIntegrationsPageTestHelper.GetEnumerableCount(pageConfiguration, "HeaderActions"), Is.Zero);
                Assert.That(BdoIntegrationsPageTestHelper.GetEnumerableCount(pageConfiguration, "TableActions"), Is.Zero);
            });
        }

        [Test]
        public async Task ConfigurePage_NoAccess_AddsQueryModifier()
        {
            channelAccessServiceMock
                .Setup(s => s.CanAccessAsync(It.IsAny<int>()))
                .ReturnsAsync(false);

            await ConfigurePageIgnoringAdminRuntimeAsync();

            var pageConfiguration = BdoIntegrationsPageTestHelper.GetPageConfiguration(page);

            Assert.That(BdoIntegrationsPageTestHelper.GetEnumerableCount(pageConfiguration, "QueryModifiers"), Is.EqualTo(1));
        }

        [Test]
        public void Delete_RequiresDeletePermission()
        {
            // Confirms the [PageCommand(Permission = SystemPermissions.DELETE)]
            // attribute is present, mirroring the pattern from
            // WebChannelSettingsListingPage / ChannelSettingsListingPage's Delete override.
            var method = typeof(BdoIntegrationListingPage).GetMethod(nameof(BdoIntegrationListingPage.Delete));
            var attr = (PageCommandAttribute)Attribute.GetCustomAttribute(method, typeof(PageCommandAttribute));

            Assert.That(attr, Is.Not.Null);
            Assert.That(attr.Permission, Is.EqualTo(SystemPermissions.DELETE));
        }

        private async Task ConfigurePageIgnoringAdminRuntimeAsync()
        {
            try
            {
                await page.ConfigurePage();
            }
            catch
            {
                // The Kentico admin runtime is not available in an isolated unit test host;
                // everything asserted here is executed before the framework call that throws.
            }
        }
    }
}
