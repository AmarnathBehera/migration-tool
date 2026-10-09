SET QUOTED_IDENTIFIER ON;
SET XACT_ABORT ON;
SET NOCOUNT ON;
BEGIN TRANSACTION;

DECLARE @websiteChannelID int = 10;
DECLARE @seedRun uniqueidentifier = NEWID();
DECLARE @workspaceID int = 1;
DECLARE @languageID int = 1;
DECLARE @now datetime2 = SYSUTCDATETIME();
DECLARE @rootPath nvarchar(450) = N'/BDO-Deal-Cards';

DECLARE @class TABLE (ClassName nvarchar(200) PRIMARY KEY, ClassID int, ClassTableName sysname);
INSERT INTO @class (ClassName, ClassID, ClassTableName)
SELECT ClassName, ClassID, ClassTableName FROM CMS_Class
WHERE ClassName IN ('BDO.DealCards','BDO.DealCardDealType','BDO.DealCardIndustry','BDO.DealCardRange','BDO.DealCardSubIndustry');

-- Idempotent cleanup of existing subtree
DECLARE @deletePages TABLE (ID int PRIMARY KEY);
INSERT INTO @deletePages
SELECT WebPageItemID FROM CMS_WebPageItem
WHERE WebPageItemWebsiteChannelID=@websiteChannelID
  AND (WebPageItemTreePath=@rootPath OR WebPageItemTreePath LIKE @rootPath + '/%');

IF EXISTS (SELECT 1 FROM @deletePages)
BEGIN
    DECLARE @deleteItems TABLE (ID int PRIMARY KEY);
    INSERT INTO @deleteItems SELECT WebPageItemContentItemID FROM CMS_WebPageItem WHERE WebPageItemID IN (SELECT ID FROM @deletePages);
    DECLARE @deleteCommon TABLE (ID int PRIMARY KEY);
    INSERT INTO @deleteCommon SELECT ContentItemCommonDataID FROM CMS_ContentItemCommonData WHERE ContentItemCommonDataContentItemID IN (SELECT ID FROM @deleteItems);
    DECLARE @deleteLang TABLE (ID int PRIMARY KEY);
    INSERT INTO @deleteLang SELECT ContentItemLanguageMetadataID FROM CMS_ContentItemLanguageMetadata WHERE ContentItemLanguageMetadataContentItemID IN (SELECT ID FROM @deleteItems);

    UPDATE CMS_WebPageItem SET WebPageItemParentID=NULL WHERE WebPageItemParentID IN (SELECT ID FROM @deletePages);
    DELETE FROM CMS_ContentItemTag WHERE ContentItemTagContentItemLanguageMetadataID IN (SELECT ID FROM @deleteLang);
    DELETE FROM CMS_ContentItemReference WHERE ContentItemReferenceSourceCommonDataID IN (SELECT ID FROM @deleteCommon) OR ContentItemReferenceTargetItemID IN (SELECT ID FROM @deleteItems);
    DELETE FROM CMS_ContentItemObjectReference WHERE ContentItemObjectReferenceSourceCommonDataID IN (SELECT ID FROM @deleteCommon);
    DELETE FROM CMS_ContentItemLanguageMetadata WHERE ContentItemLanguageMetadataContentItemID IN (SELECT ID FROM @deleteItems);
    DELETE FROM CMS_ContentItemVersion WHERE ContentItemVersionContentItemID IN (SELECT ID FROM @deleteItems);
    DELETE FROM BDO_DealCards WHERE ContentItemDataCommonDataID IN (SELECT ID FROM @deleteCommon);
    DELETE FROM BDO_DealCardDealType WHERE ContentItemDataCommonDataID IN (SELECT ID FROM @deleteCommon);
    DELETE FROM BDO_DealCardIndustry_1 WHERE ContentItemDataCommonDataID IN (SELECT ID FROM @deleteCommon);
    DELETE FROM BDO_DealCardRange WHERE ContentItemDataCommonDataID IN (SELECT ID FROM @deleteCommon);
    DELETE FROM BDO_DealCardSubIndustry WHERE ContentItemDataCommonDataID IN (SELECT ID FROM @deleteCommon);
    DELETE FROM CMS_WebPageUrlPath WHERE WebPageUrlPathWebPageItemID IN (SELECT ID FROM @deletePages);
    DELETE FROM CMS_WebPageFormerUrlPath WHERE WebPageFormerUrlPathWebPageItemID IN (SELECT ID FROM @deletePages) OR WebPageFormerUrlPathSourceWebPageItemID IN (SELECT ID FROM @deletePages);
    DELETE FROM CMS_WebPageAcl WHERE WebPageAclWebPageItemID IN (SELECT ID FROM @deletePages);
    DELETE FROM CMS_WebPageAclMapping WHERE WebPageAclMappingWebPageItemID IN (SELECT ID FROM @deletePages);
    DELETE FROM CMS_WebPageItem WHERE WebPageItemID IN (SELECT ID FROM @deletePages);
    DELETE FROM CMS_ContentItemCommonData WHERE ContentItemCommonDataID IN (SELECT ID FROM @deleteCommon);
    DELETE FROM CMS_ContentItem WHERE ContentItemID IN (SELECT ID FROM @deleteItems);
