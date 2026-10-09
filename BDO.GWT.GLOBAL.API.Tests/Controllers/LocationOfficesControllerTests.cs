using BDO.GWT.Common.Core.Exceptions;
using BDO.GWT.GLOBAL.API.Data;
using BDO.GWT.GLOBAL.API.Services;
using BDO.GWT.GLOBAL.API.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BDO.GWT.GLOBAL.API.Tests.Controllers;

public class LocationOfficesControllerTests
{
    private Mock<ILocationOfficeCrudService> _locationOfficeCrudServiceMock = null!;
    private LocationOfficesController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _locationOfficeCrudServiceMock = new Mock<ILocationOfficeCrudService>();

        var httpContext = new DefaultHttpContext();
        httpContext.Items["ChannelId"] = 42;

        _controller = new LocationOfficesController(_locationOfficeCrudServiceMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            }
        };
    }

    [Test]
    public async Task Get_All_ReturnsOk_WithPageList()
    {
        var expected = new List<PageOutput<LocationOfficePageContent>>();
        var serviceResult = new ActionResult<IEnumerable<PageOutput<LocationOfficePageContent>>>(expected);

        _locationOfficeCrudServiceMock
            .Setup(x => x.GetPagesAsync(42, "en-gb", true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(serviceResult);

        var result = await _controller.Get(1, "en-gb", true, CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = (OkObjectResult)result.Result!;
        Assert.That(okResult.Value, Is.EqualTo(serviceResult));

        _locationOfficeCrudServiceMock.Verify(
            x => x.GetPagesAsync(42, "en-gb", true, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [TestCase(false)]
    [TestCase(null)]
    public async Task Get_All_ForwardsOnlyPublishedVariants_AndReturnsOk(bool? onlyPublished)
    {
        var expected = new List<PageOutput<LocationOfficePageContent>>();
        var serviceResult = new ActionResult<IEnumerable<PageOutput<LocationOfficePageContent>>>(expected);

        _locationOfficeCrudServiceMock
            .Setup(x => x.GetPagesAsync(42, "en-gb", onlyPublished, It.IsAny<CancellationToken>()))
            .ReturnsAsync(serviceResult);

        var result = await _controller.Get(1, "en-gb", onlyPublished, CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());

        _locationOfficeCrudServiceMock.Verify(
            x => x.GetPagesAsync(42, "en-gb", onlyPublished, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task Get_ByGuid_ReturnsOk_WithPage()
    {
        var pageGuid = Guid.NewGuid();
        var expected = new PageOutput<LocationOfficePageContent>
        {
            General = new PageGeneralOutput { PageGuid = pageGuid },
            Content = new LocationOfficePageContent
            {
                MetadataTitle = "Office",
                MetadataTeaserImageAltText = "alt",
                OfficeImage = "img.png",
                OfficeImageAltText = "office alt",
                LocationAddressLine1 = "123 Street",
                LocationZip = "12345",
                LocationZipDisplayType = "show",
                LocationCountry = "UK",
                LocationLatitude = "51.5",
                LocationLongitude = "-0.1",
                LocationPhone = "0123456789"
            }
        };
        var serviceResult = new ActionResult<PageOutput<LocationOfficePageContent>>(expected);

        _locationOfficeCrudServiceMock
            .Setup(x => x.GetPageAsync(42, pageGuid, "en-gb", It.IsAny<CancellationToken>()))
            .ReturnsAsync(serviceResult);

        var result = await _controller.Get(1, pageGuid, "en-gb", CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = (OkObjectResult)result.Result!;
        Assert.That(okResult.Value, Is.EqualTo(serviceResult));

        _locationOfficeCrudServiceMock.Verify(
            x => x.GetPageAsync(42, pageGuid, "en-gb", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task Post_ReturnsCreatedAtAction_WithNewPage()
    {
        var request = new LocationOfficePageContent
        {
            MetadataTitle = "New Office",
            MetadataTeaserImageAltText = "alt",
            OfficeImage = "img.png",
            OfficeImageAltText = "office alt",
            LocationAddressLine1 = "123 Street",
            LocationZip = "12345",
            LocationZipDisplayType = "show",
            LocationCountry = "UK",
            LocationLatitude = "51.5",
            LocationLongitude = "-0.1",
            LocationPhone = "0123456789"
        };
        var pageGuid = Guid.NewGuid();
        var expected = new PageOutput<LocationOfficePageContent>
        {
            General = new PageGeneralOutput { PageGuid = pageGuid },
            Content = request
        };
        var serviceResult = new ActionResult<PageOutput<LocationOfficePageContent>>(expected);

        _locationOfficeCrudServiceMock
            .Setup(x => x.CreatePageAsync(42, 100, "en-gb", request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(serviceResult);

        var result = await _controller.Post(1, 100, "en-gb", request, CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<CreatedAtActionResult>());
        var createdResult = (CreatedAtActionResult)result.Result!;
        using (Assert.EnterMultipleScope())
        {
            Assert.That(createdResult.ActionName, Is.EqualTo("Get"));
            Assert.That(createdResult.Value, Is.EqualTo(expected));
        }

        _locationOfficeCrudServiceMock.Verify(
            x => x.CreatePageAsync(42, 100, "en-gb", request, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task CreateNewCultureVersion_ReturnsCreatedAtAction()
    {
        var pageGuid = Guid.NewGuid();
        var request = new LocationOfficePageContent
        {
            MetadataTitle = "Office FR",
            MetadataTeaserImageAltText = "alt",
            OfficeImage = "img.png",
            OfficeImageAltText = "office alt",
            LocationAddressLine1 = "123 Rue",
            LocationZip = "75001",
            LocationZipDisplayType = "show",
            LocationCountry = "FR",
            LocationLatitude = "48.8",
            LocationLongitude = "2.3",
            LocationPhone = "0123456789"
        };
        var expected = new PageOutput<LocationOfficePageContent>
        {
            General = new PageGeneralOutput { PageGuid = pageGuid },
            Content = request
        };
        var serviceResult = new ActionResult<PageOutput<LocationOfficePageContent>>(expected);

        _locationOfficeCrudServiceMock
            .Setup(x => x.CreatePageCultureVersionAsync(42, pageGuid, "en-gb", "fr-fr", request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(serviceResult);

        var result = await _controller.CreateNewCultureVersion(1, pageGuid, "en-gb", "fr-fr", request, CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<CreatedAtActionResult>());
        var createdResult = (CreatedAtActionResult)result.Result!;
        using (Assert.EnterMultipleScope())
        {
            Assert.That(createdResult.ActionName, Is.EqualTo("Get"));
            Assert.That(createdResult.Value, Is.EqualTo(expected));
        }

        _locationOfficeCrudServiceMock.Verify(
            x => x.CreatePageCultureVersionAsync(42, pageGuid, "en-gb", "fr-fr", request, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task Put_ReturnsOk_WithUpdatedPage()
    {
        var pageGuid = Guid.NewGuid();
        var request = new LocationOfficePageContent
        {
            MetadataTitle = "Updated Office",
            MetadataTeaserImageAltText = "alt",
            OfficeImage = "img.png",
            OfficeImageAltText = "office alt",
            LocationAddressLine1 = "456 Street",
            LocationZip = "67890",
            LocationZipDisplayType = "show",
            LocationCountry = "UK",
            LocationLatitude = "51.6",
            LocationLongitude = "-0.2",
            LocationPhone = "0987654321"
        };
        var expected = new PageOutput<LocationOfficePageContent>
        {
            General = new PageGeneralOutput { PageGuid = pageGuid },
            Content = request
        };
        var serviceResult = new ActionResult<PageOutput<LocationOfficePageContent>>(expected);

        _locationOfficeCrudServiceMock
            .Setup(x => x.UpdatePageAsync(42, pageGuid, "en-gb", request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(serviceResult);

        var result = await _controller.Put(1, pageGuid, "en-gb", request, CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = (OkObjectResult)result.Result!;
        Assert.That(okResult.Value, Is.EqualTo(expected));

        _locationOfficeCrudServiceMock.Verify(
            x => x.UpdatePageAsync(42, pageGuid, "en-gb", request, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task Delete_Success_ReturnsOk()
    {
        var pageGuid = Guid.NewGuid();

        _locationOfficeCrudServiceMock
            .Setup(x => x.DeletePageAsync(42, pageGuid, "en-gb", false, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _controller.Delete(1, pageGuid, "en-gb", false, CancellationToken.None);

        Assert.That(result, Is.InstanceOf<OkObjectResult>());

        _locationOfficeCrudServiceMock.Verify(
            x => x.DeletePageAsync(42, pageGuid, "en-gb", false, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task Delete_ReturnsNotFound_OnNotFoundException()
    {
        var pageGuid = Guid.NewGuid();

        _locationOfficeCrudServiceMock
            .Setup(x => x.DeletePageAsync(42, pageGuid, "en-gb", false, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException("Page not found"));

        var result = await _controller.Delete(1, pageGuid, "en-gb", false, CancellationToken.None);

        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
        var notFound = (NotFoundObjectResult)result;
        Assert.That(notFound.Value, Is.EqualTo("Page not found"));
    }

    [Test]
    public async Task Delete_ReturnsUnprocessableEntity_OnUnprocessableEntityException()
    {
        var pageGuid = Guid.NewGuid();

        _locationOfficeCrudServiceMock
            .Setup(x => x.DeletePageAsync(42, pageGuid, "en-gb", false, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnprocessableEntityException());

        var result = await _controller.Delete(1, pageGuid, "en-gb", false, CancellationToken.None);

        Assert.That(result, Is.InstanceOf<UnprocessableEntityObjectResult>());
    }

    [Test]
    public async Task Delete_Returns500_OnException()
    {
        var pageGuid = Guid.NewGuid();

        _locationOfficeCrudServiceMock
            .Setup(x => x.DeletePageAsync(42, pageGuid, "en-gb", false, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Unexpected error"));

        var result = await _controller.Delete(1, pageGuid, "en-gb", false, CancellationToken.None);

        Assert.That(result, Is.InstanceOf<ObjectResult>());
        var objectResult = (ObjectResult)result;
        Assert.That(objectResult.StatusCode, Is.EqualTo(500));
    }

    [Test]
    public async Task Delete_WithDeleteAllCultures_True_ReturnsOk()
    {
        var pageGuid = Guid.NewGuid();

        _locationOfficeCrudServiceMock
            .Setup(x => x.DeletePageAsync(42, pageGuid, "en-gb", true, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _controller.Delete(1, pageGuid, "en-gb", true, CancellationToken.None);

        Assert.That(result, Is.InstanceOf<OkObjectResult>());

        _locationOfficeCrudServiceMock.Verify(
            x => x.DeletePageAsync(42, pageGuid, "en-gb", true, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
