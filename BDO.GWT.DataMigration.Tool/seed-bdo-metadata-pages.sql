SET QUOTED_IDENTIFIER ON;
SET XACT_ABORT ON;
SET NOCOUNT ON;
BEGIN TRANSACTION;

DECLARE @websiteChannelID int = 10;
DECLARE @seedRun uniqueidentifier = NEWID();
DECLARE @workspaceID int = 1;
DECLARE @languageID int = 1;
DECLARE @now datetime2 = SYSUTCDATETIME();
DECLARE @rootPath nvarchar(450) = N'/BDO-Metadata';
DECLARE @rootContentItemID int;
DECLARE @rootWebPageItemID int;

DECLARE @class TABLE (ClassName nvarchar(200) PRIMARY KEY, ClassID int, ClassGUID uniqueidentifier, ClassTableName sysname);
INSERT INTO @class (ClassName, ClassID, ClassGUID, ClassTableName)
SELECT ClassName, ClassID, ClassGUID, ClassTableName
FROM CMS_Class
WHERE ClassName IN (
    'BDO.BDOMetadata','BDO.BusinessIssuesFolder','BDO.Businessissue','BDO.CareerEmploymentTypesFolder','BDO.CareerEmploymentTypes',
    'BDO.CareerJobTitleFolder','BDO.CareerJobTitle','BDO.CareerLevelsFolder','BDO.CareerLevels','BDO.CareerTypesFolder','BDO.CareerTypes',
    'BDO.ContentTypesFolder','BDO.Contenttypes','BDO.CredentialsFolder','BDO.Credentials','BDO.EventTypeFolder','BDO.EventType',
    'BDO.ProfileGroupsFolder','BDO.ProfileGroups','BDO.ProfileRelationshipsFolder','BDO.ProfileRelationships','BDO.RSSCategoriesFolder','BDO.RSSCategories',
    'BDO.TestimonialClientTypesFolder','BDO.TestimonialClientTypes','BDO.TestimonialIndustriesFolder','BDO.TestimonialIndustries',
    'BDO.TestimonialServiceLineFolder','BDO.TestimonialServiceLine'
);

CREATE TABLE #folders (
    SortOrder int PRIMARY KEY,
    FolderClass nvarchar(200),
    ItemClass nvarchar(200),
    FolderTitle nvarchar(200),
    FolderSlug nvarchar(200),
    SourceTable sysname,
    NameColumn sysname,
    CodeColumn sysname,
    ExtraColumn sysname NULL,
    ExtraTargetColumn sysname NULL,
    SourceSiteColumn sysname NULL,
    CultureColumn sysname NULL
);

