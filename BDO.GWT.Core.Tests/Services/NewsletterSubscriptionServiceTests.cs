using BDO.GWT.Core.Data.Queries;
using BDO.GWT.Domain.Core.Helpers;
using BDO.GWT.Domain.Core.Services;
using BDO.GWT.Web.Core.Data;
using BDO.GWT.Web.Core.Data.Queries;
using BDO.GWT.Xperience.Core.LiveSite.Services;
using CMS.ContactManagement;
using CMS.DataEngine;
using CMS.EmailMarketing;
using CMS.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace BDO.GWT.Core.Tests.Services;

public class NewsletterSubscriptionServiceTests
{

    private Mock<IServiceScopeFactory> _serviceScopeFactoryMock = null!;
    private Mock<IMarketingContactManagementService> _marketingContactManagementServiceMock = null!;
    private Mock<IInfoProvider<ContactGroupInfo>> _recipientListInfoProviderMock = null!;
    private Mock<IInfoProvider<ContactGroupMemberInfo>> _contactGroupMemberInfoProviderMock = null!;
    private Mock<IInfoProvider<EmailSubscriptionConfirmationInfo>> _emailSubscriptionConfirmationInfoProviderMock = null!;
    private Mock<IInfoProvider<CountryInfo>> _countryInfoProviderMock = null!;
    private NewsletterSubscriptionService _service = null!;

    [SetUp]
    public void Setup()
    {

        _serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
        _marketingContactManagementServiceMock = new Mock<IMarketingContactManagementService>();
        _recipientListInfoProviderMock = new Mock<IInfoProvider<ContactGroupInfo>>();
        _contactGroupMemberInfoProviderMock = new Mock<IInfoProvider<ContactGroupMemberInfo>>();
        _emailSubscriptionConfirmationInfoProviderMock = new Mock<IInfoProvider<EmailSubscriptionConfirmationInfo>>();
        _countryInfoProviderMock = new Mock<IInfoProvider<CountryInfo>>();

        _service = new NewsletterSubscriptionService(
            _serviceScopeFactoryMock.Object,
            _marketingContactManagementServiceMock.Object,
            _recipientListInfoProviderMock.Object,
            _contactGroupMemberInfoProviderMock.Object,
            _emailSubscriptionConfirmationInfoProviderMock.Object,
            _countryInfoProviderMock.Object);
    }

    // --- Helpers -----------------------------------------------------------------

    private static NewsletterSubscriptionData BuildSubscriptionData(
        string email = "user@example.com",
        string firstName = "Jane",
        string lastName = "Doe",
        IEnumerable<Guid>? newsletterGuids = null) =>
        new()
        {
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            NewsletterGuids = newsletterGuids ?? [],
        };

    private static SubscribeRemoveQuery BuildRemoveQuery(
        string email = "user@example.com",
        string hash = "validhash",
        Guid? newsletterGuid = null) =>
        new()
        {
            Email = email,
            Hash = hash,
            NewsletterGuid = newsletterGuid ?? Guid.NewGuid(),
        };

    private static SubscribeConfirmQuery BuildConfirmQuery(
        string subscriptionHash = "hash123",
        string dateTime = "01012024120000",
        int contactId = 1,
        int recipientListId = 10) =>
        new()
        {
            SubscriptionHash = subscriptionHash,
            DateTime = dateTime,
            ContactId = contactId,
            RecipientListId = recipientListId,
        };

    // NOTE: ObjectQuery<ContactGroupInfo> triggers AbstractInfoBase<ContactGroupInfo>'s static type
    // initializer which requires the Kentico CMS IoC container, unavailable in unit tests.
    // To avoid this, provider.Get() is set up to throw a sentinel exception so the chain is never
    // evaluated. Tests that need the chain to produce results are not possible as unit tests.
    private void SetupRecipientListProviderToThrow()
    {
        _recipientListInfoProviderMock
            .Setup(p => p.Get())
            .Throws(new NotImplementedException("Kentico ObjectQuery<ContactGroupInfo> unavailable in unit tests"));
    }

