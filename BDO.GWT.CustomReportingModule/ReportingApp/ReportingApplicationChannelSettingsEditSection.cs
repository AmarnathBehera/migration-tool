using Kentico.Xperience.Admin.Base;
[assembly: UIPage(
    parentType: typeof(BDO.GWT.CustomReportingModule.ReportingApp.ReportingApplicationChannelSettingsListingPage),
    slug: PageParameterConstants.PARAMETERIZED_SLUG,
    uiPageType: typeof(BDO.GWT.CustomReportingModule.ReportingApp.ReportingApplicationChannelSettingsEditSection),
    name: "Edit Reporting Application",
    templateName: TemplateNames.SECTION_LAYOUT,
    order: 0)]

namespace BDO.GWT.CustomReportingModule.ReportingApp
{
    public class ReportingApplicationChannelSettingsEditSection : EditSectionPage<ReportingChannelSettingInfo>
    {
    }
}
