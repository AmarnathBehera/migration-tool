using BDO;
using BDO.GWT.Core;
using BDO.GWT.Core.ValueObjects;
using BDO.GWT.GLOBAL.API.Data;
using BDO.GWT.GLOBAL.API.Helpers;
using CMS.Base;
using CMS.ContentEngine;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Membership;
using CMS.Scheduler;
using CMS.Websites;
using Kentico.Content.Web.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

[assembly: RegisterScheduledTask(identifier: "Custom.CopyDealPageToGlobalSiteTask", typeof(CopyDealPageToGlobalSiteTask))]

public class CopyDealPageToGlobalSiteTask : IScheduledTask
{
    private readonly IInfoProvider<ChannelInfo> _channelProvider;
    private readonly IInfoProvider<ContentLanguageInfo> _languageProvider;
    private readonly IUserInfoProvider _userInfoProvider;
    private readonly IWebPageManagerFactory _webPageManagerFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<CopyDealPageToGlobalSiteTask> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    private const string REMOTE_DEAL_CONTENT_TYPE = RemoteDeal.CONTENT_TYPE_NAME;
    private const string WEB_PAGE_GUID = "WebPageItemGUID";

    public CopyDealPageToGlobalSiteTask(
        IInfoProvider<ChannelInfo> channelProvider,
        IInfoProvider<ContentLanguageInfo> languageProvider,
        IUserInfoProvider userInfoProvider,
        IWebPageManagerFactory webPageManagerFactory,
        IConfiguration configuration,
        ILogger<CopyDealPageToGlobalSiteTask> logger,
        IServiceScopeFactory serviceScopeFactory
        )
    {
        _channelProvider = channelProvider;
        _languageProvider = languageProvider;
        _userInfoProvider = userInfoProvider;
        _webPageManagerFactory = webPageManagerFactory;
        _configuration = configuration;
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task<ScheduledTaskExecutionResult> Execute(ScheduledTaskConfigurationInfo task, CancellationToken cancellationToken)
    {
        var globalGuid = ParseGuidSetting(_configuration["Deals:GlobalSiteGUID"], "19BDEA77-FE92-4A62-A0A6-F50797CB371C");

        var globalSite = await GetEntityAsync(globalGuid, cancellationToken);

        if (globalSite is null)
        {
            _logger.LogError("Could not resolve global site channel for GUID {Guid}", globalGuid);
            return new ScheduledTaskExecutionResult($"Could not resolve global site channel for GUID {globalGuid}");
        }

        // All channels other than the global site are treated as source channels
        // whose deals get synced onto the global site.
        var sourceChannels = await GetAllChannelsExceptAsync(globalGuid, cancellationToken);

        if (sourceChannels.Count == 0)
        {
            _logger.LogInformation("No channels found other than the global site; nothing to sync.");
            return ScheduledTaskExecutionResult.Success;
        }

        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        var contentService = scope.ServiceProvider.GetRequiredService<IContentService>();

        const string globalCulture = "en-gb";

        // NOTE: WebPageManager must be scoped to the *website channel*, not the
        // content-hub ChannelID.
        var adminUser = _userInfoProvider.Get("administrator");
        if (adminUser is null)
        {
            _logger.LogError("Could not resolve the 'administrator' user account");
            return new ScheduledTaskExecutionResult("Could not resolve the 'administrator' user account");
        }
        var webPageManager = _webPageManagerFactory.Create(globalSite.General.ID, adminUser.UserID);

        var retrieveParametersRemoteDeal = new RetrievePagesParameters
        {
            LinkedItemsMaxLevel = 5,
            ChannelName = globalSite.General.CodeName,
            LanguageName = globalCulture,
            IsForPreview = true,
        };

        var sectionDeals = (await contentService.GetPagesAsync<SectionDeals>(
            retrieveParametersRemoteDeal,
            query => { },
            RetrievalCacheSettings.CacheDisabled,
            cancellationToken)).ToList();

        var sectionDeal = sectionDeals.FirstOrDefault();
        if (sectionDeal is null)
        {
            // TODO: confirm parent page ID / placement for a newly created SectionDeals
            // page on the global site. Currently created under the channel root (0).
            sectionDeal = await CreateSectionDealAsync(webPageManager, globalCulture, cancellationToken);
            if (sectionDeal is null)
            {
                _logger.LogError("Could not create a SectionDeals landing page on the global site");
                return new ScheduledTaskExecutionResult("Could not create a SectionDeals landing page on the global site");
            }
        }

        var remoteDeals = (await contentService.GetPagesAsync<RemoteDeal>(
            retrieveParametersRemoteDeal,
            query => { },
            RetrievalCacheSettings.CacheDisabled,
            cancellationToken)).ToList();

        var lastRunTime = task.ScheduledTaskConfigurationLastRunTime != default
            ? task.ScheduledTaskConfigurationLastRunTime
            : DateTime.MinValue;

        // Pull opted-in deals from every non-global channel, keeping track of which
        // channel each deal came from (needed for RemoteDeal.OriginalSiteGUID).
        var dealsWithChannel = new List<(Deal Deal, Guid ChannelGuid)>();

        foreach (var channel in sourceChannels)
        {
            var retrieveParameters = new RetrievePagesParameters
            {
                LinkedItemsMaxLevel = 2,
                ChannelName = channel.General.CodeName,
                LanguageName = globalCulture,
                IsForPreview = true,
            };

            List<Deal> channelDeals;
            try
            {
                channelDeals = (await contentService.GetPagesAsync<Deal>(
                    retrieveParameters,
                    query =>
                    {
                        query.Where(w => w.WhereEquals(nameof(Deal.SharingOptIn), true));
                    },
                    RetrievalCacheSettings.CacheDisabled,
                    cancellationToken)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve deals for channel {ChannelName} ({ChannelGuid})",
                    channel.General.CodeName, channel.General.GUID);
                continue;
            }

            dealsWithChannel.AddRange(channelDeals.Select(d => (Deal: d, ChannelGuid: channel.General.GUID)));
        }

        if (dealsWithChannel.Count == 0)
        {
            return ScheduledTaskExecutionResult.Success;
        }

        // A deal that has no matching RemoteDeal yet needs to be created.
        var createRemoteDeal = dealsWithChannel
            .Where(x => !remoteDeals.Any(rd => rd.OriginalPageGUID == x.Deal.SystemFields.WebPageItemGUID))
            .ToList();

        // A deal that already has a matching RemoteDeal needs to be updated.
        // We need the existing RemoteDeal's own WebPageItemID (not the source
        // deal's) to know which page to update.
        var updateRemoteDeal = dealsWithChannel
            .Select(x => new
            {
                x.Deal,
                x.ChannelGuid,
                Existing = remoteDeals.FirstOrDefault(rd => rd.OriginalPageGUID == x.Deal.SystemFields.WebPageItemGUID)
            })
            .Where(x => x.Existing is not null)
            .ToList();

        int created = 0, updated = 0, failed = 0;

        foreach (var entry in createRemoteDeal)
        {
            var deal = entry.Deal;
            try
            {
                var itemData = await BuildRemoteDealItemData(contentService, deal, entry.ChannelGuid, cancellationToken);

                var contentItemParameters = new ContentItemParameters(REMOTE_DEAL_CONTENT_TYPE, itemData);
                var createPageParameters = new CreateWebPageParameters(
                    deal.MetadataTitle ?? "Remote Deal",
                    globalCulture,
                    contentItemParameters)
                {
                    ParentWebPageItemID = sectionDeal.SystemFields.WebPageItemID
                };

                int newPageId = await webPageManager.Create(createPageParameters, cancellationToken);

                if (!await webPageManager.TryUpdateDraft(newPageId, globalCulture, new UpdateDraftData(itemData), cancellationToken))
                {
                    throw new InvalidOperationException($"Failed to update draft for new remote deal (source {deal.SystemFields.WebPageItemGUID}).");
                }

                if (!await webPageManager.TryPublish(newPageId, globalCulture, cancellationToken))
                {
                    throw new InvalidOperationException($"Failed to publish new remote deal (source {deal.SystemFields.WebPageItemGUID}).");
                }

                created++;
            }
            catch (Exception ex)
            {
                failed++;
                _logger.LogError(ex, "Failed to create remote deal for source page {Guid}", deal.SystemFields?.WebPageItemGUID);
            }
        }

        foreach (var entry in updateRemoteDeal)
        {
            var deal = entry.Deal;
            var existingRemoteDealPageId = entry.Existing!.SystemFields.WebPageItemID;

            try
            {
                var itemData = await BuildRemoteDealItemData(contentService, deal, entry.ChannelGuid, cancellationToken);

                await webPageManager.TryCreateDraft(existingRemoteDealPageId, globalCulture, cancellationToken);

                if (!await webPageManager.TryUpdateDraft(existingRemoteDealPageId, globalCulture, new UpdateDraftData(itemData), cancellationToken))
                {
                    throw new InvalidOperationException($"Failed to update draft for remote deal {deal.SystemFields.WebPageItemGUID}.");
                }

                if (!await webPageManager.TryPublish(existingRemoteDealPageId, globalCulture, cancellationToken))
                {
                    throw new InvalidOperationException($"Failed to publish remote deal {deal.SystemFields.WebPageItemGUID}.");
                }

                updated++;
            }
            catch (Exception ex)
            {
                failed++;
                _logger.LogError(ex, "Failed to update remote deal for source page {Guid}", deal.SystemFields?.WebPageItemGUID);
            }
        }

        _logger.LogInformation("Deal sync complete: {Created} created, {Updated} updated, {Failed} failed", created, updated, failed);

        return ScheduledTaskExecutionResult.Success;
    }

    /// <summary>
    /// Creates a SectionDeals landing page on the global site to act as the parent
    /// for RemoteDeal pages, used the first time the task runs against a fresh site.
    /// TODO: confirm the correct parent page under which this should be created -
    /// currently created at the channel root.
    /// </summary>
    private static async Task<SectionDeals?> CreateSectionDealAsync(
        IWebPageManager webPageManager, string culture, CancellationToken cancellationToken)
    {
        var itemData = new ContentItemData(new Dictionary<string, object>
        {
            { nameof(SectionDeals.MetadataTitle), "DealCards" },
            // { nameof(SectionDeals.SectionDealsFilteringEnabled), deal.MetadataDescription ?? string.Empty },
        });

        var contentItemParameters = new ContentItemParameters(SectionDeals.CONTENT_TYPE_NAME, itemData);

        var createPageParameters = new CreateWebPageParameters(
            "DealCards",
            culture,
            contentItemParameters)
        {
            ParentWebPageItemID = 0
        };

        int newPageId = await webPageManager.Create(createPageParameters, cancellationToken);

        if (!await webPageManager.TryPublish(newPageId, culture, cancellationToken))
        {
            return null;
        }

        // Re-fetch isn't wired up here since IContentService isn't passed in;
        // callers that need the persisted entity should re-query after this call.
        // For our purposes we only need the new WebPageItemID, so build a
        // lightweight stand-in.
        return new SectionDeals
        {
            SystemFields = new WebPageFields
            {
                WebPageItemID = newPageId
            }
        };
    }

    private async Task<ContentItemData> BuildRemoteDealItemData(
        IContentService contentService, Deal deal, Guid channelGUID, CancellationToken cancellationToken)
    {
        string teaserImageUrl = deal.MetadataTeaserImage?.FirstOrDefault()?.ContentAttachmentAsset.Url ?? string.Empty;
        var teaserImageReferences = await BuildContentAttachmentImageReferences(contentService, teaserImageUrl, cancellationToken);

        var industryReferences = await BuildReferencesAsync<DealCardIndustry>(
            contentService, deal.Industry.Select(i => i.Name), i => i.Name, cancellationToken);
        var subIndustryReferences = await BuildReferencesAsync<DealCardSubIndustry>(
            contentService, deal.SubIndustry.Select(s => s.Name), s => s.Name, cancellationToken);
        var dealTypeReferences = await BuildReferencesAsync<DealCardDealType>(
            contentService, deal.DealType.Select(d => d.Name), d => d.Name, cancellationToken);

        var URL = deal.GetUrl().AbsoluteUrl;

        return new ContentItemData(new Dictionary<string, object>
        {
            { nameof(RemoteDeal.Industry), industryReferences },
            { nameof(RemoteDeal.SubIndustry), subIndustryReferences },
            { nameof(RemoteDeal.DealType), dealTypeReferences },
            { nameof(RemoteDeal.DateField), deal.DateField },
            { nameof(RemoteDeal.DealCountry), deal.DealCountry ?? string.Empty },

            { nameof(RemoteDeal.OriginalSiteGUID), channelGUID },
            { nameof(RemoteDeal.OriginalPageGUID), deal.SystemFields.WebPageItemGUID },
            { nameof(RemoteDeal.OriginalPageURL),URL ?? string.Empty },
            { nameof(RemoteDeal.OriginalModifiedWhen), DateTime.Now },
            { nameof(RemoteDeal.ForceUpdateOnNextSync), false },

            { nameof(RemoteDeal.MetadataNofollow), deal.MetadataNofollow },
            { nameof(RemoteDeal.MetadataNoindex), deal.MetadataNoindex },
            { nameof(RemoteDeal.IncludeInSitemap), deal.IncludeInSitemap },
            { nameof(RemoteDeal.IsSearchExcluded), deal.IsSearchExcluded },
            { nameof(RemoteDeal.MetadataTitle), deal.MetadataTitle ?? string.Empty },
            { nameof(RemoteDeal.MetadataDescription), deal.MetadataDescription ?? string.Empty },
            { nameof(RemoteDeal.MetadataTeaserImage), teaserImageReferences },
            { nameof(RemoteDeal.MetadataTeaserImageAltText), deal.MetadataTeaserImageAltText ?? string.Empty },
            { nameof(RemoteDeal.MetadataCanonical), deal.MetadataCanonical ?? string.Empty },
        });
    }

    private static Guid ParseGuidSetting(string? configuredValue, string fallback) =>
        Guid.TryParse(configuredValue, out var parsed) ? parsed : Guid.Parse(fallback);

    public async Task<ChannelOutput?> GetEntityAsync(Guid entityGUID, CancellationToken cancellationToken)
    {
        var channels = await _channelProvider.Get()
            .Source(s => s.Join<WebsiteChannelInfo>(
                nameof(ChannelInfo.ChannelID),
                nameof(WebsiteChannelInfo.WebsiteChannelChannelID)))
            .Columns(
                nameof(ChannelInfo.ChannelID),
                nameof(ChannelInfo.ChannelGUID),
                nameof(ChannelInfo.ChannelDisplayName),
                nameof(ChannelInfo.ChannelName),
                nameof(WebsiteChannelInfo.WebsiteChannelID),
                nameof(WebsiteChannelInfo.WebsiteChannelDomain),
                nameof(WebsiteChannelInfo.WebsiteChannelPrimaryContentLanguageID))
            .WhereEquals(nameof(ChannelInfo.ChannelGUID), entityGUID)
            .GetDataContainerResultAsync(cancellationToken: cancellationToken);

        var result = channels.FirstOrDefault();
        if (result is null)
        {
            return null;
        }
        return await ToChannelOutputAsync(result, _languageProvider, cancellationToken);
    }

    /// <summary>
    /// Returns every website channel except the one matching <paramref name="excludeGUID"/>
    /// (typically the global site). Used to discover all "region"-style channels whose
    /// deal pages should be synced onto the global site.
    /// </summary>
    public async Task<List<ChannelOutput>> GetAllChannelsExceptAsync(Guid excludeGUID, CancellationToken cancellationToken)
    {
        var channels = await _channelProvider.Get()
            .Source(s => s.Join<WebsiteChannelInfo>(
                nameof(ChannelInfo.ChannelID),
                nameof(WebsiteChannelInfo.WebsiteChannelChannelID)))
            .Columns(
                nameof(ChannelInfo.ChannelID),
                nameof(ChannelInfo.ChannelGUID),
                nameof(ChannelInfo.ChannelDisplayName),
                nameof(ChannelInfo.ChannelName),
                nameof(WebsiteChannelInfo.WebsiteChannelID),
                nameof(WebsiteChannelInfo.WebsiteChannelDomain),
                nameof(WebsiteChannelInfo.WebsiteChannelPrimaryContentLanguageID))
            .WhereNotEquals(nameof(ChannelInfo.ChannelGUID), excludeGUID)
            .GetDataContainerResultAsync(cancellationToken: cancellationToken);

        var results = new List<ChannelOutput>();
        foreach (var channel in channels)
        {
            results.Add(await ToChannelOutputAsync(channel, _languageProvider, cancellationToken));
        }

        return results;
    }

    private static async Task<ChannelOutput> ToChannelOutputAsync(
        IDataContainer channel,
        IInfoProvider<ContentLanguageInfo> languageProvider,
        CancellationToken cancellationToken)
    {
        string domain = channel.GetValue(nameof(WebsiteChannelInfo.WebsiteChannelDomain))?.ToString() ?? string.Empty;
        int primaryLanguageId = ValidationHelper.GetInteger(
            channel.GetValue(nameof(WebsiteChannelInfo.WebsiteChannelPrimaryContentLanguageID)), 0);

        var language = primaryLanguageId > 0
            ? (await languageProvider.Get()
                .WhereEquals(nameof(ContentLanguageInfo.ContentLanguageID), primaryLanguageId)
                .GetEnumerableTypedResultAsync(cancellationToken: cancellationToken))
                .FirstOrDefault()
            : null;

        var general = new SiteGeneralOutput
        {
            // Fix: use WebsiteChannelID (the website channel), not ChannelID (content-hub channel).
            ID = ValidationHelper.GetInteger(channel.GetValue(nameof(WebsiteChannelInfo.WebsiteChannelID)), 0),
            GUID = ValidationHelper.GetGuid(channel.GetValue(nameof(ChannelInfo.ChannelGUID)), Guid.Empty),
            DisplayName = channel.GetValue(nameof(ChannelInfo.ChannelDisplayName))?.ToString() ?? string.Empty,
            CodeName = channel.GetValue(nameof(ChannelInfo.ChannelName))?.ToString() ?? string.Empty,
            WebsitedomainresentationURL = domain,
        };
        return new ChannelOutput
        {
            General = general
        };
    }

    public static async Task<List<ContentItemReference>> BuildReferencesAsync<T>(
            IContentService contentService,
            IEnumerable<string>? identifiers,
            Func<T, string> nameSelector,
            CancellationToken cancellationToken)
            where T : class, IWebPageFieldsSource
    {
        var names = identifiers?
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Select(name => name.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList() ?? new List<string>();

        if (names.Count == 0)
        {
            return new List<ContentItemReference>();
        }

        var items = await contentService.GetItemsAsync<T>(
            builder => builder.Parameters(p => p.Where(w => w.WhereIn("Name", names))),
            cancellationToken);

        return items
            .Where(item => names.Contains(nameSelector(item), StringComparer.OrdinalIgnoreCase))
            .Select(item => item.SystemFields.ContentItemGUID)
            .Distinct()
            .Select(guid => new ContentItemReference { Identifier = guid })
            .ToList();
    }

    public static async Task<List<ContentItemReference>> BuildContentAttachmentImageReferences(
        IContentService contentService, string? url, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return new List<ContentItemReference>();
        }

        var value = url.Trim();

        if (Guid.TryParse(value, out var guid))
        {
            return new List<ContentItemReference> { new ContentItemReference { Identifier = guid } };
        }

        var attachments = await contentService.GetItemsAsync<ContentAttachment>(
            builder => builder.Parameters(p => p.Where(w => w.WhereNotNull("ContentAttachmentAsset"))),
            cancellationToken);

        var match = attachments.FirstOrDefault(a => UrlsMatch(a.ContentAttachmentAsset?.Url, value));

        return match is null
            ? new List<ContentItemReference>()
            : new List<ContentItemReference> { new ContentItemReference { Identifier = match.SystemFields.ContentItemGUID } };
    }

    private static bool UrlsMatch(string? assetUrl, string? requestedUrl)
    {
        if (string.IsNullOrWhiteSpace(assetUrl) || string.IsNullOrWhiteSpace(requestedUrl))
        {
            return false;
        }

        static string Normalize(string url)
        {
            var normalized = url.Trim().TrimStart('~');

            var queryIndex = normalized.IndexOf('?');
            if (queryIndex >= 0)
            {
                normalized = normalized[..queryIndex];
            }

            return normalized.TrimEnd('/');
        }

        var normalizedAsset = Normalize(assetUrl);
        var normalizedRequested = Normalize(requestedUrl);

        if (string.Equals(normalizedAsset, normalizedRequested, StringComparison.OrdinalIgnoreCase)
            || normalizedAsset.EndsWith(normalizedRequested, StringComparison.OrdinalIgnoreCase)
            || normalizedRequested.EndsWith(normalizedAsset, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        static string FileName(string url) => url[(url.LastIndexOf('/') + 1)..];

        return string.Equals(FileName(normalizedAsset), FileName(normalizedRequested), StringComparison.OrdinalIgnoreCase);
    }
}