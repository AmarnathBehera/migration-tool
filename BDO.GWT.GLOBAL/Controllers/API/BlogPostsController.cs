using BDO.GWT.Core.API.Utilities;
using BDO.GWT.Core.Data.Queries;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Cards;
using BDO.GWT.Core.LiveSite.Filters;
using BDO.GWT.Core.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Web.Controllers
{
    [ApiController]
    [Route($"api/{{{CultureInjectorAttribute.RouteParameter}}}/[controller]")]
    public class BlogPostsController : ControllerBase
    {
        private readonly IBlogsPageService _blogPageService;
        
        public BlogPostsController(IBlogsPageService blogPageService)
        {
            _blogPageService = blogPageService;
        }

        [HttpGet("{pageId:int}/[action]")]
        public async Task<ActionResult<PaginationDto<BlogPageCardDto>>> Get([FromRoute] int pageId, [FromQuery] CardsQuery query, CancellationToken cancellationToken)
        {
           var language = RouteData.Values[CultureInjectorAttribute.RouteParameter]?.ToString();

            query.Filters = RequestQueryUtility.ExtractFilterQueryParameters(Request.Query);
            // Call the service method to get the paginated blog posts based on the content Item GUID, language, and query parameters
            var result = await _blogPageService.GetBlogsPaginationResultAsync(pageId, language, query, cancellationToken);

            
            return Ok(result);
        }
    }
}
