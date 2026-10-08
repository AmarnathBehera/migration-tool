-- Wire Insight reference fields to migrated pages / metadata pages.
-- Page-ref fields store '|'-separated source CMS_Tree NodeIDs; metadata-module fields store module row IDs.
-- Idempotent: fields are overwritten; reference rows are de-duped on insert.
SET QUOTED_IDENTIFIER ON;
SET XACT_ABORT ON;
BEGIN TRANSACTION;

DECLARE @channel int = 10;
DECLARE @lang int = 1;                -- en-GB

-- ---- Correlate each target Insight -> source InsightID (en-GB): NodeGUID first, then normalized path ----
IF OBJECT_ID('tempdb..#srcnode') IS NOT NULL DROP TABLE #srcnode;
SELECT t.NodeGUID, d.DocumentCulture,
       LOWER(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(t.NodeAliasPath,'/',''),'-',''),'_',''),' ',''),'(',''),')',''),'.',''),'''','')) AS NormPath,
       d.DocumentForeignKeyValue AS InsightID
INTO #srcnode
FROM [BDO-DB-GWT-TST-EUR].dbo.CMS_Tree t
JOIN [BDO-DB-GWT-TST-EUR].dbo.CMS_Class cl ON cl.ClassID = t.NodeClassID AND cl.ClassName = 'BDO.Insight'
JOIN [BDO-DB-GWT-TST-EUR].dbo.CMS_Document d ON d.DocumentNodeID = t.NodeID
  AND (d.DocumentCulture='en-GB' OR (LOWER(d.DocumentCulture)='en-gb' AND @lang=(
    SELECT WebsiteChannelPrimaryContentLanguageID FROM CMS_WebsiteChannel WHERE WebsiteChannelID=10)))
WHERE t.NodeSiteID = 1;

IF OBJECT_ID('tempdb..#corr') IS NOT NULL DROP TABLE #corr;
CREATE TABLE #corr (TargetItemID int, CommonID int, SourceInsightID int);
INSERT INTO #corr (TargetItemID, CommonID, SourceInsightID)
SELECT ci.ContentItemID, cd.ContentItemCommonDataID, sn.InsightID
FROM CMS_WebPageItem w
JOIN CMS_ContentItem ci ON ci.ContentItemID = w.WebPageItemContentItemID
JOIN CMS_Class cl ON cl.ClassID = ci.ContentItemContentTypeID AND cl.ClassName = 'BDO.Insight'
JOIN CMS_ContentItemCommonData cd ON cd.ContentItemCommonDataContentItemID = ci.ContentItemID
     AND cd.ContentItemCommonDataContentLanguageID = @lang AND cd.ContentItemCommonDataIsLatest = 1
OUTER APPLY (
    SELECT TOP 1 s.InsightID FROM #srcnode s
    WHERE s.NodeGUID = ci.ContentItemGUID
       OR s.NormPath COLLATE DATABASE_DEFAULT = LOWER(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(w.WebPageItemTreePath,'/',''),'-',''),'_',''),' ',''),'(',''),')',''),'.',''),'''',''))
    ORDER BY CASE WHEN s.NodeGUID = ci.ContentItemGUID THEN 0 ELSE 1 END,
      CASE WHEN LOWER(s.DocumentCulture)='en-gb' THEN 1 ELSE 0 END
) sn
WHERE w.WebPageItemWebsiteChannelID = @channel AND sn.InsightID IS NOT NULL;

-- ---- Source Insight reference field values for the correlated insights ----
IF OBJECT_ID('tempdb..#srcref') IS NOT NULL DROP TABLE #srcref;
SELECT bi.InsightID,
       bi.MetadataBusinessLines, bi.MetadataServiceAreas, bi.MetadataServices,
       bi.MetadataIndustryCategories, bi.MetadataIndustries, bi.MicrositePages, bi.MicrositeMasterPages,
       bi.MetadataSpecialtiesCategories, bi.MetadataSpecialtiesAreas, bi.MetadataSpecialtiesPages,
       bi.MetadataOfficeLocation, bi.MetadataBusinessIssues, bi.MetadataRssCategory, bi.ContentType,
       bi.RelevantContactPerson, bi.RelatedExternalPeople
