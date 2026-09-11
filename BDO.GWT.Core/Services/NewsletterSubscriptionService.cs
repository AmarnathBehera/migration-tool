using BDO;
using BDO.GWT.Core;
using BDO.GWT.Core.Data.Queries;
using BDO.GWT.Domain.Core.Services;
using BDO.GWT.Web.Core.Data;
using BDO.GWT.Web.Core.Data.Queries;
using CMS.ContactManagement;
using CMS.DataEngine;
using CMS.EmailEngine;
using CMS.EmailLibrary;
using CMS.EmailMarketing;
using CMS.FormEngine;
using CMS.Globalization;
using CMS.Websites;
using CMS.Websites.Routing;
using Kentico.Content.Web.Mvc.Routing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;
using System.Web;

namespace BDO.GWT.Xperience.Core.LiveSite.Services;

public interface INewsletterSubscriptionService
{
    Task<bool> SubscribeToNewslettersAsync(NewsletterSubscriptionData newsletterSubscriptionData, CancellationToken cancellationToken, Guid? subscribePageGuid = null);
    Task<bool> ConfirmSubscriptionForNewsletterAsync(SubscribeConfirmQuery subscribeConfirmQuery, string siteName, CancellationToken cancellationToken);
    Task<bool> UnsubscribeFromNewsletterAsync(SubscribeRemoveQuery subscribeRemoveQuery, CancellationToken cancellationToken);
}

