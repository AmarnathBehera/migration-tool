using BDO.GWT.Core.Data;
using System.Text;

namespace BDO.GWT.Core.Helpers;

public static class VCardHelper
{
    public static string GenerateVCardString(VCardData vCard)
    {
        var builder = new StringBuilder();

        builder.AppendLine("BEGIN:VCARD");

        var nameParts = (vCard.FullName ?? "").Split(' ', 2);
        var firstName = nameParts.Length > 1 ? nameParts[0] : "";
        var lastName = nameParts.Length > 1 ? nameParts[1] : nameParts[0];
        var nLine = $"{lastName};{firstName};;;";
        builder.AppendLine("VERSION:3.0");
        AppendSimpleProperty(builder,"N",nLine, "CHARSET=utf-8");
        AppendSimpleProperty(builder, "FN", vCard.FullName, "CHARSET=utf-8");
        AppendSimpleProperty(builder, "CATEGORIES", vCard.Categories is null ? null : string.Join(',', vCard.Categories), "CHARSET=utf-8");
        AppendSimpleProperty(builder, "EMAIL", vCard.EMail);
        AppendSimpleProperty(builder, "SOURCE", vCard.Source);
        AppendSimpleProperty(builder, "ORG", vCard.Organization, "CHARSET=utf-8");
        AppendSimpleProperty(builder, "TITLE", vCard.Title, "CHARSET=utf-8");
        AppendSimpleProperty(builder, "URL", vCard.URL);
        AppendSimpleProperty(builder, "REV", vCard.RevisionDate?.ToString("yyyyMMddTHHmmssZ"));
        AppendLocationProperty(builder, vCard.Locations);
        AppendTelephoneNumbers(builder, vCard.TelephoneNumbers);
        AppendImageProperty(builder, "LOGO", vCard.LogoFormat, vCard.LogoBase64);
        AppendImageProperty(builder, "PHOTO", vCard.PhotoFormat, vCard.PhotoBase64);
        builder.AppendLine("END:VCARD");

        return builder.ToString();
    }

    private static void AppendSimpleProperty(StringBuilder builder, string property, string? value, params string[] parameters)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }
        
        var joinedParameters = parameters.Length > 0 ? $";{string.Join(';', parameters)}" : string.Empty;
        builder.AppendLine($"{property}{joinedParameters}:{value}");
    }

    private static void AppendTelephoneNumbers(StringBuilder builder, IEnumerable<(string?, string?)>? telephoneNumbers)
    {
        if (telephoneNumbers is null)
        {
            return;
        }

        foreach (var (type, number) in telephoneNumbers)
        {
            if (!string.IsNullOrWhiteSpace(type) && !string.IsNullOrWhiteSpace(number))
            {
                builder.AppendLine($"TEL;TYPE={type}:{number}");
            }
        }
    }

    private static void AppendImageProperty(StringBuilder builder, string property, string? imageFormat, string? imageBase64)
    {
        if (!string.IsNullOrWhiteSpace(imageFormat) && !string.IsNullOrWhiteSpace(imageBase64))
        {
            builder.AppendLine($"{property};TYPE=image/{imageFormat.ToUpperInvariant()}:{imageBase64}");
        }
    }

    private static void AppendLocationProperty(StringBuilder builder, IEnumerable<(string locationType, VCardLocationData locationData)>? vCardLocations)
    {
        if (vCardLocations is null)
        {
            return;
        }

        foreach (var (locationType, locationData) in vCardLocations)
        {
            AppendSimpleProperty(builder, "ADR", $";{locationData.SecondAddress};{locationData.Address};{locationData.City};{locationData.State};{locationData.Zip};{locationData.Country}", "CHARSET=utf-8", $"TYPE={locationType}");
            AppendSimpleProperty(builder, "LABEL", $"{locationData.Address}=0D=0A{locationData.SecondAddress}=0D=0A{locationData.City}=0D=0A{locationData.State}=0D=0A{locationData.Zip}", "CHARSET=utf-8", $"TYPE={locationType}", "ENCODING=QUOTED-PRINTABLE");
        }
    }
}
