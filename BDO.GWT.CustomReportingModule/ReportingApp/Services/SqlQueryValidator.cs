using System.Text.RegularExpressions;

namespace BDO.GWT.CustomReportingModule.ReportingApp
{
    public class SqlQueryValidator : ISqlQueryValidator
    {
        private const string UnsupportedSelectPatternMessage = "Query must match the supported SELECT pattern.";

        private static readonly Regex SupportedSelectPattern = new(
            @"^\s*SELECT\s+(?:TOP\s*(?:\(\s*\d+\s*\)|\d+)\s+)?[\s\S]+?\s+FROM\s+(?:\[[^\]]+\]|[A-Za-z_][A-Za-z0-9_]*)(?:\s*\.\s*(?:\[[^\]]+\]|[A-Za-z_][A-Za-z0-9_]*)){0,2}(?:\s+WHERE\b[\s\S]*)?(?:\s+GROUP\s+BY\b[\s\S]*)?(?:\s+HAVING\b[\s\S]*)?(?:\s+ORDER\s+BY\b[\s\S]*)?(?:\s+OFFSET\b[\s\S]*)?\s*$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static bool Validate(string sql, out string? errorMessage)
        {
            errorMessage = null;
            if (string.IsNullOrWhiteSpace(sql))
            {
                errorMessage = "SQL is empty";
                return false;
            }

            var normalizedSql = RemoveComments(sql).Trim();
            var lowered = normalizedSql.ToLowerInvariant();
            if (lowered.StartsWith("drop ") || lowered.StartsWith("truncate ") || normalizedSql.Contains(';'))
            {
                errorMessage = "Destructive or multi-statement SQL is not allowed.";
                return false;
            }

            if (!SupportedSelectPattern.IsMatch(normalizedSql))
            {
                errorMessage = UnsupportedSelectPatternMessage;
                return false;
            }

            return true;
        }

        public ValidationResult ValidateSqlStatement(string sqlStatement)
        {
            if (SqlQueryValidator.Validate(sqlStatement, out var errorMessage))
            {
                return ValidationResult.Success;
            }

            return new ValidationResult(errorMessage ?? "Invalid SQL statement.");
        }

        private static string RemoveComments(string sql)
        {
            var withoutBlockComments = Regex.Replace(sql, @"/\*.*?\*/", " ", RegexOptions.Singleline);
            return Regex.Replace(withoutBlockComments, @"--.*?$", " ", RegexOptions.Multiline);
        }
    }
}
