namespace BDO.GWT.CustomReportingModule.ReportingApp
{
    /// <summary>
    /// Client-side representation of a SQL Browser query execution result.
    /// </summary>
    public class SqlBrowserQueryResult
    {
        public string[] Columns { get; set; } = [];

        public string[][] Rows { get; set; } = [];

        public string? ErrorMessage { get; set; }

        public SavedQuery? AutoSavedQuery { get; set; }
    }
}
