SET QUOTED_IDENTIFIER ON;
SET XACT_ABORT ON;
BEGIN TRANSACTION;

DECLARE @channel int = 10;
DECLARE @lang int = 1;

SELECT cd.ContentItemCommonDataID CommonID, source.Industry, source.SubIndustry, source.DealType
INTO #dealSource
FROM CMS_WebPageItem page
JOIN CMS_ContentItem item ON item.ContentItemID=page.WebPageItemContentItemID
JOIN CMS_Class cl ON cl.ClassID=item.ContentItemContentTypeID AND cl.ClassName='BDO.RemoteDeal'
JOIN CMS_ContentItemCommonData cd ON cd.ContentItemCommonDataContentItemID=item.ContentItemID
    AND cd.ContentItemCommonDataContentLanguageID=@lang
JOIN BDO_RemoteDeal remote ON remote.ContentItemDataCommonDataID=cd.ContentItemCommonDataID
CROSS APPLY (
    SELECT TOP 1 deal.Industry,deal.SubIndustry,deal.DealType
    FROM [BDO-DB-GWT-TST-EUR].dbo.CMS_Tree node
    JOIN [BDO-DB-GWT-TST-EUR].dbo.CMS_Class sourceClass ON sourceClass.ClassID=node.NodeClassID AND sourceClass.ClassName='BDO.Deal'
    JOIN [BDO-DB-GWT-TST-EUR].dbo.CMS_Document document ON document.DocumentNodeID=node.NodeID
    JOIN [BDO-DB-GWT-TST-EUR].dbo.BDO_Deal deal ON deal.DealID=document.DocumentForeignKeyValue
    WHERE node.NodeSiteID=1 AND node.NodeGUID=remote.OriginalPageGUID
      AND (document.DocumentCulture='en-GB' OR (LOWER(document.DocumentCulture)='en-gb' AND @lang=(
        SELECT WebsiteChannelPrimaryContentLanguageID FROM CMS_WebsiteChannel WHERE WebsiteChannelID=@channel)))
    ORDER BY CASE WHEN LOWER(document.DocumentCulture)='en-gb' THEN 1 ELSE 0 END
) source
WHERE page.WebPageItemWebsiteChannelID=@channel;

SELECT CAST('Industry' AS varchar(30)) FieldName,CodeName INTO #lookupValues FROM BDO_DealCardIndustry_1 WHERE 1=0;
ALTER TABLE #lookupValues ADD CommonID int,TargetItemID int,Identifier uniqueidentifier;

INSERT INTO #lookupValues (FieldName,CodeName,CommonID,TargetItemID,Identifier)
SELECT lookup.FieldName,lookup.CodeName,lookup.CommonID,page.WebPageItemContentItemID,item.ContentItemGUID
FROM (
    SELECT 'Industry' FieldName,CodeName,ContentItemDataCommonDataID CommonID FROM BDO_DealCardIndustry_1
    UNION ALL SELECT 'SubIndustry',CodeName,ContentItemDataCommonDataID FROM BDO_DealCardSubIndustry
    UNION ALL SELECT 'DealType',CodeName,ContentItemDataCommonDataID FROM BDO_DealCardDealType
) lookup
JOIN CMS_ContentItemCommonData cd ON cd.ContentItemCommonDataID=lookup.CommonID
JOIN CMS_WebPageItem page ON page.WebPageItemContentItemID=cd.ContentItemCommonDataContentItemID AND page.WebPageItemWebsiteChannelID=@channel
JOIN CMS_ContentItem item ON item.ContentItemID=page.WebPageItemContentItemID;

SELECT DISTINCT source.CommonID,field.FieldName,lookup.TargetItemID,lookup.Identifier
INTO #resolvedDeals
FROM #dealSource source
CROSS APPLY (VALUES ('Industry',source.Industry),('SubIndustry',source.SubIndustry),('DealType',source.DealType)) field(FieldName,Value)
CROSS APPLY STRING_SPLIT(REPLACE(field.Value,';','|'),'|') token
LEFT JOIN [BDO-DB-GWT-TST-EUR].dbo.BDODealCards_DealType dealType
    ON field.FieldName='DealType' AND dealType.DealTypeID=TRY_CONVERT(int,token.value)
JOIN #lookupValues lookup ON lookup.FieldName=field.FieldName
    AND lookup.CodeName=COALESCE(dealType.DealTypeCodeName,LTRIM(RTRIM(token.value))) COLLATE DATABASE_DEFAULT;

UPDATE remote SET
    Industry=COALESCE(industry.JsonValue,'[]'),
    SubIndustry=COALESCE(subIndustry.JsonValue,'[]'),
    DealType=COALESCE(dealType.JsonValue,'[]')
FROM BDO_RemoteDeal remote JOIN #dealSource source ON source.CommonID=remote.ContentItemDataCommonDataID
OUTER APPLY (SELECT '['+STRING_AGG(CONVERT(nvarchar(max),CONCAT('{"Identifier":"',LOWER(CONVERT(nvarchar(36),Identifier)),'"}')),',')+']' JsonValue FROM #resolvedDeals WHERE CommonID=source.CommonID AND FieldName='Industry') industry
OUTER APPLY (SELECT '['+STRING_AGG(CONVERT(nvarchar(max),CONCAT('{"Identifier":"',LOWER(CONVERT(nvarchar(36),Identifier)),'"}')),',')+']' JsonValue FROM #resolvedDeals WHERE CommonID=source.CommonID AND FieldName='SubIndustry') subIndustry
OUTER APPLY (SELECT '['+STRING_AGG(CONVERT(nvarchar(max),CONCAT('{"Identifier":"',LOWER(CONVERT(nvarchar(36),Identifier)),'"}')),',')+']' JsonValue FROM #resolvedDeals WHERE CommonID=source.CommonID AND FieldName='DealType') dealType;

INSERT INTO CMS_ContentItemReference (ContentItemReferenceGUID,ContentItemReferenceSourceCommonDataID,ContentItemReferenceTargetItemID,ContentItemReferenceGroupGUID)
SELECT NEWID(),resolved.CommonID,resolved.TargetItemID,NEWID()
FROM (SELECT DISTINCT CommonID,TargetItemID FROM #resolvedDeals) resolved
WHERE NOT EXISTS (SELECT 1 FROM CMS_ContentItemReference existing WHERE existing.ContentItemReferenceSourceCommonDataID=resolved.CommonID AND existing.ContentItemReferenceTargetItemID=resolved.TargetItemID);

SELECT COUNT(*) RemoteDealVersionsCorrelated FROM #dealSource;
SELECT FieldName,COUNT(*) ReferencesMapped FROM #resolvedDeals GROUP BY FieldName;
COMMIT TRANSACTION;