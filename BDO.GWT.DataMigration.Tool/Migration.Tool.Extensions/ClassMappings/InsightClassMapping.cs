using CMS.DataEngine;
using Microsoft.Extensions.DependencyInjection;
using Migration.Tool.Common.Builders;

namespace Migration.Tool.Extensions.ClassMappings;

public static class InsightClassMapping
{
    private const string SourceClassName = "BDO.Insight";

    public static IServiceCollection AddInsightMapping(this IServiceCollection services)
    {
        var mapping = new MultiClassMapping(SourceClassName, target =>
        {
            target.ClassName = SourceClassName;
            target.ClassTableName = "BDO_Insight";
            target.ClassDisplayName = "Insight";
            target.ClassType = ClassType.CONTENT_TYPE;
            target.ClassContentTypeType = ClassContentTypeType.WEBSITE;
            target.ClassWebPageHasUrl = true;
        });

        mapping.BuildField("InsightID").AsPrimaryKey();

        mapping.MapPageMetadata(SourceClassName);

        // Coupled-table specific scalar fields
        mapping.BuildField("ComingSoon").SetFrom(SourceClassName, "ComingSoon");
        mapping.BuildField("UseNarrowLayout").SetFrom(SourceClassName, "UseNarrowLayout");
        mapping.BuildField("ShowAuthorsAtTheTop").SetFrom(SourceClassName, "ShowAuthorsAtTheTop");

        // Coupled-table specific reference fields
        MapEmptyReference(mapping, "InsightContent");
        MapEmptyReference(mapping, "MetadataBusinessIssues");
        MapEmptyReference(mapping, "MetadataSpecialtiesCategories");
        MapEmptyReference(mapping, "MetadataSpecialtiesAreas");
        MapEmptyReference(mapping, "MetadataSpecialtiesPages");
        MapEmptyReference(mapping, "MetadataOfficeLocation");
        MapEmptyReference(mapping, "Insights");

        // Shared/common schema fields (CMS_ContentItemCommonData)
        mapping.BuildField("ShowSidebar").SetFrom(SourceClassName, "ShowSidebar");
        mapping.BuildField("ShowPrintButton").SetFrom(SourceClassName, "ShowPrintButton");
        MapEmptyReference(mapping, "DownloadButtonAttachment");
        mapping.BuildField("ShowSocialMediaButtons").SetFrom(SourceClassName, "ShowSocialMediaButtons");
        mapping.BuildField("MetadataOGTitle").SetFrom(SourceClassName, "MetadataOGTitle");
        mapping.BuildField("MetadataOGDescription").SetFrom(SourceClassName, "MetadataOGDescription");
        MapEmptyReference(mapping, "MetadataOGImage");
        mapping.BuildField("MetadataOGTwitterImageSizeCheckbox").SetFrom(SourceClassName, "MetadataOGTwitterImageSizeCheckbox");
        MapEmptyReference(mapping, "MetadataOGTwitterImage");
        mapping.BuildField("MetadataNofollow").SetFrom(SourceClassName, "MetadataNofollow");
        mapping.BuildField("MetadataNoindex").SetFrom(SourceClassName, "MetadataNoindex");
        mapping.BuildField("IncludeInSitemap").SetFrom(SourceClassName, "IncludeInSitemap");
        mapping.BuildField("IsSearchExcluded").SetFrom(SourceClassName, "IsSearchExcluded");
        mapping.BuildField("MetadataTitle").SetFrom(SourceClassName, "MetadataTitle");
        mapping.BuildField("MetadataDescription").SetFrom(SourceClassName, "MetadataDescription");
        MapEmptyReference(mapping, "MetadataTeaserImage");
        mapping.BuildField("MetadataTeaserImageAltText").SetFrom(SourceClassName, "MetadataTeaserImageAltText");
        mapping.BuildField("MetadataCanonical").SetFrom(SourceClassName, "MetadataCanonical");
        MapEmptyReference(mapping, "ContentType");
        MapEmptyReference(mapping, "MetadataBusinessLine");
        MapEmptyReference(mapping, "MetadataServiceAreas");
        MapEmptyReference(mapping, "MetadataServices");
        MapEmptyReference(mapping, "MetadataIndustryCategories");
        MapEmptyReference(mapping, "MetadataIndustries");
        MapEmptyReference(mapping, "MicrositePages");
        MapEmptyReference(mapping, "MicrositeMasterPages");
        MapEmptyReference(mapping, "MetadataRssCategory");
        MapEmptyReference(mapping, "MetadataRssImage");
        MapEmptyReference(mapping, "RelevantContactPerson");
        MapEmptyReference(mapping, "RelatedExternalPeople");

        services.AddSingleton<IClassMapping>(mapping);

        return services;
    }

    private static void MapEmptyReference(MultiClassMapping mapping, string targetFieldName) =>
        mapping.BuildField(targetFieldName).ConvertFrom(SourceClassName, "InsightID", false, static (_, _) => "[]");
}
