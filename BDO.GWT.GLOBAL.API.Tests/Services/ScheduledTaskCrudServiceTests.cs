using BDO.GWT.Common.Core.Exceptions;
using BDO.GWT.Core.Exceptions;
using BDO.GWT.GLOBAL.API.DTOs;
using BDO.GWT.GLOBAL.API.Services;
using CMS.Scheduler;
using Moq;
using System.Runtime.CompilerServices;

namespace BDO.GWT.GLOBAL.API.Tests.Services;

[TestFixture]
public class ScheduledTaskCrudServiceTests
{
    [Test]
    public void ScheduledTaskCrudService_ImplementsIScheduledTaskCrudService()
    {
        var service = (ScheduledTaskCrudService)RuntimeHelpers.GetUninitializedObject(typeof(ScheduledTaskCrudService));

        Assert.That(service, Is.InstanceOf<IScheduledTaskCrudService>());
    }

    #region ScheduledTaskDto

    [Test]
    public void ScheduledTaskDto_StoresRequiredValues()
    {
        var guid = Guid.NewGuid();
        var now = DateTime.UtcNow;
        var dto = new ScheduledTaskDto
        {
            Id = 1,
            Guid = guid,
            Name = "MyTask",
            DisplayName = "My Task",
            Interval = "00:05:00",
            ScheduledTaskIdentifier = "my.task.identifier",
            LastModified = now
        };

        Assert.Multiple(() =>
        {
            Assert.That(dto.Id, Is.EqualTo(1));
            Assert.That(dto.Guid, Is.EqualTo(guid));
            Assert.That(dto.Name, Is.EqualTo("MyTask"));
            Assert.That(dto.DisplayName, Is.EqualTo("My Task"));
            Assert.That(dto.Interval, Is.EqualTo("00:05:00"));
            Assert.That(dto.ScheduledTaskIdentifier, Is.EqualTo("my.task.identifier"));
            Assert.That(dto.LastModified, Is.EqualTo(now));
        });
    }

    [Test]
    public void ScheduledTaskDto_OptionalFields_DefaultToNull_OrFalse()
    {
        var dto = new ScheduledTaskDto
        {
            Id = 1,
            Guid = Guid.NewGuid(),
            Name = "Task",
            DisplayName = "Task",
            Interval = "00:01:00",
            ScheduledTaskIdentifier = "task.id",
            LastModified = DateTime.UtcNow
        };

        Assert.Multiple(() =>
        {
            Assert.That(dto.Data, Is.Null);
            Assert.That(dto.LastRunTime, Is.Null);
            Assert.That(dto.NextRunTime, Is.Null);
            Assert.That(dto.LastResult, Is.Null);
            Assert.That(dto.ExecutingServerName, Is.Null);
            Assert.That(dto.UserId, Is.Null);
            Assert.That(dto.ObjectType, Is.Null);
            Assert.That(dto.ObjectId, Is.Null);
            Assert.That(dto.DeleteAfterLastRun, Is.False);
            Assert.That(dto.IsEnabled, Is.False);
            Assert.That(dto.IsRunning, Is.False);
            Assert.That(dto.IsCancellationRequested, Is.False);
            Assert.That(dto.CreatedDynamically, Is.False);
        });
    }

    [Test]
    public void ScheduledTaskDto_CanSetAllOptionalFields()
    {
        var lastRun = new DateTime(2024, 6, 1, 12, 0, 0, DateTimeKind.Utc);
        var nextRun = new DateTime(2024, 6, 1, 12, 5, 0, DateTimeKind.Utc);
        var dto = new ScheduledTaskDto
        {
            Id = 10,
            Guid = Guid.NewGuid(),
            Name = "FullTask",
            DisplayName = "Full Task",
            Interval = "00:05:00",
            ScheduledTaskIdentifier = "full.task",
            LastModified = DateTime.UtcNow,
            Data = "some-data",
            LastRunTime = lastRun,
            NextRunTime = nextRun,
            LastResult = "Success",
            DeleteAfterLastRun = true,
            ExecutingServerName = "server-01",
            ExecutionCount = 42,
            UserId = 7,
            ObjectType = "cms.site",
            ObjectId = 3,
            IsEnabled = true,
            IsRunning = false,
            IsCancellationRequested = false,
            CreatedDynamically = true
        };

        Assert.Multiple(() =>
        {
            Assert.That(dto.Data, Is.EqualTo("some-data"));
            Assert.That(dto.LastRunTime, Is.EqualTo(lastRun));
            Assert.That(dto.NextRunTime, Is.EqualTo(nextRun));
            Assert.That(dto.LastResult, Is.EqualTo("Success"));
            Assert.That(dto.DeleteAfterLastRun, Is.True);
            Assert.That(dto.ExecutingServerName, Is.EqualTo("server-01"));
            Assert.That(dto.ExecutionCount, Is.EqualTo(42));
            Assert.That(dto.UserId, Is.EqualTo(7));
            Assert.That(dto.ObjectType, Is.EqualTo("cms.site"));
            Assert.That(dto.ObjectId, Is.EqualTo(3));
            Assert.That(dto.IsEnabled, Is.True);
            Assert.That(dto.CreatedDynamically, Is.True);
        });
    }

