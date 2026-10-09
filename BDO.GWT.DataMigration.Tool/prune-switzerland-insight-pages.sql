SET XACT_ABORT ON;
SET QUOTED_IDENTIFIER ON;

DECLARE @apply bit = 0;
DECLARE @channelID int = (
    SELECT wc.WebsiteChannelID FROM CMS_WebsiteChannel wc
    JOIN CMS_Channel ch ON ch.ChannelID=wc.WebsiteChannelChannelID
    WHERE ch.ChannelName='Switzerland'
);
IF @channelID IS NULL THROW 50000, 'Switzerland channel was not found.', 1;

BEGIN TRANSACTION;

SELECT wp.WebPageItemID, wp.WebPageItemContentItemID, wp.WebPageItemParentID,
    wp.WebPageItemTreePath, ci.ContentItemGUID, cl.ClassName
INTO #pages
FROM CMS_WebPageItem wp
JOIN CMS_ContentItem ci ON ci.ContentItemID=wp.WebPageItemContentItemID
JOIN CMS_Class cl ON cl.ClassID=ci.ContentItemContentTypeID
WHERE wp.WebPageItemWebsiteChannelID=@channelID;

CREATE TABLE #keep (PageID int PRIMARY KEY);
CREATE TABLE #edges (SourceItemID int, TargetItemID int, PRIMARY KEY (SourceItemID,TargetItemID));
INSERT INTO #edges
SELECT DISTINCT cd.ContentItemCommonDataContentItemID, reference.ContentItemReferenceTargetItemID
FROM CMS_ContentItemReference reference
JOIN CMS_ContentItemCommonData cd ON cd.ContentItemCommonDataID=reference.ContentItemReferenceSourceCommonDataID;

