using BDO;
using BDO.GWT.Xperience;
using CMS.DataEngine;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GWTXbyKWeb.Components.ViewComponents
{
    public class StartScriptsHead : ViewComponent
    {
        private readonly IInfoProvider<GlobalsettingskeyInfo> _provider;
        private const string ScriptHeadKey = "BDO_ScriptsTopofPageHead";
        public StartScriptsHead(IInfoProvider<GlobalsettingskeyInfo> provider)
        {
            _provider = provider;

        }
        public async Task<IViewComponentResult> InvokeAsync(CancellationToken cancellationToken)
        {
            var scriptHeadValue = _provider.Get()
                             .WhereEquals(
                                 nameof(GlobalsettingskeyInfo.GlobalSettingsKeyName),
                                 ScriptHeadKey)
                             .TopN(1)
                             .GetEnumerableTypedResult();
            var first = scriptHeadValue?.FirstOrDefault();
            var model = new GlobalsettingDTO
            {
                value = first?.GlobalSettingsKeyValue
            };
            return View("~/Views/Shared/Components/Scripts/Default.cshtml", model.value);
        }
    }
}
