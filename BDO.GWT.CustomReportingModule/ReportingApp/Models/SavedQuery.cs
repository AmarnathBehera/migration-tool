namespace BDO.GWT.CustomReportingModule.ReportingApp
{
    /// <summary>
    /// Model which maps info from <see cref="SqlBrowserSavedQueryInfo"/> to a client-side object.
    /// </summary>
    public class SavedQuery()
    {
        public int ID { get; set; }


        public string? Name { get; set; }


        public string? Text { get; set; }


        public int Order { get; set; }


        public SavedQuery(ReportingReportInfo source) : this()
        {
            ID = source.ReportingReportID;
            Name = source.ReportingReportDisplayName;
            Text = source.ReportingReportQuery;
            //Order = source.Repor;
        }
    }
}
