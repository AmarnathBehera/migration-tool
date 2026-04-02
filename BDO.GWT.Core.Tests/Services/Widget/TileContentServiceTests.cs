using BDO.GWT.Core.Services.Widget;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.Tests.Helpers;
using CMS.ContentEngine;
using Kentico.Content.Web.Mvc;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Core.Tests.Services.Widget
{
    public class TileContentServiceTests
    {
        private Mock<IContentRetriever> _contentRetrieverMock;
        private TileContentService _service;

        [SetUp]
        public void Setup()
        {
            _contentRetrieverMock = new Mock<IContentRetriever>();
            _service = new TileContentService(_contentRetrieverMock.Object);
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

        //[Test]
        //public async Task GetTilePagesAsync_ReturnsMappedTilePages()
        //{
        //    // Arrange
        //    var guids = new List<Guid> { Guid.NewGuid() };

        //    var contentItems = new List<IContentItemFieldsSource>
        //    {
        //        PageMockHelper.CreateContentItem("Page 1"),
        //        PageMockHelper.CreateContentItem("Page 2")
        //    };

        //    _contentRetrieverMock
        //        .Setup(x => x.RetrieveContentOfContentTypesByGuids<IContentItemFieldsSource>(
        //            It.IsAny<string[]>(),
        //            It.IsAny<IEnumerable<Guid>>(),
        //            It.IsAny<CancellationToken>()))
        //        .ReturnsAsync(contentItems);

        //    // Act
        //    var result = await _service.GetTilePagesAsync(guids, CancellationToken.None);

        //    // Assert
        //    Assert.That(result, Is.Not.Null);
        //    Assert.That(result.Count, Is.EqualTo(2));

        //    var first = result[0];
        //    var second = result[1];

        //    Assert.That(first.Title, Is.EqualTo("Page 1"));
        //    Assert.That(second.Title, Is.EqualTo("Page 2"));
        //}

        //[Test]
        //public async Task GetTilePagesAsync_ReturnsEmpty_WhenNoContentFound()
        //{
        //    // Arrange
        //    var guids = new List<Guid> { Guid.NewGuid() };

        //    _contentRetrieverMock
        //        .Setup(x => x.RetrieveContentOfContentTypesByGuids<IContentItemFieldsSource>(
        //            It.IsAny<string[]>(),
        //            It.IsAny<IEnumerable<Guid>>(),
        //            It.IsAny<CancellationToken>()))
        //        .ReturnsAsync(new List<IContentItemFieldsSource>());

        //    // Act
        //    var result = await _service.GetTilePagesAsync(guids, CancellationToken.None);

        //    // Assert
        //    Assert.That(result, Is.Not.Null);
        //    Assert.That(result, Is.Empty);
        //}
    }
}