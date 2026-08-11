using BDO.GWT.Web.Core.LiveSite.Enums;
using BDO.GWT.Web.Core.LiveSite.Services;
using BDO.GWT.Xperience.Core.LiveSite.Services;
using CMS.Helpers;
using Microsoft.AspNetCore.Http;
using Moq;

namespace BDO.GWT.Core.Tests.Services;

public class CookieConsentServiceTests
{
    private Mock<ICurrentCookieLevelProvider> _cookieLevelProviderMock = null!;
    private Mock<IVisitorContext> _visitorContextMock = null!;
    private Mock<IHttpContextAccessor> _httpContextAccessorMock = null!;
    private CookieConsentService _service = null!;

    [SetUp]
    public void Setup()
    {
        _cookieLevelProviderMock = new Mock<ICurrentCookieLevelProvider>();
        _visitorContextMock = new Mock<IVisitorContext>();
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();

        _service = new CookieConsentService(
            _cookieLevelProviderMock.Object,
            _visitorContextMock.Object,
            _httpContextAccessorMock.Object);
    }

    // ??? SetVisitorCookieConsentLevel ?????????????????????????????????????????

    [Test]
    public void SetVisitorCookieConsentLevel_CallsSetCurrentCookieLevel_WithCorrectIntValue()
    {
        _service.SetVisitorCookieConsentLevel(CookieConsentLevel.Essential);

        _cookieLevelProviderMock.Verify(
            p => p.SetCurrentCookieLevel((int)CookieConsentLevel.Essential),
            Times.Once);
    }

    [Test]
    public void SetVisitorCookieConsentLevel_CallsSetCurrentCookieLevel_WithAllLevel()
    {
        _service.SetVisitorCookieConsentLevel(CookieConsentLevel.All);

        _cookieLevelProviderMock.Verify(
            p => p.SetCurrentCookieLevel((int)CookieConsentLevel.All),
            Times.Once);
    }

    [Test]
    public void SetVisitorCookieConsentLevel_CallsSetCurrentCookieLevel_WithNoneLevel()
    {
        _service.SetVisitorCookieConsentLevel(CookieConsentLevel.None);

        _cookieLevelProviderMock.Verify(
            p => p.SetCurrentCookieLevel((int)CookieConsentLevel.None),
            Times.Once);
    }

    [TestCase(CookieConsentLevel.None)]
    [TestCase(CookieConsentLevel.Essential)]
    [TestCase(CookieConsentLevel.Visitor)]
    [TestCase(CookieConsentLevel.All)]
    public void SetVisitorCookieConsentLevel_MapsEnumToInt_Correctly(CookieConsentLevel level)
    {
        _service.SetVisitorCookieConsentLevel(level);

        _cookieLevelProviderMock.Verify(
            p => p.SetCurrentCookieLevel((int)level),
            Times.Once);
    }

    // ??? GetDefaultCookieConsentLevelAsync ????????????????????????????????????

    [Test]
    public async Task GetDefaultCookieConsentLevelAsync_ReturnsEssential_WhenProviderReturnsZero()
    {
        _cookieLevelProviderMock.Setup(p => p.GetDefaultCookieLevel()).Returns(0);

        var result = await _service.GetDefaultCookieConsentLevelAsync(CancellationToken.None);

        Assert.That(result, Is.EqualTo(CookieConsentLevel.Essential));
    }

    [Test]
    public async Task GetDefaultCookieConsentLevelAsync_ReturnsNone_WhenProviderReturnsMinusThousand()
    {
        _cookieLevelProviderMock.Setup(p => p.GetDefaultCookieLevel()).Returns(-1000);

        var result = await _service.GetDefaultCookieConsentLevelAsync(CancellationToken.None);

        Assert.That(result, Is.EqualTo(CookieConsentLevel.None));
    }

    [Test]
    public async Task GetDefaultCookieConsentLevelAsync_ReturnsAll_WhenProviderReturnsThousand()
    {
        _cookieLevelProviderMock.Setup(p => p.GetDefaultCookieLevel()).Returns(1000);

        var result = await _service.GetDefaultCookieConsentLevelAsync(CancellationToken.None);

        Assert.That(result, Is.EqualTo(CookieConsentLevel.All));
    }

    [Test]
    public async Task GetDefaultCookieConsentLevelAsync_CallsGetDefaultCookieLevel_Once()
    {
        _cookieLevelProviderMock.Setup(p => p.GetDefaultCookieLevel()).Returns(0);

        await _service.GetDefaultCookieConsentLevelAsync(CancellationToken.None);

        _cookieLevelProviderMock.Verify(p => p.GetDefaultCookieLevel(), Times.Once);
    }

    [Test]
    public async Task GetDefaultCookieConsentLevelAsync_ReturnsCompletedTask()
    {
        _cookieLevelProviderMock.Setup(p => p.GetDefaultCookieLevel()).Returns(0);

        var task = _service.GetDefaultCookieConsentLevelAsync(CancellationToken.None);

        Assert.That(task.IsCompleted, Is.True);
        await task;
    }

    // ??? RemoveNotAcceptedCookies ?????????????????????????????????????????????
    // NOTE: RemoveNotAcceptedCookies ultimately calls CookieHelper.RemoveAllCookies,
    // which is a Kentico static method that requires the IoC container to be
    // initialised. Full coverage of that method requires an integration/
    // application-context test; unit tests for it are intentionally omitted here.
}
