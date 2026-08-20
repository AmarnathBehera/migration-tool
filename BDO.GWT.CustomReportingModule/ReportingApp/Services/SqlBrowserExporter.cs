using Microsoft.AspNetCore.Hosting;
using System.Text;
using System.Text.Json;

namespace BDO.GWT.CustomReportingModule.ReportingApp
{
    public class SqlBrowserExporter : ISqlBrowserExporter
    {
        private readonly IWebHostEnvironment env;
        private readonly ISqlBrowserResultProvider resultProvider;

        public SqlBrowserExporter(
            IWebHostEnvironment env,
            ISqlBrowserResultProvider resultProvider)
        {
            this.env = env;
            this.resultProvider = resultProvider;
        }

        public async Task<(byte[] Content, string FileName, string ContentType)> Export(SqlBrowserExportType exportType, string? fileName = null)
        {
            string name = string.IsNullOrWhiteSpace(fileName)
                ? DateTime.UtcNow.ToString("yyyyMMdd_HHmmss")
                : Path.GetFileNameWithoutExtension(fileName);

            string ext = exportType switch
            {
                SqlBrowserExportType.Csv => ".csv",
                SqlBrowserExportType.Excel => ".xlsx",
                SqlBrowserExportType.Json => ".json",
                _ => ".dat"
            };

            var columns = resultProvider.GetColumnNames().ToArray();
            var rows = await resultProvider.GetRowsAsDynamic();

            if (columns.Length == 0)
            {
                return (Array.Empty<byte>(), string.Empty, "application/octet-stream");
            }

            // JSON
            if (exportType == SqlBrowserExportType.Json)
            {
                var list = rows.Select(ToDictionary).ToList();

                string json = JsonSerializer.Serialize(
                    list,
                    IndentedJsonOptions);

                return (
                    Encoding.UTF8.GetBytes(json),
                    $"{name}{ext}",
                    "application/json"
                );
            }

            // CSV
            var sb = new StringBuilder();

            sb.AppendLine(string.Join(",", columns));

            foreach (var row in rows)
            {
                var dictionary = ToDictionary(row);

                sb.AppendLine(string.Join(",",
                    columns.Select(column =>
                    {
                        dictionary.TryGetValue(column, out object? value);
                        return value?.ToString() ?? string.Empty;
                    })));
            }

            return (
                Encoding.UTF8.GetBytes(sb.ToString()),
                $"{name}{ext}",
                "text/csv"
            );
        }

        private static IDictionary<string, object?> ToDictionary(object row)
        {
            if (row is IDictionary<string, object> expandoDictionary)
            {
                return expandoDictionary.ToDictionary(
                    x => x.Key,
                    x => (object?)x.Value);
            }

            return row.GetType()
                .GetProperties()
                .ToDictionary(
                    property => property.Name,
                    property => property.GetValue(row));
        }

        public string GetExportDirectory()
        {
            return Path.Combine(
                env.ContentRootPath,
                "App_Data",
                "Export");
        }

        private static readonly JsonSerializerOptions IndentedJsonOptions = new()
        {
            WriteIndented = true
        };
    }
}
