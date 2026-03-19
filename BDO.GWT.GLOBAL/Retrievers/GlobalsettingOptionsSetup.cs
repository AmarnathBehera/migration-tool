using CMS.DataEngine;
using System.Linq;

namespace BDO.GWT.Xperience
{
    public class GlobalSettingsService
    {
        private const string ScriptHeadKey = "BDO_ScriptHead";

        private readonly IInfoProvider<GlobalsettingskeyInfo> _provider;

        public GlobalSettingsService(IInfoProvider<GlobalsettingskeyInfo> provider)
        {
            _provider = provider;
        }

        public void Configure()
        {

            var scriptHeadValue = _provider.Get()
                .WhereEquals(
                    nameof(GlobalsettingskeyInfo.GlobalSettingsKeyName),
                    ScriptHeadKey)
                .TopN(1)
                .GetEnumerableTypedResult()
                .FirstOrDefault()
                ?.GlobalSettingsKeyValue;

            
        }
    }
}
