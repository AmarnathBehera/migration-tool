using BDO.GWT.Xperience.Core.Retrievers;
using CMS.ContentEngine;
using CMS.Websites;

namespace BDO.GWT.Web.Core.Helpers;

public interface IChildPageHelper
{
    Task<IDictionary<string, List<int>>> GetDescendantNodeIdsByContentType(string[] contentTypeNames, string treePathPrefix);
}

public class ChildPageHelper : IChildPageHelper
{
    private readonly IMultiTypePageRetriever _multiTypePageRetriever;

    public ChildPageHelper(IMultiTypePageRetriever multiTypePageRetriever)
    {
        _multiTypePageRetriever = multiTypePageRetriever;
    }

    public async Task<IDictionary<string, List<int>>> GetDescendantNodeIdsByContentType(
        string[] contentTypeNames,
        string treePathPrefix)
    {
        Func<IWebPageContentQueryDataContainer, WebPageItem> mapper =
            container => new WebPageItem(
                container.ContentTypeName,
                container.WebPageItemID,
                container.WebPageItemTreePath
            );

        var descendantPages = await _multiTypePageRetriever.RetrievePagesAsync(
            contentTypeNames,
            mapper,
            decorateParameters: p => p.Where(w => w.WhereStartsWith(nameof(IWebPageContentQueryDataContainer.WebPageItemTreePath), treePathPrefix))
        );

        var idsByContentType = new Dictionary<string, List<int>>();

        foreach (var page in descendantPages)
        {
            idsByContentType.TryAdd(page.ContentTypeName, []);
            idsByContentType[page.ContentTypeName].Add(page.WebPageItemID);
        }

        return idsByContentType;
    }
}

public record WebPageItem(string ContentTypeName, int WebPageItemID, string TreePath);