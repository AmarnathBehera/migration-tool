using CMS.ContentEngine;
using Migration.Tool.Common.Services;
using Migration.Tool.KXP.Api.Services.CmsClass;
using Newtonsoft.Json.Linq;

namespace Migration.Tool.Extensions.CommunityMigrations;

public class InsightPageWidgetMigration : IWidgetMigration
{
    private const string TrendingTopics = "BDO.Widget.TrendingTopicsCarousel";
    private const string AnimationCard = "BDO.Widget.InsightAnimationCard";
    private const string TargetAnimationCard = "BDO.Widget.AnimatedTileCarouselWidget";

    public int Rank => 1;

    public bool ShallMigrate(WidgetMigrationContext context, WidgetIdentifier identifier) =>
        string.Equals(identifier.TypeIdentifier, TrendingTopics, StringComparison.InvariantCultureIgnoreCase) ||
        string.Equals(identifier.TypeIdentifier, AnimationCard, StringComparison.InvariantCultureIgnoreCase);

    public Task<WidgetMigrationResult> MigrateWidget(WidgetIdentifier identifier, JToken? value, WidgetMigrationContext context)
    {
        var variants = value?["variants"] as JArray
            ?? throw new InvalidOperationException($"Insight page widget '{identifier.TypeIdentifier}' has no variants.");
        var migrations = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);

        if (string.Equals(identifier.TypeIdentifier, AnimationCard, StringComparison.InvariantCultureIgnoreCase))
        {
            // Insight page migration: the XbyK animation card widget has a different identifier.
            value!["type"] = TargetAnimationCard;
        }

        foreach (var properties in variants.Select(variant => variant["properties"]).OfType<JObject>())
        {
            var selector = string.Equals(identifier.TypeIdentifier, AnimationCard, StringComparison.InvariantCultureIgnoreCase)
                ? "insightCards"
                : properties.Value<string>("trendingTopicsPageType") switch
                {
                    "Service" => "servicePages",
                    "Industry" => "industryPages",
                    "Microsite" => "micrositePages",
                    "Specialty" => "specialtyPages",
                    "Insight" => "insightPages",
                    _ => "pages"
                };

            var sourceSelector = string.Equals(identifier.TypeIdentifier, AnimationCard, StringComparison.InvariantCultureIgnoreCase)
                ? "insightCards"
                : "pages";
            if (!properties.TryGetValue(sourceSelector, StringComparison.OrdinalIgnoreCase, out var pages))
            {
                properties.TryGetValue(selector, StringComparison.OrdinalIgnoreCase, out pages);
            }

            if (pages is not null && pages is not JArray && pages.Type != JTokenType.Null)
            {
                throw new InvalidOperationException($"Insight page widget '{identifier.TypeIdentifier}' has an invalid page selector '{sourceSelector}'.");
            }

            if (selector != sourceSelector && properties.Property(sourceSelector, StringComparison.OrdinalIgnoreCase) is { } sourceProperty)
            {
                sourceProperty.Remove();
            }

            properties[selector] = pages is JArray ? pages : new JArray();
            migrations[selector] = typeof(InsightPageSelectorMigration);
        }

        return Task.FromResult(new WidgetMigrationResult(value, migrations));
    }
}

public class InsightPageSelectorMigration(ISpoiledGuidContext spoiledGuidContext) : IWidgetPropertyMigration
{
    public int Rank => 1;

    public bool ShallMigrate(WidgetPropertyMigrationContext context, string propertyName) => false;

    public Task<WidgetPropertyMigrationResult> MigrateWidgetProperty(string key, JToken? value, WidgetPropertyMigrationContext context)
    {
        if (value is not JArray selectedPages)
        {
            throw new InvalidOperationException($"Insight page selector '{key}' must contain a page array.");
        }

        // Insight page migration: preserve the selected page order and resolve site-specific node GUIDs.
        var references = selectedPages.Select(page =>
        {
            if (page is not JObject selection)
            {
                throw new InvalidOperationException($"Insight page selector '{key}' contains an invalid selection.");
            }

            if (Guid.TryParse(selection.GetValue("Identifier", StringComparison.OrdinalIgnoreCase)?.ToString(), out var existingGuid) && existingGuid != Guid.Empty)
            {
                return new ContentItemReference { Identifier = existingGuid };
            }

            if (!Guid.TryParse(selection.GetValue("nodeGuid", StringComparison.OrdinalIgnoreCase)?.ToString(), out var nodeGuid) || nodeGuid == Guid.Empty)
            {
                throw new InvalidOperationException($"Insight page selector '{key}' contains an invalid or empty page GUID.");
            }

            return new ContentItemReference
            {
                Identifier = spoiledGuidContext.EnsureNodeGuid(nodeGuid, context.SiteId)
            };
        }).ToList();

        return Task.FromResult(new WidgetPropertyMigrationResult(JToken.FromObject(references), AllowDefaultMigrations: false));
    }
}
