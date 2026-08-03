using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using CMS.DataEngine;
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
    private readonly IInfoProvider<UserInfo> userInfoProvider;

    public GDPRUserService(IInfoProvider<UserInfo> userInfoProvider)
    {
        this.userInfoProvider = userInfoProvider;
    }

    public string GetContactAsText(string email)
    {
        string xml = FetchPersonalDataXml(email);
        if (string.IsNullOrWhiteSpace(xml))
            return $"There was no result returned for the given email {email}.";

        try
        {
            var root = XElement.Parse(xml);
            using var sw = new StringWriter();
            FormatRootToText(sw, root);
            return sw.ToString();
        }
        catch (System.Xml.XmlException)
        {
            return $"There was no result returned for the given email {email}.";
        }
    }

    public string GetContactAsXml(string email)
    {
        string xml = FetchPersonalDataXml(email);
        if (string.IsNullOrWhiteSpace(xml))
            return $"There was no result returned for the given email {email}.";

        try
        {
            return XElement.Parse(xml).ToString(SaveOptions.None);
        }
        catch (System.Xml.XmlException)
        {
            return $"There was no result returned for the given email {email}.";
        }
    }

    public bool DeleteContactByEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        // Confirm personal data exists before proceeding.
        if (string.IsNullOrWhiteSpace(FetchPersonalDataXml(email)) &&
            !userInfoProvider.Get().WhereEquals("Email", email).Any())
            return false;

        var users = userInfoProvider.Get().WhereEquals("Email", email);
        foreach (var user in users)
            userInfoProvider.Delete(user);

        // Sanitize always runs regardless of whether a CMS user existed.
        SanitizePersonalData(email);
        return true;
    }

    private static string FetchPersonalDataXml(string email)
    {
        var parameters = new QueryDataParameters
        {
            { "@email_address", email.Replace("'", "''") }
        };

        var sb = new StringBuilder();
        using var reader = ConnectionHelper.ExecuteReader(
            "dbo.BDO_GetPersonalData",
            parameters,
            QueryTypeEnum.StoredProcedure,
            CommandBehavior.Default);

        while (reader.Read())
            sb.Append(reader.GetString(0));

        return sb.ToString();
    }

    private static void SanitizePersonalData(string email)
    {
        var parameters = new QueryDataParameters
        {
            { "@email_address", email.Replace("'", "''") }
        };

        ConnectionHelper.ExecuteNonQuery(
            "dbo.BDO_GDPR_SanitizePersonalData",
            parameters,
            QueryTypeEnum.StoredProcedure);
    }

    private static void FormatRootToText(TextWriter dst, XElement root)
    {
        var activities = new List<XElement>();
        var forms = new List<XElement>();

        foreach (var el in root.Elements())
        {
            if (el.Name.LocalName.StartsWith("Form", System.StringComparison.OrdinalIgnoreCase))
                forms.Add(el);
            else
                activities.Add(el);
        }

        dst.WriteLine("Site Activity:");
        foreach (var activity in activities)
        {
            dst.WriteLine($"\t{FormatName(activity.Name.LocalName)}");
            FormatElement(dst, activity, indent: 2);
        }

        if (forms.Count > 0)
        {
            dst.WriteLine("Forms Completed:");
            foreach (var form in forms)
            {
                dst.WriteLine($"\t{FormatName(form.Name.LocalName)}:");
                FormatElement(dst, form, indent: 2);
                dst.WriteLine();
            }
        }
    }

    private static void FormatElement(TextWriter dst, XElement ele, int indent = 0)
    {
        string pfx = new string('\t', indent);

        foreach (XAttribute field in ele.Attributes())
            dst.WriteLine($"{pfx}{FormatName(field.Name.LocalName)}:  {field.Value}");

        foreach (var child in ele.Elements())
        {
            dst.Write($"{pfx}{FormatName(child.Name.LocalName)}:");
            if (child.HasElements || child.HasAttributes)
            {
                dst.WriteLine();
                FormatElement(dst, child, indent + 1);
            }
            if (!string.IsNullOrWhiteSpace(child.Value) && !child.HasElements)
                dst.WriteLine($"  {child.Value}");
            else
                dst.WriteLine();
        }
    }

    private static string FormatName(string oldName)
    {
        var sb = new StringBuilder();
        bool isCap = true;
        foreach (char c in oldName)
        {
            if (char.IsUpper(c))
            {
                if (isCap) sb.Append(c);
                else { sb.Append($" {c}"); isCap = true; }
            }
            else if (c == '_' || c == '-')
            {
                sb.Append(' ');
                isCap = true;
            }
            else
            {
                sb.Append(c);
                isCap = false;
            }
        }
        return sb.ToString();
    }
}