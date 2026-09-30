using BDO;
using BDO.GWT.Core.Services;
using BDO.GWT.GLOBAL.Controllers;
using CMS.ContentEngine;
using CMS.Core;
using CMS.DataEngine;
using CMS.Websites;
using CMS.Websites.Routing;
using Kentico.Content.Web.Mvc;
using Kentico.Content.Web.Mvc.Routing;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.GLOBAL.Tests.Controllers
{
    public class ErrorControllerTests
    {
        private Mock<IErrorService> _errorServiceMock = null!;
        private Mock<IWebPageDataContextInitializer> _webPageDataContextInitializerMock = null!;
        private Mock<IWebsiteChannelContext> _websiteChannelContextMock = null!;
        private Mock<IEventLogService> _eventLogServiceMock = null!;
        private Mock<IInfoProvider<ContentLanguageInfo>> _languageProviderMock = null!;
        private ErrorController _controller = null!;
        private DefaultHttpContext _httpContext = null!;

        [SetUp]
        public void Setup()
        {
            _errorServiceMock = new Mock<IErrorService>();
            _webPageDataContextInitializerMock = new Mock<IWebPageDataContextInitializer>();
            _websiteChannelContextMock = new Mock<IWebsiteChannelContext>();
            _eventLogServiceMock = new Mock<IEventLogService>();
            _languageProviderMock = new Mock<IInfoProvider<ContentLanguageInfo>>();

            _websiteChannelContextMock
                .Setup(x => x.WebsiteChannelName)
                .Returns("TestChannel");

            // Pre-seed the static valid-languages cache so the tests don't need to mock
            // the Kentico ObjectQuery fluent chain used by LoadValidLanguagesAsync.
            SeedValidLanguages("en-gb", "fr-fr");

            _httpContext = new DefaultHttpContext();

            _controller = new ErrorController(
                _errorServiceMock.Object,
                _webPageDataContextInitializerMock.Object,
                _websiteChannelContextMock.Object,
                _eventLogServiceMock.Object,
                _languageProviderMock.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = _httpContext
                }
            };
        }

        [TearDown]
        public void TearDown()
        {
            _controller.Dispose();
            ResetValidLanguagesCache();
        }

        [Test]
        public async Task Index_ReturnsStatusCodeResult_WhenNoStatusCodePageFound()
        {
            SetReExecuteFeature("/en-gb/missing");

            _errorServiceMock
                .Setup(x => x.GetStatusCodePageAsync(
                    It.IsAny<IStatusCodeReExecuteFeature>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((StatusCode?)null);

            var result = await _controller.ReExecute(404, CancellationToken.None);

            var statusResult = result as StatusCodeResult;
            Assert.That(statusResult, Is.Not.Null);
            Assert.That(statusResult!.StatusCode, Is.EqualTo(404));
        }

        [Test]
        public async Task Index_ReturnsViewResult_WhenStatusCodePageFound()
        {
            SetReExecuteFeature("/en-gb/broken");

            var statusPage = CreateStatusCodePage();

            _errorServiceMock
                .Setup(x => x.GetStatusCodePageAsync(
                    It.IsAny<IStatusCodeReExecuteFeature>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(statusPage);

            var result = await _controller.ReExecute(500, CancellationToken.None);

            var viewResult = result as ViewResult;
            Assert.That(viewResult, Is.Not.Null);
            Assert.That(viewResult!.Model, Is.SameAs(statusPage));

            _webPageDataContextInitializerMock.Verify(
                x => x.Initialize(It.IsAny<RoutedWebPage>()),
                Times.Once);
        }

        [Test]
        public async Task Index_LogsException_WhenExceptionFeaturePresent()
        {
            SetReExecuteFeature("/en-gb/error");

            var exception = new InvalidOperationException("Deliberate test 500");
            _httpContext.Features.Set<IExceptionHandlerFeature>(new ExceptionHandlerFeature
            {
                Error = exception,
                Path = "/en-gb/error"
            });

            _errorServiceMock
                .Setup(x => x.GetStatusCodePageAsync(
                    It.IsAny<IStatusCodeReExecuteFeature>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((StatusCode?)null);

            await _controller.ReExecute(500, CancellationToken.None);

            _eventLogServiceMock.Verify(
                x => x.LogEvent(It.Is<EventLogData>(d =>
                    d.Source == nameof(ErrorController) &&
                    d.EventCode == "500" &&
                    d.Exception == exception)),
                Times.Once);
        }

        [Test]
        public async Task Index_DoesNotLogException_WhenNoExceptionFeature()
        {
            SetReExecuteFeature("/en-gb/missing");

            _errorServiceMock
                .Setup(x => x.GetStatusCodePageAsync(
                    It.IsAny<IStatusCodeReExecuteFeature>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((StatusCode?)null);

            await _controller.ReExecute(404, CancellationToken.None);

            _eventLogServiceMock.Verify(
                x => x.LogEvent(It.IsAny<EventLogData>()),
                Times.Never);
        }

        [Test]
        public async Task Index_ExtractsLanguageFromUrl_WhenLanguageIsConfigured()
        {
            SetReExecuteFeature("/fr-fr/introuvable");

            var capturedLanguage = await CaptureLanguageAsync(404);

            Assert.That(capturedLanguage, Is.EqualTo("fr-fr"));
        }

        [Test]
        public async Task Index_DefaultsToEnGb_WhenNoLanguageInUrl()
        {
            SetReExecuteFeature("/broken-page");

            var capturedLanguage = await CaptureLanguageAsync(404);

            Assert.That(capturedLanguage, Is.EqualTo("en-gb"));
        }

        [Test]
        public async Task Index_DefaultsToEnGb_WhenSegmentLooksLikeLanguageButIsNotConfigured()
        {
            // "ln" matches the shape regex but is NOT a real Kentico language.
            // Previously this caused: InvalidOperationException "Language 'ln' does not exist."
            SetReExecuteFeature("/ln/some-page");

            var capturedLanguage = await CaptureLanguageAsync(404);

            Assert.That(capturedLanguage, Is.EqualTo("en-gb"));
        }

        [Test]
        public async Task Index_DefaultsToEnGb_WhenFirstSegmentIsNotLanguageShape()
        {
            SetReExecuteFeature("/products/details");

            var capturedLanguage = await CaptureLanguageAsync(404);

            Assert.That(capturedLanguage, Is.EqualTo("en-gb"));
        }

        [Test]
        public async Task Index_DefaultsToEnGb_WhenOriginalPathIsEmpty()
        {
            SetReExecuteFeature(string.Empty);

            var capturedLanguage = await CaptureLanguageAsync(404);

            Assert.That(capturedLanguage, Is.EqualTo("en-gb"));
        }

        [Test]
        public async Task Index_UsesResolvedLanguage_WhenInitializingWebPageDataContext()
        {
            SetReExecuteFeature("/fr-fr/broken");

            var statusPage = CreateStatusCodePage();

            _errorServiceMock
                .Setup(x => x.GetStatusCodePageAsync(
                    It.IsAny<IStatusCodeReExecuteFeature>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(statusPage);

            RoutedWebPage? captured = null;
            _webPageDataContextInitializerMock
                .Setup(x => x.Initialize(It.IsAny<RoutedWebPage>()))
                .Callback<RoutedWebPage>(p => captured = p);

            await _controller.ReExecute(500, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(captured, Is.Not.Null);
                Assert.That(captured!.LanguageName, Is.EqualTo("fr-fr"));
                Assert.That(captured.WebsiteChannelName, Is.EqualTo("TestChannel"));
            });
        }

        private async Task<string?> CaptureLanguageAsync(int code)
        {
            string? capturedLanguage = null;

            _errorServiceMock
                .Setup(x => x.GetStatusCodePageAsync(
                    It.IsAny<IStatusCodeReExecuteFeature>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .Callback<IStatusCodeReExecuteFeature, string, string, CancellationToken>(
                    (_, lang, _, _) => capturedLanguage = lang)
                .ReturnsAsync((StatusCode?)null);

            await _controller.ReExecute(code, CancellationToken.None);

            return capturedLanguage;
        }

        private void SetReExecuteFeature(string originalPath)
        {
            _httpContext.Features.Set<IStatusCodeReExecuteFeature>(new StatusCodeReExecuteFeature
            {
                OriginalPath = originalPath,
                OriginalPathBase = string.Empty
            });
        }

        private static StatusCode CreateStatusCodePage()
        {
            return new StatusCode
            {
                HttpStatusCode = 500,
                MetadataTitle = "Server Error",
                SystemFields = new WebPageFields
                {
                    WebPageItemID = 42
                }
            };
        }

        // Seeds the static valid-languages cache on ErrorController so tests don't need
        // to mock the Kentico ObjectQuery<ContentLanguageInfo> fluent chain.
        private static void SeedValidLanguages(params string[] languageNames)
        {
            var set = new HashSet<string>(languageNames, StringComparer.OrdinalIgnoreCase);
            GetValidLanguagesField().SetValue(null, Task.FromResult(set));
        }

        private static void ResetValidLanguagesCache()
        {
            GetValidLanguagesField().SetValue(null, null);
        }

        private static System.Reflection.FieldInfo GetValidLanguagesField()
        {
            return typeof(ErrorController).GetField(
                "_validLanguagesTask",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
                ?? throw new InvalidOperationException(
                    "Static field '_validLanguagesTask' was not found on ErrorController.");
        }
    }
}
