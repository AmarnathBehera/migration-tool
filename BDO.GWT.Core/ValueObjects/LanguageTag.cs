using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.Json.Serialization;

namespace BDO.GWT.Core.ValueObjects;

[method: JsonConstructor]
public record LanguageTag(string Language, string? Script, string Region)
{
    public static readonly LanguageTag AU = new("en", "AU");
    public static readonly LanguageTag DE = new("de", "DE");
    public static readonly LanguageTag GB = new("en", "GB");
    public static readonly LanguageTag LT = new("lt", "LT");
    public static readonly LanguageTag US = new("en", "US");

    public LanguageTag(string Language, string Region) : this(Language, null, Region)
    {
    }

    public static LanguageTag Parse(string languageTag)
    {
        if (TryParse(languageTag, out var culture))
        {
            return culture;
        }

        throw new ArgumentException($"Incorrect language tag '{languageTag}' format.");
    }

    public static bool TryParse(string languageTag, [NotNullWhen(true)] out LanguageTag? culture)
    {
        culture = null;

        var parts = languageTag.Split('-');

        if (parts.Length == 2)
        {
            if ((IsTwoLetterAlpha(parts[0]) || IsThreeLetterAlpha(parts[0])) && (IsTwoLetterAlpha(parts[1]) || IsThreeDigitNumeric(parts[1])))
            {
                var language = parts[0].ToLowerInvariant();
                var region = parts[1].ToUpperInvariant();

                culture = new LanguageTag(language, region);

                return true;
            }
        }
        else if (parts.Length == 3)
        {
            if ((IsTwoLetterAlpha(parts[0]) || IsThreeLetterAlpha(parts[0])) && IsFourLetterAlpha(parts[1]) && (IsTwoLetterAlpha(parts[2]) || IsThreeDigitNumeric(parts[2])))
            {
                var language = parts[0].ToLowerInvariant();
                var script = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(parts[1]);
                var region = parts[2].ToUpperInvariant();

                culture = new LanguageTag(language, script, region);

                return true;
            }
        }

        return false;
    }

    public override string ToString()
    {
        return $"{Language}{(Script is not null ? "-" + Script : "")}-{Region}";
    }

    private static bool IsTwoLetterAlpha(string str)
    {
        return str.Length == 2 && char.IsLetter(str[0]) && char.IsLetter(str[1]);
    }

    private static bool IsThreeDigitNumeric(string str)
    {
        return str.Length == 3 && char.IsDigit(str[0]) && char.IsDigit(str[1]) && char.IsDigit(str[2]);
    }

    private static bool IsThreeLetterAlpha(string str)
    {
        return str.Length == 3 && char.IsLetter(str[0]) && char.IsLetter(str[1]) && char.IsLetter(str[2]);
    }

    private static bool IsFourLetterAlpha(string str)
    {
        return str.Length == 4 && char.IsLetter(str[0]) && char.IsLetter(str[1]) && char.IsLetter(str[2]) && char.IsLetter(str[3]);
    }
}
