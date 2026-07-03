using BDO;
using BDO.GWT.Core;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.Services;
using CMS.Websites;
using Kentico.Content.Web.Mvc;
using Moq;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace BDO.GWT.GLOBAL.Tests.Services
{
    public class BreadcrumbsServiceTests
    {
        private static readonly string[] HomePlusCurrentPageTitles = ["Home", "Current page"];

        private Mock<IWebPageDataContextRetriever> _dataContextRetrieverMock = null!;
        private Mock<IWebPageUrlRetriever> _pageUrlRetrieverMock = null!;
        private Mock<IContentService> _contentServiceMock = null!;
        private Mock<IPageContext> _pageContextMock = null!;
        private BreadcrumbsService _service = null!;

        [SetUp]
        public void SetUp()
        {
            _dataContextRetrieverMock = new Mock<IWebPageDataContextRetriever>();
            _pageUrlRetrieverMock = new Mock<IWebPageUrlRetriever>();
            _contentServiceMock = new Mock<IContentService>();
            _pageContextMock = new Mock<IPageContext>();

            _service = new BreadcrumbsService(
                _dataContextRetrieverMock.Object,
                _pageUrlRetrieverMock.Object,
                _pageContextMock.Object,
                _contentServiceMock.Object
            );
        }

        [Test]
        public async Task GetAsync_WhenContextNotAvailable_ReturnsEmptyBreadcrumbsDto()
        {
            _dataContextRetrieverMock
                .Setup(r => r.TryRetrieve(out It.Ref<WebPageDataContext>.IsAny))
                .Returns(false);

            var result = await _service.GetAsync(CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Links, Is.Null);
        }

        [Test]
        public async Task GetAsync_WhenContextNotAvailable_DoesNotCallContentService()
        {
            _dataContextRetrieverMock
                .Setup(r => r.TryRetrieve(out It.Ref<WebPageDataContext>.IsAny))
                .Returns(false);

            await _service.GetAsync(CancellationToken.None);

            _contentServiceMock.Verify(
                s => s.RetrievePagesAsync<HomePage>(It.IsAny<CancellationToken>()),
                Times.Never
            );
        }

        [Test]
        public async Task GetAsync_WhenContextNotAvailable_DoesNotCallPageContext()
        {
            _dataContextRetrieverMock
                .Setup(r => r.TryRetrieve(out It.Ref<WebPageDataContext>.IsAny))
                .Returns(false);

            await _service.GetAsync(CancellationToken.None);

            _pageContextMock.Verify(
                p => p.GetCurrentPageBasePropertiesAsync(It.IsAny<CancellationToken>()),
                Times.Never
            );
        }

        [Test]
        public async Task GetAsync_WhenContextNotAvailable_DoesNotCallUrlRetriever()
        {
            _dataContextRetrieverMock
                .Setup(r => r.TryRetrieve(out It.Ref<WebPageDataContext>.IsAny))
                .Returns(false);

            await _service.GetAsync(CancellationToken.None);

            _pageUrlRetrieverMock.Verify(
                r => r.Retrieve(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()),
                Times.Never
            );
        }

        [Test]
        public async Task GetAsync_WhenContextNotAvailable_ReturnsNullLinks()
        {
            _dataContextRetrieverMock
                .Setup(r => r.TryRetrieve(out It.Ref<WebPageDataContext>.IsAny))
                .Returns(false);

            var result = await _service.GetAsync(CancellationToken.None);

            Assert.That(result.Links, Is.Null);
        }

        [Test]
        public async Task GetAsync_WhenAncestorIsSectionMicrosites_SkipsSectionMicrositesBreadcrumb()
        {
            SetupDataContext();
            _contentServiceMock
                .Setup(s => s.RetrievePagesAsync<HomePage>(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Enumerable.Empty<HomePage>());
            _pageContextMock
                .Setup(p => p.GetCurrentPageBasePropertiesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new BaseWebPage
                {
                    WebPageItemID = 200,
                    WebPageItemName = "Current page",
                    WebPageItemParentID = 100
                });
            _pageContextMock
                .Setup(p => p.GetAnyPageByWebPageItemId(100, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new SectionMicrosites
                {
                    SystemFields = CreateWebPageFields(100, "Section microsite", 0)
                });
            _pageContextMock
                .Setup(p => p.GetPageTitleByWebPageItemId(200, It.IsAny<CancellationToken>()))
                .ReturnsAsync("Current page");
            _pageUrlRetrieverMock
                .Setup(r => r.Retrieve(100, "en", false, It.IsAny<CancellationToken>()))
                .ReturnsAsync(CreateWebPageUrl("~/section-microsite"));
            _pageUrlRetrieverMock
                .Setup(r => r.Retrieve(200, "en", false, It.IsAny<CancellationToken>()))
                .ReturnsAsync(CreateWebPageUrl("~/current-page"));

            var result = await _service.GetAsync(CancellationToken.None);

            Assert.That(result.Links, Is.Not.Null);
            Assert.That(result.Links!.Select(link => link.Title), Is.EqualTo(HomePlusCurrentPageTitles));
            _pageContextMock.Verify(
                p => p.GetPageTitleByWebPageItemId(100, It.IsAny<CancellationToken>()),
                Times.Never
            );
        }

        [Test]
        public async Task GetAsync_WhenAncestorUrlIsWhitespace_SkipsAncestorBreadcrumb()
        {
            SetupDataContext();
            _contentServiceMock
                .Setup(s => s.RetrievePagesAsync<HomePage>(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Enumerable.Empty<HomePage>());
            _pageContextMock
                .Setup(p => p.GetCurrentPageBasePropertiesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new BaseWebPage
                {
                    WebPageItemID = 200,
                    WebPageItemName = "Current page",
                    WebPageItemParentID = 100
                });
            _pageContextMock
                .Setup(p => p.GetAnyPageByWebPageItemId(100, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new MicrositeMasterPage
                {
                    SystemFields = CreateWebPageFields(100, "Ancestor page", 0)
                });
            _pageContextMock
                .Setup(p => p.GetPageTitleByWebPageItemId(200, It.IsAny<CancellationToken>()))
                .ReturnsAsync("Current page");
            _pageUrlRetrieverMock
                .Setup(r => r.Retrieve(100, "en", false, It.IsAny<CancellationToken>()))
                .ReturnsAsync(CreateWebPageUrl("   "));
            _pageUrlRetrieverMock
                .Setup(r => r.Retrieve(200, "en", false, It.IsAny<CancellationToken>()))
                .ReturnsAsync(CreateWebPageUrl("~/current-page"));

            var result = await _service.GetAsync(CancellationToken.None);

            Assert.That(result.Links, Is.Not.Null);
            Assert.That(result.Links!.Select(link => link.Title), Is.EqualTo(HomePlusCurrentPageTitles));
            _pageContextMock.Verify(
                p => p.GetPageTitleByWebPageItemId(100, It.IsAny<CancellationToken>()),
                Times.Never
            );
        }

        private void SetupDataContext(
            string contentTypeName = "BDO.Event",
            int webPageItemId = 200,
            string languageName = "en")
        {
            var context = CreateWebPageDataContext(contentTypeName, webPageItemId, languageName);

            _dataContextRetrieverMock
                .Setup(r => r.TryRetrieve(out context))
                .Returns(true);
        }

        private static WebPageDataContext CreateWebPageDataContext(
            string contentTypeName,
            int webPageItemId,
            string languageName)
        {
            var context = CreateUninitialized<WebPageDataContext>();
            var webPageType = typeof(WebPageDataContext)
                .GetProperty("WebPage", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!
                .PropertyType;
            var webPage = CreateUninitialized(webPageType);

            SetMember(webPage, "ContentTypeName", contentTypeName);
            SetMember(webPage, "WebPageItemID", webPageItemId);
            SetMember(webPage, "LanguageName", languageName);
            SetMember(context, "WebPage", webPage);

            return context;
        }

        private static WebPageFields CreateWebPageFields(int webPageItemId, string webPageItemName, int webPageItemParentId)
        {
            var fields = CreateUninitialized<WebPageFields>();

            SetMember(fields, "WebPageItemID", webPageItemId);
            SetMember(fields, "WebPageItemName", webPageItemName);
            SetMember(fields, "WebPageItemParentID", webPageItemParentId);

            return fields;
        }

        private static WebPageUrl CreateWebPageUrl(string relativePath)
        {
            var url = CreateUninitialized<WebPageUrl>();
            SetMember(url, "RelativePath", relativePath);
            return url;
        }

        private static T CreateUninitialized<T>() where T : class =>
            (T)RuntimeHelpers.GetUninitializedObject(typeof(T));

        private static object CreateUninitialized(Type type) =>
            RuntimeHelpers.GetUninitializedObject(type);

        private static void SetMember(object target, string memberName, object? value)
        {
            var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            var type = target.GetType();
            var property = type.GetProperty(memberName, flags);

            var setter = property?.GetSetMethod(nonPublic: true);
            if (setter != null)
            {
                setter.Invoke(target, new[] { value });
                return;
            }

            var field = type.GetField(memberName, flags)
                ?? type.GetField($"<{memberName}>k__BackingField", flags);

            if (field != null)
            {
                field.SetValue(target, value);
                return;
            }

            throw new InvalidOperationException($"Member '{memberName}' was not found on type '{type.FullName}'.");
        }
    }
}
