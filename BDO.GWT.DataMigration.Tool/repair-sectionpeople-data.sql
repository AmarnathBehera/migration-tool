SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRANSACTION;

DECLARE @channelID int = (SELECT w.WebsiteChannelID FROM CMS_WebsiteChannel w
    JOIN CMS_Channel c ON c.ChannelID = w.WebsiteChannelChannelID WHERE c.ChannelName = 'Switzerland');
IF @channelID IS NULL THROW 50001, 'Switzerland channel not found.', 1;

INSERT INTO BDO_SectionPeoplePage
    (ContentItemDataCommonDataID, ContentItemDataGUID, PeopleListFilteringEnabled,
     PeopleListFilters, PeopleListOrderingEnabled, PeopleListDefaultOrder, EnableForm,
     DisplayEmailAddressInPersonPage, DisplayFullEmailAddress, DocumentName)
SELECT cd.ContentItemCommonDataID, NEWID(), sourceData.PeopleListFilteringEnabled,
       sourceData.PeopleListFilters, sourceData.PeopleListOrderingEnabled, sourceData.PeopleListDefaultOrder,
       ISNULL(sourceData.EnableForm, 0), ISNULL(sourceData.DisplayEmailAddressInPersonPage, 1),
       ISNULL(sourceData.DisplayFullEmailAddress, 0), ISNULL(sourceData.DocumentName, '')
FROM CMS_WebPageItem w
JOIN CMS_ContentItem ci ON ci.ContentItemID = w.WebPageItemContentItemID
JOIN CMS_Class cl ON cl.ClassID = ci.ContentItemContentTypeID AND cl.ClassName = 'BDO.SectionPeoplePage'
JOIN CMS_ContentItemCommonData cd ON cd.ContentItemCommonDataContentItemID = ci.ContentItemID
JOIN CMS_ContentLanguage lang ON lang.ContentLanguageID = cd.ContentItemCommonDataContentLanguageID
CROSS APPLY (
    SELECT TOP 1 people.*, document.DocumentName
    FROM [bdo-db-gwt-tst2-eur].dbo.CMS_Tree tree
    JOIN [bdo-db-gwt-tst2-eur].dbo.CMS_Site site ON site.SiteID = tree.NodeSiteID AND site.SiteName = 'Switzerland'
    JOIN [bdo-db-gwt-tst2-eur].dbo.CMS_Class sourceClass ON sourceClass.ClassID = tree.NodeClassID AND sourceClass.ClassName = 'BDO.SectionPeoplePage'
    JOIN [bdo-db-gwt-tst2-eur].dbo.CMS_Document document ON document.DocumentNodeID = tree.NodeID
    JOIN [bdo-db-gwt-tst2-eur].dbo.BDO_SectionPeoplePage people ON people.SectionPeoplePageID = document.DocumentForeignKeyValue
    WHERE (tree.NodeGUID = ci.ContentItemGUID OR tree.NodeAliasPath COLLATE DATABASE_DEFAULT = w.WebPageItemTreePath)
      AND document.DocumentCulture COLLATE DATABASE_DEFAULT = lang.ContentLanguageCultureFormat
    ORDER BY CASE WHEN tree.NodeGUID = ci.ContentItemGUID THEN 0 ELSE 1 END, document.DocumentID
) sourceData
WHERE w.WebPageItemWebsiteChannelID = @channelID
  AND NOT EXISTS (SELECT 1 FROM BDO_SectionPeoplePage existing WHERE existing.ContentItemDataCommonDataID = cd.ContentItemCommonDataID);

SELECT @@ROWCOUNT AS RepairedPeopleVersions;
IF EXISTS (
    SELECT 1 FROM CMS_ContentItemCommonData cd
    JOIN CMS_ContentItem ci ON ci.ContentItemID = cd.ContentItemCommonDataContentItemID
    JOIN CMS_Class cl ON cl.ClassID = ci.ContentItemContentTypeID AND cl.ClassName = 'BDO.SectionPeoplePage'
    JOIN CMS_WebPageItem w ON w.WebPageItemContentItemID = ci.ContentItemID AND w.WebPageItemWebsiteChannelID = @channelID
    WHERE NOT EXISTS (SELECT 1 FROM BDO_SectionPeoplePage people WHERE people.ContentItemDataCommonDataID = cd.ContentItemCommonDataID)
) THROW 50002, 'Some People versions could not be correlated with source data. Repair rolled back.', 1;

COMMIT TRANSACTION;