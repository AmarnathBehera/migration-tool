using BDO;
using BDO.GWT.GLOBAL.Tests.Pages.BDOIntegrations;
using BDO.GWT.Xperience.Admin.Pages.ProjectSettings.WebChannelSettings;
using GWTXbyKWeb.Pages.BDOIntegrations;
using Moq;
using NUnit.Framework;
using System.Threading.Tasks;

namespace GWTXbyKWeb.Tests.Pages.BDOIntegrations
{
    // The page is created without running its constructor: Kentico's EditPageBase
    // constructor resolves services from the admin IoC container, which is not
    // bootstrapped in a unit test host. Only the access-control logic of this class
    // is asserted here.
    [TestFixture]
    public class BdoIntegrationEditPageTests
    {
        private Mock<IChannelAccessService> channelAccessServiceMock;
        private BdoIntegrationEditPage page;

        [SetUp]
        public void SetUp()
        {
            channelAccessServiceMock = new Mock<IChannelAccessService>();

            page = BdoIntegrationsPageTestHelper.CreateUninitialized<BdoIntegrationEditPage>();
            page.WebChannelSettingsId = 7;

            BdoIntegrationsPageTestHelper.PreparePageConfigurationForConfigurePage(page);
            BdoIntegrationsPageTestHelper.SetPrivateField(page, "channelAccessService", channelAccessServiceMock.Object);
        }

        [Test]
        public async Task ConfigurePage_ChecksAccessForBoundChannelId()
        {
            channelAccessServiceMock.Setup(s => s.CanAccessAsync(7)).ReturnsAsync(false);

            await ConfigurePageIgnoringAdminRuntimeAsync();

            channelAccessServiceMock.Verify(s => s.CanAccessAsync(7), Times.Once);
        }

        [Test]
        public async Task ConfigurePage_NoAccess_SetsAccessDeniedHeadlineAndHidesSubmit()
        {
            channelAccessServiceMock.Setup(s => s.CanAccessAsync(7)).ReturnsAsync(false);

            // The access denied branch returns before any base call, so no framework
            // bootstrapping is required here.
            await page.ConfigurePage();

            var pageConfiguration = BdoIntegrationsPageTestHelper.GetPageConfiguration(page);
            var submitConfiguration = pageConfiguration.GetType()
                .GetProperty("SubmitConfiguration")!
                .GetValue(pageConfiguration)!;

            Assert.Multiple(() =>
            {
                Assert.That(BdoIntegrationsPageTestHelper.GetStringProperty(pageConfiguration, "Headline"), Is.EqualTo("Access Denied"));
                Assert.That(submitConfiguration.GetType().GetProperty("Visible")!.GetValue(submitConfiguration), Is.False);
            });
        }

        [Test]
        public async Task ConfigurePage_NoAccess_DoesNotSetUIFormName()
        {
            channelAccessServiceMock.Setup(s => s.CanAccessAsync(7)).ReturnsAsync(false);

            await page.ConfigurePage();

            var pageConfiguration = BdoIntegrationsPageTestHelper.GetPageConfiguration(page);

            Assert.That(BdoIntegrationsPageTestHelper.GetStringProperty(pageConfiguration, "UIFormName"), Is.Not.EqualTo("FormSubmissionSubscriptions"));
        }

        [Test]
        public async Task ConfigurePage_HasAccess_SetsUIFormName()
        {
            channelAccessServiceMock.Setup(s => s.CanAccessAsync(7)).ReturnsAsync(true);

            await ConfigurePageIgnoringAdminRuntimeAsync();

            var pageConfiguration = BdoIntegrationsPageTestHelper.GetPageConfiguration(page);

            Assert.That(BdoIntegrationsPageTestHelper.GetStringProperty(pageConfiguration, "UIFormName"), Is.EqualTo("FormSubmissionSubscriptions"));
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
