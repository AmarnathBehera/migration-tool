namespace BDO.GWT.GLOBAL.API.Data
{
    public interface IPageAttachmentService
    {
        Task<IEnumerable<ContentAttachment>> GetPageAttachmentsAsync(
            Guid pageGuid,
            string channelName,
            string culture,
            CancellationToken cancellationToken = default);
    }
}
