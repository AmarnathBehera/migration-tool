using BDO.GWT.Web.Core.LiveSite.Controllers.API;
using BDO.GWT.Web.Core.LiveSite.Data;
using BDO.GWT.Web.Core.LiveSite.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BDO.GWT.GLOBAL.Tests.Controllers.API;

public class NewsletterControllerTests
{
    private Mock<ISubscribeFormService> _subscribeFormServiceMock = null!;
    private NewsletterController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _subscribeFormServiceMock = new Mock<ISubscribeFormService>();
        _controller = new NewsletterController(_subscribeFormServiceMock.Object);
    }

    [TearDown]
    public void TearDown() => _controller.Dispose();

    private static NewsletterSubscriptionFormData BuildFormData() => new()
    {
        Email = "user@example.com",
        FirstName = "Jane",
        LastName = "Doe",
        RecaptchaToken = "valid-token",
        NewsletterGuids = [Guid.NewGuid()],
    };

    // --- Subscribe ---------------------------------------------------------------

    [Test]
    public async Task Subscribe_ReturnsOkResult()
    {
        var pageGuid = Guid.NewGuid();
        var formData = BuildFormData();
        _subscribeFormServiceMock
            .Setup(s => s.SubscribeToNewslettersAsync(pageGuid, formData, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _controller.Subscribe(pageGuid, formData, CancellationToken.None);

        Assert.That(result, Is.InstanceOf<OkObjectResult>());
    }

    [Test]
    public async Task Subscribe_WhenServiceReturnsTrue_ReturnsOkWithTrue()
    {
        var pageGuid = Guid.NewGuid();
        var formData = BuildFormData();
        _subscribeFormServiceMock
            .Setup(s => s.SubscribeToNewslettersAsync(pageGuid, formData, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = (OkObjectResult)await _controller.Subscribe(pageGuid, formData, CancellationToken.None);

        Assert.That(result.Value, Is.True);
    }

    [Test]
    public async Task Subscribe_WhenServiceReturnsFalse_ReturnsOkWithFalse()
    {
        var pageGuid = Guid.NewGuid();
        var formData = BuildFormData();
        _subscribeFormServiceMock
            .Setup(s => s.SubscribeToNewslettersAsync(pageGuid, formData, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = (OkObjectResult)await _controller.Subscribe(pageGuid, formData, CancellationToken.None);

        Assert.That(result.Value, Is.False);
    }

    [Test]
    public async Task Subscribe_PassesPageGuid_ToSubscribeFormService()
    {
        var pageGuid = Guid.NewGuid();
        var formData = BuildFormData();
        _subscribeFormServiceMock
            .Setup(s => s.SubscribeToNewslettersAsync(It.IsAny<Guid>(), It.IsAny<NewsletterSubscriptionFormData>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await _controller.Subscribe(pageGuid, formData, CancellationToken.None);

        _subscribeFormServiceMock.Verify(
            s => s.SubscribeToNewslettersAsync(pageGuid, It.IsAny<NewsletterSubscriptionFormData>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task Subscribe_PassesFormData_ToSubscribeFormService()
    {
        var pageGuid = Guid.NewGuid();
        var formData = BuildFormData();
        _subscribeFormServiceMock
            .Setup(s => s.SubscribeToNewslettersAsync(pageGuid, formData, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await _controller.Subscribe(pageGuid, formData, CancellationToken.None);

        _subscribeFormServiceMock.Verify(
            s => s.SubscribeToNewslettersAsync(pageGuid, formData, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task Subscribe_PassesCancellationToken_ToSubscribeFormService()
    {
        using var cts = new CancellationTokenSource();
        var token = cts.Token;
        var pageGuid = Guid.NewGuid();
        var formData = BuildFormData();
        _subscribeFormServiceMock
            .Setup(s => s.SubscribeToNewslettersAsync(pageGuid, formData, token))
            .ReturnsAsync(true);

        await _controller.Subscribe(pageGuid, formData, token);

        _subscribeFormServiceMock.Verify(
            s => s.SubscribeToNewslettersAsync(pageGuid, formData, token),
            Times.Once);
    }

    [Test]
    public void Subscribe_PropagatesException_FromSubscribeFormService()
    {
        var pageGuid = Guid.NewGuid();
        var formData = BuildFormData();
        _subscribeFormServiceMock
            .Setup(s => s.SubscribeToNewslettersAsync(pageGuid, formData, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("service error"));

        Assert.ThrowsAsync<InvalidOperationException>(
            () => _controller.Subscribe(pageGuid, formData, CancellationToken.None));
    }

    [Test]
    public async Task Subscribe_CallsSubscribeFormService_ExactlyOnce()
    {
        var pageGuid = Guid.NewGuid();
        var formData = BuildFormData();
        _subscribeFormServiceMock
            .Setup(s => s.SubscribeToNewslettersAsync(It.IsAny<Guid>(), It.IsAny<NewsletterSubscriptionFormData>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await _controller.Subscribe(pageGuid, formData, CancellationToken.None);

        _subscribeFormServiceMock.Verify(
            s => s.SubscribeToNewslettersAsync(It.IsAny<Guid>(), It.IsAny<NewsletterSubscriptionFormData>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
