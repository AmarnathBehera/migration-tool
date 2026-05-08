using CMS.ContentEngine;
using CMS.DataEngine;
using TaxonomyTag = CMS.ContentEngine.Tag;

namespace BDO.GWT.Core.Retrievers;

public interface ITaxonomyTagRetriever
{
    Task<IEnumerable<TaxonomyTag>> GetTaxonomyTagsAsync(
        string taxonomyName,
        string languageName,
        HashSet<string>? filterTagNames = null,
        CancellationToken cancellationToken = default);
    Task<TaxonomyData?> GetTagAsync(string taxonomyName, string value, string culture);
    Task<TagInfo> GetTaxonomyTagByNameAsync(string tagName,CancellationToken cancellationToken = default);
    Task<IEnumerable<TaxonomyTag>> GetTaxonomyTagByGUIDs(IEnumerable<TagReference> tagGuids, string languageName);
}
public class TaxonomyTagRetriever : ITaxonomyTagRetriever
{
    private readonly ITaxonomyRetriever _taxonomyRetriever;
    private readonly IInfoProvider<TagInfo> _tagProvider;

    public TaxonomyTagRetriever(ITaxonomyRetriever taxonomyRetriever, IInfoProvider<TagInfo> tagProvider)
    {
        _taxonomyRetriever = taxonomyRetriever;
        _tagProvider = tagProvider;
    }

    public async Task<IEnumerable<TaxonomyTag>> GetTaxonomyTagsAsync(
        string taxonomyName,
        string languageName,
        HashSet<string>? filterTagNames = null,
        CancellationToken cancellationToken = default)
    {
        var taxonomyData = await _taxonomyRetriever.RetrieveTaxonomy(
            taxonomyName,
            languageName);

        if (taxonomyData is null)
        {
            return Enumerable.Empty<TaxonomyTag>();
        }
        var tags = taxonomyData.Tags;

        if (filterTagNames == null || filterTagNames.Count == 0)
        {
            return tags;
        }

        return tags.Where(tag => filterTagNames.Contains(tag.Name));
    }
    public async Task<TaxonomyData?> GetTagAsync(string taxonomyName, string value, string culture)
    {
        var taxonomy = await _taxonomyRetriever.RetrieveTaxonomy(taxonomyName, culture);
        return taxonomy;
    }
    public async Task<TagInfo> GetTaxonomyTagByNameAsync(
        string tagName,
        CancellationToken cancellationToken = default)
    {
        var taxonomyItems = await _tagProvider.GetAsync(tagName);
        if (taxonomyItems == null)
        {
            return null;
        }
        return taxonomyItems;
    }

    public async Task<IEnumerable<TaxonomyTag>> GetTaxonomyTagByGUIDs(IEnumerable<TagReference> tagGuids,string languageName)
    {
        IEnumerable<TaxonomyTag> tags = await _taxonomyRetriever.RetrieveTags(
            tagGuids.Select(t => t.Identifier),
            languageName
        );
        if(tags == null)
        {
            return Enumerable.Empty<TaxonomyTag>();
        }
        return tags;
    }
}
