using BDO;
using BDO.GWT.Core;
using BDO.GWT.Core.Services;
using BDO.GWT.Web.Core.Data;
using BDO.GWT.Web.Core.LiveSite.Data;
using BDO.GWT.Web.Core.LiveSite.Services;
using BDO.GWT.Xperience.Core.LiveSite.Services;
using BDO.GWT.Xperience.Core.Services;
using Kentico.Content.Web.Mvc;
using Moq;

namespace BDO.GWT.Core.Tests.Services;

public class SubscribeFormServiceTests
{
    private Mock<INewsletterSubscriptionService> _newsletterSubscriptionServiceMock = null!;
    private Mock<IContentRetriever> _contentRetrieverMock = null!;
    private Mock<IRecaptchaValidationService> _recaptchaValidationServiceMock = null!;
    private SubscribeFormService _service = null!;

    private static readonly Guid PageGuid = Guid.NewGuid();

    [SetUp]
    public void Setup()
    {
        _newsletterSubscriptionServiceMock = new Mock<INewsletterSubscriptionService>();
        _contentRetrieverMock = new Mock<IContentRetriever>();
        _recaptchaValidationServiceMock = new Mock<IRecaptchaValidationService>();

        _service = new SubscribeFormService(
            _newsletterSubscriptionServiceMock.Object,
            _contentRetrieverMock.Object,
            _recaptchaValidationServiceMock.Object);
    }

    // ??? Helpers ??????????????????????????????????????????????????????????????

    private static NewsletterSubscriptionFormData ValidFormData() => new()
    {
        Email = "test@example.com",
        FirstName = "Jane",
        LastName = "Doe",
        RecaptchaToken = "token123",
    };

    private static Subscribe BuildSubscribePage(
        bool recaptchaUseV3 = false,
        decimal recaptchaScore = 0.5m,
        bool showCompany = false,
        bool isCompanyRequired = false,
        bool showJobTitle = false,
        bool isJobTitleRequired = false,
        bool showCountry = false,
        bool isCountryRequired = false) =>
        new()
        {
            RecaptchaUseV3 = recaptchaUseV3,
            RecaptchaScoreTreshhold = recaptchaScore,
            ShowCompanyField = showCompany,
            IsCompanyRequired = isCompanyRequired,
            ShowJobTitleField = showJobTitle,
            IsJobTitleRequired = isJobTitleRequired,
            ShowCountryField = showCountry,
            IsCountryRequired = isCountryRequired,
        };

    private void SetupContentRetriever(Subscribe? page)
    {
        var pages = page is null
            ? Array.Empty<Subscribe>()
            : [page];

        _contentRetrieverMock
            .Setup(r => r.RetrieveAllPagesByGuids<Subscribe>(
                It.IsAny<Guid[]>(),
                cancellationToken: It.IsAny<CancellationToken>()))
            .ReturnsAsync(pages);
    }

