using Microsoft.Extensions.Logging;
using Migration.Tool.Common.Enumerations;
using Migration.Tool.Extensions.DefaultMigrations;
using Migration.Tool.KXP.Api.Services.CmsClass;
using Migration.Tool.Source;
using Migration.Tool.Source.Contexts;
using Migration.Tool.Source.Model;
using Newtonsoft.Json.Linq;

namespace Migration.Tool.Extensions.CommunityMigrations;

public class BdoWidgetMigration(AssetMigration assetMigration, ModelFacade modelFacade, ILogger<BdoWidgetMigration> logger) : IWidgetMigration
{
    private static readonly Dictionary<string, string> WidgetNames = new(StringComparer.InvariantCultureIgnoreCase)
    {
        ["BDO.Widget.Accordion"] = "BDO.Widget.AccordionWidget",
        ["BDO.Widget.Faq"] = "BDO.Widget.FaqWidget",
        ["BDO.Widget.EventsWebinarsSlider"] = "BDO.Widget.EventWebinarSliderWidget"
    };

    private static readonly Dictionary<string, string[]> AssetProperties = new(StringComparer.InvariantCultureIgnoreCase)
    {
        ["BDO.Widget.Hero"] = ["backgroundImage", "mobileBackgroundImage"],
        ["BDO.Widget.FlexibleBanner"] = ["backgroundImage"],
        ["BDO.Widget.ImageWithText"] = ["imageURL", "backgroundImageURL"],
        ["BDO.Widget.InFocus"] = ["imageURL"],
        ["BDO.Widget.QuoteCard"] = ["image"]
    };

    public int Rank => 2;

    public bool ShallMigrate(WidgetMigrationContext context, WidgetIdentifier identifier) =>
        identifier.TypeIdentifier.StartsWith("BDO.Widget.", StringComparison.InvariantCultureIgnoreCase);

    public async Task<WidgetMigrationResult> MigrateWidget(WidgetIdentifier identifier, JToken? value, WidgetMigrationContext context)
    {
        if (value is not JObject widget)
        {
            throw new InvalidOperationException($"Widget '{identifier.TypeIdentifier}' has no configuration.");
        }

        if (WidgetNames.TryGetValue(identifier.TypeIdentifier, out var targetIdentifier))
        {
            widget["type"] = targetIdentifier;
        }

        var migrations = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
        foreach (var properties in widget["variants"]?.Children()
                     .Select(variant => variant["properties"]).OfType<JObject>() ?? [])
        {
            if (AssetProperties.TryGetValue(identifier.TypeIdentifier, out var assetProperties))
            {
                var site = modelFacade.SelectById<ICmsSite>(context.SiteId)
                    ?? throw new InvalidOperationException($"Source site '{context.SiteId}' was not found.");
                foreach (var propertyName in assetProperties)
                {
                    var property = properties.Property(propertyName, StringComparison.OrdinalIgnoreCase);
                    if (property?.Value.Type is JTokenType.String)
                    {
                        var sourceUrl = property.Value.Value<string>();
                        if (string.IsNullOrWhiteSpace(sourceUrl))
                        {
                            property.Value = new JArray();
                            continue;
                        }

                        Guid.TryParse(properties.Parent?.Parent?["identifier"]?.ToString(), out var variantGuid);
                        var migrationContext = new FieldMigrationContext(KsFieldDataType.Text,
                            Kx13FormControls.UserControlForText.MediaSelectionControl, property.Name,
                            new WidgetSourceObjectContext(site, identifier.InstanceIdentifier, variantGuid));
                        var result = await assetMigration.MigrateValue(sourceUrl, migrationContext);
                        if (!result.Success)
                        {
                            logger.LogWarning("Widget '{WidgetType}' asset property '{PropertyName}' could not be migrated; keeping the widget with an empty asset selection", identifier.TypeIdentifier, property.Name);
                            property.Value = new JArray();
                            continue;
                        }

                        property.Value = result.MigratedValue is string serialized
                            ? JToken.Parse(serialized)
                            : result.MigratedValue == null ? new JArray() : JToken.FromObject(result.MigratedValue);
                    }
                }
            }

            if ((identifier.TypeIdentifier.Equals("BDO.Widget.ImageWithText", StringComparison.InvariantCultureIgnoreCase)
                || identifier.TypeIdentifier.Equals("BDO.Widget.QuoteCard", StringComparison.InvariantCultureIgnoreCase))
              && properties.Property("buttonURL", StringComparison.OrdinalIgnoreCase) is { Value.Type: JTokenType.String } button)
            {
                properties["urlSource"] = "manual";
                properties["manualUrl"] = button.Value.DeepClone();
                button.Value = new JArray();
            }

            if (identifier.TypeIdentifier.Equals("BDO.Widget.TabAccordion", StringComparison.InvariantCultureIgnoreCase))
            {
                var targetProperty = properties.Value<string>("pageType") switch
                {
                    "Service" => "servicePages",
                    "Microsite" => "micrositePages",
                    _ => "industryPages"
                };
                MoveSelector(properties, "pages", targetProperty);
            }

            if (identifier.TypeIdentifier.Equals("BDO.Widget.TiledLinkSection", StringComparison.InvariantCultureIgnoreCase))
            {
                var targetProperty = properties.Value<string>("tilePageType") == "Service"
                    ? "tilePagesService"
                    : "tilePagesIndustry";
                MoveSelector(properties, "tilePages", targetProperty);
            }

            foreach (var property in properties.Properties())
            {
                if (property.Value is not JArray { Count: > 0 } selections)
                {
                    continue;
                }

                if (selections.All(selection => selection is JObject selected && selected.GetValue("nodeGuid", StringComparison.OrdinalIgnoreCase) != null))
                {
                    migrations[property.Name] = typeof(InsightPageSelectorMigration);
                }
                else if (selections.All(selection => selection is JObject selected && selected.GetValue("fileGuid", StringComparison.OrdinalIgnoreCase) != null))
                {
                    migrations[property.Name] = typeof(WidgetFileMigration);
                }
            }
        }

        return new WidgetMigrationResult(widget, migrations);
    }

    private static void MoveSelector(JObject properties, string source, string target)
    {
        if (properties.Property(source, StringComparison.OrdinalIgnoreCase) is { } sourceProperty)
        {
            var selections = sourceProperty.Value;
            sourceProperty.Remove();
            properties[target] = selections;
        }
    }
}
