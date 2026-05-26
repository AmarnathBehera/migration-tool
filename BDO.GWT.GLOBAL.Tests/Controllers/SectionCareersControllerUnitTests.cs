namespace BDO.GWT.GLOBAL.Tests.Controllers;

using BDO.GWT.Core.DTOs;
using global::BDO.GWT.GLOBAL.Tests.Utilities;
using global::BDO.GWT.Web.Controllers;
using global::BDO.GWT.Web.Core.DTOs;
using global::BDO.GWT.Web.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using System.Threading;
using System.Threading.Tasks;

public class CareerControllerUnitTests
{
    private readonly Mock<ICareerPageService> _mockService = new();

    private CareerController _controller = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _controller = new CareerController(_mockService.Object);
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
            .Setup(x => x.GetCareerDtoAsync(cancellationToken))
            .ReturnsAsync(expectedDto);

        // Act
        var result = await _controller.Index(cancellationToken);

        // Assert
        _mockService.Verify(
            x => x.GetCareerDtoAsync(cancellationToken),
            Times.Once);

        AssertUtility.IsInstanceOf<ViewResult>(
            result,
            out var viewResult);

        Assert.Multiple(() =>
        {
            Assert.That(
                viewResult.ViewName,
                Is.EqualTo("~/Views/Career/Index.cshtml"));

            Assert.That(
                viewResult.Model,
                Is.EqualTo(expectedDto));
        });
    }
}


