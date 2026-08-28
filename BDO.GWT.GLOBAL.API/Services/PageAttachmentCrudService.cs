using BDO.GWT.Core;
using BDO.GWT.GLOBAL.API.Data;
using CMS.ContentEngine;
using CMS.ContentEngine.Internal;
using CMS.Membership;
using CMS.Websites;

namespace BDO.GWT.GLOBAL.API.Services
{
    public class PageAttachmentCrudService : IPageAttachmentCrudService
    {
        private readonly IPageAttachmentService pageAttachmentService;
        private readonly IPageChannelResolver pageChannelResolver;
        private readonly IContentService contentService;
        private readonly IWebPageManagerFactory webPageManagerFactory;
        private readonly IContentItemManagerFactory contentItemManagerFactory;
        private readonly IUserInfoProvider userInfoProvider;
        private readonly IContentItemInfoService contentItemInfoService;

        public PageAttachmentCrudService(
            IPageAttachmentService pageAttachmentService,
            IPageChannelResolver pageChannelResolver,
            IContentService contentService,
            IWebPageManagerFactory webPageManagerFactory,
            IContentItemManagerFactory contentItemManagerFactory,
            IUserInfoProvider userInfoProvider,
            IContentItemInfoService contentItemInfoService)
        {
            this.pageAttachmentService = pageAttachmentService;
            this.pageChannelResolver = pageChannelResolver;
            this.contentService = contentService;
            this.webPageManagerFactory = webPageManagerFactory;
            this.contentItemManagerFactory = contentItemManagerFactory;
            this.userInfoProvider = userInfoProvider;
            this.contentItemInfoService = contentItemInfoService;
        }

        public async Task<PageAttachmentOutput?> GetAttachmentAsync(
            int channelId,
            Guid pageGuid,
            Guid attachmentGuid,
            string culture,
            CancellationToken cancellationToken = default)
        {
            var channelName = await pageChannelResolver.GetChannelNameByPageGuidAsync(pageGuid, cancellationToken);
            if (channelName is null)
            {
                return null;
            }
            var attachments =
                await pageAttachmentService.GetPageAttachmentsAsync(
                    pageGuid,
                    channelName,
                    culture,
                    cancellationToken);

            var attachment = attachments.FirstOrDefault(
                x => x.SystemFields.ContentItemGUID == attachmentGuid);

            return attachment == null
                ? null
                : Map(attachment);
        }

        public async Task<IEnumerable<PageAttachmentOutput>> GetAttachmentsAsync(
            int channelId,
            Guid pageGuid,
            string culture,
            CancellationToken cancellationToken = default)
        {
            var channelName = await pageChannelResolver.GetChannelNameByPageGuidAsync(pageGuid, cancellationToken);
            ArgumentNullException.ThrowIfNull(channelName);
            var attachments =
                await pageAttachmentService.GetPageAttachmentsAsync(
                    pageGuid,
                    channelName,
                    culture,
                    cancellationToken);

            return attachments.Select(Map);
        }