INTO #srcref
FROM [BDO-DB-GWT-TST-EUR].dbo.BDO_Insight bi
WHERE bi.InsightID IN (SELECT SourceInsightID FROM #corr);

-- ---- Source NodeID -> target ContentItem (preserved NodeGUID = ContentItemGUID) ----
IF OBJECT_ID('tempdb..#nodemap') IS NOT NULL DROP TABLE #nodemap;
SELECT t.NodeID, ci.ContentItemID AS TargetItemID, ci.ContentItemGUID
INTO #nodemap
FROM [BDO-DB-GWT-TST-EUR].dbo.CMS_Tree t
JOIN CMS_ContentItem ci ON ci.ContentItemGUID = t.NodeGUID
WHERE EXISTS (SELECT 1 FROM CMS_WebPageItem w WHERE w.WebPageItemContentItemID = ci.ContentItemID AND w.WebPageItemWebsiteChannelID = 10);
CREATE INDEX IX_nodemap ON #nodemap (NodeID);

-- ---- Metadata-module ID -> target metadata page (matched by CodeName) ----
IF OBJECT_ID('tempdb..#modBI') IS NOT NULL DROP TABLE #modBI;
SELECT m.BusinessIssueID AS ModuleID, ci.ContentItemID AS TargetItemID, ci.ContentItemGUID
INTO #modBI
FROM [BDO-DB-GWT-TST-EUR].dbo.BDOMetadata_BusinessIssue m
JOIN BDO_Businessissues b ON b.CodeName = m.BusinessIssueCodeName COLLATE DATABASE_DEFAULT
JOIN CMS_ContentItemCommonData cd ON cd.ContentItemCommonDataID = b.ContentItemDataCommonDataID
JOIN CMS_ContentItem ci ON ci.ContentItemID = cd.ContentItemCommonDataContentItemID
WHERE EXISTS (SELECT 1 FROM CMS_WebPageItem w WHERE w.WebPageItemContentItemID = ci.ContentItemID AND w.WebPageItemWebsiteChannelID = 10);

IF OBJECT_ID('tempdb..#modRss') IS NOT NULL DROP TABLE #modRss;
SELECT m.RSSCategoryID AS ModuleID, ci.ContentItemID AS TargetItemID, ci.ContentItemGUID
INTO #modRss
FROM [BDO-DB-GWT-TST-EUR].dbo.BDOMetadata_RSSCategory m
JOIN BDO_RSSCategories b ON b.CodeName = m.RSSCategoryCodeName COLLATE DATABASE_DEFAULT
JOIN CMS_ContentItemCommonData cd ON cd.ContentItemCommonDataID = b.ContentItemDataCommonDataID
JOIN CMS_ContentItem ci ON ci.ContentItemID = cd.ContentItemCommonDataContentItemID
WHERE EXISTS (SELECT 1 FROM CMS_WebPageItem w WHERE w.WebPageItemContentItemID = ci.ContentItemID AND w.WebPageItemWebsiteChannelID = 10);

IF OBJECT_ID('tempdb..#modCT') IS NOT NULL DROP TABLE #modCT;
SELECT m.ContentTypeID AS ModuleID, ci.ContentItemID AS TargetItemID, ci.ContentItemGUID
INTO #modCT
FROM [BDO-DB-GWT-TST-EUR].dbo.BDOMetadata_ContentType m
JOIN BDO_Contenttypes b ON b.CodeName = m.ContentTypeCodeName COLLATE DATABASE_DEFAULT
JOIN CMS_ContentItemCommonData cd ON cd.ContentItemCommonDataID = b.ContentItemDataCommonDataID
JOIN CMS_ContentItem ci ON ci.ContentItemID = cd.ContentItemCommonDataContentItemID
WHERE EXISTS (SELECT 1 FROM CMS_WebPageItem w WHERE w.WebPageItemContentItemID = ci.ContentItemID AND w.WebPageItemWebsiteChannelID = 10);

-- ---- IndustryCategory node -> target page: NodeGUID match, else trimmed-name fallback ----
-- (some IndustryCategory pages got deterministic GUIDs, not the preserved NodeGUID)
IF OBJECT_ID('tempdb..#ic_tgt') IS NOT NULL DROP TABLE #ic_tgt;
SELECT ci.ContentItemID AS TargetItemID, ci.ContentItemGUID, LTRIM(RTRIM(lm.ContentItemLanguageMetadataDisplayName)) AS Name
INTO #ic_tgt
FROM CMS_WebPageItem w
JOIN CMS_ContentItem ci ON ci.ContentItemID = w.WebPageItemContentItemID
JOIN CMS_Class cl ON cl.ClassID = ci.ContentItemContentTypeID AND cl.ClassName = 'BDO.IndustryCategory'
JOIN CMS_ContentItemLanguageMetadata lm ON lm.ContentItemLanguageMetadataContentItemID = ci.ContentItemID AND lm.ContentItemLanguageMetadataContentLanguageID = 1
WHERE w.WebPageItemWebsiteChannelID = 10;

IF OBJECT_ID('tempdb..#icmap') IS NOT NULL DROP TABLE #icmap;
SELECT s.NodeID, COALESCE(g.TargetItemID, n.TargetItemID) AS TargetItemID, COALESCE(g.ContentItemGUID, n.ContentItemGUID) AS ContentItemGUID
INTO #icmap
FROM (SELECT DISTINCT t.NodeID, t.NodeGUID, LTRIM(RTRIM(t.NodeName)) AS NodeName
      FROM [BDO-DB-GWT-TST-EUR].dbo.CMS_Tree t
      JOIN [BDO-DB-GWT-TST-EUR].dbo.CMS_Class cl ON cl.ClassID = t.NodeClassID AND cl.ClassName = 'BDO.IndustryCategory') s
LEFT JOIN #ic_tgt g ON g.ContentItemGUID = s.NodeGUID
LEFT JOIN #ic_tgt n ON n.Name = s.NodeName COLLATE DATABASE_DEFAULT
WHERE COALESCE(g.TargetItemID, n.TargetItemID) IS NOT NULL;
CREATE INDEX IX_icmap ON #icmap (NodeID);

-- ---- SpecialtiesCategory node -> target page: NodeGUID match, else trimmed-name fallback ----
IF OBJECT_ID('tempdb..#sc_tgt') IS NOT NULL DROP TABLE #sc_tgt;
SELECT ci.ContentItemID AS TargetItemID, ci.ContentItemGUID, LTRIM(RTRIM(lm.ContentItemLanguageMetadataDisplayName)) AS Name
INTO #sc_tgt
FROM CMS_WebPageItem w
JOIN CMS_ContentItem ci ON ci.ContentItemID = w.WebPageItemContentItemID
JOIN CMS_Class cl ON cl.ClassID = ci.ContentItemContentTypeID AND cl.ClassName = 'BDO.SpecialtiesCategory'
JOIN CMS_ContentItemLanguageMetadata lm ON lm.ContentItemLanguageMetadataContentItemID = ci.ContentItemID AND lm.ContentItemLanguageMetadataContentLanguageID = 1
WHERE w.WebPageItemWebsiteChannelID = 10;

IF OBJECT_ID('tempdb..#scmap') IS NOT NULL DROP TABLE #scmap;
SELECT s.NodeID, COALESCE(g.TargetItemID, n.TargetItemID) AS TargetItemID, COALESCE(g.ContentItemGUID, n.ContentItemGUID) AS ContentItemGUID
INTO #scmap
FROM (SELECT DISTINCT t.NodeID, t.NodeGUID, LTRIM(RTRIM(t.NodeName)) AS NodeName
      FROM [BDO-DB-GWT-TST-EUR].dbo.CMS_Tree t
      JOIN [BDO-DB-GWT-TST-EUR].dbo.CMS_Class cl ON cl.ClassID = t.NodeClassID AND cl.ClassName = 'BDO.SpecialtiesCategory') s
LEFT JOIN #sc_tgt g ON g.ContentItemGUID = s.NodeGUID
LEFT JOIN #sc_tgt n ON n.Name = s.NodeName COLLATE DATABASE_DEFAULT
WHERE COALESCE(g.TargetItemID, n.TargetItemID) IS NOT NULL;
CREATE INDEX IX_scmap ON #scmap (NodeID);

-- ---- SpecialtiesArea node -> target page: NodeGUID match, else trimmed-name fallback ----
IF OBJECT_ID('tempdb..#sa_tgt') IS NOT NULL DROP TABLE #sa_tgt;
SELECT ci.ContentItemID AS TargetItemID, ci.ContentItemGUID, LTRIM(RTRIM(lm.ContentItemLanguageMetadataDisplayName)) AS Name
INTO #sa_tgt
FROM CMS_WebPageItem w
JOIN CMS_ContentItem ci ON ci.ContentItemID = w.WebPageItemContentItemID
JOIN CMS_Class cl ON cl.ClassID = ci.ContentItemContentTypeID AND cl.ClassName = 'BDO.SpecialtiesArea'
JOIN CMS_ContentItemLanguageMetadata lm ON lm.ContentItemLanguageMetadataContentItemID = ci.ContentItemID AND lm.ContentItemLanguageMetadataContentLanguageID = 1
WHERE w.WebPageItemWebsiteChannelID = 10;

IF OBJECT_ID('tempdb..#samap') IS NOT NULL DROP TABLE #samap;
SELECT s.NodeID, COALESCE(g.TargetItemID, n.TargetItemID) AS TargetItemID, COALESCE(g.ContentItemGUID, n.ContentItemGUID) AS ContentItemGUID
INTO #samap
FROM (SELECT DISTINCT t.NodeID, t.NodeGUID, LTRIM(RTRIM(t.NodeName)) AS NodeName
      FROM [BDO-DB-GWT-TST-EUR].dbo.CMS_Tree t
      JOIN [BDO-DB-GWT-TST-EUR].dbo.CMS_Class cl ON cl.ClassID = t.NodeClassID AND cl.ClassName = 'BDO.SpecialtiesArea') s
LEFT JOIN #sa_tgt g ON g.ContentItemGUID = s.NodeGUID
LEFT JOIN #sa_tgt n ON n.Name = s.NodeName COLLATE DATABASE_DEFAULT
WHERE COALESCE(g.TargetItemID, n.TargetItemID) IS NOT NULL;
CREATE INDEX IX_samap ON #samap (NodeID);

-- ---- RelevantContactPerson: source stores DocumentID of Person/InternalPerson -> target page (NodeGUID, else name within same class) ----
IF OBJECT_ID('tempdb..#pp_tgt') IS NOT NULL DROP TABLE #pp_tgt;
SELECT ci.ContentItemID AS TargetItemID, ci.ContentItemGUID, cl.ClassName AS Cls, LTRIM(RTRIM(lm.ContentItemLanguageMetadataDisplayName)) AS Name
INTO #pp_tgt
FROM CMS_WebPageItem w
JOIN CMS_ContentItem ci ON ci.ContentItemID = w.WebPageItemContentItemID
JOIN CMS_Class cl ON cl.ClassID = ci.ContentItemContentTypeID AND cl.ClassName IN ('BDO.Person','BDO.InternalPerson')
JOIN CMS_ContentItemLanguageMetadata lm ON lm.ContentItemLanguageMetadataContentItemID = ci.ContentItemID AND lm.ContentItemLanguageMetadataContentLanguageID = 1
WHERE w.WebPageItemWebsiteChannelID = 10;

IF OBJECT_ID('tempdb..#rcpmap') IS NOT NULL DROP TABLE #rcpmap;
SELECT s.DocumentID, COALESCE(g.TargetItemID, n.TargetItemID) AS TargetItemID, COALESCE(g.ContentItemGUID, n.ContentItemGUID) AS ContentItemGUID
INTO #rcpmap
FROM (SELECT d.DocumentID, t.NodeGUID, cl.ClassName AS Cls, LTRIM(RTRIM(t.NodeName)) AS NodeName
      FROM [BDO-DB-GWT-TST-EUR].dbo.CMS_Document d
      JOIN [BDO-DB-GWT-TST-EUR].dbo.CMS_Tree t ON t.NodeID = d.DocumentNodeID
      JOIN [BDO-DB-GWT-TST-EUR].dbo.CMS_Class cl ON cl.ClassID = t.NodeClassID AND cl.ClassName IN ('BDO.Person','BDO.InternalPerson')) s
LEFT JOIN #pp_tgt g ON g.ContentItemGUID = s.NodeGUID
LEFT JOIN #pp_tgt n ON n.Cls = s.Cls COLLATE DATABASE_DEFAULT AND n.Name = s.NodeName COLLATE DATABASE_DEFAULT
WHERE COALESCE(g.TargetItemID, n.TargetItemID) IS NOT NULL;
CREATE INDEX IX_rcpmap ON #rcpmap (DocumentID);

-- ---- RelatedExternalPeople: source stores ExternalPerson NodeGUID -> target page (GUID match, else name) ----
IF OBJECT_ID('tempdb..#ep_tgt') IS NOT NULL DROP TABLE #ep_tgt;
SELECT ci.ContentItemID AS TargetItemID, ci.ContentItemGUID, LTRIM(RTRIM(lm.ContentItemLanguageMetadataDisplayName)) AS Name
INTO #ep_tgt
FROM CMS_WebPageItem w
JOIN CMS_ContentItem ci ON ci.ContentItemID = w.WebPageItemContentItemID
JOIN CMS_Class cl ON cl.ClassID = ci.ContentItemContentTypeID AND cl.ClassName = 'BDO.ExternalPerson'
JOIN CMS_ContentItemLanguageMetadata lm ON lm.ContentItemLanguageMetadataContentItemID = ci.ContentItemID AND lm.ContentItemLanguageMetadataContentLanguageID = 1
WHERE w.WebPageItemWebsiteChannelID = 10;

IF OBJECT_ID('tempdb..#epmap') IS NOT NULL DROP TABLE #epmap;
SELECT s.NodeGUID, COALESCE(g.ContentItemGUID, n.ContentItemGUID) AS ContentItemGUID, COALESCE(g.TargetItemID, n.TargetItemID) AS TargetItemID
INTO #epmap
FROM (SELECT DISTINCT t.NodeGUID, LTRIM(RTRIM(t.NodeName)) AS NodeName
      FROM [BDO-DB-GWT-TST-EUR].dbo.CMS_Tree t
      JOIN [BDO-DB-GWT-TST-EUR].dbo.CMS_Class cl ON cl.ClassID = t.NodeClassID AND cl.ClassName = 'BDO.ExternalPerson') s
LEFT JOIN #ep_tgt g ON g.ContentItemGUID = s.NodeGUID
LEFT JOIN #ep_tgt n ON n.Name = s.NodeName COLLATE DATABASE_DEFAULT
WHERE COALESCE(g.TargetItemID, n.TargetItemID) IS NOT NULL;

-- ---- MicrositeMasterPage node -> target page: NodeGUID match, else trimmed-name fallback ----
IF OBJECT_ID('tempdb..#mmp_tgt') IS NOT NULL DROP TABLE #mmp_tgt;
SELECT ci.ContentItemID AS TargetItemID, ci.ContentItemGUID, LTRIM(RTRIM(lm.ContentItemLanguageMetadataDisplayName)) AS Name
INTO #mmp_tgt
FROM CMS_WebPageItem w
JOIN CMS_ContentItem ci ON ci.ContentItemID = w.WebPageItemContentItemID
JOIN CMS_Class cl ON cl.ClassID = ci.ContentItemContentTypeID AND cl.ClassName = 'BDO.MicrositeMasterPage'
JOIN CMS_ContentItemLanguageMetadata lm ON lm.ContentItemLanguageMetadataContentItemID = ci.ContentItemID AND lm.ContentItemLanguageMetadataContentLanguageID = 1
WHERE w.WebPageItemWebsiteChannelID = 10;

IF OBJECT_ID('tempdb..#mmpmap') IS NOT NULL DROP TABLE #mmpmap;
SELECT s.NodeID, COALESCE(g.TargetItemID, n.TargetItemID) AS TargetItemID, COALESCE(g.ContentItemGUID, n.ContentItemGUID) AS ContentItemGUID
INTO #mmpmap
FROM (SELECT DISTINCT t.NodeID, t.NodeGUID, LTRIM(RTRIM(t.NodeName)) AS NodeName
      FROM [BDO-DB-GWT-TST-EUR].dbo.CMS_Tree t
      JOIN [BDO-DB-GWT-TST-EUR].dbo.CMS_Class cl ON cl.ClassID = t.NodeClassID AND cl.ClassName = 'BDO.MicrositeMasterPage') s
LEFT JOIN #mmp_tgt g ON g.ContentItemGUID = s.NodeGUID
LEFT JOIN #mmp_tgt n ON n.Name = s.NodeName COLLATE DATABASE_DEFAULT
WHERE COALESCE(g.TargetItemID, n.TargetItemID) IS NOT NULL;
CREATE INDEX IX_mmpmap ON #mmpmap (NodeID);

-- ---- Page-reference fields (data-driven) ----
IF OBJECT_ID('tempdb..#pfields') IS NOT NULL DROP TABLE #pfields;
CREATE TABLE #pfields (Ord int IDENTITY, TargetTable sysname, TargetKeyCol sysname, TargetCol sysname, SourceCol sysname);
INSERT INTO #pfields (TargetTable, TargetKeyCol, TargetCol, SourceCol) VALUES
('CMS_ContentItemCommonData','ContentItemCommonDataID','MetadataBusinessLine','MetadataBusinessLines'),
('CMS_ContentItemCommonData','ContentItemCommonDataID','MetadataServiceAreas','MetadataServiceAreas'),
('CMS_ContentItemCommonData','ContentItemCommonDataID','MetadataServices','MetadataServices'),
('CMS_ContentItemCommonData','ContentItemCommonDataID','MetadataIndustries','MetadataIndustries'),
('CMS_ContentItemCommonData','ContentItemCommonDataID','MicrositePages','MicrositePages'),
('BDO_Insight','ContentItemDataCommonDataID','MetadataSpecialtiesPages','MetadataSpecialtiesPages'),
('BDO_Insight','ContentItemDataCommonDataID','MetadataOfficeLocation','MetadataOfficeLocation');

DECLARE @tt sysname,@tk sysname,@tc sysname,@sc sysname,@i int=1,@max int=(SELECT MAX(Ord) FROM #pfields);
WHILE @i <= @max
BEGIN
    SELECT @tt=TargetTable,@tk=TargetKeyCol,@tc=TargetCol,@sc=SourceCol FROM #pfields WHERE Ord=@i;
    DECLARE @sql nvarchar(max) = N'
    WITH resolved AS (
        SELECT c.CommonID, ''[''+STRING_AGG(''{"Identifier":"''+LOWER(CONVERT(nvarchar(36),nm.ContentItemGUID))+''"}'', '','')+'']'' AS js
        FROM #corr c JOIN #srcref s ON s.InsightID=c.SourceInsightID
        CROSS APPLY STRING_SPLIT(s.'+QUOTENAME(@sc)+N', ''|'') ss
        JOIN #nodemap nm ON nm.NodeID = TRY_CAST(LTRIM(RTRIM(ss.value)) AS int)
        WHERE s.'+QUOTENAME(@sc)+N' IS NOT NULL AND s.'+QUOTENAME(@sc)+N' <> ''''
        GROUP BY c.CommonID)
    UPDATE t SET t.'+QUOTENAME(@tc)+N' = r.js
    FROM '+QUOTENAME(@tt)+N' t JOIN resolved r ON r.CommonID = t.'+QUOTENAME(@tk)+N';

    INSERT INTO CMS_ContentItemReference (ContentItemReferenceGUID, ContentItemReferenceSourceCommonDataID, ContentItemReferenceTargetItemID, ContentItemReferenceGroupGUID)
    SELECT DISTINCT NEWID(), c.CommonID, nm.TargetItemID, NEWID()
    FROM #corr c JOIN #srcref s ON s.InsightID=c.SourceInsightID
    CROSS APPLY STRING_SPLIT(s.'+QUOTENAME(@sc)+N', ''|'') ss
    JOIN #nodemap nm ON nm.NodeID = TRY_CAST(LTRIM(RTRIM(ss.value)) AS int)
    WHERE s.'+QUOTENAME(@sc)+N' IS NOT NULL AND s.'+QUOTENAME(@sc)+N' <> ''''
      AND NOT EXISTS (SELECT 1 FROM CMS_ContentItemReference r WHERE r.ContentItemReferenceSourceCommonDataID=c.CommonID AND r.ContentItemReferenceTargetItemID=nm.TargetItemID);';
    EXEC sp_executesql @sql;
    SET @i += 1;
END

-- ---- MicrositeMasterPages (common data) via name-robust map -> BDO.MicrositeMasterPage ----
;WITH resolved AS (
    SELECT c.CommonID, '['+STRING_AGG('{"Identifier":"'+LOWER(CONVERT(nvarchar(36),mm.ContentItemGUID))+'"}', ',')+']' AS js
    FROM #corr c JOIN #srcref s ON s.InsightID=c.SourceInsightID
    CROSS APPLY STRING_SPLIT(s.MicrositeMasterPages, '|') ss
    JOIN #mmpmap mm ON mm.NodeID = TRY_CAST(LTRIM(RTRIM(ss.value)) AS int)
    WHERE s.MicrositeMasterPages IS NOT NULL AND s.MicrositeMasterPages <> ''
    GROUP BY c.CommonID)
UPDATE t SET t.MicrositeMasterPages = r.js
FROM CMS_ContentItemCommonData t JOIN resolved r ON r.CommonID = t.ContentItemCommonDataID;

INSERT INTO CMS_ContentItemReference (ContentItemReferenceGUID, ContentItemReferenceSourceCommonDataID, ContentItemReferenceTargetItemID, ContentItemReferenceGroupGUID)
SELECT DISTINCT NEWID(), c.CommonID, mm.TargetItemID, NEWID()
FROM #corr c JOIN #srcref s ON s.InsightID=c.SourceInsightID
CROSS APPLY STRING_SPLIT(s.MicrositeMasterPages, '|') ss
JOIN #mmpmap mm ON mm.NodeID = TRY_CAST(LTRIM(RTRIM(ss.value)) AS int)
WHERE s.MicrositeMasterPages IS NOT NULL AND s.MicrositeMasterPages <> ''
  AND NOT EXISTS (SELECT 1 FROM CMS_ContentItemReference r WHERE r.ContentItemReferenceSourceCommonDataID=c.CommonID AND r.ContentItemReferenceTargetItemID=mm.TargetItemID);

-- ---- Metadata-module field: MetadataBusinessIssues (coupled BDO_Insight) ----
;WITH resolved AS (
    SELECT c.CommonID, '['+STRING_AGG('{"Identifier":"'+LOWER(CONVERT(nvarchar(36),mm.ContentItemGUID))+'"}', ',')+']' AS js
    FROM #corr c JOIN #srcref s ON s.InsightID=c.SourceInsightID
    CROSS APPLY STRING_SPLIT(s.MetadataBusinessIssues, '|') ss
    JOIN #modBI mm ON mm.ModuleID = TRY_CAST(LTRIM(RTRIM(ss.value)) AS int)
    WHERE s.MetadataBusinessIssues IS NOT NULL AND s.MetadataBusinessIssues <> ''
    GROUP BY c.CommonID)
UPDATE t SET t.MetadataBusinessIssues = r.js
FROM BDO_Insight t JOIN resolved r ON r.CommonID = t.ContentItemDataCommonDataID;

INSERT INTO CMS_ContentItemReference (ContentItemReferenceGUID, ContentItemReferenceSourceCommonDataID, ContentItemReferenceTargetItemID, ContentItemReferenceGroupGUID)
SELECT DISTINCT NEWID(), c.CommonID, mm.TargetItemID, NEWID()
FROM #corr c JOIN #srcref s ON s.InsightID=c.SourceInsightID
CROSS APPLY STRING_SPLIT(s.MetadataBusinessIssues, '|') ss
JOIN #modBI mm ON mm.ModuleID = TRY_CAST(LTRIM(RTRIM(ss.value)) AS int)
WHERE s.MetadataBusinessIssues IS NOT NULL AND s.MetadataBusinessIssues <> ''
  AND NOT EXISTS (SELECT 1 FROM CMS_ContentItemReference r WHERE r.ContentItemReferenceSourceCommonDataID=c.CommonID AND r.ContentItemReferenceTargetItemID=mm.TargetItemID);

-- ---- Metadata-module field: MetadataRssCategory (common data) ----
;WITH resolved AS (
    SELECT c.CommonID, '['+STRING_AGG('{"Identifier":"'+LOWER(CONVERT(nvarchar(36),mm.ContentItemGUID))+'"}', ',')+']' AS js
    FROM #corr c JOIN #srcref s ON s.InsightID=c.SourceInsightID
    CROSS APPLY STRING_SPLIT(s.MetadataRssCategory, '|') ss
    JOIN #modRss mm ON mm.ModuleID = TRY_CAST(LTRIM(RTRIM(ss.value)) AS int)
    WHERE s.MetadataRssCategory IS NOT NULL AND s.MetadataRssCategory <> ''
    GROUP BY c.CommonID)
UPDATE t SET t.MetadataRssCategory = r.js
FROM CMS_ContentItemCommonData t JOIN resolved r ON r.CommonID = t.ContentItemCommonDataID;

INSERT INTO CMS_ContentItemReference (ContentItemReferenceGUID, ContentItemReferenceSourceCommonDataID, ContentItemReferenceTargetItemID, ContentItemReferenceGroupGUID)
SELECT DISTINCT NEWID(), c.CommonID, mm.TargetItemID, NEWID()
FROM #corr c JOIN #srcref s ON s.InsightID=c.SourceInsightID
CROSS APPLY STRING_SPLIT(s.MetadataRssCategory, '|') ss
JOIN #modRss mm ON mm.ModuleID = TRY_CAST(LTRIM(RTRIM(ss.value)) AS int)
WHERE s.MetadataRssCategory IS NOT NULL AND s.MetadataRssCategory <> ''
  AND NOT EXISTS (SELECT 1 FROM CMS_ContentItemReference r WHERE r.ContentItemReferenceSourceCommonDataID=c.CommonID AND r.ContentItemReferenceTargetItemID=mm.TargetItemID);

-- ---- Metadata-module field: ContentType (common data) -> BDO Metadata Content types ----
;WITH resolved AS (
    SELECT c.CommonID, '['+STRING_AGG('{"Identifier":"'+LOWER(CONVERT(nvarchar(36),mm.ContentItemGUID))+'"}', ',')+']' AS js
    FROM #corr c JOIN #srcref s ON s.InsightID=c.SourceInsightID
    CROSS APPLY STRING_SPLIT(s.ContentType, '|') ss
    JOIN #modCT mm ON mm.ModuleID = TRY_CAST(LTRIM(RTRIM(ss.value)) AS int)
    WHERE s.ContentType IS NOT NULL AND s.ContentType <> ''
    GROUP BY c.CommonID)
UPDATE t SET t.ContentType = r.js
FROM CMS_ContentItemCommonData t JOIN resolved r ON r.CommonID = t.ContentItemCommonDataID;

INSERT INTO CMS_ContentItemReference (ContentItemReferenceGUID, ContentItemReferenceSourceCommonDataID, ContentItemReferenceTargetItemID, ContentItemReferenceGroupGUID)
SELECT DISTINCT NEWID(), c.CommonID, mm.TargetItemID, NEWID()
FROM #corr c JOIN #srcref s ON s.InsightID=c.SourceInsightID
CROSS APPLY STRING_SPLIT(s.ContentType, '|') ss
JOIN #modCT mm ON mm.ModuleID = TRY_CAST(LTRIM(RTRIM(ss.value)) AS int)
WHERE s.ContentType IS NOT NULL AND s.ContentType <> ''
  AND NOT EXISTS (SELECT 1 FROM CMS_ContentItemReference r WHERE r.ContentItemReferenceSourceCommonDataID=c.CommonID AND r.ContentItemReferenceTargetItemID=mm.TargetItemID);

-- ---- Page field via name-robust map: MetadataIndustryCategories (common data) -> IndustryCategory ----
;WITH resolved AS (
    SELECT c.CommonID, '['+STRING_AGG('{"Identifier":"'+LOWER(CONVERT(nvarchar(36),mm.ContentItemGUID))+'"}', ',')+']' AS js
    FROM #corr c JOIN #srcref s ON s.InsightID=c.SourceInsightID
    CROSS APPLY STRING_SPLIT(s.MetadataIndustryCategories, '|') ss
    JOIN #icmap mm ON mm.NodeID = TRY_CAST(LTRIM(RTRIM(ss.value)) AS int)
    WHERE s.MetadataIndustryCategories IS NOT NULL AND s.MetadataIndustryCategories <> ''
    GROUP BY c.CommonID)
UPDATE t SET t.MetadataIndustryCategories = r.js
FROM CMS_ContentItemCommonData t JOIN resolved r ON r.CommonID = t.ContentItemCommonDataID;

INSERT INTO CMS_ContentItemReference (ContentItemReferenceGUID, ContentItemReferenceSourceCommonDataID, ContentItemReferenceTargetItemID, ContentItemReferenceGroupGUID)
SELECT DISTINCT NEWID(), c.CommonID, mm.TargetItemID, NEWID()
FROM #corr c JOIN #srcref s ON s.InsightID=c.SourceInsightID
CROSS APPLY STRING_SPLIT(s.MetadataIndustryCategories, '|') ss
JOIN #icmap mm ON mm.NodeID = TRY_CAST(LTRIM(RTRIM(ss.value)) AS int)
WHERE s.MetadataIndustryCategories IS NOT NULL AND s.MetadataIndustryCategories <> ''
  AND NOT EXISTS (SELECT 1 FROM CMS_ContentItemReference r WHERE r.ContentItemReferenceSourceCommonDataID=c.CommonID AND r.ContentItemReferenceTargetItemID=mm.TargetItemID);

-- ---- Coupled field via name-robust map: MetadataSpecialtiesCategories -> SpecialtiesCategory ----
;WITH resolved AS (
    SELECT c.CommonID, '['+STRING_AGG('{"Identifier":"'+LOWER(CONVERT(nvarchar(36),mm.ContentItemGUID))+'"}', ',')+']' AS js
    FROM #corr c JOIN #srcref s ON s.InsightID=c.SourceInsightID
    CROSS APPLY STRING_SPLIT(s.MetadataSpecialtiesCategories, '|') ss
    JOIN #scmap mm ON mm.NodeID = TRY_CAST(LTRIM(RTRIM(ss.value)) AS int)
    WHERE s.MetadataSpecialtiesCategories IS NOT NULL AND s.MetadataSpecialtiesCategories <> ''
    GROUP BY c.CommonID)
UPDATE t SET t.MetadataSpecialtiesCategories = r.js
FROM BDO_Insight t JOIN resolved r ON r.CommonID = t.ContentItemDataCommonDataID;

INSERT INTO CMS_ContentItemReference (ContentItemReferenceGUID, ContentItemReferenceSourceCommonDataID, ContentItemReferenceTargetItemID, ContentItemReferenceGroupGUID)
SELECT DISTINCT NEWID(), c.CommonID, mm.TargetItemID, NEWID()
FROM #corr c JOIN #srcref s ON s.InsightID=c.SourceInsightID
CROSS APPLY STRING_SPLIT(s.MetadataSpecialtiesCategories, '|') ss
JOIN #scmap mm ON mm.NodeID = TRY_CAST(LTRIM(RTRIM(ss.value)) AS int)
WHERE s.MetadataSpecialtiesCategories IS NOT NULL AND s.MetadataSpecialtiesCategories <> ''
  AND NOT EXISTS (SELECT 1 FROM CMS_ContentItemReference r WHERE r.ContentItemReferenceSourceCommonDataID=c.CommonID AND r.ContentItemReferenceTargetItemID=mm.TargetItemID);

-- ---- Coupled field via name-robust map: MetadataSpecialtiesAreas -> SpecialtiesArea ----
;WITH resolved AS (
    SELECT c.CommonID, '['+STRING_AGG('{"Identifier":"'+LOWER(CONVERT(nvarchar(36),mm.ContentItemGUID))+'"}', ',')+']' AS js
    FROM #corr c JOIN #srcref s ON s.InsightID=c.SourceInsightID
    CROSS APPLY STRING_SPLIT(s.MetadataSpecialtiesAreas, '|') ss
    JOIN #samap mm ON mm.NodeID = TRY_CAST(LTRIM(RTRIM(ss.value)) AS int)
    WHERE s.MetadataSpecialtiesAreas IS NOT NULL AND s.MetadataSpecialtiesAreas <> ''
    GROUP BY c.CommonID)
UPDATE t SET t.MetadataSpecialtiesAreas = r.js
FROM BDO_Insight t JOIN resolved r ON r.CommonID = t.ContentItemDataCommonDataID;

INSERT INTO CMS_ContentItemReference (ContentItemReferenceGUID, ContentItemReferenceSourceCommonDataID, ContentItemReferenceTargetItemID, ContentItemReferenceGroupGUID)
SELECT DISTINCT NEWID(), c.CommonID, mm.TargetItemID, NEWID()
FROM #corr c JOIN #srcref s ON s.InsightID=c.SourceInsightID
CROSS APPLY STRING_SPLIT(s.MetadataSpecialtiesAreas, '|') ss
JOIN #samap mm ON mm.NodeID = TRY_CAST(LTRIM(RTRIM(ss.value)) AS int)
WHERE s.MetadataSpecialtiesAreas IS NOT NULL AND s.MetadataSpecialtiesAreas <> ''
  AND NOT EXISTS (SELECT 1 FROM CMS_ContentItemReference r WHERE r.ContentItemReferenceSourceCommonDataID=c.CommonID AND r.ContentItemReferenceTargetItemID=mm.TargetItemID);

-- ---- RelevantContactPerson (common data) -> Person/InternalPerson (DocumentID-keyed) ----
;WITH resolved AS (
    SELECT c.CommonID, '['+STRING_AGG('{"Identifier":"'+LOWER(CONVERT(nvarchar(36),mm.ContentItemGUID))+'"}', ',')+']' AS js
    FROM #corr c JOIN #srcref s ON s.InsightID=c.SourceInsightID
    CROSS APPLY STRING_SPLIT(s.RelevantContactPerson, '|') ss
    JOIN #rcpmap mm ON mm.DocumentID = TRY_CAST(LTRIM(RTRIM(ss.value)) AS int)
    WHERE s.RelevantContactPerson IS NOT NULL AND s.RelevantContactPerson <> ''
    GROUP BY c.CommonID)
UPDATE t SET t.RelevantContactPerson = r.js
FROM CMS_ContentItemCommonData t JOIN resolved r ON r.CommonID = t.ContentItemCommonDataID;

INSERT INTO CMS_ContentItemReference (ContentItemReferenceGUID, ContentItemReferenceSourceCommonDataID, ContentItemReferenceTargetItemID, ContentItemReferenceGroupGUID)
SELECT DISTINCT NEWID(), c.CommonID, mm.TargetItemID, NEWID()
FROM #corr c JOIN #srcref s ON s.InsightID=c.SourceInsightID
CROSS APPLY STRING_SPLIT(s.RelevantContactPerson, '|') ss
JOIN #rcpmap mm ON mm.DocumentID = TRY_CAST(LTRIM(RTRIM(ss.value)) AS int)
WHERE s.RelevantContactPerson IS NOT NULL AND s.RelevantContactPerson <> ''
  AND NOT EXISTS (SELECT 1 FROM CMS_ContentItemReference r WHERE r.ContentItemReferenceSourceCommonDataID=c.CommonID AND r.ContentItemReferenceTargetItemID=mm.TargetItemID);

-- ---- RelatedExternalPeople (common data) -> ExternalPerson (NodeGUID-keyed) ----
;WITH resolved AS (
    SELECT c.CommonID, '['+STRING_AGG('{"Identifier":"'+LOWER(CONVERT(nvarchar(36),mm.ContentItemGUID))+'"}', ',')+']' AS js
    FROM #corr c JOIN #srcref s ON s.InsightID=c.SourceInsightID
    CROSS APPLY STRING_SPLIT(s.RelatedExternalPeople, '|') ss
    JOIN #epmap mm ON mm.NodeGUID = TRY_CONVERT(uniqueidentifier, LTRIM(RTRIM(ss.value)))
    WHERE s.RelatedExternalPeople IS NOT NULL AND s.RelatedExternalPeople <> ''
    GROUP BY c.CommonID)
UPDATE t SET t.RelatedExternalPeople = r.js
FROM CMS_ContentItemCommonData t JOIN resolved r ON r.CommonID = t.ContentItemCommonDataID;

INSERT INTO CMS_ContentItemReference (ContentItemReferenceGUID, ContentItemReferenceSourceCommonDataID, ContentItemReferenceTargetItemID, ContentItemReferenceGroupGUID)
SELECT DISTINCT NEWID(), c.CommonID, mm.TargetItemID, NEWID()
FROM #corr c JOIN #srcref s ON s.InsightID=c.SourceInsightID
CROSS APPLY STRING_SPLIT(s.RelatedExternalPeople, '|') ss
JOIN #epmap mm ON mm.NodeGUID = TRY_CONVERT(uniqueidentifier, LTRIM(RTRIM(ss.value)))
WHERE s.RelatedExternalPeople IS NOT NULL AND s.RelatedExternalPeople <> ''
  AND NOT EXISTS (SELECT 1 FROM CMS_ContentItemReference r WHERE r.ContentItemReferenceSourceCommonDataID=c.CommonID AND r.ContentItemReferenceTargetItemID=mm.TargetItemID);

-- ---- Summary ----
SELECT 'Insights correlated' AS Metric, COUNT(*) AS Val FROM #corr
UNION ALL SELECT 'MetadataBusinessLine set', COUNT(*) FROM CMS_ContentItemCommonData cd JOIN #corr c ON c.CommonID=cd.ContentItemCommonDataID WHERE cd.MetadataBusinessLine LIKE '%Identifier%'
UNION ALL SELECT 'MetadataServices set', COUNT(*) FROM CMS_ContentItemCommonData cd JOIN #corr c ON c.CommonID=cd.ContentItemCommonDataID WHERE cd.MetadataServices LIKE '%Identifier%'
UNION ALL SELECT 'MetadataOfficeLocation set', COUNT(*) FROM BDO_Insight bi JOIN #corr c ON c.CommonID=bi.ContentItemDataCommonDataID WHERE bi.MetadataOfficeLocation LIKE '%Identifier%'
UNION ALL SELECT 'MetadataBusinessIssues set', COUNT(*) FROM BDO_Insight bi JOIN #corr c ON c.CommonID=bi.ContentItemDataCommonDataID WHERE bi.MetadataBusinessIssues LIKE '%Identifier%'
UNION ALL SELECT 'ContentType set', COUNT(*) FROM CMS_ContentItemCommonData cd JOIN #corr c ON c.CommonID=cd.ContentItemCommonDataID WHERE cd.ContentType LIKE '%Identifier%'
UNION ALL SELECT 'MetadataIndustryCategories set', COUNT(*) FROM CMS_ContentItemCommonData cd JOIN #corr c ON c.CommonID=cd.ContentItemCommonDataID WHERE cd.MetadataIndustryCategories LIKE '%Identifier%'
UNION ALL SELECT 'MetadataSpecialtiesCategories set', COUNT(*) FROM BDO_Insight bi JOIN #corr c ON c.CommonID=bi.ContentItemDataCommonDataID WHERE bi.MetadataSpecialtiesCategories LIKE '%Identifier%'
UNION ALL SELECT 'MetadataSpecialtiesAreas set', COUNT(*) FROM BDO_Insight bi JOIN #corr c ON c.CommonID=bi.ContentItemDataCommonDataID WHERE bi.MetadataSpecialtiesAreas LIKE '%Identifier%'
UNION ALL SELECT 'RelevantContactPerson set', COUNT(*) FROM CMS_ContentItemCommonData cd JOIN #corr c ON c.CommonID=cd.ContentItemCommonDataID WHERE cd.RelevantContactPerson LIKE '%Identifier%'
UNION ALL SELECT 'RelatedExternalPeople set', COUNT(*) FROM CMS_ContentItemCommonData cd JOIN #corr c ON c.CommonID=cd.ContentItemCommonDataID WHERE cd.RelatedExternalPeople LIKE '%Identifier%'
UNION ALL SELECT 'MicrositeMasterPages set', COUNT(*) FROM CMS_ContentItemCommonData cd JOIN #corr c ON c.CommonID=cd.ContentItemCommonDataID WHERE cd.MicrositeMasterPages LIKE '%Identifier%';

COMMIT TRANSACTION;
