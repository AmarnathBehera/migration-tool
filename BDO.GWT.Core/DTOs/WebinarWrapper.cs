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
    public IEnumerable<Service> MetadataServices => _webinar.MetadataServicesEvent;
    public IEnumerable<BusinessLine> MetadataBusinessLines => _webinar.MetadataBusinessLineEvent;
    public IEnumerable<ServiceArea> MetadataServiceAreas => _webinar.MetadataServiceAreasEvent;
    public IEnumerable<IWebPageFieldsSource> MetadataSpecialtiesCategories => _webinar.MetadataSpecialtiesCategories;
    public IEnumerable<IWebPageFieldsSource> MetadataSpecialtiesAreas => _webinar.MetadataSpecialtiesAreas;
    public IEnumerable<IWebPageFieldsSource> MetadataSpecialtiesPages => _webinar.MetadataSpecialtiesPages;
    public IEnumerable<MicrositeMasterPage> MicrositeMasterPages => _webinar.MicrositeMasterPagesEvent;
    public IEnumerable<MicrositePage> MicrositePages => _webinar.MicrositePagesEvent;
    public IEnumerable<Industry> MetadataIndustries => _webinar.MetadataIndustriesEvent;
    public IEnumerable<IndustryCategory> MetadataIndustryCategories => _webinar.MetadataIndustryCategoriesEvent;
    public IEnumerable<LocationOffice> MetadataOfficeLocation => _webinar.MetadataOfficeLocation;
    public IEnumerable<EventType> EventType => _webinar.EventType;
    public IEnumerable<Businessissue> MetadataBusinessIssues => _webinar.MetadataBusinessIssues;
    public bool ShowPastEvent => _webinar.ShowPastEvent;
    public WebPageFields SystemFields => _webinar.SystemFields;
}