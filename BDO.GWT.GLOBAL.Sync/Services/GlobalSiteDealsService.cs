using BDO.GWT.GLOBAL.Sync.Data;
using CMS.ContentEngine;
using CMS.Membership;
using CMS.Websites;

namespace BDO.GWT.GLOBAL.Sync.Services;

public interface IGlobalSiteDealsService
{
    Task SynchronizeChannelGlobalDealsAsync(
        ChannelOutput globalChannel,
        IReadOnlyCollection<ChannelOutput> sourceChannels,
        bool forceUpdate,
        CancellationToken cancellationToken);
}

public class GlobalSiteDealsService : IGlobalSiteDealsService
{
    private const string GLOBAL_CULTURE = "en-gb";
    private const string REMOTE_DEAL_CONTENT_TYPE = RemoteDeal.CONTENT_TYPE_NAME;
    private const string SECTION_DEALS_CONTENT_TYPE = SectionDeals.CONTENT_TYPE_NAME;
    private const string DEAL_CONTENT_TYPE = Deal.CONTENT_TYPE_NAME;
    private const string ADMIN_USER_NAME = "administrator";
    private const int DEAL_LINKED_ITEMS_MAX_LEVEL = 2;

    private const string DEAL_CARD_CODE_NAME_FIELD = "CodeName";

    private readonly IContentQueryExecutor _contentQueryExecutor;
    private readonly IWebPageManagerFactory _webPageManagerFactory;
    private readonly IUserInfoProvider _userInfoProvider;
    private readonly ILogger<GlobalSiteDealsService> _logger;

    public GlobalSiteDealsService(
        IContentQueryExecutor contentQueryExecutor,
        IWebPageManagerFactory webPageManagerFactory,
        IUserInfoProvider userInfoProvider,
        ILogger<GlobalSiteDealsService> logger)
    {
        _contentQueryExecutor = contentQueryExecutor;
        _webPageManagerFactory = webPageManagerFactory;
        _userInfoProvider = userInfoProvider;
        _logger = logger;
    }

    public async Task SynchronizeChannelGlobalDealsAsync(
        ChannelOutput globalChannel,
        IReadOnlyCollection<ChannelOutput> sourceChannels,
        bool forceUpdate,
        CancellationToken cancellationToken)
    {
        if (sourceChannels.Count == 0)
        {
            _logger.LogInformation("No source channels supplied; nothing to synchronize.");
            return;
        }

        var adminUser = _userInfoProvider.Get(ADMIN_USER_NAME);
        if (adminUser is null)
        {
            _logger.LogError("Could not resolve the '{User}' user account", ADMIN_USER_NAME);
            return;
        }
        var webPageManager = _webPageManagerFactory.Create(globalChannel.General.ID, adminUser.UserID);

        var sectionDeal = await GetSectionDealsPageAsync(globalChannel, cancellationToken);
        if (sectionDeal is null)
        {
            _logger.LogError("Could not resolve the SectionDeals landing page on channel {Channel}", globalChannel.General.CodeName);
            return;
        }

        var remoteDeals = await GetRemoteDealsAsync(globalChannel, cancellationToken);

        var sourceDeals = await GetSourceDealsAsync(sourceChannels, cancellationToken);

        var (dealsToCreate, dealsToUpdate, dealsToRemove) = BuildDealsDiff(remoteDeals, sourceDeals, forceUpdate);

        var dealsNeedingMapping = dealsToCreate
            .Select(x => x.Deal)
            .Concat(dealsToUpdate.Select(x => x.Deal))
            .ToList();

        var industryReferenceByCodeName = await GetGlobalDealCardReferencesByCodeNameAsync<DealCardIndustry>(
            DealCardIndustry.CONTENT_TYPE_NAME,
            globalChannel,
            CollectDistinctCodeNames(dealsNeedingMapping, d => d.Industry, i => i.CodeName),
            i => i.CodeName,
            cancellationToken);

        var subIndustryReferenceByCodeName = await GetGlobalDealCardReferencesByCodeNameAsync<DealCardSubIndustry>(
            DealCardSubIndustry.CONTENT_TYPE_NAME,
            globalChannel,
            CollectDistinctCodeNames(dealsNeedingMapping, d => d.SubIndustry, s => s.CodeName),
            s => s.CodeName,
            cancellationToken);

        var dealTypeReferenceByCodeName = await GetGlobalDealCardReferencesByCodeNameAsync<DealCardDealType>(
            DealCardDealType.CONTENT_TYPE_NAME,
            globalChannel,
            CollectDistinctCodeNames(dealsNeedingMapping, d => d.DealType, d => d.CodeName),
            d => d.CodeName,
            cancellationToken);

        var dealCardReferences = new DealCardReferenceMaps(
            industryReferenceByCodeName, subIndustryReferenceByCodeName, dealTypeReferenceByCodeName);

        await DeleteRemovedDealsAsync(webPageManager, dealsToRemove, cancellationToken);
        await UpdateOutdatedDealsAsync(webPageManager, dealsToUpdate, dealCardReferences, cancellationToken);
        await CreateMissingDealsAsync(webPageManager, sectionDeal.SystemFields.WebPageItemID, dealsToCreate, dealCardReferences, cancellationToken);
    }

