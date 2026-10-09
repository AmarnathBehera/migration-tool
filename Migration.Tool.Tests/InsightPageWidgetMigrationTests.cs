using Migration.Tool.Common;
using Migration.Tool.Common.Helpers;
using Migration.Tool.Common.Services;
using Migration.Tool.Extensions.CommunityMigrations;
using Migration.Tool.Extensions.DefaultMigrations;
using Migration.Tool.KXP.Api.Services.CmsClass;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json.Linq;

namespace Migration.Tool.Tests;

public class InsightPageWidgetMigrationTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void CommandParser_AcceptsUnattendedMigrationFlags(bool rerun)
    {
        var arguments = new Queue<string>(new[]
        {
            "migrate", "--nowait", "--sites", "--custom-modules", "--users",
            "--page-types", "--pages", "--categories", "--media-libraries", "--forms"
        });
        if (rerun)
        {
            arguments.Enqueue("--bypass-dependency-check");
        }
        var bypassDependencyCheck = false;

        var (masterCommand, commands) = new CommandParser().Parse(arguments, ref bypassDependencyCheck);

        Assert.NotNull(masterCommand);
        Assert.Equal(9, commands.Count);
        Assert.Contains(commands, command => command is MigratePagesCommand);
        Assert.Contains(commands, command => command is MigrateMediaLibrariesCommand);
        Assert.Contains(commands, command => command is MigrateAttachmentsCommand);
        Assert.Equal(rerun, bypassDependencyCheck);
        Assert.Empty(arguments);
    }

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

    [Theory]
    [InlineData("Accordion", "AccordionWidget")]
    [InlineData("Button", "Button")]
    [InlineData("EditorsPick", "EditorsPick")]
    [InlineData("EventsWebinarsSlider", "EventWebinarSliderWidget")]
    [InlineData("ExternalNavigation", "ExternalNavigation")]
    [InlineData("Faq", "FaqWidget")]
    [InlineData("FlexibleBanner", "FlexibleBanner")]
    [InlineData("FlexibleContent", "FlexibleContent")]
    [InlineData("Hero", "Hero")]
    [InlineData("HTML", "HTML")]
    [InlineData("ImageWithText", "ImageWithText")]
    [InlineData("InFocus", "InFocus")]
    [InlineData("InsightAnimationCard", "AnimatedTileCarouselWidget")]
    [InlineData("InsightCarousel", "InsightCarousel")]
    [InlineData("KeyContacts", "KeyContacts")]
    [InlineData("KeyTakeaways", "KeyTakeaways")]
    [InlineData("LocalNavigation", "LocalNavigation")]
    [InlineData("NewsCarousel", "NewsCarousel")]
    [InlineData("QuoteCard", "QuoteCard")]
    [InlineData("RelatedContacts", "RelatedContacts")]
    [InlineData("RelatedContentCarousel", "RelatedContentCarousel")]
    [InlineData("RichText", "RichText")]
    [InlineData("SocialMediaButtons", "SocialMediaButtons")]
    [InlineData("TabAccordion", "TabAccordion")]
    [InlineData("TiledLinkSection", "TiledLinkSection")]
    [InlineData("TrendingTopicsCarousel", "TrendingTopicsCarousel")]
    public async Task ObservedBdoWidgets_MapToTargetIdentifiers(string sourceName, string targetName)
    {
        var sourceType = "BDO.Widget." + sourceName;
        var identifier = new WidgetIdentifier(sourceType, Guid.NewGuid());
        var context = new WidgetMigrationContext(7);
        var widget = new JObject
        {
            ["type"] = sourceType,
            ["variants"] = new JArray(new JObject { ["properties"] = new JObject() })
        };
        IWidgetMigration migration = new InsightPageWidgetMigration();
        if (!migration.ShallMigrate(context, identifier))
        {
            migration = new BdoWidgetMigration(null!, null!, NullLogger<BdoWidgetMigration>.Instance);
        }

        Assert.True(migration.ShallMigrate(context, identifier));
        var result = await migration.MigrateWidget(identifier, widget, context);

        Assert.Equal("BDO.Widget." + targetName, result.Value!["type"]!.Value<string>());
    }

    [Fact]
    public async Task BdoWidget_DelegatesPageAndFileSelectorsWithoutApiDiscovery()
    {
        var widget = new JObject
        {
            ["type"] = "BDO.Widget.InsightCarousel",
            ["variants"] = new JArray(new JObject
            {
                ["properties"] = new JObject
                {
                    ["featuredInsights"] = new JArray(new JObject { ["nodeGuid"] = Guid.NewGuid() }),
                    ["backgroundImages"] = new JArray(new JObject { ["fileGuid"] = Guid.NewGuid() })
                }
            })
        };
        var migration = new BdoWidgetMigration(null!, null!, NullLogger<BdoWidgetMigration>.Instance);

        var result = await migration.MigrateWidget(new WidgetIdentifier("BDO.Widget.InsightCarousel", Guid.NewGuid()), widget, new WidgetMigrationContext(7));

        Assert.Equal(typeof(InsightPageSelectorMigration), result.PropertyMigrations["featuredInsights"]);
        Assert.Equal(typeof(WidgetFileMigration), result.PropertyMigrations["backgroundImages"]);
    }

    private sealed class PassThroughGuidContext : ISpoiledGuidContext
    {
        public Guid EnsureNodeGuid(Guid nodeGuid, int siteId) => nodeGuid;
        public Guid EnsureNodeGuid(Guid nodeGuid, int siteId, int nodeId) => nodeGuid;
        public Guid EnsureDocumentGuid(Guid documentGuid, int siteId, int nodeId, int documentId) => documentGuid;
        public Guid? GetNodeGuid(int nodeId, int siteId) => null;
    }
}
