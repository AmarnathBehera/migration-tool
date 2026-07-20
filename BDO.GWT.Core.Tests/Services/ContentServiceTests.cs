using BDO.GWT.Core;
using BDO.GWT.Core.Services;
using CMS.ContentEngine;
using CMS.Helpers;
using CMS.Websites.Routing;
using Kentico.Content.Web.Mvc;
using Moq;

namespace BDO.GWT.Core.Tests.Services
{
    public class ContentServiceTests
    {
        private Mock<IContentQueryExecutor> _executorMock = null!;
        private Mock<IWebsiteChannelContext> _websiteChannelContextMock = null!;
        private Mock<IProgressiveCache> _cacheMock = null!;
        private Mock<IContentRetriever> _contentRetrieverMock = null!;
        private Mock<IContentQueryModelTypeMapper> _modelTypeMapperMock = null!;
        private Mock<ICultureContext> _cultureContextMock = null!;

        private ContentService _service = null!;

        [SetUp]
        public void Setup()
        {
            _executorMock = new Mock<IContentQueryExecutor>();
            _websiteChannelContextMock = new Mock<IWebsiteChannelContext>();
            _cacheMock = new Mock<IProgressiveCache>();
            _contentRetrieverMock = new Mock<IContentRetriever>();
            _modelTypeMapperMock = new Mock<IContentQueryModelTypeMapper>();
            _cultureContextMock = new Mock<ICultureContext>();

            _websiteChannelContextMock.SetupGet(x => x.WebsiteChannelID).Returns(42);
            _websiteChannelContextMock.SetupGet(x => x.WebsiteChannelName).Returns("BDOGlobal");
            _websiteChannelContextMock.SetupGet(x => x.IsPreview).Returns(false);
            _cultureContextMock.SetupGet(x => x.CurrentLanguageTag).Returns("en-US");

            _service = new ContentService(
                _executorMock.Object,
                _websiteChannelContextMock.Object,
                _cacheMock.Object,
                _contentRetrieverMock.Object,
                _executorMock.Object,
                _modelTypeMapperMock.Object,
                _cultureContextMock.Object);
        }

