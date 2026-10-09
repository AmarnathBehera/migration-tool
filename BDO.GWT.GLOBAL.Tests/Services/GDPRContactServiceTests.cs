using BDO.GWT.GLOBAL.Services;
using CMS.Membership;
using CMS.DataEngine;
using Moq;

namespace BDO.GWT.GLOBAL.Tests.Services;

public class GDPRContactServiceTests
{
    private Mock<IInfoProvider<UserInfo>> _userProviderMock = null!;
    private GDPRUserService _service = null!;

    [SetUp]
    public void Setup()
    {
        _userProviderMock = new Mock<IInfoProvider<UserInfo>>();
        _service = new GDPRUserService(_userProviderMock.Object);
    }

    // DeleteContactByEmail — guard clauses that never hit the database

    [Test]
    public void DeleteContactByEmail_WhenEmailIsNull_ReturnsFalse()
    {
        var result = _service.DeleteContactByEmail(null!);

        Assert.That(result, Is.False);
        _userProviderMock.Verify(p => p.Get(), Times.Never);
    }

    [Test]
    public void DeleteContactByEmail_WhenEmailIsEmpty_ReturnsFalse()
    {
        var result = _service.DeleteContactByEmail(string.Empty);

        Assert.That(result, Is.False);
        _userProviderMock.Verify(p => p.Get(), Times.Never);
    }

    [TestCase("   ")]
    [TestCase("\t")]
    public void DeleteContactByEmail_WhenEmailIsWhitespace_ReturnsFalse(string email)
    {
        var result = _service.DeleteContactByEmail(email);

        Assert.That(result, Is.False);
        _userProviderMock.Verify(p => p.Get(), Times.Never);
    }

    // Interface contract

    [Test]
    public void GDPRUserService_ImplementsIGDPRUserLookupService()
    {
        Assert.That(_service, Is.InstanceOf<IGDPRUserLookupService>());
    }

    [Test]
    public void IGDPRUserLookupService_DefinesGetContactAsText()
    {
        var method = typeof(IGDPRUserLookupService).GetMethod(nameof(IGDPRUserLookupService.GetContactAsText));

        Assert.That(method, Is.Not.Null);
        Assert.That(method!.ReturnType, Is.EqualTo(typeof(string)));
    }

    [Test]
    public void IGDPRUserLookupService_DefinesGetContactAsXml()
    {
        var method = typeof(IGDPRUserLookupService).GetMethod(nameof(IGDPRUserLookupService.GetContactAsXml));

        Assert.That(method, Is.Not.Null);
        Assert.That(method!.ReturnType, Is.EqualTo(typeof(string)));
    }

    [Test]
    public void IGDPRUserLookupService_DefinesDeleteContactByEmail()
    {
        var method = typeof(IGDPRUserLookupService).GetMethod(nameof(IGDPRUserLookupService.DeleteContactByEmail));

        Assert.That(method, Is.Not.Null);
        Assert.That(method!.ReturnType, Is.EqualTo(typeof(bool)));
    }
}
