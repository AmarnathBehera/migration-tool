using BDO.GWT.Core;
using BDO.GWT.GLOBAL.Services;
using CMS.ContentEngine;
using CMS.Websites;
using CMS.Websites.Routing;
using Kentico.Content.Web.Mvc;
using Kentico.Content.Web.Mvc.Routing;
using Kentico.PageBuilder.Web.Mvc;
using Kentico.PageBuilder.Web.Mvc.Internal;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Reflection;

namespace BDO.GWT.GLOBAL.Tests.Services
{
    public class XperiencePageContextTests
    {
        private Mock<IHttpContextAccessor> _httpContextAccessorMock = null!;
        private Mock<IWebPageDataContextRetriever> _webPageDataContextRetrieverMock = null!;
        private Mock<IPageBuilderDataContextRetriever> _pageBuilderDataContextRetrieverMock = null!;
        private Mock<IWebsiteChannelContext> _websiteChannelContextMock = null!;
        private Mock<IContentQueryExecutor> _queryExecutorMock = null!;
        private XperiencePageContext _service = null!;

        [SetUp]
        public void Setup()
        {
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            _webPageDataContextRetrieverMock = new Mock<IWebPageDataContextRetriever>();
            _pageBuilderDataContextRetrieverMock = new Mock<IPageBuilderDataContextRetriever>();
            _websiteChannelContextMock = new Mock<IWebsiteChannelContext>();
            _queryExecutorMock = new Mock<IContentQueryExecutor>();

            _service = new XperiencePageContext(
                _httpContextAccessorMock.Object,
                _webPageDataContextRetrieverMock.Object,
                _pageBuilderDataContextRetrieverMock.Object,
                _websiteChannelContextMock.Object,
                _queryExecutorMock.Object);
        }

        // --- IsInPreviewMode -----------------------------------------------------

        [Test]
        public void IsInPreviewMode_ReturnsTrue_WhenChannelIsPreview()
        {
            _websiteChannelContextMock.Setup(x => x.IsPreview).Returns(true);

            Assert.That(_service.IsInPreviewMode, Is.True);
        }

        [Test]
        public void IsInPreviewMode_ReturnsFalse_WhenChannelIsNotPreview()
        {
            _websiteChannelContextMock.Setup(x => x.IsPreview).Returns(false);

            Assert.That(_service.IsInPreviewMode, Is.False);
        }

        // --- IsInPageBuilderMode -------------------------------------------------

        [Test]
        public void IsInPageBuilderMode_ReturnsTrue_WhenEditMode()
        {
            var httpContext = new DefaultHttpContext();
            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);
            _pageBuilderDataContextRetrieverMock
                .Setup(x => x.Retrieve())
                .Returns(new PageBuilderDataContext { EditMode = true });

            Assert.That(_service.IsInPageBuilderMode, Is.True);
        }

        [Test]
        public void IsInPageBuilderMode_ReturnsFalse_WhenNoHttpContext()
        {
            _httpContextAccessorMock
                .Setup(x => x.HttpContext)
                .Returns((HttpContext?)null);

            Assert.That(_service.IsInPageBuilderMode, Is.False);
        }

        [Test]
        public void IsInPageBuilderMode_ReturnsFalse_WhenNotEditMode()
        {
            var httpContext = new DefaultHttpContext();
            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);
            _pageBuilderDataContextRetrieverMock
                .Setup(x => x.Retrieve())
                .Returns(new PageBuilderDataContext { EditMode = false });

