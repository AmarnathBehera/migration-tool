using Kentico.Xperience.Admin.Base;

namespace BDO.GWT.CustomReportingModule.ReportingApp
{
    public class EditSqlTemplateClientProperties : TemplateClientProperties
    {
        /// <summary>
        /// The list of tables and columns present in the database.
        /// </summary>
        public IEnumerable<DatabaseTable> Tables { get; set; } = [];


        /// <summary>
        /// The query to pre-fill in the editor text area.
        /// </summary>
        public string? Query { get; set; }


        /// <summary>
        /// The saved SQL queries in the database.
        /// </summary>
        public IEnumerable<SavedQuery> SavedQueries { get; set; } = [];
        public int ReportingChannelSettingId { get; set; }

    }
}
