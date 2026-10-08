using CMS.DataEngine;
using Microsoft.Extensions.DependencyInjection;
using Migration.Tool.Common.Builders;

namespace Migration.Tool.Extensions.ClassMappings;

public static class SectionInsightsPageClassMapping
{
    private const string SourceClassName = "BDO.SectionInsightsPage";

    public static IServiceCollection AddSectionInsightsPageMapping(this IServiceCollection services)
    {
        var mapping = new MultiClassMapping(SourceClassName, target =>
        {
            target.ClassName = SourceClassName;
            target.ClassTableName = "BDO_SectionInsightsPage";
            target.ClassDisplayName = "Section Insights Page";
            target.ClassType = ClassType.CONTENT_TYPE;
            target.ClassContentTypeType = ClassContentTypeType.WEBSITE;
            target.ClassWebPageHasUrl = true;
        });

        mapping.BuildField("SectionInsightsPageID").AsPrimaryKey();

        mapping.MapPageMetadata(SourceClassName);

        // Coupled-table specific fields
        mapping.BuildField("InsightListFilters").SetFrom(SourceClassName, "InsightListFilters");
        mapping.BuildField("ShowCopyUrlButtonInInsightPage").SetFrom(SourceClassName, "ShowCopyUrlButtonInInsightPage");
        MapEmptyReference(mapping, "Descendantpages");
        MapEmptyReference(mapping, "InsightListFeaturedItems");

        // Shared/common schema fields (CMS_ContentItemCommonData)
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
        mapping.BuildField("ItemListTitle").SetFrom(SourceClassName, "ItemListTitle");
        mapping.BuildField("ItemListDescription").SetFrom(SourceClassName, "ItemListDescription");
        mapping.BuildField("ItemListFilteringEnabled").SetFrom(SourceClassName, "ItemListFilteringEnabled");
        mapping.BuildField("ItemListAdvancedSearchDescription").SetFrom(SourceClassName, "ItemListAdvancedSearchDescription");
        mapping.BuildField("ItemListAdvancedSearchLinkText").SetFrom(SourceClassName, "ItemListAdvancedSearchLinkText");
        MapEmptyReference(mapping, "ItemListAdvancedSearchLinkUrl");
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
        mapping.BuildField(targetFieldName).ConvertFrom(SourceClassName, "SectionInsightsPageID", false, static (_, _) => "[]");
}
