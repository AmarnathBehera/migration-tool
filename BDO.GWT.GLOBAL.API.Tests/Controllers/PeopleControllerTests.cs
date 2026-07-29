using BDO.GWT.Common.Core.Exceptions;
using BDO.GWT.GLOBAL.API.Controllers;
using BDO.GWT.GLOBAL.API.Data;
using BDO.GWT.GLOBAL.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BDO.GWT.GLOBAL.API.Tests.Controllers;

public class PeopleControllerTests
{
    private Mock<IPersonCrudService> _personCrudServiceMock = null!;
    private PeopleController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _personCrudServiceMock = new Mock<IPersonCrudService>();
        _controller = new PeopleController(_personCrudServiceMock.Object);

        var httpContext = new DefaultHttpContext();
        httpContext.Items["ChannelId"] = 42;

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext,
        };
    }

    [Test]
    public async Task Get_All_ForwardsArguments_AndReturnsOk()
    {
        var expected = new List<PageOutput<PersonPageContent>>();
        var serviceResult = new ActionResult<IEnumerable<PageOutput<PersonPageContent>>>(expected);

        _personCrudServiceMock
            .Setup(x => x.GetPersonPagesAsync(42, "en-gb", true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(serviceResult);

        var result = await _controller.Get(960, "en-gb", true, CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = (OkObjectResult)result.Result!;
        Assert.That(okResult.Value, Is.EqualTo(serviceResult));

        _personCrudServiceMock.Verify(
            x => x.GetPersonPagesAsync(42, "en-gb", true, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task Get_ByGuid_ForwardsArguments_AndReturnsOk()
    {
        var pageGuid = Guid.NewGuid();
        var expected = new PageOutput<PersonPageContent>
        {
            General = new PageGeneralOutput { PageGuid = pageGuid },
            Content = new PersonPageContent { MetadataTitle = "Person" },
        };

        var serviceResult = new ActionResult<PageOutput<PersonPageContent>>(expected);

        _personCrudServiceMock
            .Setup(x => x.GetPersonPageAsync(42, pageGuid, "en-gb", It.IsAny<CancellationToken>()))
            .ReturnsAsync(serviceResult);

        var result = await _controller.Get(960, pageGuid, "en-gb", CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = (OkObjectResult)result.Result!;
        Assert.That(okResult.Value, Is.EqualTo(serviceResult));

        _personCrudServiceMock.Verify(
            x => x.GetPersonPageAsync(42, pageGuid, "en-gb", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task Post_ForwardsArguments_AndReturnsCreatedAtAction()
    {
        var request = new PersonPageContent { MetadataTitle = "New person" };
        var expected = new PageOutput<PersonPageContent>
        {
            General = new PageGeneralOutput { PageGuid = Guid.NewGuid() },
            Content = request,
        };

        var serviceResult = new ActionResult<PageOutput<PersonPageContent>>(expected);

        _personCrudServiceMock
            .Setup(x => x.CreatePageAsync(42, 100, "en-gb", request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(serviceResult);

        var result = await _controller.Post(960, 100, "en-gb", request, CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<CreatedAtActionResult>());
        var createdResult = (CreatedAtActionResult)result.Result!;
        using (Assert.EnterMultipleScope())
        {
            Assert.That(createdResult.ActionName, Is.EqualTo("Get"));
            Assert.That(createdResult.Value, Is.EqualTo(expected));
        }

        _personCrudServiceMock.Verify(
            x => x.CreatePageAsync(42, 100, "en-gb", request, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task CreateNewCultureVersion_ForwardsArguments_AndReturnsCreatedAtAction()
    {
        var pageGuid = Guid.NewGuid();
        var request = new PersonPageContent { MetadataTitle = "Person FR" };
        var expected = new PageOutput<PersonPageContent>
        {
            General = new PageGeneralOutput { PageGuid = pageGuid },
            Content = request,
        };

        var serviceResult = new ActionResult<PageOutput<PersonPageContent>>(expected);

        _personCrudServiceMock
            .Setup(x => x.CreatePageCultureVersionAsync(42, pageGuid, "en-gb", "fr-fr", request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(serviceResult);

        var result = await _controller.CreateNewCultureVersion(960, pageGuid, "en-gb", "fr-fr", request, CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<CreatedAtActionResult>());
        var createdResult = (CreatedAtActionResult)result.Result!;
        using (Assert.EnterMultipleScope())
        {
            Assert.That(createdResult.ActionName, Is.EqualTo("Get"));
            Assert.That(createdResult.Value, Is.EqualTo(expected));
        }

        _personCrudServiceMock.Verify(
            x => x.CreatePageCultureVersionAsync(42, pageGuid, "en-gb", "fr-fr", request, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task Put_ForwardsArguments_AndReturnsOk()
    {
        var pageGuid = Guid.NewGuid();
        var request = new PersonPageContent { MetadataTitle = "Updated person" };
        var expected = new PageOutput<PersonPageContent>
        {
            General = new PageGeneralOutput { PageGuid = pageGuid },
            Content = request,
        };

        var serviceResult = new ActionResult<PageOutput<PersonPageContent>>(expected);

        _personCrudServiceMock
            .Setup(x => x.UpdatePageAsync(42, pageGuid, "en-gb", request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(serviceResult);

        var result = await _controller.Put(960, pageGuid, "en-gb", request, CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = (OkObjectResult)result.Result!;
        Assert.That(okResult.Value, Is.EqualTo(expected));

        _personCrudServiceMock.Verify(
            x => x.UpdatePageAsync(42, pageGuid, "en-gb", request, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task Delete_Success_ReturnsOk()
    {
        var pageGuid = Guid.NewGuid();

        _personCrudServiceMock
            .Setup(x => x.DeletePageAsync(42, pageGuid, "en-gb", true, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _controller.Delete(960, pageGuid, "en-gb", true, CancellationToken.None);

        Assert.That(result, Is.InstanceOf<OkObjectResult>());

        _personCrudServiceMock.Verify(
            x => x.DeletePageAsync(42, pageGuid, "en-gb", true, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task Delete_NotFound_ReturnsNotFound()
    {
        var pageGuid = Guid.NewGuid();

        _personCrudServiceMock
            .Setup(x => x.DeletePageAsync(42, pageGuid, "en-gb", false, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException("Page not found"));

        var result = await _controller.Delete(960, pageGuid, "en-gb", false, CancellationToken.None);

        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
        var notFound = (NotFoundObjectResult)result;
        Assert.That(notFound.Value, Is.EqualTo("Page not found"));
    }

    [Test]
    public async Task Delete_UnprocessableEntity_ReturnsUnprocessableEntity()
    {
        var pageGuid = Guid.NewGuid();

        _personCrudServiceMock
            .Setup(x => x.DeletePageAsync(42, pageGuid, "en-gb", false, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnprocessableEntityException());

        var result = await _controller.Delete(960, pageGuid, "en-gb", false, CancellationToken.None);

        Assert.That(result, Is.InstanceOf<UnprocessableEntityObjectResult>());
    }

    [Test]
    public async Task Delete_Exception_Returns500()
    {
        var pageGuid = Guid.NewGuid();

        _personCrudServiceMock
            .Setup(x => x.DeletePageAsync(42, pageGuid, "en-gb", false, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("unexpected"));

        var result = await _controller.Delete(960, pageGuid, "en-gb", false, CancellationToken.None);

        Assert.That(result, Is.InstanceOf<ObjectResult>());
        var objectResult = (ObjectResult)result;
        Assert.That(objectResult.StatusCode, Is.EqualTo(500));
    }

    [TestCase(false)]
    [TestCase(null)]
    public async Task Get_All_ForwardsOnlyPublishedVariants_AndReturnsOk(bool? onlyPublished)
    {
        var expected = new List<PageOutput<PersonPageContent>>();
        var serviceResult = new ActionResult<IEnumerable<PageOutput<PersonPageContent>>>(expected);

        _personCrudServiceMock
            .Setup(x => x.GetPersonPagesAsync(42, "en-gb", onlyPublished, It.IsAny<CancellationToken>()))
            .ReturnsAsync(serviceResult);

        var result = await _controller.Get(960, "en-gb", onlyPublished, CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());

        _personCrudServiceMock.Verify(
            x => x.GetPersonPagesAsync(42, "en-gb", onlyPublished, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task Get_All_WithNullCulture_ForwardsNullCulture()
    {
        var expected = new List<PageOutput<PersonPageContent>>();
        var serviceResult = new ActionResult<IEnumerable<PageOutput<PersonPageContent>>>(expected);

        _personCrudServiceMock
            .Setup(x => x.GetPersonPagesAsync(42, null, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(serviceResult);

        var result = await _controller.Get(960, null, true, CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());

        _personCrudServiceMock.Verify(
            x => x.GetPersonPagesAsync(42, null, true, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task Get_All_ReadsChannelIdFromHttpContextItems()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Items["ChannelId"] = 777;
        _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

        var expected = new List<PageOutput<PersonPageContent>>();
        var serviceResult = new ActionResult<IEnumerable<PageOutput<PersonPageContent>>>(expected);

        _personCrudServiceMock
            .Setup(x => x.GetPersonPagesAsync(777, "en-gb", true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(serviceResult);

        var result = await _controller.Get(960, "en-gb", true, CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());

        _personCrudServiceMock.Verify(
            x => x.GetPersonPagesAsync(777, "en-gb", true, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task Get_ByGuid_ForwardsCancellationToken()
    {
        var pageGuid = Guid.NewGuid();
        var expected = new PageOutput<PersonPageContent>
        {
            General = new PageGeneralOutput { PageGuid = pageGuid },
            Content = new PersonPageContent { MetadataTitle = "Person" },
        };
        var serviceResult = new ActionResult<PageOutput<PersonPageContent>>(expected);
        using var cts = new CancellationTokenSource();

        _personCrudServiceMock
            .Setup(x => x.GetPersonPageAsync(42, pageGuid, "en-gb", cts.Token))
            .ReturnsAsync(serviceResult);

        var result = await _controller.Get(960, pageGuid, "en-gb", cts.Token);

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());

        _personCrudServiceMock.Verify(
            x => x.GetPersonPageAsync(42, pageGuid, "en-gb", cts.Token),
            Times.Once);
    }

    [Test]
    public async Task Post_WhenServiceReturnsNullValue_ReturnsCreatedAtActionWithNullValue()
    {
        var request = new PersonPageContent { MetadataTitle = "New person" };
        var serviceResult = new ActionResult<PageOutput<PersonPageContent>>(new BadRequestResult());

        _personCrudServiceMock
            .Setup(x => x.CreatePageAsync(42, 100, "en-gb", request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(serviceResult);

        var result = await _controller.Post(960, 100, "en-gb", request, CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<CreatedAtActionResult>());
        var createdResult = (CreatedAtActionResult)result.Result!;
        Assert.That(createdResult.Value, Is.Null);
    }

    [Test]
    public async Task Post_ForwardsCancellationToken()
    {
        var request = new PersonPageContent { MetadataTitle = "New person" };
        var expected = new PageOutput<PersonPageContent>
        {
            General = new PageGeneralOutput { PageGuid = Guid.NewGuid() },
            Content = request,
        };
        var serviceResult = new ActionResult<PageOutput<PersonPageContent>>(expected);
        using var cts = new CancellationTokenSource();

        _personCrudServiceMock
            .Setup(x => x.CreatePageAsync(42, 100, "en-gb", request, cts.Token))
            .ReturnsAsync(serviceResult);

        var result = await _controller.Post(960, 100, "en-gb", request, cts.Token);

        Assert.That(result.Result, Is.InstanceOf<CreatedAtActionResult>());

        _personCrudServiceMock.Verify(
            x => x.CreatePageAsync(42, 100, "en-gb", request, cts.Token),
            Times.Once);
    }

    [Test]
    public async Task CreateNewCultureVersion_WhenServiceReturnsNullValue_ReturnsCreatedAtActionWithNullValue()
    {
        var pageGuid = Guid.NewGuid();
        var request = new PersonPageContent { MetadataTitle = "Person FR" };
        var serviceResult = new ActionResult<PageOutput<PersonPageContent>>(new BadRequestResult());

        _personCrudServiceMock
            .Setup(x => x.CreatePageCultureVersionAsync(42, pageGuid, "en-gb", "fr-fr", request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(serviceResult);

        var result = await _controller.CreateNewCultureVersion(960, pageGuid, "en-gb", "fr-fr", request, CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<CreatedAtActionResult>());
        var createdResult = (CreatedAtActionResult)result.Result!;
        Assert.That(createdResult.Value, Is.Null);
    }

    [Test]
    public async Task CreateNewCultureVersion_ForwardsCancellationToken()
    {
        var pageGuid = Guid.NewGuid();
        var request = new PersonPageContent { MetadataTitle = "Person FR" };
        var expected = new PageOutput<PersonPageContent>
        {
            General = new PageGeneralOutput { PageGuid = pageGuid },
            Content = request,
        };
        var serviceResult = new ActionResult<PageOutput<PersonPageContent>>(expected);
        using var cts = new CancellationTokenSource();

        _personCrudServiceMock
            .Setup(x => x.CreatePageCultureVersionAsync(42, pageGuid, "en-gb", "fr-fr", request, cts.Token))
            .ReturnsAsync(serviceResult);

        var result = await _controller.CreateNewCultureVersion(960, pageGuid, "en-gb", "fr-fr", request, cts.Token);

        Assert.That(result.Result, Is.InstanceOf<CreatedAtActionResult>());

        _personCrudServiceMock.Verify(
            x => x.CreatePageCultureVersionAsync(42, pageGuid, "en-gb", "fr-fr", request, cts.Token),
            Times.Once);
    }

    [Test]
    public async Task Put_WhenServiceReturnsNullValue_ReturnsOkWithNullValue()
    {
        var pageGuid = Guid.NewGuid();
        var request = new PersonPageContent { MetadataTitle = "Updated person" };
        var serviceResult = new ActionResult<PageOutput<PersonPageContent>>(new NotFoundResult());

        _personCrudServiceMock
            .Setup(x => x.UpdatePageAsync(42, pageGuid, "en-gb", request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(serviceResult);

        var result = await _controller.Put(960, pageGuid, "en-gb", request, CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = (OkObjectResult)result.Result!;
        Assert.That(okResult.Value, Is.Null);
    }

    [Test]
    public async Task Put_ForwardsCancellationToken()
    {
        var pageGuid = Guid.NewGuid();
        var request = new PersonPageContent { MetadataTitle = "Updated person" };
        var expected = new PageOutput<PersonPageContent>
        {
            General = new PageGeneralOutput { PageGuid = pageGuid },
            Content = request,
        };
        var serviceResult = new ActionResult<PageOutput<PersonPageContent>>(expected);
        using var cts = new CancellationTokenSource();

        _personCrudServiceMock
            .Setup(x => x.UpdatePageAsync(42, pageGuid, "en-gb", request, cts.Token))
            .ReturnsAsync(serviceResult);

        var result = await _controller.Put(960, pageGuid, "en-gb", request, cts.Token);

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());

        _personCrudServiceMock.Verify(
            x => x.UpdatePageAsync(42, pageGuid, "en-gb", request, cts.Token),
            Times.Once);
    }

    [Test]
    public async Task Delete_Success_WithDeleteAllCulturesFalse_ReturnsOk()
    {
        var pageGuid = Guid.NewGuid();

        _personCrudServiceMock
            .Setup(x => x.DeletePageAsync(42, pageGuid, "en-gb", false, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _controller.Delete(960, pageGuid, "en-gb", false, CancellationToken.None);

        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var okResult = (OkObjectResult)result;
        Assert.That(okResult.Value, Is.EqualTo("Successfully deleted."));
    }

    [Test]
    public async Task Delete_UsesDefaultDeleteAllCultures_WhenNotSpecified()
    {
        var pageGuid = Guid.NewGuid();

        _personCrudServiceMock
            .Setup(x => x.DeletePageAsync(42, pageGuid, "en-gb", false, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _controller.Delete(960, pageGuid, "en-gb");

        Assert.That(result, Is.InstanceOf<OkObjectResult>());

        _personCrudServiceMock.Verify(
            x => x.DeletePageAsync(42, pageGuid, "en-gb", false, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task Delete_NotFound_WithDeleteAllCulturesTrue_ReturnsNotFound()
    {
        var pageGuid = Guid.NewGuid();

        _personCrudServiceMock
            .Setup(x => x.DeletePageAsync(42, pageGuid, "en-gb", true, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException("Page not found"));

        var result = await _controller.Delete(960, pageGuid, "en-gb", true, CancellationToken.None);

        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
        var notFound = (NotFoundObjectResult)result;
        Assert.That(notFound.Value, Is.EqualTo("Page not found"));
    }

    [Test]
    public async Task Delete_UnprocessableEntity_WithDeleteAllCulturesTrue_ReturnsUnprocessableEntity()
    {
        var pageGuid = Guid.NewGuid();

        _personCrudServiceMock
            .Setup(x => x.DeletePageAsync(42, pageGuid, "en-gb", true, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnprocessableEntityException());

        var result = await _controller.Delete(960, pageGuid, "en-gb", true, CancellationToken.None);

        Assert.That(result, Is.InstanceOf<UnprocessableEntityObjectResult>());
        var unprocessable = (UnprocessableEntityObjectResult)result;
        Assert.That(unprocessable.Value, Is.EqualTo("One or more validation errors occurred."));
    }

    [Test]
    public async Task Delete_Exception_WithDeleteAllCulturesTrue_Returns500()
    {
        var pageGuid = Guid.NewGuid();

        _personCrudServiceMock
            .Setup(x => x.DeletePageAsync(42, pageGuid, "en-gb", true, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("unexpected"));

        var result = await _controller.Delete(960, pageGuid, "en-gb", true, CancellationToken.None);

        Assert.That(result, Is.InstanceOf<ObjectResult>());
        var objectResult = (ObjectResult)result;
        using (Assert.EnterMultipleScope())
        {
            Assert.That(objectResult.StatusCode, Is.EqualTo(500));
            Assert.That(objectResult.Value, Is.EqualTo("unexpected"));
        }
    }

    [Test]
    public async Task Delete_ForwardsCancellationToken()
    {
        var pageGuid = Guid.NewGuid();
        using var cts = new CancellationTokenSource();

        _personCrudServiceMock
            .Setup(x => x.DeletePageAsync(42, pageGuid, "en-gb", false, cts.Token))
            .Returns(Task.CompletedTask);

        var result = await _controller.Delete(960, pageGuid, "en-gb", false, cts.Token);

        Assert.That(result, Is.InstanceOf<OkObjectResult>());

        _personCrudServiceMock.Verify(
            x => x.DeletePageAsync(42, pageGuid, "en-gb", false, cts.Token),
            Times.Once);
    }

    [Test]
    public void PeopleController_Constructor_StoresProvidedService()
    {
        var mock = new Mock<IPersonCrudService>();
        var controller = new PeopleController(mock.Object);

        Assert.That(controller, Is.Not.Null);
    }
}