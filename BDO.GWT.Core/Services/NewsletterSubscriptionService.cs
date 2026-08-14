using BDO.GWT.Core.Data.Queries;
using BDO.GWT.Domain.Core.Helpers;
using BDO.GWT.Domain.Core.Services;
using BDO.GWT.Web.Core.Data;
using BDO.GWT.Web.Core.Data.Queries;
using CMS.ContactManagement;
using CMS.DataEngine;
using CMS.EmailEngine;
using CMS.EmailMarketing;
using CMS.Websites.Routing;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;

namespace BDO.GWT.Xperience.Core.LiveSite.Services;

public interface INewsletterSubscriptionService
{
    Task<bool> SubscribeToNewslettersAsync(NewsletterSubscriptionData newsletterSubscriptionData, CancellationToken cancellationToken);
    Task<bool> ConfirmSubscriptionForNewsletterAsync(SubscribeConfirmQuery subscribeConfirmQuery, string siteName, CancellationToken cancellationToken);
    Task<bool> UnsubscribeFromNewsletterAsync(SubscribeRemoveQuery subscribeRemoveQuery, CancellationToken cancellationToken);
}

public class NewsletterSubscriptionService(
    IServiceScopeFactory scopeFactory,
    IMarketingContactManagementService marketingContactManagementService,
    IInfoProvider<ContactGroupInfo> recipientListInfoProvider,
    IInfoProvider<ContactGroupMemberInfo> contactGroupMemberInfoProvider,
    IInfoProvider<EmailSubscriptionConfirmationInfo> emailSubscriptionConfirmationInfoProvider) : INewsletterSubscriptionService
{
    private readonly IMarketingContactManagementService _marketingContactManagementService = marketingContactManagementService;
    private readonly IInfoProvider<ContactGroupInfo> _recipientListInfoProvider = recipientListInfoProvider;
    private readonly IInfoProvider<ContactGroupMemberInfo> _contactGroupMemberInfoProvider = contactGroupMemberInfoProvider;
    private readonly IInfoProvider<EmailSubscriptionConfirmationInfo> _emailSubscriptionConfirmationInfoProvider = emailSubscriptionConfirmationInfoProvider;
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private const string EmailKey = "Email";

    public async Task<bool> SubscribeToNewslettersAsync(NewsletterSubscriptionData newsletterSubscriptionData, CancellationToken cancellationToken)
    {
        var contact = await _marketingContactManagementService.GetMarketingContactByEmailAsync(newsletterSubscriptionData.Email, cancellationToken);

        await UpdateContactDataAsync(contact, newsletterSubscriptionData, cancellationToken);
        var newsletterGuidList = newsletterSubscriptionData.NewsletterGuids.ToList();

        var recipientLists = (await _recipientListInfoProvider.Get()
            .WhereIn(nameof(ContactGroupInfo.ContactGroupGUID), newsletterGuidList)
            .WhereEquals(nameof(ContactGroupInfo.ContactGroupIsRecipientList), true)
            .Columns(
                nameof(ContactGroupInfo.ContactGroupID),
                nameof(ContactGroupInfo.ContactGroupName),
                nameof(ContactGroupInfo.ContactGroupDisplayName),
                nameof(ContactGroupInfo.ContactGroupGUID))
            .OrderBy(nameof(ContactGroupInfo.ContactGroupDisplayName))
            .GetEnumerableTypedResultAsync(cancellationToken: cancellationToken))
            .ToList();
        foreach (var recipientList in recipientLists)
        {
            var alreadyMember = (await _contactGroupMemberInfoProvider.Get()
                .WhereEquals(nameof(ContactGroupMemberInfo.ContactGroupMemberRelatedID), contact.ContactID)
                .WhereEquals(nameof(ContactGroupMemberInfo.ContactGroupMemberType), ContactGroupMemberTypeEnum.Contact)
                .WhereEquals(nameof(ContactGroupMemberInfo.ContactGroupMemberContactGroupID), recipientList.ContactGroupID)
                .GetEnumerableTypedResultAsync(cancellationToken: cancellationToken))
                .Any();

            if (alreadyMember)
            {
                continue;
            }

            var member = new ContactGroupMemberInfo
            {
                ContactGroupMemberRelatedID = contact.ContactID,
                ContactGroupMemberType = ContactGroupMemberTypeEnum.Contact,
                ContactGroupMemberContactGroupID = recipientList.ContactGroupID,
                ContactGroupMemberFromManual = true,
            };
            member.Insert();
            var confirmation = new EmailSubscriptionConfirmationInfo
            {
                EmailSubscriptionConfirmationContactID = contact.ContactID,
                EmailSubscriptionConfirmationRecipientListID = recipientList.ContactGroupID,
                EmailSubscriptionConfirmationIsApproved = false,
                EmailSubscriptionConfirmationDate = DateTime.Now,
            };
            confirmation.Insert();
            var fromEmail = ResolveFromEmailForCurrentChannel() ?? string.Empty;

            var email = new EmailMessage
            {
                From = fromEmail,
                Recipients = newsletterSubscriptionData.Email,
                Subject = "Newsletter Subscription Confirmation",
                EmailFormat = EmailFormatEnum.Html
            };
            using (var scope = _scopeFactory.CreateScope())
            {
                var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
                await emailService.SendEmail(email).ConfigureAwait(false);
            }
        }
     

        return true;
    }

    public async Task<bool> ConfirmSubscriptionForNewsletterAsync(SubscribeConfirmQuery subscribeConfirmQuery, string siteName, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(subscribeConfirmQuery.SubscriptionHash) ||
            !DateTime.TryParseExact(subscribeConfirmQuery.DateTime, "ddMMyyyyHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
        {
            return false;
        }


        var confirmation = (await _emailSubscriptionConfirmationInfoProvider.Get()
            .WhereEquals(nameof(EmailSubscriptionConfirmationInfo.EmailSubscriptionConfirmationContactID), subscribeConfirmQuery.ContactId)
            .WhereEquals(nameof(EmailSubscriptionConfirmationInfo.EmailSubscriptionConfirmationRecipientListID), subscribeConfirmQuery.RecipientListId)
            .GetEnumerableTypedResultAsync(cancellationToken: cancellationToken))
            .FirstOrDefault();

        if (confirmation is null)
        {
            return false;
        }

        confirmation.EmailSubscriptionConfirmationIsApproved = true;
        confirmation.Update();

        return true;
    }

    public async Task<bool> UnsubscribeFromNewsletterAsync(SubscribeRemoveQuery subscribeRemoveQuery, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(subscribeRemoveQuery.Email) ||
            subscribeRemoveQuery.NewsletterGuid == Guid.Empty)     {
            return false;
        }

        var recipientList = (await _recipientListInfoProvider.Get()
            .WhereEquals(nameof(ContactGroupInfo.ContactGroupGUID), subscribeRemoveQuery.NewsletterGuid)
            .WhereEquals(nameof(ContactGroupInfo.ContactGroupIsRecipientList), true)
            .Columns(nameof(ContactGroupInfo.ContactGroupID))
            .GetEnumerableTypedResultAsync(cancellationToken: cancellationToken))
            .FirstOrDefault();

        if (recipientList is null)
        {
            return false;
        }

        var recipientListId = recipientList.ContactGroupID;

        var contact = await _marketingContactManagementService.GetMarketingContactByEmailAsync(subscribeRemoveQuery.Email, cancellationToken);

        _emailSubscriptionConfirmationInfoProvider.BulkDelete(new WhereCondition()
            .WhereEquals(nameof(EmailSubscriptionConfirmationInfo.EmailSubscriptionConfirmationContactID), contact.ContactID)
            .WhereEquals(nameof(EmailSubscriptionConfirmationInfo.EmailSubscriptionConfirmationRecipientListID), recipientListId));

        _contactGroupMemberInfoProvider.BulkDelete(new WhereCondition()
            .WhereEquals(nameof(ContactGroupMemberInfo.ContactGroupMemberRelatedID), contact.ContactID)
            .WhereEquals(nameof(ContactGroupMemberInfo.ContactGroupMemberType), ContactGroupMemberTypeEnum.Contact)
            .WhereEquals(nameof(ContactGroupMemberInfo.ContactGroupMemberContactGroupID), recipientListId));

        return true;
    }

    private async Task UpdateContactDataAsync(ContactInfo contact, NewsletterSubscriptionData data, CancellationToken cancellationToken)
    {
        contact.ContactFirstName = data.FirstName;
        contact.ContactLastName = data.LastName;

        if (!string.IsNullOrWhiteSpace(data.Company))
        {
            contact.ContactCompanyName = data.Company;
        }

        if (!string.IsNullOrWhiteSpace(data.JobTitle))
        {
            contact.ContactJobTitle = data.JobTitle;
        }

        await _marketingContactManagementService.UpdateMarketingContactAsync(contact, cancellationToken);
    }

    private string? ResolveFromEmailForCurrentChannel()
    {
        using var scope = _scopeFactory.CreateScope();
        var websiteChannelContext = scope.ServiceProvider.GetRequiredService<IWebsiteChannelContext>();
        var webChannelSettingsInfoProvider = scope.ServiceProvider.GetRequiredService<IInfoProvider<WebChannelSettingsInfo>>();
        var emailInfoProvider = scope.ServiceProvider.GetRequiredService<IInfoProvider<EmailInfo>>();

        int currentWebsiteID = websiteChannelContext.WebsiteChannelID;
       
        var currentChannelSettings = webChannelSettingsInfoProvider
            .Get()
            .WhereEquals(nameof(WebChannelSettingsInfo.WebChannelSettingsChannelID), currentWebsiteID)
            .TopN(1)
            .FirstOrDefault();

        if (currentChannelSettings is null)
        {
            return null;
        }

        return emailInfoProvider.Get()
            .WhereEquals(nameof(EmailInfo.DisplayName), EmailKey)
            .WhereEquals(nameof(EmailInfo.EmailSettingsWebChannelSettingID), currentChannelSettings.WebChannelSettingsID)
            .TopN(1)
            .GetEnumerableTypedResult()
            .FirstOrDefault()?.FromEmail;
    }
}