using BDO.GWT.Domain.Core.Constants;
using BDO.GWT.GLOBAL.API.Filters;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net.Mime;

namespace BDO.GWT.GLOBAL.API.Controllers
{
    namespace BDO.GWT.GLOBAL.API.Controllers
    {
        /// <summary>
        /// Controller for weather forecast operations.
        /// </summary>
        [ApiController]
        [Route("[controller]")]
        public class WeatherForecastController : ControllerBase
        {
            /// <summary>
            /// Retrieves all weather forecast records.
            /// </summary>
            /// <returns>A successful response with all records.</returns>
            [HttpGet]
            [HasPermission(ApiMethodGroupName.Deals, ApiPermission.Read)]
            [SwaggerResponse(StatusCodes.Status200OK, null, typeof(string), MediaTypeNames.Application.Json)]
            public IActionResult Get()
            {
                return Ok("All records");
            }

            /// <summary>
            /// Retrieves a weather forecast record by its identifier.
            /// </summary>
            /// <param name="id">The identifier of the record to retrieve.</param>
            /// <returns>A successful response with the requested record.</returns>
            [HttpGet("{id}")]
            [HasPermission(ApiMethodGroupName.Deals, ApiPermission.Read)]
            [SwaggerResponse(StatusCodes.Status200OK, null, typeof(string), MediaTypeNames.Application.Json)]
            public IActionResult GetById(int id)
            {
                return Ok($"Record {id}");
            }
        }
    }
}