    private async Task<SectionDeals?> GetSectionDealsPageAsync(ChannelOutput globalChannel, CancellationToken cancellationToken)
    {
        var builder = new ContentItemQueryBuilder()
            .ForContentType(SECTION_DEALS_CONTENT_TYPE, q => q
                .ForWebsite(globalChannel.General.CodeName)
                .TopN(1))
            .InLanguage(GLOBAL_CULTURE);

        var sectionDeals = await _contentQueryExecutor.GetMappedResult<SectionDeals>(
            builder, new ContentQueryExecutionOptions { ForPreview = true }, cancellationToken: cancellationToken);

        return sectionDeals.FirstOrDefault();
    }

    private async Task<List<RemoteDeal>> GetRemoteDealsAsync(ChannelOutput globalChannel, CancellationToken cancellationToken)
    {
        var builder = new ContentItemQueryBuilder()
            .ForContentType(REMOTE_DEAL_CONTENT_TYPE, q => q
                .ForWebsite(globalChannel.General.CodeName))
            .InLanguage(GLOBAL_CULTURE);

        var remoteDeals = await _contentQueryExecutor.GetMappedResult<RemoteDeal>(
            builder, new ContentQueryExecutionOptions { ForPreview = true }, cancellationToken: cancellationToken);

        return remoteDeals.ToList();
    }

    private async Task<List<(Deal Deal, Guid ChannelGuid)>> GetSourceDealsAsync(
        IReadOnlyCollection<ChannelOutput> sourceChannels, CancellationToken cancellationToken)
    {
        var dealsWithChannel = new List<(Deal Deal, Guid ChannelGuid)>();

        foreach (var channel in sourceChannels)
        {
            var builder = new ContentItemQueryBuilder()
                .ForContentType(DEAL_CONTENT_TYPE, q => q
                    .ForWebsite(channel.General.CodeName)
                    .Where(w => w.WhereEquals(nameof(Deal.SharingOptIn), true))
                    .WithLinkedItems(DEAL_LINKED_ITEMS_MAX_LEVEL))
                .InLanguage(GLOBAL_CULTURE);

            try
            {
                var channelDeals = await _contentQueryExecutor.GetMappedResult<Deal>(
                    builder, new ContentQueryExecutionOptions { ForPreview = true }, cancellationToken: cancellationToken);

                dealsWithChannel.AddRange(channelDeals.Select(d => (Deal: d, ChannelGuid: channel.General.GUID)));
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Failed to retrieve deals for channel {ChannelName} ({ChannelGuid})",
                    channel.General.CodeName, channel.General.GUID);
            }
        }

