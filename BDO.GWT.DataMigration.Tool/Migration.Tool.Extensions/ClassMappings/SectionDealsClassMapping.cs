using CMS.DataEngine;
using Microsoft.Extensions.DependencyInjection;
using Migration.Tool.Common.Builders;

namespace Migration.Tool.Extensions.ClassMappings;

public static class SectionDealsClassMapping
{
    private const string SourceClassName = "BDO.SectionDeals";

    public static IServiceCollection AddSectionDealsMapping(this IServiceCollection services)
    {
        var mapping = new MultiClassMapping(SourceClassName, target =>
        {
            target.ClassName = SourceClassName;
            target.ClassTableName = "BDO_SectionDeals";
            target.ClassDisplayName = "Section Deals";
            target.ClassType = ClassType.CONTENT_TYPE;
            target.ClassContentTypeType = ClassContentTypeType.WEBSITE;
            target.ClassWebPageHasUrl = true;
        });

        mapping.BuildField("SectionDealsID").AsPrimaryKey();

        // Target-only NOT NULL field with no source equivalent
        mapping.BuildField("IsGlobal").ConvertFrom(SourceClassName, "SectionDealsID", false, static (_, _) => false);

        // Asset/reference fields — source stores raw paths that are not valid target references
        MapEmptyReference(mapping, "MetadataTeaserImage");
        MapEmptyReference(mapping, "MetadataOGImage");
        MapEmptyReference(mapping, "MetadataOGTwitterImage");

        // Remaining scalar fields (SectionDealsFilteringEnabled, SectionDealsFilters, ShowCopyUrlButtonInDealPage,
        // Metadata* text/bool, IncludeInSitemap) pass through by same name.

        services.AddSingleton<IClassMapping>(mapping);

        return services;
    }

    private static void MapEmptyReference(MultiClassMapping mapping, string targetFieldName) =>
        mapping.BuildField(targetFieldName).ConvertFrom(SourceClassName, "SectionDealsID", false, static (_, _) => "[]");
}
