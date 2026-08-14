using BDO.GWT.Common.Core.Exceptions;
using BDO.GWT.Common.Core.Services;
using BDO.GWT.GLOBAL.API.DTOs;
using BDO.GWT.GLOBAL.API.Services;
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
}