INSERT INTO #folders VALUES
(10,'BDO.BusinessIssuesFolder','BDO.Businessissue','Business issues','Business-Issues','BDOMetadata_BusinessIssue','BusinessIssueName','BusinessIssueCodeName',NULL,NULL,'BusinessIssueSiteID','BusinessIssueCultureCode'),
(20,'BDO.CareerEmploymentTypesFolder','BDO.CareerEmploymentTypes','Career employment types','Career-Employment-Types','BDOMetadata_CareerEmploymentType','CareerEmploymentTypeName','CareerEmploymentTypeCodeName',NULL,NULL,'CareerEmploymentTypeSiteID','CareerEmploymentTypeCultureCode'),
(30,'BDO.CareerJobTitleFolder','BDO.CareerJobTitle','Career job titles','Career-Job-Titles','BDOMetadata_CareerJobTitle','CareerJobTitleName','CareerJobTitleCodeName',NULL,NULL,'CareerJobTitleSiteID','CareerJobTitleCultureCode'),
(40,'BDO.CareerLevelsFolder','BDO.CareerLevels','Career levels','Career-Levels','BDOMetadata_CareerLevel','CareerLevelName','CareerLevelCodeName',NULL,NULL,'CareerLevelSiteID','CareerLevelCultureCode'),
(50,'BDO.CareerTypesFolder','BDO.CareerTypes','Career types','Career-Types','BDOMetadata_CareerType','CareerTypeName','CareerTypeCodeName',NULL,NULL,'CareerTypeSiteID','CareerTypeCultureCode'),
(60,'BDO.ContentTypesFolder','BDO.Contenttypes','Content types','Content-Types','BDOMetadata_ContentType','ContentTypeName','ContentTypeCodeName',NULL,NULL,'ContentTypeSiteID','ContentTypeCultureCode'),
(70,'BDO.CredentialsFolder','BDO.Credentials','Credentials','Credentials','BDOMetadata_Credentials','CredentialsName','CredentialsCodeName',NULL,NULL,'CredentialsSiteID','CredentialsCultureCode'),
(80,'BDO.EventTypeFolder','BDO.EventType','Event types','Event-Types','BDOMetadata_EventType','EventTypeName','EventTypeCodeName','EventTypeCallToActionText','Calltoactiontext','EventTypeSiteID','EventTypeCultureCode'),
(90,'BDO.ProfileGroupsFolder','BDO.ProfileGroups','Profile groups','Profile-Groups','BDOMetadata_ProfileGroup','ProfileGroupName','ProfileGroupCodeName',NULL,NULL,'ProfileGroupSiteID','ProfileGroupCultureCode'),
(100,'BDO.ProfileRelationshipsFolder','BDO.ProfileRelationships','Profile relationships','Profile-Relationships','BDOMetadata_ProfileRelationship','ProfileRelationshipName','ProfileRelationshipCodeName',NULL,NULL,'ProfileRelationshipSiteID','ProfileRelationshipCultureCode'),
(110,'BDO.RSSCategoriesFolder','BDO.RSSCategories','RSS categories','RSS-Categories','BDOMetadata_RSSCategory','RSSCategoryName','RSSCategoryCodeName',NULL,NULL,'RSSCategorySiteID','RSSCategoryCultureCode'),
(120,'BDO.TestimonialClientTypesFolder','BDO.TestimonialClientTypes','Testimonial client types','Testimonial-Client-Types','BDOMetadata_TestimonialClientType','ClientTypeName','ClientTypeCodeName',NULL,NULL,'TestimonialClientTypeSiteID','ClientTypeCultureCode'),
(130,'BDO.TestimonialIndustriesFolder','BDO.TestimonialIndustries','Testimonial industries','Testimonial-Industries','BDOMetadata_TestimonialIndustry','IndustryName','IndustryCodeName','IndustryName','ClientType','TestimonialIndustrySiteID','IndustryCultureCode'),
(140,'BDO.TestimonialServiceLineFolder','BDO.TestimonialServiceLine','Testimonial service lines','Testimonial-Service-Lines','BDOMetadata_TestimonialServiceLine','ServiceLineName','ServiceLineCodeName',NULL,NULL,'TestimonialServiceLineSiteID','ServiceLineCultureCode');

