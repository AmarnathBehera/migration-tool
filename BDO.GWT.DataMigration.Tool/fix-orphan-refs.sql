-- Global fix for broken content-item references that crash the XbyK admin selector
-- ("Workspace ID must be specified for reusable content items").
-- Applies to EVERY channel-10 web page. Strict rule per stored Identifier in every reference-array JSON
-- field (value starts with '[' and contains "Identifier"):
--   * target has a channel-10 web page                 -> keep
--   * target is reusable WITH a workspace              -> keep (CopyInsight, attachments, etc.)
--   * target is a ch1/other duplicate (no ch10 page, NULL workspace) with a ch10 twin (type+name) -> re-point
--   * dangling GUID or orphan without a twin           -> drop
-- Also syncs CMS_ContentItemReference rows. Excludes Page Builder widget/template columns. Idempotent.
SET QUOTED_IDENTIFIER ON;
SET XACT_ABORT ON;
BEGIN TRANSACTION;

-- Common-data rows of all channel-10 web pages.
IF OBJECT_ID('tempdb..#src') IS NOT NULL DROP TABLE #src;
SELECT DISTINCT cd.ContentItemCommonDataID AS CommonID
INTO #src
FROM CMS_WebPageItem w
JOIN CMS_ContentItem ci ON ci.ContentItemID = w.WebPageItemContentItemID
JOIN CMS_ContentItemCommonData cd ON cd.ContentItemCommonDataContentItemID = ci.ContentItemID
WHERE w.WebPageItemWebsiteChannelID = 10;
CREATE INDEX IX_src ON #src (CommonID);

-- Channel-10 web-page twin: one per (content type, trimmed display name).
IF OBJECT_ID('tempdb..#twin') IS NOT NULL DROP TABLE #twin;
WITH tw AS (
    SELECT t.ContentItemID AS TwinID, t.ContentItemContentTypeID AS Cls,
           LTRIM(RTRIM(tl.ContentItemLanguageMetadataDisplayName)) AS Nm,
           LOWER(CONVERT(nvarchar(36), t.ContentItemGUID)) AS TwinGuid,
           ROW_NUMBER() OVER (PARTITION BY t.ContentItemContentTypeID, LTRIM(RTRIM(tl.ContentItemLanguageMetadataDisplayName)) ORDER BY t.ContentItemID) rn
    FROM CMS_ContentItem t
    JOIN CMS_ContentItemLanguageMetadata tl ON tl.ContentItemLanguageMetadataContentItemID = t.ContentItemID AND tl.ContentItemLanguageMetadataContentLanguageID = 1
    JOIN CMS_WebPageItem w ON w.WebPageItemContentItemID = t.ContentItemID AND w.WebPageItemWebsiteChannelID = 10
)
SELECT TwinID, Cls, Nm, TwinGuid INTO #twin FROM tw WHERE rn = 1;
CREATE INDEX IX_twin ON #twin (Cls, Nm);

-- Orphan targets referenced (reference rows) by ch10 pages: no ch10 page + NULL workspace.
IF OBJECT_ID('tempdb..#map') IS NOT NULL DROP TABLE #map;
SELECT DISTINCT tci.ContentItemID AS OrphanID, tw.TwinID
INTO #map
FROM CMS_ContentItemReference r
JOIN #src s ON s.CommonID = r.ContentItemReferenceSourceCommonDataID
JOIN CMS_ContentItem tci ON tci.ContentItemID = r.ContentItemReferenceTargetItemID
JOIN CMS_ContentItemLanguageMetadata lm ON lm.ContentItemLanguageMetadataContentItemID = tci.ContentItemID AND lm.ContentItemLanguageMetadataContentLanguageID = 1
LEFT JOIN #twin tw ON tw.Cls = tci.ContentItemContentTypeID AND tw.Nm = LTRIM(RTRIM(lm.ContentItemLanguageMetadataDisplayName)) COLLATE DATABASE_DEFAULT
WHERE tci.ContentItemWorkspaceID IS NULL
  AND NOT EXISTS (SELECT 1 FROM CMS_WebPageItem w WHERE w.WebPageItemContentItemID = tci.ContentItemID AND w.WebPageItemWebsiteChannelID = 10);
