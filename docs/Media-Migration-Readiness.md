# Page, Widget, and Media Migration Readiness

## Entry Point

The original `BDO.GWT.DataMigration.Tool/run-insight-migration.ps1` was absent from this checkout and the available script history. A replacement is now tracked at [run-insight-migration.ps1](../BDO.GWT.DataMigration.Tool/run-insight-migration.ps1). It does not recover or assume any filters from the missing original script.

Run from the repository root:

```powershell
.\BDO.GWT.DataMigration.Tool\run-insight-migration.ps1
```

This only reads databases and source files, and writes audit reports. Review the inventories before executing:

```powershell
.\BDO.GWT.DataMigration.Tool\run-insight-migration.ps1 -Execute
```

For a rerun, add `-BypassDependencyCheck`. The execution path builds the CLI, then invokes all flags together:

```text
migrate --nowait --sites --custom-modules --users --page-types --pages --categories --media-libraries --forms
```

Media libraries run before pages according to the command ranks. Registered widget migrations run within `--pages`; there is no separate widget migration flag.

`Settings.SourceSqlCommandTimeoutSeconds` applies to source database queries made through `ModelFacade` and the KX13 custom user/member/media field reads. `0` means no SQL command execution timeout; use a positive value to impose a limit. The SQL connection's `Connect Timeout` remains separate and still limits connection establishment.

## Verified Environment and Scope

Read-only preflight on 2026-10-08 confirmed:

| Setting or inventory                        | Result                                                     |
| ------------------------------------------- | ---------------------------------------------------------- |
| Source database                             | `20261001-bdo-db-gwt-tst2-eur`                             |
| Target database                             | `dev-local-xbyk-08102026`                                  |
| Target database and Kentico package version | `31.7.0`                                                   |
| Included source site                        | `GWT_TST2`                                                 |
| Page path filters                           | Empty: all paths within the included site                  |
| Excluded classes                            | None configured                                            |
| Existing page behavior                      | `ReimportExistingPages=true`                               |
| Source nodes / classes / language documents | 668 / 92 / 1,177                                           |
| Selected candidate nodes                    | 663                                                        |
| Skipped source nodes                        | One `CMS.Root`; four `BDO.Insight` nodes without documents |
| Observed widget types                       | 27: 26 BDO widgets and `Kentico.FormWidget`                |
| Media library records                       | 1,656                                                      |
| Available physical files                    | 66                                                         |
| Missing physical files                      | 1,590, all in the `BDO` library                            |

Candidate selection does not guarantee import success. Linked nodes, class mappings, language availability, field requirements, target schemas, and importer validation still control final outcomes. The script does not modify the page path filters or silently expand them.

## Widget and Area Mapping

The observed widget types have corresponding target identifier constants. Existing migrations rename Accordion to `BDO.Widget.AccordionWidget`, Faq to `BDO.Widget.FaqWidget`, EventsWebinarsSlider to `BDO.Widget.EventWebinarSliderWidget`, and InsightAnimationCard to `BDO.Widget.AnimatedTileCarouselWidget`. Other observed BDO identifiers remain unchanged. Form migration uses the built-in Kentico form widget support.

Page selectors are explicitly delegated to `InsightPageSelectorMigration`, and media-file selectors to `WidgetFileMigration`, without requiring API discovery. Asset URLs are converted for Hero, FlexibleBanner, ImageWithText, InFocus, and QuoteCard. Specialized migrations handle TrendingTopicsCarousel, TabAccordion, TiledLinkSection, and manual button URLs.

`TargetEditableAreaIdentifier=mainArea` globally merges source areas in source order. This was explicitly requested after inspecting the target views. **The target also renders other identifiers, including `topArea`, `bottomArea`, `heroSection`, `TopSection`, and `BottomSection`; merging is not proof that every widget renders in the intended target slot.** Visual validation remains required.

## Missing Media

Content hub media migration now checks the resolved physical path. Missing files produce identifying warnings and are skipped; available files continue through import. Metadata-only mode bypasses physical-file validation. The migration prints imported, missing-file, and failed counts separately.

Widget file selections and asset fields omit references to media items absent from the target Content hub, with warnings, instead of creating dangling references. Existing target media may still be referenced. Required asset fields may still fail target validation if no replacement asset exists.

The 1,590 missing files are a material content gap. Restore the source files or verify that the configured CMS directory is the intended snapshot before running if those assets should be retained. Skipping is intentional behavior, not recovery of missing binaries.

## Timeout Investigation

The 2026-10-08 execution completed sites, custom modules, page types, media libraries, users, and attachments. Media logged 66 imported, 1,590 skipped as missing, and zero import failures. The attachment step logged 413 records with empty binary data but completed. Page migration then timed out while lazily building the duplicate-node-GUID lookup in `SpoiledGuidContext`.

The old lookup used a correlated duplicate scan over `View_CMS_Tree_Joined`. It now groups GUIDs once in `CMS_Tree`/`CMS_Document` and joins the duplicates back to their rows. Read-only timings against the configured source database were approximately 0.18s for the node scan (28,329 tree rows) and 0.75s for the document scan (34,220 rows). All source model reads and KX13 custom user/member/media field reads use `SourceSqlCommandTimeoutSeconds`, configured as `0` for this migration. This avoids an arbitrary client command timeout; it cannot prevent a disconnected database, server shutdown, network failure, deadlock, server-side query limits, or other SQL errors. Unbounded query execution can wait indefinitely if the server stops responding after connection.

The failed run is partial: the earlier commands have already written to the target. `-BypassDependencyCheck` is not a rollback or a general exactly-once guarantee. Before retrying, inspect the target state and confirm rerun behavior for each completed step. The runner now writes `Status=Failed` to `summary.json` when it catches a CLI failure rather than leaving a stale `Running` status.

## Tracking and Verification

Each preflight creates `Migration.Tool.CLI/logs/runs/<timestamp>/` containing `pages.csv`, `page-types.csv`, `widget-instances.csv`, `widgets.csv`, `editable-areas.csv`, `media.csv`, `summary.json`, and `preflight.log`. Failure details are saved in `failure.json`. Reports are generated artifacts and are ignored by Git.

Execution additionally records `build.log` and `migration-run.log`, writes a timestamped console log alongside the configured migration protocol, and copies the protocol into the run directory if available. A zero process exit code is **not** treated as proof of per-item success: the status requires log review and target-data validation.

Verification completed: CLI build succeeded; 60 focused configuration, widget, selector, editable-area, and media-helper tests passed; read-only preflight completed; duplicate-GUID queries were benchmarked against the configured source database. A migration was subsequently run and partially completed as recorded above. The optimized full page migration has not yet been rerun, so end-to-end completion remains unverified.
