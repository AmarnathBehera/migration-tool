using CMS.Websites;

namespace BDO.GWT.Core.DTOs;

public class EventWrapper : IEventOrWebinar
{
    private readonly Event _event;

    public EventWrapper(Event @event) => _event = @event;

    public string MetadataTitle => _event.MetadataTitle;
    public DateTime EventDateFrom => _event.EventDateFrom;
    public DateTime EventDateTo => _event.EventDateTo;
    public bool EventIsPrivate => _event.EventIsPrivate;
    public string? EventLocationCity => _event.EventLocationCity;
    public string? MetadataDescription => _event.MetadataDescription;
    public IEnumerable<Service> MetadataServices => _event.MetadataServicesEvent;
    public IEnumerable<BusinessLine> MetadataBusinessLines => _event.MetadataBusinessLineEvent;
    public IEnumerable<ServiceArea> MetadataServiceAreas => _event.MetadataServiceAreasEvent;
    public IEnumerable<IWebPageFieldsSource> MetadataSpecialtiesCategories => _event.MetadataSpecialtiesCategories;
    public IEnumerable<IWebPageFieldsSource> MetadataSpecialtiesAreas => _event.MetadataSpecialtiesCategories;
    public IEnumerable<IWebPageFieldsSource> MetadataSpecialtiesPages => _event.MetadataSpecialtiesCategories;
    public IEnumerable<MicrositeMasterPage> MicrositeMasterPages => _event.MicrositeMasterPagesEvent;
    public IEnumerable<MicrositePage> MicrositePages => _event.MicrositePagesEvent;
    public IEnumerable<Industry> MetadataIndustries => _event.MetadataIndustriesEvent;
    public IEnumerable<IndustryCategory> MetadataIndustryCategories => _event.MetadataIndustryCategoriesEvent;
    public IEnumerable<LocationOffice> MetadataOfficeLocation => _event.MetadataOfficeLocation;
    public IEnumerable<EventType> EventType => _event.EventType;
    public IEnumerable<Businessissue> MetadataBusinessIssues => _event.MetadataBusinessIssues;

    public bool ShowPastEvent => _event.ShowPastEvent;
    public WebPageFields SystemFields => _event.SystemFields;
}