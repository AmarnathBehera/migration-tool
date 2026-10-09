using Migration.Tool.KXP.Api.Services.CmsClass;
using Newtonsoft.Json.Linq;

namespace Migration.Tool.Extensions.CommunityMigrations;

public sealed class BdoWidgetIdentifierMigration : IWidgetMigration
{
    private static readonly IReadOnlyDictionary<string, string> TargetIdentifiers =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["BDO.Widget.Accordion"] = "BDO.Widget.AccordionWidget",
            ["BDO.Widget.Faq"] = "BDO.Widget.FaqWidget",
            ["BDO.Widget.EventsWebinarsSlider"] = "BDO.Widget.EventWebinarSliderWidget",
            ["BDO.Widget.InsightAnimationCard"] = "BDO.Widget.AnimatedTileCarouselWidget"
        };

    public int Rank => 1;

    public bool ShallMigrate(WidgetMigrationContext context, WidgetIdentifier identifier) =>
        TargetIdentifiers.ContainsKey(identifier.TypeIdentifier);

    public Task<WidgetMigrationResult> MigrateWidget(WidgetIdentifier identifier, JToken? value, WidgetMigrationContext context)
    {
        value!["type"] = TargetIdentifiers[identifier.TypeIdentifier];
        return Task.FromResult(new WidgetMigrationResult(value, new Dictionary<string, Type>()));
    }
}