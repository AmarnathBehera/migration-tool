using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Membership;
using Kentico.Xperience.Admin.Base;
using System.Text.RegularExpressions;

namespace BDO.GWT.CustomReportingModule.ReportingApp
{
    [UINavigation(true)]
    [UIEvaluatePermission(SystemPermissions.VIEW)]
    public class EditQuery(
      IEventLogService eventLogService,
      IProgressiveCache cache,
      ISqlBrowserResultProvider sqlBrowserResultProvider,
      ISqlBrowserExporter sqlBrowserExporter,
      IPageLinkGenerator pageLinkGenerator,
      IInfoProvider<ReportingReportInfo> savedQueryProvider)
      : Page<EditSqlTemplateClientProperties>
    {
        [PageParameter(typeof(IntPageModelBinder))]
        public int ReportingChannelSettingId { get; set; }

        public override async Task<EditSqlTemplateClientProperties> ConfigureTemplateProperties(EditSqlTemplateClientProperties properties)
        {
            // Always show the default starter query on page load instead of previously executed query
            properties.Query = $"""
                                /*
                                No query has been configured.
                                Use ChannelID = {ReportingChannelSettingId} in your custom query.
                                */
                                """;
            properties.SavedQueries = (await GetSavedQueries())
                .Select(q => new SavedQuery(q));

            properties.ReportingChannelSettingId = ReportingChannelSettingId;
            return properties;
        }

        [PageCommand]
        public Task<ICommandResponse> Notify(string message) =>
            Task.FromResult(Response().AddSuccessMessage(message));

        [PageCommand]
        public Task<ICommandResponse<SqlBrowserQueryResult>> RunSql(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return Task.FromResult(
                    ResponseFrom(new SqlBrowserQueryResult
                    {
                        ErrorMessage = "No query entered."
                    })
                        .AddErrorMessage("No query entered."));
            }

            // Only allow SELECT queries (skip leading comments/whitespace and inspect first token)
            static string? GetFirstTokenSkippingComments(string sql)
            {
                if (string.IsNullOrEmpty(sql))
                {
                    return null;
                }

                int i = 0;
                int len = sql.Length;
                while (i < len)
                {
                    // skip whitespace
                    while (i < len && char.IsWhiteSpace(sql[i])) i++;
                    if (i >= len) break;

                    // skip line comment -- until end of line
                    if (i + 1 < len && sql[i] == '-' && sql[i + 1] == '-')
                    {
                        i += 2;
                        while (i < len && sql[i] != '\n') i++;
                        continue;
                    }

                    // skip block comment /* ... */
                    if (i + 1 < len && sql[i] == '/' && sql[i + 1] == '*')
                    {
                        i += 2;
                        while (i + 1 < len && !(sql[i] == '*' && sql[i + 1] == '/')) i++;
                        if (i + 1 < len) i += 2;
                        continue;
                    }

                    // next token starts here
                    int start = i;
                    while (i < len && (char.IsLetter(sql[i]) || sql[i] == '_')) i++;
                    if (start == i) // no letter token
                    {
                        // consume a non-letter char and continue
                        i++;
                        continue;
                    }

                    return sql.Substring(start, i - start);
                }

                return null;
            }

            var firstToken = GetFirstTokenSkippingComments(query)?.ToUpperInvariant();
            if (firstToken is null || !(firstToken == "SELECT" || firstToken == "WITH"))
            {
                // Return an empty result with an error message
                var errorResult = new SqlBrowserQueryResult { ErrorMessage = "Only SELECT queries (optionally with CTEs) are allowed." };
                return Task.FromResult(ResponseFrom(errorResult).AddErrorMessage("Only SELECT queries (optionally with CTEs) are allowed."));
            }

            sqlBrowserResultProvider.SetQuery(query);
            var result = sqlBrowserResultProvider.GetQueryResult();

            var response = ResponseFrom(result);
            if (!string.IsNullOrWhiteSpace(result.ErrorMessage))
            {
                return Task.FromResult(response.AddErrorMessage(result.ErrorMessage));
            }

            return Task.FromResult(response.AddSuccessMessage("Query executed."));
        }

        [PageCommand]
        public Task<ICommandResponse<SavedQuery?>> RenameQuery(SavedQuery query)
        {
            if (query.ID <= 0 || string.IsNullOrWhiteSpace(query.Name))
            {
                return Task.FromResult(
                    ResponseFrom<SavedQuery?>(null)
                        .AddErrorMessage("Received empty parameter"));
            }

            var savedQuery = savedQueryProvider.Get()
                .TopN(1)
                .WhereEquals(nameof(ReportingReportInfo.ReportingReportID), query.ID)
                .WhereEquals(nameof(ReportingReportInfo.ReportingReportChannelSettingsID), ReportingChannelSettingId)
                .FirstOrDefault();

            if (savedQuery is null)
            {
                return Task.FromResult(
                    ResponseFrom<SavedQuery?>(null)
                        .AddErrorMessage($"Query {query.ID} not found"));
            }

            savedQuery.ReportingReportDisplayName = query.Name;
            savedQuery.ReportingReportCodeName = GenerateQueryCodeName(query.Name);
            savedQuery.Update();

            query.Text = savedQuery.ReportingReportQuery;

            return Task.FromResult(
                ResponseFrom<SavedQuery?>(query)
                    .AddSuccessMessage("Query renamed!"));
        }

