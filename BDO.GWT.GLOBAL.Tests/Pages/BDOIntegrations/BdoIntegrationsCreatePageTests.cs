using BDO;
using BDO.GWT.GLOBAL.Tests.Pages.BDOIntegrations;
using CMS.DataEngine;
using GWTXbyKWeb.Pages.BDOIntegrations;
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.Forms;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GWTXbyKWeb.Tests.Pages.BDOIntegrations
{
    // Exposes the protected GetSubmitSuccessResponse for direct testing,
    // same pattern as TestableRecaptchaValidationService etc.
    internal class TestableBdoIntegrationsCreatePage : BdoIntegrationsCreatePage
    {
        public TestableBdoIntegrationsCreatePage(
            IFormComponentMapper formComponentMapper,
            IFormDataBinder formDataBinder,
            IPageLinkGenerator pageLinkGenerator,
            IInfoProvider<FormSubmissionSubscriptionsInfo> infoProvider)
            : base(formComponentMapper, formDataBinder, pageLinkGenerator, infoProvider)
        {
        }

        public Task<ICommandResponse> PublicGetSubmitSuccessResponse(
            FormSubmissionSubscriptionsInfo savedInfoObject, ICollection<IFormItem> items)
            => GetSubmitSuccessResponse(savedInfoObject, items);
    }

    // The Kentico generated info class cannot be proxied by Moq and its real property
    // accessors require an initialised data engine, so the virtual members used by the
    // page are overridden with plain in-memory values.
    internal class StubFormSubmissionSubscriptionsInfo : FormSubmissionSubscriptionsInfo
    {
        public override int FormSubmissionSubscriptionsID { get; set; }

        public override int ChannelSnippetWebChannelSettingsID { get; set; }

        public override string Displayname { get; set; } = string.Empty;
    }

    // The page is created without running its constructor: Kentico's CreatePage
    // constructor resolves services from the admin IoC container, which is not
    // bootstrapped in a unit test host. Only the channel FK stamping logic of this
    // class is asserted here; the response building that follows it belongs to the
    // Kentico admin runtime.
    [TestFixture]
    public class BdoIntegrationsCreatePageTests
    {
        private Mock<IInfoProvider<FormSubmissionSubscriptionsInfo>> infoProviderMock;
        private TestableBdoIntegrationsCreatePage page;

        [SetUp]
        public void SetUp()
        {
            infoProviderMock = new Mock<IInfoProvider<FormSubmissionSubscriptionsInfo>>();

            page = BdoIntegrationsPageTestHelper.CreateUninitialized<TestableBdoIntegrationsCreatePage>();
            page.WebChannelSettingsId = 15;

            BdoIntegrationsPageTestHelper.SetPrivateField(page, "formSubmissionSubscriptionsInfoProvider", infoProviderMock.Object);
        }

        [Test]
        public async Task GetSubmitSuccessResponse_FkNotSet_StampsChannelIdAndReSaves()
        {
            var savedInfo = CreateSavedInfo(channelSettingsId: 0);

            await SubmitIgnoringAdminRuntimeAsync(savedInfo);

            Assert.Multiple(() =>
            {
                Assert.That(savedInfo.ChannelSnippetWebChannelSettingsID, Is.EqualTo(15));
                infoProviderMock.Verify(p => p.SetAsync(savedInfo, It.IsAny<CancellationToken>()), Times.Once);
            });
        }

        [Test]
        public async Task GetSubmitSuccessResponse_FkAlreadyCorrect_DoesNotReSave()
        {
            var savedInfo = CreateSavedInfo(channelSettingsId: 15);

            await SubmitIgnoringAdminRuntimeAsync(savedInfo);

            infoProviderMock.Verify(p => p.SetAsync(It.IsAny<FormSubmissionSubscriptionsInfo>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        private static StubFormSubmissionSubscriptionsInfo CreateSavedInfo(int channelSettingsId) =>
            new StubFormSubmissionSubscriptionsInfo
            {
                ChannelSnippetWebChannelSettingsID = channelSettingsId,
                FormSubmissionSubscriptionsID = 101,
                Displayname = "Test Integration",
            };

        private async Task SubmitIgnoringAdminRuntimeAsync(FormSubmissionSubscriptionsInfo savedInfo)
        {
            try
            {
                await page.PublicGetSubmitSuccessResponse(savedInfo, new List<IFormItem>());
            }
            catch
            {
                // Building the command response requires the Kentico admin runtime, which is not
                // available in an isolated unit test host. The FK stamping asserted here runs first.
            }
        }
    }
}
