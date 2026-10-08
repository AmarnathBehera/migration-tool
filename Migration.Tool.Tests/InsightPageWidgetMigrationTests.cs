using Migration.Tool.Common;
using Migration.Tool.Common.Helpers;
using Migration.Tool.Common.Services;
using Migration.Tool.Extensions.CommunityMigrations;
using Migration.Tool.KXP.Api.Services.CmsClass;
using Newtonsoft.Json.Linq;

namespace Migration.Tool.Tests;

public class InsightPageWidgetMigrationTests
{
    [Fact]
    public void TargetChannelDomain_UsesDistinctHostOnXbyKPort()
    {
        var configuration = new ToolConfiguration
        {
            TargetWebsiteChannelDomains = new Dictionary<string, string> { ["Site"] = "127.0.0.1:12393" }
        };
        var fallbackPort = 5000;

        var result = UriHelper.GetUniqueDomainCandidate(configuration.TargetWebsiteChannelDomains["Site"],
            ref fallbackPort, domain => !domain.Equals("localhost:12393", StringComparison.OrdinalIgnoreCase));

        Assert.True(result.Success);
        Assert.Equal("127.0.0.1:12393", result.Result);
    }

    [Fact]
    public void PagePathSelection_IncludesOnlyConfiguredPagesAndDescendants()
    {
        var configuration = new ToolConfiguration
        {
            MigrateOnlyPagePaths = ["/Insights/*", "/Animation-card/*", "/Blog-Post"]
        };

        Assert.True(configuration.IncludesPagePath("/Insights"));
        Assert.True(configuration.IncludesPagePath("Insights"));
        Assert.True(configuration.IncludesPagePath("/Insights/"));
        Assert.True(configuration.IncludesPagePath("/insights/Group/Article"));
        Assert.True(configuration.IncludesPagePath("/Animation-card/Title"));
        Assert.True(configuration.IncludesPagePath("/Blog-Post"));
        Assert.False(configuration.IncludesPagePath("/Insights-old"));
        Assert.False(configuration.IncludesPagePath("/Blog-Post/Other"));
    }

    [Theory]
    [InlineData("Specialty", "specialtyPages")]
    [InlineData("Insight", "insightPages")]
    [InlineData("Blog", "pages")]
    public async Task TrendingTopics_RoutesSelectionsToMatchingTargetProperty(string pageType, string targetProperty)
    {
        var widget = new JObject
        {
            ["type"] = "BDO.Widget.TrendingTopicsCarousel",
            ["variants"] = new JArray(new JObject
            {
                ["properties"] = new JObject
                {
                    ["trendingTopicsPageType"] = pageType,
                    ["pages"] = new JArray(new JObject { ["nodeGuid"] = Guid.NewGuid() })
                }
            })
        };
        var migration = new InsightPageWidgetMigration();

        var result = await migration.MigrateWidget(new WidgetIdentifier("BDO.Widget.TrendingTopicsCarousel", Guid.NewGuid()), widget, new WidgetMigrationContext(7));

        Assert.Equal("BDO.Widget.TrendingTopicsCarousel", result.Value!["type"]!.Value<string>());
        Assert.Single((JArray)result.Value["variants"]![0]!["properties"]![targetProperty]!);
        Assert.Equal(typeof(InsightPageSelectorMigration), result.PropertyMigrations[targetProperty]);
    }

    [Fact]
    public async Task AnimationCard_RenamesWidgetAndConvertsSelectedPages()
    {
        var nodeGuid = Guid.NewGuid();
        var widget = new JObject
        {
            ["type"] = "BDO.Widget.InsightAnimationCard",
            ["variants"] = new JArray(new JObject
            {
                ["properties"] = new JObject
                {
                    ["insightCards"] = new JArray(new JObject { ["nodeGuid"] = nodeGuid })
                }
            })
        };
        var migration = new InsightPageWidgetMigration();
        var result = await migration.MigrateWidget(new WidgetIdentifier("BDO.Widget.InsightAnimationCard", Guid.NewGuid()), widget, new WidgetMigrationContext(7));
        var selectorMigration = new InsightPageSelectorMigration(new PassThroughGuidContext());
        var selectedPages = result.Value!["variants"]![0]!["properties"]!["insightCards"];
        var converted = await selectorMigration.MigrateWidgetProperty("insightCards", selectedPages, new WidgetPropertyMigrationContext(7, null));

        Assert.Equal("BDO.Widget.AnimatedTileCarouselWidget", result.Value["type"]!.Value<string>());
        Assert.Equal(nodeGuid, converted.Value![0]!["Identifier"]!.Value<Guid>());
        Assert.False(converted.AllowDefaultMigrations);
    }

    private sealed class PassThroughGuidContext : ISpoiledGuidContext
    {
        public Guid EnsureNodeGuid(Guid nodeGuid, int siteId) => nodeGuid;
        public Guid EnsureNodeGuid(Guid nodeGuid, int siteId, int nodeId) => nodeGuid;
        public Guid EnsureDocumentGuid(Guid documentGuid, int siteId, int nodeId, int documentId) => documentGuid;
        public Guid? GetNodeGuid(int nodeId, int siteId) => null;
    }
}
