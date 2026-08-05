using BDO.GWT.Xperience.Admin.Pages.ProjectSettings.WebChannelSettings;
using CMS.ContentEngine;
using CMS.DataEngine;
using CMS.Websites;

using Kentico.Xperience.Admin.Base.FormAnnotations;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class ChannelDropdownProvider : IDropDownOptionsProvider
{
    private readonly IInfoProvider<ChannelInfo> channelInfoProvider;
    private readonly IChannelAccessService channelAccessService;

    public ChannelDropdownProvider(
        IInfoProvider<ChannelInfo> channelInfoProvider,
        IChannelAccessService channelAccessService)
    {
        this.channelInfoProvider = channelInfoProvider;
        this.channelAccessService = channelAccessService;
    }

    public async Task<IEnumerable<DropDownOptionItem>> GetOptionItems()
    {
       
        var accessibleChannelIds = await channelAccessService.GetAccessibleWebChannelSettingsIdsAsync();

        var query = channelInfoProvider.Get()
            .WhereEquals(nameof(ChannelInfo.ChannelType), "Website");

        if (accessibleChannelIds != null)
        {
            query = accessibleChannelIds.Count == 0
                ? query.WhereEquals(nameof(ChannelInfo.ChannelID), -1)
                : query.WhereIn(nameof(ChannelInfo.ChannelID), accessibleChannelIds.ToList());
        }

        return (await query.GetEnumerableTypedResultAsync())
            .Select(channel => new DropDownOptionItem
            {
                Value = channel.ChannelID.ToString(),
                Text = channel.ChannelDisplayName
            });
    }
}