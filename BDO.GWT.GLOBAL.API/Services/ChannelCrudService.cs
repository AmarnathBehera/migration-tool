using BDO.GWT.Common.Core.Exceptions;
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
    Task<ChannelOutput> CreateEntityAsync(ChannelInput channelInput, CancellationToken cancellationToken);
    Task<ChannelOutput> UpdateEntityAsync(Guid entityGUID, ChannelInput channelInput, CancellationToken cancellationToken);
    Task DeleteEntityAsync(Guid entityGUID, CancellationToken cancellationToken);
}

public class ChannelCrudService : IChannelCrudService
{
    private readonly IInfoProvider<ChannelInfo> _channelInfoProvider;
    private readonly IInfoProvider<WebsiteChannelInfo> _websiteChannelInfoProvider;
    private readonly IInfoProvider<ContentLanguageInfo> _contentLanguageInfoProvider;

    public ChannelCrudService(
        IInfoProvider<ChannelInfo> channelInfoProvider,
        IInfoProvider<WebsiteChannelInfo> websiteChannelInfoProvider,
        IInfoProvider<ContentLanguageInfo> contentLanguageInfoProvider)
    {
        _channelInfoProvider = channelInfoProvider;
        _websiteChannelInfoProvider = websiteChannelInfoProvider;
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

    public async Task<ChannelOutput> CreateEntityAsync(ChannelInput channelInput, CancellationToken cancellationToken)
    {
        int primaryLanguageId = await ResolvePrimaryLanguageIdAsync(channelInput, cancellationToken);
        var domain = ResolvePresentationUrl(channelInput);

        await ValidateDomainAvailabilityAsync(domain, null, cancellationToken);

        var channelInfo = new ChannelInfo
        {
            ChannelDisplayName = channelInput.General.DisplayName,
            ChannelName = channelInput.General.CodeName
        };

        _channelInfoProvider.Set(channelInfo);

        var websiteChannelInfo = new WebsiteChannelInfo
        {
            WebsiteChannelChannelID = channelInfo.ChannelID,
            WebsiteChannelDomain = domain,
            WebsiteChannelPrimaryContentLanguageID = primaryLanguageId
        };

        await _websiteChannelInfoProvider.SetAsync(websiteChannelInfo, cancellationToken);

        var output = await GetEntityAsync(channelInfo.ChannelGUID, cancellationToken);

        return output ?? throw new UnprocessableEntityException("Failed to create channel.");
    }

    public async Task<ChannelOutput> UpdateEntityAsync(Guid entityGUID, ChannelInput channelInput, CancellationToken cancellationToken)
    {
        var channelInfo = await GetChannelInfoByGuidAsync(entityGUID, cancellationToken);
        int primaryLanguageId = await ResolvePrimaryLanguageIdAsync(channelInput, cancellationToken);
        var domain = ResolvePresentationUrl(channelInput);

        await ValidateDomainAvailabilityAsync(domain, channelInfo.ChannelID, cancellationToken);

        channelInfo.ChannelDisplayName = channelInput.General.DisplayName;
        channelInfo.ChannelName = channelInput.General.CodeName;

        try
        {
            await _channelInfoProvider.SetAsync(channelInfo);
        }
        catch (DataClassNotFoundException ex)
            when (string.Equals(ex.Message, "Class with name 'BDO.GDPRSettings' doesn't exist.", StringComparison.Ordinal))
        {
            throw new ValidationException(
                "Channel update failed because CMS class 'BDO.GDPRSettings' is not available in this environment.");
        }

        var websiteChannelInfo = (await _websiteChannelInfoProvider.Get()
            .WhereEquals(nameof(WebsiteChannelInfo.WebsiteChannelChannelID), channelInfo.ChannelID)
            .TopN(1)
            .GetEnumerableTypedResultAsync(cancellationToken: cancellationToken))
            .FirstOrDefault();

        if (websiteChannelInfo is null)
        {
            websiteChannelInfo = new WebsiteChannelInfo
            {
                WebsiteChannelChannelID = channelInfo.ChannelID
            };
        }

        websiteChannelInfo.WebsiteChannelDomain = domain;
        websiteChannelInfo.WebsiteChannelPrimaryContentLanguageID = primaryLanguageId;

        await _websiteChannelInfoProvider.SetAsync(websiteChannelInfo, cancellationToken);

        var output = await GetEntityAsync(channelInfo.ChannelGUID, cancellationToken);

        return output ?? throw new NotFoundException("Channel not found.");
    }

    public async Task DeleteEntityAsync(Guid entityGUID, CancellationToken cancellationToken)
    {
        var channelInfo = await GetChannelInfoByGuidAsync(entityGUID, cancellationToken);

        var websiteChannels = await _websiteChannelInfoProvider.Get()
            .WhereEquals(nameof(WebsiteChannelInfo.WebsiteChannelChannelID), channelInfo.ChannelID)
            .GetEnumerableTypedResultAsync(cancellationToken: cancellationToken);

        foreach (var websiteChannel in websiteChannels)
        {
            await _websiteChannelInfoProvider.DeleteAsync(websiteChannel, cancellationToken);
        }

        try
        {
            await _channelInfoProvider.DeleteAsync(channelInfo, cancellationToken);
        }
        catch (DataClassNotFoundException ex)
            when (string.Equals(ex.Message, "Class with name 'BDO.GDPRSettings' doesn't exist.", StringComparison.Ordinal))
        {
            throw new ValidationException(
                "Channel delete failed because CMS class 'BDO.GDPRSettings' is not available in this environment.");
        }
    }

    private async Task<ChannelInfo> GetChannelInfoByGuidAsync(Guid entityGUID, CancellationToken cancellationToken)
    {
        var channelInfo = (await _channelInfoProvider.Get()
            .WhereEquals(nameof(ChannelInfo.ChannelGUID), entityGUID)
            .TopN(1)
            .GetEnumerableTypedResultAsync(cancellationToken: cancellationToken))
            .FirstOrDefault();

        return channelInfo ?? throw new NotFoundException("Channel not found.");
    }

    private async Task<int> ResolvePrimaryLanguageIdAsync(ChannelInput channelInput, CancellationToken cancellationToken)
    {
        var defaultCulture = channelInput.General.DefaultContentCulture;

        if (string.IsNullOrWhiteSpace(defaultCulture))
        {
            defaultCulture = channelInput.Cultures.FirstOrDefault()?.Code;
        }

        if (string.IsNullOrWhiteSpace(defaultCulture))
        {
            throw new UnprocessableEntityException("Default content culture is required.");
        }

        var language = (await _contentLanguageInfoProvider.Get()
            .WhereEquals(nameof(ContentLanguageInfo.ContentLanguageName), defaultCulture)
            .TopN(1)
            .GetEnumerableTypedResultAsync(cancellationToken: cancellationToken))
            .FirstOrDefault();

        return language?.ContentLanguageID ?? throw new UnprocessableEntityException($"Culture '{defaultCulture}' was not found.");
    }

    private static string ResolvePresentationUrl(ChannelInput channelInput)
    {
        var rawUrl = channelInput.DomainAliases.LiveSite.FirstOrDefault()?.PresentationURL
            ?? channelInput.General.PresentationURL;

        return NormalizeWebsiteChannelDomain(rawUrl);
    }

    private async Task ValidateDomainAvailabilityAsync(string domain, int? currentChannelId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(domain))
        {
            throw new UnprocessableEntityException("Presentation URL is required.");
        }

        var conflictingChannels = await _websiteChannelInfoProvider.Get()
            .WhereEquals(nameof(WebsiteChannelInfo.WebsiteChannelDomain), domain)
            .GetEnumerableTypedResultAsync(cancellationToken: cancellationToken);

        var hasConflict = conflictingChannels.Any(channel => !currentChannelId.HasValue || channel.WebsiteChannelChannelID != currentChannelId.Value);

        if (hasConflict)
        {
            throw new UnprocessableEntityException($"Website channel domain '{domain}' is already used by another channel.");
        }
    }

    private static string NormalizeWebsiteChannelDomain(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var trimmed = value.Trim();

        if (Uri.TryCreate(trimmed, UriKind.Absolute, out var absoluteUri))
        {
            return absoluteUri.IsDefaultPort
                ? absoluteUri.Host
                : $"{absoluteUri.Host}:{absoluteUri.Port}";
        }

        var withoutPath = trimmed.Split('/', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? trimmed;

        return withoutPath;
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
