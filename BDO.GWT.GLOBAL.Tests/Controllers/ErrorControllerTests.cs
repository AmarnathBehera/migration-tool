using BDO;
using BDO.GWT.Core.Services;
using CMS.Core;
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
using System.Threading;
using System.Threading.Tasks;
using BDO.GWT.GLOBAL.Controllers;

namespace BDO.GWT.GLOBAL.Tests.Controllers
{
    public class ErrorControllerTests
    {
        private Mock<IErrorService> _errorServiceMock = null!;
        private Mock<IWebPageDataContextInitializer> _webPageDataContextInitializerMock = null!;
        private Mock<IWebsiteChannelContext> _websiteChannelContextMock = null!;
        private Mock<IEventLogService> _eventLogServiceMock = null!;
        private ErrorController _controller = null!;
        private DefaultHttpContext _httpContext = null!;

        [SetUp]
        public void Setup()
        {
            _errorServiceMock = new Mock<IErrorService>();
            _webPageDataContextInitializerMock = new Mock<IWebPageDataContextInitializer>();
            _websiteChannelContextMock = new Mock<IWebsiteChannelContext>();
            _eventLogServiceMock = new Mock<IEventLogService>();

            _websiteChannelContextMock
                .Setup(x => x.WebsiteChannelName)
                .Returns("TestChannel");

            _httpContext = new DefaultHttpContext();

            _controller = new ErrorController(
                _errorServiceMock.Object,
                _webPageDataContextInitializerMock.Object,
                _websiteChannelContextMock.Object,
                _eventLogServiceMock.Object)
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

            var result = await _controller.Index(404, CancellationToken.None);

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

            var result = await _controller.Index(500, CancellationToken.None);

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

            await _controller.Index(500, CancellationToken.None);

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

            await _controller.Index(404, CancellationToken.None);

            _eventLogServiceMock.Verify(
                x => x.LogEvent(It.IsAny<EventLogData>()),
                Times.Never);
        }

        [Test]
        public async Task Index_ExtractsLanguageFromUrl_AndPassesToErrorService()
        {
            SetReExecuteFeature("/fr-fr/introuvable");

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

            await _controller.Index(404, CancellationToken.None);

            Assert.That(capturedLanguage, Is.EqualTo("fr-fr"));
        }

        [Test]
        public async Task Index_DefaultsToEnGb_WhenNoLanguageInUrl()
        {
            SetReExecuteFeature("/broken-page");

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

            await _controller.Index(404, CancellationToken.None);

            Assert.That(capturedLanguage, Is.EqualTo("en-gb"));
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
    }
}
