
using BDO.GWT.GLOBAL.Tests.Utilities;
using BDO.GWT.Web.Controllers;
using BDO.GWT.Web.Core.DTOs;
using BDO.GWT.Web.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BDO.GWT.GLOBAL.Tests.Controllers;


public class CareerCategoryControllerUnitTests
{
    private readonly Mock<ICareerCategoryPageService> _mockService = new();

    private CareerCategoryController _controller = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _controller = new CareerCategoryController(_mockService.Object);
    }

    [SetUp]
    public void SetUp()
    {
        _mockService.Reset();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _controller.Dispose();
    }

    [Test]
    public async Task Index_ReturnsViewResult_WithCorrectModel()
    {
        // Arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var expectedDto = new CareerCategoryDto();

        _mockService
            .Setup(x => x.GetCareerCategoryDtoAsync(cancellationToken))
            .ReturnsAsync(expectedDto);

        // Act
        var result = await _controller.Index(cancellationToken);

        // Assert
        _mockService.Verify(
            x => x.GetCareerCategoryDtoAsync(cancellationToken),
            Times.Once);

        AssertUtility.IsInstanceOf<ViewResult>(result, out var viewResult);

        Assert.Multiple(() =>
        {
            Assert.That(viewResult.ViewName, Is.Null);
            Assert.That(viewResult.Model, Is.EqualTo(expectedDto));
        });
    }
}