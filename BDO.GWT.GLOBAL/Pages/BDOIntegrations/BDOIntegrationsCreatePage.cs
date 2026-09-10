using BDO;
using CMS.DataEngine;
using GWTXbyKWeb.Pages.BDOIntegrations;
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.Forms;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GWTXbyKWeb.Pages.BDOIntegrations
{
    public class BdoIntegrationsCreatePage : CreatePage<FormSubmissionSubscriptionsInfo, BdoIntegrationsEditSection>
    {
        private readonly IInfoProvider<FormSubmissionSubscriptionsInfo> formSubmissionSubscriptionsInfoProvider;

        [PageParameter(typeof(IntPageModelBinder))]
        public int WebChannelSettingsId { get; set; }

        public BdoIntegrationsCreatePage(
            IFormComponentMapper formComponentMapper,
            IFormDataBinder formDataBinder,
            IPageLinkGenerator pageLinkGenerator,
            IInfoProvider<FormSubmissionSubscriptionsInfo> formSubmissionSubscriptionsInfoProvider)
            : base(formComponentMapper, formDataBinder, pageLinkGenerator)
        {
            this.formSubmissionSubscriptionsInfoProvider = formSubmissionSubscriptionsInfoProvider;
        }

        public override Task ConfigurePage()
        {
            PageConfiguration.UIFormName = "FormSubmissionSubscriptions";
            return base.ConfigurePage();
        }

        protected override async Task<ICommandResponse> GetSubmitSuccessResponse(
            FormSubmissionSubscriptionsInfo savedInfoObject, ICollection<IFormItem> items)
        {
            // Record is already persisted at this point — stamp the channel FK and
            // re-save, since we couldn't locate a pre-save hook to do this earlier.
            if (savedInfoObject.ChannelSnippetWebChannelSettingsID != WebChannelSettingsId)
            {
                savedInfoObject.ChannelSnippetWebChannelSettingsID = WebChannelSettingsId;
                await formSubmissionSubscriptionsInfoProvider.SetAsync(savedInfoObject);
            }

            var result = new EditPageSuccessFormSubmissionResult()
            {
                Items = await items.OnlyVisible().GetClientProperties(),
                ObjectDisplayName = savedInfoObject.Displayname,
                ObjectId = savedInfoObject.FormSubmissionSubscriptionsID,
                RefetchAll = RefetchAll
            };

            return ResponseFrom(result).AddSuccessMessage(LocalizationService?.GetString("base.forms.saved"));
        }
    }
}