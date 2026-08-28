using BDO.GWT.GLOBAL.API.Data;

namespace BDO.GWT.GLOBAL.API.Services
{
    public class PageAttachmentCrudService : IPageAttachmentCrudService
    {
        private readonly IPageAttachmentService pageAttachmentService;
        private readonly IPageChannelResolver pageChannelResolver;

        public PageAttachmentCrudService(
            IPageAttachmentService pageAttachmentService,
            IPageChannelResolver pageChannelResolver)
        {
            this.pageAttachmentService = pageAttachmentService;
            this.pageChannelResolver = pageChannelResolver;
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


        private static PageAttachmentOutput Map(ContentAttachment attachment)
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
