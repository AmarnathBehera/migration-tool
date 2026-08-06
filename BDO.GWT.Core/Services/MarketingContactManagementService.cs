using BDO.GWT.Common.Core.Services;
using CMS.ContactManagement;
using CMS.DataEngine;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Domain.Core.Services;

public interface IMarketingContactManagementService
{
    Task<ContactInfo> GetMarketingContactByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task UpdateMarketingContactAsync(ContactInfo contact, CancellationToken cancellationToken);
}

public class MarketingContactManagementService : IMarketingContactManagementService
{
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IInfoProvider<ContactGroupInfo> _recipientListInfoProvider;
    private readonly IInfoProvider<ContactInfo> _contactInfoProvider;

    public MarketingContactManagementService(
        IDateTimeProvider dateTimeProvider,
        IInfoProvider<ContactGroupInfo> recipientListInfoProvider,
        IInfoProvider<ContactInfo> contactInfoProvider)
    {
        _dateTimeProvider = dateTimeProvider;
        _recipientListInfoProvider = recipientListInfoProvider;
        _contactInfoProvider = contactInfoProvider;
    }

    public async Task<ContactInfo> GetMarketingContactByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var results = await _contactInfoProvider
           .Get()
           .WhereEquals(nameof(ContactInfo.ContactEmail), email)
           .TopN(1)
           .GetEnumerableTypedResultAsync(cancellationToken: cancellationToken);

        var contact = results.FirstOrDefault();

        if (contact is null)
        {
            contact = CreateAnonymousContact(email);
        }

        _contactInfoProvider.Set(contact);

        return contact;
    }

    public Task UpdateMarketingContactAsync(ContactInfo contact, CancellationToken cancellationToken)
    {
        _contactInfoProvider.Set(contact);
        return Task.CompletedTask;
    }

    private ContactInfo CreateAnonymousContact(string email)
    {
        var now = _dateTimeProvider.Now;

        return new ContactInfo
        {
            ContactEmail = email,
            ContactLastName = $"Anonymous - {now:yyyy-MM-dd HH:mm:ss.fff}",
            ContactMonitored = true,
            ContactCreated = now,
            ContactLastModified = now,
            ContactGUID = Guid.NewGuid(),
        };
    }
}