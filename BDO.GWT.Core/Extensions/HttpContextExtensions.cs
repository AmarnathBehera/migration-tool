using BDO.GWT.Core.DTOs.SchemaMarkups;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using System.Text;

namespace BDO.GWT.Core.Extensions;

public static class HttpContextExtensions
{
    public const string LD_JSON_SCHEMA_KEY = "ComponentLdJsonSchemas";

    public static void AddLdJsonSchema(this HttpContext httpContext, BaseSchemaMarkupDto schema)
    {
        var schemas = GetLdJsonSchemas(httpContext);
        schemas.Add(schema.ToSchemaMarkupJsString());
        httpContext.Items[LD_JSON_SCHEMA_KEY] = schemas;
    }

    public static List<string> GetLdJsonSchemas(this HttpContext httpContext)
    {
        return httpContext.Items[LD_JSON_SCHEMA_KEY] as List<string> ?? [];
    }

    public static bool HasLdJsonSchemas(this HttpContext httpContext)
    {
        return httpContext.Items[LD_JSON_SCHEMA_KEY] is List<string> schemas && schemas.Count > 0;
    }

    public static HtmlString RenderLdJsonSchemas(this HttpContext httpContext)
    {
        var schemas = GetLdJsonSchemas(httpContext);

        if (!HasLdJsonSchemas(httpContext))
        {
            return HtmlString.Empty;
        }

        var sb = new StringBuilder();

        foreach (var schema in schemas)
        {
            sb.AppendLine(schema);
        }

        var htmlString = new HtmlString(sb.ToString());

        return htmlString;
    }
}
