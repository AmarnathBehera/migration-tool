using BDO.GWT.CustomReportingModule;
using CMS.Membership;
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.UIPages;
[assembly: UIApplication(
    identifier: ReportingApplicationPage.IDENTIFIER,
    type: typeof(ReportingApplicationPage),
    slug: "reporting-application",
    name: "Reporting Application",
    category: BaseApplicationCategories.CONFIGURATION,
    icon: Icons.Table,
    templateName: TemplateNames.SECTION_LAYOUT)]
namespace BDO.GWT.CustomReportingModule
{
    [UIPermission(SystemPermissions.VIEW)]
    [UIPermission(SystemPermissions.CREATE)]
    [UIPermission(SystemPermissions.DELETE)]
    [UIPermission(SystemPermissions.UPDATE)]
    public class ReportingApplicationPage : ApplicationPage
    {
        public const string IDENTIFIER = "BDO.CustomTableSettingsApplication";
    }
}
