using BDO.GWT.Core.DTOs.Footer;
using BDO.GWT.Core.Services.Footer;
using BDO.GWT.Core.ValueObjects;
using BDO.GWT.Domain.Core.Constants;
using BDO.GWT.GLOBAL.API.Filters;
using BDO.GWT.GLOBAL.API.Helpers;
using BDO.GWT.Web.Core.DTOs;
using BDO.GWT.Web.Core.Services;
using CMS.ContentEngine;
using CMS.DataEngine;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Globalization;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.GLOBAL.API.Controllers
{
    /// <summary>
    /// Provides methods to read footer data.
    /// </summary>
    //Footer API Controller, this will retun the footer content for the website based on the current culture and channel context.
    //The footer content will be retrieved from the CMS and will be returned in json format to be consumed by caller.
    //[Authorize]
    [AllowAnonymous]
    [ApiController]
    [Route("/memberFirms/{memberFirmID:int}/{culture}/[controller]")]

    public class FooterController : Controller
    {
        private readonly IFooterService _footerService;
        private readonly ICultureValidationService _cultureValidationService;
        private readonly IInfoProvider<ChannelInfo> _channelInfoProvider;
        private static readonly LanguageTag defaultCulture = LanguageTag.GB;
        /// <summary>
        ///constructor for FooterController.
        /// </summary>
        public FooterController(IFooterService footerService, ICultureValidationService cultureValidationService, IInfoProvider<ChannelInfo> channelInfoProvider)
        {
            _footerService = footerService;
            _cultureValidationService = cultureValidationService;
            _channelInfoProvider = channelInfoProvider;
        }
        /// <summary>
        /// Retrieves the footer data.
        /// </summary>

        [HttpGet]
        [HasPermission(ApiMethodGroupName.Footer, ApiPermission.Read)]
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

            var channel = await _channelInfoProvider.GetAsync(channelId);

            if (channel == null)
            {
                errors["ChannelId"] = $"Path parameter 'ChannelId' value '{channelId}' is not valid.";
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                await HttpContext.Response.WriteAsJsonAsync(errors, cancellationToken: cancellationToken);

                return new FooterDto();
            }

            var footer = await _footerService.GetAsyncForAPI(channelId, culture, cancellationToken);

            return Ok(footer);
        }
    }
}
