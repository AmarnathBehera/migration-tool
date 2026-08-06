using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace BDO.GWT.Domain.Core.Utilities;

public static class RegexUtilities
{
    public static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return false;
        }

        try
        {
            email = Regex.Replace(email, "(@)(.+)$", DomainMapper, RegexOptions.None, TimeSpan.FromMilliseconds(200));

            return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));

            string DomainMapper(Match match)
            {
                var idn = new IdnMapping();

                var domainName = idn.GetAscii(match.Groups[2].Value);

                return match.Groups[1].Value + domainName;
            }
        }
        catch
        {
            return false;
        }
    }
}
