
namespace BDO.GWT.Common;
public interface IWebsiteContext
{
    int CurrentWebsiteID { get; }

    string CurrentWebsiteName { get; }

    bool IsPreview { get; }
}
