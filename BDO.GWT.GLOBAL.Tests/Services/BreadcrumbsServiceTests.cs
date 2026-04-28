using BDO.GWT.Core;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.ReusableFieldsDTOs;
using BDO.GWT.Core.Services;
using BDO.GWT.Web.Core.LiveSite.Services;
using CMS.Websites;
using Kentico.Content.Web.Mvc;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.GLOBAL.Tests.Services
{
    public class BreadcrumbsServiceTests
    {
        private Mock<IWebPageDataContextRetriever> _dataContextRetrieverMock = null!;
        private Mock<IWebPageUrlRetriever> _pageUrlRetrieverMock = null!;
        private Mock<IHomePageService> _homePageServiceMock = null!;
        private Mock<IPageContext> _pageContextMock = null!;
        private BreadcrumbsService _service = null!;

        [SetUp]
        public void SetUp()
        {
            _dataContextRetrieverMock = new Mock<IWebPageDataContextRetriever>();
            _pageUrlRetrieverMock = new Mock<IWebPageUrlRetriever>();
            _homePageServiceMock = new Mock<IHomePageService>();
            _pageContextMock = new Mock<IPageContext>();

            _service = new BreadcrumbsService(
                _dataContextRetrieverMock.Object,
                _pageUrlRetrieverMock.Object,
                _homePageServiceMock.Object,
                _pageContextMock.Object
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
        public async Task GetAsync_WhenContextNotAvailable_DoesNotCallHomePageService()
        {
            _dataContextRetrieverMock
                .Setup(r => r.TryRetrieve(out It.Ref<WebPageDataContext>.IsAny))
                .Returns(false);

            await _service.GetAsync(CancellationToken.None);

            _homePageServiceMock.Verify(
                s => s.GetHomePageDtoAsync(It.IsAny<BDO.HomePage?>(), It.IsAny<CancellationToken>()),
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
                r => r.Retrieve(It.IsAny<int>(), It.IsAny<string>()),
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
