using BDO.GWT.GLOBAL.Sync.Controllers;
using BDO.GWT.GLOBAL.Sync.Services;
using Moq;

namespace BDO.GWT.GLOBAL.Sync.Tests.Controllers;

[TestFixture]
public class GlobalSiteControllerTests
{
    private Mock<IGlobalDealsSynchronizer> _synchronizerMock = null!;
    private GlobalSiteController _controller = null!;

    [SetUp]
    public void SetUp()
    {
        _synchronizerMock = new Mock<IGlobalDealsSynchronizer>(MockBehavior.Strict);
        _controller = new GlobalSiteController(_synchronizerMock.Object);
    }

    [Test]
    public async Task SynchronizeGlobalDeals_WhenForceUpdateTrue_CallsSynchronizerWithTrue()
    {
        _synchronizerMock
            .Setup(s => s.SynchronizeGlobalDealsAsync(true, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _controller.SynchronizeGlobalDeals(forceUpdate: true);

        _synchronizerMock.Verify(
            s => s.SynchronizeGlobalDealsAsync(true, CancellationToken.None),
            Times.Once);
        _synchronizerMock.VerifyNoOtherCalls();
    }

    [Test]
    public async Task SynchronizeGlobalDeals_WhenForceUpdateFalse_CallsSynchronizerWithFalse()
    {
        _synchronizerMock
            .Setup(s => s.SynchronizeGlobalDealsAsync(false, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _controller.SynchronizeGlobalDeals(forceUpdate: false);

        _synchronizerMock.Verify(
            s => s.SynchronizeGlobalDealsAsync(false, CancellationToken.None),
            Times.Once);
        _synchronizerMock.VerifyNoOtherCalls();
    }

    [Test]
    public void SynchronizeGlobalDeals_WhenSynchronizerThrows_PropagatesException()
    {
        var expected = new InvalidOperationException("sync failed");
        _synchronizerMock
            .Setup(s => s.SynchronizeGlobalDealsAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(expected);

        var actual = Assert.ThrowsAsync<InvalidOperationException>(
            () => _controller.SynchronizeGlobalDeals(forceUpdate: true));

        Assert.That(actual, Is.SameAs(expected));
    }

    [Test]
    public void Constructor_WhenSynchronizerProvided_DoesNotThrow()
    {
        Assert.DoesNotThrow(() => new GlobalSiteController(_synchronizerMock.Object));
    }
}
