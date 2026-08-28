using BDO.GWT.Core.ValueObjects;
using CMS.Websites;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Core
{
    public interface IWebPageUrlResolver
    {
        string GetRelativePath(IWebPageFieldsSource page);
        string GetAbsoluteUrl(IWebPageFieldsSource page);
        Task<string> ResolvePageUrlAsync(int webPageItemId, string language, CancellationToken cancellationToken);
        Task<string> GetHomePageUrlAsync(CancellationToken cancellationToken);
        Task<IDictionary<int, RelativeLink>> GetPageRelativeUrlByNodeIdAsync(
            IEnumerable<int> nodeIds, LanguageTag languageTag, CancellationToken cancellationToken);
    }
}
