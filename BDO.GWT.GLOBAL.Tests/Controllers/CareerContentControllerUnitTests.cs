using BDO.GWT.Core.DTOs;
using BDO.GWT.GLOBAL.Tests.Utilities;
using BDO.GWT.Web.Controllers;
using BDO.GWT.Web.Core.DTOs;
using BDO.GWT.Web.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;


namespace BDO.GWT.GLOBAL.Tests.Controllers;


public class CareerContentControllerUnitTests
{
    private readonly Mock<ICareerContentPageService> _mockService = new();

    private CareerContentController _controller = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _controller = new CareerContentController(_mockService.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _mockService.Reset();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _controller.Dispose();
    }

    [Test]
    public async Task Index_ReturnsViewResult_WithCorrectViewAndModel()
    {
        // Arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var expectedDto = new CareerDto();

        _mockService
            .Setup(x => x.GetCareerContentDtoAsync(cancellationToken))
            .ReturnsAsync(expectedDto);

        // Act
        var result = await _controller.Index(cancellationToken);

        // Assert
        _mockService.Verify(
            x => x.GetCareerContentDtoAsync(cancellationToken),
            Times.Once);

        AssertUtility.IsInstanceOf<ViewResult>(
            result,
            out var viewResult);

        Assert.Multiple(() =>
        {
            Assert.That(
                viewResult.ViewName,
                Is.EqualTo("~/Views/CareerContent/Index.cshtml"));

            Assert.That(
                viewResult.Model,
                Is.EqualTo(expectedDto));
        });
    }
}