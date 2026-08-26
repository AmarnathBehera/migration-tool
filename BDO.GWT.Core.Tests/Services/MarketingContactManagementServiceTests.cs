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
    public async Task UpdateMarketingContactAsync_CallsSet()
    {
        var contact = CreateContactInfoStub();

        await _service.UpdateMarketingContactAsync(contact, CancellationToken.None);

        _contactInfoProviderMock.Verify(p => p.Set(contact), Times.Once);
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
    public async Task GetMarketingContactByEmailAsync_WhenContactExists_ReturnsContactAndCallsSet()
    {
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
        _contactInfoProviderMock.Verify(p => p.Set(existingContact), Times.Once);
    }

    private static ContactInfo CreateContactInfoStub() =>
        (ContactInfo)System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject(typeof(ContactInfo));
}
