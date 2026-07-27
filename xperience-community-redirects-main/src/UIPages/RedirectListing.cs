using BDO.GWT.Xperience.Admin.Pages.ProjectSettings.WebChannelSettings;
using CMS.Base;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Membership;
using Kentico.Xperience.Admin.Base;
using XperienceCommunity.Redirects.UIPages;

[assembly: UIPage(
    parentType: typeof(RedirectApplicationPage),
    slug: "list",
    uiPageType: typeof(RedirectListing),
    name: "List",
    templateName: TemplateNames.LISTING,
    order: UIPageOrder.First)]

namespace XperienceCommunity.Redirects.UIPages;

public class RedirectListing : ListingPage
{
    protected override string ObjectType => RedirectInfo.OBJECT_TYPE;

    private readonly IChannelAccessService _channelAccessService;
    private readonly IInfoProvider<RedirectInfo> _redirectInfoProvider;
    public RedirectListing(
        IChannelAccessService channelAccessService,
        IInfoProvider<RedirectInfo> redirectInfoProvider)
    {
        _channelAccessService = channelAccessService;
        _redirectInfoProvider = redirectInfoProvider;
    }

    public override async Task ConfigurePage()
    {
        PageConfiguration.HeaderActions.AddLink<RedirectCreate>("New redirect");
        PageConfiguration.TableActions.AddDeleteAction(nameof(Delete));
        PageConfiguration.AddEditRowAction<RedirectEdit>();
        PageConfiguration.ColumnConfigurations
            .AddColumn(nameof(RedirectInfo.RedirectSourceUrl), "Source URL", searchable: true)
            .AddColumn(nameof(RedirectInfo.RedirectTargetPageURL), "Target web page", formatter: GetWebPageUrl)
            .AddColumn(nameof(RedirectInfo.RedirectChannelName), "Channel", searchable: true);
        PageConfiguration.AddEditRowAction<RedirectEditSection>();
        PageConfiguration.MassActions.AddCommandWithConfirmation(
           label: "Delete",
           command: nameof(MassDelete),
           icon: Icons.Bin,
           confirmation: "Are you sure you want to delete the selected redirect(s)? This cannot be undone.",
           confirmationButton: "Delete"
       );
        var accessibleChannelIds = await _channelAccessService.GetAccessibleWebChannelSettingsIdsAsync();

        if (accessibleChannelIds != null)
        {
            PageConfiguration.QueryModifiers.AddModifier((query, _) =>
            {
                if (accessibleChannelIds.Count == 0)
                {
                    return query.WhereEquals(nameof(RedirectInfo.RedirectChannelID), -1);
                }

                return query.WhereIn(nameof(RedirectInfo.RedirectChannelID), accessibleChannelIds.ToList());
            });
        }

        await base.ConfigurePage();
    }

    [PageCommand(Permission = SystemPermissions.DELETE)]
    public override Task<ICommandResponse<RowActionResult>> Delete(int id) => base.Delete(id);

    [PageCommand(Permission = SystemPermissions.DELETE)]
    public async Task<ICommandResponse<MassActionResult>> MassDelete(IEnumerable<int> identifiers, CancellationToken cancellationToken)
    {
        // Re-check channel access server-side for every row - don't rely solely
        // on the client only having been shown authorized rows
        var accessibleChannelIds = await _channelAccessService.GetAccessibleWebChannelSettingsIdsAsync();

        int deletedCount = 0;
        int skippedCount = 0;

        foreach (var id in identifiers)
        {
            var redirect = _redirectInfoProvider.Get(id);
            if (redirect == null)
            {
                continue;
            }

            if (accessibleChannelIds != null && !accessibleChannelIds.Contains(redirect.RedirectChannelID))
            {
                skippedCount++;
                continue;
            }

            redirect.Delete();
            deletedCount++;
        }

        var response = ResponseFrom(new MassActionResult(reload: true));

        return skippedCount > 0
            ? response.AddWarningMessage($"Deleted {deletedCount} redirect(s). Skipped {skippedCount} you don't have access to.")
            : response.AddSuccessMessage($"Deleted {deletedCount} redirect(s).");
    }

    
    private string GetWebPageUrl(object objectValue, IDataContainer dataContainer)
    {
        string pageUrl = ValidationHelper.GetString(objectValue, string.Empty);

        return string.IsNullOrEmpty(pageUrl) ? "Page deleted" : pageUrl;
    }
}