using BDO;
using BDO.GWT.Core;
using BDO.GWT.Core.Utilities;
using BDO.GWT.Web.Core.Data.Queries;
using BDO.GWT.Web.Core.DTOs.Pages;
using BDO.GWT.Web.Core.LiveSite.Services;
using BDO.GWT.Xperience.Core.LiveSite.Services;
using Moq;

namespace BDO.GWT.Core.Tests.Services;

public class SubscribeRemovePageServiceTests
{
    private Mock<INewsletterSubscriptionService> _newsletterSubscriptionServiceMock = null!;
    private SubscribeRemovePageService _service = null!;

    [SetUp]
    public void Setup()
    {
        _newsletterSubscriptionServiceMock = new Mock<INewsletterSubscriptionService>();

        _service = new SubscribeRemovePageService(
            _newsletterSubscriptionServiceMock.Object);
    }

    private static SubscribeRemoveQuery BuildQuery() => new()
    {
        Email = "user@example.com",
        NewsletterGuid = Guid.NewGuid(),
        Hash = "validhash",
    };

    private static SubscribeRemove BuildPage(
        string successMsg = "You have been unsubscribed.",
        string failMsg = "Unsubscription failed.") =>
        new()
        {
            SuccessfulUnubscribeMessage = successMsg,
            UnsuccessfulUnsubscribeMessage = failMsg,
        };

    // --- GetSubscribeRemoveDtoAsync ---------------------------------------------

    [Test]
    public async Task GetSubscribeRemoveDtoAsync_ReturnsDto()
    {
        var page = BuildPage();
        var query = BuildQuery();
        _newsletterSubscriptionServiceMock
            .Setup(s => s.UnsubscribeFromNewsletterAsync(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _service.GetSubscribeRemoveDtoAsync(page, query, CancellationToken.None);

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<SubscribeRemoveDto>());
    }

    [Test]
    public async Task GetSubscribeRemoveDtoAsync_ReturnsSuccessMessage_WhenUnsubscribeSucceeds()
    {
        const string successMsg = "You have been successfully unsubscribed.";
        var page = BuildPage(successMsg: successMsg);
        var query = BuildQuery();
        _newsletterSubscriptionServiceMock
            .Setup(s => s.UnsubscribeFromNewsletterAsync(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _service.GetSubscribeRemoveDtoAsync(page, query, CancellationToken.None);

        Assert.That(result.Message, Is.EqualTo(successMsg));
    }

    [Test]
    public async Task GetSubscribeRemoveDtoAsync_ReturnsUnsuccessfulMessage_WhenUnsubscribeFails()
    {
        const string failMsg = "Could not unsubscribe.";
        var page = BuildPage(failMsg: failMsg);
        var query = BuildQuery();
        _newsletterSubscriptionServiceMock
            .Setup(s => s.UnsubscribeFromNewsletterAsync(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _service.GetSubscribeRemoveDtoAsync(page, query, CancellationToken.None);

        Assert.That(result.Message, Is.EqualTo(failMsg));
    }

    [Test]
    public async Task GetSubscribeRemoveDtoAsync_CallsUnsubscribe_WithProvidedQuery()
    {
        var page = BuildPage();
        var query = BuildQuery();
        _newsletterSubscriptionServiceMock
            .Setup(s => s.UnsubscribeFromNewsletterAsync(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await _service.GetSubscribeRemoveDtoAsync(page, query, CancellationToken.None);

        _newsletterSubscriptionServiceMock.Verify(
            s => s.UnsubscribeFromNewsletterAsync(query, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task GetSubscribeRemoveDtoAsync_PassesCancellationToken_ToNewsletterService()
    {
        using var cts = new CancellationTokenSource();
        var token = cts.Token;
        var page = BuildPage();
        var query = BuildQuery();
        _newsletterSubscriptionServiceMock
            .Setup(s => s.UnsubscribeFromNewsletterAsync(query, token))
            .ReturnsAsync(true);

        await _service.GetSubscribeRemoveDtoAsync(page, query, token);

        _newsletterSubscriptionServiceMock.Verify(
            s => s.UnsubscribeFromNewsletterAsync(query, token),
            Times.Once);
    }

    [Test]
    public void GetSubscribeRemoveDtoAsync_PropagatesException_FromNewsletterService()
    {
        var page = BuildPage();
        var query = BuildQuery();
        _newsletterSubscriptionServiceMock
            .Setup(s => s.UnsubscribeFromNewsletterAsync(query, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("newsletter error"));

        Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.GetSubscribeRemoveDtoAsync(page, query, CancellationToken.None));
    }
}