DECLARE @deletePages TABLE (ID int PRIMARY KEY);
INSERT INTO @deletePages
SELECT WebPageItemID
FROM CMS_WebPageItem
WHERE WebPageItemWebsiteChannelID=@websiteChannelID
  AND (WebPageItemTreePath = @rootPath OR WebPageItemTreePath LIKE @rootPath + '/%');

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
    DELETE FROM BDO_BDOMetadata WHERE ContentItemDataCommonDataID IN (SELECT ID FROM @deleteCommon);
    DELETE FROM BDO_BusinessIssuesFolder WHERE ContentItemDataCommonDataID IN (SELECT ID FROM @deleteCommon);
    DELETE FROM BDO_Businessissues WHERE ContentItemDataCommonDataID IN (SELECT ID FROM @deleteCommon);
    DELETE FROM BDO_CareerEmploymentTypesFolder WHERE ContentItemDataCommonDataID IN (SELECT ID FROM @deleteCommon);
    DELETE FROM BDO_CareerEmploymentTypes WHERE ContentItemDataCommonDataID IN (SELECT ID FROM @deleteCommon);
    DELETE FROM BDO_CareerJobTitleFolder WHERE ContentItemDataCommonDataID IN (SELECT ID FROM @deleteCommon);
    DELETE FROM BDO_CareerJobTitle WHERE ContentItemDataCommonDataID IN (SELECT ID FROM @deleteCommon);
    DELETE FROM BDO_CareerLevelsFolder WHERE ContentItemDataCommonDataID IN (SELECT ID FROM @deleteCommon);
    DELETE FROM BDO_CareerLevels WHERE ContentItemDataCommonDataID IN (SELECT ID FROM @deleteCommon);
    DELETE FROM BDO_CareerTypesFolder WHERE ContentItemDataCommonDataID IN (SELECT ID FROM @deleteCommon);
    DELETE FROM BDO_CareerTypes WHERE ContentItemDataCommonDataID IN (SELECT ID FROM @deleteCommon);
    DELETE FROM BDO_ContentTypesFolder WHERE ContentItemDataCommonDataID IN (SELECT ID FROM @deleteCommon);
    DELETE FROM BDO_Contenttypes WHERE ContentItemDataCommonDataID IN (SELECT ID FROM @deleteCommon);
    DELETE FROM BDO_CredentialsFolder WHERE ContentItemDataCommonDataID IN (SELECT ID FROM @deleteCommon);
    DELETE FROM BDO_Credentials WHERE ContentItemDataCommonDataID IN (SELECT ID FROM @deleteCommon);
    DELETE FROM BDO_EventTypeFolder WHERE ContentItemDataCommonDataID IN (SELECT ID FROM @deleteCommon);
    DELETE FROM BDO_EventType WHERE ContentItemDataCommonDataID IN (SELECT ID FROM @deleteCommon);
    DELETE FROM BDO_ProfileGroupsFolder WHERE ContentItemDataCommonDataID IN (SELECT ID FROM @deleteCommon);
    DELETE FROM BDO_ProfileGroup WHERE ContentItemDataCommonDataID IN (SELECT ID FROM @deleteCommon);
    DELETE FROM BDO_ProfileRelationshipsFolder WHERE ContentItemDataCommonDataID IN (SELECT ID FROM @deleteCommon);
    DELETE FROM BDO_ProfileRelationships WHERE ContentItemDataCommonDataID IN (SELECT ID FROM @deleteCommon);
    DELETE FROM BDO_RSSCategoriesFolder WHERE ContentItemDataCommonDataID IN (SELECT ID FROM @deleteCommon);
    DELETE FROM BDO_RSSCategories WHERE ContentItemDataCommonDataID IN (SELECT ID FROM @deleteCommon);
    DELETE FROM BDO_TestimonialClientTypesFolder WHERE ContentItemDataCommonDataID IN (SELECT ID FROM @deleteCommon);
    DELETE FROM BDO_TestimonialClientTypes WHERE ContentItemDataCommonDataID IN (SELECT ID FROM @deleteCommon);
    DELETE FROM BDO_TestimonialIndustriesFolder WHERE ContentItemDataCommonDataID IN (SELECT ID FROM @deleteCommon);
    DELETE FROM BDO_TestimonialIndustries WHERE ContentItemDataCommonDataID IN (SELECT ID FROM @deleteCommon);
    DELETE FROM BDO_TestimonialServiceLineFolder WHERE ContentItemDataCommonDataID IN (SELECT ID FROM @deleteCommon);
    DELETE FROM BDO_TestimonialServiceLine WHERE ContentItemDataCommonDataID IN (SELECT ID FROM @deleteCommon);
    DELETE FROM CMS_WebPageUrlPath WHERE WebPageUrlPathWebPageItemID IN (SELECT ID FROM @deletePages);
    DELETE FROM CMS_WebPageFormerUrlPath WHERE WebPageFormerUrlPathWebPageItemID IN (SELECT ID FROM @deletePages) OR WebPageFormerUrlPathSourceWebPageItemID IN (SELECT ID FROM @deletePages);
    DELETE FROM CMS_WebPageAcl WHERE WebPageAclWebPageItemID IN (SELECT ID FROM @deletePages);
    DELETE FROM CMS_WebPageAclMapping WHERE WebPageAclMappingWebPageItemID IN (SELECT ID FROM @deletePages);
    DELETE FROM CMS_WebPageItem WHERE WebPageItemID IN (SELECT ID FROM @deletePages);
    DELETE FROM CMS_ContentItemCommonData WHERE ContentItemCommonDataID IN (SELECT ID FROM @deleteCommon);
    DELETE FROM CMS_ContentItem WHERE ContentItemID IN (SELECT ID FROM @deleteItems);
END

