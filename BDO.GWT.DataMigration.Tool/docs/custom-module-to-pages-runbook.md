# Runbook: Custom Module → XbyK Page Tree Migration (BDO Metadata & Deal Cards)

Repeatable process for migrating **KX13 custom module data** (rows that are NOT in `CMS_Tree`)
into an **Xperience by Kentico website page tree**, using the project's prefabricated content types.

## When to use this

Use this when source data lives in **custom module tables** (e.g. `BDOMetadata_*`, `BDODealCards_*`)
rather than the KX13 page tree. The stock Kentico Migration Tool cannot turn these rows into web
pages — its custom-module path only bulk-copies data or creates **reusable** content-hub items
(`ContentItemIsReusable = true`, no `WebPageItemModel`). To get a browsable page tree you must
synthesize the `CMS_WebPageItem` hierarchy directly.

Do NOT use this for normal page-tree content — that goes through `migrate --pages` with class mappings.

## Environment (this project)

- Server: `(localdb)\MSSQLLocalDB`, auth `sa` / `Admin!123`, sqlcmd flags `-N -C -W`
- Source DB (KX13): `BDO-DB-GWT-TST-EUR`, migrated site `GWT_TST` = `SiteID 1`
- Target DB (XbyK): `bdo-db-gwt-xbyk-dev-local-25082026`
- Target channel: `XbyK_TST` = `WebsiteChannelID 10`; default language `en-GB` = `ContentLanguageID 1`; workspace `KenticoDefault` = `WorkspaceID 1`
- Seed scripts (idempotent) live at repo root:
  - `seed-bdo-metadata-pages.sql`
  - `seed-bdo-dealcards-pages.sql`

## Step 1 — Inventory source & target

1. Source module tables + columns:
   ```sql
   SELECT TABLE_NAME, COLUMN_NAME, DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS
   WHERE TABLE_NAME LIKE 'BDOMetadata[_]%' ORDER BY TABLE_NAME, ORDINAL_POSITION;
   ```
2. Target content types (class name → table name → GUID, and `ClassWebPageHasUrl`):
   ```sql
   SELECT ClassName, ClassTableName, ClassGUID, ClassWebPageHasUrl FROM CMS_Class
   WHERE ClassName LIKE 'BDO.DealCard%';
   ```
3. Target coupled-table columns (map only real columns; most fields are `Name`/`CodeName`/`Title`/`Language`):
   ```sql
   SELECT TABLE_NAME, COLUMN_NAME, DATA_TYPE, IS_NULLABLE FROM INFORMATION_SCHEMA.COLUMNS
   WHERE TABLE_NAME IN ('BDO_DealCardDealType','BDO_DealCardIndustry_1', ...);
   ```
4. Read the generated content type field lists in
   `C:\Work\GWT\GWT.XbyK.Web\BDO.GWT.Core\ContentTypes\PageContentTypes\<Type>\<Type>.generated.cs`.
   The coupled table is often much leaner than the generated C# (many props are shared/common data).

> WARNING: class table name may not match the class name. `BDO.DealCardIndustry` → `BDO_DealCardIndustry_1`
> (the plain `BDO_DealCardIndustry` table belongs to `BDO.DealCardCommon`). Always resolve via `CMS_Class`.

## Step 2 — Confirm scope with the user (do not guess)

- **Site scope**: module rows are site-bound. Default to the migrated site only (`SiteID = 1`).
  Other sites (BDOWOW, Demo, E2E_Performance, GWTExport, …) are usually test/demo noise.
- **Culture scope**: rows carry a `*CultureCode`. Confirm which culture to keep (Metadata used `en-GB`,
  Deals used `en-US`). Check distribution first — the requested culture may have few/zero rows:
  ```sql
  SELECT IndustryCultureCode, COUNT(*) FROM BDODealCards_Industry
  WHERE IndustrySiteID = 1 GROUP BY IndustryCultureCode;
  ```
- **Folder type**: if there is no dedicated `*Folder` content type, ask which existing type to reuse
  for container nodes (Deals reused the root type `BDO.DealCards`).
- Global/no-culture tables (e.g. `BDODealCards_Global*`, `BDODealCards_Range`) have no culture filter.

## Step 3 — Build the pages (what each row needs)

For every page (root, folder, item) create this full set inside ONE transaction (`SET XACT_ABORT ON`):

1. `CMS_ContentItem` — `ContentItemGUID`, unique `ContentItemName`, `ContentItemContentTypeID`,
   `ContentItemWorkspaceID = 1`, `ContentItemChannelID = NULL`, `IsReusable = 0`.
2. `CMS_WebPageItem` — `WebPageItemParentID`, `WebPageItemTreePath`, `WebPageItemWebsiteChannelID = 10`,
   `WebPageItemContentItemID`, `WebPageItemName`, `WebPageItemOrder`.
