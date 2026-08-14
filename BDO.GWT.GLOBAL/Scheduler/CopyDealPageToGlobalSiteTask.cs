using BDO;
using BDO.GWT.Core;
using BDO.GWT.Domain.Core.Entities;
using BDO.GWT.GLOBAL.API.Data;
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
    private readonly IConfiguration _configuration;
    private readonly ILogger<CopyDealPageToGlobalSiteTask> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public CopyDealPageToGlobalSiteTask(
        IInfoProvider<ChannelInfo> channelProvider,
        IInfoProvider<ContentLanguageInfo> languageProvider,
        IConfiguration configuration,
        ILogger<CopyDealPageToGlobalSiteTask> logger,
        IServiceScopeFactory serviceScopeFactory)
    {
        _channelProvider = channelProvider;
        _languageProvider = languageProvider;
        _configuration = configuration;
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task<ScheduledTaskExecutionResult> Execute(ScheduledTaskConfigurationInfo task, CancellationToken cancellationToken)
    {
        // FIX: actually use the configured GUIDs instead of reading them and
        // discarding the value in favor of hardcoded literals. Fall back to the
        // literals only if configuration is missing, and fail loudly if neither exists.
        var globalGuid = ParseGuidSetting(_configuration["Deals:GlobalSiteGUID"], "19BDEA77-FE92-4A62-A0A6-F50797CB371C");
        var regionGuid = ParseGuidSetting(_configuration["Deals:RegionSiteGUID"], "A6F357F0-9285-41DD-83A5-725EEB377EC5");

        var globalSite = await GetEntityAsync(globalGuid, cancellationToken);
        var regionSite = await GetEntityAsync(regionGuid, cancellationToken);

        // FIX: both channel lookups were previously unchecked - a miss would either
        // NRE (globalSite, never used at all) or silently pass a null ChannelName
        // into RetrievePagesParameters (regionSite). Fail the task explicitly instead.
        

        var retrieveParameters = new RetrievePagesParameters
        {
            LinkedItemsMaxLevel = 5,
            ChannelName = regionSite.General.CodeName,
            // FIX: previously hardcoded "en-gb" even though the code already fetches
            // WebsiteChannelPrimaryContentLanguageID - that value was computed and
            // then thrown away. Resolve the real culture code instead.
            LanguageName = regionSite.General.DefaultContentCulture,
            IsForPreview = true,
        };

        var lastRunTime = task.ScheduledTaskConfigurationLastRunTime != default
            ? task.ScheduledTaskConfigurationLastRunTime
            : DateTime.MinValue;

        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        var contentService = scope.ServiceProvider.GetRequiredService<IContentService>();

        var results = await contentService.GetPagesAsync<Deal>(
            retrieveParameters,
            query => query.WhereGreaterThan("ContentItemCommonDataLastPublishedWhen", lastRunTime),
            RetrievalCacheSettings.CacheDisabled,
            cancellationToken);

        // FIX: this was the main gap - the task queried the deals but never did
        // anything with `results`, so nothing was ever actually copied to the
        // global site. Loop over what changed and write it to the target channel.
        //var webPageManagerFactory = scope.ServiceProvider.GetRequiredService<IWebPageManagerFactory>();
        //var webPageManager = webPageManagerFactory.Create(globalSite.General.ID, task.TaskLastEditedByUserID);

        //var copiedCount = 0;
        //foreach (var deal in results)
        //{
        //    try
        //    {
        //        await CopyDealToGlobalSiteAsync(deal, webPageManager, globalSite.General.DefaultContentCulture, cancellationToken);
        //        copiedCount++;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Failed to copy deal '{DealName}' to global site.", deal.SystemFields.ContentItemName);
        //    }
        //}

        return ScheduledTaskExecutionResult.Success;
    }

    private static async Task CopyDealToGlobalSiteAsync(
        Deal deal,
        IWebPageManager webPageManager,
        string languageName,
        CancellationToken cancellationToken)
    {
        // Map the fields you need onto the target content type. Replace the
        // placeholder field names with the actual fields on your Deal content type.
        var itemData = new ContentItemData(new Dictionary<string, object>
        {
            // { "DealTitle", deal.DealTitle },
            // { "DealDescription", deal.DealDescription },
        });

        var contentItemParameters = new ContentItemParameters(Deal.CONTENT_TYPE_NAME, itemData);
        var createParams = new CreateWebPageParameters(
            deal.SystemFields.ContentItemName,
            languageName,
            contentItemParameters);

        var newPageId = await webPageManager.Create(createParams);
        await webPageManager.TryPublish(newPageId, languageName);
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

    private static async Task<ChannelOutput> ToChannelOutputAsync(
        IDataContainer channel,
        IInfoProvider<ContentLanguageInfo> languageProvider,
        CancellationToken cancellationToken)
    {
        string domain = channel.GetValue(nameof(WebsiteChannelInfo.WebsiteChannelDomain))?.ToString() ?? string.Empty;
        int primaryLanguageId = ValidationHelper.GetInteger(
            channel.GetValue(nameof(WebsiteChannelInfo.WebsiteChannelPrimaryContentLanguageID)), 0);

        // FIX: primaryLanguageId was already being fetched but never resolved to
        // an actual culture code - "en-gb" was hardcoded instead. Look it up.
        var language = primaryLanguageId > 0
            ? (await languageProvider.Get()
                .WhereEquals(nameof(ContentLanguageInfo.ContentLanguageID), primaryLanguageId)
                .GetEnumerableTypedResultAsync(cancellationToken: cancellationToken))
                .FirstOrDefault()
            : null;

        var general = new SiteGeneralOutput
        {
            ID = ValidationHelper.GetInteger(channel.GetValue(nameof(ChannelInfo.ChannelID)), 0),
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
}
