using BDO.GWT.GLOBAL.API.Services;
using CMS.ContentEngine.Internal;
using CMS.DataEngine;
using CMS.EmailLibrary;
using CMS.EmailLibrary.Internal;
using CMS.Websites.Routing;
using Kentico.Xperience.Admin.DigitalMarketing.Internal;
using Moq;

namespace  BDO.GWT.GLOBAL.API.Tests.Services;

public class FormCrudServiceTests
{
    [Test]
    public void Constructor_CreatesInstance()
    {
        var service = CreateService();

        Assert.That(service, Is.Not.Null);
        Assert.That(service, Is.InstanceOf<IFormCrudService>());
    }

    [Test]
    public void Constructor_ImplementsIFormCrudService()
    {
        IFormCrudService service = CreateService();

        Assert.That(service, Is.Not.Null);
    }

    [Test]
    public void FileStreamData_DefaultValues_AreInitialized()
    {
        var data = new FileStreamData();

        Assert.That(data.Stream, Is.Null);
        Assert.That(data.ContentType, Is.Null);
        Assert.That(data.FileName, Is.Null);
    }

    [Test]
    public void FileStreamData_CanStoreAttachmentMetadata()
    {
        using var stream = new MemoryStream([1, 2, 3]);
        var data = new FileStreamData
        {
            Stream = stream,
            ContentType = "application/pdf",
            FileName = "resume.pdf"
        };

        Assert.That(data.Stream, Is.SameAs(stream));
        Assert.That(data.ContentType, Is.EqualTo("application/pdf"));
        Assert.That(data.FileName, Is.EqualTo("resume.pdf"));
    }

    [Test]
    public void FileStreamData_CanStoreVariousContentTypes()
    {
        using var stream = new MemoryStream([0xFF, 0xD8]);
        var data = new FileStreamData
        {
            Stream = stream,
            ContentType = "image/jpeg",
            FileName = "photo.jpg"
        };

        Assert.That(data.ContentType, Is.EqualTo("image/jpeg"));
        Assert.That(data.FileName, Is.EqualTo("photo.jpg"));
    }

    [Test]
    public void FileStreamData_StreamCanBeReplaced()
    {
        using var stream1 = new MemoryStream([1]);
        using var stream2 = new MemoryStream([2, 3]);

        var data = new FileStreamData { Stream = stream1 };
        Assert.That(data.Stream, Is.SameAs(stream1));

        data.Stream = stream2;
        Assert.That(data.Stream, Is.SameAs(stream2));
    }

    [Test]
    public void FormOutput_DefaultValues()
    {
        var output = new FormOutput();

        Assert.That(output.General, Is.Null);
        Assert.That(output.FormBuilder, Is.Null);
        Assert.That(output.EmailNotification, Is.Null);
        Assert.That(output.Autoresponder, Is.Null);
    }

    [Test]
    public void FormOutput_CanSetAllProperties()
    {
        var general = new FormGeneralOutput { FormCodeName = "test", FormDisplayName = "Test" };
        var autoresponder = new FormAutoresponderOutput
        {
            EmailConfigurationGuid = Guid.NewGuid(),
            EmailName = "auto-email",
            Subject = "Thanks",
            SenderEmail = "no-reply@test.com",
            SenderDisplayName = "Test Sender"
        };

        var output = new FormOutput
        {
            General = general,
            FormBuilder = new { layout = "two-column" },
            EmailNotification = null,
            Autoresponder = autoresponder
        };

        Assert.That(output.General, Is.SameAs(general));
        Assert.That(output.FormBuilder, Is.Not.Null);
        Assert.That(output.Autoresponder, Is.SameAs(autoresponder));
    }

    [Test]
    public void FormGeneralOutput_DefaultValues()
    {
        var general = new FormGeneralOutput();

        Assert.That(general.FormDisplayName, Is.Null);
        Assert.That(general.FormCodeName, Is.Null);
        Assert.That(general.AfterTheFormIsSubmitted, Is.EqualTo(AfterTheFormIsSubmitted.ReloadForm));
        Assert.That(general.DisplayText, Is.Null);
        Assert.That(general.RedirectToUri, Is.Null);
        Assert.That(general.SubmitButtonText, Is.Null);
        Assert.That(general.SubmitButtonImage, Is.Null);
    }

    [Test]
    public void FormGeneralOutput_CanSetAllProperties()
    {
        var general = new FormGeneralOutput
        {
            FormDisplayName = "Contact Us",
            FormCodeName = "ContactUs",
            AfterTheFormIsSubmitted = AfterTheFormIsSubmitted.DisplayText,
            DisplayText = "Thank you!",
            RedirectToUri = "/thanks",
            SubmitButtonText = "Send",
            SubmitButtonImage = "/img/send.png"
        };

        Assert.That(general.FormDisplayName, Is.EqualTo("Contact Us"));
        Assert.That(general.FormCodeName, Is.EqualTo("ContactUs"));
        Assert.That(general.AfterTheFormIsSubmitted, Is.EqualTo(AfterTheFormIsSubmitted.DisplayText));
        Assert.That(general.DisplayText, Is.EqualTo("Thank you!"));
        Assert.That(general.RedirectToUri, Is.EqualTo("/thanks"));
        Assert.That(general.SubmitButtonText, Is.EqualTo("Send"));
        Assert.That(general.SubmitButtonImage, Is.EqualTo("/img/send.png"));
    }

