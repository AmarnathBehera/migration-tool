using CMS.ContentEngine;
using CMS.Websites;

using Kentico.Xperience.UMT.Model;
using Kentico.Xperience.UMT.Services;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Migration.Tool.Common;
using Migration.Tool.Common.Abstractions;
using Migration.Tool.Common.Helpers;
using Migration.Tool.Common.MigrationProtocol;
using Migration.Tool.Core.KX13.Helpers;
using Migration.Tool.KX13;
using Migration.Tool.KX13.Context;
using Migration.Tool.KX13.Models;

namespace Migration.Tool.Core.KX13.Handlers;

// ReSharper disable once UnusedType.Global
public class MigrateSitesCommandHandler(
    ILogger<MigrateSitesCommandHandler> logger,
    IDbContextFactory<KX13Context> kx13ContextFactory,
    IProtocol protocol,
    IImporter importer,
    ToolConfiguration toolConfiguration)
    : IRequestHandler<MigrateSitesCommand, CommandResult>
{
    public async Task<CommandResult> Handle(MigrateSitesCommand request, CancellationToken cancellationToken)
    {
        await using var kx13Context = await kx13ContextFactory.CreateDbContextAsync(cancellationToken);
        var migratedCultureCodes = new Dictionary<string, ContentLanguageInfo>(StringComparer.CurrentCultureIgnoreCase);
        string GetDestinationCultureCode(string sourceCultureCode) =>
            toolConfiguration.CultureCodeMappings.TryGetValue(sourceCultureCode, out var destinationCultureCode)
                ? destinationCultureCode
                : sourceCultureCode;
        var entityConfiguration = toolConfiguration.EntityConfigurations.GetEntityConfiguration<CmsSite>();
        var domainSanitizer = new WebsiteChannelDomainSanitizer(logger, WebsiteChannelInfo.Provider.Get());
        var existingChannels = ChannelInfo.Provider.Get();

        foreach (var kx13CmsSite in kx13Context.CmsSites.Include(s => s.Cultures))
        {
            protocol.FetchedSource(kx13CmsSite);
            logger.LogInformation("Migrating site {SiteName} with SiteGuid {SiteGuid}", kx13CmsSite.SiteName, kx13CmsSite.SiteGuid);
            if (!entityConfiguration.IncludesCodeName(kx13CmsSite.SiteName))
            {
                logger.LogInformation("Site excluded in settings");
                continue;
            }

            string defaultCultureCode = GetSiteCulture(kx13CmsSite);
            string destinationDefaultCultureCode = GetDestinationCultureCode(defaultCultureCode);
            var migratedSiteCultures = kx13CmsSite.Cultures.ToList();
            if (!migratedSiteCultures.Any(x => x.CultureCode.Equals(defaultCultureCode, StringComparison.InvariantCultureIgnoreCase)))
            {
                await using var ctx = await kx13ContextFactory.CreateDbContextAsync(cancellationToken);
                if (ctx.CmsCultures.FirstOrDefault(c => c.CultureCode == defaultCultureCode) is { } defaultCulture)
                {
                    migratedSiteCultures.Add(defaultCulture);
                }
            }

            foreach (var cmsCulture in migratedSiteCultures)
            {
                string destinationCultureCode = GetDestinationCultureCode(cmsCulture.CultureCode);
                if (migratedCultureCodes.ContainsKey(destinationCultureCode))
                {
                    continue;
                }

                var existing = ContentLanguageInfo.Provider.Get()
                    .WhereEquals(nameof(ContentLanguageInfo.ContentLanguageCultureFormat), destinationCultureCode)
                    .FirstOrDefault();

                if (existing != null)
                {
                    // Insight page migration: preserve shared XbyK language GUIDs and their existing channel references.
                    migratedCultureCodes.TryAdd(destinationCultureCode, existing);
                }
                else
                {
                    var langResult = await importer.ImportAsync(new ContentLanguageModel
                    {
                        ContentLanguageGUID = cmsCulture.CultureGuid,
                        ContentLanguageDisplayName = cmsCulture.CultureName,
                        ContentLanguageName = destinationCultureCode,
                        ContentLanguageIsDefault = string.Equals(destinationCultureCode, destinationDefaultCultureCode, StringComparison.InvariantCultureIgnoreCase),
                        ContentLanguageFallbackContentLanguageGuid = null,
                        ContentLanguageCultureFormat = destinationCultureCode
                    });

                    if (langResult is { Success: true, Imported: ContentLanguageInfo importedLanguage })
                    {
                        migratedCultureCodes.TryAdd(destinationCultureCode, importedLanguage);
                        logger.LogTrace("Imported language {Language} from {Culture}", importedLanguage.ContentLanguageName, cmsCulture.CultureCode);
                    }
                }
            }

            // Insight page migration: use the configured XbyK channel name instead of creating a second Site channel.
            var targetChannelName = toolConfiguration.TargetWebsiteChannelMappings.TryGetValue(kx13CmsSite.SiteName, out var mappedName)
                ? mappedName
                : kx13CmsSite.SiteName;
            var existingChannel = existingChannels.FirstOrDefault(ch => ch.ChannelName.Equals(targetChannelName, StringComparison.OrdinalIgnoreCase));
            if (existingChannel != null)
            {
                var existingWebsiteChannel = WebsiteChannelInfo.Provider.Get()
                    .WhereEquals(nameof(WebsiteChannelInfo.WebsiteChannelChannelID), existingChannel.ChannelID)
                    .FirstOrDefault()
                    ?? throw new InvalidOperationException($"Target channel '{targetChannelName}' is not a website channel.");
                MigrateCaptchaSettings(kx13CmsSite, existingWebsiteChannel);
                logger.LogInformation("Site skipped after synchronizing cultures. It already exists.");
                continue;
            }

            string? homePagePath = KenticoHelper.GetSettingsKey(kx13ContextFactory, kx13CmsSite.SiteId, SettingsKeys.CMSHomePagePath);
            int? cookieLevel = KenticoHelper.GetSettingsKey(kx13ContextFactory, kx13CmsSite.SiteId, SettingsKeys.CMSDefaultCookieLevel) switch
            {
                "all" => CookieLevelConstants.ALL,
                "visitor" => CookieLevelConstants.VISITOR,
                "editor" => CookieLevelConstants.EDITOR,
                "system" => CookieLevelConstants.SYSTEM,
                "essential" => CookieLevelConstants.ESSENTIAL,
                _ => null
            };
            bool? storeFormerUrls = KenticoHelper.GetSettingsKey(kx13ContextFactory, kx13CmsSite.SiteId, "CMSStoreFormerUrls") is string storeFormerUrlsStr
                ? bool.TryParse(storeFormerUrlsStr, out bool sfu) ? sfu : null
                : null;

            var requestedDomain = toolConfiguration.TargetWebsiteChannelDomains.TryGetValue(kx13CmsSite.SiteName, out var targetDomain)
                ? targetDomain
                : kx13CmsSite.SiteDomainName.Trim('/');
            if (!domainSanitizer.GetCandidate(kx13CmsSite.SiteName, requestedDomain, out var domainName))
            {
                continue;
            }

            var channelResult = await importer.ImportAsync(new ChannelModel { ChannelDisplayName = kx13CmsSite.SiteDisplayName, ChannelName = targetChannelName, ChannelGUID = kx13CmsSite.SiteGuid, ChannelType = ChannelType.Website });

            var websiteChannelModel = new WebsiteChannelModel
            {
                WebsiteChannelGUID = kx13CmsSite.SiteGuid,
                WebsiteChannelChannelGuid = kx13CmsSite.SiteGuid,
                WebsiteChannelDomain = domainName,
                WebsiteChannelHomePage = homePagePath,
                WebsiteChannelPrimaryContentLanguageGuid = migratedCultureCodes[destinationDefaultCultureCode].ContentLanguageGUID,
                WebsiteChannelDefaultCookieLevel = cookieLevel,
                WebsiteChannelStoreFormerUrls = storeFormerUrls
            };
            var webSiteChannelResult = await importer.ImportAsync(websiteChannelModel);

            if (!webSiteChannelResult.Success)
            {
                if (webSiteChannelResult.ModelValidationResults != null)
                {
                    foreach (var mvr in webSiteChannelResult.ModelValidationResults)
                    {
                        logger.LogError("Invalid channel properties {Members}: {ErrorMessage}", string.Join(", ", mvr.MemberNames), mvr.ErrorMessage);
                    }
                }
                else
                {
                    logger.LogError(webSiteChannelResult.Exception, "Failed to migrate site");
                }

                return new CommandFailureResult();
            }

            if (webSiteChannelResult.Imported is WebsiteChannelInfo webSiteChannel)
            {
                domainSanitizer.CommitCandidate(domainName);
                MigrateCaptchaSettings(kx13CmsSite, webSiteChannel);
            }
        }

        return new GenericCommandResult();
    }

    private void MigrateCaptchaSettings(CmsSite sourceSite, WebsiteChannelInfo websiteChannel)
    {
        string? v2SiteKey = KenticoHelper.GetSettingsKey(kx13ContextFactory, sourceSite.SiteId, "CMSReCaptchaPublicKey");
        string? v2SecretKey = KenticoHelper.GetSettingsKey(kx13ContextFactory, sourceSite.SiteId, "CMSReCaptchaPrivateKey");
        string? v3SiteKey = KenticoHelper.GetSettingsKey(kx13ContextFactory, sourceSite.SiteId, "CMSRecaptchaV3PublicKey");
        string? v3SecretKey = KenticoHelper.GetSettingsKey(kx13ContextFactory, sourceSite.SiteId, "CMSReCaptchaV3PrivateKey");
        bool hasV2 = !string.IsNullOrWhiteSpace(v2SiteKey) || !string.IsNullOrWhiteSpace(v2SecretKey);
        bool hasV3 = !string.IsNullOrWhiteSpace(v3SiteKey) || !string.IsNullOrWhiteSpace(v3SecretKey);

        if (toolConfiguration.TargetWebsiteChannelReCaptchaVersions.TryGetValue(sourceSite.SiteName, out var preferredVersion))
        {
            if (preferredVersion.Equals("v3", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(v3SiteKey) || string.IsNullOrWhiteSpace(v3SecretKey))
                {
                    throw new InvalidOperationException($"Source site '{sourceSite.SiteName}' must have both reCAPTCHA v3 keys configured.");
                }
                hasV2 = false;
            }
            else if (preferredVersion.Equals("v2", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(v2SiteKey) || string.IsNullOrWhiteSpace(v2SecretKey))
                {
                    throw new InvalidOperationException($"Source site '{sourceSite.SiteName}' must have both reCAPTCHA v2 keys configured.");
                }
                hasV3 = false;
            }
            else
            {
                throw new InvalidOperationException($"Invalid reCAPTCHA preference for '{sourceSite.SiteName}'; use 'v2' or 'v3'.");
            }
        }

        if (hasV2 && hasV3)
        {
            throw new InvalidOperationException($"Source site '{sourceSite.SiteName}' has conflicting reCAPTCHA settings. Configure TargetWebsiteChannelReCaptchaVersions to select 'v2' or 'v3'.");
        }

        if (!hasV2 && !hasV3)
        {
            return;
        }

        var settings = WebsiteCaptchaSettingsInfo.Provider.Get()
            .WhereEquals(nameof(WebsiteCaptchaSettingsInfo.WebsiteCaptchaSettingsWebsiteChannelID), websiteChannel.WebsiteChannelID)
            .FirstOrDefault() ?? new WebsiteCaptchaSettingsInfo();
        settings.WebsiteCaptchaSettingsWebsiteChannelID = websiteChannel.WebsiteChannelID;
        settings.WebsiteCaptchaSettingsReCaptchaSiteKey = hasV3 ? v3SiteKey : v2SiteKey;
        settings.WebsiteCaptchaSettingsReCaptchaSecretKey = hasV3 ? v3SecretKey : v2SecretKey;
        settings.WebsiteCaptchaSettingsReCaptchaVersion = hasV3 ? ReCaptchaVersion.ReCaptchaV3 : ReCaptchaVersion.ReCaptchaV2;
        if (hasV3)
        {
            settings.WebsiteCaptchaSettingsReCaptchaThreshold = KenticoHelper.GetSettingsKey<double>(kx13ContextFactory, sourceSite.SiteId, "CMSRecaptchaV3Threshold") ?? 0.5d;
        }
        WebsiteCaptchaSettingsInfo.Provider.Set(settings);
        logger.LogInformation("Migrated reCAPTCHA {Version} settings for source site '{SiteName}'", hasV3 ? "v3" : "v2", sourceSite.SiteName);
    }

    private string GetSiteCulture(CmsSite site)
    {
        // simplified logic from CMS.DocumentEngine.DefaultPreferredCultureEvaluator.Evaluate()
        // domain alias skipped, HttpContext logic skipped
        string? siteCulture = site.SiteDefaultVisitorCulture
                              ?? KenticoHelper.GetSettingsKey(kx13ContextFactory, site.SiteId, SettingsKeys.CMSDefaultCultureCode);

        return siteCulture
               ?? throw new InvalidOperationException("Unknown site culture");
    }
}
