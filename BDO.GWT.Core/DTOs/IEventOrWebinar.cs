using CMS.ContentEngine;
using CMS.Websites;

namespace BDO.GWT.Core.DTOs;

public interface IEventOrWebinar : IWebPageFieldsSource
{
    string MetadataTitle { get; }
    DateTime EventDateFrom { get; }
    DateTime EventDateTo { get; }
    bool EventIsPrivate { get; }
    string? EventLocationCity { get; }
    string? MetadataDescription { get; }
    IEnumerable<Service> MetadataServices { get; }
    IEnumerable<BusinessLine> MetadataBusinessLines { get; }
    IEnumerable<ServiceArea> MetadataServiceAreas { get; }
    IEnumerable<IWebPageFieldsSource> MetadataSpecialtiesCategories { get; }
    IEnumerable<IWebPageFieldsSource> MetadataSpecialtiesAreas { get; }
    IEnumerable<IWebPageFieldsSource> MetadataSpecialtiesPages { get; }
    IEnumerable<MicrositeMasterPage> MicrositeMasterPages { get; }
    IEnumerable<MicrositePage> MicrositePages { get; }
    IEnumerable<Industry> MetadataIndustries { get; }
    IEnumerable<IndustryCategory> MetadataIndustryCategories { get; }
    IEnumerable<LocationOffice> MetadataOfficeLocation { get; }
    IEnumerable<EventType> EventType { get; }
    IEnumerable<Businessissue> MetadataBusinessIssues { get; }
    bool ShowPastEvent { get; }
}