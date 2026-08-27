using BDO.GWT.Core;
using BDO.GWT.GLOBAL.API.Data;
using CMS.ContentEngine;
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

        public PageAttachmentCrudService(
            IPageAttachmentService pageAttachmentService,
            IPageChannelResolver pageChannelResolver,
            IContentService contentService,
            IWebPageManagerFactory webPageManagerFactory,
            IContentItemManagerFactory contentItemManagerFactory,
            IUserInfoProvider userInfoProvider)
        {
            this.pageAttachmentService = pageAttachmentService;
            this.pageChannelResolver = pageChannelResolver;
            this.contentService = contentService;
            this.webPageManagerFactory = webPageManagerFactory;
            this.contentItemManagerFactory = contentItemManagerFactory;
            this.userInfoProvider = userInfoProvider;
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
            ArgumentNullException.ThrowIfNull(file);
            if (file.Length <= 0)
            {
                throw new ArgumentException("File cannot be empty.", nameof(file));
            }

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

            if (!allAttachmentProperties.Any())
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
                return new PageAttachmentOutput
                {
                    GUID = duplicateAttachment.SystemFields.ContentItemGUID,
                    Name = duplicateAttachment.ContentAttachmentAsset?.Metadata.Name ?? fileName,
                    Extension = duplicateAttachment.ContentAttachmentAsset?.Metadata.Extension ?? string.Empty,
                    Size = (int)(duplicateAttachment.ContentAttachmentAsset?.Metadata.Size ?? 0),
                    MimeType = GetMimeType(duplicateAttachment.ContentAttachmentAsset?.Metadata.Extension ?? string.Empty),
                    ImageWidth = duplicateAttachment.ContentAttachmentAsset?.Metadata.Width,
                    ImageHeight = duplicateAttachment.ContentAttachmentAsset?.Metadata.Height,
                    Title = duplicateAttachment.ContentAttachmentAsset?.Metadata.Name,
                    Description = duplicateAttachment.ContentAttachmentAsset?.Metadata.Name,
                    LastModified = duplicateAttachment.ContentAttachmentAsset?.Metadata.LastModified,
                    Order = 0,
                    Message = "Image already available on this page"
                };
            }

            // Find the first empty attachment field
            var emptyAttachmentProperty = allAttachmentProperties.FirstOrDefault(prop =>
            {
                var value = prop.GetValue(page) as IEnumerable<BDO.ContentAttachment>;
                return value == null || !value.Any();
            });

            string attachmentPlacementMessage = null;

            // If no empty field found, prepare message but continue with creation
            if (emptyAttachmentProperty is null)
            {
                attachmentPlacementMessage = "No available image field to add the attachment. The attachment has been created in the content hub but could not be placed on the page.";
                // We'll return this message along with the created attachment info
            }

            var extension = System.IO.Path.GetExtension(file.FileName ?? string.Empty);
            var attachmentGuid = Guid.NewGuid();
            var user = userInfoProvider.Get("administrator");

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

            CreateContentItemParameters attachmentParameters = new(
                                                                ContentAttachment.CONTENT_TYPE_NAME,
                                                                fileName,
                                                                fileName,
                                                                culture,
                                                                "KenticoDefault")
            {
                VersionStatus = VersionStatus.Published
            };
            var createParameters = attachmentParameters;

            var contentItemManager = contentItemManagerFactory.Create(user.UserID);
            await contentItemManager.Create(createParameters, createItemData, cancellationToken);

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
                await webPageManager.TryCreateDraft(page.SystemFields.WebPageItemID, culture, cancellationToken);

                if (!await webPageManager.TryUpdateDraft(page.SystemFields.WebPageItemID, culture, new UpdateDraftData(updateData), cancellationToken))
                {
                    throw new InvalidOperationException("Failed to update page with the new attachment.");
                }

                if (!await webPageManager.TryPublish(page.SystemFields.WebPageItemID, culture, cancellationToken))
                {
                    throw new InvalidOperationException("Failed to publish page after adding attachment.");
                }
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
    }
}