END

CREATE TABLE #map (Path nvarchar(450) PRIMARY KEY, WebPageID int);
CREATE TABLE #items (Name nvarchar(450), CodeName nvarchar(450));

DECLARE @dealCardsClassID int = (SELECT ClassID FROM @class WHERE ClassName='BDO.DealCards');

-- Root
DECLARE @rootContentID int, @rootWebID int, @rootCommonID int;
INSERT INTO CMS_ContentItem (ContentItemGUID, ContentItemName, ContentItemIsReusable, ContentItemIsSecured, ContentItemContentTypeID, ContentItemChannelID, ContentItemContentFolderID, ContentItemWorkspaceID)
VALUES (NEWID(), CONCAT('DealCards-',@websiteChannelID,'-',@seedRun,'-Root'), 0, 0, @dealCardsClassID, NULL, NULL, @workspaceID);
SET @rootContentID=SCOPE_IDENTITY();
INSERT INTO CMS_WebPageItem (WebPageItemParentID, WebPageItemGUID, WebPageItemName, WebPageItemTreePath, WebPageItemWebsiteChannelID, WebPageItemContentItemID, WebPageItemOrder)
VALUES (NULL, (SELECT ContentItemGUID FROM CMS_ContentItem WHERE ContentItemID=@rootContentID), 'DealCards-Root', @rootPath, @websiteChannelID, @rootContentID, 100);
SET @rootWebID=SCOPE_IDENTITY();
INSERT INTO CMS_ContentItemCommonData (ContentItemCommonDataGUID, ContentItemCommonDataContentItemID, ContentItemCommonDataContentLanguageID, ContentItemCommonDataVersionStatus, ContentItemCommonDataIsLatest)
VALUES (NEWID(), @rootContentID, @languageID, 2, 1);
SET @rootCommonID=SCOPE_IDENTITY();
INSERT INTO BDO_DealCards (ContentItemDataCommonDataID, ContentItemDataGUID) VALUES (@rootCommonID, NEWID());
INSERT INTO CMS_ContentItemLanguageMetadata (ContentItemLanguageMetadataContentItemID, ContentItemLanguageMetadataDisplayName, ContentItemLanguageMetadataLatestVersionStatus, ContentItemLanguageMetadataGUID, ContentItemLanguageMetadataCreatedWhen, ContentItemLanguageMetadataModifiedWhen, ContentItemLanguageMetadataHasImageAsset, ContentItemLanguageMetadataContentLanguageID, ContentItemLanguageMetadataVersionTimestamp)
VALUES (@rootContentID, 'BDO Deal Cards', 2, NEWID(), @now, @now, 0, @languageID, @now);
INSERT INTO #map VALUES (@rootPath, @rootWebID);

