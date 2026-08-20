using BDO.GWT.CustomReportingModule.ReportingApp;
using CMS.DataEngine;
using CMS.Membership;
using Kentico.Xperience.Admin.Base;
using Microsoft.Extensions.Localization;

[assembly: UIPage(
    parentType: typeof(BDO.GWT.CustomReportingModule.ReportingApp.ReportingApplicationChannelSettingsEditSection),
    slug: "reporting-report",
    uiPageType: typeof(BDO.GWT.CustomReportingModule.ReportingApp.ReportingReportListingPage),
    name: "Reporting Report Settings",
    templateName: TemplateNames.SECTION_LAYOUT,
    order: 10)]

[assembly: UIPage(
    parentType: typeof(ReportingReportListingPage),
    slug: "queries",
    uiPageType: typeof(EditQuery),
    name: "Queries",
    templateName: "@bdo/custom-components/EditQuery",
    order: UIPageOrder.First)]

[assembly: UIPage(
    parentType: typeof(ReportingReportListingPage),
    slug: "query-browser",
    uiPageType: typeof(ReportingQueryBrowserPage),
    name: "Query Browser",
    templateName: TemplateNames.SECTION_LAYOUT,
    order: 10)]

namespace BDO.GWT.CustomReportingModule.ReportingApp
{
    [UIPermission(EXPORT_PERMISSION)]
    [UIPermission(SystemPermissions.VIEW)]
    [UIPermission(SystemPermissions.UPDATE, "Execute")]
    public class ReportingReportListingPage : ListingPage
    {
        public const string EXPORT_PERMISSION = "Export";
        public const string IDENTIFIER = "DancingGoat.Reporting";
        private readonly IStringLocalizer<SharedResources> stringLocalizer;
        protected override string ObjectType => ReportingReportInfo.OBJECT_TYPE;
        [PageParameter(typeof(IntPageModelBinder))]
        public int ReportingReportId { get; set; }

        public ReportingReportListingPage(IStringLocalizer<SharedResources> stringLocalizer) : base()
        {
            this.stringLocalizer = stringLocalizer;
        }
        public override Task ConfigurePage()
        {
            PageConfiguration.ColumnConfigurations
                .AddColumn(nameof(ReportingReportInfo.ReportingReportQuery), stringLocalizer["ReportQuery"]);

            PageConfiguration.TableActions
                .AddDeleteAction(nameof(Delete));

            PageConfiguration.QueryModifiers
                .AddModifier((query, _) =>
                {
                    return query.Where(new WhereCondition().WhereEquals(nameof(ReportingReportInfo.ReportingReportChannelSettingsID), ReportingReportId));
                });

            return base.ConfigurePage();
        }

    }

}
