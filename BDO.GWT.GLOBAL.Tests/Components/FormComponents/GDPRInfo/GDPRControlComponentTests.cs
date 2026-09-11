using BDO.GWT.GLOBAL.Services;
using GWTXbyKWeb.Components.FormComponents.GDPRInfo;
using Moq;

namespace BDO.GWT.GLOBAL.Tests.Components.FormComponents.GDPRInfo;

public class GDPRControlComponentTests
{
    private Mock<IGDPRUserLookupService> _serviceMock = null!;

    [SetUp]
    public void Setup()
    {
        _serviceMock = new Mock<IGDPRUserLookupService>();
    }

    // IDENTIFIER — static constant, no instantiation required

    [Test]
    public void IDENTIFIER_HasExpectedValue()
    {
        Assert.That(GDPRControlComponent.IDENTIFIER, Is.EqualTo("BDO.GdprActionButton"));
    }

    // HandleFetchText

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public void HandleFetchText_WhenEmailIsNullOrWhitespace_ReturnsFailureWithMessage(string? email)
    {
        var result = GdprCommandHandler.HandleFetchText(_serviceMock.Object, email!);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.Message, Is.EqualTo("Email address is empty."));
        });
        _serviceMock.Verify(s => s.GetContactAsText(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public void HandleFetchText_WithValidEmail_CallsServiceAndReturnsSuccess()
    {
        const string email = "user@example.com";
        const string expected = "Site Activity:\n\tOM Contact";
        _serviceMock.Setup(s => s.GetContactAsText(email)).Returns(expected);

        var result = GdprCommandHandler.HandleFetchText(_serviceMock.Object, email);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.Message, Is.EqualTo(expected));
        });
        _serviceMock.Verify(s => s.GetContactAsText(email), Times.Once);
    }

    [Test]
    public void HandleFetchText_TrimsEmailBeforeCallingService()
    {
        _serviceMock.Setup(s => s.GetContactAsText("user@example.com")).Returns("data");

        GdprCommandHandler.HandleFetchText(_serviceMock.Object, "  user@example.com  ");

        _serviceMock.Verify(s => s.GetContactAsText("user@example.com"), Times.Once);
    }

    // HandleFetchXml

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public void HandleFetchXml_WhenEmailIsNullOrWhitespace_ReturnsFailureWithMessage(string? email)
    {
        var result = GdprCommandHandler.HandleFetchXml(_serviceMock.Object, email!);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.Message, Is.EqualTo("Email address is empty."));
        });
        _serviceMock.Verify(s => s.GetContactAsXml(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public void HandleFetchXml_WithValidEmail_CallsServiceAndReturnsSuccess()
    {
        const string email = "user@example.com";
        const string expectedXml = "<XML><OM_Contact /></XML>";
        _serviceMock.Setup(s => s.GetContactAsXml(email)).Returns(expectedXml);

        var result = GdprCommandHandler.HandleFetchXml(_serviceMock.Object, email);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.Message, Is.EqualTo(expectedXml));
        });
        _serviceMock.Verify(s => s.GetContactAsXml(email), Times.Once);
    }

    [Test]
    public void HandleFetchXml_TrimsEmailBeforeCallingService()
    {
        _serviceMock.Setup(s => s.GetContactAsXml("user@example.com")).Returns("<XML />");

        GdprCommandHandler.HandleFetchXml(_serviceMock.Object, "  user@example.com  ");

        _serviceMock.Verify(s => s.GetContactAsXml("user@example.com"), Times.Once);
    }

    // HandleDelete

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public void HandleDelete_WhenEmailIsNullOrWhitespace_ReturnsFailureWithMessage(string? email)
    {
        var result = GdprCommandHandler.HandleDelete(_serviceMock.Object, email!);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.Message, Is.EqualTo("Email address is empty."));
        });
        _serviceMock.Verify(s => s.DeleteContactByEmail(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public void HandleDelete_WhenDataFound_ReturnsSuccessMessage()
    {
        const string email = "user@example.com";
        _serviceMock.Setup(s => s.DeleteContactByEmail(email)).Returns(true);

        var result = GdprCommandHandler.HandleDelete(_serviceMock.Object, email);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.Message, Is.EqualTo("Personal data successfully deleted."));
        });
    }

    [Test]
    public void HandleDelete_WhenNoDataFound_ReturnsNotFoundMessage()
    {
        const string email = "ghost@example.com";
        _serviceMock.Setup(s => s.DeleteContactByEmail(email)).Returns(false);

        var result = GdprCommandHandler.HandleDelete(_serviceMock.Object, email);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.Message, Is.EqualTo($"No personal data found for {email}."));
        });
    }

    [Test]
    public void HandleDelete_TrimsEmailBeforeCallingService()
    {
        _serviceMock.Setup(s => s.DeleteContactByEmail("user@example.com")).Returns(true);

        GdprCommandHandler.HandleDelete(_serviceMock.Object, "  user@example.com  ");

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

