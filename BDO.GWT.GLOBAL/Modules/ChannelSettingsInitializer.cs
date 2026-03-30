using BDO;
using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.Websites;
using CMS.Websites.Routing;
using System.Linq;

[assembly: RegisterModule(typeof(ChannelSettingsInitializer))]

public class ChannelSettingsInitializer : Module
{
    private IWebsiteChannelContext _websiteChannelContext;
    private IInfoProvider<WebChannelSettingsInfo> _channelProvider;
    private IInfoProvider<WebChannelSnippetInfo> _snippetProvider;
    private IInfoProvider<GlobalsettingskeyInfo> _globalProvider;

    public ChannelSettingsInitializer() : base("ChannelSettingsInitializer")
    {
       
    }

    protected override void OnInit()
    {
        base.OnInit();

        // ✅ Resolve services AFTER IoC is initialized
        _websiteChannelContext = Service.Resolve<IWebsiteChannelContext>();
        _channelProvider = Service.Resolve<IInfoProvider<WebChannelSettingsInfo>>();
        _snippetProvider = Service.Resolve<IInfoProvider<WebChannelSnippetInfo>>();
        _globalProvider = Service.Resolve<IInfoProvider<GlobalsettingskeyInfo>>();

        // ✅ Event when new channel is created
        WebChannelSettingsInfo.TYPEINFO.Events.Insert.After += OnChannelCreated;

        // ✅ Ensure snippets for current channel on app start
        EnsureSnippetsForCurrentChannel();
    }

    /// <summary>
    /// Triggered when a new channel is created
    /// </summary>
    private void OnChannelCreated(object sender, ObjectEventArgs e)
    {
        var channel = e.Object as WebChannelSettingsInfo;

        if (channel == null)
        {
            return;
        }

        CreateDefaultSnippets(channel);
    }

    /// <summary>
    /// Ensure snippets exist for current channel
    /// </summary>
    private void EnsureSnippetsForCurrentChannel()
    {
        int currentWebsiteID = _websiteChannelContext.WebsiteChannelID;

        if (currentWebsiteID <= 0)
        {
            return;
        }

        var currentChannel = _channelProvider
            .Get()
            .WhereEquals(nameof(WebChannelSettingsInfo.WebChannelSettingsChannelID), currentWebsiteID)
            .TopN(1)
            .FirstOrDefault();

        if (currentChannel == null)
        {
            return;
        }

        CreateDefaultSnippets(currentChannel);
    }

    /// <summary>
    /// Create missing snippets for a channel
    /// </summary>
    private void CreateDefaultSnippets(WebChannelSettingsInfo channel)
    {
        var globalSettings = _globalProvider.Get().GetEnumerableTypedResult();

        // ✅ Safe & clean channel name
        string safeChannelName = (channel.WebChannelSettingsChannelDisplayName ?? "site")
                                    .ToLower()
                                    .Replace(" ", "_");

        foreach (var global in globalSettings)
        {
            // ✅ Unique codename per channel
            string combinedCodeName = $"{safeChannelName}_{global.GlobalSettingsKeyName}".ToLower();

            bool exists = _snippetProvider.Get()
                .WhereEquals(nameof(WebChannelSnippetInfo.WebChannelSnippetCodeName), combinedCodeName)
                .WhereEquals(nameof(WebChannelSnippetInfo.WebChannelSnippetWebChannelSettingsID), channel.WebChannelSettingsID)
                .Any();

            if (exists)
            {
                continue;
            }

            var snippet = new WebChannelSnippetInfo
            {
                WebChannelSnippetDisplayName = global.GlobalSettingsKeyDisplayName,
                WebchannelCodeName= combinedCodeName,
                WebChannelSnippetCodeName = global.GlobalSettingsKeyName,
                WebChannelSnippetCode = global.GlobalSettingsKeyValue,
                WebChannelSnippetWebChannelSettingsID = channel.WebChannelSettingsID,
                WebChannelSnippetType = "Javascript"
            };

            _snippetProvider.Set(snippet);
        }
    }
}