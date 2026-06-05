using BDO.GWT.GLOBAL.Tests.Utilities;
using BDO.GWT.Web.Core.DTOs;
using BDO.GWT.Web.Core.LiveSite.Controllers;
using BDO.GWT.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using System.Threading;
using System.Threading.Tasks;
using BDO.GWT.Core.DTOs;

namespace BDO.GWT.GLOBAL.Tests.Controllers;

[TestFixture]
public class SectionSpecialtiesControllerUnitTests
{
    private readonly Mock<ISectionSpecialtiesService> _mockService = new();

    private SectionSpecialtiesController _controller = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _controller = new SectionSpecialtiesController(
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
    public async Task Index_ReturnsViewResult_WithCorrectModel()
    {
        // Arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var expectedDto = new SectionSpecialtiesPageDto();

        _mockService
            .Setup(x => x.GetSectionSpecialtiesDtoAsync(
                cancellationToken))
            .ReturnsAsync(expectedDto);

        // Act
        var result = await _controller.Index(
            cancellationToken);

        // Assert
        _mockService.Verify(
            x => x.GetSectionSpecialtiesDtoAsync(
                cancellationToken),
            Times.Once);

        AssertUtility.IsInstanceOf<ViewResult>(
            result,
            out var viewResult);

        Assert.That(
            viewResult.Model,
            Is.EqualTo(expectedDto));
    }
}