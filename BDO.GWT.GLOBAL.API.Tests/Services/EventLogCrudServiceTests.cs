using BDO.GWT.GLOBAL.API.Services;
using CMS.DataEngine;
using CMS.EventLog;
using Moq;
using System.Runtime.CompilerServices;

namespace BDO.GWT.GLOBAL.API.Tests.Services;

[TestFixture]
public class EventLogCrudServiceTests
{
    private Mock<IInfoProvider<EventLogInfo>> _eventLogInfoProviderMock = null!;
    private EventLogCrudService _service = null!;

    [SetUp]
    public void Setup()
    {
        _eventLogInfoProviderMock = new Mock<IInfoProvider<EventLogInfo>>();
        _service = new EventLogCrudService(_eventLogInfoProviderMock.Object);
    }

    [Test]
    public void EventLogCrudService_ImplementsIEventLogCrudService()
    {
        var service = (EventLogCrudService)RuntimeHelpers.GetUninitializedObject(typeof(EventLogCrudService));

        Assert.That(service, Is.InstanceOf<IEventLogCrudService>());
    }

    [Test]
    public void EventLogCrudService_Interface_HasExpectedMethods()
    {
        var interfaceType = typeof(IEventLogCrudService);

        Assert.That(interfaceType.GetMethod(nameof(IEventLogCrudService.DeleteAllEventLogEntriesAsync)), Is.Not.Null);
    }

    [Test]
    public void DeleteAllEventLogEntriesAsync_WhenProviderUnavailable_Throws()
    {
        var service = (EventLogCrudService)RuntimeHelpers.GetUninitializedObject(typeof(EventLogCrudService));

        // BulkDelete is an extension method that validates the provider argument.
        Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await service.DeleteAllEventLogEntriesAsync(CancellationToken.None));
    }

    [Test]
    public void Constructor_WithProvider_CreatesInstance()
    {
        var service = new EventLogCrudService(_eventLogInfoProviderMock.Object);

        Assert.That(service, Is.Not.Null);
    }

    [Test]
    public void Constructor_AssignsProviderField()
    {
        var service = new EventLogCrudService(_eventLogInfoProviderMock.Object);

        var field = typeof(EventLogCrudService)
            .GetField("_eventLogInfoProvider", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

        Assert.That(field, Is.Not.Null);
        Assert.That(field!.GetValue(service), Is.SameAs(_eventLogInfoProviderMock.Object));
    }

    [Test]
    public void IEventLogCrudService_DeleteAllEventLogEntriesAsync_ReturnsTask()
    {
        var method = typeof(IEventLogCrudService).GetMethod(nameof(IEventLogCrudService.DeleteAllEventLogEntriesAsync));

        Assert.That(method!.ReturnType, Is.EqualTo(typeof(Task)));
    }

    [Test]
    public void IEventLogCrudService_DeleteAllEventLogEntriesAsync_AcceptsCancellationToken()
    {
        var method = typeof(IEventLogCrudService).GetMethod(nameof(IEventLogCrudService.DeleteAllEventLogEntriesAsync));

        Assert.That(
            method!.GetParameters().Any(p => p.ParameterType == typeof(CancellationToken)),
            Is.True);
    }
}
