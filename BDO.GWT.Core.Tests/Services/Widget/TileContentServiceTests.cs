using BDO.GWT.Core.Services.Widget;
using CMS.Websites;
using Kentico.Content.Web.Mvc;
using Moq;

namespace BDO.GWT.Core.Tests.Services.Widget
{
    public class TileContentServiceTests
    {
        private Mock<IContentRetriever> _contentRetrieverMock;
        private Mock<IWebPageUrlRetriever> _urlRetrieverMock;
        private TileContentService _service;

        [SetUp]
        public void Setup()
        {
            _contentRetrieverMock = new Mock<IContentRetriever>();
            _urlRetrieverMock = new Mock<IWebPageUrlRetriever>();
            _service = new TileContentService(_contentRetrieverMock.Object, _urlRetrieverMock.Object);
        }

        [Test]
        public async Task GetTilePagesAsync_ReturnsEmpty_WhenGuidsIsNull()
        {
            // Act
            var result = await _service.GetTilePagesAsync(null, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task GetTilePagesAsync_ReturnsEmpty_WhenGuidsIsEmpty()
        {
            // Act
            var result = await _service.GetTilePagesAsync(new List<Guid>(), CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Empty);
        }
    }
}