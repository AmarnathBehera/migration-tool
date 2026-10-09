using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using System.Data;
using System.Dynamic;
using System.Text;

namespace BDO.GWT.CustomReportingModule.ReportingApp
{

    public class SqlBrowserResultProvider(IEventLogService eventLogService, ISqlQueryValidator validator) : ISqlBrowserResultProvider
    {
        private string? query;
        private DataSet? _result;
        private string? errorMessage;
        public const string ROW_IDENTIFIER_COLUMN = $"{nameof(SqlBrowserResultProvider)}_result_identifier";

        public IEnumerable<string> GetColumnNames()
        {
            EnsureResult();

            if (ResultsAreEmpty())
            {
                return Array.Empty<string>();
            }

            return _result!.Tables[0].Columns.OfType<DataColumn>().Select(c => c.ColumnName);
        }


        public Task<string> GetRowAsText(int rowIdentifier)
        {
            EnsureResult();

            if (ResultsAreEmpty())
            {
                throw new InvalidOperationException($"Failed to load row {rowIdentifier}");
            }

            var row = _result!.Tables[0].Rows[rowIdentifier] ?? throw new InvalidOperationException($"Failed to load row {rowIdentifier}");
            var textResult = new StringBuilder();
            foreach (string col in GetColumnNames())
            {
                textResult
                    .Append(col)
                    .Append(": ")
                    .Append(row[col])
                    .Append(Environment.NewLine);
            }

            return Task.FromResult(textResult.ToString());
        }


        public int GetTotalRecordCount()
        {
            EnsureResult();

            if (ResultsAreEmpty())
            {
                return 0;
            }

            return _result!.Tables[0].Rows.Count;
        }


        public Task<IEnumerable<IDataContainer>> GetRowsAsDataContainer()
        {
            EnsureResult();

            if (ResultsAreEmpty())
            {
                return Task.FromResult<IEnumerable<IDataContainer>>([]);
            }

            var columnNames = GetColumnNames();
            var containers = _result!.Tables[0].Rows.OfType<DataRow>().Select((row, i) =>
            {
                var data = new DataContainer
                {
                    [ROW_IDENTIFIER_COLUMN] = i,
                };
                foreach (string col in columnNames)
                {
                    data[col] = row[col];
                }

                return data;
            });

            return Task.FromResult<IEnumerable<IDataContainer>>(containers);
        }


        public Task<IEnumerable<dynamic>> GetRowsAsDynamic()
        {
            EnsureResult();

            if (ResultsAreEmpty())
            {
                return Task.FromResult<IEnumerable<dynamic>>([]);
            }

            var columnNames = GetColumnNames();
            var dynamics = _result!.Tables[0].Rows.OfType<DataRow>().Select(row =>
            {
                var obj = new ExpandoObject();
                foreach (string col in columnNames)
                {
                    (obj as IDictionary<string, object>).Add(col, row[col]);
                }

                return obj;
            });

            return Task.FromResult<IEnumerable<dynamic>>(dynamics);
        }


        public string? GetQuery() => query;


        public SqlBrowserQueryResult GetQueryResult()
        {
            EnsureResult();

            if (ResultsAreEmpty())
            {
                return new SqlBrowserQueryResult
                {
                    ErrorMessage = errorMessage
                };
            }

            var columnNames = GetColumnNames().ToArray();
            var rows = _result!.Tables[0]
                .Rows
                .OfType<DataRow>()
                .Select(row => columnNames.Select(column => row[column]?.ToString() ?? string.Empty).ToArray())
                .ToArray();

            return new SqlBrowserQueryResult
            {
                Columns = columnNames,
                Rows = rows,
                ErrorMessage = errorMessage
            };
        }


        public void SetQuery(string queryText)
        {
            _result = null;
            errorMessage = null;
            query = queryText;
        }


        private void EnsureResult()
        {
            if (_result is not null)
            {
                return;
            }

            if (string.IsNullOrEmpty(query))
            {
                _result = new DataSet();
                errorMessage = "No query entered.";
                eventLogService.LogWarning(nameof(SqlBrowserResultProvider), nameof(EnsureResult),
                    $"No query present in {nameof(SqlBrowserResultProvider)}");
                return;
            }

            try
            {
                var validationResult = validator.ValidateSqlStatement(query);
                if (validationResult is null)
                {
                    eventLogService.LogWarning(nameof(SqlBrowserResultProvider), nameof(EnsureResult),
                        "The SQL validator did not return a result. Executing the query without validator feedback.");
                }
                else if (!validationResult.IsValid)
                {
                    _result = new DataSet();
                    errorMessage = validationResult.ErrorMessage;
                    eventLogService.LogWarning(nameof(SqlBrowserResultProvider), nameof(EnsureResult),
                        $"User attempted to execute invalid query:{Environment.NewLine}{query}{Environment.NewLine}Error: {validationResult.ErrorMessage}");

                    return;
                }

                // Validate SQL syntax by attempting to parse/compile the query without executing it
                var syntaxError = ValidateSqlSyntax(query);
                if (!string.IsNullOrEmpty(syntaxError))
                {
                    _result = new DataSet();
                    errorMessage = syntaxError;
                    eventLogService.LogWarning(nameof(SqlBrowserResultProvider), nameof(EnsureResult),
                        $"SQL syntax validation failed:{Environment.NewLine}{query}{Environment.NewLine}Error: {syntaxError}");
                    return;
                }

                _result = ConnectionHelper.ExecuteQuery(query, null, QueryTypeEnum.SQLQuery);
                eventLogService.LogInformation(nameof(SqlBrowserResultProvider), nameof(EnsureResult),
                    $"User executed query:{Environment.NewLine}{query}");
            }
            catch (Exception ex)
            {
                _result = new DataSet();
                errorMessage = ex.Message;
                eventLogService.LogException(nameof(SqlBrowserResultProvider), nameof(EnsureResult), ex);
            }
        }

        /// <summary>
        /// Validates SQL syntax by using SET NOEXEC ON to compile without executing.
        /// Returns null if syntax is valid, or the error message if invalid.
        /// </summary>
        private static string? ValidateSqlSyntax(string sqlQuery)
        {
            try
            {
                // Use SET NOEXEC ON to validate syntax without executing the query
                // This compiles the query and catches syntax errors without returning data
                var validationSql = $@"
SET NOEXEC ON;
{sqlQuery}
SET NOEXEC OFF;";

                ConnectionHelper.ExecuteQuery(validationSql, null, QueryTypeEnum.SQLQuery);
                return null; // Syntax is valid
            }
            catch (Exception ex)
            {
                // Extract meaningful error message from the exception
                return $"SQL syntax error: {ex.Message}";
            }
        }


        private bool ResultsAreEmpty() => _result is null || _result.Tables.Count == 0 || _result.Tables[0].Columns.Count == 0;
    }

}
