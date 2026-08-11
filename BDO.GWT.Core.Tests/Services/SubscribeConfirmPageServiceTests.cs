using BDO.GWT.Core;
using BDO.GWT.Core.Data.Queries;
using BDO.GWT.Web.Core.DTOs.Pages;
using BDO.GWT.Web.Core.LiveSite.Services;
using BDO.GWT.Xperience.Core.LiveSite.Services;
using BDO.GWT.Xperience.Core.Services;
using Moq;
using BDO;

namespace BDO.GWT.Core.Tests.Services;

public class SubscribeConfirmPageServiceTests
{
    private Mock<ISiteContext> _siteContextMock = null!;
    private Mock<INewsletterSubscriptionService> _newsletterSubscriptionServiceMock = null!;
    private SubscribeConfirmPageService _service = null!;

    private const string SiteName = "TestSite";

    [SetUp]
    public void Setup()
    {
        _siteContextMock = new Mock<ISiteContext>();
        _newsletterSubscriptionServiceMock = new Mock<INewsletterSubscriptionService>();

        _siteContextMock.Setup(s => s.CurrentSiteName).Returns(SiteName);

        _service = new SubscribeConfirmPageService(
            _siteContextMock.Object,
            _newsletterSubscriptionServiceMock.Object);
    }

    private static SubscribeConfirmQuery BuildQuery(string hash = "hash123") => new()
    {
        SubscriptionHash = hash,
        DateTime = "2024-01-01T00:00:00",
        ContactId = 1,
        RecipientListId = 10,
    };

    private static SubscribeConfirm BuildPage(
        string successMsg = "<p>Success!</p>",
        string failMsg = "<p>Failed!</p>") =>
        new()
        {
            SuccessfulSubscribeMessage = successMsg,
            UnsuccessfulSubscribeMessage = failMsg,
        };

    // ??? GetSubscribeConfirmDtoAsync ??????????????????????????????????????????

    [Test]
    public async Task GetSubscribeConfirmDtoAsync_ReturnsDto()
    {
        var page = BuildPage();
        var query = BuildQuery();
        _newsletterSubscriptionServiceMock
            .Setup(s => s.ConfirmSubscriptionForNewsletterAsync(query, SiteName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _service.GetSubscribeConfirmDtoAsync(page, query, CancellationToken.None);

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<SubscribeConfirmDto>());
    }

    [Test]
    public async Task GetSubscribeConfirmDtoAsync_ReturnsSuccessMessage_WhenSubscriptionSucceeds()
    {
        const string successMsg = "You are now subscribed.";
        var page = BuildPage(successMsg: successMsg);
        var query = BuildQuery();
        _newsletterSubscriptionServiceMock
            .Setup(s => s.ConfirmSubscriptionForNewsletterAsync(query, SiteName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _service.GetSubscribeConfirmDtoAsync(page, query, CancellationToken.None);

        Assert.That(result.Message, Is.EqualTo(successMsg));
    }

    [Test]
    public async Task GetSubscribeConfirmDtoAsync_ReturnsUnsuccessfulMessage_WhenSubscriptionFails()
    {
        const string failMsg = "Subscription failed.";
        var page = BuildPage(failMsg: failMsg);
        var query = BuildQuery();
        _newsletterSubscriptionServiceMock
            .Setup(s => s.ConfirmSubscriptionForNewsletterAsync(query, SiteName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _service.GetSubscribeConfirmDtoAsync(page, query, CancellationToken.None);

        Assert.That(result.Message, Is.EqualTo(failMsg));
    }

    [Test]
    public async Task GetSubscribeConfirmDtoAsync_CallsConfirmSubscription_WithCurrentSiteName()
    {
        var page = BuildPage();
        var query = BuildQuery();
        _newsletterSubscriptionServiceMock
            .Setup(s => s.ConfirmSubscriptionForNewsletterAsync(query, SiteName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await _service.GetSubscribeConfirmDtoAsync(page, query, CancellationToken.None);

        _newsletterSubscriptionServiceMock.Verify(
            s => s.ConfirmSubscriptionForNewsletterAsync(query, SiteName, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task GetSubscribeConfirmDtoAsync_PassesCancellationToken_ToNewsletterService()
    {
        using var cts = new CancellationTokenSource();
        var token = cts.Token;
        var page = BuildPage();
        var query = BuildQuery();

        _newsletterSubscriptionServiceMock
            .Setup(s => s.ConfirmSubscriptionForNewsletterAsync(query, SiteName, token))
            .ReturnsAsync(true);

        await _service.GetSubscribeConfirmDtoAsync(page, query, token);

        _newsletterSubscriptionServiceMock.Verify(
            s => s.ConfirmSubscriptionForNewsletterAsync(query, SiteName, token),
            Times.Once);
    }

    [Test]
    public async Task GetSubscribeConfirmDtoAsync_ReadsSiteNameFromSiteContext()
    {
        var page = BuildPage();
        var query = BuildQuery();
        _newsletterSubscriptionServiceMock
            .Setup(s => s.ConfirmSubscriptionForNewsletterAsync(query, SiteName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        await _service.GetSubscribeConfirmDtoAsync(page, query, CancellationToken.None);

        _siteContextMock.Verify(s => s.CurrentSiteName, Times.AtLeastOnce);
    }

    [Test]
    public void GetSubscribeConfirmDtoAsync_PropagatesException_FromNewsletterService()
    {
        var page = BuildPage();
        var query = BuildQuery();
        _newsletterSubscriptionServiceMock
            .Setup(s => s.ConfirmSubscriptionForNewsletterAsync(query, SiteName, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("newsletter error"));

        Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.GetSubscribeConfirmDtoAsync(page, query, CancellationToken.None));
    }
}
