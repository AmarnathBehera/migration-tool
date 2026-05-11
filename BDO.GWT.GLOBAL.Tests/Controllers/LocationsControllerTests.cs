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

public class LocationsControllerTests
{
    private Mock<IContentService> _contentServiceMock = null!;
    private Mock<ILocationsPageService> _locationsPageServiceMock = null!;
    private LocationsController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _contentServiceMock = new Mock<IContentService>();
        _locationsPageServiceMock = new Mock<ILocationsPageService>();
        _controller = new LocationsController(_contentServiceMock.Object, _locationsPageServiceMock.Object);
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
            .Setup(s => s.RetrieveCurrentPageAsync<Locations>(It.IsAny<RetrieveCurrentPageParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Locations?)null);

        var result = await _controller.Index(CancellationToken.None);

        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task Index_ReturnsViewResult_WithExpectedTuple()
    {
        var page = new Locations();
        var pageDto = new LocationsDto
        {
            Hero = new HeroDto { Title = "Our Locations" },
            MainContent = new LocationsMainDto(),
            Banner = new LocationsBannerDto()
        };
        var schemaDto = new OrganizationSchemaMarkupDto
        {
            Name = "BDO",
            Url = "https://example.com/locations"
        };

        _contentServiceMock
            .Setup(s => s.RetrieveCurrentPageAsync<Locations>(It.IsAny<RetrieveCurrentPageParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(page);

        _locationsPageServiceMock
            .Setup(s => s.GetLocationsDtoAsync(page, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pageDto);

        _locationsPageServiceMock
            .Setup(s => s.GetLocationsSchemaMarkupDto(page, It.IsAny<CancellationToken>()))
            .ReturnsAsync(schemaDto);

        var result = await _controller.Index(CancellationToken.None);

        Assert.That(result, Is.InstanceOf<ViewResult>());
        var viewResult = (ViewResult)result;
        var model = ((LocationsDto, OrganizationSchemaMarkupDto))viewResult.Model!;
        Assert.Multiple(() =>
        {
            Assert.That(model.Item1, Is.EqualTo(pageDto));
            Assert.That(model.Item2, Is.EqualTo(schemaDto));
        });
    }

    [Test]
    public async Task Index_CallsServicesWithCorrectPage()
    {
        var page = new Locations();

        _contentServiceMock
            .Setup(s => s.RetrieveCurrentPageAsync<Locations>(It.IsAny<RetrieveCurrentPageParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(page);

        _locationsPageServiceMock
            .Setup(s => s.GetLocationsDtoAsync(page, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new LocationsDto { Hero = new HeroDto(), MainContent = new LocationsMainDto(), Banner = new LocationsBannerDto() });

        _locationsPageServiceMock
            .Setup(s => s.GetLocationsSchemaMarkupDto(page, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OrganizationSchemaMarkupDto());

        await _controller.Index(CancellationToken.None);

        _locationsPageServiceMock.Verify(s => s.GetLocationsDtoAsync(page, It.IsAny<CancellationToken>()), Times.Once);
        _locationsPageServiceMock.Verify(s => s.GetLocationsSchemaMarkupDto(page, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Index_DoesNotCallPageService_WhenCurrentPageIsNull()
    {
        _contentServiceMock
            .Setup(s => s.RetrieveCurrentPageAsync<Locations>(It.IsAny<RetrieveCurrentPageParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Locations?)null);

        await _controller.Index(CancellationToken.None);

        _locationsPageServiceMock.Verify(s => s.GetLocationsDtoAsync(It.IsAny<Locations>(), It.IsAny<CancellationToken>()), Times.Never);
        _locationsPageServiceMock.Verify(s => s.GetLocationsSchemaMarkupDto(It.IsAny<Locations>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}