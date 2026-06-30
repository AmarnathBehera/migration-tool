using BDO.GWT.Domain.Core.Constants;
using BDO.GWT.GLOBAL.API.Filters;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net.Mime;

namespace BDO.GWT.GLOBAL.API.Controllers
{
    namespace BDO.GWT.GLOBAL.API.Controllers
    {
        [ApiController]
        [Route("[controller]")]
        public class WeatherForecastController : ControllerBase
        {
            [HttpGet]
            [HasPermission(ApiMethodGroupName.Deals, ApiPermission.Read)]
            [SwaggerResponse(StatusCodes.Status200OK, null, typeof(string), MediaTypeNames.Application.Json)]
            public IActionResult Get()
            {
                return Ok("All records");
            }

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
