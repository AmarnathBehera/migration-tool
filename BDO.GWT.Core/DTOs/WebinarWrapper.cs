using CMS.Websites;

namespace BDO.GWT.Core.DTOs;

public class WebinarWrapper : IEventOrWebinar
{
    private readonly Webinar _webinar;

    public WebinarWrapper(Webinar webinar) => _webinar = webinar;

    public string MetadataTitle => _webinar.MetadataTitle;
    public DateTime EventDateFrom => _webinar.EventDateFrom;
    public DateTime EventDateTo => _webinar.EventDateTo;
    public bool EventIsPrivate => _webinar.EventIsPrivate;
    public string? EventLocationCity => _webinar.EventLocationCity;
    public string? MetadataDescription => _webinar.MetadataDescription;
    public IEnumerable<Service> MetadataServices => _webinar.MetadataServices;
    public IEnumerable<BusinessLine> MetadataBusinessLines => _webinar.MetadataBusinessLines;
    public IEnumerable<ServiceArea> MetadataServiceAreas => _webinar.MetadataServiceAreas;
    public IEnumerable<IWebPageFieldsSource> MetadataSpecialtiesCategories => _webinar.MetadataSpecialtiesCategories;
    public IEnumerable<IWebPageFieldsSource> MetadataSpecialtiesAreas => _webinar.MetadataSpecialtiesAreas;
    public IEnumerable<IWebPageFieldsSource> MetadataSpecialtiesPages => _webinar.MetadataSpecialtiesPages;
    public IEnumerable<MicrositePlaceholder> MicrositeMasterPages => _webinar.MicrositeMasterPages;
    public IEnumerable<MicrositePlaceholder> MicrositePages => _webinar.MicrositePages;
    public IEnumerable<Industry> MetadataIndustries => _webinar.MetadataIndustries;
    public IEnumerable<IndustryCategory> MetadataIndustryCategories => _webinar.MetadataIndustryCategories;
    public IEnumerable<LocationOffice> MetadataOfficeLocation => _webinar.MetadataOfficeLocation;
    public IEnumerable<EventType> EventType => _webinar.EventType;
    public IEnumerable<Businessissues> MetadataBusinessIssues => _webinar.MetadataBusinessIssues;
    public bool ShowPastEvent => _webinar.ShowPastEvent;
    public WebPageFields SystemFields => _webinar.SystemFields;
}