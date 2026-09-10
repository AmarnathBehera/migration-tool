using BDO;
using BDO.GWT.Xperience.Admin.Pages.ProjectSettings.WebChannelSettings;
using GWTXbyKWeb.Pages.BDOIntegrations;
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.Forms;
using System.Threading.Tasks;


namespace GWTXbyKWeb.Pages.BDOIntegrations
{
    public class BdoIntegrationEditPage : InfoEditPage<FormSubmissionSubscriptionsInfo>
    {
        private readonly IChannelAccessService channelAccessService;

        [PageParameter(typeof(IntPageModelBinder))]
        public override int ObjectId { get; set; }

        [PageParameter(typeof(IntPageModelBinder))]
        public int WebChannelSettingsId { get; set; }

        public BdoIntegrationEditPage(
            IFormComponentMapper formComponentMapper,
            IFormDataBinder formDataBinder,
            IChannelAccessService channelAccessService)
            : base(formComponentMapper, formDataBinder)
        {
            this.channelAccessService = channelAccessService;
        }

        public override async Task ConfigurePage()
        {
            bool canAccess = await channelAccessService.CanAccessAsync(WebChannelSettingsId);
            if (!canAccess)
            {
                PageConfiguration.SubmitConfiguration.Visible = false;
                PageConfiguration.Headline = "Access Denied";
                return;
            }

            PageConfiguration.UIFormName = "FormSubmissionSubscriptions";
            await base.ConfigurePage();
        }
    }
}