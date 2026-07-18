using BDO.GWT.Core.DTOs.Footer;
using BDO.GWT.Core.Services.Footer;
using BDO.GWT.Web.Core.DTOs;
using BDO.GWT.Web.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using System.Globalization;
using BDO.GWT.Core.ValueObjects;
using BDO.GWT.GLOBAL.API.Helpers;

namespace BDO.GWT.GLOBAL.API.Controllers
{
    /// <summary>
    /// Provides methods to read footer data.
    /// </summary>
    //Footer API Controller, this will retun the footer content for the website based on the current culture and channel context.
    //The footer content will be retrieved from the CMS and will be returned in json format to be consumed by caller.
    //[Authorize]
    [ApiController]
    [Route("/memberFirms/{memberFirmID:int}/{culture}/[controller]")]

    public class FooterController : Controller
    {
        private readonly IFooterService _footerService;
        private readonly ICultureValidationService _cultureValidationService;
        private static readonly LanguageTag defaultCulture = LanguageTag.GB;
        /// <summary>
        ///constructor for FooterController.
        /// </summary>
        public FooterController(IFooterService footerService, ICultureValidationService cultureValidationService)
        {
            _footerService = footerService;
            _cultureValidationService = cultureValidationService;
        }
        /// <summary>
        /// Retrieves the footer data.
        /// </summary>

        [HttpGet]
        [SwaggerResponse(StatusCodes.Status200OK, null, typeof(FooterDto), MediaTypeNames.Application.Json)]        
        public async Task<ActionResult<FooterDto>> Get(CancellationToken cancellationToken)
        {
           

            var errors = new Dictionary<string, string>();

            if (!int.TryParse((string?)HttpContext.Request.RouteValues["memberFirmID"], out _))
            {
                errors["memberFirmID"] = "Path parameter 'memberFirmID' value must be an integer.";
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                await HttpContext.Response.WriteAsJsonAsync(errors, cancellationToken: cancellationToken);

                return new FooterDto();
            }

            var culture = (string?)HttpContext.Request.RouteValues["culture"];

            if (culture == null)
            { 
              culture = defaultCulture.ToString();
            }
            else if (!_cultureValidationService.IsValidCulture(culture))
            {
                errors["culture"] = $"Path parameter 'culture' value '{culture}' is not valid.";
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                await HttpContext.Response.WriteAsJsonAsync(errors, cancellationToken: cancellationToken);

                return new FooterDto();
            }

            var channelId = HttpContext.Items["ChannelId"] as int? ?? 1; // Default to 1

            var footer = await _footerService.GetAsyncForAPI(channelId, culture, cancellationToken);

            return Ok(footer);
        }
    }
}
