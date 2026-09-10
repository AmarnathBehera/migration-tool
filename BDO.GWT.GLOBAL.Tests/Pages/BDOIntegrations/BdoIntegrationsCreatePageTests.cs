using BDO;
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

    [TestFixture]
    public class BdoIntegrationsCreatePageTests
    {
        private Mock<IFormComponentMapper> formComponentMapperMock;
        private Mock<IFormDataBinder> formDataBinderMock;
        private Mock<IPageLinkGenerator> pageLinkGeneratorMock;
        private Mock<IInfoProvider<FormSubmissionSubscriptionsInfo>> infoProviderMock;
        private TestableBdoIntegrationsCreatePage page;

        [SetUp]
        public void SetUp()
        {
            formComponentMapperMock = new Mock<IFormComponentMapper>();
            formDataBinderMock = new Mock<IFormDataBinder>();
            pageLinkGeneratorMock = new Mock<IPageLinkGenerator>();
            infoProviderMock = new Mock<IInfoProvider<FormSubmissionSubscriptionsInfo>>();

            page = new TestableBdoIntegrationsCreatePage(
                formComponentMapperMock.Object,
                formDataBinderMock.Object,
                pageLinkGeneratorMock.Object,
                infoProviderMock.Object)
            {
                WebChannelSettingsId = 15
            };
        }

        [Test]
        public async Task GetSubmitSuccessResponse_FkNotSet_StampsChannelIdAndReSaves()
        {
            var savedInfoMock = new Mock<FormSubmissionSubscriptionsInfo>();
            savedInfoMock.SetupProperty(i => i.ChannelSnippetWebChannelSettingsID, 0);
            savedInfoMock.SetupGet(i => i.FormSubmissionSubscriptionsID).Returns(101);
            savedInfoMock.SetupGet(i => i.Displayname).Returns("Test Integration");
            var savedInfo = savedInfoMock.Object;

            var response = await page.PublicGetSubmitSuccessResponse(savedInfo, new List<IFormItem>());

            Assert.Multiple(() =>
            {
                Assert.That(savedInfo.ChannelSnippetWebChannelSettingsID, Is.EqualTo(15));
                infoProviderMock.Verify(p => p.Set(savedInfo), Times.Once);
                Assert.That(response, Is.Not.Null);
            });
        }

        [Test]
        public async Task GetSubmitSuccessResponse_FkAlreadyCorrect_DoesNotReSave()
        {
            var savedInfoMock = new Mock<FormSubmissionSubscriptionsInfo>();
            savedInfoMock.SetupProperty(i => i.ChannelSnippetWebChannelSettingsID, 15);
            savedInfoMock.SetupGet(i => i.FormSubmissionSubscriptionsID).Returns(102);
            savedInfoMock.SetupGet(i => i.Displayname).Returns("Already Correct");
            var savedInfo = savedInfoMock.Object;

            await page.PublicGetSubmitSuccessResponse(savedInfo, new List<IFormItem>());

            infoProviderMock.Verify(p => p.Set(It.IsAny<FormSubmissionSubscriptionsInfo>()), Times.Never);
        }
    }
}