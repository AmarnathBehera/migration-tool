namespace BDO.GWT.CustomReportingModule.ReportingApp
{
    public class SqlQueryValidator : ISqlQueryValidator
    {
        public bool Validate(string sql, out string? errorMessage)
        {
            errorMessage = null;
            if (string.IsNullOrWhiteSpace(sql))
            {
                errorMessage = "SQL is empty";
                return false;
            }

            var lowered = sql.Trim().ToLowerInvariant();
            if (lowered.StartsWith("drop ") || lowered.StartsWith("truncate ") || lowered.Contains(";"))
            {
                errorMessage = "Destructive or multi-statement SQL is not allowed.";
                return false;
            }

            return true;
        }

        public ValidationResult ValidateSqlStatement(string sqlStatement)
        {
            if (Validate(sqlStatement, out var errorMessage))
            {
                return ValidationResult.Success;
            }

            return new ValidationResult(errorMessage ?? "Invalid SQL statement.");
        }
    }
}
