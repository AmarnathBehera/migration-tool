using BDO.GWT.Common.Core.Exceptions;
using BDO.GWT.GLOBAL.API.Data;
using BDO.GWT.GLOBAL.API.Services;
using BDO.GWT.Web.Core.API.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BDO.GWT.GLOBAL.API.Tests.Controllers;

public class EventSectionsControllerTests
{
    private Mock<ISectionEventsCrudService> _sectionEventsCrudServiceMock = null!;
    private EventSectionsController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _sectionEventsCrudServiceMock = new Mock<ISectionEventsCrudService>();

        var httpContext = new DefaultHttpContext();
        httpContext.Items["ChannelId"] = 42;

        _controller = new EventSectionsController(_sectionEventsCrudServiceMock.Object)
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
        var expected = new List<PageOutput<SectionEventsPageContent>>();
        var serviceResult = new ActionResult<IEnumerable<PageOutput<SectionEventsPageContent>>>(expected);

        _sectionEventsCrudServiceMock
            .Setup(x => x.GetPagesAsync(42, "en-gb", true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(serviceResult);

        var result = await _controller.Get(1, "en-gb", true, CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = (OkObjectResult)result.Result!;
        Assert.That(okResult.Value, Is.EqualTo(serviceResult));

        _sectionEventsCrudServiceMock.Verify(
            x => x.GetPagesAsync(42, "en-gb", true, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task Get_All_ReturnsNotFound_OnNotFoundException()
    {
        _sectionEventsCrudServiceMock
            .Setup(x => x.GetPagesAsync(42, "en-gb", null, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException("Channel not found"));

        var result = await _controller.Get(1, "en-gb", null, CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<NotFoundObjectResult>());
        var notFound = (NotFoundObjectResult)result.Result!;
        Assert.That(notFound.Value, Is.EqualTo("Channel not found"));
    }

    [Test]
    public async Task Get_All_Returns500_OnException()
    {
        _sectionEventsCrudServiceMock
            .Setup(x => x.GetPagesAsync(42, "en-gb", null, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Unexpected error"));

        var result = await _controller.Get(1, "en-gb", null, CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<ObjectResult>());
        var objectResult = (ObjectResult)result.Result!;
        Assert.That(objectResult.StatusCode, Is.EqualTo(500));
    }

    [Test]
    public async Task Get_ByGuid_ReturnsOk_WithPage()
    {
        var pageGuid = Guid.NewGuid();
        var expected = new PageOutput<SectionEventsPageContent>
        {
            General = new PageGeneralOutput { PageGuid = pageGuid },
            Content = new SectionEventsPageContent()
        };
        var serviceResult = new ActionResult<PageOutput<SectionEventsPageContent>>(expected);

        _sectionEventsCrudServiceMock
            .Setup(x => x.GetPageAsync(42, pageGuid, "en-gb", It.IsAny<CancellationToken>()))
            .ReturnsAsync(serviceResult);

        var result = await _controller.Get(1, pageGuid, "en-gb", CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = (OkObjectResult)result.Result!;
        Assert.That(okResult.Value, Is.EqualTo(serviceResult));

        _sectionEventsCrudServiceMock.Verify(
            x => x.GetPageAsync(42, pageGuid, "en-gb", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task Get_ByGuid_ReturnsNotFound_OnNotFoundException()
    {
        var pageGuid = Guid.NewGuid();

        _sectionEventsCrudServiceMock
            .Setup(x => x.GetPageAsync(42, pageGuid, "en-gb", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException("Page not found"));

        var result = await _controller.Get(1, pageGuid, "en-gb", CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<NotFoundObjectResult>());
        var notFound = (NotFoundObjectResult)result.Result!;
        Assert.That(notFound.Value, Is.EqualTo("Page not found"));
    }

    [Test]
    public async Task Get_ByGuid_Returns500_OnException()
    {
        var pageGuid = Guid.NewGuid();

        _sectionEventsCrudServiceMock
            .Setup(x => x.GetPageAsync(42, pageGuid, "en-gb", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Unexpected error"));

        var result = await _controller.Get(1, pageGuid, "en-gb", CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<ObjectResult>());
        var objectResult = (ObjectResult)result.Result!;
        Assert.That(objectResult.StatusCode, Is.EqualTo(500));
    }

    [Test]
    public async Task Post_ReturnsCreatedAtAction_WithNewPage()
    {
        var request = new SectionEventsPageContent();
        var expected = new PageOutput<SectionEventsPageContent>
        {
            General = new PageGeneralOutput { PageGuid = Guid.NewGuid() },
            Content = request
        };
        var serviceResult = new ActionResult<PageOutput<SectionEventsPageContent>>(expected);

        _sectionEventsCrudServiceMock
            .Setup(x => x.CreatePageAsync(42, 100, "en-gb", request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(serviceResult);

        var result = await _controller.Post(1, 100, "en-gb", request, CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<CreatedAtActionResult>());
        var createdResult = (CreatedAtActionResult)result.Result!;
        Assert.That(createdResult.ActionName, Is.EqualTo("Get"));

        _sectionEventsCrudServiceMock.Verify(
            x => x.CreatePageAsync(42, 100, "en-gb", request, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task Post_ReturnsNotFound_OnNotFoundException()
    {
        var request = new SectionEventsPageContent();

        _sectionEventsCrudServiceMock
            .Setup(x => x.CreatePageAsync(42, 100, "en-gb", request, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException("Parent not found"));

        var result = await _controller.Post(1, 100, "en-gb", request, CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task Post_ReturnsUnprocessableEntity_OnUnprocessableEntityException()
    {
        var request = new SectionEventsPageContent();

        _sectionEventsCrudServiceMock
            .Setup(x => x.CreatePageAsync(42, 100, "en-gb", request, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnprocessableEntityException());

        var result = await _controller.Post(1, 100, "en-gb", request, CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<UnprocessableEntityObjectResult>());
    }

    [Test]
    public async Task Post_Returns500_OnException()
    {
        var request = new SectionEventsPageContent();

        _sectionEventsCrudServiceMock
            .Setup(x => x.CreatePageAsync(42, 100, "en-gb", request, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Unexpected error"));

        var result = await _controller.Post(1, 100, "en-gb", request, CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<ObjectResult>());
        var objectResult = (ObjectResult)result.Result!;
        Assert.That(objectResult.StatusCode, Is.EqualTo(500));
    }

    [Test]
    public async Task CreateNewCultureVersion_ReturnsCreatedAtAction()
    {
        var pageGuid = Guid.NewGuid();
        var request = new SectionEventsPageContent();
        var expected = new PageOutput<SectionEventsPageContent>
        {
            General = new PageGeneralOutput { PageGuid = pageGuid },
            Content = request
        };
        var serviceResult = new ActionResult<PageOutput<SectionEventsPageContent>>(expected);

        _sectionEventsCrudServiceMock
            .Setup(x => x.CreatePageCultureVersionAsync(42, pageGuid, "en-gb", "fr-fr", request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(serviceResult);

        var result = await _controller.CreateNewCultureVersion(1, pageGuid, "en-gb", "fr-fr", request, CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<CreatedAtActionResult>());
        var createdResult = (CreatedAtActionResult)result.Result!;
        Assert.That(createdResult.ActionName, Is.EqualTo("Get"));

        _sectionEventsCrudServiceMock.Verify(
            x => x.CreatePageCultureVersionAsync(42, pageGuid, "en-gb", "fr-fr", request, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task CreateNewCultureVersion_ReturnsNotFound_OnNotFoundException()
    {
        var pageGuid = Guid.NewGuid();
        var request = new SectionEventsPageContent();

        _sectionEventsCrudServiceMock
            .Setup(x => x.CreatePageCultureVersionAsync(42, pageGuid, "en-gb", "fr-fr", request, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException("Page not found"));

        var result = await _controller.CreateNewCultureVersion(1, pageGuid, "en-gb", "fr-fr", request, CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task CreateNewCultureVersion_ReturnsUnprocessableEntity_OnUnprocessableEntityException()
    {
        var pageGuid = Guid.NewGuid();
        var request = new SectionEventsPageContent();

        _sectionEventsCrudServiceMock
            .Setup(x => x.CreatePageCultureVersionAsync(42, pageGuid, "en-gb", "fr-fr", request, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnprocessableEntityException());

        var result = await _controller.CreateNewCultureVersion(1, pageGuid, "en-gb", "fr-fr", request, CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<UnprocessableEntityObjectResult>());
    }

    [Test]
    public async Task CreateNewCultureVersion_Returns500_OnException()
    {
        var pageGuid = Guid.NewGuid();
        var request = new SectionEventsPageContent();

        _sectionEventsCrudServiceMock
            .Setup(x => x.CreatePageCultureVersionAsync(42, pageGuid, "en-gb", "fr-fr", request, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Unexpected error"));

        var result = await _controller.CreateNewCultureVersion(1, pageGuid, "en-gb", "fr-fr", request, CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<ObjectResult>());
        var objectResult = (ObjectResult)result.Result!;
        Assert.That(objectResult.StatusCode, Is.EqualTo(500));
    }

    [Test]
    public async Task Put_ReturnsOk_WithUpdatedPage()
    {
        var pageGuid = Guid.NewGuid();
        var request = new SectionEventsPageContent();
        var expected = new PageOutput<SectionEventsPageContent>
        {
            General = new PageGeneralOutput { PageGuid = pageGuid },
            Content = request
        };
        var serviceResult = new ActionResult<PageOutput<SectionEventsPageContent>>(expected);

        _sectionEventsCrudServiceMock
            .Setup(x => x.UpdatePageAsync(42, pageGuid, "en-gb", request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(serviceResult);

        var result = await _controller.Put(1, pageGuid, "en-gb", request, CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());

        _sectionEventsCrudServiceMock.Verify(
            x => x.UpdatePageAsync(42, pageGuid, "en-gb", request, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task Put_ReturnsNotFound_OnNotFoundException()
    {
        var pageGuid = Guid.NewGuid();
        var request = new SectionEventsPageContent();

        _sectionEventsCrudServiceMock
            .Setup(x => x.UpdatePageAsync(42, pageGuid, "en-gb", request, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException("Page not found"));

        var result = await _controller.Put(1, pageGuid, "en-gb", request, CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task Put_ReturnsUnprocessableEntity_OnUnprocessableEntityException()
    {
        var pageGuid = Guid.NewGuid();
        var request = new SectionEventsPageContent();

        _sectionEventsCrudServiceMock
            .Setup(x => x.UpdatePageAsync(42, pageGuid, "en-gb", request, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnprocessableEntityException());

        var result = await _controller.Put(1, pageGuid, "en-gb", request, CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<UnprocessableEntityObjectResult>());
    }

    [Test]
    public async Task Put_Returns500_OnException()
    {
        var pageGuid = Guid.NewGuid();
        var request = new SectionEventsPageContent();

        _sectionEventsCrudServiceMock
            .Setup(x => x.UpdatePageAsync(42, pageGuid, "en-gb", request, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Unexpected error"));

        var result = await _controller.Put(1, pageGuid, "en-gb", request, CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<ObjectResult>());
        var objectResult = (ObjectResult)result.Result!;
        Assert.That(objectResult.StatusCode, Is.EqualTo(500));
    }

    [Test]
    public async Task Delete_Success_ReturnsOk()
    {
        var pageGuid = Guid.NewGuid();

        _sectionEventsCrudServiceMock
            .Setup(x => x.DeletePageAsync(42, pageGuid, "en-gb", false, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _controller.Delete(1, pageGuid, "en-gb", false, CancellationToken.None);

        Assert.That(result, Is.InstanceOf<OkObjectResult>());

        _sectionEventsCrudServiceMock.Verify(
            x => x.DeletePageAsync(42, pageGuid, "en-gb", false, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task Delete_ReturnsNotFound_OnNotFoundException()
    {
        var pageGuid = Guid.NewGuid();

        _sectionEventsCrudServiceMock
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

        _sectionEventsCrudServiceMock
            .Setup(x => x.DeletePageAsync(42, pageGuid, "en-gb", false, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnprocessableEntityException());

        var result = await _controller.Delete(1, pageGuid, "en-gb", false, CancellationToken.None);

        Assert.That(result, Is.InstanceOf<UnprocessableEntityObjectResult>());
    }

    [Test]
    public async Task Delete_Returns500_OnException()
    {
        var pageGuid = Guid.NewGuid();

        _sectionEventsCrudServiceMock
            .Setup(x => x.DeletePageAsync(42, pageGuid, "en-gb", false, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Unexpected error"));

        var result = await _controller.Delete(1, pageGuid, "en-gb", false, CancellationToken.None);

        Assert.That(result, Is.InstanceOf<ObjectResult>());
        var objectResult = (ObjectResult)result;
        Assert.That(objectResult.StatusCode, Is.EqualTo(500));
    }
}
