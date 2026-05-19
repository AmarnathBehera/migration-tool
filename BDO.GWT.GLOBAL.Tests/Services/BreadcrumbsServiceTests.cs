using BDO.GWT.Core;
using BDO.GWT.Core.Services;
using CMS.Websites;
using Kentico.Content.Web.Mvc;
using Moq;

namespace BDO.GWT.GLOBAL.Tests.Services
{
    public class BreadcrumbsServiceTests
    {
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
    }
}
