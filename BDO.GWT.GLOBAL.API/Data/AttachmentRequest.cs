using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace BDO.GWT.GLOBAL.API.Data
{
    /// <summary>
    /// Request model for creating or updating a page attachment.
    /// </summary>
    public class AttachmentRequest
    {
        /// <summary>
        /// Language tag in the form of RFC 5646 (e.g. "en-US").
        /// </summary>
        [Required]
        public string Culture { get; set; } = null!;

        /// <summary>
        /// The attachment file to upload.
        /// </summary>
        [Required]
        public IFormFile File { get; set; } = null!;
    }
}