    private void SetupRecaptcha(bool result)
    {
        _recaptchaValidationServiceMock
            .Setup(r => r.ValidateAsync(
                It.IsAny<bool>(),
                It.IsAny<decimal>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);
    }

    // ??? SubscribeToNewslettersAsync — page not found ?????????????????????????

    [Test]
    public async Task SubscribeToNewslettersAsync_ReturnsFalse_WhenPageNotFound()
    {
        SetupContentRetriever(null);

        var result = await _service.SubscribeToNewslettersAsync(
            PageGuid, ValidFormData(), CancellationToken.None);

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task SubscribeToNewslettersAsync_DoesNotCallNewsletterService_WhenPageNotFound()
    {
        SetupContentRetriever(null);

        await _service.SubscribeToNewslettersAsync(PageGuid, ValidFormData(), CancellationToken.None);

        _newsletterSubscriptionServiceMock.Verify(
            s => s.SubscribeToNewslettersAsync(It.IsAny<NewsletterSubscriptionFormData>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // ??? SubscribeToNewslettersAsync — field validation failures ?????????????

    [Test]
    public async Task SubscribeToNewslettersAsync_ReturnsFalse_WhenEmailIsInvalid()
    {
        var page = BuildSubscribePage();
        SetupContentRetriever(page);
        var data = ValidFormData();
        data.Email = "not-an-email";

        var result = await _service.SubscribeToNewslettersAsync(PageGuid, data, CancellationToken.None);

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task SubscribeToNewslettersAsync_ReturnsFalse_WhenFirstNameIsEmpty()
    {
        var page = BuildSubscribePage();
        SetupContentRetriever(page);
        var data = ValidFormData();
        data.FirstName = string.Empty;

        var result = await _service.SubscribeToNewslettersAsync(PageGuid, data, CancellationToken.None);

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task SubscribeToNewslettersAsync_ReturnsFalse_WhenLastNameIsEmpty()
    {
        var page = BuildSubscribePage();
        SetupContentRetriever(page);
        var data = ValidFormData();
        data.LastName = string.Empty;

        var result = await _service.SubscribeToNewslettersAsync(PageGuid, data, CancellationToken.None);

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task SubscribeToNewslettersAsync_ReturnsFalse_WhenCompanyRequiredButMissing()
    {
        var page = BuildSubscribePage(showCompany: true, isCompanyRequired: true);
        SetupContentRetriever(page);
        var data = ValidFormData();
        data.Company = null;

        var result = await _service.SubscribeToNewslettersAsync(PageGuid, data, CancellationToken.None);

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task SubscribeToNewslettersAsync_ReturnsFalse_WhenJobTitleRequiredButMissing()
    {
        var page = BuildSubscribePage(showJobTitle: true, isJobTitleRequired: true);
        SetupContentRetriever(page);
        var data = ValidFormData();
        data.JobTitle = null;

        var result = await _service.SubscribeToNewslettersAsync(PageGuid, data, CancellationToken.None);

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task SubscribeToNewslettersAsync_ReturnsFalse_WhenCountryRequiredButMissing()
    {
        var page = BuildSubscribePage(showCountry: true, isCountryRequired: true);
        SetupContentRetriever(page);
        var data = ValidFormData();
        data.CountryGuid = null;

        var result = await _service.SubscribeToNewslettersAsync(PageGuid, data, CancellationToken.None);

        Assert.That(result, Is.False);
    }

    // ??? SubscribeToNewslettersAsync — optional fields do not block ???????????

    [Test]
    public async Task SubscribeToNewslettersAsync_DoesNotFail_WhenOptionalCompanyIsEmpty()
    {
        var page = BuildSubscribePage(showCompany: true, isCompanyRequired: false);
        SetupContentRetriever(page);
        SetupRecaptcha(true);
        _newsletterSubscriptionServiceMock
            .Setup(s => s.SubscribeToNewslettersAsync(It.IsAny<NewsletterSubscriptionFormData>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var data = ValidFormData();
        data.Company = null;

        var result = await _service.SubscribeToNewslettersAsync(PageGuid, data, CancellationToken.None);

        Assert.That(result, Is.True);
    }

    // ??? SubscribeToNewslettersAsync — recaptcha ??????????????????????????????

    [Test]
    public async Task SubscribeToNewslettersAsync_ReturnsFalse_WhenRecaptchaFails()
    {
        var page = BuildSubscribePage();
        SetupContentRetriever(page);
        SetupRecaptcha(false);

        var result = await _service.SubscribeToNewslettersAsync(PageGuid, ValidFormData(), CancellationToken.None);

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task SubscribeToNewslettersAsync_DoesNotCallNewsletterService_WhenRecaptchaFails()
    {
        var page = BuildSubscribePage();
        SetupContentRetriever(page);
        SetupRecaptcha(false);

        await _service.SubscribeToNewslettersAsync(PageGuid, ValidFormData(), CancellationToken.None);

        _newsletterSubscriptionServiceMock.Verify(
            s => s.SubscribeToNewslettersAsync(It.IsAny<NewsletterSubscriptionFormData>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // ??? SubscribeToNewslettersAsync — happy path ?????????????????????????????

    [Test]
    public async Task SubscribeToNewslettersAsync_ReturnsTrue_WhenAllValidationPasses()
    {
        var page = BuildSubscribePage();
        SetupContentRetriever(page);
        SetupRecaptcha(true);
        _newsletterSubscriptionServiceMock
            .Setup(s => s.SubscribeToNewslettersAsync(It.IsAny<NewsletterSubscriptionFormData>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _service.SubscribeToNewslettersAsync(PageGuid, ValidFormData(), CancellationToken.None);

        Assert.That(result, Is.True);
    }

    [Test]
    public async Task SubscribeToNewslettersAsync_ReturnsFalse_WhenNewsletterServiceReturnsFalse()
    {
        var page = BuildSubscribePage();
        SetupContentRetriever(page);
        SetupRecaptcha(true);
        _newsletterSubscriptionServiceMock
            .Setup(s => s.SubscribeToNewslettersAsync(It.IsAny<NewsletterSubscriptionFormData>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _service.SubscribeToNewslettersAsync(PageGuid, ValidFormData(), CancellationToken.None);

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task SubscribeToNewslettersAsync_PassesFormData_ToNewsletterService()
    {
        var page = BuildSubscribePage();
        SetupContentRetriever(page);
        SetupRecaptcha(true);
        var formData = ValidFormData();
        _newsletterSubscriptionServiceMock
            .Setup(s => s.SubscribeToNewslettersAsync(formData, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await _service.SubscribeToNewslettersAsync(PageGuid, formData, CancellationToken.None);

        _newsletterSubscriptionServiceMock.Verify(
            s => s.SubscribeToNewslettersAsync(formData, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task SubscribeToNewslettersAsync_PassesCancellationToken_ToAllServices()
    {
        using var cts = new CancellationTokenSource();
        var token = cts.Token;
        var page = BuildSubscribePage();
        var formData = ValidFormData();

        _contentRetrieverMock
            .Setup(r => r.RetrieveAllPagesByGuids<Subscribe>(
                It.IsAny<Guid[]>(), cancellationToken: token))
            .ReturnsAsync([page]);
        _recaptchaValidationServiceMock
            .Setup(r => r.ValidateAsync(It.IsAny<bool>(), It.IsAny<decimal>(), It.IsAny<string>(), token))
            .ReturnsAsync(true);
        _newsletterSubscriptionServiceMock
            .Setup(s => s.SubscribeToNewslettersAsync(formData, token))
            .ReturnsAsync(true);

        await _service.SubscribeToNewslettersAsync(PageGuid, formData, token);

        _contentRetrieverMock.Verify(
            r => r.RetrieveAllPagesByGuids<Subscribe>(It.IsAny<Guid[]>(), cancellationToken: token),
            Times.Once);
        _recaptchaValidationServiceMock.Verify(
            r => r.ValidateAsync(It.IsAny<bool>(), It.IsAny<decimal>(), It.IsAny<string>(), token),
            Times.Once);
        _newsletterSubscriptionServiceMock.Verify(
            s => s.SubscribeToNewslettersAsync(formData, token),
            Times.Once);
    }

    [Test]
    public async Task SubscribeToNewslettersAsync_CallsRecaptchaValidation_WithRecaptchaTokenFromFormData()
    {
        var page = BuildSubscribePage();
        SetupContentRetriever(page);
        var formData = ValidFormData();
        formData.RecaptchaToken = "my-token";

        _recaptchaValidationServiceMock
            .Setup(r => r.ValidateAsync(It.IsAny<bool>(), It.IsAny<decimal>(), "my-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _newsletterSubscriptionServiceMock
            .Setup(s => s.SubscribeToNewslettersAsync(formData, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await _service.SubscribeToNewslettersAsync(PageGuid, formData, CancellationToken.None);

        _recaptchaValidationServiceMock.Verify(
            r => r.ValidateAsync(It.IsAny<bool>(), It.IsAny<decimal>(), "my-token", It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