        public async Task<PageAttachmentOutput> CreateAttachmentAsync(
            int channelId,
            Guid pageGuid,
            string culture,
            IFormFile file,
            CancellationToken cancellationToken = default)
        {
            ValidateFile(file);

            var channelName = await pageChannelResolver.GetChannelNameByPageGuidAsync(pageGuid, cancellationToken);
            ArgumentNullException.ThrowIfNull(channelName);

            var page = (await contentService.GetItemsByGUIDForAPIAsync(new List<Guid> { pageGuid }, channelName, culture))
                ?.FirstOrDefault();
            if (page is null)
            {
                throw new InvalidOperationException($"Page '{pageGuid}' was not found.");
            }

            // Find all properties that can hold attachments/images (IEnumerable<ContentAttachment>)
            var allAttachmentProperties = page.GetType().GetProperties()
                .Where(x => typeof(IEnumerable<BDO.ContentAttachment>).IsAssignableFrom(x.PropertyType))
                .ToList();

            if (allAttachmentProperties.Count == 0)
            {
                throw new InvalidOperationException($"Page '{pageGuid}' does not contain any attachment-capable fields.");
            }

            var fileName = System.IO.Path.GetFileName(file.FileName ?? "attachment");

            // Check if the same image/attachment already exists on the page
            var existingAttachments = allAttachmentProperties
                .SelectMany(prop => (prop.GetValue(page) as IEnumerable<BDO.ContentAttachment>) ?? Enumerable.Empty<BDO.ContentAttachment>())
                .ToList();

            var duplicateAttachment = existingAttachments.FirstOrDefault(att =>
                att.ContentAttachmentAsset?.Metadata.Name?.Equals(fileName, StringComparison.OrdinalIgnoreCase) == true);

            if (duplicateAttachment is not null)
            {
                var output = Map(duplicateAttachment);
                output.Message = "Image already available on this page";
                return output;
            }

            // Find the first empty attachment field
            var emptyAttachmentProperty = allAttachmentProperties.FirstOrDefault(prop =>
            {
                var value = prop.GetValue(page) as IEnumerable<BDO.ContentAttachment>;
                return value == null || !value.Any();
            });

            string? attachmentPlacementMessage = null;

            // If no empty field found, prepare message but continue with creation
            if (emptyAttachmentProperty is null)
            {
                attachmentPlacementMessage = "No available image field to add the attachment. The attachment has been created in the content hub but could not be placed on the page.";
            }

            var extension = System.IO.Path.GetExtension(file.FileName ?? string.Empty);
            var user = userInfoProvider.Get("administrator");

            var attachmentGuid = await CreateAttachmentContentItemAsync(file, pageGuid, culture, user.UserID, cancellationToken);

            // Only update the page if we found an empty attachment field
            if (emptyAttachmentProperty is not null)
            {
                var fieldAttachments = emptyAttachmentProperty.GetValue(page) as IEnumerable<BDO.ContentAttachment>
                    ?? Enumerable.Empty<BDO.ContentAttachment>();

                var references = fieldAttachments
                    .Select(x => new ContentItemReference { Identifier = x.SystemFields.ContentItemGUID })
                    .ToList();
                references.Add(new ContentItemReference { Identifier = attachmentGuid });

                var updateData = new ContentItemData(new Dictionary<string, object>
                {
                    { emptyAttachmentProperty.Name, references }
                });

                var webPageManager = webPageManagerFactory.Create(channelId, user.UserID);
                await UpdatePageWithReferencesAsync(
                    webPageManager,
                    page.SystemFields.WebPageItemID,
                    culture,
                    updateData,
                    "Failed to update page with the new attachment.",
                    "Failed to publish page after adding attachment.",
                    cancellationToken);
            }

            var savedAttachment = (await pageAttachmentService.GetPageAttachmentsAsync(pageGuid, channelName, culture, cancellationToken))
                .FirstOrDefault(x => x.SystemFields.ContentItemGUID == attachmentGuid);

            if (savedAttachment is not null)
            {
                var output = Map(savedAttachment);
                if (attachmentPlacementMessage != null)
                {
                    output.Message = attachmentPlacementMessage;
                }
                return output;
            }

            return new PageAttachmentOutput
            {
                GUID = attachmentGuid,
                Name = fileName,
                Extension = extension,
                MimeType = GetMimeType(extension),
                Size = (int)file.Length,
                Title = fileName,
                Description = fileName,
                LastModified = DateTime.UtcNow,
                Order = 0,
                Message = attachmentPlacementMessage
            };
        }

        public async Task<(byte[] Data, string ContentType, string FileName)?> GetAttachmentBinaryAsync(
            int channelId,
            Guid pageGuid,
            Guid attachmentGuid,
            string culture,
            CancellationToken cancellationToken = default)
        {
            var channelName = await pageChannelResolver.GetChannelNameByPageGuidAsync(pageGuid, cancellationToken);
            if (channelName is null)
            {
                return null;
            }

            return await pageAttachmentService.GetPageAttachmentBinaryAsync(
                pageGuid,
                attachmentGuid,
                channelName,
                culture,
                cancellationToken);
        }

