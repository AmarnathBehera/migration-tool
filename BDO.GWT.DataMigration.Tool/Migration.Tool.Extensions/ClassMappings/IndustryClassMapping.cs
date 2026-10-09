using CMS.DataEngine;
using Microsoft.Extensions.DependencyInjection;
using Migration.Tool.Common.Builders;

namespace Migration.Tool.Extensions.ClassMappings;

public static class IndustryClassMapping
{
    private const string SourceClassName = "BDO.Industry";

    public static IServiceCollection AddIndustryMapping(this IServiceCollection services)
    {
        var mapping = new MultiClassMapping(SourceClassName, target =>
        {
            target.ClassName = SourceClassName;
            target.ClassTableName = "BDO_Industry";
            target.ClassDisplayName = "Industry";
            target.ClassType = ClassType.CONTENT_TYPE;
            target.ClassContentTypeType = ClassContentTypeType.WEBSITE;
            target.ClassWebPageHasUrl = true;
        });

        mapping.BuildField("IndustryID").AsPrimaryKey();
        mapping.MapPageMetadata(SourceClassName);

        // Coupled-table specific fields
        MapEmptyReference(mapping, "TileIcon");
        mapping.BuildField("DescriptionTitle").SetFrom(SourceClassName, "DescriptionTitle");
        mapping.BuildField("DescriptionBody").SetFrom(SourceClassName, "DescriptionBody");
        mapping.BuildField("ShowSideNavigation").SetFrom(SourceClassName, "ShowSideNavigation");
        mapping.BuildField("SideboxTitle").SetFrom(SourceClassName, "SideboxTitle");

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

    private static void MapEmptyReference(MultiClassMapping mapping, string targetFieldName) =>
        mapping.BuildField(targetFieldName).ConvertFrom(SourceClassName, "IndustryID", false, static (_, _) => "[]");
}
