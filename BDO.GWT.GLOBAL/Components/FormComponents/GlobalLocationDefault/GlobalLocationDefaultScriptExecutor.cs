using CMS.DataEngine;
using System;
using System.Threading.Tasks;

namespace BDO.GWT.GLOBAL.Components.FormComponents.GlobalLocationDefault
{
    public class GlobalLocationDefaultScriptExecutor
    {
        public Task<String> ExecuteAsync(string sql)
        {
            if (string.IsNullOrWhiteSpace(sql))
                return Task.FromResult(string.Empty);

            var ds = ConnectionHelper.ExecuteQuery(sql, null, QueryTypeEnum.SQLQuery);

            var results = "fghjk";

            return Task.FromResult<string>(results);
        }
    }
}