using BDO.GWT.GLOBAL.Services;
using GWTXbyKWeb.Components.FormComponents.GDPRInfo;
using Moq;

namespace BDO.GWT.GLOBAL.Tests.Components.FormComponents.GDPRInfo;

public class GDPRControlComponentTests
{
    private Mock<IGDPRUserLookupService> _serviceMock = null!;
    private GDPRControlComponent _component = null!;

    [SetUp]
    public void Setup()
    {
        _serviceMock = new Mock<IGDPRUserLookupService>();
        _component = new GDPRControlComponent(_serviceMock.Object);
    }

    // IDENTIFIER

    [Test]
    public void IDENTIFIER_HasExpectedValue()
    {
        Assert.That(GDPRControlComponent.IDENTIFIER, Is.EqualTo("BDO.GdprActionButton"));
    }

    // FetchTextData

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public async Task FetchTextData_WhenEmailIsNullOrWhitespace_ReturnsFailureWithMessage(string? email)
    {
        var result = await _component.FetchTextData(email!);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.Message, Is.EqualTo("Email address is empty."));
        });
        _serviceMock.Verify(s => s.GetContactAsText(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task FetchTextData_WithValidEmail_CallsServiceAndReturnsSuccess()
    {
        const string email = "user@example.com";
        const string expectedText = "Site Activity:\n\tOM Contact";
        _serviceMock.Setup(s => s.GetContactAsText(email)).Returns(expectedText);

        var result = await _component.FetchTextData(email);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.Message, Is.EqualTo(expectedText));
        });
        _serviceMock.Verify(s => s.GetContactAsText(email), Times.Once);
    }

    [Test]
    public async Task FetchTextData_TrimsEmailBeforeCallingService()
    {
        _serviceMock.Setup(s => s.GetContactAsText("user@example.com")).Returns("data");

        await _component.FetchTextData("  user@example.com  ");

        _serviceMock.Verify(s => s.GetContactAsText("user@example.com"), Times.Once);
    }

    // FetchXmlData

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public async Task FetchXmlData_WhenEmailIsNullOrWhitespace_ReturnsFailureWithMessage(string? email)
    {
        var result = await _component.FetchXmlData(email!);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.Message, Is.EqualTo("Email address is empty."));
        });
        _serviceMock.Verify(s => s.GetContactAsXml(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task FetchXmlData_WithValidEmail_CallsServiceAndReturnsSuccess()
    {
        const string email = "user@example.com";
        const string expectedXml = "<XML><OM_Contact /></XML>";
        _serviceMock.Setup(s => s.GetContactAsXml(email)).Returns(expectedXml);

        var result = await _component.FetchXmlData(email);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.Message, Is.EqualTo(expectedXml));
        });
        _serviceMock.Verify(s => s.GetContactAsXml(email), Times.Once);
    }

    [Test]
    public async Task FetchXmlData_TrimsEmailBeforeCallingService()
    {
        _serviceMock.Setup(s => s.GetContactAsXml("user@example.com")).Returns("<XML />");

        await _component.FetchXmlData("  user@example.com  ");

        _serviceMock.Verify(s => s.GetContactAsXml("user@example.com"), Times.Once);
    }

    // Delete

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public async Task Delete_WhenEmailIsNullOrWhitespace_ReturnsFailureWithMessage(string? email)
    {
        var result = await _component.Delete(email!);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.Message, Is.EqualTo("Email address is empty."));
        });
        _serviceMock.Verify(s => s.DeleteContactByEmail(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task Delete_WhenDataFound_ReturnsSuccessMessage()
    {
        const string email = "user@example.com";
        _serviceMock.Setup(s => s.DeleteContactByEmail(email)).Returns(true);

        var result = await _component.Delete(email);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.Message, Is.EqualTo("Personal data successfully deleted."));
        });
    }

    [Test]
    public async Task Delete_WhenNoDataFound_ReturnsNotFoundMessage()
    {
        const string email = "ghost@example.com";
        _serviceMock.Setup(s => s.DeleteContactByEmail(email)).Returns(false);

        var result = await _component.Delete(email);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.Message, Is.EqualTo("No personal data found for the given email."));
        });
    }

    [Test]
    public async Task Delete_TrimsEmailBeforeCallingService()
    {
        _serviceMock.Setup(s => s.DeleteContactByEmail("user@example.com")).Returns(true);

        await _component.Delete("  user@example.com  ");

        _serviceMock.Verify(s => s.DeleteContactByEmail("user@example.com"), Times.Once);
    }

    // GdprCommandResult defaults

    [Test]
    public void GdprCommandResult_DefaultMessage_IsEmptyString()
    {
        var result = new GdprCommandResult();

        Assert.That(result.Message, Is.EqualTo(string.Empty));
    }

    [Test]
    public void GdprCommandResult_DefaultSuccess_IsFalse()
    {
        var result = new GdprCommandResult();

        Assert.That(result.Success, Is.False);
    }
}
