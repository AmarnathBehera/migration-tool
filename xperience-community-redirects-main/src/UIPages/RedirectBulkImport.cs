using CMS.ContentEngine;
using CMS.Core;
using CMS.DataEngine;
using CMS.Membership;
using CMS.Websites;
using CMS.Websites.Internal;
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BDO;
using XperienceCommunity.Redirects.UIPages;
using IFormItemCollectionProvider = Kentico.Xperience.Admin.Base.Forms.Internal.IFormItemCollectionProvider;

[assembly: UIPage(
    parentType: typeof(RedirectApplicationPage),
    slug: "bulk-import",
    uiPageType: typeof(RedirectBulkImport),
    name: "Bulk import",
    templateName: TemplateNames.EDIT,
    order: UIPageOrder.NoOrder)]

namespace XperienceCommunity.Redirects.UIPages;

// Explicitly requires CREATE permission to access this page, matching the
// semantics of what it actually does (bulk-creates RedirectInfo records) -
// don't rely on the EDIT template's default permission inference.
[UIEvaluatePermission(SystemPermissions.CREATE)]
internal class RedirectBulkImport : ModelEditPage<RedirectBulkImportModel>
{
    private static readonly char[] LineSeparators = { '\r', '\n' };
    private const int PageSize = 500;
    private const int MaxRows =200;

    private readonly record struct PageUrlLookupRow(int WebPageItemID, Guid WebPageItemGUID);

    private readonly IInfoProvider<RedirectInfo> _redirectInfoProvider;
    private readonly IInfoProvider<WebsiteChannelInfo> _websiteChannelInfoProvider;
    private readonly IContentQueryExecutor _executor;
    private readonly IWebPageUrlRetriever _webPageUrlRetriever;
    private readonly IEventLogService _eventLogService;

    private RedirectBulkImportModel? _model;

    public RedirectBulkImport(
        IFormItemCollectionProvider formItemCollectionProvider,
        IFormDataBinder formDataBinder,
        IInfoProvider<RedirectInfo> redirectInfoProvider,
        IInfoProvider<WebsiteChannelInfo> websiteChannelInfoProvider,
        IContentQueryExecutor executor,
        IWebPageUrlRetriever webPageUrlRetriever,
        IEventLogService eventLogService)
        : base(formItemCollectionProvider, formDataBinder)
    {
        _redirectInfoProvider = redirectInfoProvider;
        _websiteChannelInfoProvider = websiteChannelInfoProvider;
        _executor = executor;
        _webPageUrlRetriever = webPageUrlRetriever;
        _eventLogService = eventLogService;
    }

    protected override RedirectBulkImportModel Model => _model ??= new RedirectBulkImportModel();

    protected override async Task<ICommandResponse> ProcessFormData(RedirectBulkImportModel model, ICollection<IFormItem> formItems)
    {
        var response = await base.ProcessFormData(model, formItems);

        if (!int.TryParse(model.WebsiteChannelName, out int selectedChannelId))
        {
            return response.AddErrorMessage("The selected site is invalid.");
        }

        var websiteChannel = _websiteChannelInfoProvider.Get()
            .WhereEquals(nameof(WebsiteChannelInfo.WebsiteChannelChannelID), selectedChannelId)
            .TopN(1)
            .FirstOrDefault();

        var channel = ChannelInfo.Provider.Get(selectedChannelId);

        if (websiteChannel == null || channel == null)
        {
            return response.AddErrorMessage("The selected site could not be found.");
        }

        char delimiter = model.Delimiter switch
        {
            "comma" => ',',
            _ => ';',
        };

        var defaultLanguageName = GetDefaultLanguageName();

        if (string.IsNullOrEmpty(defaultLanguageName))
        {
            return response.AddErrorMessage("No default content language is configured.");
        }

        var rows = (model.CsvData ?? string.Empty)
            .Split(LineSeparators, StringSplitOptions.RemoveEmptyEntries)
            .Select(line => line.Trim())
            .Where(line => line.Length > 0)
            .ToList();

        if (model.HasHeaderRow && rows.Count > 0)
        {
            rows.RemoveAt(0);
        }

        if (rows.Count == 0)
        {
            return response.AddErrorMessage("No CSV rows were found to import.");
        }

        if (rows.Count > MaxRows)
        {
            return response.AddErrorMessage(
                $"This import contains {rows.Count} rows, which exceeds the {MaxRows}-row limit for a single import. Split the CSV into smaller batches.");
        }

        Dictionary<string, (Guid Guid, string Url)> urlLookup;

        try
        {
            urlLookup = await BuildChannelUrlLookupAsync(channel.ChannelName, defaultLanguageName);
        }
        catch (Exception ex)
        {
            _eventLogService.LogException(nameof(RedirectBulkImport), "BUILDURLLOOKUP", ex,
                additionalMessage: $"Failed to build the URL lookup for channel '{channel.ChannelName}'.");

            return response.AddErrorMessage(
                "The import could not resolve page URLs for the selected site. Check the Event Log for details.");
        }

        // Load every existing source URL for this channel ONCE, up front, instead of
        // issuing a separate DB query per CSV row inside the loop below. This is the
        var existingSourceUrls = new HashSet<string>(
            _redirectInfoProvider.Get()
                .WhereEquals(nameof(RedirectInfo.RedirectChannelID), websiteChannel.WebsiteChannelChannelID)
                .Column(nameof(RedirectInfo.RedirectSourceUrl))
                .GetListResult<string>(),
            StringComparer.OrdinalIgnoreCase);

        int imported = 0, duplicates = 0, notFound = 0, malformed = 0;

        try
        {
            // Wraps every insert in a single transaction so that an
            // unexpected failure partway through rolls back the whole
            // batch instead of leaving a half-imported set of redirects.
            using var transaction = new CMSTransactionScope();

            foreach (var row in rows)
            {
                var columns = row.Split(delimiter);

                if (columns.Length < 2)
                {
                    malformed++;
                    continue;
                }

                string sourceUrl = columns[0].Trim().Trim('"').ToLowerInvariant();
                string targetUrl = NormalizeUrl(columns[1].Trim().Trim('"'));

                if (string.IsNullOrWhiteSpace(sourceUrl) || string.IsNullOrWhiteSpace(targetUrl))
                {
                    malformed++;
                    continue;
                }

                // In-memory check against the pre-loaded set. Also catches duplicate
                // source URLs that appear more than once within the same CSV, since
                // we add each newly-imported URL back into the set below.
                if (existingSourceUrls.Contains(sourceUrl))
                {
                    duplicates++;
                    continue;
                }

                if (!urlLookup.TryGetValue(targetUrl, out var targetPage))
                {
                    notFound++;
                    continue;
                }

                var redirect = new RedirectInfo
                {
                    RedirectSourceUrl = sourceUrl,
                    RedirectTargetWebPageItemGUID = targetPage.Guid,
                    RedirectChannelID = websiteChannel.WebsiteChannelChannelID,
                    RedirectChannelName = channel.ChannelDisplayName,
                    RedirectTargetPageURL = targetPage.Url
                };

                _redirectInfoProvider.Set(redirect);
                existingSourceUrls.Add(sourceUrl);
                imported++;
            }

            transaction.Commit();
        }
        catch (Exception ex)
        {
            _eventLogService.LogException(nameof(RedirectBulkImport), "IMPORT", ex,
                additionalMessage: $"Bulk redirect import failed for channel '{channel.ChannelName}' after {imported} row(s). Transaction rolled back.");

            return response.AddErrorMessage(
                "The import failed partway through and no redirects were saved. Check the Event Log for details.");
        }

        var summary = $"Imported {imported} redirect(s).";
        if (duplicates > 0) summary += $" Skipped {duplicates} duplicate(s).";
        if (notFound > 0) summary += $" {notFound} row(s) had a target URL that could not be matched to a page.";
        if (malformed > 0) summary += $" {malformed} row(s) were malformed.";

        return (duplicates > 0 || notFound > 0 || malformed > 0)
            ? response.AddWarningMessage(summary)
            : response.AddSuccessMessage(summary);
    }