        [Test]
        public void Constructor_ThrowsArgumentNullException_WhenExecutorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new ContentService(
                    null!,
                    _websiteChannelContextMock.Object,
                    _cacheMock.Object,
                    _contentRetrieverMock.Object,
                    _executorMock.Object,
                    _modelTypeMapperMock.Object,
                    _cultureContextMock.Object));
        }

        [Test]
        public void Constructor_ThrowsArgumentNullException_WhenWebsiteChannelContextIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new ContentService(
                    _executorMock.Object,
                    null!,
                    _cacheMock.Object,
                    _contentRetrieverMock.Object,
                    _executorMock.Object,
                    _modelTypeMapperMock.Object,
                    _cultureContextMock.Object));
        }

        [Test]
        public void Constructor_ThrowsArgumentNullException_WhenCacheIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new ContentService(
                    _executorMock.Object,
                    _websiteChannelContextMock.Object,
                    null!,
                    _contentRetrieverMock.Object,
                    _executorMock.Object,
                    _modelTypeMapperMock.Object,
                    _cultureContextMock.Object));
        }

        [Test]
        public void Constructor_ThrowsArgumentNullException_WhenContentRetrieverIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new ContentService(
                    _executorMock.Object,
                    _websiteChannelContextMock.Object,
                    _cacheMock.Object,
                    null!,
                    _executorMock.Object,
                    _modelTypeMapperMock.Object,
                    _cultureContextMock.Object));
        }

        [Test]
        public void Constructor_ThrowsArgumentNullException_WhenContentQueryExecutorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new ContentService(
                    _executorMock.Object,
                    _websiteChannelContextMock.Object,
                    _cacheMock.Object,
                    _contentRetrieverMock.Object,
                    null!,
                    _modelTypeMapperMock.Object,
                    _cultureContextMock.Object));
        }

        [Test]
        public void Constructor_ThrowsArgumentNullException_WhenContentQueryModelTypeMapperIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new ContentService(
                    _executorMock.Object,
                    _websiteChannelContextMock.Object,
                    _cacheMock.Object,
                    _contentRetrieverMock.Object,
                    _executorMock.Object,
                    null!,
                    _cultureContextMock.Object));
        }

        [Test]
        public void GetCurrentChannelID_ReturnsWebsiteChannelID()
        {
            var id = _service.GetCurrentChannelID();

            Assert.That(id, Is.EqualTo(42));
        }

        [Test]
        public void GetCurrentSiteLegalNameAsync_ReturnsWebsiteChannelName()
        {
            var name = _service.GetCurrentSiteLegalNameAsync();

            Assert.That(name, Is.EqualTo("BDOGlobal"));
        }

        [Test]
        public void GetCurrentSiteLogoImageUrlAsync_ReturnsDefaultBdoLogoUrl()
        {
            var url = _service.GetCurrentSiteLogoImageUrlAsync();

            Assert.That(url, Is.EqualTo("https://cdn.bdo.global/images/bdo_logo/1.0.0/bdo_logo_color.png"));
        }

        [Test]
        public void GetFallbackMetadataDescription_ReturnsMetaDescription_WhenProvided()
        {
            var result = _service.GetFallbackMetadataDescription("primary", "fallback");

            Assert.That(result, Is.EqualTo("primary"));
        }

        [Test]
        public void GetFallbackMetadataDescription_ReturnsOgDescription_WhenMetaDescriptionIsNull()
        {
            var result = _service.GetFallbackMetadataDescription(null, "fallback");

            Assert.That(result, Is.EqualTo("fallback"));
        }

        [Test]
        public void GetFallbackMetadataDescription_ReturnsOgDescription_WhenMetaDescriptionIsEmpty()
        {
            var result = _service.GetFallbackMetadataDescription(string.Empty, "fallback");

            Assert.That(result, Is.EqualTo("fallback"));
        }

        [Test]
        public void GetFallbackMetadataDescription_ReturnsEmpty_WhenBothAreNull()
        {
            var result = _service.GetFallbackMetadataDescription(null, null);

            Assert.That(result, Is.EqualTo(string.Empty));
        }

        [Test]
        public async Task GetItemsByTypesAsync_WithEmptyTypes_ReturnsEmptyList()
        {
            var result = await _service.GetItemsByTypesAsync(
                Array.Empty<Type>(),
                1,
                CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task GetItemsByTypesAsync_WithEmptyTypes_ReturnsReadOnlyList()
        {
            var result = await _service.GetItemsByTypesAsync(
                Array.Empty<Type>(),
                1,
                CancellationToken.None);

            Assert.That(result, Is.InstanceOf<IReadOnlyList<object>>());
        }

        [Test]
        public void GetCurrentChannelID_ReflectsChangedChannelID()
        {
            _websiteChannelContextMock.SetupGet(x => x.WebsiteChannelID).Returns(99);

            Assert.That(_service.GetCurrentChannelID(), Is.EqualTo(99));
        }

        [Test]
        public void GetCurrentSiteLegalNameAsync_ReflectsChangedChannelName()
        {
            _websiteChannelContextMock.SetupGet(x => x.WebsiteChannelName).Returns("OtherChannel");

            Assert.That(_service.GetCurrentSiteLegalNameAsync(), Is.EqualTo("OtherChannel"));
        }

        [Test]
        public void GetFallbackMetadataDescription_ReturnsWhitespacePrimary_WhenProvided()
        {
            // Implementation uses string.IsNullOrEmpty (not whitespace), so whitespace counts as present.
            var result = _service.GetFallbackMetadataDescription("   ", "fallback");

            Assert.That(result, Is.EqualTo("   "));
        }

        [Test]
        public void GetFallbackMetadataDescription_ReturnsEmpty_WhenPrimaryNullAndOgEmpty()
        {
            var result = _service.GetFallbackMetadataDescription(null, string.Empty);

            Assert.That(result, Is.EqualTo(string.Empty));
        }

        [Test]
        public void Constructor_AllowsNullCultureContext()
        {
            // Constructor does not null-check the culture context.
            Assert.DoesNotThrow(() => new ContentService(
                _executorMock.Object,
                _websiteChannelContextMock.Object,
                _cacheMock.Object,
                _contentRetrieverMock.Object,
                _executorMock.Object,
                _modelTypeMapperMock.Object,
                null!));
        }

        [Test]
        public async Task GetItemsChannelOnChannelIdAsync_ReturnsMappedResults()
        {
            var expected = new List<TestContentItem> { new(), new() };

            _executorMock
                .Setup(x => x.GetResult<TestContentItem>(
                    It.IsAny<ContentItemQueryBuilder>(),
                    It.IsAny<Func<IContentQueryDataContainer, TestContentItem>>(),
                    It.IsAny<ContentQueryExecutionOptions>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            var result = await _service.GetItemsChannelOnChannelIdAsync<TestContentItem>(
                cultureName: "en-US",
                configure: null,
                cancellationToken: CancellationToken.None,
                channelId: 5);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(2));
        }

        [Test]
        public async Task GetItemsChannelOnChannelIdAsync_WhenNoResults_ReturnsEmptyList()
        {
            _executorMock
                .Setup(x => x.GetResult<TestContentItem>(
                    It.IsAny<ContentItemQueryBuilder>(),
                    It.IsAny<Func<IContentQueryDataContainer, TestContentItem>>(),
                    It.IsAny<ContentQueryExecutionOptions>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(Enumerable.Empty<TestContentItem>());

            var result = await _service.GetItemsChannelOnChannelIdAsync<TestContentItem>(
                cultureName: "en-US",
                configure: null,
                cancellationToken: CancellationToken.None,
                channelId: 5);

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task GetItemsChannelOnChannelIdAsync_InvokesConfigureCallback()
        {
            var configureInvoked = false;

            _executorMock
                .Setup(x => x.GetResult<TestContentItem>(
                    It.IsAny<ContentItemQueryBuilder>(),
                    It.IsAny<Func<IContentQueryDataContainer, TestContentItem>>(),
                    It.IsAny<ContentQueryExecutionOptions>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(Enumerable.Empty<TestContentItem>());

            await _service.GetItemsChannelOnChannelIdAsync<TestContentItem>(
                cultureName: "en-GB",
                configure: _ => configureInvoked = true,
                cancellationToken: CancellationToken.None,
                channelId: 10);

            Assert.That(configureInvoked, Is.True);
        }

        [Test]
        public async Task GetItemsChannelOnChannelIdAsync_UsesPreviewFlag_FromWebsiteChannelContext()
        {
            _websiteChannelContextMock.SetupGet(x => x.IsPreview).Returns(true);

            ContentQueryExecutionOptions? capturedOptions = null;

            _executorMock
                .Setup(x => x.GetResult<TestContentItem>(
                    It.IsAny<ContentItemQueryBuilder>(),
                    It.IsAny<Func<IContentQueryDataContainer, TestContentItem>>(),
                    It.IsAny<ContentQueryExecutionOptions>(),
                    It.IsAny<CancellationToken>()))
                .Callback<ContentItemQueryBuilder, Func<IContentQueryDataContainer, TestContentItem>, ContentQueryExecutionOptions, CancellationToken>(
                    (_, _, options, _) => capturedOptions = options)
                .ReturnsAsync(Enumerable.Empty<TestContentItem>());

            await _service.GetItemsChannelOnChannelIdAsync<TestContentItem>(
                cultureName: "en-US",
                configure: null,
                cancellationToken: CancellationToken.None,
                channelId: 1);

            Assert.That(capturedOptions, Is.Not.Null);
            Assert.That(capturedOptions!.ForPreview, Is.True);
        }

        [Test]
        public async Task GetItemsChannelOnChannelIdAsync_PassesCancellationToken_ToExecutor()
        {
            var token = new CancellationTokenSource().Token;

            _executorMock
                .Setup(x => x.GetResult<TestContentItem>(
                    It.IsAny<ContentItemQueryBuilder>(),
                    It.IsAny<Func<IContentQueryDataContainer, TestContentItem>>(),
                    It.IsAny<ContentQueryExecutionOptions>(),
                    token))
                .ReturnsAsync(Enumerable.Empty<TestContentItem>());

            await _service.GetItemsChannelOnChannelIdAsync<TestContentItem>(
                cultureName: "en-US",
                configure: null,
                cancellationToken: token,
                channelId: 1);

            _executorMock.Verify(
                x => x.GetResult<TestContentItem>(
                    It.IsAny<ContentItemQueryBuilder>(),
                    It.IsAny<Func<IContentQueryDataContainer, TestContentItem>>(),
                    It.IsAny<ContentQueryExecutionOptions>(),
                    token),
                Times.Once);
        }

        /// <summary>
        /// Minimal stand-in model exposing the CONTENT_TYPE_NAME convention required by
        /// ContentService's internal GetContentTypeName&lt;T&gt;() reflection helper.
        /// </summary>
        private class TestContentItem
        {
            public const string CONTENT_TYPE_NAME = "Test.ContentItem";
        }
    }
}