        public async Task<PageAttachmentOutput?> UpdateAttachmentAsync(
            int channelId,
            Guid pageGuid,
            Guid attachmentGuid,
            string culture,
            IFormFile file,
            CancellationToken cancellationToken = default)
        {
            ValidateFile(file);

            var channelName = await pageChannelResolver.GetChannelNameByPageGuidAsync(pageGuid, cancellationToken);
            if (channelName is null)
            {
                return null;
            }

            // Get the page to find which field contains the attachment
            var page = (await contentService.GetItemsByGUIDForAPIAsync(new List<Guid> { pageGuid }, channelName, culture))
                ?.FirstOrDefault();
            if (page is null)
            {
                return null;
            }

            // Find the attachment property that contains the existing attachment GUID
            var targetProperty = FindAttachmentPropertyContaining(page, attachmentGuid);
            if (targetProperty is null)
            {
                // Attachment not found on this page
                return null;
            }

            var user = userInfoProvider.Get("administrator");

            // Create new attachment in content hub
            var newAttachmentGuid = await CreateAttachmentContentItemAsync(file, pageGuid, culture, user.UserID, cancellationToken);

            // Get current attachments in the field and replace the old GUID with new GUID
            var fieldAttachments = targetProperty.GetValue(page) as IEnumerable<BDO.ContentAttachment>
                ?? Enumerable.Empty<BDO.ContentAttachment>();

            var references = fieldAttachments
                .Select(x => new ContentItemReference
                {
                    Identifier = x.SystemFields.ContentItemGUID == attachmentGuid
                        ? newAttachmentGuid  // Replace old with new
                        : x.SystemFields.ContentItemGUID
                })
                .ToList();

            var updateData = new ContentItemData(new Dictionary<string, object>
            {
                { targetProperty.Name, references }
            });

            // Update the page with the new attachment reference
            var webPageManager = webPageManagerFactory.Create(channelId, user.UserID);
            await UpdatePageWithReferencesAsync(
                webPageManager,
                page.SystemFields.WebPageItemID,
                culture,
                updateData,
                "Failed to update page with the new attachment reference.",
                "Failed to publish page after updating attachment reference.",
                cancellationToken);

            // Retrieve and return the new attachment
            var updatedAttachments = await pageAttachmentService.GetPageAttachmentsAsync(
                pageGuid,
                channelName,
                culture,
                cancellationToken);

            var newAttachment = updatedAttachments.FirstOrDefault(
                x => x.SystemFields.ContentItemGUID == newAttachmentGuid);

            return newAttachment is null ? null : Map(newAttachment);
        }

        public async Task<bool> DeleteAttachmentAsync(
            int channelId,
            Guid pageGuid,
            Guid attachmentGuid,
            string culture,
            CancellationToken cancellationToken = default)
        {
            var channelName = await pageChannelResolver.GetChannelNameByPageGuidAsync(pageGuid, cancellationToken);
            if (channelName is null)
            {
                return false;
            }

            // Get the page to find which field contains the attachment
            var page = (await contentService.GetItemsByGUIDForAPIAsync(new List<Guid> { pageGuid }, channelName, culture))
                ?.FirstOrDefault();
            if (page is null)
            {
                return false;
            }

            // Find the attachment property that contains the attachment GUID
            var targetProperty = FindAttachmentPropertyContaining(page, attachmentGuid);
            if (targetProperty is null)
            {
                // Attachment not found on this page
                return false;
            }

            var user = userInfoProvider.Get("administrator");

            // Get current attachments in the field and remove the one to delete
            var fieldAttachments = targetProperty.GetValue(page) as IEnumerable<BDO.ContentAttachment>
                ?? Enumerable.Empty<BDO.ContentAttachment>();

            var references = fieldAttachments
                .Where(x => x.SystemFields.ContentItemGUID != attachmentGuid) // Exclude the one to delete
                .Select(x => new ContentItemReference { Identifier = x.SystemFields.ContentItemGUID })
                .ToList();

            var updateData = new ContentItemData(new Dictionary<string, object>
            {
                { targetProperty.Name, references }
            });

            // Update the page to remove the attachment reference
            var webPageManager = webPageManagerFactory.Create(channelId, user.UserID);
            await UpdatePageWithReferencesAsync(
                webPageManager,
                page.SystemFields.WebPageItemID,
                culture,
                updateData,
                "Failed to update page to remove attachment reference.",
                "Failed to publish page after removing attachment reference.",
                cancellationToken);

            return true;
        }


