SET XACT_ABORT ON;
SET QUOTED_IDENTIFIER ON;
BEGIN TRANSACTION;

DECLARE @channelID int = (
    SELECT wc.WebsiteChannelID FROM CMS_WebsiteChannel wc
    JOIN CMS_Channel ch ON ch.ChannelID=wc.WebsiteChannelChannelID
    WHERE ch.ChannelName='Switzerland'
);
IF @channelID IS NULL THROW 50000, 'Switzerland channel was not found.', 1;

SELECT wp.WebPageItemID INTO #emptyPages
FROM CMS_WebPageItem wp
JOIN CMS_ContentItem ci ON ci.ContentItemID=wp.WebPageItemContentItemID
JOIN [bdo-db-gwt-tst2-eur].dbo.CMS_Tree sourceNode ON sourceNode.NodeGUID=ci.ContentItemGUID
WHERE wp.WebPageItemWebsiteChannelID=@channelID
  AND sourceNode.NodeSiteID=17
  AND NOT EXISTS (SELECT 1 FROM [bdo-db-gwt-tst2-eur].dbo.CMS_Document sourceDocument WHERE sourceDocument.DocumentNodeID=sourceNode.NodeID)
  AND NOT EXISTS (SELECT 1 FROM CMS_ContentItemCommonData cd WHERE cd.ContentItemCommonDataContentItemID=ci.ContentItemID)
  AND NOT EXISTS (SELECT 1 FROM CMS_ContentItemLanguageMetadata lm WHERE lm.ContentItemLanguageMetadataContentItemID=ci.ContentItemID)
  AND NOT EXISTS (SELECT 1 FROM CMS_WebPageItem child WHERE child.WebPageItemParentID=wp.WebPageItemID);

DELETE FROM CMS_WebPageAclMapping WHERE WebPageAclMappingWebPageItemID IN (SELECT WebPageItemID FROM #emptyPages);
DELETE FROM CMS_WebPageUrlPath WHERE WebPageUrlPathWebPageItemID IN (SELECT WebPageItemID FROM #emptyPages);
DELETE FROM CMS_WebPageItem WHERE WebPageItemID IN (SELECT WebPageItemID FROM #emptyPages);
SELECT @@ROWCOUNT AS RemovedDocumentlessTreeEntries;

INSERT INTO BDO_SectionPeoplePage (
    ContentItemDataCommonDataID, ContentItemDataGUID,
    PeopleListFilteringEnabled, PeopleListFilters, PeopleListOrderingEnabled,
    PeopleListDefaultOrder, EnableForm, DisplayEmailAddressInPersonPage,
    DisplayFullEmailAddress, DocumentName
)
SELECT cd.ContentItemCommonDataID, NEWID(),
    sourceData.PeopleListFilteringEnabled, sourceData.PeopleListFilters,
    sourceData.PeopleListOrderingEnabled, sourceData.PeopleListDefaultOrder,
    COALESCE(sourceData.EnableForm,0), COALESCE(sourceData.DisplayEmailAddressInPersonPage,0),
    COALESCE(sourceData.DisplayFullEmailAddress,0), COALESCE(sourceDocument.DocumentName,'')
FROM CMS_WebPageItem wp
JOIN CMS_ContentItem ci ON ci.ContentItemID=wp.WebPageItemContentItemID
JOIN CMS_Class cl ON cl.ClassID=ci.ContentItemContentTypeID AND cl.ClassName='BDO.SectionPeoplePage'
JOIN CMS_ContentItemCommonData cd ON cd.ContentItemCommonDataContentItemID=ci.ContentItemID
JOIN CMS_ContentLanguage language ON language.ContentLanguageID=cd.ContentItemCommonDataContentLanguageID
JOIN [bdo-db-gwt-tst2-eur].dbo.CMS_Tree sourceNode ON sourceNode.NodeSiteID=17
    AND sourceNode.NodeAliasPath COLLATE DATABASE_DEFAULT=wp.WebPageItemTreePath
JOIN [bdo-db-gwt-tst2-eur].dbo.CMS_Document sourceDocument ON sourceDocument.DocumentNodeID=sourceNode.NodeID
    AND sourceDocument.DocumentCulture COLLATE DATABASE_DEFAULT=language.ContentLanguageCultureFormat
JOIN [bdo-db-gwt-tst2-eur].dbo.BDO_SectionPeoplePage sourceData ON sourceData.SectionPeoplePageID=sourceDocument.DocumentForeignKeyValue
WHERE wp.WebPageItemWebsiteChannelID=@channelID
  AND NOT EXISTS (SELECT 1 FROM BDO_SectionPeoplePage existing WHERE existing.ContentItemDataCommonDataID=cd.ContentItemCommonDataID);
SELECT @@ROWCOUNT AS RestoredPeopleSectionVersions;

COMMIT TRANSACTION;