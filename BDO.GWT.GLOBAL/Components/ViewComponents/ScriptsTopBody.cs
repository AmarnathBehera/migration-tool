using BDO;
using BDO.GWT.Xperience;
using CMS.DataEngine;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GWTXbyKWeb.Components.ViewComponents
{
    public class ScriptsTopBody : ViewComponent
    {
        private readonly IInfoProvider<GlobalsettingskeyInfo> _provider;
        private const string ScriptintopofBODYKey = "BDO_ScriptintopofBODY";
        public ScriptsTopBody(IInfoProvider<GlobalsettingskeyInfo> provider)
        {
            _provider = provider;

        }
        public async Task<IViewComponentResult> InvokeAsync(CancellationToken cancellationToken)
        {
            var ScriptintopofBODYValue = _provider.Get()
                             .WhereEquals(
                                 nameof(GlobalsettingskeyInfo.GlobalSettingsKeyName),
                                 ScriptintopofBODYKey)
                             .TopN(1)
                             .GetEnumerableTypedResult();
            var first = ScriptintopofBODYValue?.FirstOrDefault();
            var model = new GlobalsettingDTO
            {
                value = first?.GlobalSettingsKeyValue
            };
            return View("~/Views/Shared/Components/Scripts/Default.cshtml", model.value);
        }
    }
}