    [Test]
    public void FormEmailNotificationOutput_DefaultValues()
    {
        var notification = new FormEmailNotificationOutput();

        Assert.That(notification.SenderEmail, Is.Null);
        Assert.That(notification.RecipientEmails, Is.Empty);
        Assert.That(notification.Subject, Is.Null);
        Assert.That(notification.AttachUploadedDocuments, Is.True);
        Assert.That(notification.CustomLayout, Is.Null);
    }

    [Test]
    public void FormEmailNotificationOutput_CanSetAllProperties()
    {
        var notification = new FormEmailNotificationOutput
        {
            SenderEmail = "admin@company.com",
            RecipientEmails = ["user1@test.com", "user2@test.com"],
            Subject = "New submission",
            AttachUploadedDocuments = false,
            CustomLayout = "<html>Custom</html>"
        };

        Assert.That(notification.SenderEmail, Is.EqualTo("admin@company.com"));
        Assert.That(notification.RecipientEmails, Has.Length.EqualTo(2));
        Assert.That(notification.Subject, Is.EqualTo("New submission"));
        Assert.That(notification.AttachUploadedDocuments, Is.False);
        Assert.That(notification.CustomLayout, Is.EqualTo("<html>Custom</html>"));
    }

    [Test]
    public void FormAutoresponderOutput_DefaultValues()
    {
        var autoresponder = new FormAutoresponderOutput();

        Assert.That(autoresponder.EmailConfigurationGuid, Is.EqualTo(Guid.Empty));
        Assert.That(autoresponder.EmailName, Is.Null);
        Assert.That(autoresponder.Subject, Is.Null);
        Assert.That(autoresponder.SenderEmail, Is.Null);
        Assert.That(autoresponder.SenderDisplayName, Is.Null);
    }

    [Test]
    public void FormAutoresponderOutput_CanSetAllProperties()
    {
        var guid = Guid.NewGuid();
        var autoresponder = new FormAutoresponderOutput
        {
            EmailConfigurationGuid = guid,
            EmailName = "Welcome Email",
            Subject = "Welcome!",
            SenderEmail = "hello@company.com",
            SenderDisplayName = "Company Team"
        };

        Assert.That(autoresponder.EmailConfigurationGuid, Is.EqualTo(guid));
        Assert.That(autoresponder.EmailName, Is.EqualTo("Welcome Email"));
        Assert.That(autoresponder.Subject, Is.EqualTo("Welcome!"));
        Assert.That(autoresponder.SenderEmail, Is.EqualTo("hello@company.com"));
        Assert.That(autoresponder.SenderDisplayName, Is.EqualTo("Company Team"));
    }

    [Test]
    public void AfterTheFormIsSubmitted_HasExpectedValues()
    {
        Assert.That((int)AfterTheFormIsSubmitted.DisplayText, Is.EqualTo(0));
        Assert.That((int)AfterTheFormIsSubmitted.RedirectToUrl, Is.EqualTo(1));
        Assert.That((int)AfterTheFormIsSubmitted.ReloadForm, Is.EqualTo(2));
    }

    [Test]
    public void IFormCrudService_DefinesExpectedMethods()
    {
        var interfaceType = typeof(IFormCrudService);

        Assert.That(interfaceType.GetMethod("GetFormsAsync"), Is.Not.Null);
        Assert.That(interfaceType.GetMethod("GetFormAsync"), Is.Not.Null);
        Assert.That(interfaceType.GetMethod("GetFormEntriesAsync"), Is.Not.Null);
        Assert.That(interfaceType.GetMethod("GetFormEntryAsync"), Is.Not.Null);
        Assert.That(interfaceType.GetMethod("GetFormEntryAttachmentAsync"), Is.Not.Null);
        Assert.That(interfaceType.GetMethod("DeleteFormEntryAsync"), Is.Not.Null);
    }

    [Test]
    public void IFormCrudService_GetFormsAsync_ReturnsCorrectType()
    {
        var method = typeof(IFormCrudService).GetMethod("GetFormsAsync");

        Assert.That(method, Is.Not.Null);
        Assert.That(method!.ReturnType, Is.EqualTo(typeof(Task<IEnumerable<FormOutput>>)));
    }

