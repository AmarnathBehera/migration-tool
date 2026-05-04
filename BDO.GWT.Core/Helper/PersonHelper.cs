using CMS.Websites;
using Kentico.Content.Web.Mvc;
using static BDO.GWT.Core.Constants.ResourceStringKey.LiveSite;

namespace BDO.GWT.Core.Helpers;

public interface IPersonHelper
{
    Task<PersonEmailInfo> GetPersonPageEmailId(string personTitle, string url, string culture);

}
public class PersonHelper : IPersonHelper
{
    private readonly IContentService _contentService;
    public PersonHelper(IContentService contentService)
    {
        _contentService = contentService;
    }
    public async Task<PersonEmailInfo> GetPersonPageEmailId(string personTitle, string url, string culture)
    {
        string name;
        //var people = await _contentService.GetItemsAsync<Person>(builder =>
        //{
        //    builder.InLanguage(culture);

        //    builder.Parameters(p => p
        //        .Where(w => w.WhereEquals("WebPageItemPath", url))
        //        .OrderBy(nameof(IWebPageContentQueryDataContainer.WebPageItemOrder))
        //    );
        //});
        var people = new List<Person> {  };
        var person = people.FirstOrDefault();

        if (person == null)
        {
            var fallbackPeople = await _contentService.GetItemsAsync<Person>(builder =>
            {
                //builder.InLanguage(culture);

                builder.Parameters(p => p
                    .Where(w => w.WhereEquals("MetadataTitle", personTitle))
                    //.OrderBy(nameof(IWebPageContentQueryDataContainer.WebPageItemOrder))
                );
            });

            var fallback = fallbackPeople.FirstOrDefault();
            name = fallback?.PersonFirstName ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(fallback?.PersonSurnamePrefixes))
                name += " " + fallback.PersonSurnamePrefixes;

            if (!string.IsNullOrWhiteSpace(fallback?.PersonLastName))
                name += " " + fallback.PersonLastName;

            return new PersonEmailInfo
            {
                Email = fallback?.PersonEmail ?? string.Empty,
                Name = name
            };
        }

        name = person.PersonFirstName ?? string.Empty;

        if (!string.IsNullOrWhiteSpace(person?.PersonSurnamePrefixes))
            name += " " + person.PersonSurnamePrefixes;

        if (!string.IsNullOrWhiteSpace(person?.PersonLastName))
            name += " " + person.PersonLastName;

        return new PersonEmailInfo
        {
            Email = person?.PersonEmail ?? string.Empty,
            Name = name
        };
    }
}
public class PersonEmailInfo
{
    public string? Email { get; set; }
    public string? Name { get; set; }
}
