using BDO.GWT.Core;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Pages;
using BDO.GWT.Core.Services;
using BDO.GWT.Web.Controllers;
using BDO.GWT.Web.Core.DTOs;
using Kentico.Content.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.GLOBAL.Tests.Controllers;

public class GlobalLocationsControllerTests
{
    private Mock<IContentService> _contentServiceMock = null!;
    private Mock<IGlobalLocationsPageService> _globalLocationsPageServiceMock = null!;
    private GlobalLocationsController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _contentServiceMock = new Mock<IContentService>();
        _globalLocationsPageServiceMock = new Mock<IGlobalLocationsPageService>();
        _controller = new GlobalLocationsController(_contentServiceMock.Object, _globalLocationsPageServiceMock.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _controller.Dispose();
    }

    [Test]
    public async Task Index_ReturnsNotFound_WhenCurrentPageIsNull()
    {
        _contentServiceMock
            .Setup(s => s.RetrieveCurrentPageAsync<GlobalLocations>(It.IsAny<RetrieveCurrentPageParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((GlobalLocations?)null);

        var result = await _controller.Index(CancellationToken.None);

        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task Index_DoesNotCallPageService_WhenCurrentPageIsNull()
    {
        _contentServiceMock
            .Setup(s => s.RetrieveCurrentPageAsync<GlobalLocations>(It.IsAny<RetrieveCurrentPageParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((GlobalLocations?)null);

        await _controller.Index(CancellationToken.None);

        _globalLocationsPageServiceMock.Verify(
            s => s.GetGlobalLocationsPageDtoAsync(It.IsAny<GlobalLocations>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task Index_ReturnsViewResult_WithGlobalLocationsDto()
    {
        var page = new GlobalLocations();
        var expectedDto = new GlobalLocationsDto
        {
            Hero = new HeroDto { Title = "Global Locations" },
            MainGrid = new GlobalLocationsGridDto()
        };

        _contentServiceMock
            .Setup(s => s.RetrieveCurrentPageAsync<GlobalLocations>(It.IsAny<RetrieveCurrentPageParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(page);

        _globalLocationsPageServiceMock
            .Setup(s => s.GetGlobalLocationsPageDtoAsync(page, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedDto);

        var result = await _controller.Index(CancellationToken.None);

        Assert.That(result, Is.InstanceOf<ViewResult>());
        var viewResult = (ViewResult)result;
        Assert.That(viewResult.Model, Is.EqualTo(expectedDto));
    }

    [Test]
    public async Task Index_CallsPageService_WithRetrievedPage()
    {
        var page = new GlobalLocations();

        _contentServiceMock
            .Setup(s => s.RetrieveCurrentPageAsync<GlobalLocations>(It.IsAny<RetrieveCurrentPageParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(page);

        _globalLocationsPageServiceMock
            .Setup(s => s.GetGlobalLocationsPageDtoAsync(page, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GlobalLocationsDto());

        await _controller.Index(CancellationToken.None);

        _globalLocationsPageServiceMock.Verify(
            s => s.GetGlobalLocationsPageDtoAsync(page, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task Index_PassesCancellationToken_ToServices()
    {
        using var cts = new CancellationTokenSource();
        var token = cts.Token;
        var page = new GlobalLocations();

        _contentServiceMock
            .Setup(s => s.RetrieveCurrentPageAsync<GlobalLocations>(It.IsAny<RetrieveCurrentPageParameters>(), token))
            .ReturnsAsync(page)
            .Verifiable();

        _globalLocationsPageServiceMock
            .Setup(s => s.GetGlobalLocationsPageDtoAsync(page, token))
            .ReturnsAsync(new GlobalLocationsDto())
            .Verifiable();

        await _controller.Index(token);

        _contentServiceMock.Verify(s => s.RetrieveCurrentPageAsync<GlobalLocations>(It.IsAny<RetrieveCurrentPageParameters>(), token), Times.Once);
        _globalLocationsPageServiceMock.Verify(s => s.GetGlobalLocationsPageDtoAsync(page, token), Times.Once);
    }

    [Test]
    public async Task Index_UsesLinkedItemsMaxLevelOfFour()
    {
        var page = new GlobalLocations();

        _contentServiceMock
            .Setup(s => s.RetrieveCurrentPageAsync<GlobalLocations>(It.Is<RetrieveCurrentPageParameters>(p => p.LinkedItemsMaxLevel == 4), It.IsAny<CancellationToken>()))
            .ReturnsAsync(page)
            .Verifiable();

        _globalLocationsPageServiceMock
            .Setup(s => s.GetGlobalLocationsPageDtoAsync(page, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GlobalLocationsDto());

        await _controller.Index(CancellationToken.None);

        _contentServiceMock.Verify(
            s => s.RetrieveCurrentPageAsync<GlobalLocations>(It.Is<RetrieveCurrentPageParameters>(p => p.LinkedItemsMaxLevel == 4), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
