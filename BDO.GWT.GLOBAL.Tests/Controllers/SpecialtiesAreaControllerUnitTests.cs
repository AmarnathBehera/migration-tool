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
public class SpecialtiesAreaControllerUnitTests
{
    private Mock<ISpecialtiesAreaPageService> _mockService = null!;

    private SpecialtiesAreaController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _mockService = new Mock<ISpecialtiesAreaPageService>();

        _controller = new SpecialtiesAreaController(
            _mockService.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _controller.Dispose();
        _mockService.Reset();
    }

    [Test]
    public async Task Index_ReturnsViewResult()
    {
        // Arrange
        var dto = new SpecialtiesAreaPageDto();

        _mockService
            .Setup(x => x.GetSpecialtiesAreaPageDtoAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        // Act
        var result = await _controller.Index(
            CancellationToken.None);

        // Assert
        Assert.That(result, Is.InstanceOf<ViewResult>());
    }

    [Test]
    public async Task Index_ReturnsExpectedViewName()
    {
        // Arrange
        var dto = new SpecialtiesAreaPageDto();

        _mockService
            .Setup(x => x.GetSpecialtiesAreaPageDtoAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        // Act
        var result = await _controller.Index(
            CancellationToken.None);

        // Assert
        AssertUtility.IsInstanceOf<ViewResult>(
            result,
            out var viewResult);

        Assert.That(
            viewResult.ViewName,
            Is.EqualTo("~/Views/SpecialtiesArea/Index.cshtml"));
    }

    [Test]
    public async Task Index_ReturnsModelFromService()
    {
        // Arrange
        var expectedDto = new SpecialtiesAreaPageDto();

        _mockService
            .Setup(x => x.GetSpecialtiesAreaPageDtoAsync(
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
            Is.EqualTo(expectedDto));
    }

    [Test]
    public async Task Index_CallsServiceOnce()
    {
        // Arrange
        _mockService
            .Setup(x => x.GetSpecialtiesAreaPageDtoAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SpecialtiesAreaPageDto());

        // Act
        await _controller.Index(
            CancellationToken.None);

        // Assert
        _mockService.Verify(
            x => x.GetSpecialtiesAreaPageDtoAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task Index_ForwardsCancellationTokenToService()
    {
        // Arrange
        var token = new CancellationTokenSource().Token;

        _mockService
            .Setup(x => x.GetSpecialtiesAreaPageDtoAsync(token))
            .ReturnsAsync(new SpecialtiesAreaPageDto());

        // Act
        await _controller.Index(token);

        // Assert
        _mockService.Verify(
            x => x.GetSpecialtiesAreaPageDtoAsync(token),
            Times.Once);
    }
}