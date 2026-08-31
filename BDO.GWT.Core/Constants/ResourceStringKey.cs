using System.Diagnostics.CodeAnalysis;

namespace BDO.GWT.Core.Constants;

[SuppressMessage("SonarCloud", "S3218", Justification = "Not writing code inside nested classes and fields are only used outside of the file.")]
public static class ResourceStringKey
{
    private const string BDOPrefix = "BDO.";

    public static class LiveSite
    {
        public const string LiveSitePrefix = BDOPrefix + "LiveSite.";

        public const string FilterBy = LiveSitePrefix + "FilterBy";

        public const string ReadMore = LiveSitePrefix + "ReadMore";

        public const string SortBy = LiveSitePrefix + "SortBy";

        public static class Filters
        {
            private const string FiltersPrefix = LiveSitePrefix + "Filters.";

            public const string BusinessIssue = FiltersPrefix + "BusinessIssue";

            public const string Keyword = FiltersPrefix + "Keyword";

            public const string Author = FiltersPrefix + "Author";

            public const string BusinessLine = FiltersPrefix + "BusinessLine";

            public const string ContentType = FiltersPrefix + "ContentType";

            public const string Country = FiltersPrefix + "Country";

            public const string CareerEmploymentType = FiltersPrefix + "CareerEmploymentType";

            public const string CareerJobTitle = FiltersPrefix + "CareerJobTitle";

            public const string CareerLevel = FiltersPrefix + "CareerLevel";

            public const string CareerType = FiltersPrefix + "CareerType";

            public const string Credential = FiltersPrefix + "Credential";

            public const string DealType = FiltersPrefix + "DealType";

            public const string EventType = FiltersPrefix + "EventType";

            public const string Industry = FiltersPrefix + "Industry";

            public const string IndustryCategory = FiltersPrefix + "IndustryCategory";

            public const string IndustryService = FiltersPrefix + "IndustryService";

            public const string LocationOffice = FiltersPrefix + "LocationOffice";

            public const string City = FiltersPrefix + "City";

            public const string MicrositeMasterPage = FiltersPrefix + "MicrositeMasterPage";

            public const string MicrositePage = FiltersPrefix + "MicrositePage";

            public const string ProfileGroup = FiltersPrefix + "ProfileGroup";

            public const string Service = FiltersPrefix + "Service";

            public const string ServiceArea = FiltersPrefix + "ServiceArea";

            public const string SpecialtiesCategory = FiltersPrefix + "SpecialtiesCategory";

            public const string SpecialtyArea = FiltersPrefix + "SpecialtyArea";

            public const string Specialty = FiltersPrefix + "Specialty";

            public const string SubIndustry = FiltersPrefix + "SubIndustry";

            public const string SubService = FiltersPrefix + "SubService";

            public const string Year = FiltersPrefix + "Year";
        }
        public static class ViewGlobalLocations
        {
            private const string ViewGlobalLocationsPrefix = LiveSitePrefix + "ViewGlobalLocations.";

            public const string Button = ViewGlobalLocationsPrefix + "Button";

            public const string Description = ViewGlobalLocationsPrefix + "Description";

            public const string Title = ViewGlobalLocationsPrefix + "Title";
        }
        public static class Insights
        {
            private const string InsightsPrefix = LiveSitePrefix + "Insights.";

            public const string Authors = InsightsPrefix + "Authors";
        }
        public static class People
        {
            private const string PeoplePrefix = LiveSitePrefix + "People.";

            public const string SortLastNameAtoZ = PeoplePrefix + "SortLastNameAtoZ";

            public const string SortLastNameZtoA = PeoplePrefix + "SortLastNameZtoA";

            public const string SortNodeOrder = PeoplePrefix + "SortNodeOrder";
        }
        public static class Careers
        {
            private const string CareersPrefix = LiveSitePrefix + "Careers.";

            public const string RelevantContactPeople = CareersPrefix + "RelevantContactPeople";
        }

        public static class SearchPage
        {
            private const string SearchPagePrefix = LiveSitePrefix + "SearchPage.";

            public const string Deals = SearchPagePrefix + "Deals";

            public const string Industries = SearchPagePrefix + "Industries";

            public const string Insights = SearchPagePrefix + "Insights";

            public const string Services = SearchPagePrefix + "Services";

            public const string People = SearchPagePrefix + "People";

            public const string Blogs = SearchPagePrefix + "Blogs";

            public const string News = SearchPagePrefix + "News";

            public const string Specialties = SearchPagePrefix + "Specialties";

            public const string EventsWebinars = SearchPagePrefix + "EventsWebinars";

            public const string TopResults = SearchPagePrefix + "TopResults";

            public const string MostRecent = SearchPagePrefix + "MostRecent";
        }
    }
}
