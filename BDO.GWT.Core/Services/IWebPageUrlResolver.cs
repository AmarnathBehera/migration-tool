using CMS.Websites;

namespace BDO.GWT.Core
{
    public interface IWebPageUrlResolver
    {
        string GetRelativePath(IWebPageFieldsSource page);
    }
}