CREATE TABLE #created (ContentItemID int, CommonID int, WebPageID int);
CREATE TABLE #items (Name nvarchar(450), CodeName nvarchar(450), Extra nvarchar(max));

-- Root
INSERT INTO CMS_ContentItem (ContentItemGUID, ContentItemName, ContentItemIsReusable, ContentItemIsSecured, ContentItemContentTypeID, ContentItemChannelID, ContentItemContentFolderID, ContentItemWorkspaceID)
VALUES (NEWID(), CONCAT('BDOMetadata-',@websiteChannelID,'-',@seedRun,'-Root'), 0, 0, (SELECT ClassID FROM @class WHERE ClassName='BDO.BDOMetadata'), NULL, NULL, @workspaceID);
SET @rootContentItemID=SCOPE_IDENTITY();
INSERT INTO CMS_WebPageItem (WebPageItemParentID, WebPageItemGUID, WebPageItemName, WebPageItemTreePath, WebPageItemWebsiteChannelID, WebPageItemContentItemID, WebPageItemOrder)
VALUES (NULL, (SELECT ContentItemGUID FROM CMS_ContentItem WHERE ContentItemID=@rootContentItemID), 'BDO Metadata', @rootPath, @websiteChannelID, @rootContentItemID, 100);
SET @rootWebPageItemID=SCOPE_IDENTITY();
INSERT INTO CMS_ContentItemCommonData (ContentItemCommonDataGUID, ContentItemCommonDataContentItemID, ContentItemCommonDataContentLanguageID, ContentItemCommonDataVersionStatus, ContentItemCommonDataIsLatest)
VALUES (NEWID(), @rootContentItemID, @languageID, 2, 1);
DECLARE @rootCommonID int=SCOPE_IDENTITY();
INSERT INTO BDO_BDOMetadata (ContentItemDataCommonDataID, ContentItemDataGUID) VALUES (@rootCommonID, NEWID());
INSERT INTO CMS_ContentItemLanguageMetadata (ContentItemLanguageMetadataContentItemID, ContentItemLanguageMetadataDisplayName, ContentItemLanguageMetadataLatestVersionStatus, ContentItemLanguageMetadataGUID, ContentItemLanguageMetadataCreatedWhen, ContentItemLanguageMetadataModifiedWhen, ContentItemLanguageMetadataHasImageAsset, ContentItemLanguageMetadataContentLanguageID, ContentItemLanguageMetadataVersionTimestamp)
VALUES (@rootContentItemID, 'BDO Metadata', 2, NEWID(), @now, @now, 0, @languageID, @now);

