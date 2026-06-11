using CMS.Websites;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Core
{
    public interface IWebPageUrlResolver
    {
        string GetRelativePath(IWebPageFieldsSource page);
        string GetAbsoluteUrl(IWebPageFieldsSource page);
        Task<string> ResolvePageUrlAsync(int webPageItemId, string language, CancellationToken cancellationToken);
    }
}
