using BDO.GWT.GLOBAL.API.Data;
using CMS.Base;
using CMS.ContentEngine;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Websites;

namespace BDO.GWT.GLOBAL.API.Services;

public class ChannelsQuery
{
    public string? AdministrationDomain { get; set; }
}

public interface IChannelCrudService
{
    Task<IEnumerable<ChannelOutput>> GetEntitiesAsync(ChannelsQuery entityQuery, CancellationToken cancellationToken);
    Task<ChannelOutput?> GetEntityAsync(Guid entityGUID, CancellationToken cancellationToken);
}

public class ChannelCrudService : IChannelCrudService
{
    private readonly IInfoProvider<ChannelInfo> _channelInfoProvider;
    private readonly IInfoProvider<ContentLanguageInfo> _contentLanguageInfoProvider;

    public ChannelCrudService(
        IInfoProvider<ChannelInfo> channelInfoProvider,
        IInfoProvider<ContentLanguageInfo> contentLanguageInfoProvider)
    {
        _channelInfoProvider = channelInfoProvider;
        _contentLanguageInfoProvider = contentLanguageInfoProvider;
    }

    public async Task<IEnumerable<ChannelOutput>> GetEntitiesAsync(ChannelsQuery entityQuery, CancellationToken cancellationToken)
    {
        var query = _channelInfoProvider.Get()
            .Source(s => s.Join<WebsiteChannelInfo>(
                nameof(ChannelInfo.ChannelID),
                nameof(WebsiteChannelInfo.WebsiteChannelChannelID)))
            .Columns(
                nameof(ChannelInfo.ChannelID),
                nameof(ChannelInfo.ChannelGUID),
                nameof(ChannelInfo.ChannelDisplayName),
                nameof(ChannelInfo.ChannelName),
                nameof(WebsiteChannelInfo.WebsiteChannelDomain),
                nameof(WebsiteChannelInfo.WebsiteChannelPrimaryContentLanguageID));

        if (entityQuery.AdministrationDomain != null)
        {
            query = query.WhereEquals(nameof(WebsiteChannelInfo.WebsiteChannelDomain), entityQuery.AdministrationDomain);
        }

        var channels = await query.GetDataContainerResultAsync(cancellationToken: cancellationToken);

        var allCultures = await GetAllCulturesAsync(cancellationToken);
        var languagesById = allCultures.ToDictionary(c => c.ContentLanguageID, c => c);

        return channels.Select(channel => ToChannelOutput(channel, languagesById, allCultures));
    }

    public async Task<ChannelOutput?> GetEntityAsync(Guid entityGUID, CancellationToken cancellationToken)
    {
        var channels = await _channelInfoProvider.Get()
            .Source(s => s.Join<WebsiteChannelInfo>(
                nameof(ChannelInfo.ChannelID),
                nameof(WebsiteChannelInfo.WebsiteChannelChannelID)))
            .Columns(
                nameof(ChannelInfo.ChannelID),
                nameof(ChannelInfo.ChannelGUID),
                nameof(ChannelInfo.ChannelDisplayName),
                nameof(ChannelInfo.ChannelName),
                nameof(WebsiteChannelInfo.WebsiteChannelDomain),
                nameof(WebsiteChannelInfo.WebsiteChannelPrimaryContentLanguageID))
            .WhereEquals(nameof(ChannelInfo.ChannelGUID), entityGUID)
            .GetDataContainerResultAsync(cancellationToken: cancellationToken);

        var result = channels.FirstOrDefault();
        if (result is null)
        {
            return null;
        }

        var allCultures = await GetAllCulturesAsync(cancellationToken);
        var languagesById = allCultures.ToDictionary(c => c.ContentLanguageID, c => c);

        return ToChannelOutput(result, languagesById, allCultures);
    }

    private async Task<List<ContentLanguageInfo>> GetAllCulturesAsync(CancellationToken cancellationToken)
    {
        var languages = await _contentLanguageInfoProvider.Get()
            .Columns(
                nameof(ContentLanguageInfo.ContentLanguageID),
                nameof(ContentLanguageInfo.ContentLanguageGUID),
                nameof(ContentLanguageInfo.ContentLanguageDisplayName),
                nameof(ContentLanguageInfo.ContentLanguageName))
            .GetEnumerableTypedResultAsync(cancellationToken: cancellationToken);

        return languages.ToList();
    }

    private static ChannelOutput ToChannelOutput(
        IDataContainer channel,
        Dictionary<int, ContentLanguageInfo> languagesById,
        List<ContentLanguageInfo> allCultures)
    {
        string domain = channel.GetValue(nameof(WebsiteChannelInfo.WebsiteChannelDomain))?.ToString() ?? string.Empty;
        int primaryLanguageId = ValidationHelper.GetInteger(
            channel.GetValue(nameof(WebsiteChannelInfo.WebsiteChannelPrimaryContentLanguageID)), 0);

        string? primaryCulture = primaryLanguageId != 0 && languagesById.TryGetValue(primaryLanguageId, out var lang)
            ? lang.ContentLanguageName
            : null;

        var general = new SiteGeneralOutput
        {
            ID = ValidationHelper.GetInteger(channel.GetValue(nameof(ChannelInfo.ChannelID)), 0),
            GUID = ValidationHelper.GetGuid(channel.GetValue(nameof(ChannelInfo.ChannelGUID)), Guid.Empty),
            DisplayName = channel.GetValue(nameof(ChannelInfo.ChannelDisplayName))?.ToString() ?? string.Empty,
            CodeName = channel.GetValue(nameof(ChannelInfo.ChannelName))?.ToString() ?? string.Empty,
            WebsitedomainresentationURL = domain,
            DefaultContentCulture = primaryCulture
        };

        return new ChannelOutput
        {
            General = general,
            Cultures = allCultures.Select(c => new CultureSummaryOutput
            {
                GUID = c.ContentLanguageGUID,
                Name = c.ContentLanguageDisplayName,
                Code = c.ContentLanguageName
            }).ToArray()
        };
    }
}