CREATE INDEX IX_map ON #map (OrphanID);

UPDATE r SET r.ContentItemReferenceTargetItemID = m.TwinID
FROM CMS_ContentItemReference r
JOIN #src s ON s.CommonID = r.ContentItemReferenceSourceCommonDataID
JOIN #map m ON m.OrphanID = r.ContentItemReferenceTargetItemID
WHERE m.TwinID IS NOT NULL;

;WITH d AS (
    SELECT r.ContentItemReferenceID,
           ROW_NUMBER() OVER (PARTITION BY r.ContentItemReferenceSourceCommonDataID, r.ContentItemReferenceTargetItemID ORDER BY r.ContentItemReferenceID) rn
    FROM CMS_ContentItemReference r JOIN #src s ON s.CommonID = r.ContentItemReferenceSourceCommonDataID
)
DELETE r FROM CMS_ContentItemReference r JOIN d ON d.ContentItemReferenceID = r.ContentItemReferenceID WHERE d.rn > 1;

DELETE r
FROM CMS_ContentItemReference r
JOIN #src s ON s.CommonID = r.ContentItemReferenceSourceCommonDataID
JOIN #map m ON m.OrphanID = r.ContentItemReferenceTargetItemID
WHERE m.TwinID IS NULL;

-- Text columns to sanitize: CMS_ContentItemCommonData + every coupled table (has ContentItemDataCommonDataID),
-- excluding Page Builder widget/template columns.
IF OBJECT_ID('tempdb..#cols') IS NOT NULL DROP TABLE #cols;
SELECT c.TABLE_NAME AS Tbl,
       CASE WHEN c.TABLE_NAME = 'CMS_ContentItemCommonData' THEN 'ContentItemCommonDataID' ELSE 'ContentItemDataCommonDataID' END AS KeyCol,
       c.COLUMN_NAME AS Col,
       ROW_NUMBER() OVER (ORDER BY c.TABLE_NAME, c.ORDINAL_POSITION) AS Ord
INTO #cols
FROM INFORMATION_SCHEMA.COLUMNS c
WHERE c.DATA_TYPE IN ('nvarchar','varchar')
  AND (c.CHARACTER_MAXIMUM_LENGTH = -1 OR c.CHARACTER_MAXIMUM_LENGTH >= 100)
  AND c.COLUMN_NAME NOT LIKE '%VisualBuilder%'
  AND c.COLUMN_NAME NOT LIKE '%Widget%'
  AND c.COLUMN_NAME NOT LIKE '%TemplateConfig%'
  AND (c.TABLE_NAME = 'CMS_ContentItemCommonData'
       OR c.TABLE_NAME IN (SELECT TABLE_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE COLUMN_NAME = 'ContentItemDataCommonDataID'));

