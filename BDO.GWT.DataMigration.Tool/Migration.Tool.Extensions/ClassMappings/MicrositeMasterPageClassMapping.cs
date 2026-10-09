using CMS.DataEngine;
using Microsoft.Extensions.DependencyInjection;
using Migration.Tool.Common.Builders;

namespace Migration.Tool.Extensions.ClassMappings;

public static class MicrositeMasterPageClassMapping
{
    private const string SourceClassName = "BDO.MicrositeMasterPage";

    public static IServiceCollection AddMicrositeMasterPageMapping(this IServiceCollection services)
    {
        var mapping = new MultiClassMapping(SourceClassName, target =>
        {
            target.ClassName = SourceClassName;
            target.ClassTableName = "BDO_MicrositeMasterPage";
            target.ClassDisplayName = "Microsite - Master Page";
            target.ClassType = ClassType.CONTENT_TYPE;
            target.ClassContentTypeType = ClassContentTypeType.WEBSITE;
            target.ClassWebPageHasUrl = true;
        });

        mapping.BuildField("MicrositeMasterPageID").AsPrimaryKey();
        mapping.MapPageMetadata(SourceClassName);

        // Coupled-table specific fields
        mapping.BuildField("SettingsMenuType").ConvertFrom(SourceClassName, "SettingsMenuType", false, Coalesce); // NOT NULL
        mapping.BuildField("LogoLinkTarget").SetFrom(SourceClassName, "LogoLinkTarget");
        mapping.BuildField("LogoText").SetFrom(SourceClassName, "LogoText");
        MapEmptyReference(mapping, "CustomJS");
        MapEmptyReference(mapping, "CustomCSS");
        mapping.BuildField("HeadHTML").SetFrom(SourceClassName, "HeadHTML");
        mapping.BuildField("BodyTopHTML").SetFrom(SourceClassName, "BodyTopHTML");
        mapping.BuildField("BodyBottomHTML").SetFrom(SourceClassName, "BodyBottomHTML");

        // Shared/common schema fields
        mapping.BuildField("MetadataTitle").SetFrom(SourceClassName, "MetadataTitle");
        mapping.BuildField("MetadataDescription").SetFrom(SourceClassName, "MetadataDescription");
        MapEmptyReference(mapping, "MetadataTeaserImage");
        mapping.BuildField("MetadataTeaserImageAltText").SetFrom(SourceClassName, "MetadataTeaserImageAltText");
        mapping.BuildField("MetadataCanonical").SetFrom(SourceClassName, "MetadataCanonical");
        mapping.BuildField("MetadataOGTitle").SetFrom(SourceClassName, "MetadataOGTitle");
        mapping.BuildField("MetadataOGDescription").SetFrom(SourceClassName, "MetadataOGDescription");
        MapEmptyReference(mapping, "MetadataOGImage");
        mapping.BuildField("MetadataOGTwitterImageSizeCheckbox").SetFrom(SourceClassName, "MetadataOGTwitterImageSizeCheckbox");
        MapEmptyReference(mapping, "MetadataOGTwitterImage");
        mapping.BuildField("MetadataNofollow").SetFrom(SourceClassName, "MetadataNofollow");
        mapping.BuildField("MetadataNoindex").SetFrom(SourceClassName, "MetadataNoindex");
        mapping.BuildField("IncludeInSitemap").SetFrom(SourceClassName, "IncludeInSitemap");

        services.AddSingleton<IClassMapping>(mapping);
        return services;
    }

    private static object? Coalesce(object? value, IConvertorContext context) => value ?? "";

    private static void MapEmptyReference(MultiClassMapping mapping, string targetFieldName) =>
        mapping.BuildField(targetFieldName).ConvertFrom(SourceClassName, "MicrositeMasterPageID", false, static (_, _) => "[]");
}
