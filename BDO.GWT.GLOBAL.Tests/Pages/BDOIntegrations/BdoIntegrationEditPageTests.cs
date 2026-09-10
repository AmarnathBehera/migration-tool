using BDO;
using BDO.GWT.Xperience.Admin.Pages.ProjectSettings.WebChannelSettings;
using GWTXbyKWeb.Pages.BDOIntegrations;
using Kentico.Xperience.Admin.Base.Forms;
using Moq;
using NUnit.Framework;
using System.Threading.Tasks;

namespace GWTXbyKWeb.Tests.Pages.BDOIntegrations
{
    [TestFixture]
    public class BdoIntegrationEditPageTests
    {
        private Mock<IFormComponentMapper> formComponentMapperMock;
        private Mock<IFormDataBinder> formDataBinderMock;
        private Mock<IChannelAccessService> channelAccessServiceMock;
        private BdoIntegrationEditPage page;

        [SetUp]
        public void SetUp()
        {
            formComponentMapperMock = new Mock<IFormComponentMapper>();
            formDataBinderMock = new Mock<IFormDataBinder>();
            channelAccessServiceMock = new Mock<IChannelAccessService>();

            page = new BdoIntegrationEditPage(
                formComponentMapperMock.Object,
                formDataBinderMock.Object,
                channelAccessServiceMock.Object)
            {
                WebChannelSettingsId = 7
            };
        }

        [Test]
        public async Task ConfigurePage_NoAccess_SetsAccessDeniedHeadlineAndHidesSubmit()
        {
            channelAccessServiceMock.Setup(s => s.CanAccessAsync(7)).ReturnsAsync(false);

            await page.ConfigurePage();

            Assert.Multiple(() =>
            {
                Assert.That(page.PageConfiguration.Headline, Is.EqualTo("Access Denied"));
                Assert.That(page.PageConfiguration.SubmitConfiguration.Visible, Is.False);
            });
        }

        [Test]
        public async Task ConfigurePage_HasAccess_SetsUIFormName()
        {
            channelAccessServiceMock.Setup(s => s.CanAccessAsync(7)).ReturnsAsync(true);

            // VERIFY: base.ConfigurePage() may require additional Kentico admin
            // bootstrapping to run without throwing in an isolated unit test —
            // if so, this assertion should be moved before the base call via a
            // TestableBdoIntegrationEditPage subclass that stops short of calling base.
            await page.ConfigurePage();

            Assert.That(page.PageConfiguration.UIFormName, Is.EqualTo("FormSubmissionSubscriptions"));
        }
    }
}