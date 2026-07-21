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

    public ChannelDropdownProvider(IInfoProvider<ChannelInfo> channelInfoProvider)
    {
        this.channelInfoProvider = channelInfoProvider;
    }

    public async Task<IEnumerable<DropDownOptionItem>> GetOptionItems()
    {
        return (await channelInfoProvider.Get().GetEnumerableTypedResultAsync())
            .Select(channel => new DropDownOptionItem
            {
                Value = channel.ChannelID.ToString(),
                Text = channel.ChannelDisplayName
            });
    }
}