3. `CMS_ContentItemCommonData` — `ContentLanguageID = 1`, `VersionStatus = 2` (published), `IsLatest = 1`.
4. Coupled table (e.g. `BDO_EventType`) — `ContentItemDataCommonDataID`, `ContentItemDataGUID`, mapped fields.
5. `CMS_ContentItemLanguageMetadata` — `DisplayName`, `LatestVersionStatus = 2`, `ContentLanguageID = 1`,
   `CreatedWhen`/`ModifiedWhen`/`VersionTimestamp`.

### Field mapping conventions (match the generated content types)
- Item types: source `{Prefix}Name` → `Name`, `{Prefix}CodeName` → `CodeName`.
- Folder types: literal folder title → `Title`.
- `EventType`: also `EventTypeCallToActionText` → `Calltoactiontext`.
- `TestimonialClientTypes`: `ClientTypeName` → `ClientType` (no `Name` column); `TestimonialIndustries`:
  `IndustryName` → `Name`, plus `ClientType` (no `CodeName` column).
- Deal card item types: `Name`, `CodeName`, and `[Language]` (reserved word — always bracket it).
  Global (no-culture) rows use a literal `Language = 'en-US'`; culture-bound rows use the filtered culture.
- `DealCardRange`: `RangeValue` → `Name`, `RangeCodeName` → `CodeName`, order by `RangeOrder`.
- Reference/media/asset fields: not applicable here (module rows are flat scalars).

## Step 4 — Idempotency, uniqueness & gotchas (learned the hard way)

- **Idempotent rebuild**: at the top, delete the existing subtree (`WebPageItemTreePath = @rootPath
  OR LIKE @rootPath + '/%'`) across all related tables BEFORE inserting. Re-running then updates cleanly.
- **Global uniqueness** `IX_CMS_ContentItem_ContentItemName` and `IX_CMS_WebPageItem_WebPageItemName_Unique`
  are channel-wide. Namespace internal names: `BDOMetadata-<Class>-<CodeName>-<rownum>` /
  `DealCards-<Class>-<CodeName>-<rownum>`. Keep the human-readable value only in `DisplayName`.
- **Duplicate source names**: even within one culture, `CodeName` can repeat — always append a row-number
  suffix to the internal name and tree path.
- **NOT NULL SQL defaults are bypassed by explicit NULLs** — supply real values for required columns.
- **`Language` / other reserved words** must be bracketed in dynamic SQL: `[Language]`.
- **`sqlcmd` flag conflicts**: `-W` conflicts with `-y`; `-h -1` conflicts with `-y 0`. For raw XML dumps
  use `-y 0 -o file.xml` (no `-W`).
- **Run scripts from a file** (`sqlcmd -i script.sql`), not pasted heredocs — the terminal mangles long
  multi-line here-strings.
- Folder/item classes with `ClassWebPageHasUrl = 0` still work as tree nodes (no URL path, which is fine
  for non-routable metadata/deal folders).

## Step 5 — Verify

```sql
-- Count by content type
SELECT cl.ClassName, COUNT(*) AS PageCount
FROM CMS_WebPageItem w
JOIN CMS_ContentItem ci ON ci.ContentItemID = w.WebPageItemContentItemID
JOIN CMS_Class cl ON cl.ClassID = ci.ContentItemContentTypeID
WHERE w.WebPageItemWebsiteChannelID = 10 AND w.WebPageItemTreePath LIKE '/BDO-Metadata%'
GROUP BY cl.ClassName ORDER BY cl.ClassName;

-- Confirm no null Name/CodeName, display names set, all published (LatestVersionStatus = 2)
```
Cross-check counts against the filtered source counts (site + culture). Empty folders are expected when
the chosen culture has zero source rows (e.g. Deals `en-US` had 0 DealType / 0 SubIndustry).

## Results captured so far

| Migration | Root path | Filter | Pages |
|---|---|---|---|
| BDO Metadata | `/BDO-Metadata` | `SiteID=1` + `en-GB` | 93 (1 root + 14 folders + 78 items) |
| BDO Deal Cards | `/BDO-Deal-Cards` | `SiteID=1` + `en-US`; Global/Range unfiltered | 29 (1 root + 7 folders + 21 items) |

## Reference: content type field shapes

- Metadata items: `Name`, `CodeName` (except `EventType` +`Calltoactiontext`;
  `TestimonialClientTypes` = `ClientType`+`CodeName`; `TestimonialIndustries` = `Name`+`ClientType`).
- Metadata folders: `Title`.
- Deal card items: `Name`, `CodeName`, `[Language]` (`DealCardRange` has no `Language`).
- Deal card folders/root: `BDO.DealCards` (no extra coupled fields).
