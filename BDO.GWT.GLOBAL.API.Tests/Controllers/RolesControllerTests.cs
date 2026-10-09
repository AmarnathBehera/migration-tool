using BDO.GWT.GLOBAL.API.Controllers;
using BDO.GWT.GLOBAL.API.Data;
using BDO.GWT.GLOBAL.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace BDO.GWT.GLOBAL.API.Tests.Controllers;

[TestFixture]
public class RolesControllerTests
{
    private Mock<IRoleCrudService> _roleCrudServiceMock = null!;
    private Mock<ILogger<RolesController>> _loggerMock = null!;
    private RolesController _controller = null!;

    [SetUp]
    public void SetUp()
    {
        _roleCrudServiceMock = new Mock<IRoleCrudService>(MockBehavior.Strict);
        _loggerMock = new Mock<ILogger<RolesController>>();
        _controller = new RolesController(_roleCrudServiceMock.Object, _loggerMock.Object);
    }

    private static RoleOutput CreateRoleOutput(Guid? guid = null, string roleName = "Editors") => new()
    {
        GUID = guid ?? Guid.NewGuid(),
        RoleName = roleName,
        DisplayName = roleName
    };

    private static RoleInput CreateRoleInput(string roleName = "Editors") => new()
    {
        RoleName = roleName,
        DisplayName = roleName
    };

