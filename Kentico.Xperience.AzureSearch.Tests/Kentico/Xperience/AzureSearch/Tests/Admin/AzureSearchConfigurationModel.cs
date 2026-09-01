

namespace Kentico.Xperience.AzureSearch.Tests.Admin
{
    internal class AzureSearchConfigurationModel : AzureSearch.Admin.AzureSearchConfigurationModel
    {
#pragma warning disable CS0108 // Member hides inherited member; missing new keyword
        public string IndexName { get; set; } = null!;
        public string ChannelName { get; set; } = null!;
        public string StrategyName { get; set; } = null!;
        public object LanguageNames { get; set; } = null!;

#pragma warning restore CS0108 // Member hides inherited member; missing new keyword

    }

}