SELECT cd.ContentItemCommonDataID,cd.ContentItemCommonDataContentItemID INTO #referenceCommon
FROM CMS_ContentItemCommonData cd
WHERE EXISTS (SELECT 1 FROM #pages page WHERE page.WebPageItemContentItemID=cd.ContentItemCommonDataContentItemID);
CREATE UNIQUE CLUSTERED INDEX IX_referenceCommon ON #referenceCommon(ContentItemCommonDataID);

DECLARE @tableName sysname, @columnName sysname, @commonColumn sysname, @referenceSql nvarchar(max);
DECLARE referenceColumns CURSOR LOCAL FAST_FORWARD FOR
SELECT tableInfo.name, columnInfo.name, commonColumn.name
FROM sys.tables tableInfo
JOIN sys.schemas schemaInfo ON schemaInfo.schema_id=tableInfo.schema_id AND schemaInfo.name='dbo'
JOIN sys.columns commonColumn ON commonColumn.object_id=tableInfo.object_id
    AND commonColumn.name IN ('ContentItemDataCommonDataID','ContentItemCommonDataID')
JOIN sys.columns columnInfo ON columnInfo.object_id=tableInfo.object_id AND columnInfo.system_type_id IN (167,231);
OPEN referenceColumns;
FETCH NEXT FROM referenceColumns INTO @tableName,@columnName,@commonColumn;
WHILE @@FETCH_STATUS=0
BEGIN
    SET @referenceSql=N'INSERT INTO #edges SELECT DISTINCT cd.ContentItemCommonDataContentItemID,target.ContentItemID
FROM dbo.'+QUOTENAME(@tableName)+N' data
JOIN #referenceCommon cd ON cd.ContentItemCommonDataID=data.'+QUOTENAME(@commonColumn)+N'
CROSS APPLY OPENJSON(CASE WHEN ISJSON(data.'+QUOTENAME(@columnName)+N')=1 AND LEFT(LTRIM(data.'+QUOTENAME(@columnName)+N'),1)=''['' THEN data.'+QUOTENAME(@columnName)+N' ELSE ''[]'' END)
WITH (Identifier nvarchar(100) ''$.Identifier'') reference
JOIN CMS_ContentItem target ON target.ContentItemGUID=TRY_CONVERT(uniqueidentifier,reference.Identifier)
WHERE NOT EXISTS (SELECT 1 FROM #edges edge WHERE edge.SourceItemID=cd.ContentItemCommonDataContentItemID AND edge.TargetItemID=target.ContentItemID);';
    EXEC sys.sp_executesql @referenceSql;
    FETCH NEXT FROM referenceColumns INTO @tableName,@columnName,@commonColumn;
END;
CLOSE referenceColumns;
DEALLOCATE referenceColumns;

INSERT INTO #keep
SELECT WebPageItemID FROM #pages
WHERE ClassName IN ('BDO.Insight','BDO.InsightCategory','BDO.SectionInsightsPage',
    'BDO.RemoteDeal','BDO.SectionDeals',
    'BDO.Businessissue','BDO.RSSCategories','BDO.Contenttypes');
IF NOT EXISTS (SELECT 1 FROM #pages WHERE ClassName='BDO.Insight')
    THROW 50000, 'No Switzerland Insight pages were found; cleanup refused.', 1;

SELECT DISTINCT insight.* INTO #sourceInsights
FROM [bdo-db-gwt-tst2-eur].dbo.BDO_Insight insight
JOIN [bdo-db-gwt-tst2-eur].dbo.CMS_Document document ON document.DocumentForeignKeyValue=insight.InsightID
JOIN [bdo-db-gwt-tst2-eur].dbo.CMS_Tree node ON node.NodeID=document.DocumentNodeID
JOIN [bdo-db-gwt-tst2-eur].dbo.CMS_Class cl ON cl.ClassID=node.NodeClassID AND cl.ClassName='BDO.Insight'
JOIN [bdo-db-gwt-tst2-eur].dbo.CMS_Site site ON site.SiteID=node.NodeSiteID AND site.SiteName='Switzerland';

SELECT DISTINCT field.Kind, LTRIM(RTRIM(token.value)) Token
INTO #sourceTokens
FROM #sourceInsights insight
CROSS APPLY (VALUES
    ('Node',insight.MetadataBusinessLines), ('Node',insight.MetadataServiceAreas),
    ('Node',insight.MetadataServices), ('Node',insight.MetadataIndustryCategories),
    ('Node',insight.MetadataIndustries), ('Node',insight.MetadataSpecialtiesCategories),
    ('Node',insight.MetadataSpecialtiesAreas), ('Node',insight.MetadataSpecialtiesPages),
    ('Node',insight.MetadataOfficeLocation), ('Node',insight.MicrositePages),
    ('Node',insight.MicrositeMasterPages), ('Document',insight.RelevantContactPerson),
    ('Guid',insight.RelatedExternalPeople)
) field(Kind,Value)
CROSS APPLY STRING_SPLIT(REPLACE(field.Value,';','|'),'|') token
WHERE LTRIM(RTRIM(token.value))<>'';

SELECT DISTINCT node.NodeGUID, node.NodeAliasPath, node.NodeName, cl.ClassName
INTO #sourceReferences
FROM [bdo-db-gwt-tst2-eur].dbo.CMS_Tree node
JOIN [bdo-db-gwt-tst2-eur].dbo.CMS_Class cl ON cl.ClassID=node.NodeClassID
WHERE EXISTS (
    SELECT 1 FROM #sourceTokens token
    WHERE (token.Kind='Node' AND TRY_CONVERT(int,token.Token)=node.NodeID)
       OR (token.Kind='Guid' AND TRY_CONVERT(uniqueidentifier,token.Token)=node.NodeGUID)
       OR (token.Kind='Document' AND EXISTS (
           SELECT 1 FROM [bdo-db-gwt-tst2-eur].dbo.CMS_Document document
           WHERE document.DocumentNodeID=node.NodeID AND document.DocumentID=TRY_CONVERT(int,token.Token)))
       OR (token.Kind='Node' AND EXISTS (
           SELECT 1 FROM [bdo-db-gwt-tst2-eur].dbo.BDO_LinkedPages linked
           WHERE linked.LinkedPagesID=TRY_CONVERT(int,token.Token)
             AND (linked.LinkedPagesNodeId=node.NodeID OR linked.LinkedPagesNodeGUID=node.NodeGUID)))
);

INSERT INTO #keep
SELECT page.WebPageItemID FROM #pages page
WHERE NOT EXISTS (SELECT 1 FROM #keep kept WHERE kept.PageID=page.WebPageItemID)
  AND EXISTS (
    SELECT 1 FROM #sourceReferences source
    WHERE source.ClassName COLLATE DATABASE_DEFAULT=page.ClassName
      AND (source.NodeGUID=page.ContentItemGUID
        OR source.NodeAliasPath COLLATE DATABASE_DEFAULT=page.WebPageItemTreePath
        OR EXISTS (SELECT 1 FROM CMS_ContentItemLanguageMetadata metadata
            WHERE metadata.ContentItemLanguageMetadataContentItemID=page.WebPageItemContentItemID
              AND LTRIM(RTRIM(metadata.ContentItemLanguageMetadataDisplayName))=LTRIM(RTRIM(source.NodeName)) COLLATE DATABASE_DEFAULT))
);

INSERT INTO #keep
SELECT page.WebPageItemID FROM #pages page
WHERE NOT EXISTS (SELECT 1 FROM #keep kept WHERE kept.PageID=page.WebPageItemID)
  AND (EXISTS (SELECT 1 FROM CMS_WebPageItem other
        WHERE other.WebPageItemContentItemID=page.WebPageItemContentItemID AND other.WebPageItemWebsiteChannelID<>@channelID)
        OR EXISTS (SELECT 1 FROM #edges reference
                WHERE reference.TargetItemID=page.WebPageItemContentItemID
                    AND NOT EXISTS (SELECT 1 FROM #pages source WHERE source.WebPageItemContentItemID=reference.SourceItemID))
    OR EXISTS (SELECT 1 FROM OM_CampaignReference campaign WHERE campaign.CampaignReferenceWebPageItemID=page.WebPageItemID));

DECLARE @added int = 1;
WHILE @added>0
BEGIN
    INSERT INTO #keep
    SELECT DISTINCT candidate.WebPageItemID FROM #pages candidate
    WHERE NOT EXISTS (SELECT 1 FROM #keep kept WHERE kept.PageID=candidate.WebPageItemID)
      AND (EXISTS (
          SELECT 1 FROM #keep kept JOIN #pages page ON page.WebPageItemID=kept.PageID
          WHERE page.WebPageItemParentID=candidate.WebPageItemID
             OR LEFT(page.WebPageItemTreePath,LEN(candidate.WebPageItemTreePath)+1)=candidate.WebPageItemTreePath+'/')
        OR EXISTS (
          SELECT 1 FROM #edges reference
          JOIN #pages source ON source.WebPageItemContentItemID=reference.SourceItemID
          JOIN #keep kept ON kept.PageID=source.WebPageItemID
          WHERE reference.TargetItemID=candidate.WebPageItemContentItemID));
    SET @added=@@ROWCOUNT;
END;

SELECT page.WebPageItemID INTO #deletePages FROM #pages page
WHERE NOT EXISTS (SELECT 1 FROM #keep kept WHERE kept.PageID=page.WebPageItemID);

SELECT page.ClassName, COUNT(*) PagesToRemove
FROM #pages page JOIN #deletePages removed ON removed.WebPageItemID=page.WebPageItemID
GROUP BY page.ClassName ORDER BY page.ClassName;
SELECT (SELECT COUNT(*) FROM #keep) KeptPages, (SELECT COUNT(*) FROM #deletePages) RemovedPages, @apply ApplyChanges;

IF @apply=1
BEGIN
    DELETE FROM CMS_WebPageAclMapping WHERE WebPageAclMappingWebPageItemID IN (SELECT WebPageItemID FROM #deletePages);
    DELETE FROM CMS_WebPageAcl WHERE WebPageAclWebPageItemID IN (SELECT WebPageItemID FROM #deletePages);
    DELETE FROM CMS_WebPageScope WHERE WebPageScopeWebPageItemID IN (SELECT WebPageItemID FROM #deletePages);
    DELETE FROM CMS_WebPageUrlPath WHERE WebPageUrlPathWebPageItemID IN (SELECT WebPageItemID FROM #deletePages);
    DELETE FROM CMS_WebPageFormerUrlPath
    WHERE WebPageFormerUrlPathWebPageItemID IN (SELECT WebPageItemID FROM #deletePages)
       OR WebPageFormerUrlPathSourceWebPageItemID IN (SELECT WebPageItemID FROM #deletePages);
    UPDATE CMS_WebPageItem SET WebPageItemParentID=NULL WHERE WebPageItemID IN (SELECT WebPageItemID FROM #deletePages);
    DELETE FROM CMS_WebPageItem WHERE WebPageItemID IN (SELECT WebPageItemID FROM #deletePages);
END;

COMMIT TRANSACTION;