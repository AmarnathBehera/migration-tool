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
public class SpecialtiesPageControllerUnitTests
{
    private readonly Mock<ISpecialtiesPageService> _mockService = new();

    private SpecialtiesPageController _controller = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _controller = new SpecialtiesPageController(
            _mockService.Object);
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

        var expectedDto = new SpecialtiesPageDto();

        _mockService
            .Setup(x => x.GetSpecialtiesPageDtoAsync(
                cancellationToken))
            .ReturnsAsync(expectedDto);

        // Act
        var result = await _controller.Index(
            cancellationToken);

        // Assert
        _mockService.Verify(
            x => x.GetSpecialtiesPageDtoAsync(
                cancellationToken),
            Times.Once);

        AssertUtility.IsInstanceOf<ViewResult>(
            result,
            out var viewResult);

        Assert.Multiple(() =>
        {
            Assert.That(
                viewResult.ViewName,
                Is.EqualTo("~/Views/SpecialtiesPage/Index.cshtml"));

            Assert.That(
                viewResult.Model,
                Is.EqualTo(expectedDto));
        });
    }

    [Test]
    public async Task Index_CallsServiceExactlyOnce()
    {
        // Arrange
        _mockService
            .Setup(x => x.GetSpecialtiesPageDtoAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SpecialtiesPageDto());

        // Act
        await _controller.Index(
            CancellationToken.None);

        // Assert
        _mockService.Verify(
            x => x.GetSpecialtiesPageDtoAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task Index_ForwardsCancellationTokenToService()
    {
        // Arrange
        var cancellationToken = new CancellationTokenSource().Token;

        _mockService
            .Setup(x => x.GetSpecialtiesPageDtoAsync(
                cancellationToken))
            .ReturnsAsync(new SpecialtiesPageDto());

        // Act
        await _controller.Index(
            cancellationToken);

        // Assert
        _mockService.Verify(
            x => x.GetSpecialtiesPageDtoAsync(
                cancellationToken),
            Times.Once);
    }

    [Test]
    public async Task Index_ReturnsViewResult()
    {
        // Arrange
        _mockService
            .Setup(x => x.GetSpecialtiesPageDtoAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SpecialtiesPageDto());

        // Act
        var result = await _controller.Index(
            CancellationToken.None);

        // Assert
        Assert.That(
            result,
            Is.InstanceOf<ViewResult>());
    }

    [Test]
    public async Task Index_ReturnsModelFromService()
    {
        // Arrange
        var expectedDto = new SpecialtiesPageDto();

        _mockService
            .Setup(x => x.GetSpecialtiesPageDtoAsync(
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
}