using BDO.GWT.Core;
using BDO.GWT.Core.DTOs.Pages;
using BDO.GWT.Core.Services;
using BDO.GWT.Web.Controllers;
using BDO.GWT.Web.Core.DTOs;
using Kentico.Content.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace BDO.GWT.GLOBAL.Tests.Controllers;

[TestFixture]
public class BusinessStrategyControllerTests
{
    private Mock<IContentService> _contentServiceMock = null!;
    private Mock<IBusinessStrategyService> _businessStrategyServiceMock = null!;
    private BusinessStrategyController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _contentServiceMock = new Mock<IContentService>();
        _businessStrategyServiceMock = new Mock<IBusinessStrategyService>();
        _controller = new BusinessStrategyController(
            _contentServiceMock.Object,
            _businessStrategyServiceMock.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _controller?.Dispose();
    }

    #region Null Page Tests

    [Test]
    public async Task Index_WhenCurrentPageIsNull_ReturnsNotFound()
    {
        // Arrange
        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<BusinessStrategy>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((BusinessStrategy?)null);

        // Act
        var result = await _controller.Index(CancellationToken.None);

        // Assert
        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task Index_WhenCurrentPageIsNull_DoesNotCallBusinessStrategyService()
    {
        // Arrange
        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<BusinessStrategy>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((BusinessStrategy?)null);

        // Act
        await _controller.Index(CancellationToken.None);

        // Assert
        _businessStrategyServiceMock.Verify(
            x => x.GetBusinessStrategyDtoAsync(It.IsAny<BusinessStrategy>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task Index_WhenCurrentPageIsNull_DoesNotCallContentServiceAgain()
    {
        // Arrange
        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<BusinessStrategy>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((BusinessStrategy?)null);

        // Act
        await _controller.Index(CancellationToken.None);

        // Assert
        _contentServiceMock.Verify(
            x => x.RetrieveCurrentPageAsync<BusinessStrategy>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region Happy Path Tests

    [Test]
    public async Task Index_WhenCurrentPageExists_ReturnsViewResult()
    {
        // Arrange
        var businessStrategy = CreateMockBusinessStrategy();
        var expectedDto = new BusinessStrategyDto();

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<BusinessStrategy>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(businessStrategy);

        _businessStrategyServiceMock
            .Setup(x => x.GetBusinessStrategyDtoAsync(businessStrategy, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedDto);

        // Act
        var result = await _controller.Index(CancellationToken.None);

        // Assert
        Assert.That(result, Is.InstanceOf<ViewResult>());
    }

    [Test]
    public async Task Index_WhenCurrentPageExists_ReturnsViewWithCorrectModel()
    {
        // Arrange
        var businessStrategy = CreateMockBusinessStrategy();
        var expectedDto = new BusinessStrategyDto
        {
            Hero = new HeroDto { Title = "Test Hero" }
        };

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<BusinessStrategy>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(businessStrategy);

        _businessStrategyServiceMock
            .Setup(x => x.GetBusinessStrategyDtoAsync(businessStrategy, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedDto);

        // Act
        var result = await _controller.Index(CancellationToken.None);

        // Assert
        var viewResult = result as ViewResult;
        Assert.Multiple(() =>
        {
            Assert.That(viewResult, Is.Not.Null);
            Assert.That(viewResult!.Model, Is.EqualTo(expectedDto));
        });
    }

    [Test]
    public async Task Index_WhenCurrentPageExists_ReturnsCorrectViewPath()
    {
        // Arrange
        var businessStrategy = CreateMockBusinessStrategy();

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<BusinessStrategy>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(businessStrategy);

        _businessStrategyServiceMock
            .Setup(x => x.GetBusinessStrategyDtoAsync(businessStrategy, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BusinessStrategyDto());

        // Act
        var result = await _controller.Index(CancellationToken.None);

        // Assert
        var viewResult = result as ViewResult;
        Assert.That(viewResult!.ViewName, Is.EqualTo("~/Views/BusinessStrategy/Index.cshtml"));
    }

    [Test]
    public async Task Index_WhenCurrentPageExists_CallsBusinessStrategyService()
    {
        // Arrange
        var businessStrategy = CreateMockBusinessStrategy();

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<BusinessStrategy>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(businessStrategy);

        _businessStrategyServiceMock
            .Setup(x => x.GetBusinessStrategyDtoAsync(businessStrategy, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BusinessStrategyDto());

        // Act
        await _controller.Index(CancellationToken.None);

        // Assert
        _businessStrategyServiceMock.Verify(
            x => x.GetBusinessStrategyDtoAsync(businessStrategy, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region Service Parameters Tests

    [Test]
    public async Task Index_CallsContentServiceWithCorrectParameters()
    {
        // Arrange
        var businessStrategy = CreateMockBusinessStrategy();

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<BusinessStrategy>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(businessStrategy);

        _businessStrategyServiceMock
            .Setup(x => x.GetBusinessStrategyDtoAsync(businessStrategy, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BusinessStrategyDto());

        // Act
        await _controller.Index(CancellationToken.None);

        // Assert
        _contentServiceMock.Verify(
            x => x.RetrieveCurrentPageAsync<BusinessStrategy>(
                It.Is<RetrieveCurrentPageParameters>(p => p.LinkedItemsMaxLevel == 4),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task Index_PassesCancellationTokenToContentService()
    {
        // Arrange
        var businessStrategy = CreateMockBusinessStrategy();
        var cancellationToken = new CancellationToken();

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<BusinessStrategy>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(businessStrategy);

        _businessStrategyServiceMock
            .Setup(x => x.GetBusinessStrategyDtoAsync(businessStrategy, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BusinessStrategyDto());

        // Act
        await _controller.Index(cancellationToken);

        // Assert
        _contentServiceMock.Verify(
            x => x.RetrieveCurrentPageAsync<BusinessStrategy>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                cancellationToken),
            Times.Once);
    }

    [Test]
    public async Task Index_PassesCancellationTokenToService()
    {
        // Arrange
        var businessStrategy = CreateMockBusinessStrategy();
        var cancellationToken = new CancellationToken();

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<BusinessStrategy>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(businessStrategy);

        _businessStrategyServiceMock
            .Setup(x => x.GetBusinessStrategyDtoAsync(businessStrategy, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BusinessStrategyDto());

        // Act
        await _controller.Index(cancellationToken);

        // Assert
        _businessStrategyServiceMock.Verify(
            x => x.GetBusinessStrategyDtoAsync(businessStrategy, cancellationToken),
            Times.Once);
    }

    #endregion

    #region Async and Cancellation Tests

    [Test]
    public async Task Index_RespectsAsyncCancellation_WhenContentServiceCancelled()
    {
        // Arrange
        var cancellationToken = new CancellationToken(canceled: true);

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<BusinessStrategy>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        // Act & Assert
        Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await _controller.Index(cancellationToken));
    }

    [Test]
    public async Task Index_RespectsAsyncCancellation_WhenServiceCancelled()
    {
        // Arrange
        var businessStrategy = CreateMockBusinessStrategy();
        var cancellationToken = new CancellationToken(canceled: true);

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<BusinessStrategy>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(businessStrategy);

        _businessStrategyServiceMock
            .Setup(x => x.GetBusinessStrategyDtoAsync(businessStrategy, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        // Act & Assert
        Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await _controller.Index(cancellationToken));
    }

    #endregion

    #region Exception Handling Tests

    [Test]
    public async Task Index_WhenContentServiceThrowsException_PropagatesException()
    {
        // Arrange
        var exceptionMessage = "Content service error";

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<BusinessStrategy>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException(exceptionMessage));

        // Act & Assert
        var exception = Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _controller.Index(CancellationToken.None));
        Assert.That(exception!.Message, Is.EqualTo(exceptionMessage));
    }

    [Test]
    public async Task Index_WhenServiceThrowsException_PropagatesException()
    {
        // Arrange
        var businessStrategy = CreateMockBusinessStrategy();
        var exceptionMessage = "Service error";

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<BusinessStrategy>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(businessStrategy);

        _businessStrategyServiceMock
            .Setup(x => x.GetBusinessStrategyDtoAsync(businessStrategy, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException(exceptionMessage));

        // Act & Assert
        var exception = Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _controller.Index(CancellationToken.None));
        Assert.That(exception!.Message, Is.EqualTo(exceptionMessage));
    }

    [Test]
    public async Task Index_WhenServiceThrowsArgumentException_PropagatesArgumentException()
    {
        // Arrange
        var businessStrategy = CreateMockBusinessStrategy();

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<BusinessStrategy>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(businessStrategy);

        _businessStrategyServiceMock
            .Setup(x => x.GetBusinessStrategyDtoAsync(businessStrategy, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentException("Invalid argument"));

        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(async () =>
            await _controller.Index(CancellationToken.None));
    }

    #endregion

    #region View Result Tests

    [Test]
    public async Task Index_ViewResult_HasCorrectViewName()
    {
        // Arrange
        var businessStrategy = CreateMockBusinessStrategy();
        var expectedViewName = "~/Views/BusinessStrategy/Index.cshtml";

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<BusinessStrategy>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(businessStrategy);

        _businessStrategyServiceMock
            .Setup(x => x.GetBusinessStrategyDtoAsync(businessStrategy, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BusinessStrategyDto());

        // Act
        var result = await _controller.Index(CancellationToken.None);

        // Assert
        var viewResult = (ViewResult)result;
        Assert.That(viewResult.ViewName, Is.EqualTo(expectedViewName));
    }

    [Test]
    public async Task Index_ViewResult_ModelIsNotNull()
    {
        // Arrange
        var businessStrategy = CreateMockBusinessStrategy();
        var expectedDto = new BusinessStrategyDto
        {
            Hero = new HeroDto { Title = "Hero Title" }
        };

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<BusinessStrategy>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(businessStrategy);

        _businessStrategyServiceMock
            .Setup(x => x.GetBusinessStrategyDtoAsync(businessStrategy, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedDto);

        // Act
        var result = await _controller.Index(CancellationToken.None);

        // Assert
        var viewResult = (ViewResult)result;
        Assert.That(viewResult.Model, Is.Not.Null);
        Assert.That(viewResult.Model, Is.InstanceOf<BusinessStrategyDto>());
    }

    [Test]
    public async Task Index_ViewResult_ModelMatchesServiceResult()
    {
        // Arrange
        var businessStrategy = CreateMockBusinessStrategy();
        var expectedDto = new BusinessStrategyDto
        {
            Hero = new HeroDto
            {
                Title = "Business Strategy",
                Description = "Description Text"
            }
        };

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<BusinessStrategy>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(businessStrategy);

        _businessStrategyServiceMock
            .Setup(x => x.GetBusinessStrategyDtoAsync(businessStrategy, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedDto);

        // Act
        var result = await _controller.Index(CancellationToken.None);

        // Assert
        var viewResult = (ViewResult)result;
        var model = (BusinessStrategyDto)viewResult.Model!;
        Assert.Multiple(() =>
        {
            Assert.That(model.Hero.Title, Is.EqualTo("Business Strategy"));
            Assert.That(model.Hero.Description, Is.EqualTo("Description Text"));
        });
    }

    #endregion

    #region Multiple Invocations Tests

    [Test]
    public async Task Index_CalledMultipleTimes_WorksCorrectly()
    {
        // Arrange
        var businessStrategy = CreateMockBusinessStrategy();

        _contentServiceMock
            .Setup(x => x.RetrieveCurrentPageAsync<BusinessStrategy>(
                It.IsAny<RetrieveCurrentPageParameters>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(businessStrategy);

        _businessStrategyServiceMock
            .Setup(x => x.GetBusinessStrategyDtoAsync(businessStrategy, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BusinessStrategyDto());

        // Act
        var result1 = await _controller.Index(CancellationToken.None);
        var result2 = await _controller.Index(CancellationToken.None);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result1, Is.InstanceOf<ViewResult>());
            Assert.That(result2, Is.InstanceOf<ViewResult>());
        });

        _businessStrategyServiceMock.Verify(
            x => x.GetBusinessStrategyDtoAsync(businessStrategy, It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }

    #endregion

    #region Helper Methods

    private static BusinessStrategy CreateMockBusinessStrategy()
    {
        return new BusinessStrategy
        {
            HeroTitle = "Test Business Strategy",
            HeroDescription = "Test Description",
            HeroLayout = "default",
        };
    }

    #endregion
}
