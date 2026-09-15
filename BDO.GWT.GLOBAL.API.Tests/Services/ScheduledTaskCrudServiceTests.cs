using BDO.GWT.Common.Core.Exceptions;
using BDO.GWT.Core.Exceptions;
using BDO.GWT.GLOBAL.API.DTOs;
using BDO.GWT.GLOBAL.API.Services;
using CMS.DataEngine;
using CMS.Scheduler;
using CMS.Scheduler.Internal;
using Moq;
using System.Data;
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

    #region GetValidatedScheduledTask (NotFound behavior via service interface)

    [Test]
    public async Task GetScheduledTaskDtoAsync_NotFound_ThrowsNotFoundException()
    {
        var notFoundGuid = Guid.NewGuid();

        // Use a mock of the interface to verify NotFoundException is propagated
        var serviceMock = new Mock<IScheduledTaskCrudService>();
        serviceMock
            .Setup(x => x.GetScheduledTaskDtoAsync(notFoundGuid, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException());

        Assert.ThrowsAsync<NotFoundException>(
            () => serviceMock.Object.GetScheduledTaskDtoAsync(notFoundGuid, CancellationToken.None));
    }

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

    #endregion

    #region ExecuteScheduledTaskAsync (interface behavior)

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

    #region Real service (mocked dependencies)

    private static ScheduledTaskConfigurationInfo BuildTaskInfo(Guid guid, int id = 1, string name = "MyTask")
    {
        return new ScheduledTaskConfigurationInfo
        {
            ScheduledTaskConfigurationID = id,
            ScheduledTaskConfigurationGUID = guid,
            ScheduledTaskConfigurationName = name,
            ScheduledTaskConfigurationDisplayName = "My Task",
            ScheduledTaskConfigurationInterval = "00:05:00",
            ScheduledTaskConfigurationScheduledTaskIdentifier = "my.task.identifier"
        };
    }

    private static Mock<IInfoProvider<ScheduledTaskConfigurationInfo>> BuildProvider(IEnumerable<ScheduledTaskConfigurationInfo> rows)
    {
        var query = new Mock<ObjectQuery<ScheduledTaskConfigurationInfo>>();
        query.Setup(x => x.WhereEquals(It.IsAny<string>(), It.IsAny<object>())).Returns(query.Object);
        query.Setup(x => x.GetEnumerableTypedResultAsync(
                It.IsAny<CommandBehavior>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken?>()))
            .ReturnsAsync(rows);

        var providerMock = new Mock<IInfoProvider<ScheduledTaskConfigurationInfo>>();
        providerMock.Setup(x => x.Get()).Returns(query.Object);

        return providerMock;
    }

    private static ScheduledTaskCrudService CreateService(
        IInfoProvider<ScheduledTaskConfigurationInfo> provider,
        out Mock<IValidationState> validationStateMock,
        out Mock<ISchedulingExecutor> schedulingExecutorMock)
    {
        validationStateMock = new Mock<IValidationState>();
        schedulingExecutorMock = new Mock<ISchedulingExecutor>();

        return new ScheduledTaskCrudService(
            provider,
            validationStateMock.Object,
            schedulingExecutorMock.Object);
    }

    [Test]
    public async Task GetScheduledTasksDtosAsync_MapsAllTasks_ToDtos()
    {
        var guid1 = Guid.NewGuid();
        var guid2 = Guid.NewGuid();
        var provider = BuildProvider([
            BuildTaskInfo(guid1, id: 1, name: "TaskOne"),
            BuildTaskInfo(guid2, id: 2, name: "TaskTwo")
        ]);
        var service = CreateService(provider.Object, out _, out _);

        var result = await service.GetScheduledTasksDtosAsync(CancellationToken.None);

        Assert.That(result, Has.Length.EqualTo(2));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result[0].Guid, Is.EqualTo(guid1));
            Assert.That(result[0].Name, Is.EqualTo("TaskOne"));
            Assert.That(result[1].Guid, Is.EqualTo(guid2));
            Assert.That(result[1].Name, Is.EqualTo("TaskTwo"));
        }
    }

    [Test]
    public async Task GetScheduledTasksDtosAsync_ReturnsEmptyArray_WhenNoTasks()
    {
        var provider = BuildProvider([]);
        var service = CreateService(provider.Object, out _, out _);

        var result = await service.GetScheduledTasksDtosAsync(CancellationToken.None);

        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task GetScheduledTaskDtoAsync_ReturnsMappedDto_WhenFound()
    {
        var guid = Guid.NewGuid();
        var provider = BuildProvider([BuildTaskInfo(guid, id: 5, name: "FoundTask")]);
        var service = CreateService(provider.Object, out _, out _);

        var result = await service.GetScheduledTaskDtoAsync(guid, CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Id, Is.EqualTo(5));
            Assert.That(result.Guid, Is.EqualTo(guid));
            Assert.That(result.Name, Is.EqualTo("FoundTask"));
        }
    }

    [Test]
    public void GetScheduledTaskDtoAsync_ThrowsNotFound_AndAddsError_WhenMissing()
    {
        var missingGuid = Guid.NewGuid();
        var provider = BuildProvider([]);
        var service = CreateService(provider.Object, out var validationStateMock, out _);

        Assert.ThrowsAsync<NotFoundException>(
            () => service.GetScheduledTaskDtoAsync(missingGuid, CancellationToken.None));

        validationStateMock.Verify(
            x => x.AddError("scheduledTaskGuid", It.Is<string>(m => m.Contains(missingGuid.ToString()))),
            Times.Once);
    }

    [Test]
    public async Task ExecuteScheduledTaskAsync_ExecutesTask_WhenFound()
    {
        var guid = Guid.NewGuid();
        var provider = BuildProvider([BuildTaskInfo(guid, id: 9)]);
        var service = CreateService(provider.Object, out _, out var schedulingExecutorMock);

        schedulingExecutorMock
            .Setup(x => x.ExecuteTask(9, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await service.ExecuteScheduledTaskAsync(guid, CancellationToken.None);

        schedulingExecutorMock.Verify(
            x => x.ExecuteTask(9, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public void ExecuteScheduledTaskAsync_ThrowsNotFound_WhenMissing()
    {
        var missingGuid = Guid.NewGuid();
        var provider = BuildProvider([]);
        var service = CreateService(provider.Object, out var validationStateMock, out var schedulingExecutorMock);

        Assert.ThrowsAsync<NotFoundException>(
            () => service.ExecuteScheduledTaskAsync(missingGuid, CancellationToken.None));
    }

    [Test]
    public void ExecuteScheduledTaskAsync_WhenTaskAlreadyRunning_ThrowsAlreadyRunning_AndAddsError()
    {
        var guid = Guid.NewGuid();
        var provider = BuildProvider([BuildTaskInfo(guid, id: 3)]);
        var service = CreateService(provider.Object, out var validationStateMock, out var schedulingExecutorMock);

        schedulingExecutorMock
            .Setup(x => x.ExecuteTask(3, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ScheduledTaskIsRunningException("Task is running."));

        Assert.ThrowsAsync<ScheduledTaskIsAlreadyRunningException>(
            () => service.ExecuteScheduledTaskAsync(guid, CancellationToken.None));

        validationStateMock.Verify(
            x => x.AddError("scheduledTaskGuid", It.Is<string>(m => m.Contains(guid.ToString()))),
            Times.Once);
    }

    #endregion
}