DECLARE @sort int,@folderClass nvarchar(200),@itemClass nvarchar(200),@folderTitle nvarchar(200),@folderSlug nvarchar(200),@sourceTable sysname,@nameColumn sysname,@codeColumn sysname,@extraColumn sysname,@extraTargetColumn sysname,@siteColumn sysname,@cultureColumn sysname;
DECLARE cur CURSOR LOCAL FAST_FORWARD FOR SELECT SortOrder,FolderClass,ItemClass,FolderTitle,FolderSlug,SourceTable,NameColumn,CodeColumn,ExtraColumn,ExtraTargetColumn,SourceSiteColumn,CultureColumn FROM #folders ORDER BY SortOrder;
OPEN cur;
FETCH NEXT FROM cur INTO @sort,@folderClass,@itemClass,@folderTitle,@folderSlug,@sourceTable,@nameColumn,@codeColumn,@extraColumn,@extraTargetColumn,@siteColumn,@cultureColumn;
WHILE @@FETCH_STATUS=0
BEGIN
    DECLARE @folderContentID int,@folderWebID int,@folderCommonID int,@folderPath nvarchar(450)=@rootPath+'/'+@folderSlug;
    INSERT INTO CMS_ContentItem (ContentItemGUID, ContentItemName, ContentItemIsReusable, ContentItemIsSecured, ContentItemContentTypeID, ContentItemChannelID, ContentItemContentFolderID, ContentItemWorkspaceID)
    VALUES (NEWID(), LEFT(CONCAT('BDOMetadata-',@websiteChannelID,'-',@seedRun,'-', @folderClass, '-', @folderTitle), 100), 0, 0, (SELECT ClassID FROM @class WHERE ClassName=@folderClass), NULL, NULL, @workspaceID);
    SET @folderContentID=SCOPE_IDENTITY();
    INSERT INTO CMS_WebPageItem (WebPageItemParentID, WebPageItemGUID, WebPageItemName, WebPageItemTreePath, WebPageItemWebsiteChannelID, WebPageItemContentItemID, WebPageItemOrder)
    VALUES (@rootWebPageItemID, (SELECT ContentItemGUID FROM CMS_ContentItem WHERE ContentItemID=@folderContentID), LEFT(CONCAT('BDOMetadata-', @folderSlug), 450), @folderPath, @websiteChannelID, @folderContentID, @sort);
    SET @folderWebID=SCOPE_IDENTITY();
    INSERT INTO CMS_ContentItemCommonData (ContentItemCommonDataGUID, ContentItemCommonDataContentItemID, ContentItemCommonDataContentLanguageID, ContentItemCommonDataVersionStatus, ContentItemCommonDataIsLatest)
    VALUES (NEWID(), @folderContentID, @languageID, 2, 1);
    SET @folderCommonID=SCOPE_IDENTITY();
    DECLARE @folderTable sysname=(SELECT ClassTableName FROM @class WHERE ClassName=@folderClass);
    DECLARE @folderSql nvarchar(max)=N'INSERT INTO '+QUOTENAME(@folderTable)+N' (ContentItemDataCommonDataID, ContentItemDataGUID, Title) VALUES (@commonID, NEWID(), @title);';
    EXEC sp_executesql @folderSql, N'@commonID int, @title nvarchar(200)', @commonID=@folderCommonID, @title=@folderTitle;
    INSERT INTO CMS_ContentItemLanguageMetadata (ContentItemLanguageMetadataContentItemID, ContentItemLanguageMetadataDisplayName, ContentItemLanguageMetadataLatestVersionStatus, ContentItemLanguageMetadataGUID, ContentItemLanguageMetadataCreatedWhen, ContentItemLanguageMetadataModifiedWhen, ContentItemLanguageMetadataHasImageAsset, ContentItemLanguageMetadataContentLanguageID, ContentItemLanguageMetadataVersionTimestamp)
    VALUES (@folderContentID, @folderTitle, 2, NEWID(), @now, @now, 0, @languageID, @now);

    TRUNCATE TABLE #items;
    DECLARE @loadSql nvarchar(max)=N'INSERT INTO #items(Name,CodeName,Extra) SELECT CAST('+QUOTENAME(@nameColumn)+N' AS nvarchar(450)), CAST('+QUOTENAME(@codeColumn)+N' AS nvarchar(450)), '+CASE WHEN @extraColumn IS NULL THEN N'CAST(NULL AS nvarchar(max))' ELSE N'CAST('+QUOTENAME(@extraColumn)+N' AS nvarchar(max))' END+N' FROM [BDO-DB-GWT-TST-EUR].dbo.'+QUOTENAME(@sourceTable)+N' WHERE '+QUOTENAME(@siteColumn)+N'=1 AND '+QUOTENAME(@cultureColumn)+N'=N''en-GB'' ORDER BY '+QUOTENAME(@nameColumn)+N','+QUOTENAME(@codeColumn)+N';';
    EXEC sp_executesql @loadSql;

    DECLARE @itemName nvarchar(450),@itemCode nvarchar(450),@extra nvarchar(max),@rn int=0;
    DECLARE itemcur CURSOR LOCAL FAST_FORWARD FOR SELECT Name,CodeName,Extra FROM #items ORDER BY Name,CodeName;
    OPEN itemcur;
    FETCH NEXT FROM itemcur INTO @itemName,@itemCode,@extra;
    WHILE @@FETCH_STATUS=0
    BEGIN
        SET @rn+=1;
        DECLARE @itemContentID int,@itemCommonID int,@itemPath nvarchar(450)=LEFT(@folderPath+'/'+REPLACE(REPLACE(REPLACE(COALESCE(NULLIF(@itemCode,''),@itemName),' ','-'),'.','-'),'/','-')+'-'+CAST(@rn AS nvarchar(20)),450);
        INSERT INTO CMS_ContentItem (ContentItemGUID, ContentItemName, ContentItemIsReusable, ContentItemIsSecured, ContentItemContentTypeID, ContentItemChannelID, ContentItemContentFolderID, ContentItemWorkspaceID)
        VALUES (NEWID(), LEFT(CONCAT('BDOMetadata-',@websiteChannelID,'-',@seedRun,'-',@rn,'-', @itemClass, '-', COALESCE(NULLIF(@itemCode,''),@itemName)),100), 0, 0, (SELECT ClassID FROM @class WHERE ClassName=@itemClass), NULL, NULL, @workspaceID);
        SET @itemContentID=SCOPE_IDENTITY();
        INSERT INTO CMS_WebPageItem (WebPageItemParentID, WebPageItemGUID, WebPageItemName, WebPageItemTreePath, WebPageItemWebsiteChannelID, WebPageItemContentItemID, WebPageItemOrder)
        VALUES (@folderWebID, (SELECT ContentItemGUID FROM CMS_ContentItem WHERE ContentItemID=@itemContentID), LEFT(CONCAT('BDOMetadata-', @itemClass, '-', COALESCE(NULLIF(@itemCode,''),@itemName), '-', @rn),450), @itemPath, @websiteChannelID, @itemContentID, @rn);
        INSERT INTO CMS_ContentItemCommonData (ContentItemCommonDataGUID, ContentItemCommonDataContentItemID, ContentItemCommonDataContentLanguageID, ContentItemCommonDataVersionStatus, ContentItemCommonDataIsLatest)
        VALUES (NEWID(), @itemContentID, @languageID, 2, 1);
        SET @itemCommonID=SCOPE_IDENTITY();
        DECLARE @itemTable sysname=(SELECT ClassTableName FROM @class WHERE ClassName=@itemClass);
        DECLARE @dataSql nvarchar(max)=N'INSERT INTO '+QUOTENAME(@itemTable)+N' (ContentItemDataCommonDataID, ContentItemDataGUID, Name, CodeName'+CASE WHEN @extraTargetColumn IS NOT NULL THEN N', '+QUOTENAME(@extraTargetColumn) ELSE N'' END+N') VALUES (@commonID, NEWID(), @name, @code'+CASE WHEN @extraTargetColumn IS NOT NULL THEN N', @extra' ELSE N'' END+N');';
        EXEC sp_executesql @dataSql, N'@commonID int,@name nvarchar(450),@code nvarchar(450),@extra nvarchar(max)', @commonID=@itemCommonID, @name=@itemName, @code=@itemCode, @extra=@extra;
        INSERT INTO CMS_ContentItemLanguageMetadata (ContentItemLanguageMetadataContentItemID, ContentItemLanguageMetadataDisplayName, ContentItemLanguageMetadataLatestVersionStatus, ContentItemLanguageMetadataGUID, ContentItemLanguageMetadataCreatedWhen, ContentItemLanguageMetadataModifiedWhen, ContentItemLanguageMetadataHasImageAsset, ContentItemLanguageMetadataContentLanguageID, ContentItemLanguageMetadataVersionTimestamp)
        VALUES (@itemContentID, @itemName, 2, NEWID(), @now, @now, 0, @languageID, @now);
        FETCH NEXT FROM itemcur INTO @itemName,@itemCode,@extra;
    END
    CLOSE itemcur; DEALLOCATE itemcur;

    FETCH NEXT FROM cur INTO @sort,@folderClass,@itemClass,@folderTitle,@folderSlug,@sourceTable,@nameColumn,@codeColumn,@extraColumn,@extraTargetColumn,@siteColumn,@cultureColumn;
END
CLOSE cur; DEALLOCATE cur;

SELECT cl.ClassName, COUNT(*) AS PageCount
FROM CMS_WebPageItem w JOIN CMS_ContentItem ci ON ci.ContentItemID=w.WebPageItemContentItemID JOIN CMS_Class cl ON cl.ClassID=ci.ContentItemContentTypeID
WHERE w.WebPageItemWebsiteChannelID=@websiteChannelID AND w.WebPageItemTreePath LIKE @rootPath + '%'
GROUP BY cl.ClassName ORDER BY cl.ClassName;

COMMIT TRANSACTION;