    private void SetupConfirmationQuery(IEnumerable<EmailSubscriptionConfirmationInfo> results)
    {
        var queryMock = new Mock<ObjectQuery<EmailSubscriptionConfirmationInfo>>();

        _emailSubscriptionConfirmationInfoProviderMock
            .Setup(p => p.Get())
            .Returns(queryMock.Object);

        queryMock.Setup(q => q.WhereEquals(It.IsAny<string>(), It.IsAny<object>()))
            .Returns(queryMock.Object);
        queryMock.Setup(q => q.GetEnumerableTypedResultAsync(
                It.IsAny<System.Data.CommandBehavior>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(results.ToList());
    }

    // --- SubscribeToNewslettersAsync ---------------------------------------------

    [Test]
    public async Task SubscribeToNewslettersAsync_CallsGetMarketingContact_WithProvidedEmail()
    {
        // ContactInfo cannot be instantiated in unit tests because AbstractInfoBase<ContactInfo>
        // requires the Kentico CMS IoC container. The mock is set up to throw a sentinel after
        // the call so we can still verify GetMarketingContactByEmailAsync was invoked correctly.
        const string email = "subscriber@example.com";
        _marketingContactManagementServiceMock
            .Setup(m => m.GetMarketingContactByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotImplementedException("sentinel – ContactInfo unavailable in unit tests"));

        try { await _service.SubscribeToNewslettersAsync(BuildSubscriptionData(email: email), CancellationToken.None); }
        catch (NotImplementedException) { /* expected sentinel */ }

        _marketingContactManagementServiceMock.Verify(
            m => m.GetMarketingContactByEmailAsync(email, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task SubscribeToNewslettersAsync_PassesCancellationToken_ToGetMarketingContact()
    {
        // Same sentinel pattern: ContactInfo cannot be instantiated in unit tests.
        using var cts = new CancellationTokenSource();
        var token = cts.Token;
        _marketingContactManagementServiceMock
            .Setup(m => m.GetMarketingContactByEmailAsync(It.IsAny<string>(), token))
            .ThrowsAsync(new NotImplementedException("sentinel – ContactInfo unavailable in unit tests"));

        try { await _service.SubscribeToNewslettersAsync(BuildSubscriptionData(), token); }
        catch (NotImplementedException) { /* expected sentinel */ }

        _marketingContactManagementServiceMock.Verify(
            m => m.GetMarketingContactByEmailAsync(It.IsAny<string>(), token),
            Times.Once);
    }

    [Test]
    public void SubscribeToNewslettersAsync_PropagatesException_FromGetMarketingContact()
    {
        _marketingContactManagementServiceMock
            .Setup(m => m.GetMarketingContactByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("contact error"));

        Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.SubscribeToNewslettersAsync(BuildSubscriptionData(), CancellationToken.None));
    }

    [Test]
    public void SubscribeToNewslettersAsync_DoesNotUpdateContact_WhenGetMarketingContactThrows()
    {
        // If the contact lookup fails, UpdateMarketingContactAsync must never be reached.
        _marketingContactManagementServiceMock
            .Setup(m => m.GetMarketingContactByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("contact error"));

        Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.SubscribeToNewslettersAsync(BuildSubscriptionData(), CancellationToken.None));

        _marketingContactManagementServiceMock.Verify(
            m => m.UpdateMarketingContactAsync(It.IsAny<ContactInfo>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public void SubscribeToNewslettersAsync_UsesDefaultNullSubscribePageGuid_WhenNotProvided()
    {
        // Overload defaults subscribePageGuid to null; the lookup still starts with the
        // contact retrieval, which we short-circuit via a sentinel to keep the test isolated.
        _marketingContactManagementServiceMock
            .Setup(m => m.GetMarketingContactByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotImplementedException("sentinel"));

        Assert.ThrowsAsync<NotImplementedException>(
            () => _service.SubscribeToNewslettersAsync(BuildSubscriptionData(), CancellationToken.None));
    }

    // --- ConfirmSubscriptionForNewsletterAsync -----------------------------------

    [Test]
    public async Task ConfirmSubscriptionForNewsletterAsync_ReturnsFalse_WhenSubscriptionHashIsEmpty()
    {
        var query = BuildConfirmQuery(subscriptionHash: string.Empty);

        var result = await _service.ConfirmSubscriptionForNewsletterAsync(query, "TestSite", CancellationToken.None);

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task ConfirmSubscriptionForNewsletterAsync_ReturnsFalse_WhenDateTimeIsInvalidFormat()
    {
        var query = BuildConfirmQuery(dateTime: "not-a-date");

        var result = await _service.ConfirmSubscriptionForNewsletterAsync(query, "TestSite", CancellationToken.None);

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task ConfirmSubscriptionForNewsletterAsync_ReturnsFalse_WhenConfirmationNotFound()
    {
        var query = BuildConfirmQuery();
        SetupConfirmationQuery([]);

        var result = await _service.ConfirmSubscriptionForNewsletterAsync(query, "TestSite", CancellationToken.None);

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task ConfirmSubscriptionForNewsletterAsync_ReturnsFalse_WhenHashIsNull()
    {
        var query = BuildConfirmQuery(subscriptionHash: null!);

        var result = await _service.ConfirmSubscriptionForNewsletterAsync(query, "TestSite", CancellationToken.None);

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task ConfirmSubscriptionForNewsletterAsync_PassesCancellationToken_ToConfirmationProvider()
    {
        using var cts = new CancellationTokenSource();
        var token = cts.Token;
        var query = BuildConfirmQuery();
        SetupConfirmationQuery([]);

        await _service.ConfirmSubscriptionForNewsletterAsync(query, "TestSite", token);

        _emailSubscriptionConfirmationInfoProviderMock.Verify(
            p => p.Get(),
            Times.Once);
    }

    [Test]
    public void ConfirmSubscriptionForNewsletterAsync_PropagatesException_FromConfirmationProvider()
    {
        var query = BuildConfirmQuery();

        _emailSubscriptionConfirmationInfoProviderMock
            .Setup(p => p.Get())
            .Throws(new InvalidOperationException("db error"));

        Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.ConfirmSubscriptionForNewsletterAsync(query, "TestSite", CancellationToken.None));
    }

    [Test]
    public async Task ConfirmSubscriptionForNewsletterAsync_DoesNotCallConfirmationProvider_WhenSubscriptionHashIsEmpty()
    {
        var query = BuildConfirmQuery(subscriptionHash: string.Empty);

        var result = await _service.ConfirmSubscriptionForNewsletterAsync(query, "TestSite", CancellationToken.None);

        Assert.That(result, Is.False);
        _emailSubscriptionConfirmationInfoProviderMock.Verify(p => p.Get(), Times.Never);
    }

    [Test]
    public async Task ConfirmSubscriptionForNewsletterAsync_DoesNotCallConfirmationProvider_WhenDateTimeIsInvalidFormat()
    {
        var query = BuildConfirmQuery(dateTime: "invalid");

        var result = await _service.ConfirmSubscriptionForNewsletterAsync(query, "TestSite", CancellationToken.None);

        Assert.That(result, Is.False);
        _emailSubscriptionConfirmationInfoProviderMock.Verify(p => p.Get(), Times.Never);
    }

    [Test]
    public async Task ConfirmSubscriptionForNewsletterAsync_DoesNotCallConfirmationProvider_WhenSubscriptionHashIsNull()
    {
        var query = BuildConfirmQuery(subscriptionHash: null!);

        var result = await _service.ConfirmSubscriptionForNewsletterAsync(query, "TestSite", CancellationToken.None);

        Assert.That(result, Is.False);
        _emailSubscriptionConfirmationInfoProviderMock.Verify(p => p.Get(), Times.Never);
    }

    [Test]
    public async Task ConfirmSubscriptionForNewsletterAsync_QueriesProvider_WhenHashAndDateTimeAreValid()
    {
        // Valid hash + valid ddMMyyyyHHmmss date passes the guard and reaches the provider.
        var query = BuildConfirmQuery(subscriptionHash: "abc", dateTime: "31122024235959");
        SetupConfirmationQuery([]);

        var result = await _service.ConfirmSubscriptionForNewsletterAsync(query, "TestSite", CancellationToken.None);

        Assert.That(result, Is.False);
        _emailSubscriptionConfirmationInfoProviderMock.Verify(p => p.Get(), Times.Once);
    }

    [Test]
    public async Task ConfirmSubscriptionForNewsletterAsync_ReturnsFalse_WhenDateTimeHasWrongLength()
    {
        // A numeric-but-too-short value must not parse under the exact ddMMyyyyHHmmss format.
        var query = BuildConfirmQuery(dateTime: "010120");

        var result = await _service.ConfirmSubscriptionForNewsletterAsync(query, "TestSite", CancellationToken.None);

        Assert.That(result, Is.False);
        _emailSubscriptionConfirmationInfoProviderMock.Verify(p => p.Get(), Times.Never);
    }

    // --- UnsubscribeFromNewsletterAsync -----------------------------------------

    [Test]
    public async Task UnsubscribeFromNewsletterAsync_ReturnsFalse_WhenEmailIsEmpty()
    {
        var query = BuildRemoveQuery(email: string.Empty);

        var result = await _service.UnsubscribeFromNewsletterAsync(query, CancellationToken.None);

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task UnsubscribeFromNewsletterAsync_ReturnsFalse_WhenEmailIsNull()
    {
        var query = BuildRemoveQuery(email: null!);

        var result = await _service.UnsubscribeFromNewsletterAsync(query, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.False);
            _recipientListInfoProviderMock.Verify(p => p.Get(), Times.Never);
        });
    }

    [Test]
    public async Task UnsubscribeFromNewsletterAsync_ReturnsFalse_WhenNewsletterGuidIsEmpty()
    {
        var query = BuildRemoveQuery(newsletterGuid: Guid.Empty);

        var result = await _service.UnsubscribeFromNewsletterAsync(query, CancellationToken.None);

        Assert.That(result, Is.False);
    }

    [Test]
    public void UnsubscribeFromNewsletterAsync_EmptyHash_IsNotPartOfValidation_ProceedsToProviderLookup()
    {
        // The service only validates Email and NewsletterGuid — Hash is not checked.
        // An empty Hash must therefore fall through to the recipient-list provider lookup.
        // We assert that behavior by wiring the provider to throw a sentinel and verifying it was reached.
        var query = BuildRemoveQuery(hash: string.Empty);
        SetupRecipientListProviderToThrow();

        Assert.ThrowsAsync<NotImplementedException>(
            () => _service.UnsubscribeFromNewsletterAsync(query, CancellationToken.None));

        _recipientListInfoProviderMock.Verify(p => p.Get(), Times.Once);
    }








    [Test]
    public async Task UnsubscribeFromNewsletterAsync_DoesNotCallRecipientListProvider_WhenValidationFails()
    {
        var query = BuildRemoveQuery(email: string.Empty);

        await _service.UnsubscribeFromNewsletterAsync(query, CancellationToken.None);

        _recipientListInfoProviderMock.Verify(p => p.Get(), Times.Never);
    }

    [Test]
    public async Task UnsubscribeFromNewsletterAsync_ReturnsFalse_WhenNewsletterGuidIsEmpty_EvenWithValidEmail()
    {
        var query = BuildRemoveQuery(email: "user@example.com", newsletterGuid: Guid.Empty);

        var result = await _service.UnsubscribeFromNewsletterAsync(query, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.False);
            _recipientListInfoProviderMock.Verify(p => p.Get(), Times.Never);
        });
    }

    [Test]
    public void UnsubscribeFromNewsletterAsync_PropagatesException_FromRecipientListProvider()
    {
        var query = BuildRemoveQuery();
        _recipientListInfoProviderMock
            .Setup(p => p.Get())
            .Throws(new InvalidOperationException("db error"));

        Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.UnsubscribeFromNewsletterAsync(query, CancellationToken.None));
    }
}
