using CMS.DataEngine;
using Microsoft.Extensions.DependencyInjection;
using Migration.Tool.Common.Builders;

namespace Migration.Tool.Extensions.ClassMappings;

public static class MicrositePageClassMapping
{
    private const string SourceClassName = "BDO.MicrositePage";

    public static IServiceCollection AddMicrositePageMapping(this IServiceCollection services)
    {
        var mapping = new MultiClassMapping(SourceClassName, target =>
        {
            target.ClassName = SourceClassName;
            target.ClassTableName = "BDO_MicrositePage";
            target.ClassDisplayName = "Microsite - Page";
            target.ClassType = ClassType.CONTENT_TYPE;
            target.ClassContentTypeType = ClassContentTypeType.WEBSITE;
            target.ClassWebPageHasUrl = true;
        });

        mapping.BuildField("MicrositePageID").AsPrimaryKey();
        mapping.MapPageMetadata(SourceClassName);

        // Coupled-table specific scalar fields
        mapping.BuildField("Image").SetFrom(SourceClassName, "Image");
        mapping.BuildField("MicrositeImageAltText").ConvertFrom(SourceClassName, "MicrositeImageAltText", false, Coalesce); // NOT NULL
        mapping.BuildField("PageHeadHTML").SetFrom(SourceClassName, "PageHeadHTML");
        mapping.BuildField("PageTopBodyHTML").SetFrom(SourceClassName, "PageTopBodyHTML");
        mapping.BuildField("PageBottomBodyHTML").SetFrom(SourceClassName, "PageBottomBodyHTML");

        // Page reference fields - populated post-migration by seed-microsite-references.sql
        // (source stores CMS_Tree NodeIDs / metadata module IDs that need target GUID resolution)
        MapEmptyReference(mapping, "MicrositeImage");
        MapEmptyReference(mapping, "Homepage");
        MapEmptyReference(mapping, "MicrositeContentType");
        MapEmptyReference(mapping, "BusinessLines");
        MapEmptyReference(mapping, "ServiceAreas");
        MapEmptyReference(mapping, "Services");
        MapEmptyReference(mapping, "IndustryCategories");
        MapEmptyReference(mapping, "Industries");
        MapEmptyReference(mapping, "SpecialitiesCategories");
        MapEmptyReference(mapping, "SpecialitiesAreas");
        MapEmptyReference(mapping, "SpecialitiesPages");

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
        mapping.BuildField(targetFieldName).ConvertFrom(SourceClassName, "MicrositePageID", false, static (_, _) => "[]");
}
