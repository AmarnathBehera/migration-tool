using BDO.GWT.GLOBAL.API.Controllers;
using BDO.GWT.GLOBAL.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BDO.GWT.GLOBAL.API.Tests.Controllers;

[TestFixture]
public class EventLogControllerTests
{
    private Mock<IEventLogCrudService> _eventLogCrudServiceMock = null!;
    private EventLogController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _eventLogCrudServiceMock = new Mock<IEventLogCrudService>();
        _controller = new EventLogController(_eventLogCrudServiceMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }

    [Test]
    public async Task DeleteAllEventLogEntries_ReturnsNoContent()
    {
        _eventLogCrudServiceMock
            .Setup(x => x.DeleteAllEventLogEntriesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _controller.DeleteAllEventLogEntries(CancellationToken.None);

        Assert.That(result, Is.InstanceOf<NoContentResult>());
    }

    [Test]
    public async Task DeleteAllEventLogEntries_CallsServiceOnce()
    {
        _eventLogCrudServiceMock
            .Setup(x => x.DeleteAllEventLogEntriesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _controller.DeleteAllEventLogEntries(CancellationToken.None);

        _eventLogCrudServiceMock.Verify(
            x => x.DeleteAllEventLogEntriesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public void DeleteAllEventLogEntries_WhenServiceThrows_PropagatesException()
    {
        _eventLogCrudServiceMock
            .Setup(x => x.DeleteAllEventLogEntriesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("boom"));

        Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _controller.DeleteAllEventLogEntries(CancellationToken.None));
    }

    [Test]
    public async Task DeleteAllEventLogEntries_ForwardsCancellationTokenToService()
    {
        using var cts = new CancellationTokenSource();
        _eventLogCrudServiceMock
            .Setup(x => x.DeleteAllEventLogEntriesAsync(cts.Token))
            .Returns(Task.CompletedTask);

        await _controller.DeleteAllEventLogEntries(cts.Token);

        _eventLogCrudServiceMock.Verify(x => x.DeleteAllEventLogEntriesAsync(cts.Token), Times.Once);
    }

    [Test]
    public async Task DeleteAllEventLogEntries_ReturnsNoContent_WithNull204StatusCode()
    {
        _eventLogCrudServiceMock
            .Setup(x => x.DeleteAllEventLogEntriesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _controller.DeleteAllEventLogEntries(CancellationToken.None) as NoContentResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.StatusCode, Is.EqualTo(StatusCodes.Status204NoContent));
    }

    [Test]
    public void Controller_Constructor_StoresService()
    {
        var field = typeof(EventLogController)
            .GetField("_eventLogCrudService", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

        Assert.That(field, Is.Not.Null);
        Assert.That(field!.GetValue(_controller), Is.SameAs(_eventLogCrudServiceMock.Object));
    }
}
