using Migration.Tool.Common.Builders;
using Migration.Tool.Common.Model;
using Newtonsoft.Json;

namespace Migration.Tool.Tests;

public class EditableAreaRemapperTests
{
    [Fact]
    public void Remap_MergesMappedAreasIntoExistingTargetInSourceOrder()
    {
        var sectionIds = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
        var widgetId = Guid.NewGuid();
        var configuration = new EditableAreasConfiguration();
        foreach (var (identifier, sectionId) in new[]
                 {
                     ("area1", sectionIds[0]), ("main", sectionIds[1]),
                     ("area2", sectionIds[2]), ("area3", sectionIds[3])
                 })
        {
            var area = new EditableAreaConfiguration { Identifier = identifier };
            var section = new SectionConfiguration { Identifier = sectionId, TypeIdentifier = "Kentico.SingleColumnSection" };
            if (identifier == "area1")
            {
                var zone = new ZoneConfiguration { Identifier = Guid.NewGuid(), Name = "main" };
                zone.Widgets.Add(new WidgetConfiguration { Identifier = widgetId, TypeIdentifier = "Kentico.Widget.RichText" });
                section.Zones.Add(zone);
            }
            area.Sections.Add(section);
            configuration.EditableAreas.Add(area);
        }

        var mapping = new MultiClassMapping("DancingGoatCore.Home");
        mapping.MapEditableArea("area1", "main");
        mapping.MapEditableArea("area2", "main");
        mapping.MapEditableArea("area3", "main");

        EditableAreaRemapper.Remap(configuration, mapping.EditableAreaMappings);
        EditableAreaRemapper.Remap(configuration, mapping.EditableAreaMappings);

        var migrated = JsonConvert.DeserializeObject<EditableAreasConfiguration>(JsonConvert.SerializeObject(configuration))!;
        var main = Assert.Single(migrated.EditableAreas);
        Assert.Equal("main", main.Identifier);
        Assert.Equal(sectionIds, main.Sections.Select(section => section.Identifier));
        Assert.Equal(widgetId, Assert.Single(Assert.Single(main.Sections[0].Zones).Widgets).Identifier);
    }

    [Fact]
    public void Remap_LeavesUnmappedAreasAlone()
    {
        var configuration = new EditableAreasConfiguration();
        configuration.EditableAreas.Add(new EditableAreaConfiguration { Identifier = "sidebar" });

        EditableAreaRemapper.Remap(configuration, new Dictionary<string, string> { ["area1"] = "main" });

        Assert.Equal("sidebar", Assert.Single(configuration.EditableAreas).Identifier);
    }
}
