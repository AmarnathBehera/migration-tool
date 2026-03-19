using BDO;
using BDO.GWT.Xperience;
using CMS.DataEngine;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GWTXbyKWeb.Components.ViewComponents
{
    public class ScriptsBottomBody : ViewComponent
    {
        private readonly IInfoProvider<GlobalsettingskeyInfo> _provider;
        private const string ScriptinbottomofBODYKey = "BDO_ScriptinbottomofBODY";
        public ScriptsBottomBody(IInfoProvider<GlobalsettingskeyInfo> provider)
        {
            _provider = provider;

        }
        public async Task<IViewComponentResult> InvokeAsync(CancellationToken cancellationToken)
        {
            var ScriptinbottomofBODYValue = _provider.Get()
                             .WhereEquals(
                                 nameof(GlobalsettingskeyInfo.GlobalSettingsKeyName),
                                 ScriptinbottomofBODYKey)
                             .TopN(1)
                             .GetEnumerableTypedResult();
            var first = ScriptinbottomofBODYValue?.FirstOrDefault();
            var model = new GlobalsettingDTO
            {
                value = first?.GlobalSettingsKeyValue
            };
            return View("~/Views/Shared/Components/Scripts/Default.cshtml", model.value);
        }
    }
}
