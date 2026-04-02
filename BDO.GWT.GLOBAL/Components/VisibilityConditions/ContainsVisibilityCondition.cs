using Kentico.Forms.Web.Mvc;
using System.Linq;

namespace BDO.GWT.Xperience.Core.LiveSite.Components.VisibilityConditions;

public class ContainsVisibilityCondition : AnotherPropertyVisibilityCondition<object>
{
    public object[] Values { get; set; }

    public override bool IsVisible()
    {
        return Values.Contains(DependeePropertyValue);
    }
}