        [PageCommand]
        public Task<ICommandResponse<int>> DeleteQuery(int id)
        {
            var query = savedQueryProvider.Get()
                .TopN(1)
                .WhereEquals(nameof(ReportingReportInfo.ReportingReportID), id)
                .WhereEquals(nameof(ReportingReportInfo.ReportingReportChannelSettingsID), ReportingChannelSettingId)
                .FirstOrDefault();

            if (query is null)
            {
                return Task.FromResult(
                    ResponseFrom(0)
                        .AddErrorMessage($"Query {id} not found"));
            }

            query.Delete();

            return Task.FromResult(
                ResponseFrom(id)
                    .AddSuccessMessage("Query deleted!"));
        }

        [PageCommand]
        public Task<ICommandResponse<SavedQuery?>> SaveQuery(SavedQuery query)
        {
            if (string.IsNullOrWhiteSpace(query.Name) ||
                string.IsNullOrWhiteSpace(query.Text))
            {
                return Task.FromResult(
                    ResponseFrom<SavedQuery?>(null)
                        .AddErrorMessage("Received empty parameter"));
            }

            int newOrder = 0;

            var existingQueries = savedQueryProvider.Get()
                .GetEnumerableTypedResult()
                .ToList();

            if (existingQueries.Any())
            {
                newOrder = existingQueries.Count;
            }

            try
            {
                var newQuery = new ReportingReportInfo
                {
                    ReportingReportDisplayName = query.Name,
                    ReportingReportCodeName = GenerateQueryCodeName(query.Name),
                    ReportingReportChannelSettingsID = ReportingChannelSettingId,
                    ReportingReportQuery = query.Text,
                    ReportingReportGUID = Guid.NewGuid()
                };

                newQuery.Insert();

                query.Order = newOrder;
                query.ID = newQuery.ReportingReportID;

                return Task.FromResult(
                    ResponseFrom<SavedQuery?>(query)
                        .AddSuccessMessage("Query saved!"));
            }
            catch (Exception ex)
            {
                eventLogService.LogException(
                    nameof(EditQuery),
                    nameof(SaveQuery),
                    ex);

                return Task.FromResult(
                    ResponseFrom<SavedQuery?>(null)
                        .AddErrorMessage(ex.Message));
            }
        }

        [PageCommand]
        public async Task<ICommandResponse<bool>> UpdateSavedOrder(
            SavedQuery[] newOrder)
        {
            var originalQueries = (await GetSavedQueries()).ToList();

            foreach (var newQuery in newOrder)
            {
                var original = originalQueries.Find(
                    q => q.ReportingReportID == newQuery.ID);

                if (original is null)
                {
                    return ResponseFrom(false)
                        .AddErrorMessage(
                            $"Failed to update order: query {newQuery.ID} not found");
                }

                original.Update();
            }

            return ResponseFrom(true);
        }

        private static string GenerateQueryCodeName(string displayName)
        {
            string codeName = Regex.Replace(displayName, "[^a-zA-Z0-9_.-]", "_");
            codeName = codeName.Trim('.');

            if (string.IsNullOrWhiteSpace(codeName))
            {
                codeName = "Query";
            }

            return $"{codeName}_{Guid.NewGuid():N}";
        }


        private Task<IEnumerable<ReportingReportInfo>> GetSavedQueries() =>
            savedQueryProvider.Get()
                .WhereEquals(nameof(ReportingReportInfo.ReportingReportChannelSettingsID), ReportingChannelSettingId)
                .GetEnumerableTypedResultAsync();

        public record ExportResult(string Base64, string FileName, string ContentType);

        [PageCommand]
        public async Task<ICommandResponse<ExportResult?>> ExportQuery(System.Text.Json.JsonElement? rawModel)
        {
            try
            {
                ExportConfirmationDialogModel? model = null;

                if (rawModel.HasValue && rawModel.Value.ValueKind != System.Text.Json.JsonValueKind.Null)
                {
                    model = System.Text.Json.JsonSerializer.Deserialize<ExportConfirmationDialogModel>(rawModel.Value.GetRawText());
                }

                var exportType = (model?.ExportType ?? "csv").ToLower() switch
                {
                    "csv" => SqlBrowserExportType.Csv,
                    "excel" => SqlBrowserExportType.Excel,
                    "json" => SqlBrowserExportType.Json,
                    _ => SqlBrowserExportType.Csv
                };

                var requestedFileName = string.IsNullOrWhiteSpace(model?.FileName)
                    ? $"export-{DateTime.Now:yyyy-MM-dd-HH-mm-ss}.{(exportType == SqlBrowserExportType.Excel ? "xlsx" : exportType == SqlBrowserExportType.Json ? "json" : "csv")}"
                    : model.FileName;

                var (content, returnedFileName, contentType) = await sqlBrowserExporter.Export(exportType, requestedFileName);

                if (content == null || content.Length == 0)
                {
                    return ResponseFrom<ExportResult?>(null).AddErrorMessage("Export failed, empty content returned.");
                }

                var finalFileName = string.IsNullOrWhiteSpace(returnedFileName) ? requestedFileName : returnedFileName;

                var base64 = Convert.ToBase64String(content);

                var exportResult = new ExportResult(base64, finalFileName, contentType);

                return ResponseFrom<ExportResult?>(exportResult).AddSuccessMessage("Export ready for download.");
            }
            catch (Exception ex)
            {
                eventLogService.LogException(nameof(EditQuery), nameof(ExportQuery), ex);
                return ResponseFrom<ExportResult?>(null).AddErrorMessage("Export failed, see event log.");
            }
        }
    }
}
