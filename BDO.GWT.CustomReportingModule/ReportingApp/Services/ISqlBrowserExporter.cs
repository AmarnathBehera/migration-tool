namespace BDO.GWT.CustomReportingModule.ReportingApp
{
    public interface ISqlBrowserExporter
    {
        /// <summary>
        /// Exports the current SQL query results to the specified file type.
        /// Returns a tuple with the file content bytes, the filename (including extension) and the content type.
        /// </summary>
        /// <param name="exportType">The desired export file type.</param>
        /// <param name="fileName">Optional file name (may include extension). If null or empty, exporter will generate one.</param>
        Task<(byte[] Content, string FileName, string ContentType)> Export(
            SqlBrowserExportType exportType,
            string? fileName = null);

        /// <summary>
        /// Gets the full system path of the directory which stores exported SQL query results.
        /// </summary>
        string GetExportDirectory();
    }
}
