using BDO.GWT.GLOBAL.API.Controllers;
using BDO.GWT.GLOBAL.API.Data;
using BDO.GWT.GLOBAL.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BDO.GWT.GLOBAL.API.Tests.Controllers;

public class UsersControllerTests
{
    private Mock<IUserCrudService> _userCrudServiceMock = null!;
    private UsersController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _userCrudServiceMock = new Mock<IUserCrudService>();
        _controller = new UsersController(_userCrudServiceMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }

    [Test]
    public async Task Get_All_ReturnsOk_WithUserList()
    {
        var expected = new List<UserOutput>
        {
            new UserOutput
            {
                General = new UserGeneralOutput { GUID = Guid.NewGuid(), UserName = "jdoe" }
            }
        };

        _userCrudServiceMock
            .Setup(x => x.GetUsersAsync("jdoe", It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _controller.Get("jdoe", CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = (OkObjectResult)result.Result!;
        Assert.That(okResult.Value, Is.EqualTo(expected));

        _userCrudServiceMock.Verify(
            x => x.GetUsersAsync("jdoe", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task Get_All_ReturnsOk_WithNullUserName_ReturnsAllUsers()
    {
        var expected = new List<UserOutput>();

        _userCrudServiceMock
            .Setup(x => x.GetUsersAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _controller.Get(null, CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());

        _userCrudServiceMock.Verify(
            x => x.GetUsersAsync(null, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task Get_ByGuid_ReturnsOk_WithUser()
    {
        var userGuid = Guid.NewGuid();
        var expected = new UserOutput
        {
            General = new UserGeneralOutput { GUID = userGuid, UserName = "jdoe" }
        };

        _userCrudServiceMock
            .Setup(x => x.GetUserAsync(userGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _controller.Get(userGuid, CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = (OkObjectResult)result.Result!;
        Assert.That(okResult.Value, Is.EqualTo(expected));

        _userCrudServiceMock.Verify(
            x => x.GetUserAsync(userGuid, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task Get_ByGuid_ReturnsOk_WithNull_WhenUserNotFound()
    {
        var userGuid = Guid.NewGuid();

        _userCrudServiceMock
            .Setup(x => x.GetUserAsync(userGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserOutput?)null);

        var result = await _controller.Get(userGuid, CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = (OkObjectResult)result.Result!;
        Assert.That(okResult.Value, Is.Null);
    }

    [Test]
    public async Task Post_ReturnsCreatedAtAction_WithNewUser()
    {
        var userGuid = Guid.NewGuid();
        var userInput = new UserInput
        {
            General = new UserGeneralInput { UserName = "newuser" }
        };
        var expected = new UserOutput
        {
            General = new UserGeneralOutput { GUID = userGuid, UserName = "newuser" }
        };

        _userCrudServiceMock
            .Setup(x => x.CreateUserAsync(userInput, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _controller.Post(1, userInput, CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<CreatedAtActionResult>());
        var createdResult = (CreatedAtActionResult)result.Result!;
        using (Assert.EnterMultipleScope())
        {
            Assert.That(createdResult.ActionName, Is.EqualTo("Get"));
            Assert.That(createdResult.Value, Is.EqualTo(expected));
        }

        _userCrudServiceMock.Verify(
            x => x.CreateUserAsync(userInput, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task Put_ReturnsOk_WithUpdatedUser()
    {
        var userGuid = Guid.NewGuid();
        var userInput = new UserInput
        {
            General = new UserGeneralInput { UserName = "updateduser" }
        };
        var expected = new UserOutput
        {
            General = new UserGeneralOutput { GUID = userGuid, UserName = "updateduser" }
        };

        _userCrudServiceMock
            .Setup(x => x.UpdateUserAsync(userGuid, userInput, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _controller.Put(userGuid, userInput, CancellationToken.None);

        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = (OkObjectResult)result.Result!;
        Assert.That(okResult.Value, Is.EqualTo(expected));

        _userCrudServiceMock.Verify(
            x => x.UpdateUserAsync(userGuid, userInput, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task Delete_ReturnsNoContent()
    {
        var userGuid = Guid.NewGuid();

        _userCrudServiceMock
            .Setup(x => x.DeleteUserAsync(userGuid, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _controller.Delete(userGuid, CancellationToken.None);

        Assert.That(result, Is.InstanceOf<NoContentResult>());

        _userCrudServiceMock.Verify(
            x => x.DeleteUserAsync(userGuid, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
