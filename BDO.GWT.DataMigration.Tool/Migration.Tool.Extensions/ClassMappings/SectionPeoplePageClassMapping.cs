using CMS.DataEngine;
using Microsoft.Extensions.DependencyInjection;
using Migration.Tool.Common.Builders;

namespace Migration.Tool.Extensions.ClassMappings;

public static class SectionPeoplePageClassMapping
{
    private const string SourceClassName = "BDO.SectionPeoplePage";

    public static IServiceCollection AddSectionPeoplePageMapping(this IServiceCollection services)
    {
        var mapping = new MultiClassMapping(SourceClassName, target =>
        {
            target.ClassName = SourceClassName;
            target.ClassTableName = "BDO_SectionPeoplePage";
            target.ClassDisplayName = "Section People Page";
            target.ClassType = ClassType.CONTENT_TYPE;
            target.ClassContentTypeType = ClassContentTypeType.WEBSITE;
            target.ClassWebPageHasUrl = true;
        });

        mapping.BuildField("SectionPeoplePageID").AsPrimaryKey();
        mapping.MapPageMetadata(SourceClassName);
        mapping.BuildField("PeopleListFilteringEnabled").SetFrom(SourceClassName, "PeopleListFilteringEnabled");
        mapping.BuildField("PeopleListFilters").SetFrom(SourceClassName, "PeopleListFilters");
        mapping.BuildField("PeopleListOrderingEnabled").SetFrom(SourceClassName, "PeopleListOrderingEnabled");
        mapping.BuildField("PeopleListDefaultOrder").SetFrom(SourceClassName, "PeopleListDefaultOrder");
        mapping.BuildField("EnableForm").ConvertFrom(SourceClassName, "EnableForm", false, static (value, _) => value is bool enabled && enabled);
        mapping.BuildField("DisplayEmailAddressInPersonPage").ConvertFrom(SourceClassName, "DisplayEmailAddressInPersonPage", false, static (value, _) => value is bool enabled && enabled);
        mapping.BuildField("DisplayFullEmailAddress").ConvertFrom(SourceClassName, "DisplayFullEmailAddress", false, static (value, _) => value is bool enabled && enabled);

        // Remaining fields are shared/common schema fields (CMS_ContentItemCommonData), not coupled-table specific
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
        mapping.BuildField("ShowAuthoredInsights").SetFrom(SourceClassName, "ShowAuthoredInsights");
        mapping.BuildField("AuthoredInsightsTitle").SetFrom(SourceClassName, "AuthoredInsightsTitle");
        mapping.BuildField("AuthoredInsightsDescription").SetFrom(SourceClassName, "AuthoredInsightsDescription");
        MapEmptyReference(mapping, "AuthoredInsightsBackgroundImageUrl");
        mapping.BuildField("DisplayedAuthoredContentPageTypes").SetFrom(SourceClassName, "DisplayedAuthoredContentPageTypes");
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
        mapping.BuildField(targetFieldName).ConvertFrom(SourceClassName, "SectionPeoplePageID", false, static (_, _) => "[]");
}
