namespace BDO.GWT.GLOBAL.Tests.Controllers;

using BDO.GWT.Core.DTOs.Pages;
using BDO.GWT.Web.Controllers;
using BDO.GWT.Web.Core.Services;
using global::BDO.GWT.GLOBAL.Tests.Utilities;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using System.Threading;
using System.Threading.Tasks;

public class CareersControllerUnitTests
{
    private readonly Mock<ICareersPageService> _mockService = new();

    private CareersController _controller = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _controller = new CareersController(_mockService.Object);
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
    public async Task Index_ReturnsViewResult()
    {
        // Arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var expectedDto = new CareersPageDto();

        _mockService
            .Setup(x => x.GetCareersPageDtoAsync(cancellationToken))
            .ReturnsAsync(expectedDto);

        // Act
        var result = await _controller.Index(cancellationToken);

        // Assert
        _mockService.Verify(
            x => x.GetCareersPageDtoAsync(cancellationToken),
            Times.Once);

        AssertUtility.IsInstanceOf<ViewResult>(
            result,
            out var viewResult);

        Assert.That(viewResult.Model, Is.EqualTo(expectedDto));
    }
}