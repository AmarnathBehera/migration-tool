using BDO;
using BDO.GWT.Xperience;
using CMS.DataEngine;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GWTXbyKWeb.Components.ViewComponents
{
    public class MarketoMunchkincode : ViewComponent
    {
        private readonly IInfoProvider<GlobalsettingskeyInfo> _provider;
        private const string MarketoMunchkincodeKey = "BDO_MarketoMunchkincode";
        public MarketoMunchkincode(IInfoProvider<GlobalsettingskeyInfo> provider)
        {
            _provider = provider;

        }
        public async Task<IViewComponentResult> InvokeAsync(CancellationToken cancellationToken)
        {
            var MarketoMunchkincodeValue = _provider.Get()
                             .WhereEquals(
                                 nameof(GlobalsettingskeyInfo.GlobalSettingsKeyName),
                                 MarketoMunchkincodeKey)
                             .TopN(1)
                             .GetEnumerableTypedResult();
            var first = MarketoMunchkincodeValue?.FirstOrDefault();
            var model = new GlobalsettingDTO
            {
                value = first?.GlobalSettingsKeyValue
            };
            return View("~/Views/Shared/Components/Scripts/Default.cshtml", model.value);
        }
    }
}
