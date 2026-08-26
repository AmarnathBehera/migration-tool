using BDO.GWT.Web.Core.LiveSite.Enums;
using BDO.GWT.Xperience.Core.LiveSite.Services;
using CMS.Helpers;
using Moq;

namespace BDO.GWT.Core.Tests.Services;

public class VisitorContextTests
{
    private Mock<ICurrentCookieLevelProvider> _currentCookieLevelProviderMock = null!;
    private VisitorContext _service = null!;

    [SetUp]
    public void Setup()
    {
        _currentCookieLevelProviderMock = new Mock<ICurrentCookieLevelProvider>();
        _service = new VisitorContext(_currentCookieLevelProviderMock.Object);
    }

    [Test]
    public void VisitorContext_ImplementsIVisitorContext()
    {
        Assert.That(typeof(IVisitorContext).IsAssignableFrom(typeof(VisitorContext)), Is.True);
    }

    [TestCase(-1000, CookieConsentLevel.None)]
    [TestCase(-100, CookieConsentLevel.System)]
    [TestCase(0, CookieConsentLevel.Essential)]
    [TestCase(100, CookieConsentLevel.Editor)]
    [TestCase(200, CookieConsentLevel.Visitor)]
    [TestCase(1000, CookieConsentLevel.All)]
    public void CookieConsentLevel_ReturnsMappedEnumValue(int providerValue, CookieConsentLevel expected)
    {
        _currentCookieLevelProviderMock
            .Setup(p => p.GetCurrentCookieLevel())
            .Returns(providerValue);

        var result = _service.CookieConsentLevel;

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void CookieConsentLevel_ReturnsCastedEnum_ForUnknownValue()
    {
        const int unknownValue = 555;
        _currentCookieLevelProviderMock
            .Setup(p => p.GetCurrentCookieLevel())
            .Returns(unknownValue);

        var result = _service.CookieConsentLevel;

        Assert.That(result, Is.EqualTo((CookieConsentLevel)unknownValue));
    }

    [Test]
    public void CookieConsentLevel_CallsProvider_OnEveryAccess()
    {
        _currentCookieLevelProviderMock
            .Setup(p => p.GetCurrentCookieLevel())
            .Returns((int)CookieConsentLevel.Essential);

        _ = _service.CookieConsentLevel;
        _ = _service.CookieConsentLevel;

        _currentCookieLevelProviderMock.Verify(p => p.GetCurrentCookieLevel(), Times.Exactly(2));
    }

    [Test]
    public void CookieConsentLevel_PropagatesException_FromProvider()
    {
        _currentCookieLevelProviderMock
            .Setup(p => p.GetCurrentCookieLevel())
            .Throws(new InvalidOperationException("provider error"));

        Assert.Throws<InvalidOperationException>(() => _ = _service.CookieConsentLevel);
    }
}
