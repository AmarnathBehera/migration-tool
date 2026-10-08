using Migration.Tool.Common.Builders;

namespace Migration.Tool.Extensions.ClassMappings;

internal static class PageMetadataMapping
{
    private const int MaxLength = 200;

    // PageTitle/PageDescription/PageKeywords are nvarchar(200) in CMS_ContentItemCommonData; source values can be longer.
    public static void MapPageMetadata(this MultiClassMapping mapping, string sourceClassName)
    {
        mapping.BuildField("PageTitle").ConvertFrom(sourceClassName, "DocumentPageTitle", false, Truncate);
        mapping.BuildField("PageDescription").ConvertFrom(sourceClassName, "DocumentPageDescription", false, Truncate);
        mapping.BuildField("PageKeywords").ConvertFrom(sourceClassName, "DocumentPageKeyWords", false, Truncate);
    }

    private static object? Truncate(object? value, IConvertorContext context) =>
        value is string s && s.Length > MaxLength ? s[..MaxLength] : value;
}
