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
    private readonly IInfoProvider<ContactInfo> _contactInfoProvider;

    public MarketingContactManagementService(
        IDateTimeProvider dateTimeProvider,
        IInfoProvider<ContactInfo> contactInfoProvider)
    {
        _dateTimeProvider = dateTimeProvider;
        _contactInfoProvider = contactInfoProvider;
    }

    public async Task<ContactInfo> GetMarketingContactByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        // Recipients must be uniquely identified by email. Normalize the input so a
        // trailing space or different casing does not cause a new contact to be created
        // (which could then be auto-merged onto an unrelated recipient with the same name).
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email is required to identify a recipient.", nameof(email));
        }

        var normalizedEmail = email.Trim();

        var results = await _contactInfoProvider
           .Get()
           .WhereEquals(nameof(ContactInfo.ContactEmail), normalizedEmail)
           .TopN(1)
           .GetEnumerableTypedResultAsync(cancellationToken: cancellationToken);

        var contact = results.FirstOrDefault();

        if (contact is not null)
        {
            // Existing recipient — return as-is. Do NOT call Set() here; that would
            // fire an unnecessary UPDATE and, combined with name-based contact
            // merging, can overwrite a different recipient that shares the same name.
            return contact;
        }

        // No recipient with this email exists — create a brand new, uniquely-identified record.
        contact = CreateAnonymousContact(normalizedEmail);
        _contactInfoProvider.Set(contact);
        return contact;
    }

    public Task UpdateMarketingContactAsync(ContactInfo contact, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(contact);

        // Ensure we only update the exact record identified by ContactID.
        // A ContactID of 0 would cause Kentico to INSERT and potentially match/merge
        // against an existing contact by name, which must never happen here.
        if (contact.ContactID <= 0)
        {
            throw new InvalidOperationException(
                "Refusing to update a recipient without a persisted ContactID. " +
                "Use GetMarketingContactByEmailAsync to load or create the recipient first.");
        }

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