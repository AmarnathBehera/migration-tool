-- Create BDO.CopyInsight content hub (reusable) items from K13 Insight CTA + Content sharing fields,
-- and reference each from the migrated Insight page's InsightContent field.
-- Idempotent: re-run deletes previously created items (marker 'CIHub-Insight-%') and rebuilds.
SET QUOTED_IDENTIFIER ON;
SET XACT_ABORT ON;
BEGIN TRANSACTION;

DECLARE @languageID int = 1;                 -- en-GB
DECLARE @workspaceID int = 1;                -- KenticoDefault (Default Workspace)
DECLARE @folderID int = 11;                  -- existing Insight content folder (workspace 1)
DECLARE @now datetime2 = SYSUTCDATETIME();
DECLARE @copyClassID int = (SELECT ClassID FROM CMS_Class WHERE ClassName='BDO.CopyInsight');
DECLARE @insightClassID int = (SELECT ClassID FROM CMS_Class WHERE ClassName='BDO.Insight');
-- Current channel (where these insights live) -> stored as 'Original Site' (dropdown stores ChannelName)
DECLARE @channelName nvarchar(200) = (SELECT ch.ChannelName FROM CMS_WebsiteChannel wc JOIN CMS_Channel ch ON ch.ChannelID=wc.WebsiteChannelChannelID WHERE wc.WebsiteChannelID=10);

-- ---- Idempotent cleanup of previously created hub items ----
DECLARE @old TABLE (ContentItemID int, CommonID int);
INSERT INTO @old (ContentItemID, CommonID)
SELECT ci.ContentItemID, cd.ContentItemCommonDataID
FROM CMS_ContentItem ci
LEFT JOIN CMS_ContentItemCommonData cd ON cd.ContentItemCommonDataContentItemID=ci.ContentItemID
WHERE ci.ContentItemContentTypeID=@copyClassID
    AND cd.ContentItemCommonDataContentLanguageID=@languageID
    AND EXISTS (
        SELECT 1 FROM CMS_WebPageItem page
        WHERE page.WebPageItemWebsiteChannelID=10
            AND (ci.ContentItemName=CONCAT('CIHub-Insight-',page.WebPageItemContentItemID,'-',@languageID)
                OR (ci.ContentItemName=CONCAT('CIHub-Insight-',page.WebPageItemContentItemID)
                    AND NOT EXISTS (SELECT 1 FROM CMS_ContentItemCommonData otherLanguage
                        WHERE otherLanguage.ContentItemCommonDataContentItemID=ci.ContentItemID
                            AND otherLanguage.ContentItemCommonDataContentLanguageID<>@languageID)))
    );

IF EXISTS (SELECT 1 FROM @old)
BEGIN
    DELETE FROM CMS_ContentItemReference WHERE ContentItemReferenceTargetItemID IN (SELECT ContentItemID FROM @old);
    DELETE FROM BDO_CopyInsight WHERE ContentItemDataCommonDataID IN (SELECT CommonID FROM @old);
    DELETE FROM CMS_ContentItemLanguageMetadata WHERE ContentItemLanguageMetadataContentItemID IN (SELECT ContentItemID FROM @old);
    DELETE FROM CMS_ContentItemCommonData WHERE ContentItemCommonDataContentItemID IN (SELECT ContentItemID FROM @old);
    DELETE FROM CMS_ContentItem WHERE ContentItemID IN (SELECT ContentItemID FROM @old);
END

