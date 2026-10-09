using Microsoft.AspNetCore.Http;

namespace BDO.GWT.GLOBAL.API.Data
{
    public interface IPageAttachmentCrudService
    {
        Task<PageAttachmentOutput?> GetAttachmentAsync(
            int channelId,
            Guid pageGuid,
            Guid attachmentGuid,
            string culture,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<PageAttachmentOutput>> GetAttachmentsAsync(
            int channelId,
            Guid pageGuid,
            string culture,
            CancellationToken cancellationToken = default);

        Task<PageAttachmentOutput> CreateAttachmentAsync(
            int channelId,
            Guid pageGuid,
            string culture,
            IFormFile file,
            CancellationToken cancellationToken = default);

        Task<PageAttachmentOutput?> UpdateAttachmentAsync(
            int channelId,
            Guid pageGuid,
            Guid attachmentGuid,
            string culture,
            IFormFile file,
            CancellationToken cancellationToken = default);

        Task<(byte[] Data, string ContentType, string FileName)?> GetAttachmentBinaryAsync(
            int channelId,
            Guid pageGuid,
            Guid attachmentGuid,
            string culture,
            CancellationToken cancellationToken = default);

        Task<bool> DeleteAttachmentAsync(
            int channelId,
            Guid pageGuid,
            Guid attachmentGuid,
            string culture,
            CancellationToken cancellationToken = default);
    }
}
