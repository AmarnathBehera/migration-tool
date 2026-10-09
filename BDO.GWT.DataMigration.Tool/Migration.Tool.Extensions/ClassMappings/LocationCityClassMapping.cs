using CMS.DataEngine;
using Microsoft.Extensions.DependencyInjection;
using Migration.Tool.Common.Builders;

namespace Migration.Tool.Extensions.ClassMappings;

public static class LocationCityClassMapping
{
    private const string SourceClassName = "BDO.LocationCity";

    public static IServiceCollection AddLocationCityMapping(this IServiceCollection services)
    {
        var mapping = new MultiClassMapping(SourceClassName, target =>
        {
            target.ClassName = SourceClassName;
            target.ClassTableName = "BDO_LocationCity";
            target.ClassDisplayName = "Location City";
            target.ClassType = ClassType.CONTENT_TYPE;
            target.ClassContentTypeType = ClassContentTypeType.WEBSITE;
            target.ClassWebPageHasUrl = true;
        });

        mapping.BuildField("LocationCityID").AsPrimaryKey();
        mapping.MapPageMetadata(SourceClassName);

        // Coupled-table specific fields
        MapEmptyReference(mapping, "RegionNameLocationCity");
        mapping.BuildField("ContentTitle").SetFrom(SourceClassName, "ContentTitle");
        mapping.BuildField("SectionTitle").ConvertFrom(SourceClassName, "SectionTitle", false, Coalesce); // NOT NULL

        // Shared/common schema fields
        mapping.BuildField("MetadataTitle").SetFrom(SourceClassName, "MetadataTitle");
        mapping.BuildField("MetadataDescription").SetFrom(SourceClassName, "MetadataDescription");
        MapEmptyReference(mapping, "MetadataTeaserImage");
        mapping.BuildField("MetadataTeaserImageAltText").SetFrom(SourceClassName, "MetadataTeaserImageAltText");
        mapping.BuildField("MetadataCanonical").SetFrom(SourceClassName, "MetadataCanonical");
        mapping.BuildField("HeroLayout").SetFrom(SourceClassName, "HeroLayout");
        mapping.BuildField("HeroTitle").SetFrom(SourceClassName, "HeroTitle");
        mapping.BuildField("HeroDescription").SetFrom(SourceClassName, "HeroDescription");
        MapEmptyReference(mapping, "HeroBackgroundImage");
        mapping.BuildField("HeroBackgroundImageAltText").SetFrom(SourceClassName, "HeroBackgroundImageAltText");
        MapEmptyReference(mapping, "HeroBackgroundImage_Mobile");
        mapping.BuildField("HeroBackgroundImage_MobileAltText").SetFrom(SourceClassName, "HeroBackgroundImage_MobileAltText");
        mapping.BuildField("HeroCallToActionButtonType").SetFrom(SourceClassName, "HeroCallToActionButtonType");
        mapping.BuildField("HeroCallToActionButtonText").SetFrom(SourceClassName, "HeroCallToActionButtonText");
        MapEmptyReference(mapping, "HeroCallToActionButtonUrl");
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

    private static object? Coalesce(object? value, Migration.Tool.Common.Builders.IConvertorContext context) => value ?? "";

    private static void MapEmptyReference(MultiClassMapping mapping, string targetFieldName) =>
        mapping.BuildField(targetFieldName).ConvertFrom(SourceClassName, "LocationCityID", false, static (_, _) => "[]");
}
