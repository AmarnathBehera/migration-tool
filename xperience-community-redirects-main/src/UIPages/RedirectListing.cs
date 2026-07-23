using BDO.GWT.Xperience.Admin.Pages.ProjectSettings.WebChannelSettings;
using CMS.Base;
using CMS.ContentEngine;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Membership;
using CMS.Websites;
using CMS.Websites.Internal;
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

    private readonly IContentQueryExecutor _executor;
    private readonly IWebPageUrlRetriever _webPageUrlRetriever;
    private readonly IChannelAccessService _channelAccessService;
    private readonly IInfoProvider<RedirectInfo> _redirectInfoProvider;
    public RedirectListing(
        IContentQueryExecutor executor,
        IWebPageUrlRetriever webPageUrlRetriever,
        IChannelAccessService channelAccessService,
         IInfoProvider<RedirectInfo> redirectInfoProvider)
    {
        _executor = executor;
        _webPageUrlRetriever = webPageUrlRetriever;
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
            .AddColumn(nameof(RedirectInfo.RedirectTargetWebPageItemGUID), "Target web page", formatter: GetWebPageUrl)
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
        Guid webPageItemGuid = ValidationHelper.GetGuid(objectValue, Guid.Empty);

        if (webPageItemGuid != Guid.Empty)
        {
            var pageQuery = new ContentItemQueryBuilder()
                .ForContentTypes(parameters =>
                    parameters.ForWebsite([webPageItemGuid], includeUrlPath: false)
                )
                .Parameters(parameters =>
                    parameters
                        .Columns(nameof(WebPageItemInfo.WebPageItemID))
                        .TopN(1)
                );

            var result = _executor.GetMappedResult<IWebPageFieldsSource>(pageQuery).Result.FirstOrDefault();

            if (result != null)
            {
                string pageUrl = _webPageUrlRetriever.Retrieve(result.SystemFields.WebPageItemID, GetDefaultLanguageName()).Result.RelativePath;

                if (pageUrl != null)
                {
                    return pageUrl.Replace("~", "");
                }
            }
        }

        return "Page deleted";
    }

    private string GetDefaultLanguageName()
    {
        return ContentLanguageInfo.Provider.Get()
            .TopN(1)
            .Column(nameof(ContentLanguageInfo.ContentLanguageName))
            .WhereTrue(nameof(ContentLanguageInfo.ContentLanguageIsDefault))
            .GetScalarResult<string>();
    }
}

