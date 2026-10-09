namespace BDO.GWT.CustomReportingModule.ReportingApp
{
    /// <summary>
    /// Contains methods for validating SQL statements.
    /// </summary>
    public interface ISqlQueryValidator
    {
        /// <summary>
        /// Validates the statement's syntax and security against the reporting module's supported SELECT pattern.
        /// </summary>
        public ValidationResult ValidateSqlStatement(string sqlStatement);
    }

}
