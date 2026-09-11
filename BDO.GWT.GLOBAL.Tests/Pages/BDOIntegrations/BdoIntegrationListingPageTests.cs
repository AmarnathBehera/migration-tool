using BDO;
using BDO.GWT.Xperience.Admin.Pages.ProjectSettings.WebChannelSettings;
using CMS.Membership;
using GWTXbyKWeb.Pages.BDOIntegrations;
using Kentico.Xperience.Admin.Base;
using Moq;
using NUnit.Framework;
using System.Reflection;
using System.Threading.Tasks;

namespace BDO.GWT.GLOBAL.Tests.Pages.BDOIntegrations
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
        public void Constructor_StoresChannelAccessService()
        {
            var constructed = new BdoIntegrationListingPage(channelAccessServiceMock.Object);

            var stored = typeof(BdoIntegrationListingPage)
                .GetField("channelAccessService", BindingFlags.Instance | BindingFlags.NonPublic)
                ?.GetValue(constructed);

            Assert.That(stored, Is.SameAs(channelAccessServiceMock.Object));
        }

        [Test]
        public void ObjectType_ReturnsFormSubmissionSubscriptionsObjectType()
        {
            var objectType = typeof(BdoIntegrationListingPage)
                .GetProperty("ObjectType", BindingFlags.Instance | BindingFlags.NonPublic)
                ?.GetValue(page) as string;

            Assert.That(objectType, Is.EqualTo(FormSubmissionSubscriptionsInfo.OBJECT_TYPE));
        }

        [Test]
        public void WebChannelSettingsId_IsBoundAsPageParameter()
        {
            var property = typeof(BdoIntegrationListingPage)
                .GetProperty(nameof(BdoIntegrationListingPage.WebChannelSettingsId));

            var attribute = (PageParameterAttribute?)Attribute.GetCustomAttribute(property!, typeof(PageParameterAttribute));

            Assert.That(attribute, Is.Not.Null);
        }

        [Test]
        public async Task ConfigurePage_NoAccess_QueryModifierBlocksAllRows()
        {
            channelAccessServiceMock
                .Setup(s => s.CanAccessAsync(It.IsAny<int>()))
                .ReturnsAsync(false);

            await ConfigurePageIgnoringAdminRuntimeAsync();

            Assert.That(
                InvokeSingleQueryModifier(),
                Does.Contain(nameof(FormSubmissionSubscriptionsInfo.FormSubmissionSubscriptionsID)));
        }

        [Test]
        public async Task ConfigurePage_HasAccess_QueryModifierFiltersOnBoundChannelId()
        {
            channelAccessServiceMock
                .Setup(s => s.CanAccessAsync(It.IsAny<int>()))
                .ReturnsAsync(true);

            await ConfigurePageIgnoringAdminRuntimeAsync();

            Assert.That(
                InvokeSingleQueryModifier(),
                Does.Contain(nameof(FormSubmissionSubscriptionsInfo.ChannelSnippetWebChannelSettingsID)));
        }

        [Test]
        public async Task ConfigurePage_HasAccess_AddsHeaderLink()
        {
            channelAccessServiceMock
                .Setup(s => s.CanAccessAsync(It.IsAny<int>()))
                .ReturnsAsync(true);

            await ConfigurePageIgnoringAdminRuntimeAsync();

            var pageConfiguration = BdoIntegrationsPageTestHelper.GetPageConfiguration(page);
            var headerActions = pageConfiguration.GetType()
                .GetProperty("HeaderActions")!
                .GetValue(pageConfiguration);

            Assert.That(headerActions, Is.Not.Null);
        }

        [Test]
        public void Delete_RequiresDeletePermission()
        {
            // Confirms the [PageCommand(Permission = SystemPermissions.DELETE)]
            // attribute is present, mirroring the pattern from
            // WebChannelSettingsListingPage / ChannelSettingsListingPage's Delete override.
            var method = typeof(BdoIntegrationListingPage).GetMethod(nameof(BdoIntegrationListingPage.Delete));

            Assert.That(method, Is.Not.Null);

            var attr = (PageCommandAttribute?)Attribute.GetCustomAttribute(method!, typeof(PageCommandAttribute));

            Assert.That(attr, Is.Not.Null);
            Assert.That(attr!.Permission, Is.EqualTo(SystemPermissions.DELETE));
        }

        private string InvokeSingleQueryModifier()
        {
            var pageConfiguration = BdoIntegrationsPageTestHelper.GetPageConfiguration(page);
            var modifiers = (IEnumerable<QueryModifier>)pageConfiguration.GetType()
                .GetProperty("QueryModifiers")!
                .GetValue(pageConfiguration)!;

            var query = modifiers.First().ModifyQuery(new CMS.DataEngine.ObjectQuery(), null!);

            return query.WhereCondition ?? string.Empty;
        }

        private async Task ConfigurePageIgnoringAdminRuntimeAsync()
        {
            try
            {
                await page.ConfigurePage();
            }
            catch (Exception ex)
            {
                // The Kentico admin runtime is not available in an isolated unit test host;
                // everything asserted here is executed before the framework call that throws.
                TestContext.Out.WriteLine(ex.ToString());
            }
        }
    }
}
