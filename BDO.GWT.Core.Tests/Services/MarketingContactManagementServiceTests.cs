using BDO.GWT.Common.Core.Services;
using BDO.GWT.Domain.Core.Services;
using CMS.ContactManagement;
using CMS.DataEngine;
using Moq;

namespace BDO.GWT.Core.Tests.Services;

public class MarketingContactManagementServiceTests
{
    private Mock<IDateTimeProvider> _dateTimeProviderMock = null!;
    private Mock<IInfoProvider<ContactInfo>> _contactInfoProviderMock = null!;
    private MarketingContactManagementService _service = null!;

    [SetUp]
    public void Setup()
    {
        _dateTimeProviderMock = new Mock<IDateTimeProvider>();
        _contactInfoProviderMock = new Mock<IInfoProvider<ContactInfo>>();

        _service = new MarketingContactManagementService(
            _dateTimeProviderMock.Object,
            _contactInfoProviderMock.Object);
    }

    [Test]
    public void UpdateMarketingContactAsync_WhenContactIsNull_Throws()
    {
        Assert.ThrowsAsync<ArgumentNullException>(() =>
            _service.UpdateMarketingContactAsync(null!, CancellationToken.None));
    }

    // NOTE: ContactID validation cannot be exercised directly here because reading /
    // writing ContactInfo.ContactID requires the Kentico DataClass infrastructure,
    // which is unavailable in unit tests. The guard is covered by integration tests.

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public void GetMarketingContactByEmailAsync_WhenEmailIsNullOrWhitespace_Throws(string? email)
    {
        Assert.ThrowsAsync<ArgumentException>(() =>
            _service.GetMarketingContactByEmailAsync(email!, CancellationToken.None));
    }

    [Test]
    public void GetMarketingContactByEmailAsync_WhenInfoProviderGetThrows_PropagatesException()
    {
        _contactInfoProviderMock
            .Setup(p => p.Get())
            .Throws(new InvalidOperationException("query unavailable"));

        Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.GetMarketingContactByEmailAsync("user@example.com", CancellationToken.None));
    }

    [Test]
    public async Task GetMarketingContactByEmailAsync_WhenContactExists_ReturnsContactAndDoesNotCallSet()
    {
        // Recipients are unique per email. When a contact already exists for the given
        // email, the service must return it as-is without calling Set() — otherwise
        // Kentico's name-based contact merging could overwrite an unrelated recipient
        // that shares the same first/last name.
        const string email = "existing@example.com";
        var existingContact = CreateContactInfoStub();

        var queryMock = new Mock<ObjectQuery<ContactInfo>>();
        queryMock.Setup(q => q.WhereEquals(nameof(ContactInfo.ContactEmail), email))
            .Returns(queryMock.Object);
        queryMock.Setup(q => q.TopN(1))
            .Returns(queryMock.Object);
        queryMock.Setup(q => q.GetEnumerableTypedResultAsync(
                It.IsAny<System.Data.CommandBehavior>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { existingContact });

        _contactInfoProviderMock.Setup(p => p.Get()).Returns(queryMock.Object);

        var result = await _service.GetMarketingContactByEmailAsync(email, CancellationToken.None);

        Assert.That(result, Is.SameAs(existingContact));
        _contactInfoProviderMock.Verify(p => p.Set(It.IsAny<ContactInfo>()), Times.Never);
    }

    [Test]
    public async Task GetMarketingContactByEmailAsync_TrimsEmailBeforeLookup()
    {
        // Whitespace differences in the submitted email must not cause a new duplicate
        // recipient to be created (which could then be name-merged onto a different one).
        const string normalizedEmail = "existing@example.com";
        var existingContact = CreateContactInfoStub();

        var queryMock = new Mock<ObjectQuery<ContactInfo>>();
        queryMock.Setup(q => q.WhereEquals(nameof(ContactInfo.ContactEmail), normalizedEmail))
            .Returns(queryMock.Object)
            .Verifiable();
        queryMock.Setup(q => q.TopN(1)).Returns(queryMock.Object);
        queryMock.Setup(q => q.GetEnumerableTypedResultAsync(
                It.IsAny<System.Data.CommandBehavior>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { existingContact });

        _contactInfoProviderMock.Setup(p => p.Get()).Returns(queryMock.Object);

        var result = await _service.GetMarketingContactByEmailAsync("  existing@example.com  ", CancellationToken.None);

        Assert.That(result, Is.SameAs(existingContact));
        queryMock.Verify();
    }

    // NOTE: The "creates a new ContactInfo when none exists" branch cannot be unit-tested
    // because instantiating ContactInfo / setting its fields requires the Kentico
    // DataClass IoC container, unavailable outside an integrated Kentico host.

    private static ContactInfo CreateContactInfoStub() =>
        (ContactInfo)System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject(typeof(ContactInfo));
}
