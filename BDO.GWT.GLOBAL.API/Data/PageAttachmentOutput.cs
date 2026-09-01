namespace BDO.GWT.GLOBAL.API.Data
{
    public class PageAttachmentOutput
    {
        public Guid GUID { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Extension { get; set; } = string.Empty;

        public int Size { get; set; }

        public string MimeType { get; set; } = string.Empty;

        public int? ImageWidth { get; set; }

        public int? ImageHeight { get; set; }

        public int? Order { get; set; }

        public string? Title { get; set; }

        public string? Description { get; set; }

        public DateTime? LastModified { get; set; }

        public string? Message { get; set; }
    }
}
