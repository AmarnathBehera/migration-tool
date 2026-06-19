using BDO.GWT.Core.Services;
using CMS.Websites;
using Kentico.Content.Web.Mvc;

namespace BDO.GWT.Core.Helpers;

public interface ILocationOfficeHelper
{
    Task<IEnumerable<string>> RetrieveLocationOfficeNamesAsync(Person person, CancellationToken cancellationToken);
}

public class LocationOfficeHelper : ILocationOfficeHelper
{
    private readonly IContentService _contentService;
    private readonly ICultureContext _cultureContext;

    public LocationOfficeHelper(IContentService contentService, ICultureContext cultureContext)
    {
        _contentService = contentService;
        _cultureContext = cultureContext;
    }

    public async Task<IEnumerable<string>> RetrieveLocationOfficeNamesAsync(Person person, CancellationToken cancellationToken)
    {
        var offices = await _contentService.GetPagesAsync<LocationOffice>(
           new RetrievePagesParameters
           {
               LinkedItemsMaxLevel = 1,
               LanguageName = _cultureContext.CurrentLanguageTag,
               UseLanguageFallbacks = false

           },
           query => query
               .Where(w => w.WhereEquals("ContentItemGUID", person.MetadataOfficeLocation))
               .Columns(nameof(LocationOffice.MetadataTitle)),
           RetrievalCacheSettings.CacheDisabled,
           cancellationToken
       );

        return offices.Select(o => o.MetadataTitle);
    }
}
