using BDO.GWT.GLOBAL.API.Controllers;
using BDO.GWT.GLOBAL.API.Data;
using BDO.GWT.GLOBAL.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BDO.GWT.GLOBAL.API.Tests.Controllers;

[TestFixture]
public class UsersControllerTests
{
    private Mock<IUserCrudService> _userCrudServiceMock = null!;
    private Mock<IUserApiPermissionsCrudService> _userApiPermissionsCrudServiceMock = null!;
    private UsersController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _userCrudServiceMock = new Mock<IUserCrudService>(MockBehavior.Strict);
        _userApiPermissionsCrudServiceMock = new Mock<IUserApiPermissionsCrudService>();
        _controller = new UsersController(_userCrudServiceMock.Object, _userApiPermissionsCrudServiceMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }

    private static UserOutput CreateUserOutput(Guid? guid = null, string userName = "jdoe") =>
        new()
        {
            General = new UserGeneralOutput { GUID = guid ?? Guid.NewGuid(), UserName = userName }
        };

    private static UserInput CreateUserInput(string userName = "newuser", params Guid[] roleGuids) =>
        new()
        {
            General = new UserGeneralInput { UserName = userName },
            Roles = roleGuids.Select(g => new RoleSummaryInput { GUID = g }).ToArray()
        };

    #region Get (all)

