using CMS.DataEngine;
using Microsoft.Extensions.DependencyInjection;
using Migration.Tool.Common.Builders;

namespace Migration.Tool.Extensions.ClassMappings;

public static class ContactClassMapping
{
    private const string SourceClassName = "BDO.Contact";

    public static IServiceCollection AddContactMapping(this IServiceCollection services)
    {
        var mapping = new MultiClassMapping(SourceClassName, target =>
        {
            target.ClassName = SourceClassName;
            target.ClassTableName = "BDO_Contact";
            target.ClassDisplayName = "Contact";
            target.ClassType = ClassType.CONTENT_TYPE;
            target.ClassContentTypeType = ClassContentTypeType.WEBSITE;
            target.ClassWebPageHasUrl = true;
        });

        mapping.BuildField("ContactID").AsPrimaryKey();
        mapping.MapPageMetadata(SourceClassName);
        mapping.BuildField("HeroCallToActionButtonType").SetFrom(SourceClassName, "HeroCallToActionButtonType");
        mapping.BuildField("HeroCallToActionButtonText").SetFrom(SourceClassName, "HeroCallToActionButtonText");
        MapEmptyReference(mapping, "HeroCallToActionButtonUrl");
        mapping.BuildField("HeroLayout").SetFrom(SourceClassName, "HeroLayout");
        MapEmptyReference(mapping, "HeroBackgroundImage");
        mapping.BuildField("HeroBackgroundImageAltText").SetFrom(SourceClassName, "HeroBackgroundImageAltText");
        MapEmptyReference(mapping, "HeroBackgroundImage_Mobile");
        mapping.BuildField("HeroBackgroundImage_MobileAltText").SetFrom(SourceClassName, "HeroBackgroundImage_MobileAltText");
        MapEmptyReference(mapping, "MetadataOGImage");
        MapEmptyReference(mapping, "MetadataOGTwitterImage");
        MapEmptyReference(mapping, "MetadataTeaserImage");

        services.AddSingleton<IClassMapping>(mapping);

        return services;
    }

    private static void MapEmptyReference(MultiClassMapping mapping, string targetFieldName) =>
        mapping.BuildField(targetFieldName).ConvertFrom(SourceClassName, "ContactID", false, static (_, _) => "[]");
}