DECLARE @i int = 1, @max int = (SELECT MAX(Ord) FROM #cols), @tbl sysname, @key sysname, @col sysname;
WHILE @i <= @max
BEGIN
    SELECT @tbl = Tbl, @key = KeyCol, @col = Col FROM #cols WHERE Ord = @i;
    DECLARE @sql nvarchar(max) = N'
    UPDATE t SET t.' + QUOTENAME(@col) + N' = j.newjson
    FROM ' + QUOTENAME(@tbl) + N' t
    JOIN #src s ON s.CommonID = t.' + QUOTENAME(@key) + N'
    CROSS APPLY (
        SELECT CASE WHEN COUNT(*) = 0 THEN ''[]''
               ELSE ''['' + STRING_AGG(''{"Identifier":"'' + val + ''"}'', '','') + '']'' END AS newjson
        FROM (
            SELECT DISTINCT val FROM (
                SELECT CASE
                    WHEN tci.ContentItemID IS NULL THEN NULL
                    WHEN EXISTS (SELECT 1 FROM CMS_WebPageItem w WHERE w.WebPageItemContentItemID = tci.ContentItemID AND w.WebPageItemWebsiteChannelID = 10) THEN LOWER(oj.Ident)
                    WHEN tci.ContentItemWorkspaceID IS NOT NULL THEN LOWER(oj.Ident)
                    WHEN tw.TwinGuid IS NOT NULL THEN tw.TwinGuid
                    ELSE NULL END AS val
                FROM OPENJSON(t.' + QUOTENAME(@col) + N') WITH (Ident nvarchar(50) ''$.Identifier'') oj
                LEFT JOIN CMS_ContentItem tci ON tci.ContentItemGUID = TRY_CONVERT(uniqueidentifier, oj.Ident)
                LEFT JOIN CMS_ContentItemLanguageMetadata lm ON lm.ContentItemLanguageMetadataContentItemID = tci.ContentItemID AND lm.ContentItemLanguageMetadataContentLanguageID = 1
                LEFT JOIN #twin tw ON tw.Cls = tci.ContentItemContentTypeID AND tw.Nm = LTRIM(RTRIM(lm.ContentItemLanguageMetadataDisplayName)) COLLATE DATABASE_DEFAULT
            ) a WHERE val IS NOT NULL
        ) x
    ) j
    WHERE ISJSON(t.' + QUOTENAME(@col) + N') = 1
      AND LEFT(LTRIM(t.' + QUOTENAME(@col) + N'), 1) = ''[''
      AND t.' + QUOTENAME(@col) + N' LIKE ''%Identifier%'';';
    BEGIN TRY EXEC sp_executesql @sql; END TRY BEGIN CATCH END CATCH;
    SET @i += 1;
END

-- Verify: any remaining ch10-page field JSON referencing a workspace-less no-ch10-page item?
IF OBJECT_ID('tempdb..#rc') IS NOT NULL DROP TABLE #rc;
CREATE TABLE #rc (Tbl sysname, Col sysname, TargetID int);
SET @i = 1;
WHILE @i <= @max
BEGIN
    SELECT @tbl = Tbl, @key = KeyCol, @col = Col FROM #cols WHERE Ord = @i;
    DECLARE @r nvarchar(max) = N'
    INSERT INTO #rc SELECT DISTINCT ''' + @tbl + N''', ''' + @col + N''', tci.ContentItemID
    FROM ' + QUOTENAME(@tbl) + N' t
    JOIN #src s ON s.CommonID = t.' + QUOTENAME(@key) + N'
    CROSS APPLY OPENJSON(t.' + QUOTENAME(@col) + N') WITH (Ident nvarchar(50) ''$.Identifier'') oj
    JOIN CMS_ContentItem tci ON tci.ContentItemGUID = TRY_CONVERT(uniqueidentifier, oj.Ident)
    WHERE ISJSON(t.' + QUOTENAME(@col) + N') = 1 AND LEFT(LTRIM(t.' + QUOTENAME(@col) + N'),1) = ''['' AND t.' + QUOTENAME(@col) + N' LIKE ''%Identifier%''
      AND tci.ContentItemWorkspaceID IS NULL
      AND NOT EXISTS (SELECT 1 FROM CMS_WebPageItem w WHERE w.WebPageItemContentItemID = tci.ContentItemID AND w.WebPageItemWebsiteChannelID = 10);';
    BEGIN TRY EXEC sp_executesql @r; END TRY BEGIN CATCH END CATCH;
    SET @i += 1;
END
SELECT 'Remaining orphan JSON refs' AS Metric, COUNT(*) AS Val FROM #rc;
SELECT DISTINCT Tbl, Col FROM #rc;

COMMIT TRANSACTION;