-- Folder definitions
CREATE TABLE #folders (
    SortOrder int PRIMARY KEY,
    ParentPath nvarchar(450),
    FolderSlug nvarchar(200),
    FolderTitle nvarchar(200),
    ItemClass nvarchar(200) NULL,
    SourceTable sysname NULL,
    NameColumn sysname NULL,
    CodeColumn sysname NULL,
    CultureColumn sysname NULL,
    SiteColumn sysname NULL,
    OrderColumn sysname NULL,
    TargetHasLanguage bit
);
INSERT INTO #folders VALUES
(10, N'/BDO-Deal-Cards', 'Global-Deals', 'Global Deals', NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0),
(20, N'/BDO-Deal-Cards/Global-Deals', 'Global-Deal-Type', 'Global Deal Type', 'BDO.DealCardDealType', 'BDODealCards_GlobalDealType', 'GlobalDealTypeName', 'GlobalDealTypeCodeName', NULL, NULL, NULL, 1),
(30, N'/BDO-Deal-Cards/Global-Deals', 'Global-Industry', 'Global Industry', 'BDO.DealCardIndustry', 'BDODealCards_GlobalIndustry', 'GlobalIndustryName', 'GlobalIndustryCodeName', NULL, NULL, NULL, 1),
(40, N'/BDO-Deal-Cards/Global-Deals', 'Global-Sub-Industry', 'Global Sub Industry', 'BDO.DealCardSubIndustry', 'BDODealCards_GlobalSubIndustry', 'GlobalSubIndustryName', 'GlobalSubIndustryCodeName', NULL, NULL, NULL, 1),
(50, N'/BDO-Deal-Cards', 'Range', 'Range', 'BDO.DealCardRange', 'BDODealCards_Range', 'RangeValue', 'RangeCodeName', NULL, 'RangeSiteID', 'RangeOrder', 0),
(60, N'/BDO-Deal-Cards', 'Deal-Type', 'Deal Type', 'BDO.DealCardDealType', 'BDODealCards_DealType', 'DealTypeName', 'DealTypeCodeName', 'DealTypeCultureCode', 'DealTypeSiteID', NULL, 1),
(70, N'/BDO-Deal-Cards', 'Industry', 'Industry', 'BDO.DealCardIndustry', 'BDODealCards_Industry', 'IndustryName', 'IndustryCodeName', 'IndustryCultureCode', 'IndustrySiteID', NULL, 1),
(80, N'/BDO-Deal-Cards', 'Sub-Industry', 'Sub Industry', 'BDO.DealCardSubIndustry', 'BDODealCards_SubIndustry', 'SubIndustryName', 'SubIndustryCodeName', 'SubIndustryCultureCode', 'SubIndustrySiteID', NULL, 1);

DECLARE @sort int,@parentPath nvarchar(450),@folderSlug nvarchar(200),@folderTitle nvarchar(200),@itemClass nvarchar(200),
        @sourceTable sysname,@nameColumn sysname,@codeColumn sysname,@cultureColumn sysname,@siteColumn sysname,@orderColumn sysname,@hasLang bit;
DECLARE cur CURSOR LOCAL FAST_FORWARD FOR
    SELECT SortOrder,ParentPath,FolderSlug,FolderTitle,ItemClass,SourceTable,NameColumn,CodeColumn,CultureColumn,SiteColumn,OrderColumn,TargetHasLanguage
    FROM #folders ORDER BY SortOrder;