-- ---- Source Insight CTA + Content sharing rows (en-GB), keyed by NodeGUID and normalized path ----
DECLARE @src TABLE (
    NodeGUID uniqueidentifier, NormPath nvarchar(450),
    ShowCallToAction bit, CallToActionDescription nvarchar(max), CallToActionButtonText nvarchar(max), CallToActionButtonUrl nvarchar(max), CallToActionPosition nvarchar(max),
    SharingExplanation nvarchar(max), SharingOptOut bit, SharingHeadline bit, SharingSourceFirm nvarchar(max), SharingInstructions nvarchar(max),
    OriginalDocumentAttachmentType nvarchar(max), OriginalInvolvedCountriesAll bit, OriginalInvolvedCountries nvarchar(max),
    OriginalDocumentOwnerOptOut bit, OriginalDocumentOwnerName nvarchar(max), OriginalDocumentOwnerEmail nvarchar(max), OriginalSiteGuid nvarchar(50),
    DownloadButtonTitle nvarchar(max), DownloadButtonText nvarchar(max), NodeName nvarchar(450),
    ShowSidebar bit, ShowTitle bit, ShowDescription bit,
    MetadataDescription nvarchar(max), Content nvarchar(max), SourceCulture nvarchar(20)
);
INSERT INTO @src
SELECT t.NodeGUID,
    LOWER(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(t.NodeAliasPath,'/',''),'-',''),'_',''),' ',''),'(',''),')',''),'.',''),'''','')),
    bi.ShowCallToAction, bi.CallToActionDescription, bi.CallToActionButtonText, bi.CallToActionButtonUrl, bi.CallToActionPosition,
    bi.SharingExplanation, bi.SharingOptOut, bi.SharingHeadline, bi.SharingSourceFirm, bi.SharingInstructions,
    bi.OriginalDocumentAttachmentType, bi.OriginalInvolvedCountriesAll, bi.OriginalInvolvedCountries,
    bi.OriginalDocumentOwnerOptOut, bi.OriginalDocumentOwnerName, bi.OriginalDocumentOwnerEmail, CAST(bi.OriginalSiteGuid AS nvarchar(50)),
    bi.DownloadButtonTitle, bi.DownloadButtonText, t.NodeName,
    bi.ShowSidebar, bi.ShowTitle, bi.ShowDescription,
    bi.MetadataDescription, bi.Content, d.DocumentCulture
FROM [BDO-DB-GWT-TST-EUR].dbo.CMS_Tree t
JOIN [BDO-DB-GWT-TST-EUR].dbo.CMS_Class cl ON cl.ClassID=t.NodeClassID
JOIN [BDO-DB-GWT-TST-EUR].dbo.CMS_Document d ON d.DocumentNodeID=t.NodeID
    AND (d.DocumentCulture='en-GB' OR (LOWER(d.DocumentCulture)='en-gb' AND @languageID=(
        SELECT WebsiteChannelPrimaryContentLanguageID FROM CMS_WebsiteChannel WHERE WebsiteChannelID=10)))
JOIN [BDO-DB-GWT-TST-EUR].dbo.BDO_Insight bi ON bi.InsightID=d.DocumentForeignKeyValue
WHERE cl.ClassName='BDO.Insight' AND t.NodeSiteID=1;

-- ---- Correlate each target Insight page (en-GB latest common data) to a source row ----
DECLARE @corr TABLE (
    TargetContentItemID int, TargetCommonID int, DisplayName nvarchar(450), rn int IDENTITY(1,1),
    HasSrc bit,
    ShowCallToAction bit, CallToActionDescription nvarchar(max), CallToActionButtonText nvarchar(max), CallToActionButtonUrl nvarchar(max), CallToActionPosition nvarchar(max),
    SharingExplanation nvarchar(max), SharingOptOut bit, SharingHeadline bit, SharingSourceFirm nvarchar(max), SharingInstructions nvarchar(max),
    OriginalDocumentAttachmentType nvarchar(max), OriginalInvolvedCountriesAll bit, OriginalInvolvedCountries nvarchar(max),
    OriginalDocumentOwnerOptOut bit, OriginalDocumentOwnerName nvarchar(max), OriginalDocumentOwnerEmail nvarchar(max), OriginalSiteGuid nvarchar(50),
    DownloadButtonTitle nvarchar(max), DownloadButtonText nvarchar(max),
    ShowSidebar bit, ShowTitle bit, ShowDescription bit,
    MetadataDescription nvarchar(max), Content nvarchar(max)
);
INSERT INTO @corr (TargetContentItemID, TargetCommonID, DisplayName, HasSrc,
    ShowCallToAction, CallToActionDescription, CallToActionButtonText, CallToActionButtonUrl, CallToActionPosition,
    SharingExplanation, SharingOptOut, SharingHeadline, SharingSourceFirm, SharingInstructions,
    OriginalDocumentAttachmentType, OriginalInvolvedCountriesAll, OriginalInvolvedCountries,
    OriginalDocumentOwnerOptOut, OriginalDocumentOwnerName, OriginalDocumentOwnerEmail, OriginalSiteGuid,
    DownloadButtonTitle, DownloadButtonText, ShowSidebar, ShowTitle, ShowDescription,
    MetadataDescription, Content)
SELECT ci.ContentItemID, cd.ContentItemCommonDataID, lm.ContentItemLanguageMetadataDisplayName,
    CASE WHEN s.NodeGUID IS NOT NULL THEN 1 ELSE 0 END,
    s.ShowCallToAction, s.CallToActionDescription, s.CallToActionButtonText, s.CallToActionButtonUrl, s.CallToActionPosition,
    s.SharingExplanation, s.SharingOptOut, s.SharingHeadline, s.SharingSourceFirm, s.SharingInstructions,
    s.OriginalDocumentAttachmentType, s.OriginalInvolvedCountriesAll, s.OriginalInvolvedCountries,
    s.OriginalDocumentOwnerOptOut, s.OriginalDocumentOwnerName, s.OriginalDocumentOwnerEmail, s.OriginalSiteGuid,
    s.DownloadButtonTitle, s.DownloadButtonText, s.ShowSidebar, s.ShowTitle, s.ShowDescription,
    s.MetadataDescription, s.Content
FROM CMS_WebPageItem w
JOIN CMS_ContentItem ci ON ci.ContentItemID=w.WebPageItemContentItemID
JOIN CMS_ContentItemCommonData cd ON cd.ContentItemCommonDataContentItemID=ci.ContentItemID AND cd.ContentItemCommonDataContentLanguageID=@languageID AND cd.ContentItemCommonDataIsLatest=1
LEFT JOIN CMS_ContentItemLanguageMetadata lm ON lm.ContentItemLanguageMetadataContentItemID=ci.ContentItemID AND lm.ContentItemLanguageMetadataContentLanguageID=@languageID
OUTER APPLY (
    SELECT TOP 1 * FROM @src s0
    WHERE s0.NodeGUID = ci.ContentItemGUID
       OR s0.NormPath = LOWER(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(w.WebPageItemTreePath,'/',''),'-',''),'_',''),' ',''),'(',''),')',''),'.',''),'''',''))
    ORDER BY CASE WHEN s0.NodeGUID = ci.ContentItemGUID THEN 0 ELSE 1 END,
        CASE WHEN LOWER(s0.SourceCulture)='en-gb' THEN 1 ELSE 0 END
) s
WHERE ci.ContentItemContentTypeID=@insightClassID AND w.WebPageItemWebsiteChannelID=10 AND s.NodeGUID IS NOT NULL;

