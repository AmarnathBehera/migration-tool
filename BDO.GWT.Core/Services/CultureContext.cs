using System.Globalization;
using Kentico.Content.Web.Mvc.Routing;

namespace BDO.GWT.Core.Services;

public interface ICultureContext
{
    public string CurrentLanguageTag { get; }
    public bool IsCurrentCultureRtl { get; }
}

public class CultureContext : ICultureContext
{
    private readonly IPreferredLanguageRetriever preferredLanguageRetriever;

    public CultureContext(IPreferredLanguageRetriever preferredLanguageRetriever)
    {
        this.preferredLanguageRetriever = preferredLanguageRetriever;
    }

    public string CurrentLanguageTag => preferredLanguageRetriever.Get();

    public bool IsCurrentCultureRtl =>
        CultureInfo.GetCultureInfo(preferredLanguageRetriever.Get()).TextInfo.IsRightToLeft;

}

