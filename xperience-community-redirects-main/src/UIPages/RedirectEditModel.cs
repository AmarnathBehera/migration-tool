using BDO;
using CMS.ContentEngine;
using CMS.DataEngine;
using CMS.Websites;
using CMS.Websites.Internal;
using CMS.Websites.Routing;
using Kentico.Xperience.Admin.Base.FormAnnotations;
using Kentico.Xperience.Admin.Websites;
using Kentico.Xperience.Admin.Websites.FormAnnotations;
using XperienceCommunity.Redirects;

namespace XperienceCommunity.Redirects.UIPages;

internal class RedirectEditModel
{
    [RequiredValidationRule]
    [TextInputComponent(Label = "Source URL", Order = 2)]
    public string? SourceUrl { get; set; }

    [RequiredValidationRule]
    [WebPageSelectorComponent(
        Label = "Target web page",
        ItemModifierType = typeof(WebPagesWithUrlWebPagePanelItemModifier),
        ExplanationTextAsHtml = true,
        ExplanationText = "Select the target web page to which requests for the source URL will be redirected to.",
        Order = 3,
        MaximumPages = 1)]
    public IEnumerable<WebPageRelatedItem> TargetWebPageItem { get; set; } = [];

    /// <summary>
    /// Maps the form model to the <see cref="RedirectInfo"/> object, including resolving
    /// and storing the target page's current public URL (RedirectTargetPageURL). This field
    /// is stored for reference/redirect-resolution purposes only and is not shown in the UI.
    /// </summary>
    public async Task MapToRedirectInfo(RedirectInfo info, IWebPageUrlRetriever webPageUrlRetriever)
    {
        info.RedirectSourceUrl = SourceUrl?.ToLower();
        info.RedirectTargetPageURL = string.Empty;

        var targetPage = TargetWebPageItem.FirstOrDefault();
        if (targetPage is null)
        {
            return;
        }

        info.RedirectTargetWebPageItemGUID = targetPage.WebPageGuid;

        // Look up the selected page to find which website channel it belongs to
        // and its numeric ID (needed for URL resolution).
        var webPageItem = WebPageItemInfo.Provider.Get()
            .WhereEquals(nameof(WebPageItemInfo.WebPageItemGUID), targetPage.WebPageGuid)
            .TopN(1)
            .FirstOrDefault();

        if (webPageItem is null)
        {
            return;
        }

        var websiteChannel = WebsiteChannelInfo.Provider.Get(webPageItem.WebPageItemWebsiteChannelID);
        if (websiteChannel is null)
        {
            return;
        }

        var channel = ChannelInfo.Provider.Get(websiteChannel.WebsiteChannelChannelID);
        info.RedirectChannelID = channel?.ChannelID ?? 0;
        info.RedirectChannelName = channel?.ChannelDisplayName;

        var defaultLanguageName = ContentLanguageInfo.Provider.Get()
            .TopN(1)
            .Column(nameof(ContentLanguageInfo.ContentLanguageName))
            .WhereTrue(nameof(ContentLanguageInfo.ContentLanguageIsDefault))
            .GetScalarResult<string>();

        if (string.IsNullOrWhiteSpace(defaultLanguageName))
        {
            return;
        }

        try
        {
            var urlResult = await webPageUrlRetriever.Retrieve(webPageItem.WebPageItemID, defaultLanguageName);
            info.RedirectTargetPageURL = urlResult.RelativePath?.Replace("~", "") ?? string.Empty;
        }
        catch (Exception)
        {
            // Target page has no resolvable URL in the default language/channel context
            // (e.g. unpublished, folder-only, or reordered content). Leave the field empty
            // rather than failing the save.
            info.RedirectTargetPageURL = string.Empty;
        }
    }
}