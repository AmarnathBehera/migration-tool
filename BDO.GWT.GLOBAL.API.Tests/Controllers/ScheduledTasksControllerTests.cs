using BDO.GWT.GLOBAL.API.Controllers;
using BDO.GWT.GLOBAL.API.DTOs;
using BDO.GWT.GLOBAL.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BDO.GWT.GLOBAL.API.Tests.Controllers;

[TestFixture]
public class ScheduledTasksControllerTests
{
    private Mock<IScheduledTaskCrudService> _scheduledTaskCrudServiceMock = null!;
    private ScheduledTasksController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _scheduledTaskCrudServiceMock = new Mock<IScheduledTaskCrudService>();
        _controller = new ScheduledTasksController(_scheduledTaskCrudServiceMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }

    private static ScheduledTaskDto CreateScheduledTaskDto(
        int id = 1,
        Guid? guid = null,
        string name = "TestTask",
        string displayName = "Test Task",
        string interval = "00:01:00",
        string identifier = "test.identifier") =>
        new()
        {
            Id = id,
            Guid = guid ?? Guid.NewGuid(),
            Name = name,
            DisplayName = displayName,
            Interval = interval,
            ScheduledTaskIdentifier = identifier,
            LastModified = DateTime.UtcNow
        };

    #region GetScheduledTasksAsync

    [Test]
    public async Task GetScheduledTasksAsync_ReturnsOk_WithTaskList()
    {
        var expected = new[]
        {
            CreateScheduledTaskDto(id: 1, name: "Task1"),
            CreateScheduledTaskDto(id: 2, name: "Task2")
        };
        _scheduledTaskCrudServiceMock
            .Setup(x => x.GetScheduledTasksDtosAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _controller.GetScheduledTasksAsync(CancellationToken.None);

        var okResult = result.Result as OkObjectResult;
        Assert.Multiple(() =>
        {
            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult!.Value, Is.EqualTo(expected));
        });
    }

    [Test]
    public async Task GetScheduledTasksAsync_ReturnsOk_WithEmptyArray()
    {
        _scheduledTaskCrudServiceMock
            .Setup(x => x.GetScheduledTasksDtosAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var result = await _controller.GetScheduledTasksAsync(CancellationToken.None);

        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult!.Value, Is.InstanceOf<ScheduledTaskDto[]>());
    }

    [Test]
    public async Task GetScheduledTasksAsync_CallsServiceOnce()
    {
        _scheduledTaskCrudServiceMock
            .Setup(x => x.GetScheduledTasksDtosAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        await _controller.GetScheduledTasksAsync(CancellationToken.None);

        _scheduledTaskCrudServiceMock.Verify(
            x => x.GetScheduledTasksDtosAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region GetScheduledTaskAsync

    [Test]
    public async Task GetScheduledTaskAsync_ReturnsOk_WithTask()
    {
        var taskGuid = Guid.NewGuid();
        var expected = CreateScheduledTaskDto(guid: taskGuid);
        _scheduledTaskCrudServiceMock
            .Setup(x => x.GetScheduledTaskDtoAsync(taskGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _controller.GetScheduledTaskAsync(taskGuid, CancellationToken.None);

        var okResult = result.Result as OkObjectResult;
        Assert.Multiple(() =>
        {
            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult!.Value, Is.EqualTo(expected));
        });
    }

    [Test]
    public async Task GetScheduledTaskAsync_PassesGuidToService()
    {
        var taskGuid = Guid.NewGuid();
        _scheduledTaskCrudServiceMock
            .Setup(x => x.GetScheduledTaskDtoAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateScheduledTaskDto());

        await _controller.GetScheduledTaskAsync(taskGuid, CancellationToken.None);

        _scheduledTaskCrudServiceMock.Verify(
            x => x.GetScheduledTaskDtoAsync(taskGuid, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task GetScheduledTaskAsync_ServiceThrows_PropagatesException()
    {
        var taskGuid = Guid.NewGuid();
        _scheduledTaskCrudServiceMock
            .Setup(x => x.GetScheduledTaskDtoAsync(taskGuid, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Not found"));

        Assert.ThrowsAsync<Exception>(
            () => _controller.GetScheduledTaskAsync(taskGuid, CancellationToken.None));
    }

    #endregion
}