        private static PageAttachmentOutput Map(BDO.ContentAttachment attachment)
        {
            var asset = attachment.ContentAttachmentAsset;

            return new PageAttachmentOutput
            {
                GUID = attachment.SystemFields.ContentItemGUID,
                Name = asset?.Metadata.Name ?? string.Empty,
                Extension = asset?.Metadata.Extension ?? string.Empty,
                MimeType = GetMimeType(asset?.Metadata.Extension ?? string.Empty),
                Size = (int)(asset?.Metadata.Size ?? 0),
                ImageWidth = asset?.Metadata.Width,
                ImageHeight = asset?.Metadata.Height,
                Order = 0,
                Title = asset?.Metadata.Name,
                Description = asset?.Metadata.Name,
                LastModified = asset?.Metadata.LastModified
            };
        }

        private static string GetMimeType(string extension)
        {
            return extension?.ToLowerInvariant() switch
            {
                ".png" => "image/png",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".webp" => "image/webp",
                ".gif" => "image/gif",
                ".svg" => "image/svg+xml",
                ".pdf" => "application/pdf",
                _ => "application/octet-stream"
            };
        }

        private static void ValidateFile(IFormFile file)
        {
            ArgumentNullException.ThrowIfNull(file);
            if (file.Length <= 0)
            {
                throw new ArgumentException("File cannot be empty.", nameof(file));
            }
        }

        private static System.Reflection.PropertyInfo? FindAttachmentPropertyContaining(
            IWebPageFieldsSource page,
            Guid attachmentGuid)
        {
            return page.GetType()
                .GetProperties()
                .Where(x => typeof(IEnumerable<BDO.ContentAttachment>).IsAssignableFrom(x.PropertyType))
                .FirstOrDefault(prop =>
                    (prop.GetValue(page) as IEnumerable<BDO.ContentAttachment>)
                        ?.Any(a => a.SystemFields.ContentItemGUID == attachmentGuid) == true);
        }

        private async Task<Guid> CreateAttachmentContentItemAsync(
            IFormFile file,
            Guid pageGuid,
            string culture,
            int userId,
            CancellationToken cancellationToken)
        {
            var fileName = System.IO.Path.GetFileName(file.FileName ?? "attachment");
            var extension = System.IO.Path.GetExtension(file.FileName ?? string.Empty);

            var assetSource = new ContentItemAssetStreamSource(ct => Task.FromResult<Stream>(file.OpenReadStream()));
            var assetMetadata = new ContentItemAssetMetadataWithSource(
                assetSource,
                fileName,
                extension,
                file.Length,
                DateTime.UtcNow);

            var createItemData = new ContentItemData(new Dictionary<string, object>
            {
                { nameof(BDO.ContentAttachment.ContentAttachmentAsset), assetMetadata },
                { "sys_parentItemGUID", pageGuid }
            });

            var attachmentParameters = new CreateContentItemParameters(
                ContentAttachment.CONTENT_TYPE_NAME,
                fileName,
                fileName,
                culture,
                "KenticoDefault")
            {
                VersionStatus = VersionStatus.Published
            };

            var contentItemManager = contentItemManagerFactory.Create(userId);
            int createdContentItemId = await contentItemManager.Create(attachmentParameters, createItemData, cancellationToken);

            var createdContentItem = await contentItemInfoService.GetByIdAsync(createdContentItemId, cancellationToken);
            if (createdContentItem is null)
            {
                throw new InvalidOperationException("Failed to retrieve the created attachment content item.");
            }

            return createdContentItem.ContentItemGUID;
        }

        private async Task UpdatePageWithReferencesAsync(
            IWebPageManager webPageManager,
            int webPageItemId,
            string culture,
            ContentItemData updateData,
            string updateErrorMessage,
            string publishErrorMessage,
            CancellationToken cancellationToken)
        {
            await webPageManager.TryCreateDraft(webPageItemId, culture, cancellationToken);

            if (!await webPageManager.TryUpdateDraft(webPageItemId, culture, new UpdateDraftData(updateData), cancellationToken))
            {
                throw new InvalidOperationException(updateErrorMessage);
            }

            if (!await webPageManager.TryPublish(webPageItemId, culture, cancellationToken))
            {
                throw new InvalidOperationException(publishErrorMessage);
            }
        }
    }
}