OPEN cur;
FETCH NEXT FROM cur INTO @sort,@parentPath,@folderSlug,@folderTitle,@itemClass,@sourceTable,@nameColumn,@codeColumn,@cultureColumn,@siteColumn,@orderColumn,@hasLang;
WHILE @@FETCH_STATUS=0
BEGIN
    DECLARE @parentWebID int = (SELECT WebPageID FROM #map WHERE Path=@parentPath);
    DECLARE @folderPath nvarchar(450) = @parentPath + '/' + @folderSlug;
    DECLARE @folderContentID int,@folderWebID int,@folderCommonID int;
    INSERT INTO CMS_ContentItem (ContentItemGUID, ContentItemName, ContentItemIsReusable, ContentItemIsSecured, ContentItemContentTypeID, ContentItemChannelID, ContentItemContentFolderID, ContentItemWorkspaceID)
    VALUES (NEWID(), LEFT(CONCAT('DealCards-',@websiteChannelID,'-',@seedRun,'-Folder-', @folderSlug),100), 0, 0, @dealCardsClassID, NULL, NULL, @workspaceID);
    SET @folderContentID=SCOPE_IDENTITY();
    INSERT INTO CMS_WebPageItem (WebPageItemParentID, WebPageItemGUID, WebPageItemName, WebPageItemTreePath, WebPageItemWebsiteChannelID, WebPageItemContentItemID, WebPageItemOrder)
    VALUES (@parentWebID, (SELECT ContentItemGUID FROM CMS_ContentItem WHERE ContentItemID=@folderContentID), LEFT(CONCAT('DealCards-Folder-', @folderSlug),450), @folderPath, @websiteChannelID, @folderContentID, @sort);
    SET @folderWebID=SCOPE_IDENTITY();
    INSERT INTO CMS_ContentItemCommonData (ContentItemCommonDataGUID, ContentItemCommonDataContentItemID, ContentItemCommonDataContentLanguageID, ContentItemCommonDataVersionStatus, ContentItemCommonDataIsLatest)
    VALUES (NEWID(), @folderContentID, @languageID, 2, 1);
    SET @folderCommonID=SCOPE_IDENTITY();
    INSERT INTO BDO_DealCards (ContentItemDataCommonDataID, ContentItemDataGUID) VALUES (@folderCommonID, NEWID());
    INSERT INTO CMS_ContentItemLanguageMetadata (ContentItemLanguageMetadataContentItemID, ContentItemLanguageMetadataDisplayName, ContentItemLanguageMetadataLatestVersionStatus, ContentItemLanguageMetadataGUID, ContentItemLanguageMetadataCreatedWhen, ContentItemLanguageMetadataModifiedWhen, ContentItemLanguageMetadataHasImageAsset, ContentItemLanguageMetadataContentLanguageID, ContentItemLanguageMetadataVersionTimestamp)
    VALUES (@folderContentID, @folderTitle, 2, NEWID(), @now, @now, 0, @languageID, @now);
    INSERT INTO #map VALUES (@folderPath, @folderWebID);

    IF @itemClass IS NOT NULL
    BEGIN
        TRUNCATE TABLE #items;
        DECLARE @where nvarchar(max) = N'';
        IF @siteColumn IS NOT NULL SET @where = @where + N' AND ' + QUOTENAME(@siteColumn) + N'=1';
        IF @cultureColumn IS NOT NULL SET @where = @where + N' AND ' + QUOTENAME(@cultureColumn) + N'=N''en-US''';
        DECLARE @orderBy nvarchar(max) = CASE WHEN @orderColumn IS NOT NULL THEN QUOTENAME(@orderColumn) ELSE QUOTENAME(@nameColumn) END;
        DECLARE @loadSql nvarchar(max) = N'INSERT INTO #items(Name,CodeName) SELECT CAST('+QUOTENAME(@nameColumn)+N' AS nvarchar(450)), CAST('+QUOTENAME(@codeColumn)+N' AS nvarchar(450)) FROM [BDO-DB-GWT-TST-EUR].dbo.'+QUOTENAME(@sourceTable)+N' WHERE 1=1'+@where+N' ORDER BY '+@orderBy+N';';
        EXEC sp_executesql @loadSql;

        DECLARE @itemTable sysname = (SELECT ClassTableName FROM @class WHERE ClassName=@itemClass);
        DECLARE @itemClassID int = (SELECT ClassID FROM @class WHERE ClassName=@itemClass);
        DECLARE @itemName nvarchar(450),@itemCode nvarchar(450),@rn int=0;
        DECLARE itemcur CURSOR LOCAL FAST_FORWARD FOR SELECT Name,CodeName FROM #items;
        OPEN itemcur;
        FETCH NEXT FROM itemcur INTO @itemName,@itemCode;
        WHILE @@FETCH_STATUS=0
        BEGIN
            SET @rn+=1;
            DECLARE @itemContentID int,@itemCommonID int;
            DECLARE @itemPath nvarchar(450)=LEFT(@folderPath+'/'+REPLACE(REPLACE(REPLACE(COALESCE(NULLIF(@itemCode,''),@itemName),' ','-'),'.','-'),'/','-')+'-'+CAST(@rn AS nvarchar(20)),450);
            INSERT INTO CMS_ContentItem (ContentItemGUID, ContentItemName, ContentItemIsReusable, ContentItemIsSecured, ContentItemContentTypeID, ContentItemChannelID, ContentItemContentFolderID, ContentItemWorkspaceID)
            VALUES (NEWID(), LEFT(CONCAT('DealCards-',@websiteChannelID,'-',@seedRun,'-',@rn,'-', @itemClass, '-', COALESCE(NULLIF(@itemCode,''),@itemName)),100), 0, 0, @itemClassID, NULL, NULL, @workspaceID);
            SET @itemContentID=SCOPE_IDENTITY();
            INSERT INTO CMS_WebPageItem (WebPageItemParentID, WebPageItemGUID, WebPageItemName, WebPageItemTreePath, WebPageItemWebsiteChannelID, WebPageItemContentItemID, WebPageItemOrder)
            VALUES (@folderWebID, (SELECT ContentItemGUID FROM CMS_ContentItem WHERE ContentItemID=@itemContentID), LEFT(CONCAT('DealCards-', @itemClass, '-', COALESCE(NULLIF(@itemCode,''),@itemName), '-', @rn),450), @itemPath, @websiteChannelID, @itemContentID, @rn);
            INSERT INTO CMS_ContentItemCommonData (ContentItemCommonDataGUID, ContentItemCommonDataContentItemID, ContentItemCommonDataContentLanguageID, ContentItemCommonDataVersionStatus, ContentItemCommonDataIsLatest)
            VALUES (NEWID(), @itemContentID, @languageID, 2, 1);
            SET @itemCommonID=SCOPE_IDENTITY();
            DECLARE @dataSql nvarchar(max)=N'INSERT INTO '+QUOTENAME(@itemTable)+N' (ContentItemDataCommonDataID, ContentItemDataGUID, Name, CodeName'+CASE WHEN @hasLang=1 THEN N', [Language]' ELSE N'' END+N') VALUES (@commonID, NEWID(), @name, @code'+CASE WHEN @hasLang=1 THEN N', N''en-US''' ELSE N'' END+N');';
            EXEC sp_executesql @dataSql, N'@commonID int,@name nvarchar(450),@code nvarchar(450)', @commonID=@itemCommonID, @name=@itemName, @code=@itemCode;
            INSERT INTO CMS_ContentItemLanguageMetadata (ContentItemLanguageMetadataContentItemID, ContentItemLanguageMetadataDisplayName, ContentItemLanguageMetadataLatestVersionStatus, ContentItemLanguageMetadataGUID, ContentItemLanguageMetadataCreatedWhen, ContentItemLanguageMetadataModifiedWhen, ContentItemLanguageMetadataHasImageAsset, ContentItemLanguageMetadataContentLanguageID, ContentItemLanguageMetadataVersionTimestamp)
            VALUES (@itemContentID, @itemName, 2, NEWID(), @now, @now, 0, @languageID, @now);
            FETCH NEXT FROM itemcur INTO @itemName,@itemCode;
        END
        CLOSE itemcur; DEALLOCATE itemcur;
    END

    FETCH NEXT FROM cur INTO @sort,@parentPath,@folderSlug,@folderTitle,@itemClass,@sourceTable,@nameColumn,@codeColumn,@cultureColumn,@siteColumn,@orderColumn,@hasLang;
END
CLOSE cur; DEALLOCATE cur;

SELECT cl.ClassName, COUNT(*) AS PageCount
FROM CMS_WebPageItem w JOIN CMS_ContentItem ci ON ci.ContentItemID=w.WebPageItemContentItemID JOIN CMS_Class cl ON cl.ClassID=ci.ContentItemContentTypeID
WHERE w.WebPageItemWebsiteChannelID=@websiteChannelID AND w.WebPageItemTreePath LIKE @rootPath + '%'
GROUP BY cl.ClassName ORDER BY cl.ClassName;

COMMIT TRANSACTION;