    [Test]
    public void IFormCrudService_GetFormAsync_AcceptsStringAndCancellationToken()
    {
        var method = typeof(IFormCrudService).GetMethod("GetFormAsync");

        Assert.That(method, Is.Not.Null);
        var parameters = method!.GetParameters();
        Assert.That(parameters, Has.Length.EqualTo(2));
        Assert.That(parameters[0].ParameterType, Is.EqualTo(typeof(string)));
        Assert.That(parameters[1].ParameterType, Is.EqualTo(typeof(CancellationToken)));
    }

    [Test]
    public void IFormCrudService_GetFormEntryAsync_AcceptsStringIntAndCancellationToken()
    {
        var method = typeof(IFormCrudService).GetMethod("GetFormEntryAsync");

        Assert.That(method, Is.Not.Null);
        var parameters = method!.GetParameters();
        Assert.That(parameters, Has.Length.EqualTo(3));
        Assert.That(parameters[0].ParameterType, Is.EqualTo(typeof(string)));
        Assert.That(parameters[1].ParameterType, Is.EqualTo(typeof(int)));
        Assert.That(parameters[2].ParameterType, Is.EqualTo(typeof(CancellationToken)));
    }

    [Test]
    public void IFormCrudService_GetFormEntryAttachmentAsync_ReturnsFileStreamData()
    {
        var method = typeof(IFormCrudService).GetMethod("GetFormEntryAttachmentAsync");

        Assert.That(method, Is.Not.Null);
        Assert.That(method!.ReturnType, Is.EqualTo(typeof(Task<FileStreamData>)));
    }

    [Test]
    public void IFormCrudService_DeleteFormEntryAsync_ReturnsTask()
    {
        var method = typeof(IFormCrudService).GetMethod("DeleteFormEntryAsync");

        Assert.That(method, Is.Not.Null);
        Assert.That(method!.ReturnType, Is.EqualTo(typeof(Task)));
    }

    [Test]
    public void FormCrudService_CanBeMocked()
    {
        var mock = new Mock<IFormCrudService>();
        mock.Setup(x => x.GetFormsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<FormOutput>());

        var result = mock.Object.GetFormsAsync(CancellationToken.None).Result;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count(), Is.EqualTo(0));
    }

    [Test]
    public void FormCrudService_MockCanThrowKeyNotFoundException()
    {
        var mock = new Mock<IFormCrudService>();
        mock.Setup(x => x.GetFormAsync("missing", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException("not found"));

        Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            await mock.Object.GetFormAsync("missing", CancellationToken.None));
    }

    [Test]
    public async Task FormCrudService_MockReturnsFormEntries()
    {
        var mock = new Mock<IFormCrudService>();
        var entries = new List<Dictionary<string, object?>>
        {
            new() { ["ID"] = 1, ["Name"] = "Alice" },
            new() { ["ID"] = 2, ["Name"] = "Bob" }
        };

        mock.Setup(x => x.GetFormEntriesAsync("contact", It.IsAny<CancellationToken>()))
            .ReturnsAsync(entries);

        var result = await mock.Object.GetFormEntriesAsync("contact", CancellationToken.None);

        Assert.That(result.Count(), Is.EqualTo(2));
    }

    [Test]
    public async Task FormCrudService_MockReturnsFileStreamData()
    {
        var mock = new Mock<IFormCrudService>();
        using var stream = new MemoryStream([10, 20, 30]);

        mock.Setup(x => x.GetFormEntryAttachmentAsync("form", 1, "doc", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FileStreamData
            {
                Stream = stream,
                ContentType = "application/octet-stream",
                FileName = "file.bin"
            });

        var result = await mock.Object.GetFormEntryAttachmentAsync("form", 1, "doc", CancellationToken.None);

        Assert.That(result.Stream, Is.SameAs(stream));
        Assert.That(result.ContentType, Is.EqualTo("application/octet-stream"));
        Assert.That(result.FileName, Is.EqualTo("file.bin"));
    }

    [Test]
    public async Task FormCrudService_MockDeleteDoesNotThrow()
    {
        var mock = new Mock<IFormCrudService>();
        mock.Setup(x => x.DeleteFormEntryAsync("form", 5, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        Assert.DoesNotThrowAsync(async () =>
            await mock.Object.DeleteFormEntryAsync("form", 5, CancellationToken.None));
    }

    private static FormCrudService CreateService() => new(
        new Mock<IInfoProvider<EmailConfigurationInfo>>().Object,
        new Mock<IInfoProvider<EmailChannelInfo>>().Object,
        new Mock<IInfoProvider<EmailChannelSenderInfo>>().Object,
        new Mock<IContentItemDataInfoRetriever>().Object,
        new Mock<IInfoProvider<ContentItemInfo>>().Object,
        new Mock<IEmailChannelLanguageRetriever>().Object,
        new Mock<IEmailChannelSenderEmailProvider>().Object,
        new Mock<IWebsiteChannelContext>().Object);
}
