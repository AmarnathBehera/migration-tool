using BDO;
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
    // These tests focus on the one piece of actual decision logic in this class:
    // whether channelAccessService is consulted, and with what ID.
    [TestFixture]
    public class BdoIntegrationListingPageTests
    {
        private Mock<IChannelAccessService> channelAccessServiceMock;
        private BdoIntegrationListingPage page;

        [SetUp]
        public void SetUp()
        {
            channelAccessServiceMock = new Mock<IChannelAccessService>();
            page = new BdoIntegrationListingPage(channelAccessServiceMock.Object)
            {
                WebChannelSettingsId = 42
            };
        }

        [Test]
        public async Task ConfigurePage_ChecksAccessForBoundChannelId()
        {
            channelAccessServiceMock
                .Setup(s => s.CanAccessAsync(42))
                .ReturnsAsync(true);

            // VERIFY: ConfigurePage() likely throws or no-ops on parts of
            // PageConfiguration that require a fully bootstrapped Kentico admin
            // context (PageInvoker, UITreeNode resolution for AddEditRowAction's
            // ancestor parameters, etc). If a bare call to ConfigurePage() throws
            // in isolation, wrap this in a TestableBdoIntegrationListingPage that
            // overrides just the access + query-filter portion, following the
            // TestableXxx pattern used elsewhere (e.g. TestableRecaptchaValidationService).
            await page.ConfigurePage();

            channelAccessServiceMock.Verify(s => s.CanAccessAsync(42), Times.Once);
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
    }
}