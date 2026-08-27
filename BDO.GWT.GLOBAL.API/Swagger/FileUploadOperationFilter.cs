using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace BDO.GWT.GLOBAL.API.Swagger
{
    /// <summary>
    /// Swagger operation filter that improves documentation for endpoints with file upload parameters (IFormFile).
    /// </summary>
    public class FileUploadOperationFilter : IOperationFilter
    {
        /// <summary>
        /// Applies the filter to an operation that has IFormFile parameters.
        /// </summary>
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            ArgumentNullException.ThrowIfNull(operation);
            ArgumentNullException.ThrowIfNull(context);

            var fileUploadMime = "multipart/form-data";

            // Check if the operation has [Consumes("multipart/form-data")] attribute
            if (!operation.RequestBody?.Content?.ContainsKey(fileUploadMime) ?? true)
            {
                return;
            }

            var uploadFileMediaType = operation.RequestBody.Content[fileUploadMime];

            // Get all IFormFile parameters
            var formFileParams = context.MethodInfo
                .GetParameters()
                .Where(p => p.ParameterType == typeof(IFormFile))
                .ToList();

            if (!formFileParams.Any())
            {
                return;
            }

            // Ensure schema exists
            if (uploadFileMediaType.Schema?.Properties == null)
            {
                uploadFileMediaType.Schema ??= new OpenApiSchema();
                uploadFileMediaType.Schema.Properties ??= new Dictionary<string, OpenApiSchema>();
            }

            // Add or update file parameters in the schema
            foreach (var fileParam in formFileParams)
            {
                var attributeName = fileParam.Name ?? "file";

                if (uploadFileMediaType.Schema.Properties.ContainsKey(attributeName))
                {
                    uploadFileMediaType.Schema.Properties[attributeName] = new OpenApiSchema
                    {
                        Type = "string",
                        Format = "binary",
                        Description = $"Upload {attributeName}"
                    };
                }
                else
                {
                    uploadFileMediaType.Schema.Properties.Add(attributeName, new OpenApiSchema
                    {
                        Type = "string",
                        Format = "binary",
                        Description = $"Upload {attributeName}"
                    });
                }
            }

            // Ensure required is set
            if (uploadFileMediaType.Schema.Required == null)
            {
                uploadFileMediaType.Schema.Required = new HashSet<string>();
            }

            // Mark file parameters as required if they have [Required] attribute
            foreach (var fileParam in formFileParams)
            {
                var hasRequiredAttribute = fileParam.GetCustomAttribute<RequiredAttribute>() != null;
                var paramName = fileParam.Name ?? "file";

                if (hasRequiredAttribute && !uploadFileMediaType.Schema.Required.Contains(paramName))
                {
                    uploadFileMediaType.Schema.Required.Add(paramName);
                }
            }
        }
    }
}
