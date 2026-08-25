using System.ComponentModel.DataAnnotations;

namespace BDO.GWT.Common.Options
{
    public class SecretsEncryptionOptions
    {
        /// <summary>
        /// 32 bytes secret key.
        /// </summary>
        [Required]
        public required string SecretKey { get; set; }
    }
}
