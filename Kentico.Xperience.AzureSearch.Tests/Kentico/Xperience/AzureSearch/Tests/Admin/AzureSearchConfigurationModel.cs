

namespace Kentico.Xperience.AzureSearch.Tests.Admin
{
    internal class AzureSearchConfigurationModel : AzureSearch.Admin.AzureSearchConfigurationModel
    {
        public string IndexName { get; set; }
        public string ChannelName { get; set; }
        public string StrategyName { get; set; }
        public object LanguageNames { get; set; }
    }
}