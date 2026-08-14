using CMS.Base;
using CMS.Core;
using CMS.Helpers;
using CMS.Membership;
using Kentico.Xperience.Admin.Base;

namespace BDO.GWT.CustomReportingModule.ReportingApp
{
    /// <summary>
    /// Listing UI page which displays the results of a SQL query.
    /// </summary>
    [UINavigation(true)]
    [UIEvaluatePermission(SystemPermissions.VIEW)]

    public class ResultListing(
    ISqlBrowserResultProvider sqlBrowserQueryProvider,
    ISqlBrowserExporter sqlBrowserExporter,
    IEventLogService eventLogService,
    IPageLinkGenerator pageLinkGenerator) : DataContainerListingPage
    {
        [PageParameter(typeof(IntPageModelBinder))]
        public int ReportingChannelSettingId { get; set; }
        public override async Task ConfigurePage()
        {
            // If no query is set, expose a header action to let the user enter one and show the "No results" callout.
            // Open the full editor so the user can enter SQL in a dedicated window.
            // Use OpenEditQuery which navigates to the EditQuery page (provides editor, execute, save/delete).
            PageConfiguration.HeaderActions.AddCommand(
                "Enter query",
                nameof(OpenEditQuery),
                "Open SQL editor to compose and run a query for the current channel");

            // Provide a quick navigation back to the channel list instead of opening the channel selector
            PageConfiguration.HeaderActions.AddCommandWithConfirmation(
                "Back to channel",
                nameof(BackToChannel),
                "Return to channel list",
                "Back");

            PageConfiguration.Callouts = [
       new CalloutConfiguration
                {
                    Headline = "No results",
                    Content = "Query result has no data, please check the Event log for errors or modify your query",
                    Placement = CalloutPlacement.OnPaper,
                    Type = CalloutType.FriendlyWarning
                }];

            await base.ConfigurePage();
            return;
        }


        [PageCommand(Permission = ReportingReportListingPage.EXPORT_PERMISSION)]
        public async Task<ICommandResponse> Export(ExportConfirmationDialogModel model)
        {
            var exportType = model.ExportType?.ToLower() switch
            {
                "csv" => SqlBrowserExportType.Csv,
                "excel" => SqlBrowserExportType.Excel,
                "json" => SqlBrowserExportType.Json,
                _ => SqlBrowserExportType.None
            };
            if (exportType == SqlBrowserExportType.None)
            {
                return Response().AddSuccessMessage($"Invalid export type.");
            }

            try
            {
                var (content, returnedFileName, contentType) = await sqlBrowserExporter.Export(exportType, model.FileName);

                if (content == null || content.Length == 0)
                {
                    return Response().AddErrorMessage("Export failed, empty content returned.");
                }

                var exportDir = sqlBrowserExporter.GetExportDirectory() ?? Path.GetTempPath();
                Directory.CreateDirectory(exportDir);

                var finalFileName = string.IsNullOrWhiteSpace(returnedFileName)
                    ? (string.IsNullOrWhiteSpace(model.FileName) ? $"export-{DateTime.Now:yyyy-MM-dd-HH-mm-ss}.{(exportType == SqlBrowserExportType.Excel ? "xlsx" : exportType == SqlBrowserExportType.Json ? "json" : "csv")}" : model.FileName)
                    : returnedFileName;

                var finalPath = Path.Combine(exportDir, finalFileName);
                await File.WriteAllBytesAsync(finalPath, content);

                return Response().AddSuccessMessage($"Exported results to {finalPath}");
            }
            catch (Exception ex)
            {
                eventLogService.LogException(nameof(ResultListing), nameof(Export), ex);
                return Response().AddErrorMessage("Export failed, please check the Event log for errors");
            }
        }


        [PageCommand]
        public Task<ICommandResponse> BackToChannel()
        {
            var url = "/reporting-application/reporting-application-channel-settings";
            return Task.FromResult((ICommandResponse)NavigateTo(url));
        }

        [PageCommand]
        public Task<ICommandResponse> SubmitQuery(EditQueryModel model)
        {
            if (string.IsNullOrWhiteSpace(model?.QueryText))
            {
                return Task.FromResult(Response().AddErrorMessage("No query entered"));
            }

            // Set the query and refresh the listing so it will be executed.
            sqlBrowserQueryProvider.SetQuery(model.QueryText!);

            var url = pageLinkGenerator.GetPath<ResultListing>();
            var channelId = CMS.Helpers.QueryHelper.GetString("channelId", null);
            if (!string.IsNullOrEmpty(channelId))
            {
                url += (url.Contains("?") ? "&" : "?") + $"channelId={channelId}";
            }

            return Task.FromResult((ICommandResponse)NavigateTo(url));
        }

        [PageCommand]
        public Task<ICommandResponse> OpenEditQuery()
        {
            int? _channelId = ReportingChannelSettingId;
            var channelIdString = _channelId.ToString();
            string initialQuery = $"""
/*
No query has been configured.
Use ChannelID = {channelIdString} in your custom query.
*/
""";

            sqlBrowserQueryProvider.SetQuery(initialQuery);

            PageParameterValues? parameters = null;
            if (!string.IsNullOrEmpty(channelIdString) && int.TryParse(channelIdString, out var channelId))
            {
                parameters = new PageParameterValues
                {
                    { typeof(ReportingApplicationChannelSettingsEditSection), channelId }
                };
            }

            string url;
            if (parameters == null)
            {
                url = pageLinkGenerator.GetPath<EditQuery>();
            }
            else
            {
                url = pageLinkGenerator.GetPath<EditQuery>(parameters);
            }

            return Task.FromResult((ICommandResponse)NavigateTo(url));
        }


        protected override object GetIdentifier(IDataContainer dataContainer) =>
            ValidationHelper.GetInteger(dataContainer[SqlBrowserResultProvider.ROW_IDENTIFIER_COLUMN], -1);


        protected override Task<IEnumerable<IDataContainer>> LoadDataContainers(CancellationToken cancellationToken) =>
            sqlBrowserQueryProvider.GetRowsAsDataContainer();
    }

}
