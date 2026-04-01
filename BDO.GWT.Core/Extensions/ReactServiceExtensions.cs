using BDO.GWT.Core.Services;
using Microsoft.AspNetCore.Html;
using System.Threading;
using System.Threading.Tasks;

namespace BDO.GWT.Core.Extensions;

public static class ReactServiceExtensions
{
    public static async Task<IHtmlContent> RenderComponentAsync(this IReactService reactService, int pageId, string componentType, string persistentUniqueIdentifier, CancellationToken cancellationToken)
    {
        return await reactService.RenderComponentAsync<object?>(pageId,componentType, persistentUniqueIdentifier, null, cancellationToken);
    }
}
