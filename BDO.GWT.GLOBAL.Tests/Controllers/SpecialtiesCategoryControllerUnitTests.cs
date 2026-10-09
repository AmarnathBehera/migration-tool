using BDO.GWT.Core.DTOs.Pages;
using BDO.GWT.Core.Services;
using BDO.GWT.GLOBAL.Tests.Utilities;
using BDO.GWT.Web.Core.DTOs;
using BDO.GWT.Web.Core.Services;
using GWTXbyKWeb.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.GLOBAL.Tests.Controllers;

[TestFixture]
public class SpecialtiesCategoryControllerUnitTests
{
    private readonly Mock<ISpecialtiesCategoryPageService> _mockService = new();

    private SpecialtiesCategoryController _controller = null!;

    [SetUp]
    public void OneTimeSetUp()
    {
        _controller = new SpecialtiesCategoryController(
            _mockService.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _controller.Dispose();
        _mockService.Reset();
    }

    [Test]
    public async Task Index_ReturnsViewResult_WithCorrectViewAndModel()
    {
        // Arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var expectedDto = new SpecialtiesCategoryPageDto();

        _mockService
            .Setup(x => x.GetSpecialtiesCategoryPageDtoAsync(
                cancellationToken))
            .ReturnsAsync(expectedDto);

        // Act
        var result = await _controller.Index(
            cancellationToken);

        // Assert
        _mockService.Verify(
            x => x.GetSpecialtiesCategoryPageDtoAsync(
                cancellationToken),
            Times.Once);

        AssertUtility.IsInstanceOf<ViewResult>(
            result,
            out var viewResult);

        Assert.Multiple(() =>
        {
            Assert.That(
                viewResult.ViewName,
                Is.EqualTo("~/Views/SpecialtiesCategory/Index.cshtml"));

            Assert.That(
                viewResult.Model,
                Is.EqualTo(expectedDto));
        });
    }

    [Test]
    public async Task Index_ForwardsCancellationTokenToService()
    {
        // Arrange
        var cancellationToken = new CancellationTokenSource().Token;

        _mockService
            .Setup(x => x.GetSpecialtiesCategoryPageDtoAsync(
                cancellationToken))
            .ReturnsAsync(new SpecialtiesCategoryPageDto());

        // Act
        await _controller.Index(
            cancellationToken);

        // Assert
        _mockService.Verify(
            x => x.GetSpecialtiesCategoryPageDtoAsync(
                cancellationToken),
            Times.Once);
    }

    [Test]
    public async Task Index_ReturnsViewResult()
    {
        // Arrange
        _mockService
            .Setup(x => x.GetSpecialtiesCategoryPageDtoAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SpecialtiesCategoryPageDto());

        // Act
        var result = await _controller.Index(
            CancellationToken.None);

        // Assert
        Assert.That(
            result,
            Is.InstanceOf<ViewResult>());
    }

    [Test]
    public async Task Index_ReturnsModelReturnedByService()
    {
        // Arrange
        var expectedDto = new SpecialtiesCategoryPageDto();

        _mockService
            .Setup(x => x.GetSpecialtiesCategoryPageDtoAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedDto);

        // Act
        var result = await _controller.Index(
            CancellationToken.None);

        // Assert
        AssertUtility.IsInstanceOf<ViewResult>(
            result,
            out var viewResult);

        Assert.That(
            viewResult.Model,
            Is.SameAs(expectedDto));
    }

    [Test]
    public async Task Index_CallsServiceExactlyOnce()
    {
        // Arrange
        _mockService
            .Setup(x => x.GetSpecialtiesCategoryPageDtoAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SpecialtiesCategoryPageDto());

        // Act
        await _controller.Index(
            CancellationToken.None);

        // Assert
        _mockService.Verify(
            x => x.GetSpecialtiesCategoryPageDtoAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}