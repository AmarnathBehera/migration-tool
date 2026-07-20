using BDO.GWT.Domain.Core.Constants;
using BDO.GWT.GLOBAL.API.Bases;
using BDO.GWT.GLOBAL.API.Data;
using BDO.GWT.GLOBAL.API.Filters;
using BDO.GWT.GLOBAL.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.Net.Mime;

namespace BDO.GWT.GLOBAL.API.Controllers
{
    /// <summary>
    /// Provides methods to read and manage careers.
    /// </summary>
    [AllowAnonymous]
    [ApiController]
    [Route("memberFirms/{memberFirmID:int}/Careers")]
    public class CareerController : EntityControllerBase
    {
        private readonly ICareerCrudService _careerCrudService;

        public CareerController(ICareerCrudService careerCrudService)
        {
            _careerCrudService = careerCrudService;
        }

        /// <summary>
        /// Returns a single career having the specified page GUID.
        /// </summary>
        /// <param name="memberFirmID">ID of the member firm from which the page should be retrieved.</param>
        /// <param name="pageGUID">Page GUID of a career page.</param>
        /// <param name="culture">Language tag in the form of RFC 5646 (e.g. "en-US").</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Returns one career page.</returns>
        [HttpGet("{pageGUID:guid}")]
        //[HasPermission(ApiMethodGroupName.Careers, ApiPermission.Read)]
        [SwaggerResponse(StatusCodes.Status200OK, null, typeof(PageOutput<CareerPageContent>), MediaTypeNames.Application.Json)]
        public async Task<ActionResult<PageOutput<CareerPageContent>>> Get(int memberFirmID, Guid pageGUID, [FromQuery][Required] string culture, CancellationToken cancellationToken)
        {
            int channelId = Convert.ToInt32(HttpContext.Items["ChannelId"]);
            var page = await _careerCrudService.GetCareerPageAsync(channelId, pageGUID, culture, cancellationToken);

            return Ok(page);
        }
    }
}
