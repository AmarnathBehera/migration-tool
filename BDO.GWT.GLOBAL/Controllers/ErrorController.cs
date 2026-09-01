using BDO;
using BDO.GWT.Core.Services;
using CMS.Core;
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
    public partial class ErrorController : Controller
    {
        private readonly IErrorService _errorService;
        private readonly IWebPageDataContextInitializer _webPageDataContextInitializer;
        private readonly IWebsiteChannelContext _websiteChannelContext;
        private readonly IEventLogService _eventLogService;

        public ErrorController(
            IErrorService errorService,
            IWebPageDataContextInitializer webPageDataContextInitializer,
            IWebsiteChannelContext websiteChannelContext,
            IEventLogService eventLogService)
        {
            _errorService = errorService;
            _webPageDataContextInitializer = webPageDataContextInitializer;
            _websiteChannelContext = websiteChannelContext;
            _eventLogService = eventLogService;
        }

        public async Task<IActionResult> Index(int code, CancellationToken cancellationToken)
        {
            // Log the exception to the CMS event log (only for server errors)
            LogExceptionToCms(code);

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

        private void LogExceptionToCms(int code)
        {
            // Retrieve the original exception captured by UseExceptionHandler
            var exceptionFeature = HttpContext.Features.Get<IExceptionHandlerFeature>();

            if (exceptionFeature?.Error is Exception exception)
            {
                _eventLogService.LogException(
                    source: nameof(ErrorController),
                    eventCode: code.ToString(),
                    ex: exception,
                    additionalMessage: $"An unhandled exception occurred at path: {exceptionFeature.Path}");
            }
        }

        private static string TryExtractLanguageFromUrl(IStatusCodeReExecuteFeature feature)
        {
            if (feature is null) return null;

            var parts = feature.OriginalPath.Split('/', StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 0) return null;

            return IsLanguageTag(parts[0]) ? parts[0] : null;
        }

        private static bool IsLanguageTag(string segment)
        {
            return MyRegex().IsMatch(segment);
        }

        [GeneratedRegex(@"^[a-z]{2}(-[a-zA-Z]{2,4})?$")]
        private static partial Regex MyRegex();
    }
}