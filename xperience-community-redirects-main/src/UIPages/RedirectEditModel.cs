using CMS.ContentEngine;
using CMS.DataEngine;
using CMS.Websites;
using CMS.Websites.Internal;
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

    public void MapToRedirectInfo(RedirectInfo info)
    {
        info.RedirectSourceUrl = SourceUrl?.ToLower();

        var targetPage = TargetWebPageItem.FirstOrDefault();
        if (targetPage is null)
        {
            return;
        }

        info.RedirectTargetWebPageItemGUID = targetPage.WebPageGuid;

        // Look up the selected page to find which website channel it belongs to
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
    }
}