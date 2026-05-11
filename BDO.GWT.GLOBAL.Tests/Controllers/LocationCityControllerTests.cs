using BDO.GWT.Core;
using BDO.GWT.Core.DTOs;
using BDO.GWT.Core.DTOs.Pages;
using BDO.GWT.Core.DTOs.SchemaMarkups;
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

public class LocationCityControllerTests
{
    private Mock<IContentService> _contentServiceMock = null!;
    private Mock<ILocationCityPageService> _locationCityPageServiceMock = null!;
    private LocationCityController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _contentServiceMock = new Mock<IContentService>();
        _locationCityPageServiceMock = new Mock<ILocationCityPageService>();
        _controller = new LocationCityController(_contentServiceMock.Object, _locationCityPageServiceMock.Object);
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
            .Setup(s => s.RetrieveCurrentPageAsync<LocationCity>(It.IsAny<RetrieveCurrentPageParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((LocationCity?)null);

        var result = await _controller.Index(CancellationToken.None);

        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task Index_ReturnsViewResult_WithExpectedTuple()
    {
        var page = new LocationCity();
        var pageDto = new LocationCityDto
        {
            Hero = new HeroDto { Title = "City Office" },
            MainContent = new LocationCityMainDto()
        };
        var schemaDto = new FinancialServiceSchemaMarkupDto();

        _contentServiceMock
            .Setup(s => s.RetrieveCurrentPageAsync<LocationCity>(It.IsAny<RetrieveCurrentPageParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(page);

        _locationCityPageServiceMock
            .Setup(s => s.GetLocationCityDtoAsync(page, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pageDto);

        _locationCityPageServiceMock
            .Setup(s => s.GetLocationCitySchemaMarkupDto(page, It.IsAny<CancellationToken>()))
            .ReturnsAsync(schemaDto);

        var result = await _controller.Index(CancellationToken.None);

        Assert.That(result, Is.InstanceOf<ViewResult>());
        var viewResult = (ViewResult)result;
        var model = ((LocationCityDto, FinancialServiceSchemaMarkupDto))viewResult.Model!;
        Assert.Multiple(() =>
        {
            Assert.That(model.Item1, Is.EqualTo(pageDto));
            Assert.That(model.Item2, Is.EqualTo(schemaDto));
        });
    }

    [Test]
    public async Task Index_CallsServicesWithCorrectPage()
    {
        var page = new LocationCity();

        _contentServiceMock
            .Setup(s => s.RetrieveCurrentPageAsync<LocationCity>(It.IsAny<RetrieveCurrentPageParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(page);

        _locationCityPageServiceMock
            .Setup(s => s.GetLocationCityDtoAsync(page, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new LocationCityDto { Hero = new HeroDto(), MainContent = new LocationCityMainDto() });

        _locationCityPageServiceMock
            .Setup(s => s.GetLocationCitySchemaMarkupDto(page, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FinancialServiceSchemaMarkupDto());

        await _controller.Index(CancellationToken.None);

        _locationCityPageServiceMock.Verify(s => s.GetLocationCityDtoAsync(page, It.IsAny<CancellationToken>()), Times.Once);
        _locationCityPageServiceMock.Verify(s => s.GetLocationCitySchemaMarkupDto(page, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Index_DoesNotCallPageService_WhenCurrentPageIsNull()
    {
        _contentServiceMock
            .Setup(s => s.RetrieveCurrentPageAsync<LocationCity>(It.IsAny<RetrieveCurrentPageParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((LocationCity?)null);

        await _controller.Index(CancellationToken.None);

        _locationCityPageServiceMock.Verify(s => s.GetLocationCityDtoAsync(It.IsAny<LocationCity>(), It.IsAny<CancellationToken>()), Times.Never);
        _locationCityPageServiceMock.Verify(s => s.GetLocationCitySchemaMarkupDto(It.IsAny<LocationCity>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}