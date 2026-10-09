using CMS.DataEngine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Data.SqlClient;
using Migration.Tool.Common.Builders;
using Migration.Tool.Source;
using Migration.Tool.Source.Model;

namespace Migration.Tool.Extensions.ClassMappings;

public static class RemoteDealClassMapping
{
    private const string SourceClassName = "BDO.RemoteDeal";

    public static IServiceCollection AddRemoteDealMapping(this IServiceCollection services)
    {
        var mapping = new MultiClassMapping(SourceClassName, target =>
        {
            target.ClassName = SourceClassName;
            target.ClassTableName = "BDO_RemoteDeal";
            target.ClassDisplayName = "Remote Deal";
            target.ClassType = ClassType.CONTENT_TYPE;
            target.ClassContentTypeType = ClassContentTypeType.WEBSITE;
        });

        mapping.BuildField("RemoteDealID").AsPrimaryKey();

        // Reference/asset fields — source stores raw codenames/paths that are not valid target references
        MapEmptyReference(mapping, "Industry");
        MapEmptyReference(mapping, "SubIndustry");
        MapEmptyReference(mapping, "DealType");
        MapEmptyReference(mapping, "MetadataTeaserImage");

        // Remaining scalar fields (DateField, DealCountry, Original*, ForceUpdateOnNextSync,
        // MetadataTitle, MetadataDescription, MetadataTeaserImageAltText, IncludeInSitemap) pass through by same name.

        services.AddSingleton<IClassMapping>(provider =>
        {
            var modelFacade = provider.GetRequiredService<ModelFacade>();
            const string dealClassName = "BDO.Deal";
            mapping.BuildField("RemoteDealID").SetFrom(dealClassName, "DealID");
            foreach (var fieldName in new[] { "DateField", "DealCountry", "MetadataTitle", "MetadataDescription", "MetadataTeaserImageAltText", "IncludeInSitemap" })
            {
                mapping.BuildField(fieldName).SetFrom(dealClassName, fieldName);
            }
            mapping.BuildField("OriginalPageGUID").ConvertFrom(dealClassName, "DealID", false,
                static (_, context) => context is ConvertorTreeNodeContext node ? node.NodeGuid : null);
            mapping.BuildField("OriginalSiteGUID").ConvertFrom(dealClassName, "DealID", false,
                (_, context) => context is ConvertorTreeNodeContext node ? modelFacade.SelectById<ICmsSite>(node.NodeSiteId)?.SiteGUID : null);
            mapping.BuildField("OriginalPageURL").ConvertFrom(dealClassName, "DealID", false,
                (_, context) => context is ConvertorTreeNodeContext node
                    ? modelFacade.SelectWhere<ICmsTree>("NodeGUID=@guid AND NodeSiteID=@site", new SqlParameter("guid", node.NodeGuid), new SqlParameter("site", node.NodeSiteId)).FirstOrDefault()?.NodeAliasPath
                    : null);
            mapping.BuildField("ForceUpdateOnNextSync").ConvertFrom(dealClassName, "DealID", false, static (_, _) => false);
            return mapping;
        });

        return services;
    }

    private static void MapEmptyReference(MultiClassMapping mapping, string targetFieldName) =>
        mapping.BuildField(targetFieldName)
            .ConvertFrom(SourceClassName, "RemoteDealID", false, static (_, _) => "[]")
            .ConvertFrom("BDO.Deal", "DealID", false, static (_, _) => "[]");
}
