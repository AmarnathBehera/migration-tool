using BDO.GWT.CustomReportingModule.ReportingApp;
using CMS.Membership;
using Kentico.Xperience.Admin.Base;

// Register Results and View record under the Queries page
[assembly: UIPage(
    parentType: typeof(EditQuery),
    slug: "results",
    uiPageType: typeof(ResultListing),
    name: "Results",
    templateName: TemplateNames.LISTING,
    order: UIPageOrder.NoOrder)]

[assembly: UIPage(
    parentType: typeof(ResultListing),
    slug: PageParameterConstants.PARAMETERIZED_SLUG,
    uiPageType: typeof(ViewRecord),
    name: "View record",
    templateName: TemplateNames.EDIT,
    order: UIPageOrder.NoOrder)]
namespace BDO.GWT.CustomReportingModule.ReportingApp
{
    [UIPermission(SystemPermissions.VIEW)]
    [UIPermission(SystemPermissions.UPDATE, "Execute")]
    public class ReportingQueryBrowserPage : ApplicationPage
    {
        [PageParameter(typeof(IntPageModelBinder))]
        public int ReportingChannelSettingId { get; set; }
    }
}
