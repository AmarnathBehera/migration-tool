using BDO.GWT.Core.Utilities;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;

namespace BDO.GWT.Core.API.Utilities;

public static class RequestQueryUtility
{
    public static IDictionary<string, string> ExtractFilterQueryParameters(IQueryCollection queryCollection)
    {
        var queryParameterValueByName = queryCollection.ToDictionary(x => x.Key, x => x.Value.ToString());

        return FilterUtility.GetFilterParameterValueByName(queryParameterValueByName);
    }
}
