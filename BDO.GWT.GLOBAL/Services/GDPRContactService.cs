using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using CMS.ContactManagement;
using CMS.DataEngine;
using CMS.Helpers;

namespace BDO.GWT.GLOBAL.Services;

public interface IGDPRContactLookupService
{
    string GetContactAsText(string email);
    string GetContactAsXml(string email);
    bool DeleteContactByEmail(string email);
}

public sealed class GDPRContactService : IGDPRContactLookupService
{
    private readonly IInfoProvider<ContactInfo> contactInfoProvider;

    public GDPRContactService(IInfoProvider<ContactInfo> contactInfoProvider)
    {
        this.contactInfoProvider = contactInfoProvider;
    }

    public string GetContactAsText(string email)
    {
        ContactInfo contact = GetContactByEmail(email);
        if (contact is null)
        {
            return $"No OM Contact found for email: {email}";
        }

        int contactId = ValidationHelper.GetInteger(contact.GetValue("ContactID"), 0);
        string contactLastName = ValidationHelper.GetString(contact.GetValue("ContactLastName"), string.Empty);
        string contactEmail = ValidationHelper.GetString(contact.GetValue("ContactEmail"), string.Empty);
        int contactGender = ValidationHelper.GetInteger(contact.GetValue("ContactGender"), 0);
        int contactMonitored = ValidationHelper.GetBoolean(contact.GetValue("ContactMonitored"), false) ? 1 : 0;
        Guid contactGuid = ValidationHelper.GetGuid(contact.GetValue("ContactGUID"), Guid.Empty);
        DateTime contactLastModified = ValidationHelper.GetDateTime(contact.GetValue("ContactLastModified"), DateTime.MinValue);
        DateTime contactCreated = ValidationHelper.GetDateTime(contact.GetValue("ContactCreated"), DateTime.MinValue);

        var builder = new StringBuilder();
        builder.AppendLine("Site Activity:");
        builder.AppendLine("\tOM Contact");
        builder.AppendLine($"\t\tContact ID:\t{contactId}");
        builder.AppendLine($"\t\tContact Last Name:\t{contactLastName}");
        builder.AppendLine($"\t\tContact Email:\t{contactEmail}");
        builder.AppendLine($"\t\tContact Gender:\t{contactGender}");
        builder.AppendLine($"\t\tContact Monitored:\t{contactMonitored}");
        builder.AppendLine($"\t\tContact GUID:\t{contactGuid.ToString().ToUpperInvariant()}");
        builder.AppendLine($"\t\tContact Last Modified:\t{FormatDate(contactLastModified)}");
        builder.AppendLine($"\t\tContact Created:\t{FormatDate(contactCreated)}");
        builder.AppendLine("\t\tOM Activity:");

        return builder.ToString().TrimEnd();
    }

    public string GetContactAsXml(string email)
    {
        ContactInfo contact = GetContactByEmail(email);
        if (contact is null)
        {
            return "<XML>\n  <OM_Contact />\n</XML>";
        }

        int contactId = ValidationHelper.GetInteger(contact.GetValue("ContactID"), 0);
        string contactLastName = ValidationHelper.GetString(contact.GetValue("ContactLastName"), string.Empty);
        string contactEmail = ValidationHelper.GetString(contact.GetValue("ContactEmail"), string.Empty);
        int contactGender = ValidationHelper.GetInteger(contact.GetValue("ContactGender"), 0);
        int contactMonitored = ValidationHelper.GetBoolean(contact.GetValue("ContactMonitored"), false) ? 1 : 0;
        string contactGuid = ValidationHelper.GetGuid(contact.GetValue("ContactGUID"), Guid.Empty).ToString().ToUpperInvariant();
        DateTime contactLastModified = ValidationHelper.GetDateTime(contact.GetValue("ContactLastModified"), DateTime.MinValue);
        DateTime contactCreated = ValidationHelper.GetDateTime(contact.GetValue("ContactCreated"), DateTime.MinValue);

        XElement xml = new(
            "XML",
            new XElement(
                "OM_Contact",
                new XElement("ContactID", contactId),
                new XElement("ContactLastName", contactLastName),
                new XElement("ContactEmail", contactEmail),
                new XElement("ContactGender", contactGender),
                new XElement("ContactMonitored", contactMonitored),
                new XElement("ContactGUID", contactGuid),
                new XElement("ContactLastModified", FormatDate(contactLastModified)),
                new XElement("ContactCreated", FormatDate(contactCreated)),
                new XElement("OM_Activity")
            )
        );

        return xml.ToString();
    }

    public bool DeleteContactByEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return false;
        }

        ContactInfo contact = GetContactByEmail(email.Trim());
        if (contact is null)
        {
            return false;
        }

        contactInfoProvider.Delete(contact);
        return true;
    }

    private ContactInfo GetContactByEmail(string email)
    {
        return contactInfoProvider
            .Get()
            .WhereEquals("ContactEmail", email)
            .OrderByDescending("ContactLastModified")
            .FirstOrDefault();
    }

    private static string FormatDate(DateTime dateTime)
    {
        if (dateTime == DateTime.MinValue)
        {
            return string.Empty;
        }

        return dateTime.ToString("yyyy-MM-ddTHH:mm:ss.fffffff", CultureInfo.InvariantCulture);
    }
}