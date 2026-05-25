
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.Services;
using BDO.GWT.GLOBAL.Tests.Utilities;
using BDO.GWT.Web.Controllers;
using BDO.GWT.Web.Core.DTOs;
using BDO.GWT.Web.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.GLOBAL.Tests.Controllers;


public class CareerDetailControllerUnitTests
{
    private readonly Mock<ICareerDetailPageService> _mockService = new();

    private readonly Mock<IPageContext> _mockPageContext = new();

    private CareerDetailController _controller = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _controller = new CareerDetailController(
            _mockService.Object,
            _mockPageContext.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _mockService.Reset();
        _mockPageContext.Reset();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _controller.Dispose();
    }

    [Test]
    public async Task Index_ReturnsViewResult()
    {
        // Arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var expectedDto = new CareerPageDto();

        _mockService
            .Setup(x => x.GetCareerPageDtoAsync(cancellationToken))
            .ReturnsAsync(expectedDto);

        // Act
        var result = await _controller.Index(cancellationToken);

        // Assert
        _mockService.Verify(
            x => x.GetCareerPageDtoAsync(cancellationToken),
            Times.Once);

        AssertUtility.IsInstanceOf<ViewResult>(
            result,
            out var viewResult);

        Assert.That(
            viewResult.Model,
            Is.EqualTo(expectedDto));
    }
}