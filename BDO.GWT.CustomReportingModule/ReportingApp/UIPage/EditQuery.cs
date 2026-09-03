using CMS.Core;
using CMS.DataEngine;
using CMS.Membership;
using Kentico.Xperience.Admin.Base;
using System.Text.RegularExpressions;

namespace BDO.GWT.CustomReportingModule.ReportingApp
{
    [UINavigation(true)]
    [UIEvaluatePermission(SystemPermissions.VIEW)]
    public partial class EditQuery(
      IEventLogService eventLogService,
      ISqlBrowserResultProvider sqlBrowserResultProvider,
      ISqlBrowserExporter sqlBrowserExporter,
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
            if (!SqlQueryValidator.Validate(query, out var errorMessage))
            {
                var errorResult = new SqlBrowserQueryResult
                {
                    ErrorMessage = errorMessage ?? "Invalid SQL statement."
                };

                return Task.FromResult(
                    ResponseFrom(errorResult)
                        .AddErrorMessage(errorResult.ErrorMessage));
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

        private static void SkipWhitespace(string sql, ref int index)
        {
            while (index < sql.Length && char.IsWhiteSpace(sql[index]))
            {
                index++;
            }
        }

        private static bool SkipLineComment(string sql, ref int index)
        {
            if (index + 1 >= sql.Length ||
                sql[index] != '-' ||
                sql[index + 1] != '-')
            {
                return false;
            }

            index += 2;

            while (index < sql.Length && sql[index] != '\n')
            {
                index++;
            }

            return true;
        }

        private static bool SkipBlockComment(string sql, ref int index)
        {
            if (index + 1 >= sql.Length ||
                sql[index] != '/' ||
                sql[index + 1] != '*')
            {
                return false;
            }

            index += 2;

            while (index + 1 < sql.Length &&
                   !(sql[index] == '*' && sql[index + 1] == '/'))
            {
                index++;
            }

            if (index + 1 < sql.Length)
            {
                index += 2;
            }

            return true;
        }

        private static string? ReadToken(string sql, ref int index)
        {
            int start = index;

            while (index < sql.Length &&
                   (char.IsLetter(sql[index]) || sql[index] == '_'))
            {
                index++;
            }

            return start == index
                ? null
                : sql.Substring(start, index - start);
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

            var savedQuery = GetSavedQuery(query.ID);

            if (savedQuery is null)
            {
                return QueryNotFoundResponse(query.ID);
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
        public Task<ICommandResponse<SavedQuery?>> UpdateQuery(SavedQuery query)
        {
            if (query.ID <= 0 || string.IsNullOrWhiteSpace(query.Text))
            {
                return Task.FromResult(
                    ResponseFrom<SavedQuery?>(null)
                        .AddErrorMessage("Invalid query ID or empty query text"));
            }

            var savedQuery = GetSavedQuery(query.ID);

            if (savedQuery is null)
            {
                return QueryNotFoundResponse(query.ID);
            }

            savedQuery.ReportingReportQuery = query.Text;
            if (!string.IsNullOrWhiteSpace(query.Name))
            {
                savedQuery.ReportingReportDisplayName = query.Name;
                savedQuery.ReportingReportCodeName = GenerateQueryCodeName(query.Name);
            }
            savedQuery.Update();

            query.Name = savedQuery.ReportingReportDisplayName;
            query.Text = savedQuery.ReportingReportQuery;

            return Task.FromResult(
                ResponseFrom<SavedQuery?>(query)
                    .AddSuccessMessage("Query updated!"));
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

            if (existingQueries.Count > 0)
            {
                newOrder = existingQueries.Count;
            }

            try
            {
                var newQuery = CreateReportInfo(query);

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
        private ReportingReportInfo CreateReportInfo(SavedQuery query)
        {
            return new ReportingReportInfo
            {
                ReportingReportDisplayName = query.Name,
                ReportingReportCodeName = GenerateQueryCodeName(query.Name),
                ReportingReportChannelSettingsID = ReportingChannelSettingId,
                ReportingReportQuery = query.Text,
                ReportingReportGUID = Guid.NewGuid()
            };
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

        private ReportingReportInfo? GetSavedQuery(int id) =>
            savedQueryProvider.Get()
                .TopN(1)
                .WhereEquals(nameof(ReportingReportInfo.ReportingReportID), id)
                .WhereEquals(nameof(ReportingReportInfo.ReportingReportChannelSettingsID), ReportingChannelSettingId)
                .FirstOrDefault();

        private Task<ICommandResponse<SavedQuery?>> QueryNotFoundResponse(int id) =>
            Task.FromResult(
                ResponseFrom<SavedQuery?>(null)
                    .AddErrorMessage($"Query {id} not found"));

        [GeneratedRegex("[^a-zA-Z0-9_.-]")]
        private static partial Regex CodeNameSanitizer();

        private static string GenerateQueryCodeName(string? displayName)
        {
            if (string.IsNullOrWhiteSpace(displayName))
            {
                displayName = "Query";
            }
            string codeName = CodeNameSanitizer().Replace(displayName, "_");
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

                var extension = exportType switch
                {
                    SqlBrowserExportType.Excel => "xlsx",
                    SqlBrowserExportType.Json => "json",
                    _ => "csv"
                };

                var requestedFileName = string.IsNullOrWhiteSpace(model?.FileName)
                    ? $"export-{DateTime.Now:yyyy-MM-dd-HH-mm-ss}.{extension}"
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
