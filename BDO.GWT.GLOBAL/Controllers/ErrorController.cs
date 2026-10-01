using BDO;
using BDO.GWT.Core.Services;
using CMS.ContentEngine;
using CMS.Core;
using CMS.DataEngine;
using Kentico.Content.Web.Mvc;
using Kentico.Content.Web.Mvc.Routing;
using CMS.Websites.Routing;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.GLOBAL.Controllers
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Major Code Smell",
        "S6934:A Route attribute should be added to the controller when a route template is specified at the action level",
        Justification = "Index is reached through Kentico conventional page routing; a controller-level route would make it attribute-routed only and break CMS-rendered status code pages.")]
    public partial class ErrorController : Controller
    {
        private const string DefaultLanguageName = "en-gb";

        private readonly IErrorService _errorService;
        private readonly IWebPageDataContextInitializer _webPageDataContextInitializer;
        private readonly IWebsiteChannelContext _websiteChannelContext;
        private readonly IEventLogService _eventLogService;
        private readonly IInfoProvider<ContentLanguageInfo> _languageProvider;

        // Cache of valid Kentico language names (case-insensitive). Populated lazily, once per app lifetime.
        private static Task<HashSet<string>> _validLanguagesTask;
        private static readonly object _validLanguagesLock = new();

        public ErrorController(
            IErrorService errorService,
            IWebPageDataContextInitializer webPageDataContextInitializer,
            IWebsiteChannelContext websiteChannelContext,
            IEventLogService eventLogService,
            IInfoProvider<ContentLanguageInfo> languageProvider)
        {
            _errorService = errorService;
            _webPageDataContextInitializer = webPageDataContextInitializer;
            _websiteChannelContext = websiteChannelContext;
            _eventLogService = eventLogService;
            _languageProvider = languageProvider;
        }

        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var page = await _errorService.GetCurrentStatusCodePageAsync(cancellationToken);
            if (page is null)
            {
                return NotFound();
            }

            if (page.HttpStatusCode is >= 400 and <= 599)
            {
                Response.StatusCode = page.HttpStatusCode;
            }

            return View(nameof(Index), page);
        }

        [Route("Error/{code:int}")]
        public async Task<IActionResult> ReExecute(int code, CancellationToken cancellationToken)
        {
            // Log the exception to the CMS event log (only for server errors)
            LogExceptionToCms(code);

            var statusCodeReExecuteFeature = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();

            var languageName = await TryExtractLanguageFromUrlAsync(statusCodeReExecuteFeature, cancellationToken)
                               ?? DefaultLanguageName;

            var statusCodePage = await _errorService.GetStatusCodePageAsync(
                statusCodeReExecuteFeature,
                languageName,
                _websiteChannelContext.WebsiteChannelName,
                CancellationToken.None);

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

            return View(nameof(Index), statusCodePage);
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

        private async Task<string> TryExtractLanguageFromUrlAsync(
            IStatusCodeReExecuteFeature feature,
            CancellationToken cancellationToken)
        {
            if (feature?.OriginalPath is not { Length: > 0 } path) return null;

            var parts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return null;

            var candidate = parts[0];

            // Cheap shape check first to avoid touching the provider for obvious non-language segments.
            if (!IsLanguageTag(candidate)) return null;

            var validLanguages = await GetValidLanguagesAsync(cancellationToken);

            return validLanguages.Contains(candidate) ? candidate : null;
        }

        private Task<HashSet<string>> GetValidLanguagesAsync(CancellationToken cancellationToken)
    => GetOrCreateValidLanguagesTask(_languageProvider, cancellationToken);

        private static Task<HashSet<string>> GetOrCreateValidLanguagesTask(
            IInfoProvider<ContentLanguageInfo> languageProvider,
            CancellationToken cancellationToken)
        {
            if (_validLanguagesTask is { } existing)
            {
                return existing;
            }

            lock (_validLanguagesLock)
            {
                return _validLanguagesTask ??= LoadValidLanguagesAsync(languageProvider, cancellationToken);
            }
        }

        private static async Task<HashSet<string>> LoadValidLanguagesAsync(
            IInfoProvider<ContentLanguageInfo> languageProvider,
            CancellationToken cancellationToken)
        {
            var languages = await languageProvider.Get()
                .Column(nameof(ContentLanguageInfo.ContentLanguageName))
                .GetEnumerableTypedResultAsync(cancellationToken: cancellationToken);

            return new HashSet<string>(
                languages.Select(l => l.ContentLanguageName),
                StringComparer.OrdinalIgnoreCase);
        }

        private static bool IsLanguageTag(string segment)
        {
            return MyRegex().IsMatch(segment);
        }

        [GeneratedRegex(@"^[a-z]{2}(-[a-zA-Z]{2,4})?$")]
        private static partial Regex MyRegex();
    }
}