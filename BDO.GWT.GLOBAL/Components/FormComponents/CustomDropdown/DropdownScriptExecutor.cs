using BDO.GWT.GLOBAL.Components.FormComponents.CustomDropdown;
using CMS.DataEngine;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace BDO.GWT.GLOBAL.Components.FormComponents.CustomDropdown
{
    public class DropdownScriptExecutor
    {
        public Task<IEnumerable<CustomDropdownOption>> ExecuteAsync(string sql)
        {
            if (string.IsNullOrWhiteSpace(sql))
                return Task.FromResult<IEnumerable<CustomDropdownOption>>([]);

            var ds = ConnectionHelper.ExecuteQuery(sql, null, QueryTypeEnum.SQLQuery);

            var results = new List<CustomDropdownOption>();

            if (ds?.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                var table = ds.Tables[0];
                var columns = table.Columns;
                bool hasValueCol = columns.Contains("Value");
                bool hasTextCol = columns.Contains("Text");

                foreach (DataRow row in table.Rows)
                {
                    string value;
                    string text;

                    if (hasValueCol && hasTextCol)
                    {
                        value = row["Value"]?.ToString() ?? "";
                        text = row["Text"]?.ToString() ?? "";
                    }
                    else if (columns.Count >= 2)
                    {
                        value = row[0]?.ToString() ?? "";
                        text = row[1]?.ToString() ?? "";
                    }
                    else
                    {
                        value = row[0]?.ToString() ?? "";
                        text = value;
                    }

                    results.Add(new CustomDropdownOption
                    {
                        Value = value,
                        Text = text
                    });
                }
            }

            return Task.FromResult<IEnumerable<CustomDropdownOption>>(results);
        }
    }
}