public class NewsletterSubscriptionService(
    IServiceScopeFactory scopeFactory,
    IMarketingContactManagementService marketingContactManagementService,
    IInfoProvider<ContactGroupInfo> recipientListInfoProvider,
    IInfoProvider<ContactGroupMemberInfo> contactGroupMemberInfoProvider,
    IInfoProvider<EmailSubscriptionConfirmationInfo> emailSubscriptionConfirmationInfoProvider,
    IInfoProvider<CountryInfo> countryInfoProvider) : INewsletterSubscriptionService
{
    private readonly IMarketingContactManagementService _marketingContactManagementService = marketingContactManagementService;
    private readonly IInfoProvider<ContactGroupInfo> _recipientListInfoProvider = recipientListInfoProvider;
    private readonly IInfoProvider<ContactGroupMemberInfo> _contactGroupMemberInfoProvider = contactGroupMemberInfoProvider;
    private readonly IInfoProvider<EmailSubscriptionConfirmationInfo> _emailSubscriptionConfirmationInfoProvider = emailSubscriptionConfirmationInfoProvider;
    private readonly IInfoProvider<CountryInfo> _countryInfoProvider = countryInfoProvider;
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private const string EmailKey = "Email";

    public async Task<bool> SubscribeToNewslettersAsync(NewsletterSubscriptionData newsletterSubscriptionData, CancellationToken cancellationToken, Guid? subscribePageGuid = null)
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
                EmailSubscriptionConfirmationIsApproved = true,
                EmailSubscriptionConfirmationDate = DateTime.Now,
            };
            confirmation.Insert();

            var fromEmail = ResolveFromEmailForCurrentChannel() ?? string.Empty;
            var emailConfig = EmailTemplateInfo.Provider.Get("SubscribeConfirm");
            if (emailConfig is null)
            {
                return false;
            }

            var baseUnsubscribeUrl = await ResolveSubscribeRemoveRelativeUrlAsync(subscribePageGuid, cancellationToken) ?? "/Subscribe/Remove";
            var baseConfirmsubscribeUrl = await ResolveSubscribeConfirmRelativeUrlAsync(subscribePageGuid, cancellationToken) ?? "/Subscribe/Remove";
            var unsubscribeUrl = QueryHelpers.AddQueryString(baseUnsubscribeUrl, new Dictionary<string, string?>
            {
                ["email"] = newsletterSubscriptionData.Email,
                ["newsletterGuid"] = recipientList.ContactGroupGUID.ToString(),
            });

            var safeFirstName = HttpUtility.HtmlEncode(newsletterSubscriptionData.FirstName);
            var safeRecipientList = HttpUtility.HtmlEncode(recipientList.ContactGroupDisplayName);
            var safeUnsubscribeUrl = HttpUtility.HtmlAttributeEncode(unsubscribeUrl);
            using var scope = _scopeFactory.CreateScope();

            var rawBody =
                $"<p><strong>Hello</strong> {safeFirstName}</p>" +
                $"<p><strong>you have been subscribed to:</strong> {safeRecipientList}</p>" +
                $"<p><a href='{safeUnsubscribeUrl}'>Unsubscribe</a></p>"+
                $"<p><a href='{baseConfirmsubscribeUrl}'>Subscribe Confirm</a></p>";

            var email = new EmailMessage
            {
                From = fromEmail,
                Recipients = newsletterSubscriptionData.Email,
                Subject = "Newsletter Subscription Confirmation",
                EmailFormat = EmailFormatEnum.Html,
                Body = rawBody
            };

            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
            await emailService.SendEmail(email).ConfigureAwait(false);
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

    public async Task<bool> UnsubscribeFromNewsletterAsync(
        SubscribeRemoveQuery subscribeRemoveQuery,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(subscribeRemoveQuery.Email) ||
            subscribeRemoveQuery.NewsletterGuid == Guid.Empty)
        {
            return false;
        }

        var recipientList = (await _recipientListInfoProvider
            .Get()
            .WhereEquals(
                nameof(ContactGroupInfo.ContactGroupGUID),
                subscribeRemoveQuery.NewsletterGuid)
            .WhereEquals(
                nameof(ContactGroupInfo.ContactGroupIsRecipientList),
                true)
            .Columns(
                nameof(ContactGroupInfo.ContactGroupID))
            .GetEnumerableTypedResultAsync(
                cancellationToken: cancellationToken))
            .FirstOrDefault();

        if (recipientList is null)
        {
            return false;
        }

        var contact = await _marketingContactManagementService
            .GetMarketingContactByEmailAsync(
                subscribeRemoveQuery.Email,
                cancellationToken);

        if (contact is null)
        {
            return false;
        }

        var recipientListId = recipientList.ContactGroupID;

        // Find existing subscription confirmation
        var confirmation = (await _emailSubscriptionConfirmationInfoProvider
            .Get()
            .WhereEquals(
                nameof(EmailSubscriptionConfirmationInfo.EmailSubscriptionConfirmationContactID),
                contact.ContactID)
            .WhereEquals(
                nameof(EmailSubscriptionConfirmationInfo.EmailSubscriptionConfirmationRecipientListID),
                recipientListId)
            .GetEnumerableTypedResultAsync(
                cancellationToken: cancellationToken))
            .FirstOrDefault();

        // Mark subscription as unsubscribed
        if (confirmation is not null)
        {
            confirmation.EmailSubscriptionConfirmationIsApproved = false;
            confirmation.Update();
        }


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
        if (!string.IsNullOrWhiteSpace(data.Consent))
        {
            contact.ContactNotes = data.Consent;
        }
        if (!string.IsNullOrWhiteSpace(data.JobTitle))
        {
            contact.ContactJobTitle = data.JobTitle;
        }
        if (data.CountryGuid.HasValue && data.CountryGuid.Value != Guid.Empty)
        {
            var country = (await _countryInfoProvider.Get()
                .WhereEquals(nameof(CountryInfo.CountryGUID), data.CountryGuid.Value)
                .Columns(nameof(CountryInfo.CountryID))
                .TopN(1)
                .GetEnumerableTypedResultAsync(cancellationToken: cancellationToken))
                .FirstOrDefault();

            if (country is not null)
            {
                contact.ContactCountryID = country.CountryID;
            }
        }
        await _marketingContactManagementService.UpdateMarketingContactAsync(contact, cancellationToken);
    }
    private async Task<string?> ResolveSubscribeConfirmRelativeUrlAsync(Guid? subscribePageGuid, CancellationToken cancellationToken)
    {
        if (!subscribePageGuid.HasValue || subscribePageGuid.Value == Guid.Empty)
        {
            return null;
        }

        using var scope = _scopeFactory.CreateScope();
        var contentService = scope.ServiceProvider.GetRequiredService<IContentService>();
        var urlRetriever = scope.ServiceProvider.GetRequiredService<IWebPageUrlRetriever>();
        var languageRetriever = scope.ServiceProvider.GetRequiredService<IPreferredLanguageRetriever>();

        var subscribePage = (await contentService.GetPagesByGuidsAsync<Subscribe>([subscribePageGuid.Value], cancellationToken))
            .FirstOrDefault();

        if (subscribePage?.SystemFields is null)
        {
            return null;
        }

        var subscribeRemovePage = (await contentService.GetContentType<SubscribeConfirm>(cancellationToken))
            .FirstOrDefault(p => p.SystemFields?.WebPageItemParentID == subscribePage.SystemFields.WebPageItemID);

        if (subscribeRemovePage?.SystemFields is null)
        {
            return null;
        }

        var languageName = languageRetriever.Get();
        var url = await urlRetriever.Retrieve(
            subscribeRemovePage,
            languageName,
            cancellationToken);

        return url.AbsoluteUrl;
    }

    private async Task<string?> ResolveSubscribeRemoveRelativeUrlAsync(Guid? subscribePageGuid, CancellationToken cancellationToken)
    {
        if (!subscribePageGuid.HasValue || subscribePageGuid.Value == Guid.Empty)
        {
            return null;
        }

        using var scope = _scopeFactory.CreateScope();
        var contentService = scope.ServiceProvider.GetRequiredService<IContentService>();
        var urlRetriever = scope.ServiceProvider.GetRequiredService<IWebPageUrlRetriever>();
        var languageRetriever = scope.ServiceProvider.GetRequiredService<IPreferredLanguageRetriever>();

        var subscribePage = (await contentService.GetPagesByGuidsAsync<Subscribe>([subscribePageGuid.Value], cancellationToken))
            .FirstOrDefault();

        if (subscribePage?.SystemFields is null)
        {
            return null;
        }

        var subscribeRemovePage = (await contentService.GetContentType<SubscribeRemove>(cancellationToken))
            .FirstOrDefault(p => p.SystemFields?.WebPageItemParentID == subscribePage.SystemFields.WebPageItemID);

        if (subscribeRemovePage?.SystemFields is null)
        {
            return null;
        }

        var languageName = languageRetriever.Get();
        var url = await urlRetriever.Retrieve(
            subscribeRemovePage,
            languageName,
            cancellationToken);

        return url.AbsoluteUrl;
    }

    private string? ResolveFromEmailForCurrentChannel()
    {
        using var scope = _scopeFactory.CreateScope();
        var websiteChannelContext = scope.ServiceProvider.GetRequiredService<IWebsiteChannelContext>();
        var webChannelSettingsInfoProvider = scope.ServiceProvider.GetRequiredService<IInfoProvider<WebChannelSettingsInfo>>();
        var snippetInfoProvider = scope.ServiceProvider.GetRequiredService<IInfoProvider<WebChannelSnippetInfo>>();
        var globalSettingsProvider = scope.ServiceProvider.GetRequiredService<IInfoProvider<GlobalsettingskeyInfo>>();

        int currentWebsiteID = websiteChannelContext.WebsiteChannelID;

        var currentChannelSettings = webChannelSettingsInfoProvider
            .Get()
            .WhereEquals(nameof(WebChannelSettingsInfo.WebChannelSettingsChannelID), currentWebsiteID)
            .TopN(1)
            .GetEnumerableTypedResult()
            .FirstOrDefault();

        if (currentChannelSettings is null)
        {
            return null;
        }

        var snippet = snippetInfoProvider.Get()
            .WhereEquals(nameof(WebChannelSnippetInfo.WebChannelSnippetWebChannelSettingsID), currentChannelSettings.WebChannelSettingsID)
            .TopN(1)
            .GetEnumerableTypedResult()
            .FirstOrDefault();

        if (snippet is null)
        {
            return null;
        }
        if (snippet.FromEmailInheritfromglobal)
        {
            var global = globalSettingsProvider.Get()
                .TopN(1)
                .GetEnumerableTypedResult()
                .FirstOrDefault();

            return global?.FromEmail;
        }

        return snippet.FromEmail;
    }
}