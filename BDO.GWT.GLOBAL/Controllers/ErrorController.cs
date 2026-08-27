using BDO;
using BDO.GWT.Core.Services;
using CMS.Websites.Routing;
using Kentico.Content.Web.Mvc;
using Kentico.Content.Web.Mvc.Routing;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.GLOBAL.Controllers
{
    [Route("Error/{code:int}")]
    public class ErrorController : Controller
    {
        private readonly IErrorService _errorService;
        private readonly IWebPageDataContextInitializer _webPageDataContextInitializer;
        private readonly IWebsiteChannelContext _websiteChannelContext;

        public ErrorController(
            IErrorService errorService,
            IWebPageDataContextInitializer webPageDataContextInitializer,
            IWebsiteChannelContext websiteChannelContext)
        {
            _errorService = errorService;
            _webPageDataContextInitializer = webPageDataContextInitializer;
            _websiteChannelContext = websiteChannelContext;
        }

        public async Task<IActionResult> Index(int code, CancellationToken cancellationToken)
        {
            var statusCodeReExecuteFeature = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();

            var languageName = TryExtractLanguageFromUrl(statusCodeReExecuteFeature)
                               ?? "en-gb";

            var statusCodePage = await _errorService.GetStatusCodePageAsync(
                statusCodeReExecuteFeature,
                languageName,
                _websiteChannelContext.WebsiteChannelName,
                cancellationToken);

            if (statusCodePage is null)
            {
                return StatusCode(code);
            }

            _webPageDataContextInitializer.Initialize(new RoutedWebPage
            {
                WebPageItemID = statusCodePage.SystemFields.WebPageItemID,
                WebPageItemGUID = statusCodePage.SystemFields.WebPageItemGUID,
                LanguageID = statusCodePage.SystemFields.ContentItemCommonDataContentLanguageID,
                LanguageName = languageName,
                ContentTypeID = statusCodePage.SystemFields.ContentItemContentTypeID,
                ContentTypeName = BDO.StatusCode.CONTENT_TYPE_NAME,
                WebsiteChannelID = statusCodePage.SystemFields.WebPageItemWebsiteChannelId,
                WebsiteChannelName = _websiteChannelContext.WebsiteChannelName,
            });

            return View(statusCodePage);
        }

        private static string? TryExtractLanguageFromUrl(IStatusCodeReExecuteFeature? feature)
        {
            if (feature is null) return null;

            var parts = feature.OriginalPath.Split('/', StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 0) return null;

            return IsLanguageTag(parts[0]) ? parts[0] : null;
        }

        private static bool IsLanguageTag(string segment)
        {
            return Regex.IsMatch(segment, @"^[a-z]{2}(-[a-zA-Z]{2,4})?$");
        }
    }
}