        return dealsWithChannel;
    }

    private static (
        List<(Deal Deal, Guid ChannelGuid)> ToCreate,
        List<(Deal Deal, Guid ChannelGuid, RemoteDeal Existing)> ToUpdate,
        List<RemoteDeal> ToRemove) BuildDealsDiff(
            List<RemoteDeal> remoteDeals,
            List<(Deal Deal, Guid ChannelGuid)> sourceDeals,
            bool forceUpdate)
    {
        var sourcePageGuids = sourceDeals.Select(x => x.Deal.SystemFields.WebPageItemGUID).ToHashSet();

        var toCreate = sourceDeals
            .Where(x => !remoteDeals.Any(rd => rd.OriginalPageGUID == x.Deal.SystemFields.WebPageItemGUID))
            .ToList();

        var toUpdate = sourceDeals
            .Select(x => new
            {
                x.Deal,
                x.ChannelGuid,
                Existing = remoteDeals.FirstOrDefault(rd => rd.OriginalPageGUID == x.Deal.SystemFields.WebPageItemGUID)
            })
            .Where(x => x.Existing is not null
                && (forceUpdate
                    || x.Existing!.ForceUpdateOnNextSync
                    || x.Deal.SystemFields.ContentItemCommonDataLastPublishedWhen > x.Existing.OriginalModifiedWhen))
            .Select(x => (x.Deal, x.ChannelGuid, Existing: x.Existing!))
            .ToList();

        var toRemove = remoteDeals
            .Where(rd => !sourcePageGuids.Contains(rd.OriginalPageGUID))
            .ToList();

        return (toCreate, toUpdate, toRemove);
    }

    private async Task DeleteRemovedDealsAsync(
        IWebPageManager webPageManager, List<RemoteDeal> dealsToRemove, CancellationToken cancellationToken)
    {
        foreach (var remoteDeal in dealsToRemove)
        {
            try
            {
                await webPageManager.Delete(new DeleteWebPageParameters(
                    remoteDeal.SystemFields.WebPageItemID, GLOBAL_CULTURE)
                {
                    Permanently = true,
                }, cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Failed to delete remote deal for source page {Guid}", remoteDeal.OriginalPageGUID);
            }
        }
    }

    private async Task UpdateOutdatedDealsAsync(
        IWebPageManager webPageManager,
        List<(Deal Deal, Guid ChannelGuid, RemoteDeal Existing)> dealsToUpdate,
        DealCardReferenceMaps dealCardReferences,
        CancellationToken cancellationToken)
    {
        foreach (var entry in dealsToUpdate)
        {
            var pageId = entry.Existing.SystemFields.WebPageItemID;

            try
            {
                var itemData = BuildRemoteDealItemData(entry.Deal, entry.ChannelGuid, dealCardReferences);

                await webPageManager.TryCreateDraft(pageId, GLOBAL_CULTURE, cancellationToken);

                if (!await webPageManager.TryUpdateDraft(pageId, GLOBAL_CULTURE, new UpdateDraftData(itemData), cancellationToken))
                {
                    throw new InvalidOperationException($"Failed to update draft for remote deal {entry.Deal.SystemFields?.WebPageItemGUID}.");
                }

                if (!await webPageManager.TryPublish(pageId, GLOBAL_CULTURE, cancellationToken))
                {
                    throw new InvalidOperationException($"Failed to publish remote deal {entry.Deal.SystemFields?.WebPageItemGUID}.");
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Failed to update remote deal for source page {Guid}", entry.Deal.SystemFields?.WebPageItemGUID);
            }
        }
    }

    private async Task CreateMissingDealsAsync(
        IWebPageManager webPageManager,
        int sectionDealsWebPageItemID,
        List<(Deal Deal, Guid ChannelGuid)> dealsToCreate,
        DealCardReferenceMaps dealCardReferences,
        CancellationToken cancellationToken)
    {
        foreach (var entry in dealsToCreate)
        {
            try
            {
                var itemData = BuildRemoteDealItemData(entry.Deal, entry.ChannelGuid, dealCardReferences);

                var contentItemParameters = new ContentItemParameters(REMOTE_DEAL_CONTENT_TYPE, itemData);
                var createPageParameters = new CreateWebPageParameters(
                    entry.Deal.MetadataTitle ?? "Remote Deal",
                    GLOBAL_CULTURE,
                    contentItemParameters)
                {
                    ParentWebPageItemID = sectionDealsWebPageItemID
                };

                int newPageId = await webPageManager.Create(createPageParameters, cancellationToken);

                if (!await webPageManager.TryPublish(newPageId, GLOBAL_CULTURE, cancellationToken))
                {
                    throw new InvalidOperationException($"Failed to publish new remote deal (source {entry.Deal.SystemFields?.WebPageItemGUID}).");
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Failed to create remote deal for source page {Guid}", entry.Deal.SystemFields?.WebPageItemGUID);
            }
        }
    }

    private sealed record DealCardReferenceMaps(
        IReadOnlyDictionary<string, ContentItemReference> IndustryByCodeName,
        IReadOnlyDictionary<string, ContentItemReference> SubIndustryByCodeName,
        IReadOnlyDictionary<string, ContentItemReference> DealTypeByCodeName);

    private static ContentItemData BuildRemoteDealItemData(
        Deal deal,
        Guid channelGUID,
        DealCardReferenceMaps dealCardReferences)
    {
        var industryReferences = ResolveReferences(deal.Industry, i => i.CodeName, dealCardReferences.IndustryByCodeName);
        var subIndustryReferences = ResolveReferences(deal.SubIndustry, s => s.CodeName, dealCardReferences.SubIndustryByCodeName);
        var dealTypeReferences = ResolveReferences(deal.DealType, d => d.CodeName, dealCardReferences.DealTypeByCodeName);

        return new ContentItemData(new Dictionary<string, object>
        {
            { nameof(RemoteDeal.MetadataTitle), deal.MetadataTitle ?? string.Empty },
            { nameof(RemoteDeal.MetadataDescription), deal.MetadataDescription ?? string.Empty },
            { nameof(RemoteDeal.MetadataTeaserImage), deal.MetadataTeaserImage?
                .Select(img => new ContentItemReference { Identifier = img.SystemFields.ContentItemGUID })
                .ToList() ?? new List<ContentItemReference>() },
            { nameof(RemoteDeal.MetadataTeaserImageAltText), deal.MetadataTeaserImageAltText ?? string.Empty },
            { nameof(RemoteDeal.MetadataCanonical), deal.MetadataCanonical ?? string.Empty },
            { nameof(RemoteDeal.Industry), industryReferences },
            { nameof(RemoteDeal.SubIndustry), subIndustryReferences },
            { nameof(RemoteDeal.DealType), dealTypeReferences },
            { nameof(RemoteDeal.DateField), deal.DateField },
            { nameof(RemoteDeal.DealCountry), deal.DealCountry ?? string.Empty },
            { nameof(RemoteDeal.OriginalSiteGUID), channelGUID },
            { nameof(RemoteDeal.OriginalPageGUID), deal.SystemFields.WebPageItemGUID },
            { nameof(RemoteDeal.OriginalPageURL), deal.GetUrl().AbsoluteUrl ?? string.Empty },
            { nameof(RemoteDeal.OriginalModifiedWhen), deal.SystemFields.ContentItemCommonDataLastPublishedWhen ?? DateTime.UtcNow },
            { nameof(RemoteDeal.ForceUpdateOnNextSync), false },
            { nameof(RemoteDeal.IncludeInSitemap), deal.IncludeInSitemap },
            { nameof(RemoteDeal.IsSearchExcluded), deal.IsSearchExcluded },
        });
    }

    private static List<ContentItemReference> ResolveReferences<T>(
        IEnumerable<T>? sourceItems,
        Func<T, string> codeNameSelector,
        IReadOnlyDictionary<string, ContentItemReference> referenceByCodeName)
    {
        if (sourceItems is null)
        {
            return new List<ContentItemReference>();
        }

        return sourceItems
            .Select(item => referenceByCodeName.TryGetValue(codeNameSelector(item), out var reference) ? reference : null)
            .Where(reference => reference is not null)
            .Select(reference => reference!)
            .Distinct()
            .ToList();
    }

    private static List<string> CollectDistinctCodeNames<T>(
        IEnumerable<Deal> deals, Func<Deal, IEnumerable<T>?> fieldSelector, Func<T, string> codeNameSelector)
    {
        return deals
            .SelectMany(deal => fieldSelector(deal) ?? Enumerable.Empty<T>())
            .Select(codeNameSelector)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Select(name => name.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private async Task<Dictionary<string, ContentItemReference>> GetGlobalDealCardReferencesByCodeNameAsync<T>(
        string contentType,
        ChannelOutput globalChannel,
        List<string> codeNames,
        Func<T, string> codeNameSelector,
        CancellationToken cancellationToken)
        where T : class, IWebPageFieldsSource
    {
        var referenceByCodeName = new Dictionary<string, ContentItemReference>(StringComparer.OrdinalIgnoreCase);

        if (codeNames.Count == 0)
        {
            return referenceByCodeName;
        }

        var builder = new ContentItemQueryBuilder()
            .ForContentType(contentType, q => q
                .ForWebsite(globalChannel.General.CodeName)
                .Where(w => w.WhereIn(DEAL_CARD_CODE_NAME_FIELD, codeNames)))
            .InLanguage(GLOBAL_CULTURE);

        var items = await _contentQueryExecutor.GetMappedResult<T>(
            builder, new ContentQueryExecutionOptions { ForPreview = true }, cancellationToken: cancellationToken);

        foreach (var group in items.GroupBy(codeNameSelector, StringComparer.OrdinalIgnoreCase))
        {
            var matches = group.ToList();

            if (matches.Count > 1)
            {
                _logger.LogWarning(
                    "Found {Count} '{ContentType}' pages on global channel {Channel} with CodeName '{CodeName}'; using the first match.",
                    matches.Count, contentType, globalChannel.General.CodeName, group.Key);
            }

            referenceByCodeName[group.Key] = new ContentItemReference { Identifier = matches[0].SystemFields.ContentItemGUID };
        }

        var missingCodeNames = codeNames.Except(referenceByCodeName.Keys, StringComparer.OrdinalIgnoreCase).ToList();
        if (missingCodeNames.Count > 0)
        {
            _logger.LogWarning(
                "No matching '{ContentType}' page exists on global channel {Channel} for CodeName(s): {CodeNames}. " +
                "Those references will be omitted from the synced RemoteDeal(s) until a matching page is created on the global site.",
                contentType, globalChannel.General.CodeName, string.Join(", ", missingCodeNames));
        }

        return referenceByCodeName;
    }
}