    [Test]
    public async Task Get_All_ReturnsOk_WithUserList()
    {
        var expected = new List<UserOutput> { CreateUserOutput(userName: "jdoe") };
        _userCrudServiceMock
            .Setup(x => x.GetUsersAsync("jdoe", It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _controller.Get("jdoe", CancellationToken.None);

        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult!.Value, Is.EqualTo(expected));
        _userCrudServiceMock.Verify(x => x.GetUsersAsync("jdoe", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Get_All_WithNullUserName_ReturnsAllUsers()
    {
        var expected = new List<UserOutput> { CreateUserOutput(), CreateUserOutput(userName: "asmith") };
        _userCrudServiceMock
            .Setup(x => x.GetUsersAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _controller.Get(null, CancellationToken.None);

        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That((IEnumerable<UserOutput>)okResult!.Value!, Has.Exactly(2).Items);
    }

    [Test]
    public async Task Get_All_ReturnsOk_WithEmptyList_WhenNoUsers()
    {
        _userCrudServiceMock
            .Setup(x => x.GetUsersAsync(It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserOutput>());

        var result = await _controller.Get("nomatch", CancellationToken.None);

        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That((IEnumerable<UserOutput>)okResult!.Value!, Is.Empty);
    }

    [TestCase("jdoe")]
    [TestCase("admin")]
    [TestCase("")]
    [TestCase("   ")]
    public async Task Get_All_ForwardsUserNameToService(string userName)
    {
        _userCrudServiceMock
            .Setup(x => x.GetUsersAsync(userName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserOutput>());

        await _controller.Get(userName, CancellationToken.None);

        _userCrudServiceMock.Verify(x => x.GetUsersAsync(userName, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Get_All_ForwardsCancellationTokenToService()
    {
        using var cts = new CancellationTokenSource();
        _userCrudServiceMock
            .Setup(x => x.GetUsersAsync(It.IsAny<string?>(), cts.Token))
            .ReturnsAsync(new List<UserOutput>());

        await _controller.Get("jdoe", cts.Token);

        _userCrudServiceMock.Verify(x => x.GetUsersAsync("jdoe", cts.Token), Times.Once);
    }

    [Test]
    public void Get_All_WhenServiceThrows_PropagatesException()
    {
        _userCrudServiceMock
            .Setup(x => x.GetUsersAsync(It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("boom"));

        Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _controller.Get("jdoe", CancellationToken.None));
    }

    #endregion

    #region Get (by guid)

    [Test]
    public async Task Get_ByGuid_ReturnsOk_WithUser()
    {
        var userGuid = Guid.NewGuid();
        var expected = CreateUserOutput(userGuid);
        _userCrudServiceMock
            .Setup(x => x.GetUserAsync(userGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _controller.Get(userGuid, CancellationToken.None);

        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult!.Value, Is.EqualTo(expected));
    }

    [Test]
    public async Task Get_ByGuid_ReturnsOk_WithNull_WhenUserNotFound()
    {
        var userGuid = Guid.NewGuid();
        _userCrudServiceMock
            .Setup(x => x.GetUserAsync(userGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserOutput?)null);

        var result = await _controller.Get(userGuid, CancellationToken.None);

        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult!.Value, Is.Null);
    }

    [Test]
    public async Task Get_ByGuid_WithEmptyGuid_ForwardsToService()
    {
        _userCrudServiceMock
            .Setup(x => x.GetUserAsync(Guid.Empty, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserOutput?)null);

        await _controller.Get(Guid.Empty, CancellationToken.None);

        _userCrudServiceMock.Verify(x => x.GetUserAsync(Guid.Empty, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Get_ByGuid_ForwardsCancellationTokenToService()
    {
        using var cts = new CancellationTokenSource();
        var userGuid = Guid.NewGuid();
        _userCrudServiceMock
            .Setup(x => x.GetUserAsync(userGuid, cts.Token))
            .ReturnsAsync(CreateUserOutput(userGuid));

        await _controller.Get(userGuid, cts.Token);

        _userCrudServiceMock.Verify(x => x.GetUserAsync(userGuid, cts.Token), Times.Once);
    }

    [Test]
    public void Get_ByGuid_WhenServiceThrows_PropagatesException()
    {
        _userCrudServiceMock
            .Setup(x => x.GetUserAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("boom"));

        Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _controller.Get(Guid.NewGuid(), CancellationToken.None));
    }

    #endregion

    #region Post

    [Test]
    public async Task Post_ReturnsCreatedAtAction_WithNewUser()
    {
        var userGuid = Guid.NewGuid();
        var userInput = CreateUserInput();
        var expected = CreateUserOutput(userGuid, "newuser");
        _userCrudServiceMock
            .Setup(x => x.CreateUserAsync(userInput, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _controller.Post(1, userInput, CancellationToken.None);

        var createdResult = result.Result as CreatedAtActionResult;
        Assert.That(createdResult, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(createdResult!.ActionName, Is.EqualTo("Get"));
            Assert.That(createdResult.Value, Is.EqualTo(expected));
        }
    }

    [Test]
    public async Task Post_SetsRouteValues_ForMemberFirmAndUserGuid()
    {
        const int memberFirmId = 99;
        var userGuid = Guid.NewGuid();
        var userInput = CreateUserInput();
        _userCrudServiceMock
            .Setup(x => x.CreateUserAsync(userInput, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateUserOutput(userGuid, "newuser"));

        var result = await _controller.Post(memberFirmId, userInput, CancellationToken.None);

        var createdResult = (CreatedAtActionResult)result.Result!;
        using (Assert.EnterMultipleScope())
        {
            Assert.That(createdResult.RouteValues!["memberFirmID"], Is.EqualTo(memberFirmId));
            Assert.That(createdResult.RouteValues!["userGuid"], Is.EqualTo(userGuid));
        }
    }

    [Test]
    public async Task Post_WithRoles_ForwardsInputToService()
    {
        var roleGuid = Guid.NewGuid();
        var userInput = CreateUserInput("newuser", roleGuid);
        _userCrudServiceMock
            .Setup(x => x.CreateUserAsync(It.Is<UserInput>(i => i.Roles.Length == 1 && i.Roles[0].GUID == roleGuid), It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateUserOutput(userName: "newuser"));

        await _controller.Post(1, userInput, CancellationToken.None);

        _userCrudServiceMock.Verify(
            x => x.CreateUserAsync(It.Is<UserInput>(i => i.Roles.Length == 1 && i.Roles[0].GUID == roleGuid), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task Post_ForwardsCancellationTokenToService()
    {
        using var cts = new CancellationTokenSource();
        var userInput = CreateUserInput();
        _userCrudServiceMock
            .Setup(x => x.CreateUserAsync(userInput, cts.Token))
            .ReturnsAsync(CreateUserOutput(userName: "newuser"));

        await _controller.Post(1, userInput, cts.Token);

        _userCrudServiceMock.Verify(x => x.CreateUserAsync(userInput, cts.Token), Times.Once);
    }

    [Test]
    public void Post_WhenServiceThrows_PropagatesException()
    {
        var userInput = CreateUserInput();
        _userCrudServiceMock
            .Setup(x => x.CreateUserAsync(userInput, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("duplicate"));

        Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _controller.Post(1, userInput, CancellationToken.None));
    }

    [Test]
    public void Post_WhenCallerUnauthorized_PropagatesException()
    {
        var userInput = CreateUserInput();
        _userCrudServiceMock
            .Setup(x => x.CreateUserAsync(userInput, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException("no access"));

        Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await _controller.Post(1, userInput, CancellationToken.None));
    }

    #endregion

    #region Put

    [Test]
    public async Task Put_ReturnsOk_WithUpdatedUser()
    {
        var userGuid = Guid.NewGuid();
        var userInput = CreateUserInput("updateduser");
        var expected = CreateUserOutput(userGuid, "updateduser");
        _userCrudServiceMock
            .Setup(x => x.UpdateUserAsync(userGuid, userInput, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _controller.Put(userGuid, userInput, CancellationToken.None);

        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult!.Value, Is.EqualTo(expected));
    }

    [Test]
    public async Task Put_ForwardsGuidAndInputToService()
    {
        var userGuid = Guid.NewGuid();
        var userInput = CreateUserInput("updateduser");
        _userCrudServiceMock
            .Setup(x => x.UpdateUserAsync(userGuid, userInput, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateUserOutput(userGuid, "updateduser"));

        await _controller.Put(userGuid, userInput, CancellationToken.None);

        _userCrudServiceMock.Verify(x => x.UpdateUserAsync(userGuid, userInput, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Put_ForwardsCancellationTokenToService()
    {
        using var cts = new CancellationTokenSource();
        var userGuid = Guid.NewGuid();
        var userInput = CreateUserInput("updateduser");
        _userCrudServiceMock
            .Setup(x => x.UpdateUserAsync(userGuid, userInput, cts.Token))
            .ReturnsAsync(CreateUserOutput(userGuid, "updateduser"));

        await _controller.Put(userGuid, userInput, cts.Token);

        _userCrudServiceMock.Verify(x => x.UpdateUserAsync(userGuid, userInput, cts.Token), Times.Once);
    }

    [Test]
    public void Put_WhenUserNotFound_PropagatesException()
    {
        var userGuid = Guid.NewGuid();
        var userInput = CreateUserInput("updateduser");
        _userCrudServiceMock
            .Setup(x => x.UpdateUserAsync(userGuid, userInput, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("not found"));

        Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _controller.Put(userGuid, userInput, CancellationToken.None));
    }

    #endregion

    #region Delete

    [Test]
    public async Task Delete_ReturnsNoContent()
    {
        var userGuid = Guid.NewGuid();
        _userCrudServiceMock
            .Setup(x => x.DeleteUserAsync(userGuid, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _controller.Delete(userGuid, CancellationToken.None);

        Assert.That(result, Is.InstanceOf<NoContentResult>());
        _userCrudServiceMock.Verify(x => x.DeleteUserAsync(userGuid, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Delete_ForwardsCancellationTokenToService()
    {
        using var cts = new CancellationTokenSource();
        var userGuid = Guid.NewGuid();
        _userCrudServiceMock
            .Setup(x => x.DeleteUserAsync(userGuid, cts.Token))
            .Returns(Task.CompletedTask);

        await _controller.Delete(userGuid, cts.Token);

        _userCrudServiceMock.Verify(x => x.DeleteUserAsync(userGuid, cts.Token), Times.Once);
    }

    [Test]
    public void Delete_WhenUserNotFound_PropagatesException()
    {
        var userGuid = Guid.NewGuid();
        _userCrudServiceMock
            .Setup(x => x.DeleteUserAsync(userGuid, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("not found"));

        Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _controller.Delete(userGuid, CancellationToken.None));
    }

    [Test]
    public void Delete_WhenCallerUnauthorized_PropagatesException()
    {
        var userGuid = Guid.NewGuid();
        _userCrudServiceMock
            .Setup(x => x.DeleteUserAsync(userGuid, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException("no access"));

        Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await _controller.Delete(userGuid, CancellationToken.None));
    }

    #endregion

    #region API permissions

    [Test]
    public async Task GetApiPermissions_ReturnsOk_AndUsesChannelIdFromHttpContext()
    {
        var userGuid = Guid.NewGuid();
        var expected = new Dictionary<string, ApiEndpointGroupPermissions>
        {
            ["Users"] = new() { Read = true, Manage = false }
        };
        _controller.HttpContext.Items["ChannelId"] = 42;

        _userApiPermissionsCrudServiceMock
            .Setup(x => x.GetUserApiPermissionsByEndpointGroupNameAsync(42, userGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _controller.GetApiPermissions(1, userGuid, CancellationToken.None);

        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult!.Value, Is.EqualTo(expected));
        _userApiPermissionsCrudServiceMock.Verify(
            x => x.GetUserApiPermissionsByEndpointGroupNameAsync(42, userGuid, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task GetApiPermissions_ForwardsCancellationToken()
    {
        using var cts = new CancellationTokenSource();
        var userGuid = Guid.NewGuid();
        _controller.HttpContext.Items["ChannelId"] = 9;

        _userApiPermissionsCrudServiceMock
            .Setup(x => x.GetUserApiPermissionsByEndpointGroupNameAsync(9, userGuid, cts.Token))
            .ReturnsAsync(new Dictionary<string, ApiEndpointGroupPermissions>());

        await _controller.GetApiPermissions(1, userGuid, cts.Token);

        _userApiPermissionsCrudServiceMock.Verify(
            x => x.GetUserApiPermissionsByEndpointGroupNameAsync(9, userGuid, cts.Token),
            Times.Once);
    }

    [Test]
    public async Task SetApiPermissions_ReturnsOk_AndForwardsInput()
    {
        var userGuid = Guid.NewGuid();
        var input = new Dictionary<string, ApiEndpointGroupPermissions>
        {
            ["Content"] = new() { Read = true, Manage = true }
        };
        var expected = new Dictionary<string, ApiEndpointGroupPermissions>
        {
            ["Content"] = new() { Read = true, Manage = true }
        };
        _controller.HttpContext.Items["ChannelId"] = 17;

        _userApiPermissionsCrudServiceMock
            .Setup(x => x.SetUserApiPermissionsByEndpointGroupNameAsync(17, userGuid, input, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _controller.SetApiPermissions(1, userGuid, input, CancellationToken.None);

        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult!.Value, Is.EqualTo(expected));
        _userApiPermissionsCrudServiceMock.Verify(
            x => x.SetUserApiPermissionsByEndpointGroupNameAsync(17, userGuid, input, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task SetApiPermissions_ForwardsCancellationToken()
    {
        using var cts = new CancellationTokenSource();
        var userGuid = Guid.NewGuid();
        var input = new Dictionary<string, ApiEndpointGroupPermissions>();
        _controller.HttpContext.Items["ChannelId"] = 17;

        _userApiPermissionsCrudServiceMock
            .Setup(x => x.SetUserApiPermissionsByEndpointGroupNameAsync(17, userGuid, input, cts.Token))
            .ReturnsAsync(new Dictionary<string, ApiEndpointGroupPermissions>());

        await _controller.SetApiPermissions(1, userGuid, input, cts.Token);

        _userApiPermissionsCrudServiceMock.Verify(
            x => x.SetUserApiPermissionsByEndpointGroupNameAsync(17, userGuid, input, cts.Token),
            Times.Once);
    }

    #endregion
}
