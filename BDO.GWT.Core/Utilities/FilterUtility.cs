using BDO.GWT.Core.Constants;
using BDO.GWT.Core.Data;

namespace BDO.GWT.Core.Utilities;

public static class FilterUtility
{
    private static readonly Dictionary<string, FilterParameterData> FilterParameterDataByName = new Dictionary<string, FilterParameterData>
    {
        [FilterParameterName.BusinessIssue] = new("Topic (Business Issue)", ResourceStringKey.LiveSite.Filters.BusinessIssue, typeof(Businessissue), FilterParameterType.Page),
        [FilterParameterName.BusinessLine] = new("Service (Business Line)", ResourceStringKey.LiveSite.Filters.BusinessLine, typeof(BusinessLine), FilterParameterType.Page),
        //[FilterParameterName.CareerEmploymentType] = new("Employment Type", ResourceStringKey.LiveSite.Filters.CareerEmploymentType, typeof(CareerEmploymentType), FilterParameterType.Entity),
        //[FilterParameterName.CareerJobTitle] = new("Job Title", ResourceStringKey.LiveSite.Filters.CareerJobTitle, typeof(CareerJobTitle), FilterParameterType.Entity),
        //[FilterParameterName.CareerLevel] = new("Level", ResourceStringKey.LiveSite.Filters.CareerLevel, typeof(CareerLevel), FilterParameterType.Entity),
        //[FilterParameterName.ContentType] = new(Taxonomies.Type, ResourceStringKey.LiveSite.Filters.ContentType, typeof(ContentType), FilterParameterType.Taxonomy),
        [FilterParameterName.ContentType] = new("ContentType", ResourceStringKey.LiveSite.Filters.ContentType, typeof(Contenttypes), FilterParameterType.Page),
        //[FilterParameterName.Country] = new("industryCategoryCountry", ResourceStringKey.LiveSite.Filters.Country, typeof(IDealPage), FilterParameterType.Field),
        [FilterParameterName.Credential] = new(Taxonomies.Credential, ResourceStringKey.LiveSite.Filters.Credential, typeof(Credentials), FilterParameterType.Taxonomy),
        //[FilterParameterName.DealType] = new("Deal Type", ResourceStringKey.LiveSite.Filters.DealType, typeof(DealType), FilterParameterType.Entity),
        //[FilterParameterName.EventType] = new("Type", ResourceStringKey.LiveSite.Filters.EventType, typeof(EventType), FilterParameterType.Entity),
        //[FilterParameterName.GlobalDealType] = new("Deal Type", ResourceStringKey.LiveSite.Filters.DealType, typeof(GlobalDealType), FilterParameterType.Entity),
        //[FilterParameterName.GlobalIndustry] = new("Industry", ResourceStringKey.LiveSite.Filters.Industry, typeof(GlobalIndustry), FilterParameterType.Entity),
        //[FilterParameterName.GlobalSubIndustry] = new("Sub-Industry", ResourceStringKey.LiveSite.Filters.SubIndustry, typeof(GlobalSubIndustry), FilterParameterType.Entity),
        //[FilterParameterName.Industry] = new("Industry", ResourceStringKey.LiveSite.Filters.Industry, typeof(IndustryEntity), FilterParameterType.Entity),
        [FilterParameterName.IndustryService] = new("Industry Service", ResourceStringKey.LiveSite.Filters.IndustryService, typeof(IndustryService), FilterParameterType.Page),
        [FilterParameterName.IndustryCategory] = new("Industry (Industry Category)", ResourceStringKey.LiveSite.Filters.IndustryCategory, typeof(IndustryCategory), FilterParameterType.Page),
        [FilterParameterName.IndustryPage] = new("Industry", ResourceStringKey.LiveSite.Filters.Industry, typeof(Industry), FilterParameterType.Page),
        [FilterParameterName.Keywords] = new("MetadataKeywordsBlog", ResourceStringKey.LiveSite.Filters.Keyword, typeof(BlogPost), FilterParameterType.Field),
        //[FilterParameterName.MicrositeMasterPage] = new("Microsite (Microsite Master Page)", ResourceStringKey.LiveSite.Filters.MicrositeMasterPage, typeof(MicrositeMasterPage), FilterParameterType.Page),
        //[FilterParameterName.MicrositePage] = new("Microsite Page", ResourceStringKey.LiveSite.Filters.MicrositePage, typeof(MicrositePage), FilterParameterType.Page),
        [FilterParameterName.OfficeLocation] = new("Office (Office Location)", ResourceStringKey.LiveSite.Filters.LocationOffice, typeof(LocationOffice), FilterParameterType.Page),
        [FilterParameterName.City] = new("City (City)", ResourceStringKey.LiveSite.Filters.City, typeof(LocationCity), FilterParameterType.Page),
        [FilterParameterName.Person] = new("Author (Person)", ResourceStringKey.LiveSite.Filters.Author, typeof(Person), FilterParameterType.Page),
        [FilterParameterName.ProfileGroup] = new(Taxonomies.ProfileGroup, ResourceStringKey.LiveSite.Filters.ProfileGroup, typeof(ProfileGroup), FilterParameterType.Taxonomy),
        [FilterParameterName.Service] = new("Service", ResourceStringKey.LiveSite.Filters.Service, typeof(Service), FilterParameterType.Page),
        [FilterParameterName.ServiceArea] = new("Service Area", ResourceStringKey.LiveSite.Filters.ServiceArea, typeof(ServiceArea), FilterParameterType.Page),
        //[FilterParameterName.SpecialtyCategory] = new("Specialty (Specialty Category)", ResourceStringKey.LiveSite.Filters.SpecialtiesCategory, typeof(SpecialtiesCategory), FilterParameterType.Page),
        //[FilterParameterName.SpecialtyArea] = new("Specialty Area", ResourceStringKey.LiveSite.Filters.SpecialtyArea, typeof(SpecialtiesArea), FilterParameterType.Page),
        //[FilterParameterName.Specialty] = new("Specialty", ResourceStringKey.LiveSite.Filters.Specialty, typeof(SpecialtiesPage), FilterParameterType.Page),
        //[FilterParameterName.SubIndustry] = new("Sub-Industry", ResourceStringKey.LiveSite.Filters.SubIndustry, typeof(SubIndustry), FilterParameterType.Entity),
        //[FilterParameterName.SubService] = new("Sub-Service", ResourceStringKey.LiveSite.Filters.SubService, typeof(SubService), FilterParameterType.Page),
        //[FilterParameterName.Year] = new("Year", ResourceStringKey.LiveSite.Filters.Year, typeof(IDealPage), FilterParameterType.Field),
       
        [FilterParameterName.Credential] = new(Taxonomies.Credential, ResourceStringKey.LiveSite.Filters.Credential, typeof(Credentials), FilterParameterType.Taxonomy),
        [FilterParameterName.CareerEmploymentType] = new(Taxonomies.CareerEmploymentType, ResourceStringKey.LiveSite.Filters.CareerEmploymentType, typeof(CareerEmploymentType), FilterParameterType.Taxonomy),
        [FilterParameterName.CareerJobTitle] = new(Taxonomies.CareerJobTitle, ResourceStringKey.LiveSite.Filters.CareerJobTitle, typeof(CareerJobTitle), FilterParameterType.Taxonomy),
        [FilterParameterName.CareerLevel] = new(Taxonomies.CareerLevel, ResourceStringKey.LiveSite.Filters.CareerLevel, typeof(CareerLevel), FilterParameterType.Taxonomy)
    };
    public static FilterParameterData? GetFilterParameterData(string filterParameterName)
    {
        return FilterParameterDataByName.TryGetValue(filterParameterName, out var filterParameterData) ? filterParameterData : null;
    }

    public static IDictionary<string, string> GetFilterParameterValueByName(IDictionary<string, string> valueByKey)
    {
        var filterParameterValueByName = valueByKey.Where(x => FilterParameterDataByName.ContainsKey(x.Key)).ToDictionary(x => x.Key, x => x.Value);

        return filterParameterValueByName;
    }
}
