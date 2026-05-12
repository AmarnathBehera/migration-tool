using BDO.GWT.Core.Constants;
using BDO.GWT.Core.Data;

namespace BDO.GWT.Core.Utilities;

public static class FilterUtility
{
    private static readonly Dictionary<string, FilterParameterData> FilterParameterDataByName = new Dictionary<string, FilterParameterData>
    {
        [FilterParameterName.OfficeLocation] = new("Office (Office Location)", ResourceStringKey.LiveSite.Filters.LocationOffice, typeof(LocationOffice), FilterParameterType.Page),
        [FilterParameterName.City] = new("City (City)", ResourceStringKey.LiveSite.Filters.City, typeof(LocationCity), FilterParameterType.Page),
        [FilterParameterName.Person] = new("Author (Person)", ResourceStringKey.LiveSite.Filters.Author, typeof(Person), FilterParameterType.Page),
        [FilterParameterName.ProfileGroup] = new(Taxonomies.ProfileGroup, ResourceStringKey.LiveSite.Filters.ProfileGroup, typeof(ProfileGroup), FilterParameterType.Taxonomy),
        [FilterParameterName.Credential] = new(Taxonomies.Credential, ResourceStringKey.LiveSite.Filters.Credential, typeof(Credentials), FilterParameterType.Taxonomy),
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