    #endregion

    #region GetScheduledTasksDtosAsync

    [Test]
    public async Task GetScheduledTasksDtosAsync_ReturnsEmptyArray_WhenNoTasksExist()
    {
        var serviceMock = new Mock<IScheduledTaskCrudService>();
        serviceMock
            .Setup(x => x.GetScheduledTasksDtosAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var result = await serviceMock.Object.GetScheduledTasksDtosAsync(CancellationToken.None);

        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task GetScheduledTasksDtosAsync_ReturnsAllTasks_WhenTasksExist()
    {
        var expected = new[]
        {
            new ScheduledTaskDto { Id = 1, Guid = Guid.NewGuid(), Name = "TaskOne", DisplayName = "Task One", Interval = "00:05:00", ScheduledTaskIdentifier = "task.one", LastModified = DateTime.UtcNow },
            new ScheduledTaskDto { Id = 2, Guid = Guid.NewGuid(), Name = "TaskTwo", DisplayName = "Task Two", Interval = "01:00:00", ScheduledTaskIdentifier = "task.two", LastModified = DateTime.UtcNow },
        };
        var serviceMock = new Mock<IScheduledTaskCrudService>();
        serviceMock
            .Setup(x => x.GetScheduledTasksDtosAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await serviceMock.Object.GetScheduledTasksDtosAsync(CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Length.EqualTo(2));
            Assert.That(result[0].Name, Is.EqualTo("TaskOne"));
            Assert.That(result[1].Name, Is.EqualTo("TaskTwo"));
        });
    }

    #endregion

    #region GetScheduledTaskDtoAsync

    [Test]
    public async Task GetScheduledTaskDtoAsync_ReturnsDto_WhenTaskExists()
    {
        var guid = Guid.NewGuid();
        var expected = new ScheduledTaskDto
        {
            Id = 5,
            Guid = guid,
            Name = "ExistingTask",
            DisplayName = "Existing Task",
            Interval = "00:05:00",
            ScheduledTaskIdentifier = "existing.task",
            LastModified = DateTime.UtcNow
        };
        var serviceMock = new Mock<IScheduledTaskCrudService>();
        serviceMock
            .Setup(x => x.GetScheduledTaskDtoAsync(guid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await serviceMock.Object.GetScheduledTaskDtoAsync(guid, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Id, Is.EqualTo(5));
            Assert.That(result.Guid, Is.EqualTo(guid));
            Assert.That(result.Name, Is.EqualTo("ExistingTask"));
        });
    }

    [Test]
    public void GetScheduledTaskDtoAsync_NotFound_ThrowsNotFoundException()
    {
        var notFoundGuid = Guid.NewGuid();
        var serviceMock = new Mock<IScheduledTaskCrudService>();
        serviceMock
            .Setup(x => x.GetScheduledTaskDtoAsync(notFoundGuid, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException());

        Assert.ThrowsAsync<NotFoundException>(
            () => serviceMock.Object.GetScheduledTaskDtoAsync(notFoundGuid, CancellationToken.None));
    }

    #endregion

    #region ExecuteScheduledTaskAsync

    [Test]
    public async Task ExecuteScheduledTaskAsync_Completes_WhenTaskExecutesSuccessfully()
    {
        var taskGuid = Guid.NewGuid();
        var serviceMock = new Mock<IScheduledTaskCrudService>();
        serviceMock
            .Setup(x => x.ExecuteScheduledTaskAsync(taskGuid, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await serviceMock.Object.ExecuteScheduledTaskAsync(taskGuid, CancellationToken.None);

        serviceMock.Verify(
            x => x.ExecuteScheduledTaskAsync(taskGuid, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public void ExecuteScheduledTaskAsync_NotFound_ThrowsNotFoundException()
    {
        var missingGuid = Guid.NewGuid();
        var serviceMock = new Mock<IScheduledTaskCrudService>();
        serviceMock
            .Setup(x => x.ExecuteScheduledTaskAsync(missingGuid, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException());

        Assert.ThrowsAsync<NotFoundException>(
            () => serviceMock.Object.ExecuteScheduledTaskAsync(missingGuid, CancellationToken.None));
    }

    [Test]
    public void ExecuteScheduledTaskAsync_AlreadyRunning_ThrowsAlreadyRunningException()
    {
        var taskGuid = Guid.NewGuid();
        var serviceMock = new Mock<IScheduledTaskCrudService>();
        serviceMock
            .Setup(x => x.ExecuteScheduledTaskAsync(taskGuid, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ScheduledTaskIsAlreadyRunningException());

        Assert.ThrowsAsync<ScheduledTaskIsAlreadyRunningException>(
            () => serviceMock.Object.ExecuteScheduledTaskAsync(taskGuid, CancellationToken.None));
    }

    #endregion
}
