using CMS.DataEngine;
using Microsoft.Extensions.DependencyInjection;
using Migration.Tool.Common.Builders;

namespace Migration.Tool.Extensions.ClassMappings;

public static class LocationOfficeClassMapping
{
    private const string SourceClassName = "BDO.LocationOffice";

    public static IServiceCollection AddLocationOfficeMapping(this IServiceCollection services)
    {
        var mapping = new MultiClassMapping(SourceClassName, target =>
        {
            target.ClassName = SourceClassName;
            target.ClassTableName = "BDO_LocationOffice";
            target.ClassDisplayName = "Location Office";
            target.ClassType = ClassType.CONTENT_TYPE;
            target.ClassContentTypeType = ClassContentTypeType.WEBSITE;
            target.ClassWebPageHasUrl = true;
        });

        mapping.BuildField("LocationOfficeID").AsPrimaryKey();
        mapping.MapPageMetadata(SourceClassName);

        // Coupled-table specific fields
        MapEmptyReference(mapping, "OfficeImage");
        mapping.BuildField("OfficeImageAltText").SetFrom(SourceClassName, "OfficeImageAltText");
        mapping.BuildField("LocationAddressLine1").ConvertFrom(SourceClassName, "LocationAddressLine1", false, Coalesce); // NOT NULL
        mapping.BuildField("LocationAddressLine2").SetFrom(SourceClassName, "LocationAddressLine2");
        mapping.BuildField("LocationZip").ConvertFrom(SourceClassName, "LocationZip", false, Coalesce); // NOT NULL
        mapping.BuildField("LocationZipDisplayType").ConvertFrom(SourceClassName, "LocationZipDisplayType", false, Coalesce); // NOT NULL
        mapping.BuildField("LocationCountry").ConvertFrom(SourceClassName, "LocationCountry", false, Coalesce); // NOT NULL
        mapping.BuildField("LocationLatitude").ConvertFrom(SourceClassName, "LocationLatitude", false, Coalesce); // NOT NULL
        mapping.BuildField("LocationLongitude").ConvertFrom(SourceClassName, "LocationLongitude", false, Coalesce); // NOT NULL
        mapping.BuildField("LocationEmail").SetFrom(SourceClassName, "LocationEmail");
        mapping.BuildField("LocationType").ConvertFrom(SourceClassName, "LocationType", false, Stringify); // source int -> target string
        mapping.BuildField("LocationPhone").ConvertFrom(SourceClassName, "LocationPhone", false, Coalesce); // NOT NULL
        mapping.BuildField("LocationFax").SetFrom(SourceClassName, "LocationFax");
        mapping.BuildField("OpeningHours").SetFrom(SourceClassName, "OpeningHours");
        mapping.BuildField("LocationPOBoxNumber").SetFrom(SourceClassName, "LocationPOBoxNumber");
        mapping.BuildField("LocationPOCity").SetFrom(SourceClassName, "LocationPOCity");
        mapping.BuildField("LocationPOZip").SetFrom(SourceClassName, "LocationPOZip");
        mapping.BuildField("LocationPOZipDisplayType").SetFrom(SourceClassName, "LocationPOZipDisplayType");
        mapping.BuildField("LocationPOCountry").SetFrom(SourceClassName, "LocationPOCountry");

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

    private static object? Stringify(object? value, IConvertorContext context) => value?.ToString();

    private static void MapEmptyReference(MultiClassMapping mapping, string targetFieldName) =>
        mapping.BuildField(targetFieldName).ConvertFrom(SourceClassName, "LocationOfficeID", false, static (_, _) => "[]");
}
