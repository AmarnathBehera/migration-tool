# Content Item Migration Overview

This guide brings together the main concepts, execution path, configuration, and customization points for migrating content items to Xperience by Kentico (XbyK). It focuses on how to find the right part of the repository; the linked guides and samples contain implementation details.

## What Is Migrated

The `--pages` command migrates source pages and their associated content. Depending on the target content type and configuration, page data can become a website channel page or a reusable content item in the Content hub. Page Builder data, relationships, and attachments are handled as part of the page migration flow.

Other source data can also be represented as reusable content items. For example, module classes and custom table data can be configured for Content hub migration. See [Supported Data](Supported-Data.md) for the supported source entities and limitations.

## Page Migration Flow

1. The CLI invokes the page migration command when `--pages` is selected. See [CLI migration commands](../Migration.Tool.CLI/README.md#migrate-command-parameters) for command prerequisites and examples.
2. `MigratePagesCommandHandler` orchestrates page migration and delegates mapping to `ContentItemMapper`.
3. `ContentItemMapper` gathers source page information and invokes registered `ContentItemDirectorBase` customizations to produce a migration directive.
4. A directive can drop the source item, select a target type, or alter how the page is represented. The mapper then builds the target content item and its language-specific data, fields, relationships, and Page Builder data.
5. The migration tool imports the generated models into the target instance. Some relationships and Page Builder updates may be deferred until related items are available.

The main implementation files are [MigratePagesCommandHandler.cs](../KVA/Migration.Tool.Source/Handlers/MigratePagesCommandHandler.cs), [ContentItemMapper.cs](../KVA/Migration.Tool.Source/Mappers/ContentItemMapper.cs), and [ContentItemDirectorBase.cs](../KVA/Migration.Tool.Source/Mappers/ContentItemMapperDirectives/ContentItemDirectorBase.cs).

## Choosing a Migration Approach

- **Configuration-driven:** Configure source classes for reusable Content hub migration with `ConvertClassesToContentHub` in [appsettings.json](../Migration.Tool.CLI/appsettings.json). This is appropriate when the target content type and fields already fit the source data.
- **Class mapping:** Use an `IClassMapping` when target content types or fields need to be renamed, merged, split, or otherwise remodeled. See [Class Mappings](customization/Class-Mappings.md).
- **Content item director:** Use a `ContentItemDirectorBase` when a decision depends on an individual item, its hierarchy, or linked pages. Directors can select a target type, drop an item, convert a page to a widget, or define item relationships.
- **Field or widget migration:** Use field and widget migrations to transform values or Page Builder structures. See [Data Transformation Extensions](customization/Data-Transformation-Extensions.md) for the extension-point overview.

These approaches can be combined. For example, configuration can make a content type reusable while a director handles the item-specific relationships.

## Content Item Director Actions

Implement directors in `Migration.Tool.Extensions` by deriving from `ContentItemDirectorBase`, then register each implementation in [ServiceCollectionExtensions.cs](../Migration.Tool.Extensions/ServiceCollectionExtensions.cs). The action provider exposes operations including:

- `OverrideTargetType` to choose a target content type.
- `Drop` to skip migration of an item.
- `AsWidget` to convert a page into a widget.
- `LinkChildren` to create child content item references.
- `OverridePageTemplate`, `RegenerateUrlPath`, and `MapEditableArea` to adjust page behavior.

Linked-page-specific behavior is implemented by overriding `DirectLinkedNode`, with actions to materialize, drop, or store a reference to a linked page. The detailed guide, including examples and registration instructions, is [Content Item Directors](customization/Content-Item-Directors.md).

## Code and Documentation Map

| Area                                         | Location                                                                                                                        |
| -------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------- |
| Page migration orchestration                 | [MigratePagesCommandHandler.cs](../KVA/Migration.Tool.Source/Handlers/MigratePagesCommandHandler.cs)                            |
| Content item mapping and director invocation | [ContentItemMapper.cs](../KVA/Migration.Tool.Source/Mappers/ContentItemMapper.cs)                                               |
| Director contract                            | [ContentItemDirectorBase.cs](../KVA/Migration.Tool.Source/Mappers/ContentItemMapperDirectives/ContentItemDirectorBase.cs)       |
| Available director actions                   | [IContentItemActionProvider.cs](../KVA/Migration.Tool.Source/Mappers/ContentItemMapperDirectives/IContentItemActionProvider.cs) |
| Customization registration                   | [ServiceCollectionExtensions.cs](../Migration.Tool.Extensions/ServiceCollectionExtensions.cs)                                   |
| Sample custom directors                      | [CommunityMigrations](../Migration.Tool.Extensions/CommunityMigrations/)                                                        |
| Detailed director guide                      | [Content Item Directors](customization/Content-Item-Directors.md)                                                               |
| Migration configuration                      | [CLI appsettings.json](../Migration.Tool.CLI/appsettings.json)                                                                  |
| Source data support and limitations          | [Supported Data](Supported-Data.md)                                                                                             |