    /// <summary>
    /// Resolves the current system URL of every page in the given channel and
    /// returns a dictionary keyed by normalized URL, for fast reverse lookup.
    /// </summary>
    private async Task<Dictionary<string, (Guid Guid, string Url)>> BuildChannelUrlLookupAsync(
        string websiteChannelName, string languageName)
    {
        var lookup = new Dictionary<string, (Guid Guid, string Url)>(StringComparer.OrdinalIgnoreCase);

        int offset = 0;

        while (true)
        {
            var pageQuery = new ContentItemQueryBuilder()
                .ForContentTypes(parameters =>
                    parameters.ForWebsite(websiteChannelName, includeUrlPath: false))
                .Parameters(parameters => parameters
                    .Columns(nameof(WebPageItemInfo.WebPageItemID), nameof(WebPageItemInfo.WebPageItemGUID))
                    .OrderBy(nameof(WebPageItemInfo.WebPageItemID))
                    .Offset(offset, PageSize));

            var pages = (await _executor.GetWebPageResult(
                pageQuery,
                rowData => new PageUrlLookupRow(rowData.WebPageItemID, rowData.WebPageItemGUID)))
                .ToList();

            if (pages.Count == 0)
            {
                break;
            }

            foreach (var page in pages)
            {
                try
                {
                    var urlResult = await _webPageUrlRetriever.Retrieve(page.WebPageItemID, languageName);
                    string relativeUrl = urlResult.RelativePath?.Replace("~", "") ?? string.Empty;

                    if (string.IsNullOrEmpty(relativeUrl))
                    {
                        continue;
                    }

                    string normalizedUrl = NormalizeUrl(relativeUrl);

                    if (!lookup.TryAdd(normalizedUrl, (page.WebPageItemGUID, relativeUrl)))
                    {
                        // Two pages resolved to the same URL - flag it rather than
                        // silently keeping the first and risking a wrong redirect target.
                        _eventLogService.LogWarning(nameof(RedirectBulkImport), "URLCOLLISION",
                            $"Multiple pages in channel '{websiteChannelName}' resolve to the URL '{relativeUrl}'. Page {page.WebPageItemGUID} was ignored in favor of an earlier match.");
                    }
                }
                catch (Exception ex)
                {
                    _eventLogService.LogException(nameof(RedirectBulkImport), "RESOLVEURL", ex,
                        additionalMessage: $"Could not resolve the URL of page {page.WebPageItemGUID} in channel '{websiteChannelName}'.");
                }
            }

            if (pages.Count < PageSize)
            {
                break;
            }

            offset += PageSize;
        }

        return lookup;
    }

    private static string NormalizeUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return string.Empty;
        }

        string normalized = url.Trim().Replace("~", "");

        if (!normalized.StartsWith("/"))
        {
            normalized = "/" + normalized;
        }

        if (normalized.Length > 1 && normalized.EndsWith("/"))
        {
            normalized = normalized.TrimEnd('/');
        }

        return normalized.ToLowerInvariant();
    }

    private static string GetDefaultLanguageName()
    {
        return ContentLanguageInfo.Provider.Get()
            .TopN(1)
            .Column(nameof(ContentLanguageInfo.ContentLanguageName))
            .WhereTrue(nameof(ContentLanguageInfo.ContentLanguageIsDefault))
            .GetScalarResult<string>();
    }
}