-- ---- Create a CopyInsight hub item per target Insight and wire the reference ----
DECLARE @i int = 1, @max int = (SELECT MAX(rn) FROM @corr);
WHILE @i <= @max
BEGIN
    DECLARE @tCI int,@tCommon int,@dn nvarchar(450),@hasSrc bit,
            @showCta bit,@ctaDesc nvarchar(max),@ctaText nvarchar(max),@ctaUrl nvarchar(max),@ctaPos nvarchar(max),
            @shareExpl nvarchar(max),@shareOpt bit,@shareHead bit,@shareFirm nvarchar(max),@shareInstr nvarchar(max),
            @origAttType nvarchar(max),@origAllC bit,@origC nvarchar(max),@origOwnerOpt bit,@origOwnerName nvarchar(max),@origOwnerEmail nvarchar(max),@origSite nvarchar(50),
            @dlTitle nvarchar(max),@dlText nvarchar(max),@showSidebar bit,@showTitle bit,@showDesc bit,
            @metaDesc nvarchar(max),@content nvarchar(max);
    SELECT @tCI=TargetContentItemID,@tCommon=TargetCommonID,@dn=DisplayName,@hasSrc=HasSrc,
           @showCta=ShowCallToAction,@ctaDesc=CallToActionDescription,@ctaText=CallToActionButtonText,@ctaUrl=CallToActionButtonUrl,@ctaPos=CallToActionPosition,
           @shareExpl=SharingExplanation,@shareOpt=SharingOptOut,@shareHead=SharingHeadline,@shareFirm=SharingSourceFirm,@shareInstr=SharingInstructions,
           @origAttType=OriginalDocumentAttachmentType,@origAllC=OriginalInvolvedCountriesAll,@origC=OriginalInvolvedCountries,
           @origOwnerOpt=OriginalDocumentOwnerOptOut,@origOwnerName=OriginalDocumentOwnerName,@origOwnerEmail=OriginalDocumentOwnerEmail,@origSite=OriginalSiteGuid,
           @dlTitle=DownloadButtonTitle,@dlText=DownloadButtonText,@showSidebar=ShowSidebar,@showTitle=ShowTitle,@showDesc=ShowDescription,
           @metaDesc=MetadataDescription,@content=Content
    FROM @corr WHERE rn=@i;

    DECLARE @hubCI int,@hubCommon int,@hubGUID uniqueidentifier=NEWID();
    INSERT INTO CMS_ContentItem (ContentItemGUID, ContentItemName, ContentItemIsReusable, ContentItemIsSecured, ContentItemContentTypeID, ContentItemChannelID, ContentItemContentFolderID, ContentItemWorkspaceID)
    VALUES (@hubGUID, CONCAT('CIHub-Insight-', @tCI, '-', @languageID), 1, 0, @copyClassID, NULL, @folderID, @workspaceID);
    SET @hubCI=SCOPE_IDENTITY();

    INSERT INTO CMS_ContentItemCommonData (ContentItemCommonDataGUID, ContentItemCommonDataContentItemID, ContentItemCommonDataContentLanguageID, ContentItemCommonDataVersionStatus, ContentItemCommonDataIsLatest,
        ShowCallToAction, CallToActionDescription, CallToActionButtonText, CallToActionButtonUrl, ShowSidebar)
    VALUES (NEWID(), @hubCI, @languageID, 2, 1,
        ISNULL(@showCta,0), @ctaDesc, @ctaText, @ctaUrl, ISNULL(@showSidebar,0));
    SET @hubCommon=SCOPE_IDENTITY();

    INSERT INTO BDO_CopyInsight (ContentItemDataCommonDataID, ContentItemDataGUID,
        Title, Description, ShowInsightTitle, ShowInsightDescription, InsightContent, CallToActionPosition,
        Attachments, InsightAttachment, DownloadButtonTitle, DownloadButtonText, Contentsharing,
        SharingOptOut, SharingHeadline, OriginalDocumentAttachmentType, OriginalIndustries, OriginalServiceLines,
        OriginalInvolvedCountriesAll, OriginalInvolvedCountries, OriginalDocumentOwnerOptOut, OriginalDocumentOwnerName, OriginalDocumentOwnerEmail,
        SharingSourceFirm, SharingInstructions, OriginalSiteGuid)
    VALUES (@hubCommon, NEWID(),
        LEFT(ISNULL(@dn,''),200), ISNULL(@metaDesc,''), ISNULL(@showTitle,0), ISNULL(@showDesc,0), @content, LOWER(NULLIF(LTRIM(RTRIM(@ctaPos)),'')),
        NULL, NULL, ISNULL(@dlTitle,''), ISNULL(@dlText,''), @shareExpl,
        ISNULL(@shareOpt,0), ISNULL(@shareHead,0), ISNULL(@origAttType,''), '[]', '[]',
        ISNULL(@origAllC,0), @origC, ISNULL(@origOwnerOpt,0), ISNULL(@origOwnerName,''), ISNULL(@origOwnerEmail,''),
        @shareFirm, @shareInstr, @channelName);

    INSERT INTO CMS_ContentItemLanguageMetadata (ContentItemLanguageMetadataContentItemID, ContentItemLanguageMetadataDisplayName, ContentItemLanguageMetadataLatestVersionStatus, ContentItemLanguageMetadataGUID, ContentItemLanguageMetadataCreatedWhen, ContentItemLanguageMetadataModifiedWhen, ContentItemLanguageMetadataHasImageAsset, ContentItemLanguageMetadataContentLanguageID, ContentItemLanguageMetadataVersionTimestamp)
    VALUES (@hubCI, LEFT(CONCAT('Insight Content - ', ISNULL(@dn,'')),100), 2, NEWID(), @now, @now, 0, @languageID, @now);

    -- Reference from the Insight page's InsightContent coupled field
    UPDATE BDO_Insight SET InsightContent = CONCAT('[{"Identifier":"', LOWER(CONVERT(nvarchar(36),@hubGUID)), '"}]')
    WHERE ContentItemDataCommonDataID=@tCommon;

    INSERT INTO CMS_ContentItemReference (ContentItemReferenceGUID, ContentItemReferenceSourceCommonDataID, ContentItemReferenceTargetItemID, ContentItemReferenceGroupGUID)
    VALUES (NEWID(), @tCommon, @hubCI, NEWID());

    SET @i += 1;
END

SELECT COUNT(*) AS HubItemsCreated FROM @corr;
SELECT SUM(CASE WHEN HasSrc=1 THEN 1 ELSE 0 END) AS WithSourceData, SUM(CASE WHEN HasSrc=0 THEN 1 ELSE 0 END) AS EmptyDefaults FROM @corr;

COMMIT TRANSACTION;
