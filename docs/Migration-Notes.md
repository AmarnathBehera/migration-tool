# Migration notes

Status recorded on 2026-10-07. Scope: KX13 source site `GWT_TST2` to Xperience
by Kentico, with media library files migrated as Content Hub assets.

## Media migration configuration

The controlling configuration is `Migration.Tool.CLI/appsettings.json`:

- `KxCmsDirPath`: `C:\Amarnath\BDOK13\CMS`.
- `XbyKDirPath`: `C:\Amarnath\BDO_XBYK\GWT.XbyK.Web\BDO.GWT.GLOBAL`.
  This must be the running web application directory, not the solution root.
- `MigrateMediaToMediaLibrary`: `false` (Content Hub content item assets).
- `MigrateOnlyMediaFileInfo`: `false` (include physical binaries).
- `LegacyFlatAssetTree`: `false` (preserve the content folder hierarchy).
- Source site allowlist and target channel mapping: `GWT_TST2`.
- Kentico package references remain at `31.7.0`, matching the target.

Source settings were checked: `CMSMediaLibrariesFolder` is empty and
`CMSUseMediaLibrariesSiteFolder` is true. Expected source roots are:

```text
C:\Amarnath\BDOK13\CMS\GWT_TST2\media\NewBDO2
C:\Amarnath\BDOK13\CMS\GWT_TST2\media\bdo
C:\Amarnath\BDOK13\CMS\GWT_TST2\media\NewBDO
```

Each `Media_File.FilePath` is resolved relative to its library root; preserve
its subfolders and filename when restoring source files.

## Fixes retained

- `ModelFacade.GetMigratedSites` honors both site inclusion and exclusion.
- `MediaFileMigratorToContentItem` limits media to those selected source sites.
- `MigrateMediaLibrariesCommand` depends on sites rather than also requiring
  user/custom-module reruns. Existing target prerequisites are reused for the
  media-only command below; missing authors fall back to the default admin.
- Unsorted attachment migration also respects selected sites. Null/empty
  attachment binaries are reported without aborting the remaining migration.
- Existing BDO page/widget conversions retain site-aware media resolution,
  stable widget/variant identity, and Content Hub reference conversion.

The broken-image investigation found that the original `XbyKDirPath` pointed
to the solution root. The CLI uses this value as its working directory and
Kentico web application physical path, so it wrote files into an asset folder
outside the running application's storage root. Correcting this configuration
lets the importer write binaries into:

```text
C:\Amarnath\BDO_XBYK\GWT.XbyK.Web\BDO.GWT.GLOBAL\assets\contentitems
```

One image was manually copied as a diagnostic probe only. That probe's HTTP
check timed out and did not establish success. No bulk manual asset copy was
performed. After the corrected-path rerun, the user confirmed that many images
render correctly in the CMS; this is a spot check, not full asset validation.

## Results and outstanding work

| Run                                | Imported events | Failed events | Interpretation                                                                                   |
| ---------------------------------- | --------------: | ------------: | ------------------------------------------------------------------------------------------------ |
| 2026-10-07, solution-root path     |             862 |           794 | Imported records/binaries, but binaries were outside the running app root.                       |
| 2026-10-07, corrected web-app path |             254 |           367 | Partial rerun; no normal completion marker and no migration process running at the status check. |

All counted failures above had missing-source-file errors. Prior read-only DB
validation found 862 `Legacy.MediaFile` items with valid asset JSON and no blank
asset rows. Do not add the two runs' import counts: reruns update existing items.
The corrected app folder had 257 files, including pre-existing/probe files, so
that disk count is not the number imported by the latest rerun.

The latest partial log spans about 4 hours 23 minutes. Gaps ending in successful
imports averaged about 61 seconds, compared with about 1 second for missing-file
failures. Imports are sequential; existing logs cannot distinguish database,
file I/O, and Kentico internal processing time. No performance fix is claimed.

Remaining tasks: restore missing source binaries, complete the media-only run,
check its per-item errors and completion marker, reconcile target records with
physical files, and verify representative CMS previews/downloads. Empty folders
alone are not evidence that their source files were imported successfully.

Back up the target database and web app asset folder before rerunning. Keep
existing Content Hub items; deletion is unnecessary and can break page/widget
references. Run from `Migration.Tool.CLI` after building the current code:

```powershell
dotnet run -c Debug --no-build -- migrate --nowait --media-libraries --bypass-dependency-check 2>&1 | Tee-Object -FilePath logs/media-only.log
```

The dependency bypass assumes sites/languages/workspace and other target
prerequisites already exist. It is not a clean-install command. Do not include
pages, forms, categories, users, or custom modules in a media-only retry.

Detailed logs remain outside this repository in the target application's
`logs` directory; the earlier solution-root logs are also retained. A zero
process exit code alone does not prove all assets migrated successfully.

## Other migration changes preserved

The following earlier work remains in code and is not required to run media-only:

- BDO widget identifier changes, page/media selector conversion, explicit GUID
  parsing, all-variant processing, and preserving existing target references.
- Page-path filtering, optional page reimport, configurable editable-area
  consolidation into `mainArea`, and processing widgets without source API
  discovery. Reimport can overwrite target page edits.
- Empty-document skips, guarded redirect processing, undeclared `DocumentName`
  cleanup, and target-aware content-reference array conversion. Numeric IDs and
  free-text labels still need explicit domain mappings; they are not guessed.
- Source-site/target-channel mapping, domain overrides, preservation of existing
  language GUIDs, and user-selected reCAPTCHA v3 for `GWT_TST2`.
- Custom-module and form migration repairs, reusable editable-area mapping APIs,
  and their existing tests/documentation.

The 2026-10-06 focused page validation reported no page import failures and seven
widgets under `mainArea` on `/Microsites/my-test`. This does not establish full
page/form/category migration success. Automatic `BDO.CopyInsight` creation stays
disabled; native article-body remodeling remains a separate decision.

## Cleanup

Removed the obsolete DancingGoat-only CLI `ContentTypeMapper` and its registration,
stale registration comments, generated repository log files, and three historical
session status documents. Replaced those documents with this note. Kept functional
BDO extensions, shared migration APIs, tests, operational configuration, detailed
target logs, and all physical source/target assets. Generated repository log
folders are now ignored by Git. No database deletion or migration run is part of
this cleanup.

Validation: the CLI build passed with zero warnings/errors before the final
unused-import cleanup; the subsequent focused test command rebuilt the CLI and
dependencies and passed all 25 media-helper, Insight widget/page-scope, and
editable-area tests. No live migration or full asset-delivery audit was run
as part of cleanup.
