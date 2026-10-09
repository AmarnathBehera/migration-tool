using BDO.GWT.Core.Data.Queries;
using BDO.GWT.Core.DTOs.Cards;
using BDO.GWT.Core.Services;
using BDO.GWT.Web.Core.LiveSite.Controllers.API;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BDO.GWT.Core.Tests.Controllers.API;

[TestFixture]
public class RelatedContentCarouselControllerTests
{
    private Mock<IRelatedContentCarouselService> _relatedContentCarouselServiceMock;
    private RelatedContentCarouselController _controller;

    [SetUp]
    public void SetUp()
    {
        _relatedContentCarouselServiceMock = new Mock<IRelatedContentCarouselService>();

        _controller = new RelatedContentCarouselController(
            _relatedContentCarouselServiceMock.Object);
    }

    [Test]
    public async Task Items_ShouldReturnOkResult_WhenServiceReturnsItems()
    {
        // Arrange
        var query = new RelatedContentCardsQuery();

        var expectedItems = new List<RelatedContentCardDto>
        {
            new RelatedContentCardDto(),
            new RelatedContentCardDto()
        };

        var cancellationToken = CancellationToken.None;

        _relatedContentCarouselServiceMock
            .Setup(x => x.GetAPIFilteredItemsAsync(
                It.IsAny<RelatedContentCardsQuery>(),
                cancellationToken))
            .ReturnsAsync(expectedItems);

        SetControllerContext();

        // Act
        var result = await _controller.Items(query, cancellationToken);

        // Assert
        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());

        var okResult = (OkObjectResult)result.Result;

        Assert.That(okResult.Value, Is.SameAs(expectedItems));
    }

    [Test]
    public async Task Items_ShouldCallService_WithProvidedQueryAndCancellationToken()
    {
        // Arrange
        var query = new RelatedContentCardsQuery();
        var cancellationToken = new CancellationTokenSource().Token;

        var expectedItems = new List<RelatedContentCardDto>();

        _relatedContentCarouselServiceMock
            .Setup(x => x.GetAPIFilteredItemsAsync(
                It.IsAny<RelatedContentCardsQuery>(),
                cancellationToken))
            .ReturnsAsync(expectedItems);

        SetControllerContext();

        // Act
        await _controller.Items(query, cancellationToken);

        // Assert
        _relatedContentCarouselServiceMock.Verify(
            x => x.GetAPIFilteredItemsAsync(
                query,
                cancellationToken),
            Times.Once);
    }

    [Test]
    public async Task Items_ShouldReturnItemsReturnedByService()
    {
        // Arrange
        var query = new RelatedContentCardsQuery();

        var expectedItems = new List<RelatedContentCardDto>
        {
            new RelatedContentCardDto(),
            new RelatedContentCardDto(),
            new RelatedContentCardDto()
        };

        var cancellationToken = CancellationToken.None;

        _relatedContentCarouselServiceMock
            .Setup(x => x.GetAPIFilteredItemsAsync(
                It.IsAny<RelatedContentCardsQuery>(),
                cancellationToken))
            .ReturnsAsync(expectedItems);

        SetControllerContext();

        // Act
        var result = await _controller.Items(query, cancellationToken);

        // Assert
        var okResult = result.Result as OkObjectResult;

        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult.Value, Is.EqualTo(expectedItems));
    }

    [Test]
    public async Task Items_ShouldReturnOkResult_WhenServiceReturnsEmptyCollection()
    {
        // Arrange
        var query = new RelatedContentCardsQuery();

        var expectedItems = new List<RelatedContentCardDto>();

        var cancellationToken = CancellationToken.None;

        _relatedContentCarouselServiceMock
            .Setup(x => x.GetAPIFilteredItemsAsync(
                It.IsAny<RelatedContentCardsQuery>(),
                cancellationToken))
            .ReturnsAsync(expectedItems);

        SetControllerContext();

        // Act
        var result = await _controller.Items(query, cancellationToken);

        // Assert
        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());

        var okResult = (OkObjectResult)result.Result;

        Assert.That(okResult.Value, Is.SameAs(expectedItems));
    }

    [Test]
    public async Task Items_ShouldCallService_OnlyOnce()
    {
        // Arrange
        var query = new RelatedContentCardsQuery();
        var cancellationToken = CancellationToken.None;

        _relatedContentCarouselServiceMock
            .Setup(x => x.GetAPIFilteredItemsAsync(
                It.IsAny<RelatedContentCardsQuery>(),
                cancellationToken))
            .ReturnsAsync(new List<RelatedContentCardDto>());

        SetControllerContext();

        // Act
        await _controller.Items(query, cancellationToken);

        // Assert
        _relatedContentCarouselServiceMock.Verify(
            x => x.GetAPIFilteredItemsAsync(
                It.IsAny<RelatedContentCardsQuery>(),
                cancellationToken),
            Times.Once);
    }

    private void SetControllerContext()
    {
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }
}