    [Test]
    public async Task Get_ReturnsOkAndForwardsFilter()
    {
        var expected = new[] { CreateRoleOutput(roleName: "Editors") };
        _roleCrudServiceMock.Setup(x => x.GetRolesAsync("Editors", It.IsAny<CancellationToken>())).ReturnsAsync(expected);

        var result = await _controller.Get("Editors", CancellationToken.None);

        var response = result.Result as OkObjectResult;
        Assert.Multiple(() =>
        {
            Assert.That(response, Is.Not.Null);
            Assert.That(response!.Value, Is.EqualTo(expected));
        });
        _roleCrudServiceMock.Verify(x => x.GetRolesAsync("Editors", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GetByGuid_ReturnsOkWhenRoleExists()
    {
        var guid = Guid.NewGuid();
        var expected = CreateRoleOutput(guid);
        _roleCrudServiceMock.Setup(x => x.GetRoleAsync(guid, It.IsAny<CancellationToken>())).ReturnsAsync(expected);

        var result = await _controller.Get(guid, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
            Assert.That(((OkObjectResult)result.Result!).Value, Is.EqualTo(expected));
        });
    }

    [Test]
    public async Task GetByGuid_ReturnsNotFoundWhenRoleDoesNotExist()
    {
        var guid = Guid.NewGuid();
        _roleCrudServiceMock.Setup(x => x.GetRoleAsync(guid, It.IsAny<CancellationToken>())).ReturnsAsync((RoleOutput?)null);

        var result = await _controller.Get(guid, CancellationToken.None);

        var response = result.Result as ObjectResult;
        Assert.Multiple(() =>
        {
            Assert.That(response, Is.Not.Null);
            Assert.That(response!.StatusCode, Is.EqualTo(StatusCodes.Status404NotFound));
            Assert.That(((ProblemDetails)response.Value!).Title, Is.EqualTo("Role not found"));
        });
    }

    [Test]
    public async Task Post_ReturnsCreatedAtActionAndForwardsInput()
    {
        var input = CreateRoleInput();
        var expected = CreateRoleOutput(roleName: input.RoleName);
        _roleCrudServiceMock.Setup(x => x.CreateRoleAsync(input, It.IsAny<CancellationToken>())).ReturnsAsync(expected);

        var result = await _controller.Post(input, CancellationToken.None);

        var response = result.Result as CreatedAtActionResult;
        Assert.Multiple(() =>
        {
            Assert.That(response, Is.Not.Null);
            Assert.That(response!.ActionName, Is.EqualTo(nameof(RolesController.Get)));
            Assert.That(response.RouteValues!["roleGuid"], Is.EqualTo(expected.GUID));
            Assert.That(response.Value, Is.EqualTo(expected));
        });
    }

    [Test]
    public async Task Put_ReturnsOkAndForwardsGuidAndInput()
    {
        var guid = Guid.NewGuid();
        var input = CreateRoleInput("Administrators");
        var expected = CreateRoleOutput(guid, input.RoleName);
        _roleCrudServiceMock.Setup(x => x.UpdateRoleAsync(guid, input, It.IsAny<CancellationToken>())).ReturnsAsync(expected);

        var result = await _controller.Put(guid, input, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
            Assert.That(((OkObjectResult)result.Result!).Value, Is.EqualTo(expected));
        });
    }

    [Test]
    public async Task Delete_ReturnsNoContent()
    {
        var guid = Guid.NewGuid();
        _roleCrudServiceMock.Setup(x => x.DeleteRoleAsync(guid, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var result = await _controller.Delete(guid, CancellationToken.None);

        Assert.That(result, Is.TypeOf<NoContentResult>());
        _roleCrudServiceMock.Verify(x => x.DeleteRoleAsync(guid, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Assign_ReturnsOkAndForwardsInput()
    {
        var input = new AssignRolePermissionsInput { RoleName = "Editors" };
        var expected = CreateRoleOutput(roleName: input.RoleName!);
        _roleCrudServiceMock.Setup(x => x.AssignRolePermissionsAsync(input, It.IsAny<CancellationToken>())).ReturnsAsync(expected);

        var result = await _controller.Assign(input, CancellationToken.None);
        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
            Assert.That(((OkObjectResult)result.Result!).Value, Is.EqualTo(expected));
        });
    }

    [Test]
    public async Task CreateForChannel_ReturnsCreatedWhenInputIsValid()
    {
        var expected = CreateRoleOutput(roleName: "Ind-Editor");
        _roleCrudServiceMock
            .Setup(x => x.CreateChannelRoleAsync(It.Is<CreateChannelRoleInput>(input =>
                input.WebsiteChannel == "Ind" && input.RoleType == RolesAllowedDropdown.Editor), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _controller.CreateForChannel("Ind", RolesAllowedDropdown.Editor, CancellationToken.None);

        var response = result.Result as ObjectResult;
        Assert.Multiple(() =>
        {
            Assert.That(response, Is.Not.Null);
            Assert.That(response!.StatusCode, Is.EqualTo(StatusCodes.Status201Created));
            Assert.That(response.Value, Is.EqualTo(expected));
        });
    }

    [Test]
    public async Task CreateForAllChannels_ReturnsCreated()
    {
        var expected = new[] { CreateRoleOutput(roleName: "Ind-Editor") };
        _roleCrudServiceMock.Setup(x => x.CreateChannelRolesForAllChannelsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(expected);

        var result = await _controller.CreateForAllChannels(CancellationToken.None);

        var response = result.Result as ObjectResult;
        Assert.Multiple(() =>
        {
            Assert.That(response, Is.Not.Null);
            Assert.That(response!.StatusCode, Is.EqualTo(StatusCodes.Status201Created));
            Assert.That(response.Value, Is.EqualTo(expected));
        });
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public async Task Post_WithMissingBody_ReturnsBadRequest(string? _)
    {
        var result = await _controller.Post(null!, CancellationToken.None);

        AssertProblem(result.Result, StatusCodes.Status400BadRequest, "Invalid request");
        _roleCrudServiceMock.Verify(x => x.CreateRoleAsync(It.IsAny<RoleInput>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Put_WithMissingBody_ReturnsBadRequest()
    {
        var result = await _controller.Put(Guid.NewGuid(), null!, CancellationToken.None);

        AssertProblem(result.Result, StatusCodes.Status400BadRequest, "Invalid request");
    }

    [Test]
    public async Task Assign_WithMissingBody_ReturnsBadRequest()
    {
        var result = await _controller.Assign(null!, CancellationToken.None);

        AssertProblem(result.Result, StatusCodes.Status400BadRequest, "Invalid request");
    }

    [Test]
    public async Task CreateForChannel_WithMissingChannel_ReturnsBadRequest()
    {
        var result = await _controller.CreateForChannel(" ", RolesAllowedDropdown.Editor, CancellationToken.None);

        AssertProblem(result.Result, StatusCodes.Status400BadRequest, "Invalid request");
        _roleCrudServiceMock.Verify(x => x.CreateChannelRoleAsync(It.IsAny<CreateChannelRoleInput>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task CreateForChannel_WithUndefinedRoleType_ReturnsBadRequest()
    {
        var result = await _controller.CreateForChannel("Ind", (RolesAllowedDropdown)99, CancellationToken.None);

        AssertProblem(result.Result, StatusCodes.Status400BadRequest, "Invalid request");
    }

    [TestCase(typeof(InvalidOperationException), 400, "Invalid request")]
    [TestCase(typeof(UnauthorizedAccessException), 401, "Unauthorized")]
    [TestCase(typeof(ChannelNotFoundException), 404, "Channel not found")]
    [TestCase(typeof(ChannelRoleExistException), 409, "Role already exists")]
    [TestCase(typeof(OperationCanceledException), 499, "Request cancelled")]
    public async Task Get_MapsKnownExceptionsToProblemDetails(Type exceptionType, int statusCode, string title)
    {
        var exception = exceptionType == typeof(ChannelNotFoundException)
            ? new ChannelNotFoundException("Ind")
            : exceptionType == typeof(ChannelRoleExistException)
                ? new ChannelRoleExistException("Ind-Editor")
                : (Exception)Activator.CreateInstance(exceptionType, "failure")!;
        _roleCrudServiceMock.Setup(x => x.GetRolesAsync(It.IsAny<string?>(), It.IsAny<CancellationToken>())).ThrowsAsync(exception);

        var result = await _controller.Get(null, CancellationToken.None);

        AssertProblem(result.Result, statusCode, title);
    }

    [Test]
    public async Task Get_MapsUnexpectedExceptionToInternalServerError()
    {
        _roleCrudServiceMock.Setup(x => x.GetRolesAsync(It.IsAny<string?>(), It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("failure"));

        var result = await _controller.Get(null, CancellationToken.None);

        AssertProblem(result.Result, StatusCodes.Status500InternalServerError, "Unexpected error");
    }

    [Test]
    public async Task Get_MapsRolePersistenceExceptionToInternalServerError()
    {
        var exception = new RolePersistenceException("Could not save role.", new InvalidOperationException("database failure"));
        _roleCrudServiceMock.Setup(x => x.GetRolesAsync(It.IsAny<string?>(), It.IsAny<CancellationToken>())).ThrowsAsync(exception);

        var result = await _controller.Get(null, CancellationToken.None);

        AssertProblem(result.Result, StatusCodes.Status500InternalServerError, "Failed to save role data");
        Assert.That(((ProblemDetails)((ObjectResult)result.Result!).Value!).Detail, Does.Contain("database failure"));
    }

    private static void AssertProblem(IActionResult? actionResult, int statusCode, string title)
    {
        var response = actionResult as ObjectResult;
        Assert.Multiple(() =>
        {
            Assert.That(response, Is.Not.Null);
            Assert.That(response!.StatusCode, Is.EqualTo(statusCode));
            Assert.That(response.Value, Is.TypeOf<ProblemDetails>());
            Assert.That(((ProblemDetails)response.Value!).Title, Is.EqualTo(title));
        });
    }
}
