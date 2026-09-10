using BDO;
using BDO.GWT.Xperience.Admin.Pages.ProjectSettings.WebChannelSettings;
using CMS.DataEngine;
using CMS.Membership;
using GWTXbyKWeb.Pages.BDOIntegrations;
using Kentico.Xperience.Admin.Base;
using System.Linq;
using System.Threading.Tasks;

namespace GWTXbyKWeb.Pages.BDOIntegrations
{
    public class BdoIntegrationListingPage : ListingPage
    {
        private readonly IChannelAccessService channelAccessService;

        [PageParameter(typeof(IntPageModelBinder))]
        public int WebChannelSettingsId { get; set; }

        protected override string ObjectType => FormSubmissionSubscriptionsInfo.OBJECT_TYPE;

        public BdoIntegrationListingPage(IChannelAccessService channelAccessService)
        {
            this.channelAccessService = channelAccessService;
        }

        public override async Task ConfigurePage()
        {
            bool canAccess = await channelAccessService.CanAccessAsync(WebChannelSettingsId);

            PageConfiguration.ColumnConfigurations
                        .AddColumn(nameof(FormSubmissionSubscriptionsInfo.Displayname), "Display name")
                        .AddColumn(nameof(FormSubmissionSubscriptionsInfo.Codename), "Code name")
                        .AddColumn(nameof(FormSubmissionSubscriptionsInfo.Form), "Form")
                        .AddColumn(nameof(FormSubmissionSubscriptionsInfo.Method), "Method")
                        .AddColumn(nameof(FormSubmissionSubscriptionsInfo.Endpoint), "Endpoint");

            if (canAccess)
            {
                var ancestorParams = new PageParameterValues
                {
                    { typeof(BdoIntegrationsChannelEditSection), WebChannelSettingsId }
                };

                PageConfiguration.HeaderActions.AddLink<BdoIntegrationsCreatePage>(
                    "New setting",
                    parameters: ancestorParams);

                PageConfiguration.AddEditRowAction<BdoIntegrationsEditSection>(
                    parameters: ancestorParams);

                PageConfiguration.TableActions.AddDeleteAction(nameof(Delete));
            }

            PageConfiguration.QueryModifiers.Add(new QueryModifier((query, _) =>
            {
                return canAccess
                    ? query.WhereEquals(
                        nameof(FormSubmissionSubscriptionsInfo.ChannelSnippetWebChannelSettingsID),
                        WebChannelSettingsId)
                    : query.WhereEquals(nameof(FormSubmissionSubscriptionsInfo.FormSubmissionSubscriptionsID), 0);
            }));

            await base.ConfigurePage();
        }

        [PageCommand(Permission = SystemPermissions.DELETE)]
        public override Task<ICommandResponse<RowActionResult>> Delete(int id) => base.Delete(id);
    }
}