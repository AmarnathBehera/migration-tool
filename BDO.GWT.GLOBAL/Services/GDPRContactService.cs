using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Membership;

namespace BDO.GWT.GLOBAL.Services;

public interface IGDPRUserLookupService
{
    string GetContactAsText(string email);
    string GetContactAsXml(string email);
    bool DeleteContactByEmail(string email);
}

public sealed class GDPRUserService : IGDPRUserLookupService
{
    private readonly IInfoProvider<UserInfo> UserInfoProvider;

    public GDPRUserService(IInfoProvider<UserInfo> UserInfoProvider)
    {
        this.UserInfoProvider = UserInfoProvider;
    }

    public string GetContactAsText(string email)
    {
        UserInfo user = GetUserByEmail(email);
        if (user is null)
        {
            return $"No OM Contact found for email: {email}";
        }

        int userId = ValidationHelper.GetInteger(user.GetValue("UserID"), 0);
        string userLastName = ValidationHelper.GetString(user.GetValue("LastName"), string.Empty);
        string userEmail = ValidationHelper.GetString(user.GetValue("UserName"), string.Empty);
        int userEnabled = ValidationHelper.GetBoolean(user.GetValue("UserEnabled"), false) ? 1 : 0;
        Guid userGuid = ValidationHelper.GetGuid(user.GetValue("UserGUID"), Guid.Empty);
        DateTime userLastModified = ValidationHelper.GetDateTime(user.GetValue("UserLastModified"), DateTime.MinValue);
        DateTime userCreated = ValidationHelper.GetDateTime(user.GetValue("UserCreated"), DateTime.MinValue);

        var builder = new StringBuilder();
        builder.AppendLine("Site Activity:");
        builder.AppendLine("\tOM Contact");
        builder.AppendLine($"\t\tContact ID:\t{userId}");
        builder.AppendLine($"\t\tContact Last Name:\t{userLastName}");
        builder.AppendLine($"\t\tContact Email:\t{userEmail}");
        builder.AppendLine($"\t\tContact Gender:\t{0}");
        builder.AppendLine($"\t\tContact Monitored:\t{userEnabled}");
        builder.AppendLine($"\t\tContact GUID:\t{userGuid.ToString().ToUpperInvariant()}");
        builder.AppendLine($"\t\tContact Last Modified:\t{FormatDate(userLastModified)}");
        builder.AppendLine($"\t\tContact Created:\t{FormatDate(userCreated)}");
        builder.AppendLine("\t\tOM Activity:");

        return builder.ToString().TrimEnd();
    }

    public string GetContactAsXml(string email)
    {
        UserInfo user = GetUserByEmail(email);
        if (user is null)
        {
            return "<XML>\n  <OM_Contact />\n</XML>";
        }

        int userId = ValidationHelper.GetInteger(user.GetValue("UserID"), 0);
        string userLastName = ValidationHelper.GetString(user.GetValue("LastName"), string.Empty);
        string userEmail = ValidationHelper.GetString(user.GetValue("UserName"), string.Empty);
        int userEnabled = ValidationHelper.GetBoolean(user.GetValue("UserEnabled"), false) ? 1 : 0;
        string userGuid = ValidationHelper.GetGuid(user.GetValue("UserGUID"), Guid.Empty).ToString().ToUpperInvariant();
        DateTime userLastModified = ValidationHelper.GetDateTime(user.GetValue("UserModifiedWhen"), DateTime.MinValue);
        DateTime userCreated = ValidationHelper.GetDateTime(user.GetValue("UserCreated"), DateTime.MinValue);

        XElement xml = new(
            "XML",
            new XElement(
                "OM_Contact",
                new XElement("ContactID", userId),
                new XElement("ContactLastName", userLastName),
                new XElement("ContactEmail", userEmail),
                new XElement("ContactGender", 0),
                new XElement("ContactMonitored", userEnabled),
                new XElement("ContactGUID", userGuid),
                new XElement("ContactLastModified", FormatDate(userLastModified)),
                new XElement("ContactCreated", FormatDate(userCreated)),
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

        UserInfo user = GetUserByEmail(email.Trim());
        if (user is null)
        {
            return false;
        }

        UserInfoProvider.Delete(user);
        return true;
    }

    private UserInfo GetUserByEmail(string email)
    {
        return UserInfoProvider
            .Get()
            .WhereEquals("UserName", email)
            .OrderByDescending("UserLastModified")
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