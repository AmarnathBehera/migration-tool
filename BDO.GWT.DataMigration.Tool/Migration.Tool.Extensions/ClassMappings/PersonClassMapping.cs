using CMS.DataEngine;
using Microsoft.Extensions.DependencyInjection;
using Migration.Tool.Common.Builders;

namespace Migration.Tool.Extensions.ClassMappings;

public static class PersonClassMapping
{
    private const string SourceClassName = "BDO.Person";

    public static IServiceCollection AddPersonMapping(this IServiceCollection services)
    {
        var mapping = new MultiClassMapping(SourceClassName, target =>
        {
            target.ClassName = SourceClassName;
            target.ClassTableName = "BDO_Person";
            target.ClassDisplayName = "Person";
            target.ClassType = ClassType.CONTENT_TYPE;
            target.ClassContentTypeType = ClassContentTypeType.WEBSITE;
            target.ClassWebPageHasUrl = true;
        });

        mapping.BuildField("PersonID").AsPrimaryKey();
        mapping.MapPageMetadata(SourceClassName);
        mapping.BuildField("PersonFirstName").SetFrom(SourceClassName, "PersonFirstName");
        mapping.BuildField("PersonSurnamePrefixes").SetFrom(SourceClassName, "PersonSurnamePrefixes");
        mapping.BuildField("PersonLastName").SetFrom(SourceClassName, "PersonLastName");
        mapping.BuildField("PersonJobTitle").SetFrom(SourceClassName, "PersonJobTitle");
        mapping.BuildField("PersonPhone").SetFrom(SourceClassName, "PersonPhone");
        mapping.BuildField("PersonMobilePhone").SetFrom(SourceClassName, "PersonMobilePhone");
        mapping.BuildField("PersonDirectDialNumber").SetFrom(SourceClassName, "PersonDirectDialNumber");
        mapping.BuildField("PersonEmail").SetFrom(SourceClassName, "PersonEmail");
        mapping.BuildField("EnableForm").SetFrom(SourceClassName, "EnableForm");
        mapping.BuildField("EnableVCard").SetFrom(SourceClassName, "EnableVCard");
        mapping.BuildField("LinkedIn").SetFrom(SourceClassName, "LinkedIn");
        mapping.BuildField("Facebook").SetFrom(SourceClassName, "Facebook");
        mapping.BuildField("Twitter").SetFrom(SourceClassName, "Twitter");
        MapEmptyReference(mapping, "PersonCredentials");
        MapEmptyReference(mapping, "MetadataCity");
        MapEmptyReference(mapping, "MetadataOfficeLocation");
        MapEmptyReference(mapping, "MetadataProfileGroups");
        MapEmptyReference(mapping, "MetadataBusinessLines");
        MapEmptyReference(mapping, "MetadataServiceAreasPerson");
        MapEmptyReference(mapping, "MetadataServicesPerson");
        MapEmptyReference(mapping, "MetadataSubServices");
        MapEmptyReference(mapping, "MetadataIndustriesPerson");
        MapEmptyReference(mapping, "MetadataIndustryServices");
        MapEmptyReference(mapping, "MetadataIndustryCategoriesPerson");
        MapEmptyReference(mapping, "MetadataSpecialtiesCategories");
        MapEmptyReference(mapping, "MetadataSpecialtiesPages");
        MapEmptyReference(mapping, "MetadataSpecialtiesAreas");
        MapEmptyReference(mapping, "MicrositeMasterPagesPerson");
        MapEmptyReference(mapping, "MicrositePagesPerson");

        services.AddSingleton<IClassMapping>(mapping);

        return services;
    }

    private static void MapEmptyReference(MultiClassMapping mapping, string targetFieldName) =>
        mapping.BuildField(targetFieldName).ConvertFrom(SourceClassName, "PersonID", false, static (_, _) => "[]");
}