            Assert.That(_service.IsInPageBuilderMode, Is.False);
        }

        [Test]
        public void IsInPageBuilderMode_ReturnsFalse_WhenEditModeFalseAndHttpContextPresent()
        {
            var httpContext = new DefaultHttpContext();
            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);
            _pageBuilderDataContextRetrieverMock
                .Setup(x => x.Retrieve())
                .Returns(new PageBuilderDataContext { EditMode = false });

            Assert.That(_service.IsInPageBuilderMode, Is.False);
        }

        // --- GetCurrentPageAsync -------------------------------------------------

        [Test]
        public async Task GetCurrentPageAsync_ReturnsNull_WhenNoContext()
        {
            _webPageDataContextRetrieverMock
                .Setup(x => x.TryRetrieve(out It.Ref<WebPageDataContext>.IsAny))
                .Returns(false);

            var result = await _service.GetCurrentPageAsync<object>();

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetCurrentPageAsync_CallsTryRetrieve_WhenInvoked()
        {
            _webPageDataContextRetrieverMock
                .Setup(x => x.TryRetrieve(out It.Ref<WebPageDataContext>.IsAny))
                .Returns(false);

            await _service.GetCurrentPageAsync<object>(CancellationToken.None);

            _webPageDataContextRetrieverMock.Verify(
                x => x.TryRetrieve(out It.Ref<WebPageDataContext>.IsAny),
                Times.Once);
        }

        [Test]
        public async Task GetCurrentPageAsync_DoesNotCallQueryExecutor_WhenNoContext()
        {
            // GetMappedWebPageResult / GetWebPageResult are extension methods — Moq cannot
            // intercept them. We verify the interface itself has zero calls when context is absent.
            _webPageDataContextRetrieverMock
                .Setup(x => x.TryRetrieve(out It.Ref<WebPageDataContext>.IsAny))
                .Returns(false);

            await _service.GetCurrentPageAsync<object>(CancellationToken.None);

            _queryExecutorMock.VerifyNoOtherCalls();
        }

        // --- GetCurrentPageMetadataAsync -----------------------------------------

        [Test]
        public async Task GetCurrentPageMetadataAsync_ReturnsNull_WhenNoContext()
        {
            _webPageDataContextRetrieverMock
                .Setup(x => x.TryRetrieve(out It.Ref<WebPageDataContext>.IsAny))
                .Returns(false);

            var result = await _service.GetCurrentPageMetadataAsync();

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetCurrentPageMetadataAsync_CallsTryRetrieve_WhenInvoked()
        {
            _webPageDataContextRetrieverMock
                .Setup(x => x.TryRetrieve(out It.Ref<WebPageDataContext>.IsAny))
                .Returns(false);

            await _service.GetCurrentPageMetadataAsync(CancellationToken.None);

            _webPageDataContextRetrieverMock.Verify(
                x => x.TryRetrieve(out It.Ref<WebPageDataContext>.IsAny),
                Times.Once);
        }

        [Test]
        public async Task GetCurrentPageMetadataAsync_DoesNotCallQueryExecutor_WhenNoContext()
        {
            _webPageDataContextRetrieverMock
                .Setup(x => x.TryRetrieve(out It.Ref<WebPageDataContext>.IsAny))
                .Returns(false);

            await _service.GetCurrentPageMetadataAsync(CancellationToken.None);

            _queryExecutorMock.VerifyNoOtherCalls();
        }

        [Test]
        public async Task GetCurrentPageMetadataAsync_ReadsIsPreview_WhenContextPresent()
        {
            _websiteChannelContextMock.Setup(x => x.IsPreview).Returns(true);
            var ctx = BuildWebPageDataContext();
            SetupWebPageDataContext(ctx);

            // Extension method call will throw without a real DB — swallow it.
            try { await _service.GetCurrentPageMetadataAsync(CancellationToken.None); } catch { }

            _websiteChannelContextMock.Verify(x => x.IsPreview, Times.AtLeastOnce);
        }

        // --- GetCurrentPageBasePropertiesAsync -----------------------------------

        [Test]
        public async Task GetCurrentPageBasePropertiesAsync_WhenNoContext_ReturnsEmptyBaseWebPage()
        {
            _webPageDataContextRetrieverMock
                .Setup(x => x.TryRetrieve(out It.Ref<WebPageDataContext>.IsAny))
                .Returns(false);

            var result = await _service.GetCurrentPageBasePropertiesAsync(CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result.WebPageItemParentID, Is.EqualTo(0));
                Assert.That(result.WebPageItemTreePath, Is.Null);
            });
        }

        [Test]
        public async Task GetCurrentPageBasePropertiesAsync_WhenNoContext_ReturnsFreshInstance()
        {
            _webPageDataContextRetrieverMock
                .Setup(x => x.TryRetrieve(out It.Ref<WebPageDataContext>.IsAny))
                .Returns(false);

            var result = await _service.GetCurrentPageBasePropertiesAsync(CancellationToken.None);

            Assert.That(result, Is.InstanceOf<BDO.GWT.Core.BaseWebPage>());
        }

        [Test]
        public async Task GetCurrentPageBasePropertiesAsync_CallsTryRetrieve_WhenInvoked()
        {
            _webPageDataContextRetrieverMock
                .Setup(x => x.TryRetrieve(out It.Ref<WebPageDataContext>.IsAny))
                .Returns(false);

            await _service.GetCurrentPageBasePropertiesAsync(CancellationToken.None);

            _webPageDataContextRetrieverMock.Verify(
                x => x.TryRetrieve(out It.Ref<WebPageDataContext>.IsAny),
                Times.Once);
        }

        [Test]
        public async Task GetCurrentPageBasePropertiesAsync_DoesNotCallQueryExecutor_WhenNoContext()
        {
            _webPageDataContextRetrieverMock
                .Setup(x => x.TryRetrieve(out It.Ref<WebPageDataContext>.IsAny))
                .Returns(false);

            await _service.GetCurrentPageBasePropertiesAsync(CancellationToken.None);

            _queryExecutorMock.VerifyNoOtherCalls();
        }

        // --- GetAnyPageByWebPageItemId -------------------------------------------

        [Test]
        public async Task GetAnyPageByWebPageItemId_WhenPreviewMode_ReadsIsPreview()
        {
            _websiteChannelContextMock.Setup(x => x.IsPreview).Returns(true);

            try { await _service.GetAnyPageByWebPageItemId(1, CancellationToken.None); } catch { }

            _websiteChannelContextMock.Verify(x => x.IsPreview, Times.AtLeastOnce);
        }

        [Test]
        public async Task GetAnyPageByWebPageItemId_WhenNotPreviewMode_ReadsIsPreview()
        {
            _websiteChannelContextMock.Setup(x => x.IsPreview).Returns(false);

            try { await _service.GetAnyPageByWebPageItemId(1, CancellationToken.None); } catch { }

            _websiteChannelContextMock.Verify(x => x.IsPreview, Times.AtLeastOnce);
        }

        // --- GetPageTitleByWebPageItemId -----------------------------------------

        [Test]
        public async Task GetPageTitleByWebPageItemId_WhenPreviewMode_ReadsIsPreview()
        {
            _websiteChannelContextMock.Setup(x => x.IsPreview).Returns(true);

            try { await _service.GetPageTitleByWebPageItemId(1, CancellationToken.None); } catch { }

            _websiteChannelContextMock.Verify(x => x.IsPreview, Times.AtLeastOnce);
        }

        [Test]
        public async Task GetPageTitleByWebPageItemId_WhenNotPreviewMode_ReadsIsPreview()
        {
            _websiteChannelContextMock.Setup(x => x.IsPreview).Returns(false);

            try { await _service.GetPageTitleByWebPageItemId(1, CancellationToken.None); } catch { }

            _websiteChannelContextMock.Verify(x => x.IsPreview, Times.AtLeastOnce);
        }

        // --- Helpers -------------------------------------------------------------

        private static WebPageDataContext BuildWebPageDataContext()
        {
            var routedWebPage = new RoutedWebPage
            {
                ContentTypeName = "BDO.TestPage",
                WebsiteChannelName = "global",
                LanguageName = "en",
                WebPageItemID = 42,
            };

            return (WebPageDataContext)typeof(WebPageDataContext)
                .GetConstructor(
                    BindingFlags.NonPublic | BindingFlags.Instance,
                    null,
                    [typeof(RoutedWebPage)],
                    null)!
                .Invoke([routedWebPage]);
        }

        private void SetupWebPageDataContext(WebPageDataContext ctx)
        {
            _webPageDataContextRetrieverMock
                .Setup(x => x.TryRetrieve(out It.Ref<WebPageDataContext>.IsAny))
                .Returns(new TryRetrieveDelegate((out WebPageDataContext c) =>
                {
                    c = ctx;
                    return true;
                }));
        }

        private delegate bool TryRetrieveDelegate(out WebPageDataContext ctx);
    }
}
