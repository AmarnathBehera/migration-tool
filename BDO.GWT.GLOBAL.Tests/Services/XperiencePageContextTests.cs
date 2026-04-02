using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.ReusableFieldsDTOs;
using BDO.GWT.GLOBAL.Services;
using CMS.ContentEngine;
using CMS.Websites.Routing;
using Kentico.Content.Web.Mvc;
using Kentico.PageBuilder.Web.Mvc;
using Kentico.PageBuilder.Web.Mvc.Internal;
using Microsoft.AspNetCore.Http;
using Moq;

namespace BDO.GWT.Core.Tests.Services
{
    public class XperiencePageContextTests
    {
        private Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private Mock<IWebPageDataContextRetriever> _webPageDataContextRetrieverMock;
        private Mock<IPageBuilderDataContextRetriever> _pageBuilderDataContextRetrieverMock;
        private Mock<IWebsiteChannelContext> _websiteChannelContextMock;
        private Mock<IContentQueryExecutor> _queryExecutorMock;
        private XperiencePageContext _service;

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
                _queryExecutorMock.Object
            );
        }

        [Test]
        public void IsInPreviewMode_ReturnsTrue_WhenChannelIsPreview()
        {
            _websiteChannelContextMock.Setup(x => x.IsPreview).Returns(true);

            Assert.That(_service.IsInPreviewMode, Is.True);
        }

        [Test]
        public void IsInPageBuilderMode_ReturnsTrue_WhenEditMode()
        {
            var httpContext = new DefaultHttpContext();
            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);
            _pageBuilderDataContextRetrieverMock.Setup(x => x.Retrieve()).Returns(new PageBuilderDataContext { EditMode = true });

            Assert.That(_service.IsInPageBuilderMode, Is.True);
        }

        [Test]
        public void IsInPageBuilderMode_ReturnsFalse_WhenNoHttpContext()
        {
            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns((Microsoft.AspNetCore.Http.HttpContext)null);

            Assert.That(_service.IsInPageBuilderMode, Is.False);
        }

        [Test]
        public async Task GetCurrentPageAsync_ReturnsNull_WhenNoContext()
        {
            _webPageDataContextRetrieverMock.Setup(x => x.TryRetrieve(out It.Ref<WebPageDataContext>.IsAny))
                .Returns(false);

            var result = await _service.GetCurrentPageAsync<object>();

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetCurrentPageMetadataAsync_ReturnsEmptyDto_WhenNoContext()
        {
            _webPageDataContextRetrieverMock.Setup(x => x.TryRetrieve(out It.Ref<WebPageDataContext>.IsAny))
                .Returns(false);

            var result = await _service.GetCurrentPageMetadataAsync();


            Assert.That(result, Is.Not.Null);
            Assert.That(result.MetaData, Is.Null.Or.InstanceOf<MetaDataDto>());
            Assert.That(result.OGMetaData, Is.Null.Or.InstanceOf<OGMetaDataDto>());
            Assert.That(result.SEOItemsDto, Is.Null.Or.InstanceOf<SearchAndSEODto>());
        }
        
    }
}