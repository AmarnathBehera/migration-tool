SET QUOTED_IDENTIFIER ON;
SET XACT_ABORT ON;
BEGIN TRANSACTION;

DECLARE @websiteChannelID int = 10;

DECLARE @deletePages TABLE (ID int PRIMARY KEY);
INSERT INTO @deletePages
SELECT w.WebPageItemID
FROM CMS_WebPageItem w
JOIN CMS_ContentItem ci ON ci.ContentItemID = w.WebPageItemContentItemID
JOIN CMS_Class cl ON cl.ClassID = ci.ContentItemContentTypeID
WHERE w.WebPageItemWebsiteChannelID = @websiteChannelID
  AND cl.ClassName IN ('BDO.SectionInsightsPage', 'BDO.Insight');

DECLARE @deleteItems TABLE (ID int PRIMARY KEY);
INSERT INTO @deleteItems SELECT WebPageItemContentItemID FROM CMS_WebPageItem WHERE WebPageItemID IN (SELECT ID FROM @deletePages);

DECLARE @deleteCommon TABLE (ID int PRIMARY KEY);
INSERT INTO @deleteCommon SELECT ContentItemCommonDataID FROM CMS_ContentItemCommonData WHERE ContentItemCommonDataContentItemID IN (SELECT ID FROM @deleteItems);

DECLARE @deleteLang TABLE (ID int PRIMARY KEY);
INSERT INTO @deleteLang SELECT ContentItemLanguageMetadataID FROM CMS_ContentItemLanguageMetadata WHERE ContentItemLanguageMetadataContentItemID IN (SELECT ID FROM @deleteItems);

UPDATE CMS_WebPageItem SET WebPageItemParentID = NULL WHERE WebPageItemParentID IN (SELECT ID FROM @deletePages);

DELETE FROM CMS_ContentItemTag WHERE ContentItemTagContentItemLanguageMetadataID IN (SELECT ID FROM @deleteLang);
DELETE FROM CMS_ContentItemReference WHERE ContentItemReferenceSourceCommonDataID IN (SELECT ID FROM @deleteCommon) OR ContentItemReferenceTargetItemID IN (SELECT ID FROM @deleteItems);
DELETE FROM CMS_ContentItemObjectReference WHERE ContentItemObjectReferenceSourceCommonDataID IN (SELECT ID FROM @deleteCommon);
DELETE FROM CMS_ContentItemLanguageMetadata WHERE ContentItemLanguageMetadataContentItemID IN (SELECT ID FROM @deleteItems);
DELETE FROM CMS_ContentItemVersion WHERE ContentItemVersionContentItemID IN (SELECT ID FROM @deleteItems);
DELETE FROM BDO_SectionInsightsPage WHERE ContentItemDataCommonDataID IN (SELECT ID FROM @deleteCommon);
DELETE FROM BDO_Insight WHERE ContentItemDataCommonDataID IN (SELECT ID FROM @deleteCommon);
DELETE FROM CMS_WebPageUrlPath WHERE WebPageUrlPathWebPageItemID IN (SELECT ID FROM @deletePages);
DELETE FROM CMS_WebPageFormerUrlPath WHERE WebPageFormerUrlPathWebPageItemID IN (SELECT ID FROM @deletePages) OR WebPageFormerUrlPathSourceWebPageItemID IN (SELECT ID FROM @deletePages);
DELETE FROM CMS_WebPageAclMapping WHERE WebPageAclMappingWebPageItemID IN (SELECT ID FROM @deletePages);
DELETE FROM CMS_WebPageItem WHERE WebPageItemID IN (SELECT ID FROM @deletePages);
DELETE FROM CMS_ContentItemCommonData WHERE ContentItemCommonDataID IN (SELECT ID FROM @deleteCommon);
DELETE FROM CMS_ContentItem WHERE ContentItemID IN (SELECT ID FROM @deleteItems);

SELECT COUNT(*) AS RemainingInsightPages
FROM CMS_WebPageItem w
JOIN CMS_ContentItem ci ON ci.ContentItemID = w.WebPageItemContentItemID
JOIN CMS_Class cl ON cl.ClassID = ci.ContentItemContentTypeID
WHERE w.WebPageItemWebsiteChannelID = @websiteChannelID
  AND cl.ClassName IN ('BDO.SectionInsightsPage', 'BDO.Insight');

COMMIT TRANSACTION;
